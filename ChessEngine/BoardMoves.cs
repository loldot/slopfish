namespace ChessEngine
{
    public struct GameState
    {
        public bool WhiteCanCastleKingside;
        public bool WhiteCanCastleQueenside;
        public bool BlackCanCastleKingside;
        public bool BlackCanCastleQueenside;
        public int EnPassantSquare;
        public int HalfMoveClock;
        public int FullMoveNumber;
        public Color SideToMove;
        public ulong HashKey;

        public GameState(Board board)
        {
            WhiteCanCastleKingside = board.WhiteCanCastleKingside;
            WhiteCanCastleQueenside = board.WhiteCanCastleQueenside;
            BlackCanCastleKingside = board.BlackCanCastleKingside;
            BlackCanCastleQueenside = board.BlackCanCastleQueenside;
            EnPassantSquare = board.EnPassantSquare;
            HalfMoveClock = board.HalfMoveClock;
            FullMoveNumber = board.FullMoveNumber;
            SideToMove = board.SideToMove;
            HashKey = board.HashKey;
        }
    }

    public partial class Board
    {
        private Stack<GameState> gameStateHistory = new Stack<GameState>();

        public void MakeMove(Move move)
        {
            gameStateHistory.Push(new GameState(this));

            SetPiece(move.From, Piece.None);
            SetPiece(move.To, move.IsPromotion ? move.PromotionPiece : move.MovedPiece);

            if (move.IsEnPassant)
            {
                int capturedPawnSquare = move.To + (SideToMove == Color.White ? -10 : 10);
                SetPiece(capturedPawnSquare, Piece.None);
            }

            if (move.IsCastling)
            {
                HandleCastling(move);
            }

            EnPassantSquare = -1;
            if (move.IsDoublePawnPush)
            {
                EnPassantSquare = move.From + (SideToMove == Color.White ? 10 : -10);
            }

            UpdateCastlingRights(move);

            if (Piece.IsPawn(move.MovedPiece) || move.IsCapture)
            {
                HalfMoveClock = 0;
            }
            else
            {
                HalfMoveClock++;
            }

            if (SideToMove == Color.Black)
            {
                FullMoveNumber++;
            }

            SideToMove = SideToMove == Color.White ? Color.Black : Color.White;
            
            // Recompute hash after move
            HashKey = ZobristHashing.ComputeHash(this);
            
            // Add new position to history
            AddPositionToHistory();
        }

        public void UnmakeMove(Move move)
        {
            if (gameStateHistory.Count == 0) return;

            GameState previousState = gameStateHistory.Pop();

            SetPiece(move.From, move.MovedPiece);
            SetPiece(move.To, move.CapturedPiece);

            if (move.IsEnPassant)
            {
                int capturedPawnSquare = move.To + (previousState.SideToMove == Color.White ? -10 : 10);
                int capturedPawn = previousState.SideToMove == Color.White ? Piece.BlackPawn : Piece.WhitePawn;
                SetPiece(capturedPawnSquare, capturedPawn);
            }

            if (move.IsCastling)
            {
                UndoCastling(move);
            }

            WhiteCanCastleKingside = previousState.WhiteCanCastleKingside;
            WhiteCanCastleQueenside = previousState.WhiteCanCastleQueenside;
            BlackCanCastleKingside = previousState.BlackCanCastleKingside;
            BlackCanCastleQueenside = previousState.BlackCanCastleQueenside;
            EnPassantSquare = previousState.EnPassantSquare;
            HalfMoveClock = previousState.HalfMoveClock;
            FullMoveNumber = previousState.FullMoveNumber;
            SideToMove = previousState.SideToMove;
            HashKey = previousState.HashKey;
            
            // Remove last position from history
            RemoveLastPositionFromHistory();
        }

        private void HandleCastling(Move move)
        {
            int rookFrom, rookTo;

            if (move.To == Board.MakeSquare(Board.FileG, Board.Rank1))
            {
                rookFrom = Board.MakeSquare(Board.FileH, Board.Rank1);
                rookTo = Board.MakeSquare(Board.FileF, Board.Rank1);
            }
            else if (move.To == Board.MakeSquare(Board.FileC, Board.Rank1))
            {
                rookFrom = Board.MakeSquare(Board.FileA, Board.Rank1);
                rookTo = Board.MakeSquare(Board.FileD, Board.Rank1);
            }
            else if (move.To == Board.MakeSquare(Board.FileG, Board.Rank8))
            {
                rookFrom = Board.MakeSquare(Board.FileH, Board.Rank8);
                rookTo = Board.MakeSquare(Board.FileF, Board.Rank8);
            }
            else if (move.To == Board.MakeSquare(Board.FileC, Board.Rank8))
            {
                rookFrom = Board.MakeSquare(Board.FileA, Board.Rank8);
                rookTo = Board.MakeSquare(Board.FileD, Board.Rank8);
            }
            else
            {
                return;
            }

            int rook = GetPiece(rookFrom);
            SetPiece(rookFrom, Piece.None);
            SetPiece(rookTo, rook);
        }

        private void UndoCastling(Move move)
        {
            int rookFrom, rookTo;

            if (move.To == Board.MakeSquare(Board.FileG, Board.Rank1))
            {
                rookFrom = Board.MakeSquare(Board.FileH, Board.Rank1);
                rookTo = Board.MakeSquare(Board.FileF, Board.Rank1);
            }
            else if (move.To == Board.MakeSquare(Board.FileC, Board.Rank1))
            {
                rookFrom = Board.MakeSquare(Board.FileA, Board.Rank1);
                rookTo = Board.MakeSquare(Board.FileD, Board.Rank1);
            }
            else if (move.To == Board.MakeSquare(Board.FileG, Board.Rank8))
            {
                rookFrom = Board.MakeSquare(Board.FileH, Board.Rank8);
                rookTo = Board.MakeSquare(Board.FileF, Board.Rank8);
            }
            else if (move.To == Board.MakeSquare(Board.FileC, Board.Rank8))
            {
                rookFrom = Board.MakeSquare(Board.FileA, Board.Rank8);
                rookTo = Board.MakeSquare(Board.FileD, Board.Rank8);
            }
            else
            {
                return;
            }

            int rook = GetPiece(rookTo);
            SetPiece(rookTo, Piece.None);
            SetPiece(rookFrom, rook);
        }

        private void UpdateCastlingRights(Move move)
        {
            if (Piece.IsKing(move.MovedPiece))
            {
                if (move.MovedPiece == Piece.WhiteKing)
                {
                    WhiteCanCastleKingside = false;
                    WhiteCanCastleQueenside = false;
                }
                else if (move.MovedPiece == Piece.BlackKing)
                {
                    BlackCanCastleKingside = false;
                    BlackCanCastleQueenside = false;
                }
            }

            if (Piece.IsRook(move.MovedPiece))
            {
                if (move.From == Board.MakeSquare(Board.FileA, Board.Rank1))
                    WhiteCanCastleQueenside = false;
                else if (move.From == Board.MakeSquare(Board.FileH, Board.Rank1))
                    WhiteCanCastleKingside = false;
                else if (move.From == Board.MakeSquare(Board.FileA, Board.Rank8))
                    BlackCanCastleQueenside = false;
                else if (move.From == Board.MakeSquare(Board.FileH, Board.Rank8))
                    BlackCanCastleKingside = false;
            }

            if (Piece.IsRook(move.CapturedPiece))
            {
                if (move.To == Board.MakeSquare(Board.FileA, Board.Rank1))
                    WhiteCanCastleQueenside = false;
                else if (move.To == Board.MakeSquare(Board.FileH, Board.Rank1))
                    WhiteCanCastleKingside = false;
                else if (move.To == Board.MakeSquare(Board.FileA, Board.Rank8))
                    BlackCanCastleQueenside = false;
                else if (move.To == Board.MakeSquare(Board.FileH, Board.Rank8))
                    BlackCanCastleKingside = false;
            }
        }

        public bool IsInCheck(Color color)
        {
            int kingSquare = FindKing(color);
            if (kingSquare == -1) return false;
            
            Color oppositeColor = color == Color.White ? Color.Black : Color.White;
            return MoveGenerator.IsSquareAttacked(this, kingSquare, oppositeColor);
        }

        public int FindKing(Color color)
        {
            int targetKing = color == Color.White ? Piece.WhiteKing : Piece.BlackKing;
            
            for (int square = 0; square < BoardSize; square++)
            {
                if (IsValidSquare(square) && GetPiece(square) == targetKing)
                    return square;
            }
            
            return -1;
        }

        public bool IsLegalMove(Move move)
        {
            MakeMove(move);
            Color previousSide = SideToMove == Color.White ? Color.Black : Color.White;
            bool isLegal = !IsInCheck(previousSide);
            UnmakeMove(move);
            
            return isLegal;
        }

        public List<Move> GenerateLegalMoves()
        {
            List<Move> pseudoLegalMoves = MoveGenerator.GenerateAllMoves(this);
            List<Move> legalMoves = new List<Move>();

            foreach (Move move in pseudoLegalMoves)
            {
                if (IsLegalMove(move))
                {
                    legalMoves.Add(move);
                }
            }

            return legalMoves;
        }

        public bool IsCheckmate()
        {
            if (!IsInCheck(SideToMove)) return false;
            return GenerateLegalMoves().Count == 0;
        }

        public bool IsStalemate()
        {
            if (IsInCheck(SideToMove)) return false;
            return GenerateLegalMoves().Count == 0;
        }

        public bool IsGameOver()
        {
            // Don't include repetition in IsGameOver() - only use in search for performance
            return IsCheckmate() || IsStalemate() || HalfMoveClock >= 100;
        }
    }
}