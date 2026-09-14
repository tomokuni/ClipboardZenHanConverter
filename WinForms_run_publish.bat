@echo off
setlocal

rem ============================================================
rem  ClipboardZenHanConverter (WinForms) 単一 exe ビルド&実行スクリプト
rem  WinForms_publish_single.bat で単一 exe build し、実行する
rem ============================================================

rem スクリプトの場所を基準にリポジトリルートを取得する
pushd "%~dp0"

rem ターゲットランタイム（既定: win-x64）
if "%~1"=="" (set "RID=win-x64") else (set "RID=%~1")

rem 発行先は WinForms_publish_single.bat と揃える（絶対パス）
set "OUTDIR=%CD%\publish\winforms-%RID%-single"
set "APP_EXE=%OUTDIR%\ClipboardZenHanConverter.App.WinForms.exe"

rem ビルドを実行
call "WinForms_publish_single.bat" %RID%
if errorlevel 1 (
  echo.
  echo [ERROR] Build failed. Aborting.
  popd
  exit /b 1
)

rem 生成物の存在確認
if not exist "%APP_EXE%" (
  echo.
  echo [ERROR] Executable not found: %APP_EXE%
  popd
  exit /b 1
)

echo.
echo [OK] Launching: %APP_EXE%
echo.

start "" "%APP_EXE%"
popd
endlocal
