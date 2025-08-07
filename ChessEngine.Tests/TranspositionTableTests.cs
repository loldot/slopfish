namespace ChessEngine.Tests
{
    public class TranspositionTableTests
    {
        private TranspositionTable tt;
        private Board board;

        public TranspositionTableTests()
        {
            tt = new TranspositionTable(1); // 1MB for testing
            board = new Board();
            board.SetupStartingPosition();
        }

        [Fact]
        public void TestBasicStorageAndRetrieval()
        {
            ulong key = board.HashKey;
            int depth = 5;
            int score = 150;
            Move bestMove = new Move(12, 28, Piece.WhitePawn); // e2-e4
            
            // Store an exact score
            tt.Store(key, bestMove, score, depth, TTEntryType.Exact, Color.White);
            
            // Retrieve and verify
            bool found = tt.Probe(key, out TTEntry entry);
            Assert.True(found, "Entry should be found");
            Assert.Equal(depth, entry.Depth);
            Assert.Equal(score, entry.Score);
            Assert.Equal(TTEntryType.Exact, entry.Type);
            Assert.Equal(bestMove.From, entry.BestMove.From);
            Assert.Equal(bestMove.To, entry.BestMove.To);
        }

        [Fact]
        public void TestBoundTypes()
        {
            ulong key = board.HashKey;
            int depth = 3;
            Move move = new Move(12, 28, Piece.WhitePawn); // e2-e4

            // Test lower bound
            tt.Store(key, move, -200, depth, TTEntryType.LowerBound, Color.White);
            bool found = tt.Probe(key, out TTEntry entry);
            Assert.True(found);
            Assert.Equal(TTEntryType.LowerBound, entry.Type);
            Assert.Equal(-200, entry.Score);

            // Test upper bound (overwrite)
            tt.Store(key, move, 300, depth, TTEntryType.UpperBound, Color.White);
            found = tt.Probe(key, out entry);
            Assert.True(found);
            Assert.Equal(TTEntryType.UpperBound, entry.Type);
            Assert.Equal(300, entry.Score);

            // Test exact score (overwrite)
            tt.Store(key, move, 100, depth, TTEntryType.Exact, Color.White);
            found = tt.Probe(key, out entry);
            Assert.True(found);
            Assert.Equal(TTEntryType.Exact, entry.Type);
            Assert.Equal(100, entry.Score);
        }

        [Fact]
        public void TestDepthReplacement()
        {
            ulong key = board.HashKey;
            Move move = new Move(12, 28, Piece.WhitePawn);

            // Store shallow search
            tt.Store(key, move, 100, 2, TTEntryType.Exact, Color.White);
            bool found = tt.Probe(key, out TTEntry entry);
            Assert.Equal(2, entry.Depth);
            Assert.Equal(100, entry.Score);

            // Store deeper search - should replace
            tt.Store(key, move, 200, 5, TTEntryType.Exact, Color.White);
            found = tt.Probe(key, out entry);
            Assert.Equal(5, entry.Depth);
            Assert.Equal(200, entry.Score);

            // Try to store shallower search - should still replace due to depth >= condition
            tt.Store(key, move, 300, 5, TTEntryType.Exact, Color.White);
            found = tt.Probe(key, out entry);
            Assert.Equal(5, entry.Depth);
            Assert.Equal(300, entry.Score);
        }

        [Fact]
        public void TestProbeScore()
        {
            ulong key = board.HashKey;
            Move move = new Move(12, 28, Piece.WhitePawn);

            // Test exact score
            tt.Store(key, move, 150, 5, TTEntryType.Exact, Color.White);
            int score = tt.ProbeScore(key, 3, -1000, 1000, Color.White); // depth <= stored depth
            Assert.Equal(150, score);

            // Test lower bound within window
            tt.Store(key, move, 200, 5, TTEntryType.LowerBound, Color.White);
            score = tt.ProbeScore(key, 3, -1000, 100, Color.White); // score >= beta (100)
            Assert.Equal(200, score);

            // Test lower bound outside window
            score = tt.ProbeScore(key, 3, -1000, 300, Color.White); // score < beta (300)
            Assert.Equal(int.MinValue, score);

            // Test upper bound within window
            tt.Store(key, move, 50, 5, TTEntryType.UpperBound, Color.White);
            score = tt.ProbeScore(key, 3, 100, 1000, Color.White); // score <= alpha (100)
            Assert.Equal(50, score);

            // Test upper bound outside window
            score = tt.ProbeScore(key, 3, 25, 1000, Color.White); // score > alpha (25)
            Assert.Equal(int.MinValue, score);
        }

        [Fact]
        public void TestMateScores()
        {
            ulong key = board.HashKey;
            Move move = new Move(12, 28, Piece.WhitePawn);

            // Test mate in 3 (stored at depth 6, retrieved at depth 4)
            int mateScore = 29997; // 30000 - 3
            tt.Store(key, move, mateScore, 6, TTEntryType.Exact, Color.White);
            
            int retrievedScore = tt.ProbeScore(key, 4, -30000, 30000, Color.White);
            int expectedScore = mateScore - (6 - 4); // Adjust for depth difference
            Assert.Equal(expectedScore, retrievedScore);

            // Test negative mate score
            int mateLossScore = -29995; // -30000 + 5
            tt.Store(key, move, mateLossScore, 8, TTEntryType.Exact, Color.White);
            
            retrievedScore = tt.ProbeScore(key, 6, -30000, 30000, Color.White);
            expectedScore = mateLossScore + (8 - 6); // Adjust for depth difference
            Assert.Equal(expectedScore, retrievedScore);
        }

        [Fact]
        public void TestProbeMove()
        {
            ulong key = board.HashKey;
            Move expectedMove = new Move(12, 28, Piece.WhitePawn); // e2-e4

            // Store move
            tt.Store(key, expectedMove, 100, 5, TTEntryType.Exact, Color.White);
            
            // Retrieve move
            bool found = tt.ProbeMove(key, out Move retrievedMove);
            Assert.True(found);
            Assert.Equal(expectedMove.From, retrievedMove.From);
            Assert.Equal(expectedMove.To, retrievedMove.To);
            Assert.Equal(expectedMove.MovedPiece, retrievedMove.MovedPiece);

            // Test non-existent key
            found = tt.ProbeMove(0x1234567890ABCDEF, out retrievedMove);
            Assert.False(found);
        }

        [Fact]
        public void TestStatistics()
        {
            ulong key1 = 0x1111111111111111;
            ulong key2 = 0x2222222222222222;
            Move move = new Move(12, 28, Piece.WhitePawn);

            long initialHits = tt.Hits;
            long initialMisses = tt.Misses;
            
            // Store some entries
            tt.Store(key1, move, 100, 5, TTEntryType.Exact, Color.White);
            tt.Store(key2, move, -50, 3, TTEntryType.LowerBound, Color.White);

            // Try some probes
            tt.Probe(key1, out _); // Hit
            tt.Probe(key2, out _); // Hit
            tt.Probe(0x3333333333333333, out _); // Miss

            Assert.Equal(initialHits + 2, tt.Hits);
            Assert.Equal(initialMisses + 1, tt.Misses);
        }

        [Fact]
        public void TestClear()
        {
            ulong key = board.HashKey;
            Move move = new Move(12, 28, Piece.WhitePawn);

            // Store an entry
            tt.Store(key, move, 100, 5, TTEntryType.Exact, Color.White);
            bool found = tt.Probe(key, out _);
            Assert.True(found, "Entry should exist before clear");

            // Clear and verify
            tt.Clear();
            found = tt.Probe(key, out _);
            Assert.False(found, "Entry should not exist after clear");

            Assert.Equal(0, tt.Hits);
            Assert.Equal(0, tt.Misses);
            Assert.Equal(0, tt.Collisions);
        }

        [Fact]
        public void TestNewSearch()
        {
            ulong key = board.HashKey;
            Move move = new Move(12, 28, Piece.WhitePawn);

            // Store entry in first search
            tt.Store(key, move, 100, 5, TTEntryType.Exact, Color.White);
            tt.Probe(key, out TTEntry entry1);
            byte age1 = entry1.Age;

            // Start new search
            tt.NewSearch();

            // Store another entry - should have different age
            tt.Store(key, move, 200, 5, TTEntryType.Exact, Color.White);
            tt.Probe(key, out TTEntry entry2);
            byte age2 = entry2.Age;

            Assert.NotEqual(age1, age2);
        }

        [Fact]
        public void TestInvalidDepth()
        {
            ulong key = board.HashKey;
            Move move = new Move(12, 28, Piece.WhitePawn);

            // Try to store with negative depth
            tt.Store(key, move, 100, -1, TTEntryType.Exact, Color.White);
            
            // Should not be stored
            bool found = tt.Probe(key, out _);
            Assert.False(found, "Entry with negative depth should not be stored");
        }

        [Fact]
        public void TestHitRate()
        {
            tt.Clear(); // Start fresh
            
            ulong key = board.HashKey;
            Move move = new Move(12, 28, Piece.WhitePawn);
            
            // Initial hit rate should be 0
            Assert.Equal(0.0, tt.GetHitRate());
            
            // Store and hit
            tt.Store(key, move, 100, 5, TTEntryType.Exact, Color.White);
            tt.Probe(key, out _); // Hit
            tt.Probe(0x1234567890ABCDEF, out _); // Miss
            
            // Hit rate should be 0.5 (1 hit, 1 miss)
            Assert.Equal(0.5, tt.GetHitRate(), 3);
        }
    }
}