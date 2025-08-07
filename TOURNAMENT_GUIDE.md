# OpenBench Testing Guide

This guide explains how to use OpenBench for measuring ELO improvements between engine versions. OpenBench is the industry standard for chess engine testing, using SPRT (Sequential Probability Ratio Test) for efficient and statistically rigorous evaluation.

## Why OpenBench?

OpenBench provides several advantages over traditional fixed-game testing:

- **SPRT Testing**: Stops early when statistical significance is reached
- **Distributed Computing**: Leverages multiple machines for faster testing
- **Statistical Rigor**: Provides confidence bounds and proper significance testing
- **Industry Standard**: Used by Stockfish, Leela, Ethereal, and other top engines
- **Efficient**: Typically requires fewer games than fixed-game testing

## Requirements

1. **OpenBench Account** - Sign up at http://chess.grantnet.us/
2. **Git Repository** - Host your engines in a Git repo (GitHub, GitLab, etc.)
3. **UCI-compliant engines** - Both baseline and candidate engines must support UCI protocol

## Setup

### 1. Install OpenBench Client (Optional)
```bash
pip install OpenBench
```

### 2. Create OpenBench Configuration
Create `openbench.conf`:
```ini
[SERVER]
server   = http://chess.grantnet.us/
username = your_username
password = your_password

[CLIENT]
threads = 1
```

## Basic Usage

```bash
# Prepare an SPRT test
python run-tournament.py --baseline old_engine.exe --candidate new_engine.exe

# Custom SPRT bounds and time control
python run-tournament.py \
    --baseline old_engine.exe \
    --candidate new_engine.exe \
    --sprt-bounds "[-1.0, 4.0]" \
    --time-control "60.0+1.0"

# Quick testing with tighter bounds
python run-tournament.py \
    --baseline old_engine.exe \
    --candidate new_engine.exe \
    --sprt-bounds "[-0.5, 2.5]" \
    --games 50000
```

## SPRT Bounds

SPRT bounds define the null and alternative hypotheses:

### Common Configurations
- `[-3.0, 1.0]` - **Standard**: Detect if improvement ≥ 1 ELO or regression ≤ -3 ELO
- `[-1.0, 4.0]` - **Sensitive**: For detecting larger improvements
- `[-0.5, 2.5]` - **Very sensitive**: For final validation of small improvements

### Expected Game Counts
For bounds `[-3.0, 1.0]`:
- **Pass** (if truly +1 ELO): 5,000-15,000 games
- **Fail** (if truly -3 ELO): 1,000-5,000 games  
- **Inconclusive**: Up to max games if ELO between bounds

## Time Controls

OpenBench uses decimal format:
- `"10.0+0.1"` - 10 seconds + 0.1 second increment (fast testing)
- `"60.0+1.0"` - 1 minute + 1 second increment (standard)
- `"300.0+3.0"` - 5 minutes + 3 second increment (longer games)

## Test Submission Workflow

### Method 1: Web Interface (Recommended)
1. **Upload engines** to Git repository
2. **Log into OpenBench** web interface
3. **Create new test** with your engine configurations
4. **Monitor progress** on dashboard
5. **Analyze results** when complete

### Method 2: Automated Preparation
1. **Run script** to generate test configuration:
   ```bash
   python run-tournament.py --baseline baseline.exe --candidate candidate.exe
   ```
2. **Review generated files**:
   - Test configuration JSON
   - Summary report 
   - Local fallback script
3. **Submit manually** via web interface

## Interpreting SPRT Results

### Test Outcomes
- **Pass**: Candidate statistically better than upper bound
- **Fail**: Candidate statistically worse than lower bound  
- **Inconclusive**: Performance between bounds (neither hypothesis proven)

### ELO Significance
- **±0-1 ELO**: Minimal practical difference
- **±1-3 ELO**: Small but measurable improvement
- **±3-10 ELO**: Significant improvement
- **±10+ ELO**: Major improvement

## Testing Strategy

### 1. Development Testing
```bash
# Quick feedback during development
python run-tournament.py \
    --baseline baseline.exe \
    --candidate candidate.exe \
    --sprt-bounds "[-5.0, 2.0]" \
    --time-control "5.0+0.05" \
    --games 10000
```

### 2. Standard Validation
```bash
# Balanced testing for most improvements
python run-tournament.py \
    --baseline baseline.exe \
    --candidate candidate.exe \
    --sprt-bounds "[-3.0, 1.0]" \
    --time-control "10.0+0.1" \
    --games 20000
```

### 3. Final Validation
```bash
# Rigorous testing for release candidates
python run-tournament.py \
    --baseline baseline.exe \
    --candidate candidate.exe \
    --sprt-bounds "[-1.0, 3.0]" \
    --time-control "60.0+1.0" \
    --games 50000
```

## Local Testing Fallback

If OpenBench is unavailable, the script generates a local testing script using cutechess-cli:

```bash
# Install cutechess-cli
sudo apt install cutechess-cli  # Ubuntu/Debian
brew install cutechess          # macOS

# Run generated local test
./tournaments/local_test_YYYYMMDD_HHMMSS.sh
```

Note: Local testing doesn't provide SPRT benefits but offers basic comparison.

## Opening Books

OpenBench typically uses these books:
- **UHO_XXL_+0.90_+1.19.epd** - Default, good variety
- **8moves_v3.pgn** - Popular opening positions
- **Perfect2021.pgn** - Comprehensive opening suite

## Output Files

The script generates:
- `openbench_submission_YYYYMMDD_HHMMSS.md` - Detailed report
- `test_config_YYYYMMDD_HHMMSS.json` - Test configuration  
- `local_test_YYYYMMDD_HHMMSS.sh` - Local fallback script

## Example Workflow

1. **Implement improvement** (e.g., transposition table)
2. **Compile both versions**:
   ```bash
   # Baseline
   git checkout baseline_commit
   dotnet build -c Release -o baseline/
   
   # Candidate  
   git checkout feature_branch
   dotnet build -c Release -o candidate/
   ```
3. **Prepare test**:
   ```bash
   python run-tournament.py \
       --baseline baseline/ChessEngine.exe \
       --candidate candidate/ChessEngine.exe
   ```
4. **Submit to OpenBench** via web interface
5. **Monitor results** and iterate

## OpenBench Resources

- **Main Instance**: http://chess.grantnet.us/
- **GitHub**: https://github.com/AndyGrant/OpenBench
- **Wiki**: https://github.com/AndyGrant/OpenBench/wiki
- **Discord**: https://discord.com/invite/9MVg7fBTpM

## Troubleshooting

### Common Issues
- **Engine not UCI compliant**: Test with `uci` command manually
- **Compilation errors**: Verify engines run independently  
- **Authentication fails**: Check OpenBench credentials
- **Test rejected**: Ensure engines are in accessible Git repository

### Getting Help
- **Discord community**: Active developers and users
- **GitHub issues**: For bugs and feature requests
- **Wiki documentation**: Comprehensive setup guides