using ChessEngine;

var board = new Board();
board.LoadFromFen("rnbqkr1r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1");
board.PrintBoard();

var moves = board.GenerateLegalMoves();
Console.WriteLine($"Total legal moves: {moves.Count}");

var castlingMoves = moves.Where(m => m.IsCastling);
Console.WriteLine($"Castling moves found: {castlingMoves.Count()}");

foreach (var move in castlingMoves)
{
    Console.WriteLine($"Castling move: {move}");
}

// Check if f1 is attacked by black
bool f1Attacked = MoveGenerator.IsSquareAttacked(board, Board.MakeSquare(Board.FileF, Board.Rank1), Color.Black);
Console.WriteLine($"f1 attacked by black: {f1Attacked}");

// Check if e1 (king) is in check
bool kingInCheck = board.IsInCheck(Color.White);
Console.WriteLine($"White king in check: {kingInCheck}");