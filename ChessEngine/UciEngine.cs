using System.Diagnostics;

namespace ChessEngine
{
    public class UciEngine
    {
        private Board board;
        private SearchEngine searchEngine;
        private bool isSearching;
        private CancellationTokenSource? searchCancellation;

        public string EngineName { get; set; } = "ChessEngine";
        public string EngineAuthor { get; set; } = "opencode";
        public string EngineVersion { get; set; } = "1.0";

        public UciEngine()
        {
            board = new Board();
            board.SetupStartingPosition();
            searchEngine = new SearchEngine(board);
            isSearching = false;
        }

        public void Run()
        {
            string? input;
            while ((input = Console.ReadLine()) != null)
            {
                ProcessCommand(input.Trim());
            }
        }

        private void ProcessCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return;

            string[] parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            string cmd = parts[0].ToLower();

            switch (cmd)
            {
                case "uci":
                    HandleUci();
                    break;
                case "isready":
                    HandleIsReady();
                    break;
                case "ucinewgame":
                    HandleNewGame();
                    break;
                case "position":
                    HandlePosition(parts);
                    break;
                case "go":
                    HandleGo(parts);
                    break;
                case "stop":
                    HandleStop();
                    break;
                case "quit":
                case "exit":
                    HandleQuit();
                    break;
                case "d":
                case "display":
                    HandleDisplay();
                    break;
                case "eval":
                    HandleEval();
                    break;
                default:
                    Console.WriteLine($"Unknown command: {cmd}");
                    break;
            }
        }

        private void HandleUci()
        {
            Console.WriteLine($"id name {EngineName} {EngineVersion}");
            Console.WriteLine($"id author {EngineAuthor}");
            Console.WriteLine("option name Hash type spin default 64 min 1 max 1024");
            Console.WriteLine("option name Threads type spin default 1 min 1 max 64");
            Console.WriteLine("uciok");
        }

        private void HandleIsReady()
        {
            Console.WriteLine("readyok");
        }

        private void HandleNewGame()
        {
            board.SetupStartingPosition();
            searchEngine = new SearchEngine(board);
        }

        private void HandlePosition(string[] parts)
        {
            if (parts.Length < 2)
                return;

            if (parts[1] == "startpos")
            {
                board.SetupStartingPosition();
                
                if (parts.Length > 2 && parts[2] == "moves")
                {
                    for (int i = 3; i < parts.Length; i++)
                    {
                        Move move = ParseMove(parts[i]);
                        if (!move.Equals(default(Move)))
                        {
                            board.MakeMove(move);
                        }
                    }
                }
            }
            else if (parts[1] == "fen")
            {
                if (parts.Length < 8)
                    return;

                string fen = string.Join(" ", parts, 2, 6);
                board.LoadFromFen(fen);

                int movesIndex = Array.IndexOf(parts, "moves");
                if (movesIndex != -1)
                {
                    for (int i = movesIndex + 1; i < parts.Length; i++)
                    {
                        Move move = ParseMove(parts[i]);
                        if (!move.Equals(default(Move)))
                        {
                            board.MakeMove(move);
                        }
                    }
                }
            }

            searchEngine = new SearchEngine(board);
        }

