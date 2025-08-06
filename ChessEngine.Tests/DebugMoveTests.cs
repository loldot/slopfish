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
        
        foreach (var move in moves.OrderBy(m => m.ToString()))
        {
            var piece = Piece.ToChar(move.MovedPiece);
            Console.WriteLine($"{move} ({piece})");
        }
        
        Assert.Equal(20, moves.Count);
    }
}