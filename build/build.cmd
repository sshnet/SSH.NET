@echo off

set MSBUILD14_EXE=%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe
set MSBUILD15_EXE=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\\MSBuild\15.0\bin\MSBuild.exe

call "%MSBUILD14_EXE%" build.proj /t:Clean
call "%MSBUILD15_EXE%" build.proj /t:Clean

call "%MSBUILD14_EXE%" build.proj /t:Build
call "%MSBUILD15_EXE%" build.proj /t:Build

call "%MSBUILD15_EXE%" build.proj /t:Package /p:ReleaseVersion=%1