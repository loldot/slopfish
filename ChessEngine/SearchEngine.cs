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

        public SearchEngine(Board board)
        {
            this.board = board;
        }

        public SearchResult Search(int maxDepth, TimeSpan maxTime)
        {
            nodesSearched = 0;
            searchStartTime = DateTime.UtcNow;
            maxSearchTime = maxTime;
            shouldStop = false;

            Move bestMove = default;
            int bestScore = -Infinity;

            for (int depth = 1; depth <= Math.Min(maxDepth, MaxDepth); depth++)
            {
                if (shouldStop) break;

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

            if (DateTime.UtcNow - searchStartTime >= maxSearchTime)
            {
                shouldStop = true;
                return 0;
            }

            if (depth == 0 || board.IsGameOver())
            {
                return Quiescence(alpha, beta, maximizingPlayer);
            }

            List<Move> moves = board.GenerateLegalMoves();
            
            if (moves.Count == 0)
            {
                if (board.IsInCheck(board.SideToMove))
                {
                    return maximizingPlayer ? -MateValue + (MaxDepth - depth) : MateValue - (MaxDepth - depth);
                }
                return 0;
            }

            OrderMoves(moves);

            if (maximizingPlayer)
            {
                int maxEval = -Infinity;
                
                foreach (Move move in moves)
                {
                    if (shouldStop) break;

                    board.MakeMove(move);
                    int eval = AlphaBeta(depth - 1, alpha, beta, false, out _);
                    board.UnmakeMove(move);

                    if (eval > maxEval)
                    {
                        maxEval = eval;
                        bestMove = move;
                    }

                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                
                return maxEval;
            }
            else
            {
                int minEval = Infinity;
                
                foreach (Move move in moves)
                {
                    if (shouldStop) break;

                    board.MakeMove(move);
                    int eval = AlphaBeta(depth - 1, alpha, beta, true, out _);
                    board.UnmakeMove(move);

                    if (eval < minEval)
                    {
                        minEval = eval;
                        bestMove = move;
                    }

                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                
                return minEval;
            }
        }

        private int Quiescence(int alpha, int beta, bool maximizingPlayer)
        {
            nodesSearched++;

            int standPat = Evaluator.Evaluate(board);

            if (maximizingPlayer)
            {
                if (standPat >= beta)
                    return beta;
                
                alpha = Math.Max(alpha, standPat);
            }
            else
            {
                if (standPat <= alpha)
                    return alpha;
                
                beta = Math.Min(beta, standPat);
            }

            List<Move> captureMoves = GetCaptureMoves();
            OrderMoves(captureMoves);

            foreach (Move move in captureMoves)
            {
                if (shouldStop) break;

                board.MakeMove(move);
                int score = Quiescence(alpha, beta, !maximizingPlayer);
                board.UnmakeMove(move);

                if (maximizingPlayer)
                {
                    alpha = Math.Max(alpha, score);
                    if (beta <= alpha)
                        break;
                }
                else
                {
                    beta = Math.Min(beta, score);
                    if (beta <= alpha)
                        break;
                }
            }

            return maximizingPlayer ? alpha : beta;
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

        private void OrderMoves(List<Move> moves)
        {
            moves.Sort((move1, move2) =>
            {
                int score1 = GetMoveOrderingScore(move1);
                int score2 = GetMoveOrderingScore(move2);
                return score2.CompareTo(score1);
            });
        }

        private int GetMoveOrderingScore(Move move)
        {
            int score = 0;

            if (move.IsCapture)
            {
                int capturedValue = GetSimplePieceValue(move.CapturedPiece);
                int attackerValue = GetSimplePieceValue(move.MovedPiece);
                score += 1000 + capturedValue - attackerValue;
            }

            if (move.IsPromotion)
            {
                score += 900;
            }

            if (board.IsInCheck(board.SideToMove == Color.White ? Color.Black : Color.White))
            {
                board.MakeMove(move);
                if (board.IsInCheck(board.SideToMove))
                {
                    score += 500;
                }
                board.UnmakeMove(move);
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