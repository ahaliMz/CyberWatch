@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

:: Auto elevate to Administrator (preserve --no-elevate to avoid loops if declined)
if /i "%~1"=="--no-elevate" goto AfterElevate
net session >nul 2>&1
if %errorLevel% neq 0 (
    powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -ArgumentList '--no-elevate' -Verb RunAs" 2>nul
    if !errorLevel! equ 0 exit /b
)
:AfterElevate

title Cyberwatch v2.1 (Batch Fallback)
color 0A
mode con: cols=100 lines=36

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
set "choice="
set /p choice="     SELECT OPTION : "

if "!choice!"=="1" goto ServerMenu
if "!choice!"=="2" goto ClientMenu
if "!choice!"=="3" goto NetworkMenu
if "!choice!"=="4" goto FirewallMenu
if "!choice!"=="5" goto SecurityMenu
if "!choice!"=="6" goto AutoMenu
if "!choice!"=="7" goto HealthMenu
if "!choice!"=="8" goto ScanMenu
if "!choice!"=="9" goto AdvancedMenu
if "!choice!"=="10" goto ReportsMenu
if "!choice!"=="11" goto TrafficMenu
if "!choice!"=="12" goto MaintenanceMenu
if "!choice!"=="13" goto QuickMenu
if "!choice!"=="14" goto ExitProgram
if "!choice!"=="0" goto ExitProgram
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
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" (
    cls & echo. & echo     CPU USAGE:
    powershell -NoProfile -Command "try { $avg = (Get-CimInstance Win32_Processor -ErrorAction Stop | Measure-Object -Property LoadPercentage -Average).Average; Write-Host ('     LoadPercentage: ' + [math]::Round($avg) + '%%') } catch { Write-Host '     CPU metrics unavailable' }"
    echo. & pause & goto ServerMenu
)
if "!sub!"=="2" (
    cls & echo. & echo     RAM USAGE:
    powershell -NoProfile -Command "try { $os = Get-CimInstance Win32_OperatingSystem -ErrorAction Stop; $tot = [math]::Round($os.TotalVisibleMemorySize / 1024); $free = [math]::Round($os.FreePhysicalMemory / 1024); $used = $tot - $free; $pct = if ($tot -gt 0) { [math]::Round(($used * 100) / $tot) } else { 0 }; Write-Host ('     Used: ' + $used + ' MB / ' + $tot + ' MB (' + $pct + '%%)') } catch { Write-Host '     RAM metrics unavailable' }"
    echo. & pause & goto ServerMenu
)
if "!sub!"=="3" (
    cls & echo. & echo     DISK USAGE:
    powershell -NoProfile -Command "try { Get-CimInstance Win32_LogicalDisk -Filter 'DriveType=3' -ErrorAction Stop | Select-Object DeviceID, @{N='TotalGB';E={[math]::Round($_.Size/1GB,1)}}, @{N='FreeGB';E={[math]::Round($_.FreeSpace/1GB,1)}} | Format-Table -AutoSize } catch { Write-Host '     Disk metrics unavailable' }"
    echo. & pause & goto ServerMenu
)
if "!sub!"=="4" cls & echo. & echo     RUNNING SERVICES: & sc query state= all | findstr /C:"SERVICE_NAME" /C:"STATE" | more & echo. & pause & goto ServerMenu
if "!sub!"=="5" cls & tasklist | more & pause & goto ServerMenu
if "!sub!"=="6" (
    cls & echo. & echo     SYSTEM UPTIME:
    powershell -NoProfile -Command "try { $boot = (Get-CimInstance Win32_OperatingSystem -ErrorAction Stop).LastBootUpTime; $up = (Get-Date) - $boot; Write-Host ('     Last Boot: ' + $boot); Write-Host ('     Uptime:    ' + $up.Days + 'd ' + $up.Hours + 'h ' + $up.Minutes + 'm') } catch { Write-Host '     Uptime unavailable' }"
    echo. & pause & goto ServerMenu
)
if "!sub!"=="7" goto MainMenu
goto ServerMenu

