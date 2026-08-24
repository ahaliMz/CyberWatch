@echo off
chcp 65001 >nul
:: Auto elevate to Administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    powershell start -verb runas '%0'
    exit /b
)

title Cyberwatch V2.0
color 0A
setlocal enabledelayedexpansion
mode con: cols=100 lines=35

:MainMenu
cls
echo.
echo                        █▀▀ █▄█ █▄▄ █▀▀ █▀█ █ █ █ ▄▀█ ▀█▀ █▀▀ █ █
echo                        █▄▄   █  █▄█ ██▄ █▀▄ ▀▄▀▄▀ █▀█  █  █▄▄ █▀█
echo.

echo.
echo     --------------------------------------------------------------------------------
echo     [1] Server Monitoring       [8] Network Scan Tools
echo     [2] Client Monitoring       [9] Advanced Tools
echo     [3] Network Monitoring      [10] Reports and Dashboard
echo     [4] Firewall Monitoring     [11] Live Traffic Monitor
echo     [5] Security and Logs       [12] Maintenance Tools
echo     [6] Auto Monitoring         [13] Quick Actions
echo     [7] System Health Check     [14] Exit
echo     --------------------------------------------------------------------------------
echo.
set /p choice="     SELECT OPTION : "

if "%choice%"=="1" goto ServerMenu
if "%choice%"=="2" goto ClientMenu
if "%choice%"=="3" goto NetworkMenu
if "%choice%"=="4" goto FirewallMenu
if "%choice%"=="5" goto SecurityMenu
if "%choice%"=="6" goto AutoMenu
if "%choice%"=="7" goto HealthMenu
if "%choice%"=="8" goto ScanMenu
if "%choice%"=="9" goto AdvancedMenu
if "%choice%"=="10" goto ReportsMenu
if "%choice%"=="11" goto TrafficMenu
if "%choice%"=="12" goto MaintenanceMenu
if "%choice%"=="13" goto QuickMenu
if "%choice%"=="14" goto ExitProgram
echo     Invalid option!
timeout /t 2 >nul
goto MainMenu

:ServerMenu
cls
echo.
echo     ================================================================================
echo     =                           SERVER MONITORING                                   =
echo     ================================================================================
echo.
echo     [1] CPU Usage              [4] Running Services
echo     [2] RAM Usage              [5] Process List
echo     [3] Disk Usage             [6] System Uptime
echo     [7] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" cls & echo. & echo CPU USAGE: & wmic cpu get loadpercentage & echo. & pause & goto ServerMenu
if "%sub%"=="2" cls & echo. & echo RAM USAGE: & systeminfo | findstr Memory & echo. & pause & goto ServerMenu
if "%sub%"=="3" cls & echo. & echo DISK USAGE: & wmic logicaldisk where drivetype=3 get DeviceID,Size,FreeSpace & echo. & pause & goto ServerMenu
if "%sub%"=="4" cls & echo. & echo RUNNING SERVICES: & sc query state= all | findstr /C:"SERVICE_NAME" /C:"STATE" | more & echo. & pause & goto ServerMenu
if "%sub%"=="5" cls & tasklist | more & pause & goto ServerMenu
if "%sub%"=="6" cls & echo. & echo SYSTEM UPTIME: & wmic os get lastbootuptime & systeminfo | find "Boot Time" & echo. & pause & goto ServerMenu
if "%sub%"=="7" goto MainMenu
goto ServerMenu

:ClientMenu
cls
echo.
echo     ================================================================================
echo     =                           CLIENT MONITORING                                   =
echo     ================================================================================
echo.
set /p target="     Enter PC Name or IP : "
echo.
echo     [1] Remote PC Ping        [4] Disk Info
echo     [2] Logged Users Check    [5] Remote Restart or Shutdown
echo     [3] Shared Folder Check   [6] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" cls & echo. & echo Pinging %target%... & ping %target% -n 4 & echo. & pause & goto ClientMenu
if "%sub%"=="2" cls & echo. & quser /server:%target% 2>nul || echo No access or PC offline & echo. & pause & goto ClientMenu
if "%sub%"=="3" cls & echo. & net view \\%target% 2>nul || echo No shared folders found & echo. & pause & goto ClientMenu
if "%sub%"=="4" cls & echo. & wmic /node:"%target%" logicaldisk where drivetype=3 get DeviceID,Size,FreeSpace 2>nul || echo Cannot access disk info & echo. & pause & goto ClientMenu
if "%sub%"=="5" goto RemoteAction
if "%sub%"=="6" goto MainMenu
goto ClientMenu

