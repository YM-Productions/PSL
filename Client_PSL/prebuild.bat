@echo off

REM Log-File
set LOG_FILE=preBuildOutput.txt

REM Starting Logging
echo Starting... > %LOG_FILE%
echo Batch-Script started at %DATE% %TIME% >> %LOG_FILE%

REM Define relative Dir Path
set DIRECTORY=Generated\SourceGenerator\SourceGenerator.SourceGenerator

REM Check if Dir exists
if not exist %DIRECTORY% (
	echo The Dir %DIRECTORY% was not found. >> %LOG_FILE%
	exit /b 1
)

REM Go trough all Files ending on .ygen.txt.cs
for /r "%DIRECTORY%" %%f in (*.ygen.txt.cs) do (
	echo Renaming: %%f >> %LOG_FILE%

	setlocal enabledelayedexpansion
	set "new_name=%%~nxf"

	REM Remove the last 3 chars -> ".cs"
	set "new_name=!new_name:~0,-3!"

	REM Check if file already exists, if yes, Remove
	if exist "%%~dpf!new_name!" (
		REM Delete existing File, before renaming the new one
		del /f /q "%%~dpf!new_name!" >> %LOG_FILE% 2>&1
		echo Deleted old existing File: %%~dpf!new_name! >> %LOG_FILE%
	)

	REM Rename new file
	ren "%%f" "!new_name!" >> %LOG_FILE% 2>&1

	REM Check if file was renamed
	if exist "%%~dpf!new_name!" (
		echo File was renamed successfully: %%f -> %%~dpf!new_name! >> %LOG_FILE%
	) else (
		echo ERROR: There was an error whilest renaming the file: %%f >> %LOG_FILE%
	)

	endlocal
)

REM Final Log
echo Batch_Script ended at %DATE% %TIME% >> %LOG_FILE%
echo ------------------------------------------------- >> %LOG_FILE%