:ClientMenu
cls
echo.
echo     ================================================================================
echo     =                           CLIENT MONITORING                                   =
echo     ================================================================================
echo.
set "target="
set /p target="     Enter PC Name or IP : "
call :ValidateHost target
if errorlevel 1 (
    echo     Invalid hostname or IP address!
    timeout /t 2 >nul
    goto MainMenu
)

:ClientSubMenu
cls
echo.
echo     Target: !target!
echo     --------------------------------------------------------------------------------
echo     [1] Remote PC Ping        [4] Disk Info
echo     [2] Logged Users Check    [5] Remote Restart or Shutdown
echo     [3] Shared Folder Check   [6] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" cls & echo. & echo     Pinging !target!... & ping "!target!" -n 4 & echo. & pause & goto ClientSubMenu
if "!sub!"=="2" cls & echo. & quser /server:"!target!" 2>nul || echo     No access or PC offline & echo. & pause & goto ClientSubMenu
if "!sub!"=="3" cls & echo. & net view "\\!target!" 2>nul || echo     No shared folders found or access denied & echo. & pause & goto ClientSubMenu
if "!sub!"=="4" (
    cls & echo. & echo     Querying disks on !target!...
    set "CW_TARGET=!target!"
    powershell -NoProfile -Command "try { Get-CimInstance Win32_LogicalDisk -ComputerName $env:CW_TARGET -Filter 'DriveType=3' -ErrorAction Stop | Select-Object DeviceID, @{N='TotalGB';E={[math]::Round($_.Size/1GB,1)}}, @{N='FreeGB';E={[math]::Round($_.FreeSpace/1GB,1)}} | Format-Table -AutoSize } catch { Write-Host '     Cannot access remote disk info' }"
    echo. & pause & goto ClientSubMenu
)
if "!sub!"=="5" goto RemoteAction
if "!sub!"=="6" goto MainMenu
goto ClientSubMenu

:RemoteAction
cls
echo.
echo     ================================================================================
echo     =                              REMOTE ACTION                                    =
echo     ================================================================================
echo     Target: !target!
echo.
echo     [1] Restart PC
echo     [2] Shutdown PC
echo     [3] Cancel Pending Shutdown
echo     [4] Back
echo.
echo     --------------------------------------------------------------------------------
echo.
set "action="
set /p action="     Select : "
if "!action!"=="1" (
    set "confirm="
    set /p confirm="     Confirm RESTART of !target! [y/N]: "
    if /i "!confirm!"=="y" (
        shutdown /m "\\!target!" /r /t 30 /c "Remote restart by admin" && echo     Restart command sent!
    ) else (
        echo     Cancelled.
    )
)
if "!action!"=="2" (
    set "confirm="
    set /p confirm="     Confirm SHUTDOWN of !target! [y/N]: "
    if /i "!confirm!"=="y" (
        shutdown /m "\\!target!" /s /t 30 /c "Remote shutdown by admin" && echo     Shutdown command sent!
    ) else (
        echo     Cancelled.
    )
)
if "!action!"=="3" shutdown /m "\\!target!" /a && echo     Abort command sent!
if "!action!"=="4" goto ClientSubMenu
pause
goto ClientSubMenu

:NetworkMenu
cls
echo.
echo     ================================================================================
echo     =                           NETWORK MONITORING                                  =
echo     ================================================================================
echo.
set "host="
set /p host="     Enter Host or IP (default 8.8.8.8) : "
if "!host!"=="" set "host=8.8.8.8"
call :ValidateHost host
if errorlevel 1 (
    echo     Invalid hostname or IP address!
    timeout /t 2 >nul
    goto MainMenu
)

