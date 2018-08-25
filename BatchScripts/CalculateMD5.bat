@echo off
set /a Result=0

:: Parameter Check
if "%~1"==""  goto :Usage
if not exist "%~1" goto :NotFoundPath

:: It calculates MD5 hash of input file path.
for /f "tokens=*" %%a in ('CertUtil -hashfile "%~1" MD5 ^| findstr /V :') do (
	if "%~2"=="" (
		echo %%a
	) else (
		set %~2=%%a
	)
)
goto :End

:NotFoundPath
echo Not found path. "%1"
set /a Result=-1
goto :End

:Usage
echo Usage: CalculateMD5.bat [File Path] ([Out Variable (Optional)])
set /a Result=1

:End
exit /b %Result%