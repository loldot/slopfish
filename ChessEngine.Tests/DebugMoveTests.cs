using ChessEngine;

namespace ChessEngine.Tests;

public class DebugMoveTests
{
    [Fact]
    public void DebugStartingPositionMoves()
    {
        var board = new Board();
        board.SetupStartingPosition();
        
        var moves = board.GenerateLegalMoves();
        
        // Verify we have the expected number of legal moves from starting position
        Assert.Equal(20, moves.Count);
    }
}