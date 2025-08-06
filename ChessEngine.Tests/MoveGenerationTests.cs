using ChessEngine;

namespace ChessEngine.Tests;

public class MoveGenerationTests
{
    [Fact]
    public void TestStartingPositionMoveCount()
    {
        var board = new Board();
        board.SetupStartingPosition();
        
        var moves = board.GenerateLegalMoves();
        
        // Should have 20 legal moves from starting position
        // 8 pawn moves (each pawn can move 1 or 2 squares) = 16
        // 2 knight moves each (Nf3, Nh3, Na3, Nc3) = 4
        Assert.Equal(20, moves.Count);
    }

    [Fact]
    public void TestPawnMoveNotation()
    {
        var board = new Board();
        board.SetupStartingPosition();
        
        var moves = board.GenerateLegalMoves();
        
        // Find pawn moves
        var pawnMoves = moves.Where(m => Piece.IsPawn(m.MovedPiece)).ToList();
        
        // Check some specific pawn moves
        Assert.Contains(pawnMoves, m => m.ToString() == "e2e3");
        Assert.Contains(pawnMoves, m => m.ToString() == "e2e4");
        Assert.Contains(pawnMoves, m => m.ToString() == "d2d3");
        Assert.Contains(pawnMoves, m => m.ToString() == "d2d4");
    }

    [Fact]
    public void TestKnightMoveNotation()
    {
        var board = new Board();
        board.SetupStartingPosition();
        
        var moves = board.GenerateLegalMoves();
        
        // Find knight moves
        var knightMoves = moves.Where(m => Piece.IsKnight(m.MovedPiece)).ToList();
        
        // Check knight moves from starting position
        Assert.Contains(knightMoves, m => m.ToString() == "b1a3");
        Assert.Contains(knightMoves, m => m.ToString() == "b1c3");
        Assert.Contains(knightMoves, m => m.ToString() == "g1f3");
        Assert.Contains(knightMoves, m => m.ToString() == "g1h3");
    }

    [Fact]
    public void TestMoveToStringFormat()
    {
        // Test move creation and string conversion
        var move = new Move(
            Board.AlgebraicToSquare("e2"), 
            Board.AlgebraicToSquare("e4"), 
            Piece.WhitePawn
        );
        
        Assert.Equal("e2e4", move.ToString());
    }

    [Fact]
    public void TestPromotionMoveNotation()
    {
        var board = new Board();
        // Set up position with pawn about to promote
        board.LoadFromFen("8/P7/8/8/8/8/8/8 w - - 0 1");
        
        var moves = board.GenerateLegalMoves();
        var promotionMoves = moves.Where(m => m.IsPromotion).ToList();
        
        // Should have 4 promotion moves (Q, R, B, N)
        Assert.Equal(4, promotionMoves.Count);
        
        // Check promotion notation includes piece
        Assert.Contains(promotionMoves, m => m.ToString() == "a7a8q");
        Assert.Contains(promotionMoves, m => m.ToString() == "a7a8r");
        Assert.Contains(promotionMoves, m => m.ToString() == "a7a8b");
        Assert.Contains(promotionMoves, m => m.ToString() == "a7a8n");
    }
}