using Xunit;
using Xunit.Abstractions;
using System.Linq;

namespace ChessEngine.Tests
{
    public class MoveUndoTests
    {
        private readonly ITestOutputHelper _output;

        public MoveUndoTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestBasicMoveUndo()
        {
            var board = new Board();
            board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            
            string originalFen = board.ToFen();
            _output.WriteLine($"Original position: {originalFen}");
            
            // Generate moves and test the first legal move
            var moves = board.GenerateLegalMoves();
            Assert.True(moves.Count > 0, "Should have legal moves in starting position");
            
            var testMove = moves[0]; // Take first move (should be a pawn move)
            _output.WriteLine($"Testing move: {testMove}");
            
            // Make the move
            board.MakeMove(testMove);
            string afterMoveFen = board.ToFen();
            _output.WriteLine($"After move: {afterMoveFen}");
            
            // Verify the position changed
            Assert.NotEqual(originalFen, afterMoveFen);
            
            // Undo the move
            board.UnmakeMove(testMove);
            string afterUndoFen = board.ToFen();
            _output.WriteLine($"After undo: {afterUndoFen}");
            
            // Verify we're back to the original position
            Assert.Equal(originalFen, afterUndoFen);
        }

        [Fact]
        public void TestCaptureUndo()
        {
            // Position with possible captures
            var board = new Board();
            board.LoadFromFen("rnbqkb1r/ppp2ppp/4pn2/3p4/2PP4/8/PP2PPPP/RNBQKBNR w KQkq d6 0 4");
            
            string originalFen = board.ToFen();
            _output.WriteLine($"Original position: {originalFen}");
            
            // Find a capture move (cxd5)
            var moves = board.GenerateLegalMoves();
            var captureMove = moves.FirstOrDefault(m => m.IsCapture);
            
            if (captureMove.From != 0) // Check if we found a capture
            {
                _output.WriteLine($"Testing capture: {captureMove}");
                
                board.MakeMove(captureMove);
                string afterMoveFen = board.ToFen();
                _output.WriteLine($"After capture: {afterMoveFen}");
                
                board.UnmakeMove(captureMove);
                string afterUndoFen = board.ToFen();
                _output.WriteLine($"After undo: {afterUndoFen}");
                
                Assert.Equal(originalFen, afterUndoFen);
            }
            else
            {
                _output.WriteLine("No capture moves found in test position");
            }
        }

        [Fact]
        public void TestEnPassantUndo()
        {
            // Set up en passant position
            var board = new Board();
            board.LoadFromFen("rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 3");
            
            string originalFen = board.ToFen();
            _output.WriteLine($"Original position: {originalFen}");
            
            var moves = board.GenerateLegalMoves();
            var enPassantMove = moves.FirstOrDefault(m => m.IsEnPassant);
            
            if (enPassantMove.From != 0)
            {
                _output.WriteLine($"Testing en passant: {enPassantMove}");
                
                board.MakeMove(enPassantMove);
                string afterMoveFen = board.ToFen();
                _output.WriteLine($"After en passant: {afterMoveFen}");
                
                board.UnmakeMove(enPassantMove);
                string afterUndoFen = board.ToFen();
                _output.WriteLine($"After undo: {afterUndoFen}");
                
                Assert.Equal(originalFen, afterUndoFen);
            }
            else
            {
                _output.WriteLine("No en passant moves found in test position");
            }
        }

        [Fact]
        public void TestCastlingUndo()
        {
            // Test both kingside and queenside castling
            var board = new Board();
            board.LoadFromFen("r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1");
            
            string originalFen = board.ToFen();
            _output.WriteLine($"Original position: {originalFen}");
            
            var moves = board.GenerateLegalMoves();
            var castlingMove = moves.FirstOrDefault(m => m.IsCastling);
            
            if (castlingMove.From != 0)
            {
                _output.WriteLine($"Testing castling: {castlingMove}");
                
                board.MakeMove(castlingMove);
                string afterMoveFen = board.ToFen();
                _output.WriteLine($"After castling: {afterMoveFen}");
                
                board.UnmakeMove(castlingMove);
                string afterUndoFen = board.ToFen();
                _output.WriteLine($"After undo: {afterUndoFen}");
                
                Assert.Equal(originalFen, afterUndoFen);
            }
            else
            {
                _output.WriteLine("No castling moves found in test position");
            }
        }

