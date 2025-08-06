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
        bool result2 = board.LoadFromFen("invalid");
        bool result3 = board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        bool result4 = board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP w KQkq - 0 1");
        bool result5 = board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR x KQkq - 0 1");
        
        // All invalid FEN strings should return false
        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
        Assert.False(result4);
        Assert.False(result5);
    }
}