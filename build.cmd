@echo off
setlocal enabledelayedexpansion
cd /d "%~dp0"

set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo [ERROR] .NET Framework 4.x C# compiler ^(csc.exe^) not found.
    exit /b 1
)

echo [BUILD] Compiling Cyberwatch.exe...
"%CSC%" /nologo /warn:4 /warnaserror /optimize+ /t:exe /win32manifest:app.manifest /out:Cyberwatch.exe /r:System.dll,System.Core.dll,System.Data.dll,System.Management.dll,System.ServiceProcess.dll src\Program.cs src\UITheme.cs src\Helpers.cs src\Modules\SystemModules.cs src\Modules\NetworkModules.cs src\Modules\SecurityModules.cs
if !errorlevel! neq 0 (
    echo [FAILED] Cyberwatch.exe compilation failed.
    exit /b !errorlevel!
)
echo [OK] Built Cyberwatch.exe successfully.

if /i "%~1"=="test" (
    echo [TEST] Compiling tests\Cyberwatch.Tests.cs...
    "%CSC%" /nologo /warn:4 /warnaserror /t:exe /out:Cyberwatch.Tests.exe /r:Cyberwatch.exe,System.dll,System.Core.dll tests\Cyberwatch.Tests.cs
    if !errorlevel! neq 0 (
        echo [FAILED] Test compilation failed.
        exit /b !errorlevel!
    )
    echo [TEST] Running unit tests...
    Cyberwatch.Tests.exe
    set "TEST_EXIT=!errorlevel!"
    del /q Cyberwatch.Tests.exe 2>nul
    exit /b !TEST_EXIT!
)

exit /b 0
