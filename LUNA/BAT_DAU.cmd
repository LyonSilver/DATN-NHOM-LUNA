@echo off
setlocal
cd /d "%~dp0"
set "ROOT=%cd%"
start "" /min powershell -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File "%~dp0QLDATNPresence.ps1" -Action run -Quiet -ProjectRoot "%ROOT%"
echo Da bat (chay an). Doi ~30s roi xem dashboard.
endlocal
