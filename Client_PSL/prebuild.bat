@echo off
setlocal enabledelayedexpansion

REM === Set absolute paths ===
set "SCRIPT_DIR=%~dp0"
set "LOG_FILE=%SCRIPT_DIR%preBuildOutput.txt"
set "CACHE_FILE=%SCRIPT_DIR%.spacetime_hash"
set "SCHEMA_DIR=%SCRIPT_DIR%..\server"
set "OUT_DIR=%SCRIPT_DIR%..\Client_PSL\module_bindings"

echo ------------------------ > "%LOG_FILE%"
echo Script started at %DATE% %TIME% >> "%LOG_FILE%"
echo SCRIPT_DIR: %SCRIPT_DIR% >> "%LOG_FILE%"
echo SCHEMA_DIR: %SCHEMA_DIR% >> "%LOG_FILE%"
echo OUT_DIR: %OUT_DIR% >> "%LOG_FILE%"

REM === Collect files to hash ===
set HASH_INPUT_FILES=
for /r "%SCHEMA_DIR%" %%F in (*.cs *.toml) do (
    echo %%F >> "%LOG_FILE%"
    set "HASH_INPUT_FILES=!HASH_INPUT_FILES! %%F"
)

if "%HASH_INPUT_FILES%"=="" (
    echo ERROR: No files found for hashing – aborting >> "%LOG_FILE%"
    exit /b 1
)

REM === Compute hash (using certutil as fallback) ===
type %HASH_INPUT_FILES% > "%SCRIPT_DIR%_hashinput.tmp"
for /f "tokens=1" %%H in ('certutil -hashfile "%SCRIPT_DIR%_hashinput.tmp" SHA1 ^| find /i /v "SHA1" ^| find /i /v "certutil"') do (
    set "CURRENT_HASH=%%H"
)
del "%SCRIPT_DIR%_hashinput.tmp"

echo Computed hash: %CURRENT_HASH% >> "%LOG_FILE%"

REM === Compare with cached hash ===
if exist "%CACHE_FILE%" (
    set /p CACHED_HASH=<"%CACHE_FILE%"
    echo Existing cached hash: %CACHED_HASH% >> "%LOG_FILE%"
    if /i "%CURRENT_HASH%"=="%CACHED_HASH%" (
        echo No changes detected – skipping generation >> "%LOG_FILE%"
        echo Script ended at %DATE% %TIME% >> "%LOG_FILE%"
        exit /b 0
    )
)

REM === Delete old bindings ===
echo Cleaning old bindings... >> "%LOG_FILE%"
del /q "%OUT_DIR%\*" >nul 2>&1

REM === Generate bindings ===
echo Generating SpacetimeDB Bindings at %DATE% %TIME% >> "%LOG_FILE%"
spacetime generate --lang csharp --out-dir "%OUT_DIR%" --project-path "%SCHEMA_DIR%" --yes >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
    echo Generation FAILED at %DATE% %TIME% >> "%LOG_FILE%"
    exit /b 1
)
echo Generation finished at %DATE% %TIME% >> "%LOG_FILE%"

REM === Save new hash ===
echo %CURRENT_HASH% > "%CACHE_FILE%"

echo Script ended at %DATE% %TIME% >> "%LOG_FILE%"
exit /b 0
