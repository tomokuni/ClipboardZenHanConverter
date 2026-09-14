@echo off
setlocal

rem ============================================================
rem  ClipboardZenHanConverter (WinForms) 単一 exe ビルドスクリプト
rem  Release / 自己完結 / 単一 exe を生成する
rem
rem  注意: WinForms は Native AOT に未対応で、トリミングも
rem        サポートされません（NETSDK1175）。
rem        そのため AOT / トリムは使用せず、単一ファイル化のみを行います。
rem ============================================================

rem スクリプトの場所を基準にリポジトリルートを取得する
pushd "%~dp0"

rem ターゲットランタイム（既定: win-x64）
if "%~1"=="" (set "RID=win-x64") else (set "RID=%~1")

set "APP_NAME=ClipboardZenHanConverter.App.WinForms.exe"

rem 最終的な出力先（他 UI の出力先と分ける）
set "OUTDIR=%CD%\publish\winforms-%RID%-single"

rem 発行の一時ステージング先
rem リポジトリ内へ直接発行するとフレームワーク配置のファイルが出力先へ混在する。
rem また、プロジェクトディレクトリ配下へ発行すると出力がアプリのコンテンツとして
rem 取り込まれ、単一 exe が肥大化する。
rem そのためリポジトリ外の一時領域へ発行し、単一 exe のみを出力先へ配置する。
set "STAGE=%TEMP%\czhc-single-winforms-%RID%"

echo.
echo [INFO] Mode  : Self-contained single exe (no AOT, no trimming)
echo [INFO] Target: %RID%
echo [INFO] Output: %OUTDIR%
echo.

if exist "%STAGE%" rd /s /q "%STAGE%"

rem 単一ファイル化の要件:
rem   PublishSingleFile=true  … 依存ファイルを exe へ束ねる
rem   SelfContained=true      … 配布先に .NET ランタイムを要求しない
rem   DebugType=None          … .pdb を出力しない（配布対象外）
rem 初回起動時に依存ファイルが %TEMP%\.net 配下へ展開されます。
dotnet publish "src\app_WinForms\app_WinForms.csproj" ^
  -c Release ^
  -r "%RID%" ^
  -p:PublishSingleFile=true ^
  -p:SelfContained=true ^
  -p:DebugType=None ^
  -o "%STAGE%"

if errorlevel 1 (
  echo.
  echo [ERROR] publish failed. See log above.
  if exist "%STAGE%" rd /s /q "%STAGE%"
  popd
  exit /b 1
)

if not exist "%STAGE%\%APP_NAME%" (
  echo.
  echo [ERROR] Executable not found: %STAGE%\%APP_NAME%
  if exist "%STAGE%" rd /s /q "%STAGE%"
  popd
  exit /b 1
)

rem 出力先を作り直し、単一 exe のみを配置する
if exist "%OUTDIR%" rd /s /q "%OUTDIR%"
mkdir "%OUTDIR%"
copy /y "%STAGE%\%APP_NAME%" "%OUTDIR%\" >nul
rd /s /q "%STAGE%"

echo.
echo [OK] Built: %OUTDIR%\%APP_NAME%
popd
endlocal
