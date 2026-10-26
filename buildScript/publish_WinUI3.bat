@echo off
setlocal
chcp 65001 >nul

rem ============================================================
rem  ZenHanConverter (WinUI 3) publish script
rem
rem  ASCII only.  cmd.exe reads a batch file with the console code page and counts the read
rem  position in decoded characters, so non-ASCII text garbles the output and can make cmd
rem  execute fragments of the file as commands.  The chcp below keeps the UTF-8 log of
rem  dotnet readable (safe because this file is ASCII).  Rules: .github/RELEASE.md.
rem
rem  The mode is restricted to `single`: Native AOT is not supported by WinUI 3 for this app
rem  (the publish succeeds but the XAML initialization fails at startup; measured).
rem
rem  Single file publishing is gated by `ZenHanConverterPublishSingleFile` in
rem  src\App.WinUI3\App.WinUI3.csproj.  Passing `-p:PublishSingleFile=true` on the command
rem  line flows to the referenced projects and fails there (measured), so the per-UI switch is
rem  used instead.
rem
rem  Layout: one self contained exe (the Windows App SDK runtime is bundled).
rem ============================================================

rem Move from the script folder (buildScript) to the repository root.
pushd "%~dp0.."
set "SE_MODES=single"
set "SE_SINGLE_ARGS=-p:PublishSingleFile=true -p:EnableMsixTooling=true -p:IncludeAllContentForSelfExtract=true"

call "%~dp0publish_ui.bat" winui3 "src\App.WinUI3\App.WinUI3.csproj" %*
set "RESULT=%ERRORLEVEL%"
popd
exit /b %RESULT%
