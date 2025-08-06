using ChessEngine;

namespace ChessEngine.Tests;

public class DebugCastlingPosition
{
    [Fact]
    public void DebugCastlingThroughCheck()
    {
        var board = new Board();
        // Black rook on g8, white can castle
        board.LoadFromFen("rnbqk1r1/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 0 1");
        
        // Check if f1 is attacked by black
        bool f1Attacked = MoveGenerator.IsSquareAttacked(board, Board.MakeSquare(Board.FileF, Board.Rank1), Color.Black);
        
        // Check if e1 (king) is in check
        bool kingInCheck = board.IsInCheck(Color.White);
        
        var moves = board.GenerateLegalMoves();
        
        var castlingMoves = moves.Where(m => m.IsCastling);
        
        // Check what's on g8
        int g8Piece = board.GetPiece(Board.MakeSquare(Board.FileG, Board.Rank8));
        
        // With this position, f1 should not be attacked and king should not be in check
        // Castling should be possible
        Assert.False(f1Attacked);
        Assert.False(kingInCheck);
        Assert.NotEmpty(castlingMoves); // Should be able to castle
    }
}