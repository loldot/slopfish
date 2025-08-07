# Chess Engine ELO Improvement Roadmap

This document outlines key improvements to increase the chess engine's playing strength, ordered by estimated ELO impact.

## High Impact (100+ ELO each)

### 1. Transposition Table
- **Impact**: 100-200 ELO
- **Description**: Store previously evaluated positions to avoid re-searching
- **Implementation**:
  - Implement Zobrist hashing for position keys
  - Store exact, lower-bound, and upper-bound scores
  - Add hash move to move ordering
- **Priority**: Highest - should be implemented first

### 2. Killer Move Heuristic
- **Impact**: 100+ ELO
- **Description**: Track moves that caused cutoffs at each depth
- **Implementation**:
  - Store 2 killer moves per ply
  - Try killer moves early in move ordering (after hash move)
  - Reset killer moves when changing search depth

### 3. Principal Variation Search (PVS)
- **Impact**: 100+ ELO
- **Description**: Search first move with full window, others with null window
- **Implementation**:
  - Search first move normally
  - Search remaining moves with null window
  - Re-search with full window if null window fails high

### 4. Null Move Pruning
- **Impact**: 100+ ELO
- **Description**: Skip a move and search at reduced depth to prune branches
- **Implementation**:
  - Make null move (skip turn)
  - Search at depth - R - 1 (R = 2-3)
  - If score >= beta, return beta (cutoff)
  - Avoid in zugzwang positions

## Medium Impact (50-100 ELO each)

### 5. Late Move Reductions (LMR)
- **Impact**: 50-100 ELO
- **Description**: Reduce depth for moves searched late in move ordering
- **Implementation**:
  - Reduce depth for moves after first few
  - Re-search at full depth if they raise alpha
  - Don't reduce captures, checks, or promotions

### 6. Check Extensions
- **Impact**: 50-80 ELO
- **Description**: Extend search depth when in check
- **Implementation**:
  - Add 1 ply when position is in check
  - Prevents tactical oversights
  - Currently missing from quiescence search

### 7. Enhanced Move Ordering
- **Impact**: 60-80 ELO
- **Description**: Improve move ordering beyond current MVV-LVA
- **Implementation**:
  - Hash move first (from transposition table)
  - Killer moves second
  - History heuristic for quiet moves
  - Counter moves
  - MVV-LVA for captures (current implementation)

### 8. Futility Pruning
- **Impact**: 50-70 ELO
- **Description**: Skip moves unlikely to raise alpha in quiescence
- **Implementation**:
  - If stand-pat + margin + piece value < alpha, skip move
  - Only in quiescence search
  - Reduces search time significantly

## Lower Impact (20-50 ELO each)

### 9. Better Time Management
- **Impact**: 30-50 ELO
- **Description**: Intelligent time allocation based on position complexity
- **Implementation**:
  - Allocate more time for critical positions
  - Stop search when best move is stable
  - Consider remaining time in game

### 10. Improved Evaluation
- **Impact**: 40-60 ELO total
- **Description**: Enhanced position evaluation
- **Implementation**:
  - King-pawn endgame knowledge
  - Passed pawn evaluation bonuses
  - Piece coordination bonuses
  - Better endgame piece-square tables
  - Tempo bonus

### 11. Aspiration Windows
- **Impact**: 20-40 ELO
- **Description**: Search with narrow window around previous score
- **Implementation**:
  - Start with window around previous iteration score
  - Widen window and re-search if bounds exceeded
  - Modest but consistent improvement

### 12. Multi-threaded Search
- **Impact**: 20-80 ELO (hardware dependent)
- **Description**: Parallel search of different branches
- **Implementation**:
  - Lazy SMP (shared transposition table)
  - Or more sophisticated YBWC/DTS approaches
  - Gains depend on available CPU cores

## Implementation Priority

1. **Transposition Table** - Highest impact, foundational for other improvements
2. **Killer Moves** - High impact, relatively easy to implement
3. **Principal Variation Search** - Significant search improvement
4. **Null Move Pruning** - Major pruning technique
5. **Late Move Reductions** - Good search speedup
6. **Check Extensions** - Fix tactical blind spots
7. **Enhanced Move Ordering** - Gradual but important improvement
8. **Remaining improvements** - Implement based on time/complexity tradeoffs

## Current Status

✅ **Completed**: Alpha-beta search with quiescence, basic move ordering (MVV-LVA), piece-square tables
⏳ **In Progress**: None
📋 **Planned**: All items above

## Notes

- The transposition table alone could provide 100-200 ELO improvement
- Each improvement builds on the previous ones
- Testing each improvement individually is recommended
- Total potential improvement: 500+ ELO points