namespace ChessEngine.Tests
{
    public class SearchEngineIntegrationTests
    {
        private Board board;
        private SearchEngine searchEngine;

        public SearchEngineIntegrationTests()
        {
            board = new Board();
            board.SetupStartingPosition();
            searchEngine = new SearchEngine(board);
        }

        [Fact]
        public void TestTTIntegrationWithSearch()
        {
            // First search should populate TT
            var result1 = searchEngine.Search(3, TimeSpan.FromSeconds(10));
            
            // Second identical search should be faster (hit TT)
            var result2 = searchEngine.Search(3, TimeSpan.FromSeconds(10));
            
            // Results should be identical
            Assert.Equal(result1.BestMove.From, result2.BestMove.From);
            Assert.Equal(result1.BestMove.To, result2.BestMove.To);
            Assert.Equal(result1.Score, result2.Score);
        }

        [Fact]
        public void TestTTWithDifferentDepths()
        {
            // Search at depth 2
            var shallow = searchEngine.Search(2, TimeSpan.FromSeconds(5));
            
            // Search at depth 4 should use and extend TT information
            var deep = searchEngine.Search(4, TimeSpan.FromSeconds(10));
            
            // Should get valid results from both
            Assert.NotEqual(0, shallow.BestMove.From);
            Assert.NotEqual(0, deep.BestMove.From);
            
            // Deeper search should examine more nodes
            Assert.True(deep.NodesSearched >= shallow.NodesSearched);
        }

        [Fact]
        public void TestTTAfterMakeUnmakeMove()
        {
            ulong initialHash = board.HashKey;
            
            // Search initial position
            var result1 = searchEngine.Search(3, TimeSpan.FromSeconds(5));
            
            // Make a move
            Move move = new Move(12, 28, Piece.WhitePawn); // e2-e4
            board.MakeMove(move);
            
            // Search new position
            var result2 = searchEngine.Search(3, TimeSpan.FromSeconds(5));
            
            // Unmake move
            board.UnmakeMove(move);
            
            // Verify hash is restored
            Assert.Equal(initialHash, board.HashKey);
            
            // Search original position again - should still work
            var result3 = searchEngine.Search(3, TimeSpan.FromSeconds(5));
            
            // Should get same result as first search
            Assert.Equal(result1.BestMove.From, result3.BestMove.From);
            Assert.Equal(result1.BestMove.To, result3.BestMove.To);
        }

        [Fact]
        public void TestTTWithComplexPosition()
        {
            // Make some moves to get a more complex position
            board.MakeMove(new Move(12, 28, Piece.WhitePawn)); // e2-e4
            board.MakeMove(new Move(52, 36, Piece.BlackPawn)); // e7-e5
            board.MakeMove(new Move(6, 21, Piece.WhiteKnight)); // g1-f3
            board.MakeMove(new Move(57, 42, Piece.BlackKnight)); // b8-c6
            
            // Search should find the best move and store in TT
            var result = searchEngine.Search(4, TimeSpan.FromSeconds(5));
            
            // Result should be valid
            Assert.NotEqual(0, result.BestMove.From);
            Assert.NotEqual(0, result.BestMove.To);
            Assert.True(result.NodesSearched > 0);
        }

        [Fact]
        public void TestTTStatistics()
        {
            // Search multiple times to generate TT activity
            for (int i = 0; i < 3; i++)
            {
                searchEngine.Search(3, TimeSpan.FromSeconds(1));
                
                // Make a small move to change position slightly
                if (i < 2)
                {
                    board.MakeMove(new Move(12, 28, Piece.WhitePawn)); // e2-e4
                    board.UnmakeMove(new Move(12, 28, Piece.WhitePawn)); // undo
                }
            }
            
            // TT should have some activity by now
            // This is more of an integration verification
            Assert.True(true); // Basic test to ensure no crashes
        }

        [Fact]
        public void TestTTWithTranspositions()
        {
            // Create a transposition: different move orders leading to same position
            
            // Path 1: e4, e5, Nf3, Nc6
            board.MakeMove(new Move(12, 28, Piece.WhitePawn)); // e2-e4
            board.MakeMove(new Move(52, 36, Piece.BlackPawn)); // e7-e5
            board.MakeMove(new Move(6, 21, Piece.WhiteKnight)); // g1-f3
            board.MakeMove(new Move(57, 42, Piece.BlackKnight)); // b8-c6
            
            ulong hash1 = board.HashKey;
            var result1 = searchEngine.Search(2, TimeSpan.FromSeconds(2));
            
            // Undo all moves
            board.UnmakeMove(new Move(57, 42, Piece.BlackKnight));
            board.UnmakeMove(new Move(6, 21, Piece.WhiteKnight));
            board.UnmakeMove(new Move(52, 36, Piece.BlackPawn));
            board.UnmakeMove(new Move(12, 28, Piece.WhitePawn));
            
            // Path 2: Nf3, Nc6, e4, e5 (same position, different move order)
            board.MakeMove(new Move(6, 21, Piece.WhiteKnight)); // g1-f3
            board.MakeMove(new Move(57, 42, Piece.BlackKnight)); // b8-c6
            board.MakeMove(new Move(12, 28, Piece.WhitePawn)); // e2-e4
            board.MakeMove(new Move(52, 36, Piece.BlackPawn)); // e7-e5
            
            ulong hash2 = board.HashKey;
            var result2 = searchEngine.Search(2, TimeSpan.FromSeconds(2));
            
            // Should reach the same position
            Assert.Equal(hash1, hash2);
            
            // Results might differ due to TT state, but both should be valid
            Assert.NotEqual(0, result1.BestMove.From);
            Assert.NotEqual(0, result2.BestMove.From);
        }

        [Fact]
        public void TestSearchDepthProgression()
        {
            // Test that deeper searches generally find better or equal moves
            var depth1 = searchEngine.Search(1, TimeSpan.FromSeconds(1));
            var depth2 = searchEngine.Search(2, TimeSpan.FromSeconds(2));
            var depth3 = searchEngine.Search(3, TimeSpan.FromSeconds(3));
            
            // All should return valid moves
            Assert.NotEqual(0, depth1.BestMove.From);
            Assert.NotEqual(0, depth2.BestMove.From);
            Assert.NotEqual(0, depth3.BestMove.From);
            
            // Node counts should generally increase with depth
            Assert.True(depth2.NodesSearched >= depth1.NodesSearched);
            Assert.True(depth3.NodesSearched >= depth2.NodesSearched);
        }

        [Fact]
        public void TestSearchTimeLimit()
        {
            // Test that search respects time limits
            var shortSearch = searchEngine.Search(10, TimeSpan.FromMilliseconds(100));
            
            // Should complete quickly due to time limit
            Assert.True(shortSearch.SearchTime <= TimeSpan.FromSeconds(1));
            Assert.NotEqual(0, shortSearch.BestMove.From);
        }
    }
}