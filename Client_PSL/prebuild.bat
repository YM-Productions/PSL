@echo off

REM Log-File
set LOG_FILE=preBuildOutput.txt

REM Starting Logging
echo Starting... > %LOG_FILE%
echo Batch-Script startet at %DATE% %TIME% >> %LOG_FILE%

REM change to Parent Dir
cd /d %~dp0
echo Switched directory /../ >> %LOG_FILE%

echo Starting to Generate SpacetimeDB Bindings at %DATE% %TIME% >> %LOG_FILE%
REM Generate SpacetimeDB bindings for Client_PSL
spacetime generate --lang csharp --out-dir Client_PSL/module_bindings --project-path server >> %LOG_FILE% 2>&1
echo Generating SpacetimeDB Bindings ended at %DATE% %TIME% >> %LOG_FILE%

echo Batch-Script ended at %DATE% %TIME% >> %LOG_FILE%
