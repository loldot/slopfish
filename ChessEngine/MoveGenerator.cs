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
            List<Move> moves = new List<Move>();
            Color sideToMove = board.SideToMove;

            for (int square = 0; square < Board.BoardSize; square++)
            {
                if (!board.IsValidSquare(square)) continue;

                int piece = board.GetPiece(square);
                if (piece == Piece.None || Piece.GetColor(piece) != sideToMove) continue;

                switch (Piece.GetType(piece))
                {
                    case PieceType.Pawn:
                        GeneratePawnMoves(board, square, moves);
                        break;
                    case PieceType.Knight:
                        GenerateKnightMoves(board, square, moves);
                        break;
                    case PieceType.Bishop:
                        GenerateBishopMoves(board, square, moves);
                        break;
                    case PieceType.Rook:
                        GenerateRookMoves(board, square, moves);
                        break;
                    case PieceType.Queen:
                        GenerateQueenMoves(board, square, moves);
                        break;
                    case PieceType.King:
                        GenerateKingMoves(board, square, moves);
                        break;
                }
            }

            GenerateCastlingMoves(board, moves);
            return moves;
        }

        private static void GeneratePawnMoves(Board board, int square, List<Move> moves)
        {
            int piece = board.GetPiece(square);
            Color color = Piece.GetColor(piece);
            int direction = color == Color.White ? 10 : -10;
            int startRank = color == Color.White ? Board.Rank2 : Board.Rank7;
            int promotionRank = color == Color.White ? Board.Rank8 : Board.Rank1;

            int oneSquareAhead = square + direction;
            if (board.IsValidSquare(oneSquareAhead) && board.GetPiece(oneSquareAhead) == Piece.None)
            {
                if (Board.GetRank(oneSquareAhead) == promotionRank / 10)
                {
                    AddPromotionMoves(board, square, oneSquareAhead, moves);
                }
                else
                {
                    bool isDoublePush = false;
                    moves.Add(new Move(square, oneSquareAhead, piece, Piece.None, Piece.None, false, false, isDoublePush));

                    if (Board.GetRank(square) == startRank / 10)
                    {
                        int twoSquaresAhead = square + (direction * 2);
                        if (board.IsValidSquare(twoSquaresAhead) && board.GetPiece(twoSquaresAhead) == Piece.None)
                        {
                            isDoublePush = true;
                            moves.Add(new Move(square, twoSquaresAhead, piece, Piece.None, Piece.None, false, false, isDoublePush));
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
                        AddPromotionMoves(board, square, captureSquare, moves, capturedPiece);
                    }
                    else
                    {
                        moves.Add(new Move(square, captureSquare, piece, capturedPiece));
                    }
                }
                else if (captureSquare == board.EnPassantSquare)
                {
                    moves.Add(new Move(square, captureSquare, piece, Piece.None, Piece.None, true));
                }
            }
        }

        private static void AddPromotionMoves(Board board, int from, int to, List<Move> moves, int capturedPiece = Piece.None)
        {
            int piece = board.GetPiece(from);
            Color color = Piece.GetColor(piece);

            int[] promotionPieces = color == Color.White
                ? new[] { Piece.WhiteQueen, Piece.WhiteRook, Piece.WhiteBishop, Piece.WhiteKnight }
                : new[] { Piece.BlackQueen, Piece.BlackRook, Piece.BlackBishop, Piece.BlackKnight };

            foreach (int promotionPiece in promotionPieces)
            {
                moves.Add(new Move(from, to, piece, capturedPiece, promotionPiece));
            }
        }

        private static void GenerateKnightMoves(Board board, int square, List<Move> moves)
        {
            int piece = board.GetPiece(square);
            Color color = Piece.GetColor(piece);

            foreach (int offset in KnightMoves)
            {
                int targetSquare = square + offset;
                if (!board.IsValidSquare(targetSquare)) continue;

                int targetPiece = board.GetPiece(targetSquare);
                if (targetPiece == Piece.None || Piece.GetColor(targetPiece) != color)
                {
                    moves.Add(new Move(square, targetSquare, piece, targetPiece));
                }
            }
        }

        private static void GenerateBishopMoves(Board board, int square, List<Move> moves)
        {
            GenerateSlidingMoves(board, square, BishopDirections, moves);
        }

        private static void GenerateRookMoves(Board board, int square, List<Move> moves)
        {
            GenerateSlidingMoves(board, square, RookDirections, moves);
        }

        private static void GenerateQueenMoves(Board board, int square, List<Move> moves)
        {
            GenerateSlidingMoves(board, square, QueenDirections, moves);
        }

        private static void GenerateSlidingMoves(Board board, int square, int[] directions, List<Move> moves)
        {
            int piece = board.GetPiece(square);
            Color color = Piece.GetColor(piece);

            foreach (int direction in directions)
            {
                for (int targetSquare = square + direction;
                     board.IsValidSquare(targetSquare);
                     targetSquare += direction)
                {
                    int targetPiece = board.GetPiece(targetSquare);

                    if (targetPiece == Piece.None)
                    {
                        moves.Add(new Move(square, targetSquare, piece, Piece.None));
                    }
                    else
                    {
                        if (Piece.GetColor(targetPiece) != color)
                        {
                            moves.Add(new Move(square, targetSquare, piece, targetPiece));
                        }
                        break;
                    }
                }
            }
        }

        private static void GenerateKingMoves(Board board, int square, List<Move> moves)
        {
            int piece = board.GetPiece(square);
            Color color = Piece.GetColor(piece);

            foreach (int offset in KingMoves)
            {
                int targetSquare = square + offset;
                if (!board.IsValidSquare(targetSquare)) continue;

                int targetPiece = board.GetPiece(targetSquare);
                if (targetPiece == Piece.None || Piece.GetColor(targetPiece) != color)
                {
                    moves.Add(new Move(square, targetSquare, piece, targetPiece));
                }
            }
        }

        private static void GenerateCastlingMoves(Board board, List<Move> moves)
        {
            Color color = board.SideToMove;
            Color enemyColor = color == Color.White ? Color.Black : Color.White;

            // Cannot castle if king is in check
            if (board.IsInCheck(color)) return;

            if (color == Color.White)
            {
                if (board.WhiteCanCastleKingside)
                {
                    int kingSquare = Board.MakeSquare(Board.FileE, Board.Rank1);
                    int rookSquare = Board.MakeSquare(Board.FileH, Board.Rank1);

                    // Check path is clear
                    if (board.GetPiece(Board.MakeSquare(Board.FileF, Board.Rank1)) == Piece.None &&
                        board.GetPiece(Board.MakeSquare(Board.FileG, Board.Rank1)) == Piece.None)
                    {
                        // Check king doesn't pass through or end up in check
                        if (!IsSquareAttacked(board, Board.MakeSquare(Board.FileF, Board.Rank1), enemyColor) &&
                            !IsSquareAttacked(board, Board.MakeSquare(Board.FileG, Board.Rank1), enemyColor))
                        {
                            moves.Add(new Move(kingSquare, Board.MakeSquare(Board.FileG, Board.Rank1),
                                            Piece.WhiteKing, Piece.None, Piece.None, false, true));
                        }
                    }
                }

                if (board.WhiteCanCastleQueenside)
                {
                    int kingSquare = Board.MakeSquare(Board.FileE, Board.Rank1);

                    // Check path is clear
                    if (board.GetPiece(Board.MakeSquare(Board.FileD, Board.Rank1)) == Piece.None &&
                        board.GetPiece(Board.MakeSquare(Board.FileC, Board.Rank1)) == Piece.None &&
                        board.GetPiece(Board.MakeSquare(Board.FileB, Board.Rank1)) == Piece.None)
                    {
                        // Check king doesn't pass through or end up in check (note: B1 doesn't need to be checked)
                        if (!IsSquareAttacked(board, Board.MakeSquare(Board.FileD, Board.Rank1), enemyColor) &&
                            !IsSquareAttacked(board, Board.MakeSquare(Board.FileC, Board.Rank1), enemyColor))
                        {
                            moves.Add(new Move(kingSquare, Board.MakeSquare(Board.FileC, Board.Rank1),
                                            Piece.WhiteKing, Piece.None, Piece.None, false, true));
                        }
                    }
                }
            }
            else
            {
                if (board.BlackCanCastleKingside)
                {
                    int kingSquare = Board.MakeSquare(Board.FileE, Board.Rank8);

                    // Check path is clear
                    if (board.GetPiece(Board.MakeSquare(Board.FileF, Board.Rank8)) == Piece.None &&
                        board.GetPiece(Board.MakeSquare(Board.FileG, Board.Rank8)) == Piece.None)
                    {
                        // Check king doesn't pass through or end up in check
                        if (!IsSquareAttacked(board, Board.MakeSquare(Board.FileF, Board.Rank8), enemyColor) &&
                            !IsSquareAttacked(board, Board.MakeSquare(Board.FileG, Board.Rank8), enemyColor))
                        {
                            moves.Add(new Move(kingSquare, Board.MakeSquare(Board.FileG, Board.Rank8),
                                              Piece.BlackKing, Piece.None, Piece.None, false, true));
                        }
                    }
                }
            }

            if (board.BlackCanCastleQueenside)
            {
                int kingSquare = Board.MakeSquare(Board.FileE, Board.Rank8);

                // Check path is clear
                if (board.GetPiece(Board.MakeSquare(Board.FileD, Board.Rank8)) == Piece.None &&
                    board.GetPiece(Board.MakeSquare(Board.FileC, Board.Rank8)) == Piece.None &&
                    board.GetPiece(Board.MakeSquare(Board.FileB, Board.Rank8)) == Piece.None)
                {
                    // Check king doesn't pass through or end up in check (note: B8 doesn't need to be checked)
                    if (!IsSquareAttacked(board, Board.MakeSquare(Board.FileD, Board.Rank8), enemyColor) &&
                        !IsSquareAttacked(board, Board.MakeSquare(Board.FileC, Board.Rank8), enemyColor))
                    {
                        moves.Add(new Move(kingSquare, Board.MakeSquare(Board.FileC, Board.Rank8),
                                          Piece.BlackKing, Piece.None, Piece.None, false, true));
                    }
                }
            }

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