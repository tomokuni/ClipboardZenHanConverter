@echo off
setlocal

rem ============================================================
rem  ClipboardZenHanConverter (WinUI 3) 単一 exe ビルド&実行スクリプト
rem  WinUI3_publish_single.bat で単一 exe build し、実行する
rem ============================================================

rem このバッチファイルは UTF-8 で保存しているため、コンソールが Shift-JIS（既定）のままだと
rem 日本語の出力が文字化けする。出力の前にコードページを UTF-8 へ切り替え、終了時に元へ戻す。
rem 呼び出し元のコンソールを変えないため、元のコードページは先に控える。
for /f "tokens=2 delims=:" %%a in ('chcp') do set "ORIG_CP=%%a"
set "ORIG_CP=%ORIG_CP: =%"
chcp 65001 >nul

rem スクリプトの場所（buildScript）からリポジトリルートへ移動する
pushd "%~dp0.."

rem ターゲットランタイム（既定: win-x64）
if "%~1"=="" (set "RID=win-x64") else (set "RID=%~1")

rem 発行先は WinUI3_publish_single.bat と揃える（絶対パス）
set "OUTDIR=%CD%\publish\winui3-%RID%-single"
set "APP_EXE=%OUTDIR%\ClipboardZenHanConverter.App.WinUI3.exe"

rem ビルドを実行（同じフォルダにある publish スクリプトを呼ぶ）
call "%~dp0WinUI3_publish_single.bat" %RID%
if errorlevel 1 (
  echo.
  echo [ERROR] Build failed. Aborting.
  popd
  if defined ORIG_CP chcp %ORIG_CP% >nul 2>&1
  exit /b 1
)

rem 生成物の存在確認
if not exist "%APP_EXE%" (
  echo.
  echo [ERROR] Executable not found: %APP_EXE%
  popd
  if defined ORIG_CP chcp %ORIG_CP% >nul 2>&1
  exit /b 1
)

echo.
echo [OK] Launching: %APP_EXE%
echo [INFO] 初回起動時は依存ファイルの展開に時間がかかります。
echo.

start "" "%APP_EXE%"
popd
if defined ORIG_CP chcp %ORIG_CP% >nul 2>&1
endlocal
