@echo off
setLocal EnableDelayedExpansion

:: Parameter 1 = $(SolutionDir)
:: Parameter 2 = $(ProjectDir)
:: Parameter 3 = $(TargetDir)
:: Parameter 4 = $(TargetName)
:: Parameter 5 = .vsixmanifest file name

set SolutionDir=%~1
set ProjectDir=%~2
set TargetDir=%~3
set TargetName=%~4
set VSIXManifestName=%~5

set CalculateMD5=%SolutionDir%\BatchScripts\CalculateMD5.bat

set OutputDir=%SolutionDir%\Output\
set OutputHashDir=%ProjectDir%OutputHash\

set VSIXManifestHashPath=%OutputHashDir%%VSIXManifestName%.md5
set TargetVSIXManifestPath=%ProjectDir%%VSIXManifestName%

set DllName=%TargetName%.dll
set DllHashPath=%OutputHashDir%%DllName%.md5
set TargetDllPath=%TargetDir%%DllName%

set VSIXName=%TargetName%.vsix
set TargetVSIXPath=%TargetDir%%VSIXName%
set OutputVSIXPath=%OutputDir%%VSIXName%

echo.
echo ## Starting to copy vsix to output... ##
echo.

echo * Checking Vsix file already exists...
if not exist "%OutputVSIXPath%" (
	echo. ~~ "%VSIXName%" file is not found in "Output" in Solution Directory.
	goto :ExportOutputHash
)

echo * Comparing VSIXManifest hash...
call :CompareHash "%VSIXManifestHashPath%" "%TargetVSIXManifestPath%" IsSame
if not "%IsSame%"=="Same" (
	echo. ~~ "%VSIXManifestName%" has changed.
	goto :ExportOutputHash
)

echo * Comparing Dll hash...
call :CompareHash "%DllHashPath%" "%TargetDllPath%" IsSame
if not "%IsSame%"=="Same" (
	echo. ~~ "%DllName%" has changed.
	goto :ExportOutputHash
)

echo * There are no changes.
goto :End

:ExportOutputHash
echo. - Export hashes to "OutputHash" in Project Directory.
if not exist %OutputHashDir% mkdir %OutputHashDir%
call "%CalculateMD5%" "%TargetVSIXManifestPath%" > "%VSIXManifestHashPath%"
call "%CalculateMD5%" "%TargetDllPath%" > "%DllHashPath%"

:CopyToOutput
echo. - Copy "%VSIXName%" to "Output" in Solution Directory.
xcopy /S /Y "%TargetVSIXPath%" "%OutputDir%"

:End
echo.
echo ## Copying vsix to output ended. ##
echo.
exit /b 0

:CompareHash
setlocal
if exist "%~1" for /f "tokens=1 usebackq" %%a in ("%~1") do (
	if not "%%a"=="" set Hash1=%%a
)
call "%CalculateMD5%" "%~2" Hash2
(endlocal 
	if %Hash1%==%Hash2% set "%~3=Same"
)
goto :eof