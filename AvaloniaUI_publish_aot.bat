@echo off
setlocal

rem ============================================================
rem  ClipboardZenHanConverter (Avalonia UI) AOT ビルドスクリプト
rem  Release / Native AOT / 自己完結でビルドする（単一 exe にはしない）
rem
rem  方式: Native AOT + SkiaSharp のネイティブ DLL（exe 1 個 + DLL 3 個）
rem
rem  注意: Avalonia UI は Native AOT 自体は動作しますが、単一 exe にはできません。
rem        Avalonia の描画は SkiaSharp のネイティブライブラリ（libSkiaSharp 等）に依存し、
rem        Native AOT はネイティブライブラリを exe へ同梱できないため、exe の隣に
rem        libSkiaSharp.dll / av_libglesv2.dll / libHarfBuzzSharp.dll が必要になります。
rem        IncludeNativeLibrariesForSelfExtract=true を併用しても同梱されません
rem        （exe 単体で起動すると DllNotFoundException: libSkiaSharp で失敗することを実測で確認済み）。
rem        そのため単一ファイル化は行わず、exe とネイティブ DLL を並べて配置します。
rem ============================================================

rem スクリプトの場所を基準にリポジトリルートを取得する
pushd "%~dp0"

rem ターゲットランタイム（既定: win-x64）
if "%~1"=="" (set "RID=win-x64") else (set "RID=%~1")

set "APP_NAME=ClipboardZenHanConverter.App.AvaloniaUI.exe"

rem 最終的な出力先（MewUI 版 / WinUI 3 版と分ける）
set "OUTDIR=%CD%\publish\avaloniaui-%RID%-aot"

rem 発行の一時ステージング先
rem リポジトリ内へ直接発行するとフレームワーク配置のファイルが出力先へ混在する。
rem また、プロジェクトディレクトリ配下へ発行すると出力がアプリのコンテンツとして
rem 取り込まれ、出力が肥大化する。
rem そのためリポジトリ外の一時領域へ発行し、必要なファイルのみを出力先へ配置する。
set "STAGE=%TEMP%\czhc-aot-avalonia-%RID%"

echo.
echo [INFO] Mode  : Native AOT + Skia native DLLs (self-contained)
echo [INFO] Target: %RID%
echo [INFO] Output: %OUTDIR%
echo.

if exist "%STAGE%" rd /s /q "%STAGE%"

rem Native AOT の要件:
rem   PublishAot=true       … IL をネイティブコードへ事前コンパイル（起動が速く、.NET ランタイム不要）
rem   StripSymbols=true     … ネイティブシンボルを exe から除去する
rem   SelfContained=true    … 配布先に .NET ランタイムを要求しない
rem 速度を優先してコンパイルします（OptimizationPreference は既定の Speed）。
rem exe の隣に SkiaSharp 等のネイティブ DLL が必要になるため、フォルダ単位で配布します。
dotnet publish "src\app_AvaloniaUI\app_AvaloniaUI.csproj" ^
  -c Release ^
  -r "%RID%" ^
  -p:PublishAot=true ^
  -p:StripSymbols=true ^
  -p:SelfContained=true ^
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

rem 出力先を作り直し、exe とネイティブ DLL のみを配置する（.pdb は配布対象外）
if exist "%OUTDIR%" rd /s /q "%OUTDIR%"
mkdir "%OUTDIR%"
copy /y "%STAGE%\*.exe" "%OUTDIR%\" >nul
copy /y "%STAGE%\*.dll" "%OUTDIR%\" >nul
rd /s /q "%STAGE%"

echo.
echo [OK] Built: %OUTDIR%\%APP_NAME%
echo [INFO] 実行には同じフォルダーのネイティブ DLL が必要です（フォルダー単位で配布してください）。
popd
endlocal
