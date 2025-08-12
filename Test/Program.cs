using ChessEngine;

Console.WriteLine("Chess Engine Test - Repetition Detection");

var board = new Board();
board.SetupStartingPosition();

Console.WriteLine($"Initial position hash: {board.HashKey}");
Console.WriteLine($"Is repetition initially: {board.IsRepetition()}");

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
Console.WriteLine($"After move 1, hash: {board.HashKey}, Is repetition: {board.IsRepetition()}");

board.MakeMove(move2);
Console.WriteLine($"After move 2, hash: {board.HashKey}, Is repetition: {board.IsRepetition()}");

board.MakeMove(move3);
Console.WriteLine($"After move 3, hash: {board.HashKey}, Is repetition: {board.IsRepetition()}");

board.MakeMove(move4);
Console.WriteLine($"After move 4, hash: {board.HashKey}, Is repetition: {board.IsRepetition()}");

// Second cycle
board.MakeMove(move1);
Console.WriteLine($"After move 5, hash: {board.HashKey}, Is repetition: {board.IsRepetition()}");

board.MakeMove(move2);
Console.WriteLine($"After move 6, hash: {board.HashKey}, Is repetition: {board.IsRepetition()}");

board.MakeMove(move3);
Console.WriteLine($"After move 7, hash: {board.HashKey}, Is repetition: {board.IsRepetition()}");

board.MakeMove(move4);
Console.WriteLine($"After move 8, hash: {board.HashKey}, Is repetition: {board.IsRepetition()}");
Console.WriteLine($"Is threefold repetition: {board.IsThreefoldRepetition()}");
