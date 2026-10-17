@echo off
setlocal
if "%~1"=="" (
  echo Keo tha thu muc Unity project vao file nay, hoac:
  echo   CAI_VAO_PROJECT.cmd "D:\path\to\UnityProject"
  pause
  exit /b 1
)
set "DEST=%~1"
if not exist "%DEST%\Assets" (
  echo Khong phai Unity project ^(thieu Assets^): %DEST%
  pause
  exit /b 1
)
copy /Y "%~dp0QLDATNPresence.ps1" "%DEST%\QLDATNPresence.ps1" >nul
copy /Y "%~dp0BAT_DAU.cmd" "%DEST%\BAT_DAU.cmd" >nul
copy /Y "%~dp0TAT.cmd" "%DEST%\TAT.cmd" >nul
copy /Y "%~dp0KIEM_TRA.cmd" "%DEST%\KIEM_TRA.cmd" >nul
echo Da copy vao: %DEST%
echo.
echo Buoc tiep:
echo   1. Enroll installer trong Unity ^(mot lan^)
echo   2. Double-click BAT_DAU.cmd trong project
echo   3. TAT.cmd khi muon tat
pause
endlocal
