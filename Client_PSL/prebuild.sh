#!/bin/bash

SCRIPT_PATH="$(realpath "$0")"
SCRIPT_DIR="$(dirname "$SCRIPT_PATH")"
LOG_FILE="$SCRIPT_DIR/preBuildOutput.txt"
CACHE_FILE="$SCRIPT_DIR/.spacetime_hash"
SCHEMA_DIR="$SCRIPT_DIR/../server"
OUT_DIR="$SCRIPT_DIR/../Client_PSL/module_bindings"

echo "------------------------" >"$LOG_FILE"
echo "Script started at $(date)" >>"$LOG_FILE"
echo "SCRIPT_DIR: $SCRIPT_DIR" >>"$LOG_FILE"
echo "SCHEMA_DIR: $SCHEMA_DIR" >>"$LOG_FILE"
echo "OUT_DIR: $OUT_DIR" >>"$LOG_FILE"

# Hash relevante Dateien (Inhalt)
echo "Files considered for hashing:" >>"$LOG_FILE"
FILES=$(find "$SCHEMA_DIR" -type f \( -name '*.cs' -o -name '*.toml' \))
echo "$FILES" >>"$LOG_FILE"

CURRENT_HASH=$(cat $FILES | sha1sum | awk '{print $1}')
echo "Computed hash: $CURRENT_HASH" >>"$LOG_FILE"

if [ -z "$CURRENT_HASH" ]; then
    echo "ERROR: No files found for hashing – aborting" | tee -a "$LOG_FILE"
    exit 1
fi

if [ -f "$CACHE_FILE" ]; then
    echo "Existing cached hash: $(cat "$CACHE_FILE")" >>"$LOG_FILE"
fi

if [ -f "$CACHE_FILE" ] && grep -q "$CURRENT_HASH" "$CACHE_FILE"; then
    echo "No changes detected – skipping generation" | tee -a "$LOG_FILE"
    echo "Script ended at $(date)" >>"$LOG_FILE"
    exit 0
fi

echo "Cleaning old bindings..." >>"$LOG_FILE"
rm -rf "$OUT_DIR"/*

echo "Generating SpacetimeDB Bindings at $(date)" >>"$LOG_FILE"
if ! spacetime generate --lang csharp --out-dir "$OUT_DIR" --project-path "$SCHEMA_DIR" --yes >>"$LOG_FILE" 2>&1; then
    echo "Generation FAILED at $(date)" | tee -a "$LOG_FILE"
    exit 1
fi
echo "Generation finished at $(date)" >>"$LOG_FILE"

echo "$CURRENT_HASH" >"$CACHE_FILE"
echo "Script ended at $(date)" >>"$LOG_FILE"
