namespace ChessEngine
{
    public static class ZobristHashing
    {
        private static readonly Random rng = new Random(12345); // Fixed seed for reproducibility
        
        // Zobrist keys for pieces on squares
        private static readonly ulong[,] pieceKeys = new ulong[15, 120]; // 15 piece types (0-14), 120 squares
        
        // Zobrist keys for castling rights
        private static readonly ulong whiteKingsideCastleKey;
        private static readonly ulong whiteQueensideCastleKey;
        private static readonly ulong blackKingsideCastleKey;
        private static readonly ulong blackQueensideCastleKey;
        
        // Zobrist key for en passant file
        private static readonly ulong[] enPassantKeys = new ulong[8]; // Files A-H
        
        // Zobrist key for side to move
        private static readonly ulong sideToMoveKey;
        
        static ZobristHashing()
        {
            // Initialize piece keys
            for (int piece = 0; piece < 15; piece++)
            {
                for (int square = 0; square < 120; square++)
                {
                    pieceKeys[piece, square] = NextUlong();
                }
            }
            
            // Initialize castling keys
            whiteKingsideCastleKey = NextUlong();
            whiteQueensideCastleKey = NextUlong();
            blackKingsideCastleKey = NextUlong();
            blackQueensideCastleKey = NextUlong();
            
            // Initialize en passant keys
            for (int file = 0; file < 8; file++)
            {
                enPassantKeys[file] = NextUlong();
            }
            
            // Initialize side to move key
            sideToMoveKey = NextUlong();
        }
        
        private static ulong NextUlong()
        {
            byte[] bytes = new byte[8];
            rng.NextBytes(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }
        
        public static ulong ComputeHash(Board board)
        {
            ulong hash = 0;
            
            // Hash pieces on board
            for (int square = 0; square < Board.BoardSize; square++)
            {
                if (!board.IsValidSquare(square)) continue;
                
                int piece = board.GetPiece(square);
                if (piece != Piece.None && piece != Piece.OffBoard)
                {
                    hash ^= pieceKeys[piece, square];
                }
            }
            
            // Hash castling rights
            if (board.WhiteCanCastleKingside)
                hash ^= whiteKingsideCastleKey;
            if (board.WhiteCanCastleQueenside)
                hash ^= whiteQueensideCastleKey;
            if (board.BlackCanCastleKingside)
                hash ^= blackKingsideCastleKey;
            if (board.BlackCanCastleQueenside)
                hash ^= blackQueensideCastleKey;
            
            // Hash en passant
            if (board.EnPassantSquare != -1)
            {
                int file = Board.GetFile(board.EnPassantSquare) - 1; // Convert to 0-7
                if (file >= 0 && file < 8)
                {
                    hash ^= enPassantKeys[file];
                }
            }
            
            // Hash side to move (black)
            if (board.SideToMove == Color.Black)
            {
                hash ^= sideToMoveKey;
            }
            
            return hash;
        }
        
        public static ulong UpdateHashAfterMove(ulong currentHash, Board board, Move move)
        {
            ulong hash = currentHash;
            
            // Remove piece from source square
            hash ^= pieceKeys[move.MovedPiece, move.From];
            
            // Add piece to destination square (or promotion piece)
            int destinationPiece = move.IsPromotion ? move.PromotionPiece : move.MovedPiece;
            hash ^= pieceKeys[destinationPiece, move.To];
            
            // Handle capture
            if (move.IsCapture)
            {
                int captureSquare = move.IsEnPassant ? 
                    (board.SideToMove == Color.White ? move.To - 10 : move.To + 10) : 
                    move.To;
                hash ^= pieceKeys[move.CapturedPiece, captureSquare];
            }
            
            // Handle castling
            if (move.IsCastling)
            {
                int rookFrom, rookTo;
                int rookPiece = board.SideToMove == Color.White ? Piece.WhiteRook : Piece.BlackRook;
                
                if (move.To > move.From) // Kingside
                {
                    rookFrom = board.SideToMove == Color.White ? 
                        Board.MakeSquare(Board.FileH, Board.Rank1) : 
                        Board.MakeSquare(Board.FileH, Board.Rank8);
                    rookTo = board.SideToMove == Color.White ? 
                        Board.MakeSquare(Board.FileF, Board.Rank1) : 
                        Board.MakeSquare(Board.FileF, Board.Rank8);
                }
                else // Queenside
                {
                    rookFrom = board.SideToMove == Color.White ? 
                        Board.MakeSquare(Board.FileA, Board.Rank1) : 
                        Board.MakeSquare(Board.FileA, Board.Rank8);
                    rookTo = board.SideToMove == Color.White ? 
                        Board.MakeSquare(Board.FileD, Board.Rank1) : 
                        Board.MakeSquare(Board.FileD, Board.Rank8);
                }
                
                hash ^= pieceKeys[rookPiece, rookFrom];
                hash ^= pieceKeys[rookPiece, rookTo];
            }
            
            // Switch side to move
            hash ^= sideToMoveKey;
            
            return hash;
        }
        
        public static ulong GetPieceKey(int piece, int square)
        {
            return pieceKeys[piece, square];
        }
        
        public static ulong GetCastlingKey(bool whiteKingside, bool whiteQueenside, 
                                         bool blackKingside, bool blackQueenside)
        {
            ulong key = 0;
            if (whiteKingside) key ^= whiteKingsideCastleKey;
            if (whiteQueenside) key ^= whiteQueensideCastleKey;
            if (blackKingside) key ^= blackKingsideCastleKey;
            if (blackQueenside) key ^= blackQueensideCastleKey;
            return key;
        }
        
        public static ulong GetEnPassantKey(int file)
        {
            return file >= 0 && file < 8 ? enPassantKeys[file] : 0;
        }
        
        public static ulong GetSideToMoveKey()
        {
            return sideToMoveKey;
        }
    }
}