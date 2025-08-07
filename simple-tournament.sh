#!/bin/bash

# Simple Chess Engine Tournament Script with Automated Building
# Automatically builds baseline (previous commit) and candidate (current state) versions
# Usage: ./simple-tournament.sh [games] [time_control] [concurrency]

GAMES=${1:-100}
TIME_CONTROL=${2:-"10+0.1"}
CONCURRENCY=${3:-1}

BASELINE_DIR="baseline"
CANDIDATE_DIR="candidate"
PROJECT_NAME="ChessEngine"

echo "=== Chess Engine Tournament ==="
echo "Games: $GAMES"
echo "Time Control: $TIME_CONTROL"
echo "Concurrency: $CONCURRENCY"
echo ""

# Check prerequisites
echo "Checking prerequisites..."
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found"
    exit 1
fi

if ! command -v fastchess &> /dev/null; then
    echo "ERROR: fastchess not found"
    exit 1
fi

if ! git rev-parse --git-dir > /dev/null 2>&1; then
    echo "ERROR: Not in a git repository"
    exit 1
fi

echo "✓ Prerequisites checked"
echo ""

# Build baseline and candidate versions
echo "Building engines..."

# Get current branch
CURRENT_BRANCH=$(git branch --show-current)
echo "Current branch: $CURRENT_BRANCH"

# Stash all changes including untracked files for candidate
echo "Stashing current changes for candidate..."
git add -A  # Add all untracked files to index
STASH_MESSAGE="Tournament candidate state $(date +%Y%m%d_%H%M%S)"
if git stash push -m "$STASH_MESSAGE"; then
    echo "✓ Changes stashed"
else
    echo "✓ No changes to stash"
fi

# Build baseline from previous commit
echo "Building baseline from HEAD^..."

git checkout HEAD^ > /dev/null 2>&1

rm -rf "$BASELINE_DIR"
mkdir -p "$BASELINE_DIR"

if dotnet build "$PROJECT_NAME" -c Release > build_baseline.log 2>&1; then
    if dotnet publish "$PROJECT_NAME" -c Release --self-contained -r linux-x64 -o "$BASELINE_DIR/linux-x64" >> build_baseline.log 2>&1; then
        echo "✓ Baseline built successfully"
    else
        echo "✗ Baseline publish failed - check build_baseline.log"
        git checkout "$CURRENT_BRANCH" > /dev/null 2>&1
        git stash pop > /dev/null 2>&1 || true
        exit 1
    fi
else
    echo "✗ Baseline build failed - check build_baseline.log"
    git checkout "$CURRENT_BRANCH" > /dev/null 2>&1
    git stash pop > /dev/null 2>&1 || true
    exit 1
fi

# Return to current branch and restore changes
echo "Restoring candidate state..."
git checkout "$CURRENT_BRANCH" > /dev/null 2>&1

if git stash list | grep -q "$STASH_MESSAGE"; then
    git stash pop > /dev/null 2>&1
    echo "✓ Candidate state restored"
else
    echo "✓ No stash to restore"
fi

# Build candidate from current state
echo "Building candidate from current state..."
rm -rf "$CANDIDATE_DIR"
mkdir -p "$CANDIDATE_DIR"

if dotnet build "$PROJECT_NAME" -c Release > build_candidate.log 2>&1; then
    if dotnet publish "$PROJECT_NAME" -c Release --self-contained -r linux-x64 -o "$CANDIDATE_DIR/linux-x64" >> build_candidate.log 2>&1; then
        echo "✓ Candidate built successfully"
    else
        echo "✗ Candidate publish failed - check build_candidate.log"
        exit 1
    fi
else
    echo "✗ Candidate build failed - check build_candidate.log"
    exit 1
fi

echo ""

# Verify engines
BASELINE_ENGINE="$BASELINE_DIR/linux-x64/$PROJECT_NAME"
CANDIDATE_ENGINE="$CANDIDATE_DIR/linux-x64/$PROJECT_NAME"

echo "Verifying engines..."

for ENGINE_NAME in "Baseline" "Candidate"; do
    if [ "$ENGINE_NAME" = "Baseline" ]; then
        ENGINE_PATH="$BASELINE_ENGINE"
    else
        ENGINE_PATH="$CANDIDATE_ENGINE"
    fi
    
    if [ ! -f "$ENGINE_PATH" ]; then
        echo "✗ $ENGINE_NAME engine not found: $ENGINE_PATH"
        exit 1
    fi
    
    if [ ! -x "$ENGINE_PATH" ]; then
        echo "✗ $ENGINE_NAME engine not executable: $ENGINE_PATH"
        exit 1
    fi
    
    # Quick UCI test
    if echo -e "uci\nquit" | timeout 5 "$ENGINE_PATH" | grep -q "uciok"; then
        echo "✓ $ENGINE_NAME UCI compliance verified"
    else
        echo "✗ $ENGINE_NAME failed UCI test"
        exit 1
    fi
done

echo ""

# Create tournaments directory
mkdir -p tournaments

# Generate timestamp for unique filenames
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
PGN_FILE="tournaments/tournament_${TIMESTAMP}.pgn"
RESULTS_FILE="tournaments/results_${TIMESTAMP}.txt"

echo "Starting tournament..."
echo "PGN Output: $PGN_FILE"
echo "Results Output: $RESULTS_FILE"
echo ""

# Calculate rounds (fastchess plays 2 games per round)
ROUNDS=$((GAMES / 2))

echo "Command: fastchess -engine cmd=$BASELINE_ENGINE name=Baseline -engine cmd=$CANDIDATE_ENGINE name=Candidate -each tc=$TIME_CONTROL proto=uci -rounds $ROUNDS -games 2 -concurrency $CONCURRENCY -pgnout file=$PGN_FILE min=true -repeat"
echo ""

# Run the tournament and save output
fastchess \
    -engine cmd="$BASELINE_ENGINE" name=Baseline \
    -engine cmd="$CANDIDATE_ENGINE" name=Candidate \
    -each tc="$TIME_CONTROL" proto=uci \
    -rounds "$ROUNDS" \
    -games 2 \
    -concurrency "$CONCURRENCY" \
    -pgnout file="$PGN_FILE" min=true \
    -openings file=8moves_v3.pgn format=pgn order=random \
    -repeat \
    2>&1 | tee "$RESULTS_FILE"

EXIT_CODE=${PIPESTATUS[0]}

echo ""
echo "=== Tournament Complete ==="
echo "Exit Code: $EXIT_CODE"
echo "Results saved to: $RESULTS_FILE"
echo "Games saved to: $PGN_FILE"

if [ $EXIT_CODE -eq 0 ]; then
    echo ""
    echo "=== Final Results ==="
    # Extract and display the key results
    grep -A 10 "Results of" "$RESULTS_FILE" | head -10
    echo ""
    echo "For detailed analysis, check: $RESULTS_FILE"
else
    echo "Tournament failed with exit code: $EXIT_CODE"
    exit $EXIT_CODE
fi