:NetworkSubMenu
cls
echo.
echo     Host: !host!
echo     --------------------------------------------------------------------------------
echo     [1] Standard Ping        [4] Traceroute
echo     [2] Continuous Ping      [5] Netstat Connections
echo     [3] DNS Lookup           [6] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" cls & echo. & echo     Pinging !host!... & ping "!host!" -n 4 & echo. & pause & goto NetworkSubMenu
if "!sub!"=="2" cls & echo. & echo     Continuous Ping to !host! - Press Ctrl+C to stop & ping "!host!" -t & pause & goto NetworkSubMenu
if "!sub!"=="3" cls & echo. & nslookup "!host!" & echo. & pause & goto NetworkSubMenu
if "!sub!"=="4" cls & echo. & tracert "!host!" & echo. & pause & goto NetworkSubMenu
if "!sub!"=="5" cls & netstat -an | more & pause & goto NetworkSubMenu
if "!sub!"=="6" goto MainMenu
goto NetworkSubMenu

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
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" cls & netsh advfirewall show allprofiles & pause & goto FirewallMenu
if "!sub!"=="2" cls & netsh advfirewall show domainprofile & pause & goto FirewallMenu
if "!sub!"=="3" cls & netsh advfirewall show privateprofile & pause & goto FirewallMenu
if "!sub!"=="4" cls & netsh advfirewall show publicprofile & pause & goto FirewallMenu
if "!sub!"=="5" cls & netsh advfirewall firewall show rule name=all | more & pause & goto FirewallMenu
if "!sub!"=="6" goto MainMenu
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
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" cls & wevtutil qe System /c:20 /rd:true /f:text | more & pause & goto SecurityMenu
if "!sub!"=="2" cls & wevtutil qe Security /c:10 /rd:true /f:text | more & pause & goto SecurityMenu
if "!sub!"=="3" cls & wevtutil qe Security /c:50 /rd:true /f:text /q:"*[System[(EventID=4625)]]" | more & pause & goto SecurityMenu
if "!sub!"=="4" (
    cls & echo. & echo     ANTIVIRUS STATUS:
    powershell -NoProfile -Command "try { Get-CimInstance -Namespace root/SecurityCenter2 -ClassName AntiVirusProduct -ErrorAction Stop | Select-Object displayName, productState | Format-Table -AutoSize } catch { Get-Service WinDefend -ErrorAction SilentlyContinue | Select-Object Name, Status | Format-Table -AutoSize }"
    pause & goto SecurityMenu
)
if "!sub!"=="5" cls & netstat -an | findstr "LISTENING" & pause & goto SecurityMenu
if "!sub!"=="6" goto MainMenu
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
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" goto AutoCPU
if "!sub!"=="2" goto AutoRAM
if "!sub!"=="3" goto AutoNetwork
if "!sub!"=="4" goto LiveMonitor
if "!sub!"=="5" goto MainMenu
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
set "cpu=0"
for /f %%p in ('powershell -NoProfile -Command "try { [int][math]::Round((Get-CimInstance Win32_Processor -ErrorAction Stop | Measure-Object -Property LoadPercentage -Average).Average) } catch { 0 }"') do set "cpu=%%p"
if !cpu! GEQ 80 (color 0C & echo     [ALERT] CPU at !cpu!%%! & color 0A)
echo     [!time!] CPU: !cpu!%%
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
set "usedPercent=0"
for /f %%r in ('powershell -NoProfile -Command "try { $os = Get-CimInstance Win32_OperatingSystem -ErrorAction Stop; $tot = $os.TotalVisibleMemorySize; $free = $os.FreePhysicalMemory; if ($tot -gt 0) { [int][math]::Round((($tot - $free) * 100) / $tot) } else { 0 } } catch { 0 }"') do set "usedPercent=%%r"
if !usedPercent! GEQ 90 (color 0C & echo     [ALERT] RAM at !usedPercent!%%! & color 0A)
echo     [!time!] RAM: !usedPercent!%%
timeout /t 2 >nul
goto RAMLoop

