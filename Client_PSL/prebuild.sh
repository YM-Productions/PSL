#!/bin/bash

# Log-File
SCRIPT_PATH="$(realpath "$0")"
SCRIPT_DIR="$(dirname "$SCRIPT_PATH")"
LOG_FILE="$SCRIPT_DIR/preBuildOutput.txt"

# Start Loggin
echo "Starting..." >"$LOG_FILE"
echo "Script started at $(date)" >>"$LOG_FILE"

# Move to Parent Dir
cd ..
echo "Switched to directory: $(pwd)" >>"$LOG_FILE"

# Start SpaceTimeDB Binding generation
echo "Generating SpacetimeDB Bindings at $(date)" >>"$LOG_FILE"
spacetime generate --lang csharp --out-dir Client_PSL/module_bindings --project-path server --yes >>"$LOG_FILE" 2>&1
echo "Generation finished at $(date)" >>"$LOG_FILE"

echo "Script ended at $(date)" >>"$LOG_FILE"
