using ChessEngine;

namespace ChessEngine.Tests;

public class PerftTests
{
    private Board board;

    public PerftTests()
    {
        board = new Board();
    }

    private long Perft(Board board, int depth)
    {
        if (depth == 0)
            return 1;

        var moves = board.GenerateLegalMoves();
        long nodes = 0;

        foreach (var move in moves)
        {
            board.MakeMove(move);
            nodes += Perft(board, depth - 1);
            board.UnmakeMove(move);
        }

        return nodes;
    }

    private long PerftDivide(Board board, int depth)
    {
        var moves = board.GenerateLegalMoves();
        long totalNodes = 0;

        foreach (var move in moves)
        {
            board.MakeMove(move);
            long nodes = Perft(board, depth - 1);
            totalNodes += nodes;
            board.UnmakeMove(move);
        }

        return totalNodes;
    }

    [Fact]
    public void Perft_StartingPosition_Depth1()
    {
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        
        long result = Perft(board, 1);
        
        Assert.Equal(20, result);
    }

    [Fact]
    public void Perft_StartingPosition_Depth2()
    {
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        
        long result = Perft(board, 2);
        
        Assert.Equal(400, result);
    }

    [Fact]
    public void Perft_StartingPosition_Depth3()
    {
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        
        long result = Perft(board, 3);
        
        Assert.Equal(8902, result);
    }

    [Fact]
    public void Perft_StartingPosition_Depth4()
    {
        board.LoadFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        
        long result = Perft(board, 4);
        
        Assert.Equal(197281, result);
    }

    [Fact]
    public void Perft_KiwiPete_Depth1()
    {
        board.LoadFromFen("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1");
        
        long result = Perft(board, 1);
        
        Assert.Equal(6, result);
    }

    [Fact]
    public void Perft_KiwiPete_Depth2()
    {
        board.LoadFromFen("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1");
        
        long result = Perft(board, 2);
        
        Assert.Equal(264, result);
    }

    [Fact]
    public void Perft_KiwiPete_Depth3()
    {
        board.LoadFromFen("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1");
        
        long result = Perft(board, 3);
        
        Assert.Equal(9467, result);
    }

    [Fact]
    public void Perft_Position3_Depth1()
    {
        board.LoadFromFen("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");
        
        long result = Perft(board, 1);
        
        Assert.Equal(14, result);
    }

    [Fact]
    public void Perft_Position3_Depth2()
    {
        board.LoadFromFen("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");
        
        long result = Perft(board, 2);
        
        Assert.Equal(191, result);
    }

    [Fact]
    public void Perft_Position3_Depth3()
    {
        board.LoadFromFen("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");
        
        long result = Perft(board, 3);
        
        Assert.Equal(2812, result);
    }

    [Fact]
    public void Perft_Position4_Depth1()
    {
        board.LoadFromFen("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1");
        
        long result = Perft(board, 1);
        
        Assert.Equal(6, result);
    }

    [Fact]
    public void Perft_Position4_Depth2()
    {
        board.LoadFromFen("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1");
        
        long result = Perft(board, 2);
        
        Assert.Equal(264, result);
    }

    [Fact]
    public void Perft_Position5_Depth1()
    {
        board.LoadFromFen("2K2r2/4P3/8/8/8/8/8/3k4 w - - 0 1");
        
        long result = Perft(board, 1);
        
        Assert.Equal(11, result);
    }

    [Fact]
    public void Perft_Position5_Depth2()
    {
        board.LoadFromFen("2K2r2/4P3/8/8/8/8/8/3k4 w - - 0 1");
        
        long result = Perft(board, 2);
        
        Assert.Equal(133, result);
    }

    [Fact]
    public void Perft_Position6_Depth1()
    {
        board.LoadFromFen("8/8/1P2K3/8/2n5/1q6/8/5k2 b - - 0 1");
        
        long result = Perft(board, 1);
        
        Assert.Equal(29, result);
    }

    [Fact]
    public void Perft_Position6_Depth2()
    {
        board.LoadFromFen("8/8/1P2K3/8/2n5/1q6/8/5k2 b - - 0 1");
        
        long result = Perft(board, 2);
        
        Assert.Equal(165, result);
    }
}