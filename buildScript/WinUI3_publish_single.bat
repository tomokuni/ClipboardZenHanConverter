@echo off
setlocal

rem ============================================================
rem  ClipboardZenHanConverter (WinUI 3) 単一 exe ビルドスクリプト
rem  Release / 自己完結 / 単一 exe を生成する
rem
rem  注意: WinUI 3 は Native AOT に未対応です。PublishAot=true は
rem        発行自体は成功しますが、起動時に XAML 初期化で失敗します。
rem ============================================================

rem スクリプトの場所（buildScript）からリポジトリルートへ移動する
pushd "%~dp0.."

rem ターゲットランタイム（既定: win-x64）
if "%~1"=="" (set "RID=win-x64") else (set "RID=%~1")

set "APP_NAME=ClipboardZenHanConverter.App.WinUI3.exe"

rem 最終的な出力先（MewUI 版と分ける）
set "OUTDIR=%CD%\publish\winui3-%RID%-single"

rem 発行の一時ステージング先
rem リポジトリ内へ直接発行するとフレームワーク配置のファイルが出力先へ混在する。
rem また、プロジェクトディレクトリ配下へ発行すると出力がアプリのコンテンツとして
rem 取り込まれ、単一 exe が肥大化する（約 2 倍）。
rem そのためリポジトリ外の一時領域へ発行し、単一 exe のみを出力先へ配置する。
set "STAGE=%TEMP%\czhc-single-winui-%RID%"

echo.
echo [INFO] Mode  : Self-contained single exe
echo [INFO] Target: %RID%
echo [INFO] Output: %OUTDIR%
echo.

if exist "%STAGE%" rd /s /q "%STAGE%"

rem Windows App SDK の単一ファイル要件（WindowsAppSDKSingleFileVerifyConfiguration）:
rem   WindowsPackageType=None（csproj で設定済み）
rem   EnableMsixTooling=true          … アプリの resources.pri 生成に必須
rem   IncludeAllContentForSelfExtract=true … Dll SxS リダイレクトに必須
rem   WindowsAppSDKSelfContained=true / SelfContained=true
rem 初回起動時に依存ファイルが一時ディレクトリへ展開されます。
dotnet publish "src\app_WinUI3\app_WinUI3.csproj" ^
  -c Release ^
  -r "%RID%" ^
  -p:PublishSingleFile=true ^
  -p:SelfContained=true ^
  -p:WindowsAppSDKSelfContained=true ^
  -p:EnableMsixTooling=true ^
  -p:IncludeAllContentForSelfExtract=true ^
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
