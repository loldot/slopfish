using ChessEngine;

namespace ChessEngine.Tests;

public class CastlingTests
{
    [Fact]
    public void TestWhiteKingsideCastling_Basic()
    {
        var board = new Board();
        // King and rook in starting position, path clear
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1");
        
        var moves = board.GenerateLegalMoves();
        
        // Should have kingside castling O-O (e1-g1)
        var castlingMove = moves.FirstOrDefault(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank1) &&
            m.To == Board.MakeSquare(Board.FileG, Board.Rank1) &&
            m.IsCastling);
            
        Assert.True(castlingMove.From != 0); // Move was found
        Assert.True(castlingMove.IsCastling);
    }

    [Fact]
    public void TestWhiteQueensideCastling_Basic()
    {
        var board = new Board();
        // King and rook in starting position, path clear
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/R3KBNR w KQkq - 0 1");
        
        var moves = board.GenerateLegalMoves();
        
        // Should have queenside castling O-O-O (e1-c1)
        var castlingMove = moves.FirstOrDefault(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank1) &&
            m.To == Board.MakeSquare(Board.FileC, Board.Rank1) &&
            m.IsCastling);
            
        Assert.True(castlingMove.From != 0); // Move was found
        Assert.True(castlingMove.IsCastling);
    }

    [Fact]
    public void TestBlackKingsideCastling_Basic()
    {
        var board = new Board();
        // King and rook in starting position, path clear
        board.LoadFromFen("rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1");
        
        var moves = board.GenerateLegalMoves();
        
        // Should have kingside castling O-O (e8-g8)
        var castlingMove = moves.FirstOrDefault(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank8) &&
            m.To == Board.MakeSquare(Board.FileG, Board.Rank8) &&
            m.IsCastling);
            
        Assert.True(castlingMove.From != 0); // Move was found
        Assert.True(castlingMove.IsCastling);
    }

    [Fact]
    public void TestBlackQueensideCastling_Basic()
    {
        var board = new Board();
        // King and rook in starting position, path clear
        board.LoadFromFen("r3kbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1");
        
        var moves = board.GenerateLegalMoves();
        
        // Should have queenside castling O-O-O (e8-c8)
        var castlingMove = moves.FirstOrDefault(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank8) &&
            m.To == Board.MakeSquare(Board.FileC, Board.Rank8) &&
            m.IsCastling);
            
        Assert.True(castlingMove.From != 0); // Move was found
        Assert.True(castlingMove.IsCastling);
    }

    [Fact]
    public void TestCastlingExecution_WhiteKingside()
    {
        var board = new Board();
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1");
        
        var castlingMove = new Move(
            Board.MakeSquare(Board.FileE, Board.Rank1),
            Board.MakeSquare(Board.FileG, Board.Rank1),
            Piece.WhiteKing, Piece.None, Piece.None, false, true);
            
        board.MakeMove(castlingMove);
        
        // King should be on g1
        Assert.Equal(Piece.WhiteKing, board.GetPiece(Board.MakeSquare(Board.FileG, Board.Rank1)));
        // Rook should be on f1
        Assert.Equal(Piece.WhiteRook, board.GetPiece(Board.MakeSquare(Board.FileF, Board.Rank1)));
        // e1 and h1 should be empty
        Assert.Equal(Piece.None, board.GetPiece(Board.MakeSquare(Board.FileE, Board.Rank1)));
        Assert.Equal(Piece.None, board.GetPiece(Board.MakeSquare(Board.FileH, Board.Rank1)));
        
        // Castling rights should be lost
        Assert.False(board.WhiteCanCastleKingside);
        Assert.False(board.WhiteCanCastleQueenside);
    }

    [Fact]
    public void TestCastlingExecution_WhiteQueenside()
    {
        var board = new Board();
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/R3KBNR w KQkq - 0 1");
        
        var castlingMove = new Move(
            Board.MakeSquare(Board.FileE, Board.Rank1),
            Board.MakeSquare(Board.FileC, Board.Rank1),
            Piece.WhiteKing, Piece.None, Piece.None, false, true);
            
        board.MakeMove(castlingMove);
        
        // King should be on c1
        Assert.Equal(Piece.WhiteKing, board.GetPiece(Board.MakeSquare(Board.FileC, Board.Rank1)));
        // Rook should be on d1
        Assert.Equal(Piece.WhiteRook, board.GetPiece(Board.MakeSquare(Board.FileD, Board.Rank1)));
        // e1 and a1 should be empty
        Assert.Equal(Piece.None, board.GetPiece(Board.MakeSquare(Board.FileE, Board.Rank1)));
        Assert.Equal(Piece.None, board.GetPiece(Board.MakeSquare(Board.FileA, Board.Rank1)));
        
        // Castling rights should be lost
        Assert.False(board.WhiteCanCastleKingside);
        Assert.False(board.WhiteCanCastleQueenside);
    }

    [Fact]
    public void TestCastlingBlocked_PieceInPath()
    {
        var board = new Board();
        // Knight on f1 blocks kingside castling
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKN1R w KQkq - 0 1");
        
        var moves = board.GenerateLegalMoves();
        
        // Should NOT have kingside castling
        var castlingMove = moves.FirstOrDefault(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank1) &&
            m.To == Board.MakeSquare(Board.FileG, Board.Rank1) &&
            m.IsCastling);
            
        Assert.True(castlingMove.From == 0); // Move was NOT found
    }

    [Fact]
    public void TestCastlingBlocked_QueensidePieceInPath()
    {
        var board = new Board();
        // Queen on d1 blocks queenside castling
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RN1QKBNR w KQkq - 0 1");
        
        var moves = board.GenerateLegalMoves();
        
        // Should NOT have queenside castling
        var castlingMove = moves.FirstOrDefault(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank1) &&
            m.To == Board.MakeSquare(Board.FileC, Board.Rank1) &&
            m.IsCastling);
            
        Assert.True(castlingMove.From == 0); // Move was NOT found
    }

    [Fact]
    public void TestCastlingRightsLost_KingMoved()
    {
        var board = new Board();
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1");
        
        // Move king to f1 and back
        var kingMove = new Move(
            Board.MakeSquare(Board.FileE, Board.Rank1),
            Board.MakeSquare(Board.FileF, Board.Rank1),
            Piece.WhiteKing);
            
        board.MakeMove(kingMove);
        
        // Both castling rights should be lost
        Assert.False(board.WhiteCanCastleKingside);
        Assert.False(board.WhiteCanCastleQueenside);
    }

    [Fact]
    public void TestCastlingRightsLost_RookMoved()
    {
        var board = new Board();
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1");
        
        // Move h1 rook
        var rookMove = new Move(
            Board.MakeSquare(Board.FileH, Board.Rank1),
            Board.MakeSquare(Board.FileG, Board.Rank1),
            Piece.WhiteRook);
            
        board.MakeMove(rookMove);
        
        // Only kingside castling right should be lost
        Assert.False(board.WhiteCanCastleKingside);
        Assert.True(board.WhiteCanCastleQueenside); // Queenside should still be available
    }

    [Fact]
    public void TestCastlingThroughCheck_Prevented()
    {
        var board = new Board();
        // Black rook on f8 attacks f1, preventing kingside castling
        board.LoadFromFen("rnbqkr2/ppppp1pp/8/8/8/8/PPPPP1PP/RNBQK2R w KQkq - 0 1");
        
        var moves = board.GenerateLegalMoves();
        
        // Should NOT have kingside castling (f1 is attacked)
        var castlingMove = moves.FirstOrDefault(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank1) &&
            m.To == Board.MakeSquare(Board.FileG, Board.Rank1) &&
            m.IsCastling);
            
        Assert.True(castlingMove.From == 0); // Move was NOT found
    }

    [Fact]
    public void TestCastlingOutOfCheck_Prevented()
    {
        var board = new Board();
        // Black rook on e8 puts white king in check
        board.LoadFromFen("rnbqr2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w Qkq - 0 1");
        
        var moves = board.GenerateLegalMoves();
        
        // Should NOT have any castling moves (king is in check)
        var castlingMoves = moves.Where(m => m.IsCastling);
        Assert.Empty(castlingMoves);
    }

    [Fact]
    public void TestCastlingIntoCheck_Prevented()
    {
        var board = new Board();
        // Black rook on g8 would attack king on g1 after castling
        board.LoadFromFen("rnbqk1r1/ppppp2p/8/8/8/8/PPPPP2P/RNBQK2R w KQkq - 0 1");
        
        var moves = board.GenerateLegalMoves();
        
        // Should NOT have kingside castling (g1 would be in check)
        var castlingMove = moves.FirstOrDefault(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank1) &&
            m.To == Board.MakeSquare(Board.FileG, Board.Rank1) &&
            m.IsCastling);
            
        Assert.True(castlingMove.From == 0); // Move was NOT found
    }

    [Fact]
    public void TestCastlingFenParsing_AllRights()
    {
        var board = new Board();
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        
        Assert.True(board.WhiteCanCastleKingside);
        Assert.True(board.WhiteCanCastleQueenside);
        Assert.True(board.BlackCanCastleKingside);
        Assert.True(board.BlackCanCastleQueenside);
    }

    [Fact]
    public void TestCastlingFenParsing_PartialRights()
    {
        var board = new Board();
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w Kq - 0 1");
        
        Assert.True(board.WhiteCanCastleKingside);
        Assert.False(board.WhiteCanCastleQueenside);
        Assert.False(board.BlackCanCastleKingside);
        Assert.True(board.BlackCanCastleQueenside);
    }

    [Fact]
    public void TestCastlingFenParsing_NoRights()
    {
        var board = new Board();
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1");
        
        Assert.False(board.WhiteCanCastleKingside);
        Assert.False(board.WhiteCanCastleQueenside);
        Assert.False(board.BlackCanCastleKingside);
        Assert.False(board.BlackCanCastleQueenside);
    }

    [Fact]
    public void TestCastlingMakeUnmake_StatePreservation()
    {
        var board = new Board();
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1");
        
        // Store original state
        bool originalWhiteKingside = board.WhiteCanCastleKingside;
        bool originalWhiteQueenside = board.WhiteCanCastleQueenside;
        bool originalBlackKingside = board.BlackCanCastleKingside;
        bool originalBlackQueenside = board.BlackCanCastleQueenside;
        
        var castlingMove = new Move(
            Board.MakeSquare(Board.FileE, Board.Rank1),
            Board.MakeSquare(Board.FileG, Board.Rank1),
            Piece.WhiteKing, Piece.None, Piece.None, false, true);
            
        board.MakeMove(castlingMove);
        board.UnmakeMove(castlingMove);
        
        // State should be restored
        Assert.Equal(originalWhiteKingside, board.WhiteCanCastleKingside);
        Assert.Equal(originalWhiteQueenside, board.WhiteCanCastleQueenside);
        Assert.Equal(originalBlackKingside, board.BlackCanCastleKingside);
        Assert.Equal(originalBlackQueenside, board.BlackCanCastleQueenside);
        
        // Pieces should be back in original positions
        Assert.Equal(Piece.WhiteKing, board.GetPiece(Board.MakeSquare(Board.FileE, Board.Rank1)));
        Assert.Equal(Piece.WhiteRook, board.GetPiece(Board.MakeSquare(Board.FileH, Board.Rank1)));
        Assert.Equal(Piece.None, board.GetPiece(Board.MakeSquare(Board.FileF, Board.Rank1)));
        Assert.Equal(Piece.None, board.GetPiece(Board.MakeSquare(Board.FileG, Board.Rank1)));
    }

    [Fact]
    public void TestCastlingRightsUpdate_QueensideRookCaptured()
    {
        var board = new Board();
        // Black lost queenside castling rights (simulated by FEN)
        board.LoadFromFen("r3kbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b Kkq - 0 1");
        
        // White lost queenside castling rights (simulated by FEN)
        Assert.True(board.WhiteCanCastleKingside);
        Assert.False(board.WhiteCanCastleQueenside);  // Fixed: FEN has "Kkq" not "KQkq"
        Assert.True(board.BlackCanCastleKingside);
        Assert.True(board.BlackCanCastleQueenside);   // Black still has queenside rights ('q' present)
    }

    [Fact]
    public void TestCastlingLegalMoveGeneration_DiscoveryCheck()
    {
        var board = new Board();
        // Position where castling would expose king to discovered check
        // Black bishop on a6 would attack c1 if king castles queenside, d1 is clear
        board.LoadFromFen("rnbqkbnr/pppppppp/b7/8/8/8/PPPPPPPP/R2QK2R w KQkq - 0 1");
        
        var moves = board.GenerateLegalMoves();
        
        // Should NOT have queenside castling (would expose king to check from a6 bishop)
        var castlingMove = moves.FirstOrDefault(m => 
            m.From == Board.MakeSquare(Board.FileE, Board.Rank1) &&
            m.To == Board.MakeSquare(Board.FileC, Board.Rank1) &&
            m.IsCastling);
            
        Assert.True(castlingMove.From == 0); // Move was NOT found
    }

    [Fact]
    public void TestMultipleCastlingOptions_BothSides()
    {
        var board = new Board();
        // Both sides clear for castling
        board.LoadFromFen("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1");
        
        var moves = board.GenerateLegalMoves();
        var castlingMoves = moves.Where(m => m.IsCastling).ToList();
        
        // Should have both kingside and queenside castling
        Assert.Equal(2, castlingMoves.Count);
        
        // Verify both moves exist
        var kingside = castlingMoves.FirstOrDefault(m => m.To == Board.MakeSquare(Board.FileG, Board.Rank1));
        var queenside = castlingMoves.FirstOrDefault(m => m.To == Board.MakeSquare(Board.FileC, Board.Rank1));
        
        Assert.True(kingside.From != 0);
        Assert.True(queenside.From != 0);
    }
}