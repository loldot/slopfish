namespace ChessEngine
{
    public static partial class Evaluator
    {
        private static readonly int[] PieceValues = 
        {
            0,     // None
            100,   // Pawn
            320,   // Knight
            330,   // Bishop
            500,   // Rook
            900,   // Queen
            20000  // King
        };

        private static readonly int[,] PawnPositionTable = 
        {
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
            {  5,  5, 10, 25, 25, 10,  5,  5 },
            {  0,  0,  0, 20, 20,  0,  0,  0 },
            {  5, -5,-10,  0,  0,-10, -5,  5 },
            {  5, 10, 10,-20,-20, 10, 10,  5 },
            {  0,  0,  0,  0,  0,  0,  0,  0 }
        };

        private static readonly int[,] KnightPositionTable = 
        {
            {-50,-40,-30,-30,-30,-30,-40,-50 },
            {-40,-20,  0,  0,  0,  0,-20,-40 },
            {-30,  0, 10, 15, 15, 10,  0,-30 },
            {-30,  5, 15, 20, 20, 15,  5,-30 },
            {-30,  0, 15, 20, 20, 15,  0,-30 },
            {-30,  5, 10, 15, 15, 10,  5,-30 },
            {-40,-20,  0,  5,  5,  0,-20,-40 },
            {-50,-40,-30,-30,-30,-30,-40,-50 }
        };

        private static readonly int[,] BishopPositionTable = 
        {
            {-20,-10,-10,-10,-10,-10,-10,-20 },
            {-10,  0,  0,  0,  0,  0,  0,-10 },
            {-10,  0,  5, 10, 10,  5,  0,-10 },
            {-10,  5,  5, 10, 10,  5,  5,-10 },
            {-10,  0, 10, 10, 10, 10,  0,-10 },
            {-10, 10, 10, 10, 10, 10, 10,-10 },
            {-10,  5,  0,  0,  0,  0,  5,-10 },
            {-20,-10,-10,-10,-10,-10,-10,-20 }
        };

        private static readonly int[,] RookPositionTable = 
        {
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            {  5, 10, 10, 10, 10, 10, 10,  5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            {  0,  0,  0,  5,  5,  0,  0,  0 }
        };

        private static readonly int[,] QueenPositionTable = 
        {
            {-20,-10,-10, -5, -5,-10,-10,-20 },
            {-10,  0,  0,  0,  0,  0,  0,-10 },
            {-10,  0,  5,  5,  5,  5,  0,-10 },
            { -5,  0,  5,  5,  5,  5,  0, -5 },
            {  0,  0,  5,  5,  5,  5,  0, -5 },
            {-10,  5,  5,  5,  5,  5,  0,-10 },
            {-10,  0,  5,  0,  0,  0,  0,-10 },
            {-20,-10,-10, -5, -5,-10,-10,-20 }
        };

        private static readonly int[,] KingMiddlegamePositionTable = 
        {
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-20,-30,-30,-40,-40,-30,-30,-20 },
            {-10,-20,-20,-20,-20,-20,-20,-10 },
            { 20, 20,  0,  0,  0,  0, 20, 20 },
            { 20, 30, 10,  0,  0, 10, 30, 20 }
        };

        private static readonly int[,] KingEndgamePositionTable = 
        {
            {-50,-40,-30,-20,-20,-30,-40,-50 },
            {-30,-20,-10,  0,  0,-10,-20,-30 },
            {-30,-10, 20, 30, 30, 20,-10,-30 },
            {-30,-10, 30, 40, 40, 30,-10,-30 },
            {-30,-10, 30, 40, 40, 30,-10,-30 },
            {-30,-10, 20, 30, 30, 20,-10,-30 },
            {-30,-30,  0,  0,  0,  0,-30,-30 },
            {-50,-30,-30,-30,-30,-30,-30,-50 }
        };

        public static int Evaluate(Board board)
        {
            if (board.IsCheckmate())
            {
                return board.SideToMove == Color.White ? -30000 : 30000;
            }

            if (board.IsStalemate() || board.HalfMoveClock >= 100)
            {
                return 0;
            }

            int score = 0;
            int whiteMaterial = 0;
            int blackMaterial = 0;

            for (int square = 0; square < Board.BoardSize; square++)
            {
                if (!board.IsValidSquare(square)) continue;

                int piece = board.GetPiece(square);
                if (piece == Piece.None) continue;

                int pieceValue = GetPieceValue(piece);
                int positionalValue = GetPositionalValue(piece, square, board);

                if (Piece.IsWhite(piece))
                {
                    score += pieceValue + positionalValue;
                    whiteMaterial += GetMaterialValue(piece);
                }
                else
                {
                    score -= pieceValue + positionalValue;
                    blackMaterial += GetMaterialValue(piece);
                }
            }

            score += EvaluatePawnStructure(board);
            score += EvaluateKingSafety(board);
            score += EvaluateMobility(board);

            return board.SideToMove == Color.White ? score : -score;
        }

        public static int GetPieceValue(int piece)
        {
            PieceType pieceType = Piece.GetType(piece);
            return PieceValues[(int)pieceType];
        }

        private static int GetMaterialValue(int piece)
        {
            PieceType pieceType = Piece.GetType(piece);
            if (pieceType == PieceType.King) return 0;
            return PieceValues[(int)pieceType];
        }

        private static int GetPositionalValue(int piece, int square, Board board)
        {
            PieceType pieceType = Piece.GetType(piece);
            Color color = Piece.GetColor(piece);

            int file = Board.GetFile(square) - 1;
            int rank = Board.GetRank(square) / 10 - 1;

            if (color == Color.Black)
            {
                rank = 7 - rank;
            }

            if (file < 0 || file > 7 || rank < 0 || rank > 7)
                return 0;

            return pieceType switch
            {
                PieceType.Pawn => PawnPositionTable[rank, file],
                PieceType.Knight => KnightPositionTable[rank, file],
                PieceType.Bishop => BishopPositionTable[rank, file],
                PieceType.Rook => RookPositionTable[rank, file],
                PieceType.Queen => QueenPositionTable[rank, file],
                PieceType.King => IsEndgame(board) ? KingEndgamePositionTable[rank, file] : KingMiddlegamePositionTable[rank, file],
                _ => 0
            };
        }

        private static bool IsEndgame(Board board)
        {
            int totalMaterial = 0;
            
            for (int square = 0; square < Board.BoardSize; square++)
            {
                if (!board.IsValidSquare(square)) continue;
                
                int piece = board.GetPiece(square);
                if (piece != Piece.None && !Piece.IsKing(piece))
                {
                    totalMaterial += GetMaterialValue(piece);
                }
            }
            
            return totalMaterial < 1300;
        }

        private static int EvaluatePawnStructure(Board board)
        {
            int score = 0;
            
            for (int file = Board.FileA; file <= Board.FileH; file++)
            {
                int whitePawns = 0;
                int blackPawns = 0;
                
                for (int rank = Board.Rank1; rank <= Board.Rank8; rank += 10)
                {
                    int square = Board.MakeSquare(file, rank);
                    int piece = board.GetPiece(square);
                    
                    if (piece == Piece.WhitePawn) whitePawns++;
                    else if (piece == Piece.BlackPawn) blackPawns++;
                }
                
                if (whitePawns > 1) score -= 10 * (whitePawns - 1);
                if (blackPawns > 1) score += 10 * (blackPawns - 1);
                
                if (whitePawns == 0 && blackPawns == 0) continue;
                
                bool whiteIsolated = true;
                bool blackIsolated = true;
                
                for (int adjacentFile = file - 1; adjacentFile <= file + 1; adjacentFile += 2)
                {
                    if (adjacentFile < Board.FileA || adjacentFile > Board.FileH) continue;
                    
                    for (int rank = Board.Rank1; rank <= Board.Rank8; rank += 10)
                    {
                        int adjacentSquare = Board.MakeSquare(adjacentFile, rank);
                        int adjacentPiece = board.GetPiece(adjacentSquare);
                        
                        if (adjacentPiece == Piece.WhitePawn) whiteIsolated = false;
                        if (adjacentPiece == Piece.BlackPawn) blackIsolated = false;
                    }
                }
                
                if (whiteIsolated && whitePawns > 0) score -= 20;
                if (blackIsolated && blackPawns > 0) score += 20;
            }
            
            return score;
        }

        private static int EvaluateKingSafety(Board board)
        {
            int score = 0;
            
            int whiteKing = board.FindKing(Color.White);
            int blackKing = board.FindKing(Color.Black);
            
            if (whiteKing != -1)
            {
                score += EvaluateKingSafetyForColor(board, whiteKing, Color.White);
            }
            
            if (blackKing != -1)
            {
                score -= EvaluateKingSafetyForColor(board, blackKing, Color.Black);
            }
            
            return score;
        }

        private static int EvaluateKingSafetyForColor(Board board, int kingSquare, Color color)
        {
            int safety = 0;
            
            if (!IsEndgame(board))
            {
                int[] kingArea = { -11, -10, -9, -1, 1, 9, 10, 11 };
                
                foreach (int offset in kingArea)
                {
                    int square = kingSquare + offset;
                    if (!board.IsValidSquare(square)) continue;
                    
                    int piece = board.GetPiece(square);
                    if (piece != Piece.None && Piece.GetColor(piece) == color)
                    {
                        safety += 10;
                    }
                }
                
                Color enemyColor = color == Color.White ? Color.Black : Color.White;
                if (MoveGenerator.IsSquareAttacked(board, kingSquare, enemyColor))
                {
                    safety -= 50;
                }
            }
            
            return safety;
        }

        private static int EvaluateMobility(Board board)
        {
            Color originalSide = board.SideToMove;
            
            board.SideToMove = Color.White;
            int whiteMobility = MoveGenerator.GenerateAllMoves(board).Count;
            
            board.SideToMove = Color.Black;
            int blackMobility = MoveGenerator.GenerateAllMoves(board).Count;
            
            board.SideToMove = originalSide;
            
            return (whiteMobility - blackMobility) * 2;
        }
    }
}