@echo off
REM ToastPop WPF 빌드 및 배포 스크립트
REM .NET 8 SDK 필요

echo ======================================
echo ToastPop WPF Build Script
echo ======================================

REM 환경 변수 설정
set CONFIGURATION=Release
set RUNTIME=win-x64
set OUTPUT_DIR=publish

REM 이전 빌드 결과 삭제
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"

echo.
echo [1/4] Restoring packages...
dotnet restore ToastPopWpf.sln

echo.
echo [2/4] Building solution...
dotnet build ToastPopWpf.sln -c %CONFIGURATION% --no-restore

echo.
echo [3/4] Publishing self-contained single file...
REM Self-Contained 싱글 파일 배포 (최소 크기)
dotnet publish ToastPop.App/ToastPop.App.csproj ^
    -c %CONFIGURATION% ^
    -r %RUNTIME% ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:PublishTrimmed=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o %OUTPUT_DIR%

echo.
echo [4/4] Cleaning up...
REM 불필요한 파일 삭제
if exist "%OUTPUT_DIR%\*.pdb" del /q "%OUTPUT_DIR%\*.pdb"
if exist "%OUTPUT_DIR%\*.xml" del /q "%OUTPUT_DIR%\*.xml"

echo.
echo ======================================
echo Build completed!
echo Output: %OUTPUT_DIR%\ToastPop.exe
echo ======================================

REM 파일 크기 표시
for %%A in (%OUTPUT_DIR%\ToastPop.exe) do echo File size: %%~zA bytes

pause
