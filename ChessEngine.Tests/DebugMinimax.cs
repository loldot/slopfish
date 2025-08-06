using ChessEngine;

namespace ChessEngine.Tests;

public class DebugMinimax
{
    [Fact]
    public void TestBasicMinimax()
    {
        var board = new Board();
        board.SetupStartingPosition();
        
        // Test depth 1 search
        var searchEngine = new SearchEngine(board);
        var result = searchEngine.Search(1, TimeSpan.FromSeconds(1));
        
        Console.WriteLine($"Depth 1 - Best move: {result.BestMove}, Score: {result.Score}");
        
        // Manual verification - what should the score be?
        var moves = board.GenerateLegalMoves();
        int bestStaticScore = int.MinValue;
        Move bestStaticMove = default;
        
        foreach (var move in moves)
        {
            board.MakeMove(move);
            int staticEval = Evaluator.Evaluate(board);
            // Since it's now Black's turn, we need to negate the score to get White's perspective
            int scoreFromWhitePerspective = -staticEval;
            
            if (scoreFromWhitePerspective > bestStaticScore)
            {
                bestStaticScore = scoreFromWhitePerspective;
                bestStaticMove = move;
            }
            
            Console.WriteLine($"Move {move}: Static eval after move: {staticEval}, From White's perspective: {scoreFromWhitePerspective}");
            board.UnmakeMove(move);
        }
        
        Console.WriteLine($"Best static move: {bestStaticMove}, Best static score: {bestStaticScore}");
        Console.WriteLine($"Search result: {result.BestMove}, Search score: {result.Score}");
        
        // The search score should roughly match the best static score
        Assert.True(Math.Abs(result.Score - bestStaticScore) < 50, 
            $"Search score {result.Score} doesn't match expected {bestStaticScore}");
    }
}
