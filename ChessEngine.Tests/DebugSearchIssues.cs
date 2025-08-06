using ChessEngine;

namespace ChessEngine.Tests;

public class DebugSearchIssues
{
    [Fact]
    public void TestSearchEvaluationAlignment()
    {
        var board = new Board();
        // Set up a position from the problematic game
        // After 8...Qh3 9.gxh3, Black to move
        board.LoadFromFen("rnbqkbnr/ppp1pppp/8/8/3P2q1/7P/PPP1PP1P/RNBQKBNR b KQkq - 0 9");
        
        var searchEngine = new SearchEngine(board);
        
        // Get the moves and their evaluations
        var moves = board.GenerateLegalMoves();
        foreach (var move in moves.Take(5))
        {
            board.MakeMove(move);
            int staticEval = Evaluator.Evaluate(board);
            board.UnmakeMove(move);
            
            // The static evaluation should give us a hint about move quality
            Console.WriteLine($"Move: {move}, Static eval after move: {staticEval}");
        }
        
        // Now do a search
        var result = searchEngine.Search(3, TimeSpan.FromSeconds(1));
        
        Console.WriteLine($"Best move from search: {result.BestMove}");
        Console.WriteLine($"Search score: {result.Score}");
        
        // The search should pick a reasonable move, not a clearly bad one
        Assert.NotEqual(default(Move), result.BestMove);
    }
    
    [Fact]
    public void TestEvaluationSign()
    {
        var board = new Board();
        board.SetupStartingPosition();
        
        // White to move from starting position - should be roughly equal
        int whiteEval = Evaluator.Evaluate(board);
        Console.WriteLine($"Starting position, White to move: {whiteEval}");
        
        // Make a move for White
        var moves = board.GenerateLegalMoves();
        var move = moves.First(m => m.ToString() == "e2e4");
        board.MakeMove(move);
        
        // Now Black to move - should still be roughly equal but from Black's perspective
        int blackEval = Evaluator.Evaluate(board);
        Console.WriteLine($"After e4, Black to move: {blackEval}");
        
        // Since evaluation is from side-to-move perspective, both should be close to 0
        Assert.True(Math.Abs(whiteEval) < 100, $"White eval too extreme: {whiteEval}");
        Assert.True(Math.Abs(blackEval) < 100, $"Black eval too extreme: {blackEval}");
    }
}