:AutoNetwork
cls
echo.
echo     ================================================================================
echo     =                       NETWORK MONITORING ACTIVE                               =
echo     ================================================================================
echo.
set "pingTarget="
set /p pingTarget="     Enter IP or Host to monitor : "
call :ValidateHost pingTarget
if errorlevel 1 (
    echo     Invalid hostname or IP address!
    timeout /t 2 >nul
    goto AutoMenu
)
echo.
echo     Press Ctrl+C to stop
echo     --------------------------------------------------------------------------------
:NetLoop
ping -n 1 "!pingTarget!" >nul
if errorlevel 1 (color 0C & echo     [ALERT] Lost connection to !pingTarget!! & color 0A) else (echo     [!time!] OK - !pingTarget!)
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
set "cpu=0"
set "usedPercent=0"
for /f "tokens=1,2" %%a in ('powershell -NoProfile -Command "try { $cpu = [int][math]::Round((Get-CimInstance Win32_Processor -ErrorAction Stop | Measure-Object -Property LoadPercentage -Average).Average); $os = Get-CimInstance Win32_OperatingSystem -ErrorAction Stop; $tot = $os.TotalVisibleMemorySize; $free = $os.FreePhysicalMemory; $ram = if ($tot -gt 0) { [int][math]::Round((($tot - $free) * 100) / $tot) } else { 0 }; Write-Output ('' + $cpu + ' ' + $ram) } catch { Write-Output '0 0' }"') do (
    set "cpu=%%a"
    set "usedPercent=%%b"
)
ping -n 1 8.8.8.8 >nul && set "net=ONLINE" || set "net=OFFLINE"
echo     [!time!] CPU:!cpu!%% RAM:!usedPercent!%% NET:!net!
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
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" cls & echo. & echo     Running CHKDSK... & chkdsk & echo. & pause & goto HealthMenu
if "!sub!"=="2" cls & echo. & echo     Running SFC Scan... & sfc /scannow & echo. & pause & goto HealthMenu
if "!sub!"=="3" (
    cls & echo. & echo     NETWORK CHECK:
    ping 127.0.0.1 -n 2 >nul && echo     [PASS] Loopback || echo     [FAIL] Loopback
    ping 8.8.8.8 -n 2 >nul && echo     [PASS] Internet || echo     [FAIL] Internet
    ping google.com -n 2 >nul && echo     [PASS] DNS || echo     [FAIL] DNS
    echo. & pause & goto HealthMenu
)
if "!sub!"=="4" (
    cls & echo. & echo     SERVICE CHECK:
    sc query W32Time | findstr RUNNING >nul && echo     [OK] Windows Time: RUNNING || echo     [FAIL] Windows Time: STOPPED
    sc query Spooler | findstr RUNNING >nul && echo     [OK] Print Spooler: RUNNING || echo     [FAIL] Print Spooler: STOPPED
    sc query Dhcp | findstr RUNNING >nul && echo     [OK] DHCP: RUNNING || echo     [FAIL] DHCP: STOPPED
    echo. & pause & goto HealthMenu
)
if "!sub!"=="5" cls & systeminfo | more & pause & goto HealthMenu
if "!sub!"=="6" goto MainMenu
goto HealthMenu

:ScanMenu
cls
echo.
echo     ================================================================================
echo     =                           NETWORK SCAN TOOLS                                  =
echo     ================================================================================
echo.
echo     [1] Ping Sweep          [4] Vulnerability Check
echo     [2] Port Scan           [5] Back to Main Menu
echo     [3] ARP Scan
echo.
echo     --------------------------------------------------------------------------------
echo.
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" goto ScanSweep
if "!sub!"=="2" goto ScanPort
if "!sub!"=="3" cls & arp -a & pause & goto ScanMenu
if "!sub!"=="4" (
    cls & echo. & echo     VULNERABILITY CHECK:
    net share | findstr /C:"ADMIN" >nul && echo     [WARNING] Admin shares enabled || echo     [OK] No admin shares
    reg query "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v EnableLUA | findstr "0x1" >nul && echo     [OK] UAC enabled || echo     [WARNING] UAC disabled
    echo. & pause & goto ScanMenu
)
if "!sub!"=="5" goto MainMenu
goto ScanMenu

