@echo off
setlocal
chcp 65001 >nul

rem ============================================================
rem  ZenHanConverter (MewUI) publish script
rem
rem  ASCII only.  cmd.exe reads a batch file with the console code page and counts the read
rem  position in decoded characters, so non-ASCII text garbles the output and can make cmd
rem  execute fragments of the file as commands.  The chcp below keeps the UTF-8 log of
rem  dotnet readable (safe because this file is ASCII).  Rules: .github/RELEASE.md.
rem
rem  The mode (aot > single > exe) is chosen by buildScript/publish_ui.bat.
rem
rem  Prerequisite: linking for Native AOT needs link.exe from the Visual Studio C++ build
rem  tools.
rem
rem  Layout: all managed code is compiled into one exe.  The native libraries of SkiaSharp
rem  cannot be embedded into the exe, so they are distributed next to it (the file count and
rem  the sizes change with the code, so they are not written here; the publish output is the
rem  reference).
rem ============================================================

rem Move from the script folder (buildScript) to the repository root.
pushd "%~dp0.."
call "%~dp0publish_ui.bat" mewui "src\App.MewUI\App.MewUI.csproj" %*
set "RESULT=%ERRORLEVEL%"
popd
exit /b %RESULT%
