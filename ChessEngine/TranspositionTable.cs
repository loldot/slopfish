namespace ChessEngine
{
    public enum TTEntryType : byte
    {
        Exact = 0,    // PV node - exact score
        LowerBound = 1, // Alpha cutoff - score is at least this value
        UpperBound = 2  // Beta cutoff - score is at most this value
    }

    public struct TTEntry
    {
        public ulong Key;           // Zobrist hash key
        public Move BestMove;       // Best move found
        public int Score;           // Evaluation score
        public byte Depth;          // Search depth
        public TTEntryType Type;    // Entry type (exact, lower bound, upper bound)
        public byte Age;            // Search age for replacement scheme
        public Color SideToMove;    // Side to move when score was calculated

        public TTEntry(ulong key, Move bestMove, int score, byte depth, TTEntryType type, byte age, Color sideToMove)
        {
            Key = key;
            BestMove = bestMove;
            Score = score;
            Depth = depth;
            Type = type;
            Age = age;
            SideToMove = sideToMove;
        }
    }

    public class TranspositionTable
    {
        private readonly TTEntry[] table;
        private readonly int sizeMask;
        private byte currentAge;
        
        // Statistics for debugging
        public long Hits { get; private set; }
        public long Misses { get; private set; }
        public long Collisions { get; private set; }

        public TranspositionTable(int sizeInMB = 64)
        {
            // Calculate size in entries (each entry is ~24 bytes)
            int entrySize = System.Runtime.InteropServices.Marshal.SizeOf<TTEntry>();
            int totalEntries = (sizeInMB * 1024 * 1024) / entrySize;
            
            // Round down to nearest power of 2 for efficient indexing
            int size = 1;
            while (size * 2 <= totalEntries)
                size *= 2;
            
            table = new TTEntry[size];
            sizeMask = size - 1;
            currentAge = 0;
            
            Console.WriteLine($"Transposition table initialized: {size:N0} entries ({(size * entrySize) / 1024 / 1024}MB)");
        }

        public void NewSearch()
        {
            currentAge++;
            if (currentAge == 0) // Overflow protection
                currentAge = 1;
        }

        public void Store(ulong key, Move bestMove, int score, int depth, TTEntryType type, Color sideToMove)
        {
            if (depth < 0) return; // Don't store invalid depths

            int index = (int)(key & (ulong)sizeMask);
            ref TTEntry entry = ref table[index];

            // Always replace if:
            // 1. Empty slot
            // 2. Same position (key match)
            // 3. Deeper search
            // 4. Much older entry
            bool shouldReplace = entry.Key == 0 || 
                               entry.Key == key ||
                               depth >= entry.Depth ||
                               (currentAge - entry.Age) > 4;

            if (shouldReplace)
            {
                if (entry.Key != 0 && entry.Key != key)
                    Collisions++;
                    
                entry = new TTEntry(key, bestMove, score, (byte)depth, type, currentAge, sideToMove);
            }
        }

        public bool Probe(ulong key, out TTEntry entry)
        {
            int index = (int)(key & (ulong)sizeMask);
            entry = table[index];

            if (entry.Key == key)
            {
                Hits++;
                // Update age to mark as recently used
                entry.Age = currentAge;
                table[index] = entry;
                return true;
            }

            Misses++;
            return false;
        }

        public bool ProbeMove(ulong key, out Move hashMove)
        {
            hashMove = default;
            
            if (Probe(key, out TTEntry entry))
            {
                hashMove = entry.BestMove;
                return true;
            }
            
            return false;
        }

        public int ProbeScore(ulong key, int depth, int alpha, int beta, Color currentSideToMove)
        {
            if (!Probe(key, out TTEntry entry))
                return int.MinValue; // No entry found

            if (entry.Depth >= depth)
            {
                int score = entry.Score;
                
                // Negate score if it was calculated from the opposite side's perspective
                if (entry.SideToMove != currentSideToMove)
                {
                    score = -score;
                }
                
                // Adjust mate scores based on current ply
                if (score > 29000)
                    score -= (entry.Depth - depth);
                else if (score < -29000)
                    score += (entry.Depth - depth);

                switch (entry.Type)
                {
                    case TTEntryType.Exact:
                        return score;
                    
                    case TTEntryType.LowerBound:
                        if (score >= beta)
                            return score;
                        break;
                    
                    case TTEntryType.UpperBound:
                        if (score <= alpha)
                            return score;
                        break;
                }
            }

            return int.MinValue; // Entry not usable
        }

        public void Clear()
        {
            Array.Clear(table, 0, table.Length);
            Hits = 0;
            Misses = 0;
            Collisions = 0;
            currentAge = 0;
        }

        public double GetHitRate()
        {
            long total = Hits + Misses;
            return total == 0 ? 0.0 : (double)Hits / total;
        }

        public int GetUsage()
        {
            int used = 0;
            for (int i = 0; i < Math.Min(1000, table.Length); i++) // Sample first 1000 entries
            {
                if (table[i].Key != 0)
                    used++;
            }
            return (used * 100) / Math.Min(1000, table.Length);
        }

        public void PrintStats()
        {
            Console.WriteLine($"TT Stats: Hits={Hits:N0}, Misses={Misses:N0}, Hit Rate={GetHitRate():P1}, Usage~{GetUsage()}%");
        }
    }
}