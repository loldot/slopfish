using ChessEngine;

namespace ChessEngine.Tests;

public class PromotionDebugTests
{
    [Fact]
    public void DebugPromotionPosition()
    {
        var board = new Board();
        // Set up position with pawn about to promote
        bool fenLoaded = board.LoadFromFen("8/P7/8/8/8/8/8/8 w - - 0 1");
        
        Assert.True(fenLoaded);
        
        var moves = board.GenerateLegalMoves();
        var promotionMoves = moves.Where(m => m.IsPromotion).ToList();
        
        // Should have 4 promotion moves (queen, rook, bishop, knight)
        Assert.Equal(4, promotionMoves.Count);
        Assert.All(moves, move => Assert.True(move.IsPromotion));
    }
}