using ChessEngine;

namespace ChessEngine.Tests;

public class FenDebugTests
{
    [Fact]
    public void DebugInvalidFenHandling()
    {
        var board = new Board();
        
        // Test each invalid FEN individually
        bool result1 = board.LoadFromFen("");
        Console.WriteLine($"Empty string: {result1} (should be false)");
        
        bool result2 = board.LoadFromFen("invalid");
        Console.WriteLine($"'invalid': {result2} (should be false)");
        
        bool result3 = board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        Console.WriteLine($"Missing parts: {result3} (should be false)");
        
        bool result4 = board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP w KQkq - 0 1");
        Console.WriteLine($"Missing rank: {result4} (should be false)");
        
        bool result5 = board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR x KQkq - 0 1");
        Console.WriteLine($"Invalid side: {result5} (should be false)");
        
        // This test should pass - all should be false
        Assert.False(result1 && result2 && result3 && result4 && result5);
    }
}