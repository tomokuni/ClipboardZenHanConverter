@echo off
setlocal

rem ============================================================
rem  ClipboardZenHanConverter (MewUI) 単一 exe ビルド&実行スクリプト
rem  MewUI_publish_singleaot.bat で Native AOT build し、実行する
rem ============================================================

rem スクリプトの場所を基準にリポジトリルートを取得する
pushd "%~dp0"

rem ターゲットランタイム（既定: win-x64）
if "%~1"=="" (set "RID=win-x64") else (set "RID=%~1")

rem 出力先は MewUI_publish_singleaot.bat と揃える（絶対パス）
set "OUTDIR=%CD%\publish\mewui-%RID%-singleaot"
set "APP_EXE=%OUTDIR%\ClipboardZenHanConverter.App.MewUI.exe"

rem ビルドを実行
call "MewUI_publish_singleaot.bat" %RID%
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