:RemoteAction
cls
echo.
echo     ================================================================================
echo     =                              REMOTE ACTION                                    =
echo     ================================================================================
echo.
echo     [1] Restart PC
echo     [2] Shutdown PC
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p action="     Select : "
if "%action%"=="1" shutdown /m \\%target% /r /t 30 /c "Remote restart by admin" & echo     Restart command sent!
if "%action%"=="2" shutdown /m \\%target% /s /t 30 /c "Remote shutdown by admin" & echo     Shutdown command sent!
pause
goto ClientMenu

:NetworkMenu
cls
echo.
echo     ================================================================================
echo     =                           NETWORK MONITORING                                  =
echo     ================================================================================
echo.
set /p host="     Enter Host or IP : "
echo.
echo     [1] Standard Ping        [4] Traceroute
echo     [2] Continuous Ping      [5] Netstat Connections
echo     [3] DNS Lookup           [6] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" cls & echo. & echo Pinging %host%... & ping %host% -n 4 & echo. & pause & goto NetworkMenu
if "%sub%"=="2" cls & echo. & echo Continuous Ping to %host% - Press Ctrl+C to stop & ping %host% -t & pause & goto NetworkMenu
if "%sub%"=="3" cls & echo. & nslookup %host% & echo. & pause & goto NetworkMenu
if "%sub%"=="4" cls & echo. & tracert %host% & echo. & pause & goto NetworkMenu
if "%sub%"=="5" cls & netstat -an | more & pause & goto NetworkMenu
if "%sub%"=="6" goto MainMenu
goto NetworkMenu

:FirewallMenu
cls
echo.
echo     ================================================================================
echo     =                           FIREWALL MONITORING                                 =
echo     ================================================================================
echo.
echo     [1] Firewall Status (All)  [4] Public Profile
echo     [2] Domain Profile         [5] Firewall Rules
echo     [3] Private Profile        [6] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" cls & netsh advfirewall show allprofiles & pause & goto FirewallMenu
if "%sub%"=="2" cls & netsh advfirewall show domainprofile & pause & goto FirewallMenu
if "%sub%"=="3" cls & netsh advfirewall show privateprofile & pause & goto FirewallMenu
if "%sub%"=="4" cls & netsh advfirewall show publicprofile & pause & goto FirewallMenu
if "%sub%"=="5" cls & netsh advfirewall firewall show rule name=all | more & pause & goto FirewallMenu
if "%sub%"=="6" goto MainMenu
goto FirewallMenu

:SecurityMenu
cls
echo.
echo     ================================================================================
echo     =                           SECURITY AND LOGS                                   =
echo     ================================================================================
echo.
echo     [1] System Logs           [4] Antivirus Status
echo     [2] Security Logs         [5] Open Ports
echo     [3] Failed Login Attempts [6] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" cls & wevtutil qe System /c:20 /rd:true /f:text | more & pause & goto SecurityMenu
if "%sub%"=="2" cls & wevtutil qe Security /c:10 /rd:true /f:text | more & pause & goto SecurityMenu
if "%sub%"=="3" cls & wevtutil qe Security /c:50 /rd:true /f:text /q:"*[System[(EventID=4625)]]" | more & pause & goto SecurityMenu
if "%sub%"=="4" cls & WMIC /Namespace:\\root\SecurityCenter2 Path AntiVirusProduct Get displayName,productState & pause & goto SecurityMenu
if "%sub%"=="5" cls & netstat -an | findstr "LISTENING" & pause & goto SecurityMenu
if "%sub%"=="6" goto MainMenu
goto SecurityMenu

