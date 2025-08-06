using ChessEngine;

namespace ChessEngine.Tests;

public class FenParsingTests
{
    [Fact]
    public void TestStartingPositionFen()
    {
        var board = new Board();
        string startingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        
        bool result = board.LoadFromFen(startingFen);
        Assert.True(result);
        
        // Test piece placement
        Assert.Equal(Piece.WhiteRook, board.GetPiece(Board.MakeSquare(Board.FileA, Board.Rank1)));
        Assert.Equal(Piece.WhiteKnight, board.GetPiece(Board.MakeSquare(Board.FileB, Board.Rank1)));
        Assert.Equal(Piece.WhiteBishop, board.GetPiece(Board.MakeSquare(Board.FileC, Board.Rank1)));
        Assert.Equal(Piece.WhiteQueen, board.GetPiece(Board.MakeSquare(Board.FileD, Board.Rank1)));
        Assert.Equal(Piece.WhiteKing, board.GetPiece(Board.MakeSquare(Board.FileE, Board.Rank1)));
        Assert.Equal(Piece.WhitePawn, board.GetPiece(Board.MakeSquare(Board.FileA, Board.Rank2)));
        
        Assert.Equal(Piece.BlackRook, board.GetPiece(Board.MakeSquare(Board.FileA, Board.Rank8)));
        Assert.Equal(Piece.BlackKing, board.GetPiece(Board.MakeSquare(Board.FileE, Board.Rank8)));
        Assert.Equal(Piece.BlackPawn, board.GetPiece(Board.MakeSquare(Board.FileA, Board.Rank7)));
        
        // Test game state
        Assert.Equal(Color.White, board.SideToMove);
        Assert.True(board.WhiteCanCastleKingside);
        Assert.True(board.WhiteCanCastleQueenside);
        Assert.True(board.BlackCanCastleKingside);
        Assert.True(board.BlackCanCastleQueenside);
        Assert.Equal(-1, board.EnPassantSquare);
        Assert.Equal(0, board.HalfMoveClock);
        Assert.Equal(1, board.FullMoveNumber);
    }

