@echo off

rem We use the version of MSBuild that is included in .NET 4.0 / 4.5
set MSBUILD_HOME=%WINDIR%\Microsoft.NET\Framework\v4.0.30319

rem MSBuild project file is located in the same directory as the current script
set MSBUILD_PROJECT=%~dp0\build.proj

set MSBUILD_EXE=%MSBUILD_HOME%\msbuild.exe

rem verify whether the version of MSBuild that we need is installed
if not exist %MSBUILD_EXE% goto msbuild_missing

rem launch the build script
call %MSBUILD_HOME%\msbuild.exe %MSBUILD_PROJECT% /verbosity:minimal /nologo
if errorlevel 1 goto msbuild_failure
goto :EOF

:msbuild_missing
echo.
echo Build FAILED.
echo.
echo MSBuild is not available in the following directory:
echo %MSBUILD_HOME%
echo.
echo Please check your local setup.
goto :EOF

:msbuild_failure
echo.
echo Build FAILED.
echo.
echo Please check the MSBuild output for information on the cause.