        [Fact]
        public void TestPromotionUndo()
        {
            // Pawn promotion position
            var board = new Board();
            board.LoadFromFen("8/P7/8/8/8/8/8/k6K w - - 0 1");
            
            string originalFen = board.ToFen();
            _output.WriteLine($"Original position: {originalFen}");
            
            var moves = board.GenerateLegalMoves();
            var promotionMove = moves.FirstOrDefault(m => m.IsPromotion);
            
            if (promotionMove.From != 0)
            {
                _output.WriteLine($"Testing promotion: {promotionMove}");
                
                board.MakeMove(promotionMove);
                string afterMoveFen = board.ToFen();
                _output.WriteLine($"After promotion: {afterMoveFen}");
                
                board.UnmakeMove(promotionMove);
                string afterUndoFen = board.ToFen();
                _output.WriteLine($"After undo: {afterUndoFen}");
                
                Assert.Equal(originalFen, afterUndoFen);
            }
            else
            {
                _output.WriteLine("No promotion moves found in test position");
            }
        }

        [Fact]
        public void TestMultipleMoveUndo()
        {
            var board = new Board();
            board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            
            string originalFen = board.ToFen();
            _output.WriteLine($"Original position: {originalFen}");
            
            var moveSequence = new List<Move>();
            
            // Make several moves
            for (int i = 0; i < 5; i++)
            {
                var moves = board.GenerateLegalMoves();
                if (moves.Count == 0) break;
                
                var move = moves[0]; // Take first legal move
                moveSequence.Add(move);
                _output.WriteLine($"Move {i + 1}: {move}");
                board.MakeMove(move);
            }
            
            _output.WriteLine($"After {moveSequence.Count} moves: {board.ToFen()}");
            
            // Undo all moves in reverse order
            for (int i = moveSequence.Count - 1; i >= 0; i--)
            {
                _output.WriteLine($"Undoing move {i + 1}: {moveSequence[i]}");
                board.UnmakeMove(moveSequence[i]);
            }
            
            string finalFen = board.ToFen();
            _output.WriteLine($"After undoing all moves: {finalFen}");
            
            Assert.Equal(originalFen, finalFen);
        }

        [Fact]
        public void TestGameStatePreservation()
        {
            var board = new Board();
            board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            
            // Store original game state
            var originalSideToMove = board.SideToMove;
            var originalCastlingRights = (board.WhiteCanCastleKingside, board.WhiteCanCastleQueenside, 
                                        board.BlackCanCastleKingside, board.BlackCanCastleQueenside);
            var originalEnPassant = board.EnPassantSquare;
            var originalHalfMove = board.HalfMoveClock;
            var originalFullMove = board.FullMoveNumber;
            
            var moves = board.GenerateLegalMoves();
            var testMove = moves[0];
            
            // Make and undo move
            board.MakeMove(testMove);
            board.UnmakeMove(testMove);
            
            // Verify all game state is restored
            Assert.Equal(originalSideToMove, board.SideToMove);
            Assert.Equal(originalCastlingRights.Item1, board.WhiteCanCastleKingside);
            Assert.Equal(originalCastlingRights.Item2, board.WhiteCanCastleQueenside);
            Assert.Equal(originalCastlingRights.Item3, board.BlackCanCastleKingside);
            Assert.Equal(originalCastlingRights.Item4, board.BlackCanCastleQueenside);
            Assert.Equal(originalEnPassant, board.EnPassantSquare);
            Assert.Equal(originalHalfMove, board.HalfMoveClock);
            Assert.Equal(originalFullMove, board.FullMoveNumber);
            
            _output.WriteLine("All game state correctly preserved after make/undo");
        }

        [Fact]
        public void TestSearchConsistencyWithMoveUndo()
        {
            // Test that search results are consistent when using make/undo moves
            var board = new Board();
            board.LoadFromFen("rnbqkb1r/pppp1ppp/4pn2/8/3PP3/8/PPP2PPP/RNBQKBNR w KQkq - 0 4");
            
            string originalFen = board.ToFen();
            
            // Run search twice and compare results
            var searchEngine1 = new SearchEngine(board);
            var result1 = searchEngine1.Search(3, TimeSpan.FromSeconds(2));
            
            // Position should be unchanged after search
            Assert.Equal(originalFen, board.ToFen());
            
            var searchEngine2 = new SearchEngine(board);
            var result2 = searchEngine2.Search(3, TimeSpan.FromSeconds(2));
            
            // Results should be identical
            Assert.Equal(result1.BestMove.From, result2.BestMove.From);
            Assert.Equal(result1.BestMove.To, result2.BestMove.To);
            Assert.Equal(result1.Score, result2.Score);
            
            _output.WriteLine($"Search 1: {result1.BestMove}, Score: {result1.Score}");
            _output.WriteLine($"Search 2: {result2.BestMove}, Score: {result2.Score}");
            _output.WriteLine("Search results are consistent");
        }
    }
}
