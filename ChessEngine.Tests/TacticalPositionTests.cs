using Xunit;
using Xunit.Abstractions;

namespace ChessEngine.Tests
{
    public class TacticalPositionTests
    {
        private readonly ITestOutputHelper _output;

        public TacticalPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestProblematicTacticalPosition()
        {
            // From the user's problematic game:
            // Position after moves: 1. e4 e5 2. Nf3 Nc6 3. Bc4 Nf6 4. Ng5 d5 5. exd5 Na5 6. Bb5+ c6 7. dxc6 bxc6 8. Be2 h6 9. Nf3 Be7 10. O-O O-O 11. d3 Rb8 12. Re1 Rb2
            
            var board = new Board();
            board.LoadFromFen("r1bq1rk1/p3bpp1/2p2n1p/n3p3/8/3P1N2/rPP1BPPP/RNBQR1K1 w - - 0 13");
            
            _output.WriteLine($"Testing position: {board.ToFen()}");
            
            // Test static evaluation consistency
            int staticEval = Evaluator.Evaluate(board);
            _output.WriteLine($"Static evaluation: {staticEval}");
            
            // Test search at depth 3
            var searchEngine = new SearchEngine(board);
            var result = searchEngine.Search(3, TimeSpan.FromSeconds(5));
            
            _output.WriteLine($"Best move from search: {result.BestMove}");
            _output.WriteLine($"Search score: {result.Score}");
            
            // The search score should have the same sign as static evaluation for the side to move
            // If it's White's turn and static eval is positive, search should also be positive (or at least reasonable)
            
            // Test that we get a reasonable move
            Assert.NotEqual(default(Move), result.BestMove);
            
            // Test that the evaluation magnitude is reasonable (not wildly swinging)
            Assert.True(Math.Abs(result.Score) < 2000, $"Search score {result.Score} seems unreasonably large");
            
            _output.WriteLine("Tactical position test completed successfully");
        }

        [Fact]
        public void TestEarlyGamePosition()
        {
            // Test position from earlier in the problematic game
            var board = new Board();
            board.LoadFromFen("rnbqkb1r/pppp1ppp/5n2/4p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4");
            
            _output.WriteLine($"Testing early game position: {board.ToFen()}");
            
            int staticEval = Evaluator.Evaluate(board);
            _output.WriteLine($"Static evaluation: {staticEval}");
            
            var searchEngine = new SearchEngine(board);
            var result = searchEngine.Search(4, TimeSpan.FromSeconds(5));
            
            _output.WriteLine($"Best move from search: {result.BestMove}");
            _output.WriteLine($"Search score: {result.Score}");
            
            Assert.NotEqual(default(Move), result.BestMove);
            Assert.True(Math.Abs(result.Score) < 1000, $"Search score {result.Score} seems unreasonable for early game");
        }
    }
}
