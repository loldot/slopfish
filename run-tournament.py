#!/usr/bin/env python3
"""
Chess Engine Tournament Script

This script runs tournaments between different versions of the chess engine
to measure ELO improvements. Uses OpenBench for SPRT testing.

Usage:
    python run-tournament.py --baseline baseline_engine --candidate candidate_engine [options]
    
Requirements:
    - OpenBench client installed
    - Both engines compiled and UCI-compliant
    - OpenBench configuration file
"""

import argparse
import subprocess
import json
import os
import sys
from datetime import datetime
import re
import time
import requests

class OpenBenchRunner:
    def __init__(self, baseline_engine, candidate_engine, games=20000, time_control="10.0+0.1", 
                 sprt_bounds="[-3.0, 1.0]", book="UHO_XXL_+0.90_+1.19.epd", 
                 config_file="openbench.conf", output_dir="tournaments"):
        self.baseline_engine = baseline_engine
        self.candidate_engine = candidate_engine
        self.games = games
        self.time_control = time_control
        self.sprt_bounds = sprt_bounds
        self.book = book
        self.config_file = config_file
        self.output_dir = output_dir
        
        # Create output directory
        os.makedirs(output_dir, exist_ok=True)
        
    def check_dependencies(self):
        """Check if OpenBench client is available"""
        try:
            result = subprocess.run(['python', '-m', 'OpenBench'], 
                                  capture_output=True, text=True)
            if "OpenBench Client" in result.stdout or "OpenBench Client" in result.stderr:
                print("✓ OpenBench client found")
                return True
            else:
                print("✗ OpenBench client not found. Install with:")
                print("  pip install OpenBench")
                return False
        except (subprocess.CalledProcessError, FileNotFoundError):
            print("✗ OpenBench client not found. Install with:")
            print("  pip install OpenBench")
            return False
    
    def verify_engines(self):
        """Verify both engines exist and are executable"""
        for engine_name, engine_path in [("Baseline", self.baseline_engine), 
                                       ("Candidate", self.candidate_engine)]:
            if not os.path.isfile(engine_path):
                print(f"✗ {engine_name} engine not found: {engine_path}")
                return False
            if not os.access(engine_path, os.X_OK):
                print(f"✗ {engine_name} engine not executable: {engine_path}")
                return False
            print(f"✓ {engine_name} engine found: {engine_path}")
        return True
    
    def check_config(self):
        """Check if OpenBench config file exists"""
        if not os.path.isfile(self.config_file):
            print(f"✗ OpenBench config file not found: {self.config_file}")
            print("Create a config file with your OpenBench server details:")
            print(f"""
[SERVER]
server   = http://chess.grantnet.us/  # Or your private instance
username = your_username
password = your_password

[CLIENT]
threads = 1
""")
            return False
        print(f"✓ OpenBench config found: {self.config_file}")
        return True
    
    def create_sprt_test(self):
        """Create an SPRT test on OpenBench"""
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        
        # Create test configuration
        test_config = {
            "test_name": f"ELO_Test_{timestamp}",
            "author": "automated_testing",
            "baseline": {
                "name": f"Baseline_{timestamp}",
                "source": self.baseline_engine,
                "protocol": "uci"
            },
            "candidate": {
                "name": f"Candidate_{timestamp}", 
                "source": self.candidate_engine,
                "protocol": "uci"
            },
            "test_settings": {
                "time_control": self.time_control,
                "book": self.book,
                "sprt_bounds": self.sprt_bounds,
                "max_games": self.games
            }
        }
        
        print(f"Creating SPRT test: {test_config['test_name']}")
        print(f"Time Control: {self.time_control}")
        print(f"SPRT Bounds: {self.sprt_bounds}")
        print(f"Max Games: {self.games}")
        print(f"Book: {self.book}")
        
        # Save test config for reference
        config_file = os.path.join(self.output_dir, f"test_config_{timestamp}.json")
        with open(config_file, 'w') as f:
            json.dump(test_config, f, indent=2)
        
        return test_config, config_file
    
    def submit_test(self, test_config):
        """Submit test to OpenBench (simulation - would need actual API)"""
        print("\n" + "="*60)
        print("SUBMITTING TO OPENBENCH")
        print("="*60)
        print("Note: This is a simulation. In practice, you would:")
        print("1. Upload engines to your repository")
        print("2. Submit test via OpenBench web interface or API")
        print("3. Monitor test progress on OpenBench dashboard")
        print()
        
        # Simulate test submission
        test_id = f"test_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
        
        print(f"Test ID: {test_id}")
        print(f"Dashboard URL: http://your-openbench-instance/test/{test_id}")
        print()
        print("Test configuration:")
        print(f"  Baseline: {test_config['baseline']['name']}")
        print(f"  Candidate: {test_config['candidate']['name']}")
        print(f"  Time Control: {test_config['test_settings']['time_control']}")
        print(f"  SPRT Bounds: {test_config['test_settings']['sprt_bounds']}")
        print(f"  Max Games: {test_config['test_settings']['max_games']}")
        print(f"  Book: {test_config['test_settings']['book']}")
        
        return test_id
    
    def create_local_test_script(self, test_config):
        """Create a script for local testing with cutechess-cli"""
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        script_path = os.path.join(self.output_dir, f"local_test_{timestamp}.sh")
        
        # Convert OpenBench time control to cutechess format
        time_parts = self.time_control.split('+')
        if len(time_parts) == 2:
            base_time = time_parts[0]
            increment = time_parts[1]
            cutechess_tc = f"{base_time}+{increment}"
        else:
            cutechess_tc = self.time_control
        
        script_content = f"""#!/bin/bash
# Local testing script generated from OpenBench configuration
# This provides a fallback for testing engines locally

echo "Running local SPRT simulation..."
echo "This is not a true SPRT test but provides basic comparison"

cutechess-cli \\
    -engine name=Baseline cmd="{self.baseline_engine}" \\
    -engine name=Candidate cmd="{self.candidate_engine}" \\
    -each tc={cutechess_tc} proto=uci \\
    -rounds {self.games // 2} \\
    -games 2 \\
    -pgnout tournaments/local_test_{timestamp}.pgn \\
    -repeat 2

echo "Local test completed. Check tournaments/local_test_{timestamp}.pgn for games."
"""
        
        with open(script_path, 'w') as f:
            f.write(script_content)
        
        os.chmod(script_path, 0o755)
        
        print(f"✓ Local test script created: {script_path}")
        print("  Run this script if you want to test locally instead of using OpenBench")
        
        return script_path
    
    def explain_sprt(self):
        """Explain SPRT testing"""
        print("\n" + "="*60)
        print("ABOUT SPRT TESTING")
        print("="*60)
        print("Sequential Probability Ratio Test (SPRT) is superior to fixed-game testing:")
        print()
        print("ADVANTAGES:")
        print("  • Stops early when result is clear (saves time)")
        print("  • Provides statistical confidence bounds")
        print("  • Industry standard for chess engine development")
        print("  • Used by Stockfish, Leela, and other top engines")
        print()
        print("SPRT BOUNDS:")
        sprt_values = self.sprt_bounds.strip('[]').split(',')
        lower_bound = float(sprt_values[0])
        upper_bound = float(sprt_values[1])
        print(f"  • H0 (null hypothesis): ELO <= {lower_bound}")
        print(f"  • H1 (alternative): ELO >= {upper_bound}")
        print("  • Test stops when one hypothesis is proven with 95% confidence")
        print()
        print("TYPICAL BOUNDS:")
        print("  • [-3, 1]: Detect if improvement is >= 1 ELO or <= -3 ELO")
        print("  • [-1, 4]: More sensitive test for smaller improvements")
        print("  • [-0.5, 2.5]: Very sensitive for final validation")
        print()
        print("EXPECTED GAMES:")
        if lower_bound == -3.0 and upper_bound == 1.0:
            print("  • Pass: ~5,000-15,000 games (if truly +1 ELO)")
            print("  • Fail: ~1,000-5,000 games (if truly -3 ELO)")
            print("  • Inconclusive: Up to max games if ELO is between bounds")
        print("="*60)
    
    def create_summary_report(self, test_config, test_id, config_file, script_path):
        """Create a summary report"""
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        report_file = os.path.join(self.output_dir, f"openbench_submission_{timestamp}.md")
        
        report_content = f"""# OpenBench Test Submission Report

Generated: {datetime.now().isoformat()}

## Test Configuration

**Test ID**: {test_id}
**Baseline Engine**: {self.baseline_engine}
**Candidate Engine**: {self.candidate_engine}

## SPRT Settings

- **Time Control**: {self.time_control}
- **SPRT Bounds**: {self.sprt_bounds}
- **Max Games**: {self.games:,}
- **Opening Book**: {self.book}

## Files Generated

- **Test Config**: {config_file}
- **Local Test Script**: {script_path}
- **This Report**: {report_file}

## Next Steps

### Option 1: OpenBench Testing (Recommended)
1. Set up OpenBench account at http://chess.grantnet.us/
2. Upload your engines to a Git repository
3. Submit test via OpenBench web interface
4. Monitor progress on dashboard

### Option 2: Local Testing (Fallback)
1. Install cutechess-cli
2. Run the generated script: `{script_path}`
3. Analyze results manually

## SPRT Information

SPRT (Sequential Probability Ratio Test) advantages:
- Stops early when statistical significance is reached
- More efficient than fixed-game testing
- Industry standard for chess engine development
- Provides confidence bounds

Expected outcomes for bounds {self.sprt_bounds}:
- **Pass**: Candidate is statistically better by {self.sprt_bounds.split(',')[1].strip(' ]')} ELO
- **Fail**: Candidate is worse by {self.sprt_bounds.split(',')[0].strip('[ ')} ELO  
- **Inconclusive**: Performance is between the bounds

## OpenBench Resources

- **Main Instance**: http://chess.grantnet.us/
- **Documentation**: https://github.com/AndyGrant/OpenBench/wiki
- **Discord**: https://discord.com/invite/9MVg7fBTpM
"""
        
        with open(report_file, 'w') as f:
            f.write(report_content)
        
        return report_file
    
