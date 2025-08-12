using ChessEngine;

Console.WriteLine("Chess Engine Test - Fixed Version");

var board = new Board();
board.SetupStartingPosition();

Console.WriteLine("=== Basic Search Test ===");
var searchEngine = new SearchEngine(board);

// Test a simple 3-ply search
var result = searchEngine.Search(3, TimeSpan.FromSeconds(5));
Console.WriteLine($"Best move: {result.BestMove}");
Console.WriteLine($"Score: {result.Score}");
Console.WriteLine($"Nodes: {result.NodesSearched}");
Console.WriteLine($"Depth: {result.Depth}");

// Test from Black's perspective
board.MakeMove(result.BestMove);
Console.WriteLine($"\nAfter White plays {result.BestMove}:");
searchEngine = new SearchEngine(board);
result = searchEngine.Search(3, TimeSpan.FromSeconds(5));
Console.WriteLine($"Best Black move: {result.BestMove}");
Console.WriteLine($"Score: {result.Score}");

Console.WriteLine("\n=== Evaluation Test ===");
board.SetupStartingPosition();
Console.WriteLine($"Starting eval: {Evaluator.Evaluate(board)}");

// Test known good position for White  
board.LoadFromFen("rnbqkb1r/pppp1ppp/4pn2/8/3PP3/8/PPP2PPP/RNBQKBNR w KQkq - 0 4");
Console.WriteLine($"After 1.e4 e6 2.d4 Nf6: {Evaluator.Evaluate(board)}");

Console.WriteLine("\nTest completed successfully!");
