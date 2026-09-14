@echo off
setlocal

rem ============================================================
rem  ClipboardZenHanConverter (MewUI) 単一 exe ビルドスクリプト
rem  Release / Native AOT / 単一 exe を生成する
rem ============================================================

rem スクリプトの場所を基準にリポジトリルートを取得する
pushd "%~dp0"

rem ターゲットランタイム（既定: win-x64）
if "%~1"=="" (set "RID=win-x64") else (set "RID=%~1")

set "APP_NAME=ClipboardZenHanConverter.App.MewUI.exe"

rem 最終的な出力先
set "OUTDIR=%CD%\publish\mewui-%RID%-singleaot"

rem 発行の一時ステージング先
rem リポジトリ内へ直接発行すると、参照プロジェクト側のディレクトリにも publish が作られ、
rem さらに自己完結ランタイムのファイルが出力先へ混在する。
rem そのためリポジトリ外の一時領域へ発行し、単一 exe のみを出力先へ配置する。
set "STAGE=%TEMP%\czhc-single-mew-%RID%"

echo.
echo [INFO] Mode  : Native AOT
echo [INFO] Target: %RID%
echo [INFO] Output: %OUTDIR%
echo.

if exist "%STAGE%" rd /s /q "%STAGE%"

rem Native AOT の要件:
rem   PublishAot=true     … IL をネイティブコードへ事前コンパイル（起動が速く、.NET ランタイム不要）
rem   StripSymbols=true   … ネイティブシンボルを exe から除去する
rem 速度を優先してコンパイルします（OptimizationPreference は既定の Speed）。
dotnet publish "src\app_MewUI\app_MewUI.csproj" ^
  -c Release ^
  -r "%RID%" ^
  -p:PublishAot=true ^
  -p:StripSymbols=true ^
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
