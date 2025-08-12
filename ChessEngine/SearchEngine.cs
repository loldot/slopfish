namespace ChessEngine
{
    public struct SearchResult
    {
        public Move BestMove;
        public int Score;
        public int Depth;
        public long NodesSearched;
        public TimeSpan SearchTime;

        public SearchResult(Move bestMove, int score, int depth, long nodesSearched, TimeSpan searchTime)
        {
            BestMove = bestMove;
            Score = score;
            Depth = depth;
            NodesSearched = nodesSearched;
            SearchTime = searchTime;
        }
    }

    public class SearchEngine
    {
        private const int MaxDepth = 50;
        private const int Infinity = 100000;
        private const int MateValue = 30000;

        private Board board;
        private long nodesSearched;
        private DateTime searchStartTime;
        private TimeSpan maxSearchTime;
        private bool shouldStop;
        private TranspositionTable transpositionTable;

        public SearchEngine(Board board)
        {
            this.board = board;
            this.transpositionTable = new TranspositionTable(64); // 64MB TT
        }

        public SearchResult Search(int maxDepth, TimeSpan maxTime)
        {
            nodesSearched = 0;
            searchStartTime = DateTime.UtcNow;
            maxSearchTime = maxTime;
            shouldStop = false;
            
            // Start new search in transposition table
            transpositionTable.NewSearch();

            Move bestMove = default;
            int bestScore = -Infinity;

            for (int depth = 1; depth <= Math.Min(maxDepth, MaxDepth); depth++)
            {
                if (shouldStop) break;

                // Always maximize from the current player's perspective
                int score = AlphaBeta(depth, -Infinity, Infinity, true, out Move currentBestMove);
                
                if (!shouldStop)
                {
                    bestMove = currentBestMove;
                    bestScore = score;
                }

                if (Math.Abs(score) >= MateValue - MaxDepth)
                {
                    break;
                }

                if (DateTime.UtcNow - searchStartTime >= maxSearchTime)
                {
                    break;
                }
            }

            TimeSpan searchTime = DateTime.UtcNow - searchStartTime;
            return new SearchResult(bestMove, bestScore, maxDepth, nodesSearched, searchTime);
        }

        private int AlphaBeta(int depth, int alpha, int beta, bool maximizingPlayer, out Move bestMove)
        {
            bestMove = default;
            nodesSearched++;
            
            ulong positionHash = board.HashKey;
            
            if (DateTime.UtcNow - searchStartTime >= maxSearchTime)
            {
                shouldStop = true;
                return 0;
            }

            // Check for repetition draw
            if (board.IsRepetition())
            {
                return 0; // Return draw score
            }

            // Probe transposition table
            int ttScore = transpositionTable.ProbeScore(positionHash, depth, alpha, beta, board.SideToMove);
            if (ttScore != int.MinValue)
            {
                // Get the hash move for move ordering even if we can't use the score
                transpositionTable.ProbeMove(positionHash, out bestMove);
                return ttScore;
            }

            if (depth == 0 || board.IsGameOver())
            {
                return Quiescence(alpha, beta, maximizingPlayer);
            }

            Span<Move> moveBuffer = stackalloc Move[256];
            int moveCount = board.GenerateLegalMoves(moveBuffer);
            
            if (moveCount == 0)
            {
                if (board.IsInCheck(board.SideToMove))
                {
                    return maximizingPlayer ? -MateValue + (MaxDepth - depth) : MateValue - (MaxDepth - depth);
                }
                return 0;
            }

            Move hashMove = default;
            transpositionTable.ProbeMove(positionHash, out hashMove);
            
            OrderMoves(moveBuffer.Slice(0, moveCount), hashMove);

            int maxEval = -Infinity;
            Move localBestMove = default;
            int originalAlpha = alpha;
            
            for (int i = 0; i < moveCount; i++)
            {
                if (shouldStop) break;
                
                Move move = moveBuffer[i];
                board.MakeMove(move);
                int eval = -AlphaBeta(depth - 1, -beta, -alpha, true, out _);
                board.UnmakeMove(move);

                if (eval > maxEval)
                {
                    maxEval = eval;
                    localBestMove = move;
                }

                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                {
                    break;
                }
            }
            
            bestMove = localBestMove;
            
            // Store in transposition table
            TTEntryType entryType;
            if (maxEval <= originalAlpha)
                entryType = TTEntryType.UpperBound; // All moves failed low
            else if (maxEval >= beta)
                entryType = TTEntryType.LowerBound; // Beta cutoff
            else
                entryType = TTEntryType.Exact; // Exact score
            
            transpositionTable.Store(positionHash, localBestMove, maxEval, depth, entryType, board.SideToMove);
            
            return maxEval;
        }

        private int Quiescence(int alpha, int beta, bool maximizingPlayer)
        {
            nodesSearched++;

            int standPat = Evaluator.Evaluate(board);
            // Negate evaluation if it's Black's turn (negamax)
            if (board.SideToMove == Color.Black)
                standPat = -standPat;

            if (standPat >= beta)
                return beta;
            
            alpha = Math.Max(alpha, standPat);

            Span<Move> captureMoves = stackalloc Move[64];
            int captureCount = GetCaptureMoves(captureMoves);
            OrderMoves(captureMoves.Slice(0, captureCount));

            for (int i = 0; i < captureCount; i++)
            {
                if (shouldStop) break;
                
                Move move = captureMoves[i];
                board.MakeMove(move);
                int score = -Quiescence(-beta, -alpha, true);
                board.UnmakeMove(move);

                alpha = Math.Max(alpha, score);
                if (beta <= alpha)
                    break;
            }

            return alpha;
        }

        private int GetCaptureMoves(Span<Move> captureMoves)
        {
            Span<Move> allMoves = stackalloc Move[256];
            int allMovesCount = board.GenerateLegalMoves(allMoves);
            int captureCount = 0;

            for (int i = 0; i < allMovesCount; i++)
            {
                Move move = allMoves[i];
                if (move.IsCapture || move.IsPromotion)
                {
                    if (captureCount < captureMoves.Length)
                    {
                        captureMoves[captureCount++] = move;
                    }
                }
            }

            return captureCount;
        }

        private List<Move> GetCaptureMoves()
        {
            List<Move> allMoves = board.GenerateLegalMoves();
            List<Move> captureMoves = new List<Move>();

            foreach (Move move in allMoves)
            {
                if (move.IsCapture || move.IsPromotion)
                {
                    captureMoves.Add(move);
                }
            }

            return captureMoves;
        }

        private void OrderMoves(Span<Move> moves, Move hashMove = default)
        {
            // Use insertion sort for stability with small move counts
            for (int i = 1; i < moves.Length; i++)
            {
                Move currentMove = moves[i];
                int currentScore = GetMoveOrderingScore(currentMove, hashMove);
                int j = i - 1;
                
                while (j >= 0)
                {
                    int compareScore = GetMoveOrderingScore(moves[j], hashMove);
                    if (compareScore >= currentScore) break;
                    
                    moves[j + 1] = moves[j];
                    j--;
                }
                
                moves[j + 1] = currentMove;
            }
        }

        private void OrderMoves(List<Move> moves, Move hashMove = default)
        {
            // Use stable sorting with move indices to ensure deterministic ordering
            var movesWithIndex = moves.Select((move, index) => new { Move = move, Index = index }).ToList();
            
            movesWithIndex.Sort((item1, item2) =>
            {
                int score1 = GetMoveOrderingScore(item1.Move, hashMove);
                int score2 = GetMoveOrderingScore(item2.Move, hashMove);
                
                // Primary sort by score (descending)
                int comparison = score2.CompareTo(score1);
                
                // Secondary sort by original index for stability (ascending)
                if (comparison == 0)
                {
                    comparison = item1.Index.CompareTo(item2.Index);
                }
                
                return comparison;
            });
            
            // Update the original list with sorted moves
            for (int i = 0; i < moves.Count; i++)
            {
                moves[i] = movesWithIndex[i].Move;
            }
        }

        private int GetMoveOrderingScore(Move move, Move hashMove = default)
        {
            int score = 0;
            
            // Hash move gets highest priority
            if (!hashMove.Equals(default) && move.Equals(hashMove))
            {
                score += 10000;
            }

            // Prioritize captures using MVV-LVA (Most Valuable Victim - Least Valuable Attacker)
            if (move.IsCapture)
            {
                int capturedValue = GetSimplePieceValue(move.CapturedPiece);
                int attackerValue = GetSimplePieceValue(move.MovedPiece);
                score += 1000 + capturedValue - attackerValue;
            }

            // Prioritize promotions
            if (move.IsPromotion)
            {
                score += 900;
            }

            // Prioritize castling
            if (move.IsCastling)
            {
                score += 50;
            }
            
            // Add small bonus for center moves to encourage development
            int toFile = Board.GetFile(move.To);
            int toRank = Board.GetRank(move.To);
            if ((toFile == Board.FileD || toFile == Board.FileE) && 
                (toRank == Board.Rank4 || toRank == Board.Rank5))
            {
                score += 10;
            }

            return score;
        }

        private int GetSimplePieceValue(int piece)
        {
            return Piece.GetType(piece) switch
            {
                PieceType.Pawn => 100,
                PieceType.Knight => 320,
                PieceType.Bishop => 330,
                PieceType.Rook => 500,
                PieceType.Queen => 900,
                PieceType.King => 20000,
                _ => 0
            };
        }

        public Move GetBestMove(int depth = 6, int timeMs = 5000)
        {
            TimeSpan maxTime = TimeSpan.FromMilliseconds(timeMs);
            SearchResult result = Search(depth, maxTime);
            return result.BestMove;
        }

        public void StopSearch()
        {
            shouldStop = true;
        }
    }
}