        private void HandleGo(string[] parts)
        {
            if (isSearching)
            {
                HandleStop();
            }

            int depth = 50; // Default to max depth for iterative deepening
            int moveTime = 5000;
            int whiteTime = 0;
            int blackTime = 0;
            int whiteInc = 0;
            int blackInc = 0;
            int movesToGo = 0;
            bool infinite = false;
            bool depthSpecified = false;

            for (int i = 1; i < parts.Length; i++)
            {
                switch (parts[i])
                {
                    case "depth":
                        if (i + 1 < parts.Length && int.TryParse(parts[i + 1], out int d))
                        {
                            depth = d;
                            depthSpecified = true;
                            i++;
                        }
                        break;
                    case "movetime":
                        if (i + 1 < parts.Length && int.TryParse(parts[i + 1], out int mt))
                        {
                            moveTime = mt;
                            i++;
                        }
                        break;
                    case "wtime":
                        if (i + 1 < parts.Length && int.TryParse(parts[i + 1], out int wt))
                        {
                            whiteTime = wt;
                            i++;
                        }
                        break;
                    case "btime":
                        if (i + 1 < parts.Length && int.TryParse(parts[i + 1], out int bt))
                        {
                            blackTime = bt;
                            i++;
                        }
                        break;
                    case "winc":
                        if (i + 1 < parts.Length && int.TryParse(parts[i + 1], out int wi))
                        {
                            whiteInc = wi;
                            i++;
                        }
                        break;
                    case "binc":
                        if (i + 1 < parts.Length && int.TryParse(parts[i + 1], out int bi))
                        {
                            blackInc = bi;
                            i++;
                        }
                        break;
                    case "movestogo":
                        if (i + 1 < parts.Length && int.TryParse(parts[i + 1], out int mtg))
                        {
                            movesToGo = mtg;
                            i++;
                        }
                        break;
                    case "infinite":
                        infinite = true;
                        break;
                }
            }

            // Only use time management if depth is not explicitly specified
            if (!depthSpecified && (whiteTime > 0 || blackTime > 0))
            {
                int timeLeft = board.SideToMove == Color.White ? whiteTime : blackTime;
                int increment = board.SideToMove == Color.White ? whiteInc : blackInc;
                
                if (movesToGo > 0)
                {
                    // Time control with moves to go
                    moveTime = (timeLeft / movesToGo) + increment;
                }
                else
                {
                    // Sudden death or increment time control
                    moveTime = (timeLeft / 30) + increment;
                }
                
                // Safety margins
                moveTime = Math.Max(moveTime, 100); // Minimum 100ms
                moveTime = Math.Min(moveTime, timeLeft - 100); // Leave 100ms safety margin
            }

            searchCancellation = new CancellationTokenSource();
            isSearching = true;

            Task.Run(() => PerformSearch(depth, moveTime, infinite, depthSpecified), searchCancellation.Token);
        }

        private void PerformSearch(int depth, int moveTime, bool infinite, bool depthSpecified)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                
                // Subscribe to depth completion events for UCI info output
                Action<int, int, long, long> depthHandler = (completedDepth, score, nodes, timeMs) =>
                {
                    if (searchCancellation?.Token.IsCancellationRequested != true)
                    {
                        // UCI scores should be from White's perspective
                        int uciScore = board.SideToMove == Color.White ? score : -score;
                        Console.WriteLine($"info depth {completedDepth} score cp {uciScore} nodes {nodes} time {timeMs}");
                    }
                };
                
                searchEngine.OnDepthCompleted += depthHandler;
                
                TimeSpan maxTime;
                if (infinite)
                {
                    maxTime = TimeSpan.MaxValue;
                }
                else if (depthSpecified)
                {
                    // When depth is specified, use a generous time limit but respect the depth limit
                    maxTime = TimeSpan.FromMinutes(10);
                }
                else
                {
                    // Time-based search
                    maxTime = TimeSpan.FromMilliseconds(moveTime);
                }
                
                SearchResult result = searchEngine.Search(depth, maxTime, searchCancellation?.Token ?? CancellationToken.None);
                
                stopwatch.Stop();

                if (searchCancellation != null && !searchCancellation.Token.IsCancellationRequested)
                {
                    if (!result.BestMove.Equals(default(Move)))
                    {
                        Console.WriteLine($"bestmove {result.BestMove}");
                    }
                    else
                    {
                        var legalMoves = board.GenerateLegalMoves();
                        if (legalMoves.Count > 0)
                        {
                            Console.WriteLine($"bestmove {legalMoves[0]}");
                        }
                        else
                        {
                            Console.WriteLine("bestmove 0000");
                        }
                    }
                }
                
                // Unsubscribe from events
                searchEngine.OnDepthCompleted -= depthHandler;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                isSearching = false;
            }
        }

        private void HandleStop()
        {
            if (isSearching)
            {
                searchCancellation?.Cancel();
                searchEngine.StopSearch();
            }
        }

        private void HandleQuit()
        {
            HandleStop();
            Environment.Exit(0);
        }

        private void HandleDisplay()
        {
            board.PrintBoard();
            Console.WriteLine($"FEN: {board.ToFen()}");
            Console.WriteLine($"Legal moves: {board.GenerateLegalMoves().Count}");
        }

        private void HandleEval()
        {
            int eval = Evaluator.Evaluate(board);
            Console.WriteLine($"Evaluation: {eval} (from White's perspective)");
        }

        private Move ParseMove(string moveStr)
        {
            if (string.IsNullOrEmpty(moveStr) || moveStr.Length < 4)
                return default;

            List<Move> legalMoves = board.GenerateLegalMoves();
            
            foreach (Move move in legalMoves)
            {
                if (move.ToString() == moveStr)
                {
                    return move;
                }
            }

            return default;
        }
    }
}