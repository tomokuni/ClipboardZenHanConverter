@echo off
setlocal
chcp 65001 >nul

rem ============================================================
rem  ZenHanConverter (Avalonia UI) publish script
rem
rem  ASCII only.  cmd.exe reads a batch file with the console code page and counts the read
rem  position in decoded characters, so non-ASCII text garbles the output and can make cmd
rem  execute fragments of the file as commands.  The chcp below keeps the UTF-8 log of
rem  dotnet readable (safe because this file is ASCII).  Rules: .github/RELEASE.md.
rem
rem  The mode (aot > single > exe) is chosen by buildScript/publish_ui.bat.
rem
rem  Layout: Native AOT produces one exe plus the native libraries of Avalonia (Skia and the
rem  Win32 backend).  The distribution is a folder (exe + native DLLs), not a single file.
rem ============================================================

rem Move from the script folder (buildScript) to the repository root.
pushd "%~dp0.."
call "%~dp0publish_ui.bat" avaloniaui "src\app_AvaloniaUI\app_AvaloniaUI.csproj" %*
set "RESULT=%ERRORLEVEL%"
popd
exit /b %RESULT%