:AutoMenu
cls
echo.
echo     ================================================================================
echo     =                           AUTO MONITORING                                     =
echo     ================================================================================
echo.
echo     [1] CPU Alert Monitor    [4] Live System Monitor
echo     [2] RAM Alert Monitor    [5] Back to Main Menu
echo     [3] Network Alert Monitor
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" goto AutoCPU
if "%sub%"=="2" goto AutoRAM
if "%sub%"=="3" goto AutoNetwork
if "%sub%"=="4" goto LiveMonitor
if "%sub%"=="5" goto MainMenu
goto AutoMenu

:AutoCPU
cls
echo.
echo     ================================================================================
echo     =                        CPU MONITORING ACTIVE                                  =
echo     ================================================================================
echo.
echo     Alert threshold: 80 percent (Press Ctrl+C to stop)
echo     --------------------------------------------------------------------------------
:CPULoop
for /f "skip=1" %%p in ('wmic cpu get loadpercentage') do if not "%%p"=="" set cpu=%%p
if !cpu! GEQ 80 (color 0C & echo     [ALERT] CPU at !cpu! percent! & color 0A)
echo     [%time%] CPU: !cpu!%%
timeout /t 2 >nul
goto CPULoop

:AutoRAM
cls
echo.
echo     ================================================================================
echo     =                        RAM MONITORING ACTIVE                                  =
echo     ================================================================================
echo.
echo     Alert threshold: 90 percent (Press Ctrl+C to stop)
echo     --------------------------------------------------------------------------------
:RAMLoop
for /f "tokens=2 delims==" %%r in ('wmic os get FreePhysicalMemory /value') do set free=%%r
for /f "tokens=2 delims==" %%t in ('wmic computersystem get TotalPhysicalMemory /value') do set total=%%t
set /a freeMB=!free!/1024
set /a totalMB=!total!/1048576
set /a usedMB=totalMB - freeMB
set /a usedPercent=usedMB * 100 / totalMB
if !usedPercent! GEQ 90 (color 0C & echo     [ALERT] RAM at !usedPercent! percent! & color 0A)
echo     [%time%] RAM: !usedPercent!%%
timeout /t 2 >nul
goto RAMLoop

:AutoNetwork
cls
echo.
echo     ================================================================================
echo     =                       NETWORK MONITORING ACTIVE                               =
echo     ================================================================================
echo.
set /p pingTarget="     Enter IP to monitor : "
echo.
echo     Press Ctrl+C to stop
echo     --------------------------------------------------------------------------------
:NetLoop
ping -n 1 %pingTarget% >nul
if errorlevel 1 (color 0C & echo     [ALERT] Lost connection to %pingTarget%! & color 0A) else (echo     [%time%] OK - %pingTarget%)
timeout /t 3 >nul
goto NetLoop

:LiveMonitor
cls
echo.
echo     ================================================================================
echo     =                         LIVE SYSTEM MONITOR                                   =
echo     ================================================================================
echo.
echo     Press Ctrl+C to stop
echo     --------------------------------------------------------------------------------
:LiveLoop
for /f "skip=1" %%p in ('wmic cpu get loadpercentage') do if not "%%p"=="" set cpu=%%p
for /f "tokens=2 delims==" %%r in ('wmic os get FreePhysicalMemory /value') do set free=%%r
for /f "tokens=2 delims==" %%t in ('wmic computersystem get TotalPhysicalMemory /value') do set total=%%t
set /a freeMB=!free!/1024
set /a totalMB=!total!/1048576
set /a usedMB=totalMB - freeMB
set /a usedPercent=usedMB * 100 / totalMB
ping -n 1 8.8.8.8 >nul && set net=ONLINE || set net=OFFLINE
echo     [%time%] CPU:!cpu!%% RAM:!usedPercent!%% NET:!net!
timeout /t 2 >nul
goto LiveLoop