def main():
    parser = argparse.ArgumentParser(description='Submit chess engine test to OpenBench')
    parser.add_argument('--baseline', required=True, help='Path to baseline engine executable')
    parser.add_argument('--candidate', required=True, help='Path to candidate engine executable')
    parser.add_argument('--games', type=int, default=20000, help='Maximum number of games (default: 20000)')
    parser.add_argument('--time-control', default='10.0+0.1', help='Time control (default: 10.0+0.1)')
    parser.add_argument('--sprt-bounds', default='[-3.0, 1.0]', help='SPRT bounds (default: [-3.0, 1.0])')
    parser.add_argument('--book', default='UHO_XXL_+0.90_+1.19.epd', help='Opening book')
    parser.add_argument('--config', default='openbench.conf', help='OpenBench config file')
    parser.add_argument('--output-dir', default='tournaments', help='Output directory (default: tournaments)')
    
    args = parser.parse_args()
    
    # Create OpenBench runner
    runner = OpenBenchRunner(
        baseline_engine=args.baseline,
        candidate_engine=args.candidate,
        games=args.games,
        time_control=args.time_control,
        sprt_bounds=args.sprt_bounds,
        book=args.book,
        config_file=args.config,
        output_dir=args.output_dir
    )
    
    # Check dependencies and engines
    if not runner.check_dependencies():
        print("\nWARNING: OpenBench client not found.")
        print("You can still generate local test scripts.")
        print("Continue? (y/n): ", end="")
        if input().lower() != 'y':
            sys.exit(1)
    
    if not runner.verify_engines():
        sys.exit(1)
    
    runner.check_config()  # Just warn, don't exit
    
    # Create test configuration
    test_config, config_file = runner.create_sprt_test()
    
    # Submit test (simulation)
    test_id = runner.submit_test(test_config)
    
    # Create local fallback script
    script_path = runner.create_local_test_script(test_config)
    
    # Explain SPRT
    runner.explain_sprt()
    
    # Create summary report
    report_file = runner.create_summary_report(test_config, test_id, config_file, script_path)
    
    print(f"\n✓ OpenBench test preparation completed!")
    print(f"✓ Summary report: {report_file}")
    print(f"✓ Local test script: {script_path}")
    print(f"✓ Test configuration: {config_file}")

if __name__ == '__main__':
    main()