    [Fact]
    public void TestFenPiecePlacement()
    {
        var board = new Board();
        
        // Test various piece placements
        Assert.True(board.LoadFromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1"));
        Assert.Equal(Piece.BlackRook, board.GetPiece(Board.AlgebraicToSquare("a8")));
        Assert.Equal(Piece.BlackKing, board.GetPiece(Board.AlgebraicToSquare("e8")));
        Assert.Equal(Piece.BlackRook, board.GetPiece(Board.AlgebraicToSquare("h8")));
        Assert.Equal(Piece.WhiteRook, board.GetPiece(Board.AlgebraicToSquare("a1")));
        Assert.Equal(Piece.WhiteKing, board.GetPiece(Board.AlgebraicToSquare("e1")));
        Assert.Equal(Piece.WhiteRook, board.GetPiece(Board.AlgebraicToSquare("h1")));
        
        // Test empty squares
        Assert.Equal(Piece.None, board.GetPiece(Board.AlgebraicToSquare("b8")));
        Assert.Equal(Piece.None, board.GetPiece(Board.AlgebraicToSquare("d4")));
    }

    [Fact]
    public void TestFenWithMixedPieces()
    {
        var board = new Board();
        
        // Test a complex middle game position
        Assert.True(board.LoadFromFen("r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4"));
        
        // Test specific pieces
        Assert.Equal(Piece.BlackRook, board.GetPiece(Board.AlgebraicToSquare("a8")));
        Assert.Equal(Piece.BlackBishop, board.GetPiece(Board.AlgebraicToSquare("c8")));
        Assert.Equal(Piece.BlackQueen, board.GetPiece(Board.AlgebraicToSquare("d8")));
        Assert.Equal(Piece.BlackKnight, board.GetPiece(Board.AlgebraicToSquare("c6")));
        Assert.Equal(Piece.BlackBishop, board.GetPiece(Board.AlgebraicToSquare("c5")));
        Assert.Equal(Piece.WhiteBishop, board.GetPiece(Board.AlgebraicToSquare("c4")));
        Assert.Equal(Piece.WhiteKnight, board.GetPiece(Board.AlgebraicToSquare("f3")));
        
        // Test move counters
        Assert.Equal(4, board.HalfMoveClock);
        Assert.Equal(4, board.FullMoveNumber);
    }

    [Fact]
    public void TestFenSideToMove()
    {
        var board = new Board();
        
        // Test white to move
        Assert.True(board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"));
        Assert.Equal(Color.White, board.SideToMove);
        
        // Test black to move
        Assert.True(board.LoadFromFen("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1"));
        Assert.Equal(Color.Black, board.SideToMove);
    }

    [Fact]
    public void TestFenCastlingRights()
    {
        var board = new Board();
        
        // Test all castling rights
        Assert.True(board.LoadFromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1"));
        Assert.True(board.WhiteCanCastleKingside);
        Assert.True(board.WhiteCanCastleQueenside);
        Assert.True(board.BlackCanCastleKingside);
        Assert.True(board.BlackCanCastleQueenside);
        
        // Test partial castling rights
        Assert.True(board.LoadFromFen("r3k2r/8/8/8/8/8/8/R3K2R w Kq - 0 1"));
        Assert.True(board.WhiteCanCastleKingside);
        Assert.False(board.WhiteCanCastleQueenside);
        Assert.False(board.BlackCanCastleKingside);
        Assert.True(board.BlackCanCastleQueenside);
        
        // Test no castling rights
        Assert.True(board.LoadFromFen("r3k2r/8/8/8/8/8/8/R3K2R w - - 0 1"));
        Assert.False(board.WhiteCanCastleKingside);
        Assert.False(board.WhiteCanCastleQueenside);
        Assert.False(board.BlackCanCastleKingside);
        Assert.False(board.BlackCanCastleQueenside);
    }

    [Fact]
    public void TestFenEnPassant()
    {
        var board = new Board();
        
        // Test no en passant
        Assert.True(board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"));
        Assert.Equal(-1, board.EnPassantSquare);
        
        // Test en passant square
        Assert.True(board.LoadFromFen("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1"));
        Assert.Equal(Board.AlgebraicToSquare("e3"), board.EnPassantSquare);
        
        // Test different en passant squares
        Assert.True(board.LoadFromFen("rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2"));
        Assert.Equal(Board.AlgebraicToSquare("d6"), board.EnPassantSquare);
    }

    [Fact]
    public void TestFenToString()
    {
        var board = new Board();
        
        // Test starting position round trip
        string startingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        Assert.True(board.LoadFromFen(startingFen));
        Assert.Equal(startingFen, board.ToFen());
        
        // Test another position round trip
        string testFen = "r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4";
        Assert.True(board.LoadFromFen(testFen));
        Assert.Equal(testFen, board.ToFen());
        
        // Test position with en passant
        string enPassantFen = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
        Assert.True(board.LoadFromFen(enPassantFen));
        Assert.Equal(enPassantFen, board.ToFen());
    }

    [Fact]
    public void TestInvalidFen()
    {
        var board = new Board();
        
        // Test invalid FEN strings
        Assert.False(board.LoadFromFen(""));
        Assert.False(board.LoadFromFen("invalid"));
        Assert.False(board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"));  // Missing parts
        Assert.False(board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP w KQkq - 0 1"));  // Missing rank
        Assert.False(board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR x KQkq - 0 1"));  // Invalid side
    }

    [Fact]
    public void TestSpecialPositions()
    {
        var board = new Board();
        
        // Test empty board except kings
        Assert.True(board.LoadFromFen("4k3/8/8/8/8/8/8/4K3 w - - 0 1"));
        Assert.Equal(Piece.BlackKing, board.GetPiece(Board.AlgebraicToSquare("e8")));
        Assert.Equal(Piece.WhiteKing, board.GetPiece(Board.AlgebraicToSquare("e1")));
        
        // Test pawn promotion setup
        Assert.True(board.LoadFromFen("8/P7/8/8/8/8/8/4K2k w - - 0 1"));
        Assert.Equal(Piece.WhitePawn, board.GetPiece(Board.AlgebraicToSquare("a7")));
        
        // Test en passant capture setup
        Assert.True(board.LoadFromFen("4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1"));
        Assert.Equal(Piece.BlackPawn, board.GetPiece(Board.AlgebraicToSquare("d5")));
        Assert.Equal(Piece.WhitePawn, board.GetPiece(Board.AlgebraicToSquare("e5")));
        Assert.Equal(Board.AlgebraicToSquare("d6"), board.EnPassantSquare);
    }

    [Fact]
    public void TestFenPieceCount()
    {
        var board = new Board();
        Assert.True(board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"));
        
        // Count pieces
        int whitePieces = 0, blackPieces = 0;
        for (int square = 0; square < Board.BoardSize; square++)
        {
            if (!board.IsValidSquare(square)) continue;
            
            int piece = board.GetPiece(square);
            if (piece != Piece.None)
            {
                if (Piece.IsWhite(piece)) whitePieces++;
                else if (Piece.IsBlack(piece)) blackPieces++;
            }
        }
        
        Assert.Equal(16, whitePieces);
        Assert.Equal(16, blackPieces);
    }
}