:ScanSweep
set "subnet="
set /p subnet="     Enter subnet prefix (example: 192.168.1): "
call :ValidateSubnet subnet
if errorlevel 1 (
    echo     Invalid subnet format! Expected e.g. 192.168.1
    timeout /t 2 >nul
    goto ScanMenu
)
cls & echo. & echo     Scanning !subnet!.1-254...
for /l %%i in (1,1,254) do (
    ping -n 1 -w 300 "!subnet!.%%i" | findstr "Reply" >nul && echo     [ALIVE] !subnet!.%%i
)
echo. & pause & goto ScanMenu

:ScanPort
set "targetIP="
set /p targetIP="     Enter IP or Host to scan (default 127.0.0.1): "
if "!targetIP!"=="" set "targetIP=127.0.0.1"
call :ValidateHost targetIP
if errorlevel 1 (
    echo     Invalid hostname or IP address!
    timeout /t 2 >nul
    goto ScanMenu
)
cls & echo. & echo     Scanning !targetIP!...
set "CW_TARGET=!targetIP!"
powershell -NoProfile -Command "21,22,23,25,80,443,3389,8080 | ForEach-Object { if (Test-NetConnection -ComputerName $env:CW_TARGET -Port $_ -WarningAction SilentlyContinue -InformationLevel Quiet) { Write-Host ('     [OPEN] Port: ' + $_) } }"
echo. & pause & goto ScanMenu

:AdvancedMenu
cls
echo.
echo     ================================================================================
echo     =                           ADVANCED TOOLS                                      =
echo     ================================================================================
echo.
set "domain="
set /p domain="     Enter Domain or IP : "
call :ValidateHost domain
if errorlevel 1 (
    echo     Invalid domain or IP address!
    timeout /t 2 >nul
    goto MainMenu
)

:AdvancedSubMenu
cls
echo.
echo     Target: !domain!
echo     --------------------------------------------------------------------------------
echo     [1] DNS Lookup          [3] RDAP / WHOIS Lookup (HTTPS)
echo     [2] IP Information      [4] Back to Main Menu
echo.
echo     --------------------------------------------------------------------------------
echo.
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" cls & echo. & nslookup "!domain!" & echo. & pause & goto AdvancedSubMenu
if "!sub!"=="2" (
    cls & echo.
    set "CW_DOMAIN=!domain!"
    powershell -NoProfile -Command "try { [System.Net.Dns]::GetHostEntry($env:CW_DOMAIN) | Select-Object HostName, AddressList | Format-List } catch { Write-Host ('     DNS resolution failed: ' + $_.Exception.Message) }"
    echo. & pause & goto AdvancedSubMenu
)
if "!sub!"=="3" (
    cls & echo. & echo     RDAP Lookup for !domain!...
    set "CW_DOMAIN=!domain!"
    powershell -NoProfile -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; try { $u = if ($env:CW_DOMAIN -match '^\d+\.\d+\.\d+\.\d+$') { 'https://rdap.org/ip/' + $env:CW_DOMAIN } else { 'https://rdap.org/domain/' + $env:CW_DOMAIN }; $r = Invoke-RestMethod -Uri $u -TimeoutSec 6; Write-Host ('     Handle: ' + $r.handle); Write-Host ('     Name:   ' + $r.ldhName); if ($r.status) { Write-Host ('     Status: ' + ($r.status -join ', ')) } } catch { Write-Host ('     RDAP query failed: ' + $_.Exception.Message) }"
    echo. & pause & goto AdvancedSubMenu
)
if "!sub!"=="4" goto MainMenu
goto AdvancedSubMenu

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
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" cls & systeminfo | more & pause & goto ReportsMenu
if "!sub!"=="2" cls & ipconfig /all & netstat -e & pause & goto ReportsMenu
if "!sub!"=="3" (
    cls & netsh advfirewall show allprofiles
    powershell -NoProfile -Command "try { Get-CimInstance -Namespace root/SecurityCenter2 -ClassName AntiVirusProduct -ErrorAction Stop | Select-Object displayName, productState | Format-Table -AutoSize } catch { Get-Service WinDefend -ErrorAction SilentlyContinue | Select-Object Name, Status | Format-Table -AutoSize }"
    pause & goto ReportsMenu
)
if "!sub!"=="4" goto MainMenu
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
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" cls & netstat -an | findstr "ESTABLISHED" & pause & goto TrafficMenu
if "!sub!"=="2" cls & netstat -an | findstr "LISTENING" & pause & goto TrafficMenu
if "!sub!"=="3" cls & netstat -e & pause & goto TrafficMenu
if "!sub!"=="4" goto MainMenu
goto TrafficMenu

