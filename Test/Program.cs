using ChessEngine;

Console.WriteLine("Chess Engine Test");

var board = new Board();
board.SetupStartingPosition();

Console.WriteLine("Starting position:");
board.PrintBoard();

var legalMoves = board.GenerateLegalMoves();
Console.WriteLine($"Legal moves: {legalMoves.Count}");

foreach (var move in legalMoves.Take(5))
{
    Console.WriteLine($"Move: {move}");
}

var searchEngine = new SearchEngine(board);
Console.WriteLine("Searching for best move...");

var bestMove = searchEngine.GetBestMove(3, 1000);
Console.WriteLine($"Best move: {bestMove}");

Console.WriteLine("Test completed successfully!");
