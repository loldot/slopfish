using ChessEngine;

namespace ChessEngine.Tests;

public class BoardCoordinateTests
{
    [Fact]
    public void TestSquareConstants()
    {
        // Test that our square constants are correct
        Assert.Equal(11, Board.MakeSquare(Board.FileA, Board.Rank1)); // a1
        Assert.Equal(18, Board.MakeSquare(Board.FileH, Board.Rank1)); // h1
        Assert.Equal(81, Board.MakeSquare(Board.FileA, Board.Rank8)); // a8
        Assert.Equal(88, Board.MakeSquare(Board.FileH, Board.Rank8)); // h8
    }

    [Fact]
    public void TestAlgebraicConversion()
    {
        // Test square to algebraic conversion
        Assert.Equal("a1", Board.SquareToAlgebraic(Board.MakeSquare(Board.FileA, Board.Rank1)));
        Assert.Equal("e4", Board.SquareToAlgebraic(Board.MakeSquare(Board.FileE, Board.Rank4)));
        Assert.Equal("h8", Board.SquareToAlgebraic(Board.MakeSquare(Board.FileH, Board.Rank8)));
    }

    [Fact]
    public void TestAlgebraicToSquare()
    {
        // Test algebraic to square conversion
        Assert.Equal(Board.MakeSquare(Board.FileA, Board.Rank1), Board.AlgebraicToSquare("a1"));
        Assert.Equal(Board.MakeSquare(Board.FileE, Board.Rank4), Board.AlgebraicToSquare("e4"));
        Assert.Equal(Board.MakeSquare(Board.FileH, Board.Rank8), Board.AlgebraicToSquare("h8"));
    }

    [Fact]
    public void TestFileAndRankExtraction()
    {
        int e4 = Board.MakeSquare(Board.FileE, Board.Rank4);
        Assert.Equal(Board.FileE, Board.GetFile(e4));
        Assert.Equal(4, Board.GetRank(e4)); // GetRank returns 1-8, not the constant
    }

    [Fact]
    public void TestStartingPositionSquares()
    {
        var board = new Board();
        board.SetupStartingPosition();
        
        // Test white pieces
        Assert.Equal(Piece.WhiteRook, board.GetPiece(Board.MakeSquare(Board.FileA, Board.Rank1)));
        Assert.Equal(Piece.WhiteKnight, board.GetPiece(Board.MakeSquare(Board.FileB, Board.Rank1)));
        Assert.Equal(Piece.WhiteKing, board.GetPiece(Board.MakeSquare(Board.FileE, Board.Rank1)));
        
        // Test white pawns
        Assert.Equal(Piece.WhitePawn, board.GetPiece(Board.MakeSquare(Board.FileA, Board.Rank2)));
        Assert.Equal(Piece.WhitePawn, board.GetPiece(Board.MakeSquare(Board.FileH, Board.Rank2)));
        
        // Test black pieces
        Assert.Equal(Piece.BlackRook, board.GetPiece(Board.MakeSquare(Board.FileA, Board.Rank8)));
        Assert.Equal(Piece.BlackKing, board.GetPiece(Board.MakeSquare(Board.FileE, Board.Rank8)));
        
        // Test empty squares
        Assert.Equal(Piece.None, board.GetPiece(Board.MakeSquare(Board.FileE, Board.Rank4)));
    }
}