using ChessEngine;

namespace ChessEngine.Tests;

public class DebugEvaluationConsistency
{
    [Fact]
    public void TestEvaluationConsistency()
    {
        var board = new Board();
        var searchEngine = new SearchEngine(board);
        
        // Test position from the game where there were evaluation swings
        board.LoadFromFen("rnbqkb1r/pppp1ppp/4pn2/8/3PP3/8/PPP2PPP/RNBQKBNR w KQkq - 0 4");
        
        // Get initial evaluation
        int eval1 = Evaluator.Evaluate(board);
        
        // Make a move and unmake it - evaluation should be identical
        var moves = board.GenerateLegalMoves();
        if (moves.Count > 0)
        {
            var move = moves[0];
            board.MakeMove(move);
            board.UnmakeMove(move);
            
            int eval2 = Evaluator.Evaluate(board);
            
            Assert.Equal(eval1, eval2);
        }
    }
    
    [Fact]
    public void TestSearchConsistency()
    {
        var board = new Board();
        board.LoadFromFen("rnbqkb1r/pppp1ppp/4pn2/8/3PP3/8/PPP2PPP/RNBQKBNR w KQkq - 0 4");
        
        var searchEngine = new SearchEngine(board);
        
        // Search twice with same parameters - should get same result
        var result1 = searchEngine.Search(4, TimeSpan.FromSeconds(5));
        var result2 = searchEngine.Search(4, TimeSpan.FromSeconds(5));
        
        // Results should be identical for deterministic search
        Assert.Equal(result1.BestMove.ToString(), result2.BestMove.ToString());
        Assert.Equal(result1.Score, result2.Score);
    }
}
