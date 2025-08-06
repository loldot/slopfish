namespace ChessEngine
{
    public enum PieceType
    {
        None = 0,
        Pawn = 1,
        Knight = 2,
        Bishop = 3,
        Rook = 4,
        Queen = 5,
        King = 6
    }

    public enum Color
    {
        White = 0,
        Black = 1
    }

    public static class Piece
    {
        public const int None = 0;
        public const int WhitePawn = 1;
        public const int WhiteKnight = 2;
        public const int WhiteBishop = 3;
        public const int WhiteRook = 4;
        public const int WhiteQueen = 5;
        public const int WhiteKing = 6;
        public const int BlackPawn = 9;
        public const int BlackKnight = 10;
        public const int BlackBishop = 11;
        public const int BlackRook = 12;
        public const int BlackQueen = 13;
        public const int BlackKing = 14;

        public const int OffBoard = -1;

        public static Color GetColor(int piece)
        {
            return piece < 8 ? Color.White : Color.Black;
        }

        public static PieceType GetType(int piece)
        {
            if (piece == None) return PieceType.None;
            return (PieceType)(piece % 8);
        }

        public static int MakePiece(Color color, PieceType type)
        {
            return (int)type + (color == Color.Black ? 8 : 0);
        }

        public static bool IsWhite(int piece)
        {
            return piece >= 1 && piece <= 6;
        }

        public static bool IsBlack(int piece)
        {
            return piece >= 9 && piece <= 14;
        }

        public static bool IsPawn(int piece)
        {
            return GetType(piece) == PieceType.Pawn;
        }

        public static bool IsKnight(int piece)
        {
            return GetType(piece) == PieceType.Knight;
        }

        public static bool IsBishop(int piece)
        {
            return GetType(piece) == PieceType.Bishop;
        }

        public static bool IsRook(int piece)
        {
            return GetType(piece) == PieceType.Rook;
        }

        public static bool IsQueen(int piece)
        {
            return GetType(piece) == PieceType.Queen;
        }

        public static bool IsKing(int piece)
        {
            return GetType(piece) == PieceType.King;
        }

        public static bool IsSlidingPiece(int piece)
        {
            var type = GetType(piece);
            return type == PieceType.Bishop || type == PieceType.Rook || type == PieceType.Queen;
        }

        public static char ToChar(int piece)
        {
            return piece switch
            {
                WhitePawn => 'P',
                WhiteKnight => 'N',
                WhiteBishop => 'B',
                WhiteRook => 'R',
                WhiteQueen => 'Q',
                WhiteKing => 'K',
                BlackPawn => 'p',
                BlackKnight => 'n',
                BlackBishop => 'b',
                BlackRook => 'r',
                BlackQueen => 'q',
                BlackKing => 'k',
                _ => '.'
            };
        }
    }
}