@echo off
setLocal EnableDelayedExpansion

:: Parameter 1 = SolutionDir
:: Parameter 2 = TargetDir
:: Parameter 3 = TargetName
:: Parameter 4 = TargetExt

set SolutionDir=%~1
set TargetDir=%~2
set TargetName=%~3
set TargetExt=%~4

set CalculateMD5=%SolutionDir%\BatchScripts\CalculateMD5.bat

set OutputDir=%SolutionDir%\Output\
set TargetNameWithExt=%TargetName%%TargetExt%
set TargetPath=%TargetDir%%TargetNameWithExt%
set OutputPath=%OutputDir%%TargetNameWithExt%
set /a Result=0

echo.
echo ## Starting to copy to output... ##
echo.

echo * Checking files exists...
if not exist "%TargetPath%" (
	echo. + "%TargetNameWithExt%" file is not found in "%TargetDir%".
	goto :CopyToOutput
)

if not exist "%OutputPath%" (
	echo. + "%TargetNameWithExt%" file is not found in "%OutputDir%".
	goto :CopyToOutput
)

echo * Comparing hash...
call :CompareHash "%TargetPath%" "%OutputPath%" IsSame
if not "%IsSame%"=="Same" (
	echo. + "%TargetNameWithExt%" has changed.
	goto :CopyToOutput
)

echo * There are no changes.
goto :End

:CopyToOutput
echo * Copy "%TargetNameWithExt%" to "%OutputDir%".
xcopy /S /Y "%TargetPath%" "%OutputDir%" >nul
if %ERRORLEVEL% EQU 0 (
    echo * Copied.
) else (
	echo. + Failed to copy. Check it.
	set /a Result=-1	
)

:End
echo.
echo ## Copying to output ended. ##
echo.
exit /b %Result%

:CompareHash
setlocal
call "%CalculateMD5%" "%~1" Hash1
call "%CalculateMD5%" "%~2" Hash2
(endlocal 
	if %Hash1%==%Hash2% set "%~3=Same"
)
goto :eof