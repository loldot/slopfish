namespace ChessEngine
{
    public static class FenParser
    {
        public static bool ParseFen(Board board, string fen)
        {
            if (string.IsNullOrWhiteSpace(fen))
                return false;

            string[] parts = fen.Split(' ');
            if (parts.Length != 6)
                return false;

            try
            {
                ParsePiecePlacement(board, parts[0]);
                
                if (parts[1] == "w")
                    board.SideToMove = Color.White;
                else if (parts[1] == "b")
                    board.SideToMove = Color.Black;
                else
                    throw new ArgumentException("Invalid side to move");
                
                ParseCastlingRights(board, parts[2]);
                
                ParseEnPassant(board, parts[3]);
                
                board.HalfMoveClock = int.Parse(parts[4]);
                board.FullMoveNumber = int.Parse(parts[5]);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void ParsePiecePlacement(Board board, string piecePlacement)
        {
            for (int i = 0; i < Board.BoardSize; i++)
            {
                if (board.IsValidSquare(i))
                    board.SetPiece(i, Piece.None);
            }

            string[] ranks = piecePlacement.Split('/');
            if (ranks.Length != 8)
                throw new ArgumentException("Invalid piece placement");

            for (int rank = 0; rank < 8; rank++)
            {
                int file = 0;
                foreach (char c in ranks[rank])
                {
                    if (char.IsDigit(c))
                    {
                        file += c - '0';
                    }
                    else
                    {
                        int piece = CharToPiece(c);
                        int square = Board.MakeSquare(file + 1, (8 - rank) * 10);
                        board.SetPiece(square, piece);
                        file++;
                    }
                }
            }
        }

        private static void ParseCastlingRights(Board board, string castlingRights)
        {
            board.WhiteCanCastleKingside = false;
            board.WhiteCanCastleQueenside = false;
            board.BlackCanCastleKingside = false;
            board.BlackCanCastleQueenside = false;

            if (castlingRights != "-")
            {
                foreach (char c in castlingRights)
                {
                    switch (c)
                    {
                        case 'K': board.WhiteCanCastleKingside = true; break;
                        case 'Q': board.WhiteCanCastleQueenside = true; break;
                        case 'k': board.BlackCanCastleKingside = true; break;
                        case 'q': board.BlackCanCastleQueenside = true; break;
                    }
                }
            }
        }

        private static void ParseEnPassant(Board board, string enPassant)
        {
            if (enPassant == "-")
            {
                board.EnPassantSquare = -1;
            }
            else
            {
                board.EnPassantSquare = Board.AlgebraicToSquare(enPassant);
            }
        }

        private static int CharToPiece(char c)
        {
            return c switch
            {
                'P' => Piece.WhitePawn,
                'N' => Piece.WhiteKnight,
                'B' => Piece.WhiteBishop,
                'R' => Piece.WhiteRook,
                'Q' => Piece.WhiteQueen,
                'K' => Piece.WhiteKing,
                'p' => Piece.BlackPawn,
                'n' => Piece.BlackKnight,
                'b' => Piece.BlackBishop,
                'r' => Piece.BlackRook,
                'q' => Piece.BlackQueen,
                'k' => Piece.BlackKing,
                _ => Piece.None
            };
        }

        public static string BoardToFen(Board board)
        {
            string piecePlacement = GeneratePiecePlacement(board);
            string sideToMove = board.SideToMove == Color.White ? "w" : "b";
            string castlingRights = GenerateCastlingRights(board);
            string enPassant = board.EnPassantSquare == -1 ? "-" : Board.SquareToAlgebraic(board.EnPassantSquare);
            
            return $"{piecePlacement} {sideToMove} {castlingRights} {enPassant} {board.HalfMoveClock} {board.FullMoveNumber}";
        }

        private static string GeneratePiecePlacement(Board board)
        {
            var ranks = new List<string>();

            for (int rank = 8; rank >= 1; rank--)
            {
                string rankStr = "";
                int emptyCount = 0;

                for (int file = 1; file <= 8; file++)
                {
                    int square = Board.MakeSquare(file, rank * 10);
                    int piece = board.GetPiece(square);

                    if (piece == Piece.None)
                    {
                        emptyCount++;
                    }
                    else
                    {
                        if (emptyCount > 0)
                        {
                            rankStr += emptyCount.ToString();
                            emptyCount = 0;
                        }
                        rankStr += Piece.ToChar(piece);
                    }
                }

                if (emptyCount > 0)
                {
                    rankStr += emptyCount.ToString();
                }

                ranks.Add(rankStr);
            }

            return string.Join("/", ranks);
        }

        private static string GenerateCastlingRights(Board board)
        {
            string rights = "";
            
            if (board.WhiteCanCastleKingside) rights += "K";
            if (board.WhiteCanCastleQueenside) rights += "Q";
            if (board.BlackCanCastleKingside) rights += "k";
            if (board.BlackCanCastleQueenside) rights += "q";
            
            return string.IsNullOrEmpty(rights) ? "-" : rights;
        }

        public const string StartingPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    }

    public partial class Board
    {
        public bool LoadFromFen(string fen)
        {
            return FenParser.ParseFen(this, fen);
        }

        public string ToFen()
        {
            return FenParser.BoardToFen(this);
        }

        public void SetupFromFen(string? fen = null)
        {
            string fenToUse = fen ?? FenParser.StartingPositionFen;
            if (!LoadFromFen(fenToUse))
            {
                SetupStartingPosition();
            }
        }
    }
}