:MaintenanceMenu
clshujhj.
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
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" cls & ipconfig /flushdns & echo. & echo     DNS Cache Flushed! & echo. & pause & goto MaintenanceMenu
if "!sub!"=="2" (
    cls
    set "confirm="
    set /p confirm="     Delete temporary files in %TEMP% [y/N]: "
    if /i "!confirm!"=="y" (
        del /q /f /s "%TEMP%\*" 2>nul
        echo. & echo     Temp Files Cleared!
    ) else (
        echo     Cancelled.
    )
    echo. & pause & goto MaintenanceMenu
)
if "!sub!"=="3" (
    cls
    set "confirm="
    set /p confirm="     Reset TCP/IP + Winsock and renew IP [y/N]: "
    if /i "!confirm!"=="y" (
        netsh int ip reset & netsh winsock reset & ipconfig /release & ipconfig /renew & ipconfig /flushdns
        echo. & echo     Network Stack Repaired!
    ) else (
        echo     Cancelled.
    )
    echo. & pause & goto MaintenanceMenu
)
if "!sub!"=="4" cls & driverquery | more & pause & goto MaintenanceMenu
if "!sub!"=="5" goto MainMenu
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
set "sub="
set /p sub="     Select : "

if "!sub!"=="1" cls & ipconfig /all & pause & goto QuickMenu
if "!sub!"=="2" cls & ipconfig /release & ipconfig /renew & pause & goto QuickMenu
if "!sub!"=="3" cls & netstat -an | more & pause & goto QuickMenu
if "!sub!"=="4" cls & netsh advfirewall show currentprofile & pause & goto QuickMenu
if "!sub!"=="5" cls & arp -a & pause & goto QuickMenu
if "!sub!"=="6" cls & route print & pause & goto QuickMenu
if "!sub!"=="7" cls & netsh wlan show profiles & pause & goto QuickMenu
if "!sub!"=="8" goto MainMenu
goto QuickMenu

:ExitProgram
cls
echo.
echo     ================================================================================
echo     =                        THANKS FOR USING CYBERWATCH!                          =
echo     =                           HAVE A GREAT DAY!                                  =
echo     ================================================================================
echo.
timeout /t 2 >nul
exit /b 0

:: ---------------- Validation Subroutines ----------------
:ValidateHost
set "CW_IN=!%~1!"
if "!CW_IN!"=="" exit /b 1
powershell -NoProfile -Command "$v = $env:CW_IN; if ([string]::IsNullOrWhiteSpace($v) -or $v.Length -gt 253) { exit 1 }; $ip = $null; if ([System.Net.IPAddress]::TryParse($v, [ref]$ip)) { exit 0 }; if ($v -match '^[A-Za-z0-9]([A-Za-z0-9\-\.]*[A-Za-z0-9])?$') { exit 0 } else { exit 1 }"
exit /b !errorLevel!

:ValidateSubnet
set "CW_IN=!%~1!"
if "!CW_IN!"=="" exit /b 1
powershell -NoProfile -Command "$parts = ($env:CW_IN + '').Split('.'); if ($parts.Count -ne 3) { exit 1 }; foreach ($p in $parts) { $b = 0; if (-not [byte]::TryParse($p, [ref]$b)) { exit 1 } }; exit 0"
exit /b !errorLevel!
