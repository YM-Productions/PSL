@echo off
REM === Set absolute path to script directory ===
set SCRIPT_DIR=%~dp0
set LOG_FILE=%SCRIPT_DIR%preBuildOutput.txt

REM === Start Logging ===
echo Starting... > "%LOG_FILE%"
echo Script started at %DATE% %TIME% >> "%LOG_FILE%"

REM === Change to parent directory ===
cd /d %SCRIPT_DIR%\..
echo Switched to directory: %CD% >> "%LOG_FILE%"

REM === Generate SpacetimeDB bindings ===
echo Generating SpacetimeDB Bindings at %DATE% %TIME% >> "%LOG_FILE%"
spacetime generate --lang csharp --out-dir Client_PSL/module_bindings --project-path server --yes >> "%LOG_FILE%" 2>&1
echo Generation finished at %DATE% %TIME% >> "%LOG_FILE%"

echo Script ended at %DATE% %TIME% >> "%LOG_FILE%"
