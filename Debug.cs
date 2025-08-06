using ChessEngine;

var board = new Board();

// Test castling through check
Console.WriteLine("=== Testing Castling Through Check ===");
board.LoadFromFen("rnbqkr2/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1");

Console.WriteLine($"White king on e1: {board.GetPiece(Board.MakeSquare(Board.FileE, Board.Rank1))}");
Console.WriteLine($"Black rook on f8: {board.GetPiece(Board.MakeSquare(Board.FileF, Board.Rank8))}");

Console.WriteLine($"f1 attacked by black? {MoveGenerator.IsSquareAttacked(board, Board.MakeSquare(Board.FileF, Board.Rank1), Color.Black)}");
Console.WriteLine($"g1 attacked by black? {MoveGenerator.IsSquareAttacked(board, Board.MakeSquare(Board.FileG, Board.Rank1), Color.Black)}");

var moves = board.GenerateLegalMoves();
var castlingMoves = moves.Where(m => m.IsCastling).ToList();

Console.WriteLine($"Castling moves found: {castlingMoves.Count}");
foreach (var move in castlingMoves)
{
    Console.WriteLine($"Castling move: {Board.SquareToAlgebraic(move.From)} to {Board.SquareToAlgebraic(move.To)}");
}

Console.WriteLine("\n=== Testing Castling Into Check ===");
board.LoadFromFen("rnbqk1r1/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1");

Console.WriteLine($"Black rook on g8: {board.GetPiece(Board.MakeSquare(Board.FileG, Board.Rank8))}");
Console.WriteLine($"g1 attacked by black? {MoveGenerator.IsSquareAttacked(board, Board.MakeSquare(Board.FileG, Board.Rank1), Color.Black)}");

moves = board.GenerateLegalMoves();
castlingMoves = moves.Where(m => m.IsCastling).ToList();

Console.WriteLine($"Castling moves found: {castlingMoves.Count}");
foreach (var move in castlingMoves)
{
    Console.WriteLine($"Castling move: {Board.SquareToAlgebraic(move.From)} to {Board.SquareToAlgebraic(move.To)}");
}