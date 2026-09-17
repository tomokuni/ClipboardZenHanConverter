@echo off
setlocal

rem ============================================================
rem  ClipboardZenHanConverter (WinUI 3) Release ビルド&実行スクリプト
rem  Release 構成でビルドし、生成された exe を実行する
rem  （publish を行わないため、単一ファイル化は適用されない）
rem ============================================================

rem スクリプトの場所（buildScript）からリポジトリルートへ移動する
pushd "%~dp0.."

set "PROJECT=src\app_WinUI3\app_WinUI3.csproj"
set "TFM=net10.0-windows10.0.26100.0"
set "APP_NAME=ClipboardZenHanConverter.App.WinUI3.exe"

rem WinUI 3 は自己完結（SelfContained）のため、出力は TFM 配下の RID フォルダになる
set "APPDIR=%CD%\src\app_WinUI3\bin\Release\%TFM%"
set "APP_EXE=%APPDIR%\win-x64\%APP_NAME%"

echo.
echo [INFO] Mode  : Release build
echo [INFO] Target: %TFM%
echo [INFO] Output: %APPDIR%
echo.

dotnet build "%PROJECT%" -c Release
if errorlevel 1 (
  echo.
  echo [ERROR] Build failed. Aborting.
  popd
  exit /b 1
)

rem RID フォルダの位置は構成により変わるため、見つからないときは再帰的に探す
if not exist "%APP_EXE%" (
  for /f "delims=" %%p in ('dir /s /b "%APPDIR%\%APP_NAME%" 2^>nul') do set "APP_EXE=%%p"
)

rem 生成物の存在確認
if not exist "%APP_EXE%" (
  echo.
  echo [ERROR] Executable not found: %APP_NAME% under %APPDIR%
  popd
  exit /b 1
)

echo.
echo [OK] Launching: %APP_EXE%
echo.

start "" "%APP_EXE%"
popd
endlocal
