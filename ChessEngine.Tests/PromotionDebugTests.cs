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
        
        Console.WriteLine($"FEN loaded: {fenLoaded}");
        board.PrintBoard();
        
        var moves = board.GenerateLegalMoves();
        Console.WriteLine($"Total moves: {moves.Count}");
        
        foreach (var move in moves)
        {
            Console.WriteLine($"{move} - Promotion: {move.IsPromotion}");
        }
        
        var promotionMoves = moves.Where(m => m.IsPromotion).ToList();
        Console.WriteLine($"Promotion moves: {promotionMoves.Count}");
    }
}