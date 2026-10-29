@echo off
REM Requires .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0

echo === 1) Portable single-file EXE (no runtime needed, larger) ===
dotnet publish -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:EnableCompressionInSingleFile=true -o publish\portable
if errorlevel 1 goto :err

echo.
echo === 2) Small single-file EXE (needs .NET 8 Desktop Runtime) ===
dotnet publish -c Release -r win-x64 --self-contained false ^
  -p:PublishSingleFile=true -o publish\small
if errorlevel 1 goto :err

echo.
echo Done. See publish\portable\HizliErisim.exe
pause
exit /b 0

:err
echo Build failed.
pause
exit /b 1
