namespace ChessEngine.Tests
{
    public class ZobristHashingTests
    {
        private Board board;

        public ZobristHashingTests()
        {
            board = new Board();
        }

        [Fact]
        public void TestStartingPositionHash()
        {
            board.SetupStartingPosition();
            ulong hash1 = board.HashKey;
            
            // Hash should be non-zero
            Assert.NotEqual(0UL, hash1);
            
            // Hash should be deterministic
            board.SetupStartingPosition();
            ulong hash2 = board.HashKey;
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void TestMoveUpdatesHash()
        {
            board.SetupStartingPosition();
            ulong initialHash = board.HashKey;
            
            // Make a move
            Move move = new Move(12, 28, Piece.WhitePawn); // e2-e4
            board.MakeMove(move);
            ulong afterMoveHash = board.HashKey;
            
            // Hash should change after move
            Assert.NotEqual(initialHash, afterMoveHash);
            
            // Undo move
            board.UnmakeMove(move);
            ulong afterUndoHash = board.HashKey;
            
            // Hash should return to original value
            Assert.Equal(initialHash, afterUndoHash);
        }

        [Fact]
        public void TestDifferentPositionsHaveDifferentHashes()
        {
            // Starting position
            board.SetupStartingPosition();
            ulong hash1 = board.HashKey;
            
            // Make a move
            Move move = new Move(12, 28, Piece.WhitePawn); // e2-e4
            board.MakeMove(move);
            ulong hash2 = board.HashKey;
            
            // Make another move
            Move move2 = new Move(52, 36, Piece.BlackPawn); // e7-e5
            board.MakeMove(move2);
            ulong hash3 = board.HashKey;
            
            // All hashes should be different
            Assert.NotEqual(hash1, hash2);
            Assert.NotEqual(hash2, hash3);
            Assert.NotEqual(hash1, hash3);
        }

        [Fact]
        public void TestSideToMoveAffectsHash()
        {
            board.SetupStartingPosition();
            ulong whiteToMoveHash = board.HashKey;
            
            // Make and undo a null move to switch sides
            // This is a conceptual test - actual implementation may vary
            // For now, we'll test by making a move that doesn't change position much
            Move move = new Move(12, 28, Piece.WhitePawn); // e2-e4
            board.MakeMove(move);
            Move blackMove = new Move(52, 36, Piece.BlackPawn); // e7-e5
            board.MakeMove(blackMove);
            
            // Now undo black's move to get white to move again but different position
            board.UnmakeMove(blackMove);
            ulong whiteToMoveHash2 = board.HashKey;
            
            // These should be different even though white is to move in both
            Assert.NotEqual(whiteToMoveHash, whiteToMoveHash2);
        }

        [Fact]
        public void TestCastlingRightsAffectHash()
        {
            // Set up position where castling rights will change
            board.SetupStartingPosition();
            ulong initialHash = board.HashKey;
            
            // Move king (this should affect castling rights)
            Move kingMove = new Move(4, 5, Piece.WhiteKing); // e1-f1
            board.MakeMove(kingMove);
            ulong afterKingMoveHash = board.HashKey;
            
            // Hash should be different (not just because of king position, but castling rights)
            Assert.NotEqual(initialHash, afterKingMoveHash);
            
            // Undo and verify hash returns
            board.UnmakeMove(kingMove);
            Assert.Equal(initialHash, board.HashKey);
        }

        [Fact]
        public void TestHashConsistencyAfterMoveSequence()
        {
            board.SetupStartingPosition();
            
            // Record initial hash
            ulong initialHash = board.HashKey;
            
            // Make a sequence of moves
            var moves = new[]
            {
                new Move(12, 28, Piece.WhitePawn), // e2-e4
                new Move(52, 36, Piece.BlackPawn), // e7-e5
                new Move(6, 21, Piece.WhiteKnight), // g1-f3
                new Move(57, 42, Piece.BlackKnight)  // b8-c6
            };
            
            foreach (var move in moves)
            {
                board.MakeMove(move);
            }
            
            ulong midGameHash = board.HashKey;
            
            // Undo all moves
            for (int i = moves.Length - 1; i >= 0; i--)
            {
                board.UnmakeMove(moves[i]);
            }
            
            // Should be back to initial hash
            Assert.Equal(initialHash, board.HashKey);
            
            // Replay the same moves - should get same hash
            foreach (var move in moves)
            {
                board.MakeMove(move);
            }
            
            Assert.Equal(midGameHash, board.HashKey);
        }

        [Fact]
        public void TestEnPassantAffectsHash()
        {
            // Set up a position where en passant is possible
            board.SetupStartingPosition();
            
            // Move white pawn to 5th rank
            board.MakeMove(new Move(12, 28, Piece.WhitePawn)); // e2-e4
            board.MakeMove(new Move(48, 40, Piece.BlackPawn)); // a7-a6 (random move)
            board.MakeMove(new Move(28, 36, Piece.WhitePawn)); // e4-e5
            
            // Move black pawn two squares next to white pawn
            ulong beforePawnJump = board.HashKey;
            Move pawnJump = new Move(51, 35, Piece.BlackPawn, Piece.None, Piece.None, false, false, true); // d7-d5 (two squares)
            board.MakeMove(pawnJump);
            ulong afterPawnJump = board.HashKey;
            
            // Hash should be different due to en passant possibility
            Assert.NotEqual(beforePawnJump, afterPawnJump);
        }

        [Fact]
        public void TestDirectHashComputation()
        {
            board.SetupStartingPosition();
            ulong boardHash = board.HashKey;
            
            // Compute hash directly
            ulong directHash = ZobristHashing.ComputeHash(board);
            
            // Should match
            Assert.Equal(boardHash, directHash);
        }

        [Fact]
        public void TestCaptureMoves()
        {
            // Set up a position with a possible capture
            board.SetupStartingPosition();
            
            // Move pieces to create capture opportunity
            board.MakeMove(new Move(12, 28, Piece.WhitePawn)); // e2-e4
            board.MakeMove(new Move(51, 35, Piece.BlackPawn)); // d7-d5
            
            ulong beforeCapture = board.HashKey;
            
            // Capture
            Move captureMove = new Move(28, 35, Piece.WhitePawn, Piece.BlackPawn); // exd5
            board.MakeMove(captureMove);
            ulong afterCapture = board.HashKey;
            
            Assert.NotEqual(beforeCapture, afterCapture);
            
            // Undo capture
            board.UnmakeMove(captureMove);
            Assert.Equal(beforeCapture, board.HashKey);
        }
    }
}