@echo off
setlocal
cd /d "%~dp0"
set "ROOT=%cd%"
title QLDATN Presence - Kiem tra live
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0QLDATNPresence.ps1" -Action watch -ProjectRoot "%ROOT%"
endlocal