:HealthMenu
cls
echo.
echo     ================================================================================
echo     =                           SYSTEM HEALTH CHECK                                 =
echo     ================================================================================
echo.
echo     [1] Disk Health           [4] Critical Services Check
echo     [2] SFC Scan              [5] System Information
echo     [3] Network Connectivity  [6] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" cls & echo. & echo Running CHKDSK... & chkdsk & echo. & pause & goto HealthMenu
if "%sub%"=="2" cls & echo. & echo Running SFC Scan... & sfc /scannow & echo. & pause & goto HealthMenu
if "%sub%"=="3" cls & echo. & echo NETWORK CHECK: & ping 127.0.0.1 -n 2 >nul && echo     [PASS] Loopback || echo     [FAIL] Loopback & ping 8.8.8.8 -n 2 >nul && echo     [PASS] Internet || echo     [FAIL] Internet & ping google.com -n 2 >nul && echo     [PASS] DNS || echo     [FAIL] DNS & echo. & pause & goto HealthMenu
if "%sub%"=="4" cls & echo. & echo SERVICE CHECK: & sc query W32Time | findstr RUNNING >nul && echo     [OK] Windows Time: RUNNING || echo     [FAIL] Windows Time: STOPPED & sc query Spooler | findstr RUNNING >nul && echo     [OK] Print Spooler: RUNNING || echo     [FAIL] Print Spooler: STOPPED & sc query Dhcp | findstr RUNNING >nul && echo     [OK] DHCP: RUNNING || echo     [FAIL] DHCP: STOPPED & echo. & pause & goto HealthMenu
if "%sub%"=="5" cls & systeminfo | more & pause & goto HealthMenu
if "%sub%"=="6" goto MainMenu
goto HealthMenu

:ScanMenu
cls
echo.
echo     ================================================================================
echo     =                           NETWORK SCAN TOOLS                                  =
echo     ================================================================================
echo.
set /p subnet="     Enter subnet (example: 192.168.1): "
echo.
echo     [1] Ping Sweep          [4] Vulnerability Check
echo     [2] Port Scan           [5] Back to Main Menu
echo     [3] ARP Scan
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" cls & echo. & echo Scanning %subnet%.1-254... & for /l %%i in (1,1,254) do (ping -n 1 %subnet%.%%i | findstr "Reply" >nul && echo     [ALIVE] %subnet%.%%i) & echo. & pause & goto ScanMenu
if "%sub%"=="2" set /p targetIP="     Enter IP to scan: " & cls & echo. & echo Scanning %targetIP%... & powershell "21,22,23,25,80,443,3389,8080 | ForEach-Object { if(Test-NetConnection %targetIP% -Port $_ -WarningAction SilentlyContinue -InformationLevel Quiet){Write-Host '     [OPEN] Port:' $_} }" & echo. & pause & goto ScanMenu
if "%sub%"=="3" cls & arp -a & pause & goto ScanMenu
if "%sub%"=="4" cls & echo. & echo VULNERABILITY CHECK: & net share | findstr /C:"ADMIN" >nul && echo     [WARNING] Admin shares enabled || echo     [OK] No admin shares & reg query "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v EnableLUA | findstr "0x1" >nul && echo     [OK] UAC enabled || echo     [WARNING] UAC disabled & echo. & pause & goto ScanMenu
if "%sub%"=="5" goto MainMenu
goto ScanMenu

:AdvancedMenu
cls
echo.
echo     ================================================================================
echo     =                           ADVANCED TOOLS                                      =
echo     ================================================================================
echo.
set /p domain="     Enter Domain or IP : "
echo.
echo     [1] DNS Lookup          [3] WHOIS Lookup
echo     [2] IP Information      [4] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" cls & echo. & nslookup %domain% & echo. & pause & goto AdvancedMenu
if "%sub%"=="2" cls & echo. & nslookup %domain% & for /f "tokens=2 delims=:" %%i in ('ping %domain% -n 1 ^| findstr "Pinging"') do echo     Resolved IP: %%i & echo. & pause & goto AdvancedMenu
if "%sub%"=="3" cls & echo. & echo WHOIS for %domain% & nslookup %domain% & echo. & pause & goto AdvancedMenu
if "%sub%"=="4" goto MainMenu
goto AdvancedMenu

