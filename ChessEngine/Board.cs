namespace ChessEngine
{
    public partial class Board
    {
        public const int BoardSize = 120;
        public const int FileA = 1, FileB = 2, FileC = 3, FileD = 4, FileE = 5, FileF = 6, FileG = 7, FileH = 8;
        public const int Rank1 = 10, Rank2 = 20, Rank3 = 30, Rank4 = 40, Rank5 = 50, Rank6 = 60, Rank7 = 70, Rank8 = 80;

        private readonly int[] squares = new int[BoardSize];
        
        public Color SideToMove { get; set; } = Color.White;
        public int HalfMoveClock { get; set; } = 0;
        public int FullMoveNumber { get; set; } = 1;
        public int EnPassantSquare { get; set; } = -1;
        public ulong HashKey { get; set; } = 0;
        
        public bool WhiteCanCastleKingside { get; set; } = true;
        public bool WhiteCanCastleQueenside { get; set; } = true;
        public bool BlackCanCastleKingside { get; set; } = true;
        public bool BlackCanCastleQueenside { get; set; } = true;

        public Board()
        {
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            for (int i = 0; i < BoardSize; i++)
            {
                squares[i] = Piece.OffBoard;
            }

            for (int file = FileA; file <= FileH; file++)
            {
                for (int rank = Rank1; rank <= Rank8; rank += 10)
                {
                    squares[rank + file] = Piece.None;
                }
            }
        }

        public int GetPiece(int square)
        {
            return squares[square];
        }

        public void SetPiece(int square, int piece)
        {
            squares[square] = piece;
        }

        public bool IsValidSquare(int square)
        {
            return square >= 0 && square < BoardSize && squares[square] != Piece.OffBoard;
        }

        public static int MakeSquare(int file, int rank)
        {
            return rank + file;
        }

        public static int GetFile(int square)
        {
            return square % 10;
        }

        public static int GetRank(int square)
        {
            return square / 10;
        }

        public static string SquareToAlgebraic(int square)
        {
            int file = GetFile(square);
            int rank = GetRank(square);
            return $"{(char)('a' + file - 1)}{rank}";
        }

        public static int AlgebraicToSquare(string algebraic)
        {
            if (algebraic.Length != 2) return -1;
            
            int file = algebraic[0] - 'a' + 1;
            int rank = (algebraic[1] - '1' + 1) * 10;
            
            if (file < FileA || file > FileH || rank < Rank1 || rank > Rank8)
                return -1;
                
            return rank + file;
        }

        public void SetupStartingPosition()
        {
            InitializeBoard();
            
            SetPiece(MakeSquare(FileA, Rank1), Piece.WhiteRook);
            SetPiece(MakeSquare(FileB, Rank1), Piece.WhiteKnight);
            SetPiece(MakeSquare(FileC, Rank1), Piece.WhiteBishop);
            SetPiece(MakeSquare(FileD, Rank1), Piece.WhiteQueen);
            SetPiece(MakeSquare(FileE, Rank1), Piece.WhiteKing);
            SetPiece(MakeSquare(FileF, Rank1), Piece.WhiteBishop);
            SetPiece(MakeSquare(FileG, Rank1), Piece.WhiteKnight);
            SetPiece(MakeSquare(FileH, Rank1), Piece.WhiteRook);

            for (int file = FileA; file <= FileH; file++)
            {
                SetPiece(MakeSquare(file, Rank2), Piece.WhitePawn);
                SetPiece(MakeSquare(file, Rank7), Piece.BlackPawn);
            }

            SetPiece(MakeSquare(FileA, Rank8), Piece.BlackRook);
            SetPiece(MakeSquare(FileB, Rank8), Piece.BlackKnight);
            SetPiece(MakeSquare(FileC, Rank8), Piece.BlackBishop);
            SetPiece(MakeSquare(FileD, Rank8), Piece.BlackQueen);
            SetPiece(MakeSquare(FileE, Rank8), Piece.BlackKing);
            SetPiece(MakeSquare(FileF, Rank8), Piece.BlackBishop);
            SetPiece(MakeSquare(FileG, Rank8), Piece.BlackKnight);
            SetPiece(MakeSquare(FileH, Rank8), Piece.BlackRook);

            SideToMove = Color.White;
            HalfMoveClock = 0;
            FullMoveNumber = 1;
            EnPassantSquare = -1;
            
            WhiteCanCastleKingside = true;
            WhiteCanCastleQueenside = true;
            BlackCanCastleKingside = true;
            BlackCanCastleQueenside = true;
            
            // Initialize hash key
            HashKey = ZobristHashing.ComputeHash(this);
        }

        public void PrintBoard()
        {
            Console.WriteLine("   a b c d e f g h");
            for (int rank = 8; rank >= 1; rank--)
            {
                Console.Write($"{rank}  ");
                for (int file = 1; file <= 8; file++)
                {
                    int square = MakeSquare(file, rank * 10);
                    int piece = GetPiece(square);
                    Console.Write($"{Piece.ToChar(piece)} ");
                }
                Console.WriteLine($" {rank}");
            }
            Console.WriteLine("   a b c d e f g h");
            Console.WriteLine($"Side to move: {SideToMove}");
            Console.WriteLine($"Castling: {(WhiteCanCastleKingside ? "K" : "")}{(WhiteCanCastleQueenside ? "Q" : "")}{(BlackCanCastleKingside ? "k" : "")}{(BlackCanCastleQueenside ? "q" : "")}");
            Console.WriteLine($"En passant: {(EnPassantSquare == -1 ? "-" : SquareToAlgebraic(EnPassantSquare))}");
            Console.WriteLine($"Halfmove clock: {HalfMoveClock}");
            Console.WriteLine($"Fullmove number: {FullMoveNumber}");
        }
    }
}