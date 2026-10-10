@echo off
setlocal
cd /d "%~dp0"
set "ROOT=%cd%"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0QLDATNPresence.ps1" -Action stop -ProjectRoot "%ROOT%"
echo Da gui lenh tat.
endlocal
