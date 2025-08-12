namespace ChessEngine
{
    public static class MoveGenerator
    {
        private static readonly int[] KnightMoves = { -21, -19, -12, -8, 8, 12, 19, 21 };
        private static readonly int[] KingMoves = { -11, -10, -9, -1, 1, 9, 10, 11 };
        private static readonly int[] BishopDirections = { -11, -9, 9, 11 };
        private static readonly int[] RookDirections = { -10, -1, 1, 10 };
        private static readonly int[] QueenDirections = { -11, -10, -9, -1, 1, 9, 10, 11 };

        public static List<Move> GenerateAllMoves(Board board)
        {
            Span<Move> moveBuffer = stackalloc Move[256];
            int moveCount = GenerateAllMoves(board, moveBuffer);
            
            var moves = new List<Move>(moveCount);
            for (int i = 0; i < moveCount; i++)
            {
                moves.Add(moveBuffer[i]);
            }
            return moves;
        }

        public static int GenerateAllMoves(Board board, Span<Move> moves)
        {
            int moveCount = 0;
            Color sideToMove = board.SideToMove;

            for (int square = 0; square < Board.BoardSize; square++)
            {
                if (!board.IsValidSquare(square)) continue;

                int piece = board.GetPiece(square);
                if (piece == Piece.None || Piece.GetColor(piece) != sideToMove) continue;

                switch (Piece.GetType(piece))
                {
                    case PieceType.Pawn:
                        moveCount += GeneratePawnMoves(board, square, moves[moveCount..]);
                        break;
                    case PieceType.Knight:
                        moveCount += GenerateKnightMoves(board, square, moves[moveCount..]);
                        break;
                    case PieceType.Bishop:
                        moveCount += GenerateBishopMoves(board, square, moves[moveCount..]);
                        break;
                    case PieceType.Rook:
                        moveCount += GenerateRookMoves(board, square, moves[moveCount..]);
                        break;
                    case PieceType.Queen:
                        moveCount += GenerateQueenMoves(board, square, moves[moveCount..]);
                        break;
                    case PieceType.King:
                        moveCount += GenerateKingMoves(board, square, moves[moveCount..]);
                        break;
                }
            }

            moveCount += GenerateCastlingMoves(board, moves[moveCount..]);
            return moveCount;
        }

        private static int GeneratePawnMoves(Board board, int square, Span<Move> moves)
        {
            int piece = board.GetPiece(square);
            Color color = Piece.GetColor(piece);
            int direction = color == Color.White ? 10 : -10;
            int startRank = color == Color.White ? Board.Rank2 : Board.Rank7;
            int promotionRank = color == Color.White ? Board.Rank8 : Board.Rank1;
            int moveCount = 0;

            int oneSquareAhead = square + direction;
            if (board.IsValidSquare(oneSquareAhead) && board.GetPiece(oneSquareAhead) == Piece.None)
            {
                if (Board.GetRank(oneSquareAhead) == promotionRank / 10)
                {
                    moveCount += AddPromotionMoves(board, square, oneSquareAhead, moves[moveCount..]);
                }
                else
                {
                    bool isDoublePush = false;
                    moves[moveCount++] = new Move(square, oneSquareAhead, piece, Piece.None, Piece.None, false, false, isDoublePush);

                    if (Board.GetRank(square) == startRank / 10)
                    {
                        int twoSquaresAhead = square + (direction * 2);
                        if (board.IsValidSquare(twoSquaresAhead) && board.GetPiece(twoSquaresAhead) == Piece.None)
                        {
                            isDoublePush = true;
                            moves[moveCount++] = new Move(square, twoSquaresAhead, piece, Piece.None, Piece.None, false, false, isDoublePush);
                        }
                    }
                }
            }

            int[] captureDirections = { direction - 1, direction + 1 };
            foreach (int captureDirection in captureDirections)
            {
                int captureSquare = square + captureDirection;
                if (!board.IsValidSquare(captureSquare)) continue;

                int capturedPiece = board.GetPiece(captureSquare);
                if (capturedPiece != Piece.None && Piece.GetColor(capturedPiece) != color)
                {
                    if (Board.GetRank(captureSquare) == promotionRank / 10)
                    {
                        moveCount += AddPromotionMoves(board, square, captureSquare, moves[moveCount..], capturedPiece);
                    }
                    else
                    {
                        moves[moveCount++] = new Move(square, captureSquare, piece, capturedPiece);
                    }
                }
                else if (captureSquare == board.EnPassantSquare)
                {
                    moves[moveCount++] = new Move(square, captureSquare, piece, Piece.None, Piece.None, true);
                }
            }
            
            return moveCount;
        }

        private static int AddPromotionMoves(Board board, int from, int to, Span<Move> moves, int capturedPiece = Piece.None)
        {
            int piece = board.GetPiece(from);
            Color color = Piece.GetColor(piece);
            int moveCount = 0;

            int[] promotionPieces = color == Color.White
                ? new[] { Piece.WhiteQueen, Piece.WhiteRook, Piece.WhiteBishop, Piece.WhiteKnight }
                : new[] { Piece.BlackQueen, Piece.BlackRook, Piece.BlackBishop, Piece.BlackKnight };

            foreach (int promotionPiece in promotionPieces)
            {
                moves[moveCount++] = new Move(from, to, piece, capturedPiece, promotionPiece);
            }
            
            return moveCount;
        }

        private static int GenerateKnightMoves(Board board, int square, Span<Move> moves)
        {
            int piece = board.GetPiece(square);
            Color color = Piece.GetColor(piece);
            int moveCount = 0;

            foreach (int offset in KnightMoves)
            {
                int targetSquare = square + offset;
                if (!board.IsValidSquare(targetSquare)) continue;

                int targetPiece = board.GetPiece(targetSquare);
                if (targetPiece == Piece.None || Piece.GetColor(targetPiece) != color)
                {
                    moves[moveCount++] = new Move(square, targetSquare, piece, targetPiece);
                }
            }
            
            return moveCount;
        }

        private static int GenerateBishopMoves(Board board, int square, Span<Move> moves)
        {
            return GenerateSlidingMoves(board, square, BishopDirections, moves);
        }

        private static int GenerateRookMoves(Board board, int square, Span<Move> moves)
        {
            return GenerateSlidingMoves(board, square, RookDirections, moves);
        }

        private static int GenerateQueenMoves(Board board, int square, Span<Move> moves)
        {
            return GenerateSlidingMoves(board, square, QueenDirections, moves);
        }

        private static int GenerateSlidingMoves(Board board, int square, int[] directions, Span<Move> moves)
        {
            int piece = board.GetPiece(square);
            Color color = Piece.GetColor(piece);
            int moveCount = 0;

            foreach (int direction in directions)
            {
                for (int targetSquare = square + direction;
                     board.IsValidSquare(targetSquare);
                     targetSquare += direction)
                {
                    int targetPiece = board.GetPiece(targetSquare);

                    if (targetPiece == Piece.None)
                    {
                        moves[moveCount++] = new Move(square, targetSquare, piece, Piece.None);
                    }
                    else
                    {
                        if (Piece.GetColor(targetPiece) != color)
                        {
                            moves[moveCount++] = new Move(square, targetSquare, piece, targetPiece);
                        }
                        break;
                    }
                }
            }
            
            return moveCount;
        }

        private static int GenerateKingMoves(Board board, int square, Span<Move> moves)
        {
            int piece = board.GetPiece(square);
            Color color = Piece.GetColor(piece);
            int moveCount = 0;

            foreach (int offset in KingMoves)
            {
                int targetSquare = square + offset;
                if (!board.IsValidSquare(targetSquare)) continue;

                int targetPiece = board.GetPiece(targetSquare);
                if (targetPiece == Piece.None || Piece.GetColor(targetPiece) != color)
                {
                    moves[moveCount++] = new Move(square, targetSquare, piece, targetPiece);
                }
            }
            
            return moveCount;
        }

        private static int GenerateCastlingMoves(Board board, Span<Move> moves)
        {
            Color color = board.SideToMove;
            Color enemyColor = color == Color.White ? Color.Black : Color.White;
            int moveCount = 0;

            if (board.IsInCheck(color)) return 0;

            if (color == Color.White)
            {
                if (board.WhiteCanCastleKingside)
                {
                    int kingSquare = Board.MakeSquare(Board.FileE, Board.Rank1);

                    if (board.GetPiece(Board.MakeSquare(Board.FileF, Board.Rank1)) == Piece.None &&
                        board.GetPiece(Board.MakeSquare(Board.FileG, Board.Rank1)) == Piece.None)
                    {
                        if (!IsSquareAttacked(board, Board.MakeSquare(Board.FileF, Board.Rank1), enemyColor) &&
                            !IsSquareAttacked(board, Board.MakeSquare(Board.FileG, Board.Rank1), enemyColor))
                        {
                            moves[moveCount++] = new Move(kingSquare, Board.MakeSquare(Board.FileG, Board.Rank1),
                                            Piece.WhiteKing, Piece.None, Piece.None, false, true);
                        }
                    }
                }

                if (board.WhiteCanCastleQueenside)
                {
                    int kingSquare = Board.MakeSquare(Board.FileE, Board.Rank1);

                    if (board.GetPiece(Board.MakeSquare(Board.FileD, Board.Rank1)) == Piece.None &&
                        board.GetPiece(Board.MakeSquare(Board.FileC, Board.Rank1)) == Piece.None &&
                        board.GetPiece(Board.MakeSquare(Board.FileB, Board.Rank1)) == Piece.None)
                    {
                        if (!IsSquareAttacked(board, Board.MakeSquare(Board.FileD, Board.Rank1), enemyColor) &&
                            !IsSquareAttacked(board, Board.MakeSquare(Board.FileC, Board.Rank1), enemyColor))
                        {
                            moves[moveCount++] = new Move(kingSquare, Board.MakeSquare(Board.FileC, Board.Rank1),
                                            Piece.WhiteKing, Piece.None, Piece.None, false, true);
                        }
                    }
                }
            }
            else
            {
                if (board.BlackCanCastleKingside)
                {
                    int kingSquare = Board.MakeSquare(Board.FileE, Board.Rank8);

                    if (board.GetPiece(Board.MakeSquare(Board.FileF, Board.Rank8)) == Piece.None &&
                        board.GetPiece(Board.MakeSquare(Board.FileG, Board.Rank8)) == Piece.None)
                    {
                        if (!IsSquareAttacked(board, Board.MakeSquare(Board.FileF, Board.Rank8), enemyColor) &&
                            !IsSquareAttacked(board, Board.MakeSquare(Board.FileG, Board.Rank8), enemyColor))
                        {
                            moves[moveCount++] = new Move(kingSquare, Board.MakeSquare(Board.FileG, Board.Rank8),
                                              Piece.BlackKing, Piece.None, Piece.None, false, true);
                        }
                    }
                }

                if (board.BlackCanCastleQueenside)
                {
                    int kingSquare = Board.MakeSquare(Board.FileE, Board.Rank8);

                    if (board.GetPiece(Board.MakeSquare(Board.FileD, Board.Rank8)) == Piece.None &&
                        board.GetPiece(Board.MakeSquare(Board.FileC, Board.Rank8)) == Piece.None &&
                        board.GetPiece(Board.MakeSquare(Board.FileB, Board.Rank8)) == Piece.None)
                    {
                        if (!IsSquareAttacked(board, Board.MakeSquare(Board.FileD, Board.Rank8), enemyColor) &&
                            !IsSquareAttacked(board, Board.MakeSquare(Board.FileC, Board.Rank8), enemyColor))
                        {
                            moves[moveCount++] = new Move(kingSquare, Board.MakeSquare(Board.FileC, Board.Rank8),
                                              Piece.BlackKing, Piece.None, Piece.None, false, true);
                        }
                    }
                }
            }

            return moveCount;

        }

        public static bool IsSquareAttacked(Board board, int square, Color attackingColor)
        {
            for (int fromSquare = 0; fromSquare < Board.BoardSize; fromSquare++)
            {
                if (!board.IsValidSquare(fromSquare)) continue;

                int piece = board.GetPiece(fromSquare);
                if (piece == Piece.None || Piece.GetColor(piece) != attackingColor) continue;

                if (CanPieceAttackSquare(board, fromSquare, square, piece))
                    return true;
            }

            return false;
        }

        private static bool CanPieceAttackSquare(Board board, int fromSquare, int toSquare, int piece)
        {
            PieceType pieceType = Piece.GetType(piece);
            Color color = Piece.GetColor(piece);

            switch (pieceType)
            {
                case PieceType.Pawn:
                    int direction = color == Color.White ? 10 : -10;
                    int leftCapture = fromSquare + direction - 1;
                    int rightCapture = fromSquare + direction + 1;
                    return toSquare == leftCapture || toSquare == rightCapture;

                case PieceType.Knight:
                    foreach (int offset in KnightMoves)
                    {
                        if (fromSquare + offset == toSquare)
                            return true;
                    }
                    return false;

                case PieceType.Bishop:
                    return IsSlidingAttack(board, fromSquare, toSquare, BishopDirections);

                case PieceType.Rook:
                    return IsSlidingAttack(board, fromSquare, toSquare, RookDirections);

                case PieceType.Queen:
                    return IsSlidingAttack(board, fromSquare, toSquare, QueenDirections);

                case PieceType.King:
                    foreach (int offset in KingMoves)
                    {
                        if (fromSquare + offset == toSquare)
                            return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        private static bool IsSlidingAttack(Board board, int fromSquare, int toSquare, int[] directions)
        {
            foreach (int direction in directions)
            {
                int square = fromSquare + direction;
                while (board.IsValidSquare(square))
                {
                    if (square == toSquare)
                        return true;
                    if (board.GetPiece(square) != Piece.None)
                        break;
                    square += direction;
                }
            }
            return false;
        }
    }
}