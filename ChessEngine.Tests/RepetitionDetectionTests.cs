namespace ChessEngine.Tests
{
    public class RepetitionDetectionTests
    {
        [Fact]
        public void IsRepetition_NewPosition_ReturnsFalse()
        {
            var board = new Board();
            board.SetupStartingPosition();
            
            Assert.False(board.IsRepetition());
        }

        [Fact]
        public void IsRepetition_AfterOneMove_ReturnsFalse()
        {
            var board = new Board();
            board.SetupStartingPosition();
            
            var moves = board.GenerateLegalMoves();
            board.MakeMove(moves[0]);
            
            Assert.False(board.IsRepetition());
        }

        [Fact]
        public void IsRepetition_RepeatedPosition_ReturnsTrue()
        {
            var board = new Board();
            board.SetupStartingPosition();
            
            // Make a sequence of moves that returns to starting position
            var move1 = new Move(Board.MakeSquare(Board.FileG, Board.Rank1), 
                                Board.MakeSquare(Board.FileF, Board.Rank3), 
                                Piece.WhiteKnight, Piece.None);
            var move2 = new Move(Board.MakeSquare(Board.FileG, Board.Rank8), 
                                Board.MakeSquare(Board.FileF, Board.Rank6), 
                                Piece.BlackKnight, Piece.None);
            var move3 = new Move(Board.MakeSquare(Board.FileF, Board.Rank3), 
                                Board.MakeSquare(Board.FileG, Board.Rank1), 
                                Piece.WhiteKnight, Piece.None);
            var move4 = new Move(Board.MakeSquare(Board.FileF, Board.Rank6), 
                                Board.MakeSquare(Board.FileG, Board.Rank8), 
                                Piece.BlackKnight, Piece.None);
            
            board.MakeMove(move1);
            board.MakeMove(move2);
            board.MakeMove(move3);
            board.MakeMove(move4);
            
            // Position should now be the same as starting position
            Assert.True(board.IsRepetition());
        }

        [Fact]
        public void IsThreefoldRepetition_TwoRepetitions_ReturnsFalse()
        {
            var board = new Board();
            board.SetupStartingPosition();
            
            // Make moves that repeat position once (not twice)
            var move1 = new Move(Board.MakeSquare(Board.FileG, Board.Rank1), 
                                Board.MakeSquare(Board.FileF, Board.Rank3), 
                                Piece.WhiteKnight, Piece.None);
            var move2 = new Move(Board.MakeSquare(Board.FileG, Board.Rank8), 
                                Board.MakeSquare(Board.FileF, Board.Rank6), 
                                Piece.BlackKnight, Piece.None);
            var move3 = new Move(Board.MakeSquare(Board.FileF, Board.Rank3), 
                                Board.MakeSquare(Board.FileG, Board.Rank1), 
                                Piece.WhiteKnight, Piece.None);
            var move4 = new Move(Board.MakeSquare(Board.FileF, Board.Rank6), 
                                Board.MakeSquare(Board.FileG, Board.Rank8), 
                                Piece.BlackKnight, Piece.None);
            
            // One repetition cycle - should be back to starting position for 2nd time
            board.MakeMove(move1);
            board.MakeMove(move2);
            board.MakeMove(move3);
            board.MakeMove(move4);
            
            // This is only the second occurrence, not third
            Assert.False(board.IsThreefoldRepetition());
        }

        [Fact]
        public void IsThreefoldRepetition_ThreeRepetitions_ReturnsTrue()
        {
            var board = new Board();
            board.SetupStartingPosition();
            
            var move1 = new Move(Board.MakeSquare(Board.FileG, Board.Rank1), 
                                Board.MakeSquare(Board.FileF, Board.Rank3), 
                                Piece.WhiteKnight, Piece.None);
            var move2 = new Move(Board.MakeSquare(Board.FileG, Board.Rank8), 
                                Board.MakeSquare(Board.FileF, Board.Rank6), 
                                Piece.BlackKnight, Piece.None);
            var move3 = new Move(Board.MakeSquare(Board.FileF, Board.Rank3), 
                                Board.MakeSquare(Board.FileG, Board.Rank1), 
                                Piece.WhiteKnight, Piece.None);
            var move4 = new Move(Board.MakeSquare(Board.FileF, Board.Rank6), 
                                Board.MakeSquare(Board.FileG, Board.Rank8), 
                                Piece.BlackKnight, Piece.None);
            
            // Three repetition cycles
            for (int i = 0; i < 3; i++)
            {
                board.MakeMove(move1);
                board.MakeMove(move2);
                board.MakeMove(move3);
                board.MakeMove(move4);
            }
            
            Assert.True(board.IsThreefoldRepetition());
        }

        [Fact]
        public void PositionHistory_UndoMove_RemovesFromHistory()
        {
            var board = new Board();
            board.SetupStartingPosition();
            
            var moves = board.GenerateLegalMoves();
            var initialHash = board.HashKey;
            
            board.MakeMove(moves[0]);
            board.UnmakeMove(moves[0]);
            
            // Should be back to original position
            Assert.Equal(initialHash, board.HashKey);
            Assert.False(board.IsRepetition());
        }

        [Fact]
        public void IsGameOver_WithRepetition_ReturnsFalse()
        {
            var board = new Board();
            board.SetupStartingPosition();
            
            var move1 = new Move(Board.MakeSquare(Board.FileG, Board.Rank1), 
                                Board.MakeSquare(Board.FileF, Board.Rank3), 
                                Piece.WhiteKnight, Piece.None);
            var move2 = new Move(Board.MakeSquare(Board.FileG, Board.Rank8), 
                                Board.MakeSquare(Board.FileF, Board.Rank6), 
                                Piece.BlackKnight, Piece.None);
            var move3 = new Move(Board.MakeSquare(Board.FileF, Board.Rank3), 
                                Board.MakeSquare(Board.FileG, Board.Rank1), 
                                Piece.WhiteKnight, Piece.None);
            var move4 = new Move(Board.MakeSquare(Board.FileF, Board.Rank6), 
                                Board.MakeSquare(Board.FileG, Board.Rank8), 
                                Piece.BlackKnight, Piece.None);
            
            board.MakeMove(move1);
            board.MakeMove(move2);
            board.MakeMove(move3);
            board.MakeMove(move4);
            
            // IsGameOver() no longer includes repetition detection (only used in search)
            Assert.True(board.IsRepetition());
            Assert.False(board.IsGameOver());
        }
    }
}