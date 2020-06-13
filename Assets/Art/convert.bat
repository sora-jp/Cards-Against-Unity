@ECHO OFF
setlocal enabledelayedexpansion
for /R %%f in (*.svg) do (
  echo Converting %%~nf
  if not exist "%%~dpfout" mkdir "%%~dpfout"
  inkscape "%%f" -w 512 -h 512 -o "%%~dpfout\%%~nf.png" >nul 2>&1
)
echo Done!
pause