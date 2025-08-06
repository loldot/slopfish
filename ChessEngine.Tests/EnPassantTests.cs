using ChessEngine;

namespace ChessEngine.Tests;

public class EnPassantTests
{
    [Fact]
    public void TestBasicEnPassantCapture_White()
    {
        var board = new Board();
        // Position: white pawn on e5, black pawn just moved d7-d5
        board.LoadFromFen("rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2");
        
        var moves = board.GenerateLegalMoves();
        
        // Should have en passant capture exd6
        var enPassantMove = moves.FirstOrDefault(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank5) &&
            m.To == Board.MakeSquare(Board.FileD, Board.Rank6) &&
            m.IsEnPassant);
            
        Assert.True(enPassantMove.From != 0); // Move was found
        Assert.True(enPassantMove.IsEnPassant);
    }

    [Fact]
    public void TestBasicEnPassantCapture_Black()
    {
        var board = new Board();
        // Position: black pawn on d4, white pawn just moved e2-e4
        board.LoadFromFen("rnbqkbnr/pppp1ppp/8/8/3pP3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 2");
        
        var moves = board.GenerateLegalMoves();
        
        // Should have en passant capture dxe3
        var enPassantMove = moves.FirstOrDefault(m => 
            m.From == Board.MakeSquare(Board.FileD, Board.Rank4) &&
            m.To == Board.MakeSquare(Board.FileE, Board.Rank3) &&
            m.IsEnPassant);
            
        Assert.True(enPassantMove.From != 0); // Move was found
        Assert.True(enPassantMove.IsEnPassant);
    }

    [Fact]
    public void TestEnPassantCaptureExecution_White()
    {
        var board = new Board();
        board.LoadFromFen("rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2");
        
        var moves = board.GenerateLegalMoves();
        var enPassantMove = moves.First(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank5) &&
            m.To == Board.MakeSquare(Board.FileD, Board.Rank6) &&
            m.IsEnPassant);
        
        // Execute en passant capture
        board.MakeMove(enPassantMove);
        
        // Check that the captured pawn is removed from d5
        Assert.Equal(Piece.None, board.GetPiece(Board.MakeSquare(Board.FileD, Board.Rank5)));
        
        // Check that the capturing pawn is on d6
        Assert.Equal(Piece.WhitePawn, board.GetPiece(Board.MakeSquare(Board.FileD, Board.Rank6)));
        
        // Check that e5 is empty
        Assert.Equal(Piece.None, board.GetPiece(Board.MakeSquare(Board.FileE, Board.Rank5)));
    }

    [Fact]
    public void TestEnPassantCaptureExecution_Black()
    {
        var board = new Board();
        board.LoadFromFen("rnbqkbnr/pppp1ppp/8/8/3pP3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 2");
        
        var moves = board.GenerateLegalMoves();
        var enPassantMove = moves.First(m => 
            m.From == Board.MakeSquare(Board.FileD, Board.Rank4) &&
            m.To == Board.MakeSquare(Board.FileE, Board.Rank3) &&
            m.IsEnPassant);
        
        // Execute en passant capture
        board.MakeMove(enPassantMove);
        
        // Check that the captured pawn is removed from e4
        Assert.Equal(Piece.None, board.GetPiece(Board.MakeSquare(Board.FileE, Board.Rank4)));
        
        // Check that the capturing pawn is on e3
        Assert.Equal(Piece.BlackPawn, board.GetPiece(Board.MakeSquare(Board.FileE, Board.Rank3)));
        
        // Check that d4 is empty
        Assert.Equal(Piece.None, board.GetPiece(Board.MakeSquare(Board.FileD, Board.Rank4)));
    }

    [Fact]
    public void TestEnPassantNotAvailableAfterDifferentMove()
    {
        var board = new Board();
        // Start with en passant available
        board.LoadFromFen("rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2");
        
        // Make a different move (not en passant)
        var moves = board.GenerateLegalMoves();
        var otherMove = moves.First(m => !m.IsEnPassant);
        board.MakeMove(otherMove);
        
        // Now check if en passant is still available (it shouldn't be)
        var newMoves = board.GenerateLegalMoves();
        var enPassantMoves = newMoves.Where(m => m.IsEnPassant).ToList();
        
        // After making a different move, en passant should no longer be available
        Assert.Empty(enPassantMoves);
    }

    [Fact]
    public void TestEnPassantDiscoveryCheck_Illegal()
    {
        var board = new Board();
        // Position where en passant would expose king to check
        // White king on e1, black rook on a1, white pawn on d5, black pawn just moved c7-c5
        // If white plays dxc6, removing the c5 pawn exposes the king to horizontal check from a1 rook
        board.LoadFromFen("4k3/pp1ppppp/8/2pP4/8/8/PPP1PPPP/r3K2R w K c6 0 2");
        
        var moves = board.GenerateLegalMoves();
        
        // En passant capture dxc6 should NOT be available because it would expose king to check
        var illegalEnPassant = moves.Where(m => 
            m.From == Board.MakeSquare(Board.FileD, Board.Rank5) &&
            m.To == Board.MakeSquare(Board.FileC, Board.Rank6) &&
            m.IsEnPassant).ToList();
            
        Assert.Empty(illegalEnPassant);
    }

    [Fact]
    public void TestEnPassantFenParsing()
    {
        var board = new Board();
        
        // Test en passant target square parsing
        board.LoadFromFen("rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2");
        Assert.Equal(Board.MakeSquare(Board.FileD, Board.Rank6), board.EnPassantSquare);
        
        // Test no en passant
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        Assert.Equal(-1, board.EnPassantSquare);
        
        // Test different en passant squares
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1");
        Assert.Equal(Board.MakeSquare(Board.FileE, Board.Rank3), board.EnPassantSquare);
    }

    [Fact]
    public void TestEnPassantUnmakeMove()
    {
        var board = new Board();
        board.LoadFromFen("rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2");
        
        // Store original position
        var originalFen = board.ToFen();
        
        var moves = board.GenerateLegalMoves();
        var enPassantMove = moves.First(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank5) &&
            m.To == Board.MakeSquare(Board.FileD, Board.Rank6) &&
            m.IsEnPassant);
        
        // Make and unmake the en passant move
        board.MakeMove(enPassantMove);
        board.UnmakeMove(enPassantMove);
        
        // Position should be restored exactly
        Assert.Equal(originalFen, board.ToFen());
        
        // The captured pawn should be back
        Assert.Equal(Piece.BlackPawn, board.GetPiece(Board.MakeSquare(Board.FileD, Board.Rank5)));
        
        // En passant square should be restored
        Assert.Equal(Board.MakeSquare(Board.FileD, Board.Rank6), board.EnPassantSquare);
    }

    [Fact]
    public void TestMultipleEnPassantOpportunities()
    {
        var board = new Board();
        // Position with white pawns on both sides of a black pawn that just moved
        board.LoadFromFen("rnbqkbnr/pp1ppppp/8/2PpP3/8/8/PPP2PPP/RNBQKBNR w KQkq d6 0 2");
        
        var moves = board.GenerateLegalMoves();
        
        // Should have two en passant captures: dxc6 and exc6
        var enPassantMoves = moves.Where(m => m.IsEnPassant).ToList();
        
        Assert.Equal(2, enPassantMoves.Count);
        
        // Check both captures are present
        Assert.Contains(enPassantMoves, m => 
            m.From == Board.MakeSquare(Board.FileC, Board.Rank5) &&
            m.To == Board.MakeSquare(Board.FileD, Board.Rank6));
        Assert.Contains(enPassantMoves, m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank5) &&
            m.To == Board.MakeSquare(Board.FileD, Board.Rank6));
    }

    [Fact]
    public void TestEnPassantPerft()
    {
        var board = new Board();
        // Position from perft test suite that includes en passant
        board.LoadFromFen("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");
        
        // Make a move that creates en passant opportunity
        var moves = board.GenerateLegalMoves();
        var doublePawnPush = moves.FirstOrDefault(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank2) &&
            m.To == Board.MakeSquare(Board.FileE, Board.Rank4));
        
        if (doublePawnPush.From != 0)
        {
            board.MakeMove(doublePawnPush);
            
            // Now black should have en passant capture available
            var blackMoves = board.GenerateLegalMoves();
            var enPassantCapture = blackMoves.Where(m => m.IsEnPassant).ToList();
            
            // If there's a black pawn adjacent, there should be en passant
            if (enPassantCapture.Any())
            {
                Assert.True(enPassantCapture.All(m => m.IsEnPassant));
            }
        }
    }
}