using ChessEngine;

namespace ChessEngine.Tests;

public class FenRoundTripTests
{
    [Theory]
    [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
    [InlineData("r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4")]
    [InlineData("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1")]
    [InlineData("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1")]
    [InlineData("r3k2r/8/8/8/8/8/8/R3K2R b Kq - 5 10")]
    [InlineData("8/P7/8/8/8/8/8/4K2k w - - 0 1")]
    [InlineData("4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1")]
    [InlineData("4k3/8/8/8/8/8/8/4K3 w - - 50 100")]
    public void TestFenRoundTrip(string originalFen)
    {
        var board = new Board();
        
        // Parse the FEN
        bool parseResult = board.LoadFromFen(originalFen);
        Assert.True(parseResult, $"Failed to parse FEN: {originalFen}");
        
        // Convert back to FEN
        string regeneratedFen = board.ToFen();
        
        // Should be identical
        Assert.Equal(originalFen, regeneratedFen);
        
        // Parse the regenerated FEN again to ensure it's still valid
        var board2 = new Board();
        bool parseResult2 = board2.LoadFromFen(regeneratedFen);
        Assert.True(parseResult2, $"Failed to parse regenerated FEN: {regeneratedFen}");
        
        // Both boards should have identical state
        Assert.Equal(board.SideToMove, board2.SideToMove);
        Assert.Equal(board.WhiteCanCastleKingside, board2.WhiteCanCastleKingside);
        Assert.Equal(board.WhiteCanCastleQueenside, board2.WhiteCanCastleQueenside);
        Assert.Equal(board.BlackCanCastleKingside, board2.BlackCanCastleKingside);
        Assert.Equal(board.BlackCanCastleQueenside, board2.BlackCanCastleQueenside);
        Assert.Equal(board.EnPassantSquare, board2.EnPassantSquare);
        Assert.Equal(board.HalfMoveClock, board2.HalfMoveClock);
        Assert.Equal(board.FullMoveNumber, board2.FullMoveNumber);
        
        // Check all pieces match
        for (int square = 0; square < Board.BoardSize; square++)
        {
            if (!board.IsValidSquare(square)) continue;
            Assert.Equal(board.GetPiece(square), board2.GetPiece(square));
        }
    }
}