:ReportsMenu
cls
echo.
echo     ================================================================================
echo     =                           REPORTS AND DASHBOARD                               =
echo     ================================================================================
echo.
echo     [1] System Report        [3] Security Report
echo     [2] Network Report       [4] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" cls & systeminfo | more & pause & goto ReportsMenu
if "%sub%"=="2" cls & ipconfig /all & netstat -e & pause & goto ReportsMenu
if "%sub%"=="3" cls & netsh advfirewall show allprofiles & WMIC /Namespace:\\root\SecurityCenter2 Path AntiVirusProduct Get displayName & pause & goto ReportsMenu
if "%sub%"=="4" goto MainMenu
goto ReportsMenu

:TrafficMenu
cls
echo.
echo     ================================================================================
echo     =                           LIVE TRAFFIC MONITOR                                =
echo     ================================================================================
echo.
echo     [1] Established Connections  [3] Bandwidth Statistics
echo     [2] Listening Ports          [4] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" cls & netstat -an | findstr "ESTABLISHED" & pause & goto TrafficMenu
if "%sub%"=="2" cls & netstat -an | findstr "LISTENING" & pause & goto TrafficMenu
if "%sub%"=="3" cls & netstat -e & pause & goto TrafficMenu
if "%sub%"=="4" goto MainMenu
goto TrafficMenu

:MaintenanceMenu
cls
echo.
echo     ================================================================================
echo     =                           MAINTENANCE TOOLS                                   =
echo     ================================================================================
echo.
echo     [1] Flush DNS Cache      [4] Driver Information
echo     [2] Clear Temp Files     [5] Back to Main Menu
echo     [3] Repair Network Stack
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" cls & ipconfig /flushdns & echo. & echo     DNS Cache Flushed! & echo. & pause & goto MaintenanceMenu
if "%sub%"=="2" cls & del /q /f /s "%TEMP%\*" 2>nul & echo. & echo     Temp Files Cleared! & echo. & pause & goto MaintenanceMenu
if "%sub%"=="3" cls & netsh int ip reset & netsh winsock reset & ipconfig /release & ipconfig /renew & ipconfig /flushdns & echo. & echo     Network Stack Repaired! & echo. & pause & goto MaintenanceMenu
if "%sub%"=="4" cls & driverquery | more & pause & goto MaintenanceMenu
if "%sub%"=="5" goto MainMenu
goto MaintenanceMenu

:QuickMenu
cls
echo.
echo     ================================================================================
echo     =                           QUICK ACTIONS                                       =
echo     ================================================================================
echo.
echo     [1] IPConfig all        [5] ARP Table
echo     [2] Release and Renew   [6] Route Table
echo     [3] Netstat an          [7] WiFi Profiles
echo     [4] Firewall Status     [8] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set /p sub="     Select : "

if "%sub%"=="1" cls & ipconfig /all & pause & goto QuickMenu
if "%sub%"=="2" cls & ipconfig /release & ipconfig /renew & pause & goto QuickMenu
if "%sub%"=="3" cls & netstat -an | more & pause & goto QuickMenu
if "%sub%"=="4" cls & netsh advfirewall show currentprofile & pause & goto QuickMenu
if "%sub%"=="5" cls & arp -a & pause & goto QuickMenu
if "%sub%"=="6" cls & route print & pause & goto QuickMenu
if "%sub%"=="7" cls & netsh wlan show profiles & pause & goto QuickMenu
if "%sub%"=="8" goto MainMenu
goto QuickMenu

:ExitProgram
cls
echo.
echo     ================================================================================
echo     =                        THANKS FOR USING CYBERWATCH!                          =
echo     =                           HAVE A GREAT DAY!                                  =
echo     ================================================================================
echo.
timeout /t 3 >nul
exit