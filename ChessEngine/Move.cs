namespace ChessEngine
{
    public struct Move
    {
        public int From { get; set; }
        public int To { get; set; }
        public int MovedPiece { get; set; }
        public int CapturedPiece { get; set; }
        public int PromotionPiece { get; set; }
        public bool IsEnPassant { get; set; }
        public bool IsCastling { get; set; }
        public bool IsDoublePawnPush { get; set; }

        public Move(int from, int to, int movedPiece, int capturedPiece = Piece.None, 
                   int promotionPiece = Piece.None, bool isEnPassant = false, 
                   bool isCastling = false, bool isDoublePawnPush = false)
        {
            From = from;
            To = to;
            MovedPiece = movedPiece;
            CapturedPiece = capturedPiece;
            PromotionPiece = promotionPiece;
            IsEnPassant = isEnPassant;
            IsCastling = isCastling;
            IsDoublePawnPush = isDoublePawnPush;
        }

        public bool IsCapture => CapturedPiece != Piece.None || IsEnPassant;
        public bool IsPromotion => PromotionPiece != Piece.None;

        public override string ToString()
        {
            string moveStr = Board.SquareToAlgebraic(From) + Board.SquareToAlgebraic(To);
            
            if (IsPromotion)
            {
                moveStr += char.ToLower(Piece.ToChar(PromotionPiece));
            }
            
            return moveStr;
        }

        public static Move ParseUCI(string uciMove)
        {
            if (uciMove.Length < 4) return default;
            
            int from = Board.AlgebraicToSquare(uciMove.Substring(0, 2));
            int to = Board.AlgebraicToSquare(uciMove.Substring(2, 2));
            
            int promotionPiece = Piece.None;
            if (uciMove.Length == 5)
            {
                char promChar = uciMove[4];
                promotionPiece = promChar switch
                {
                    'q' => Piece.BlackQueen,
                    'r' => Piece.BlackRook,
                    'b' => Piece.BlackBishop,
                    'n' => Piece.BlackKnight,
                    'Q' => Piece.WhiteQueen,
                    'R' => Piece.WhiteRook,
                    'B' => Piece.WhiteBishop,
                    'N' => Piece.WhiteKnight,
                    _ => Piece.None
                };
            }
            
            return new Move(from, to, Piece.None, Piece.None, promotionPiece);
        }
    }
}