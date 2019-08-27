@echo off

SET PKGVER=%1
SET APIKEY=%2
SET SOURCE=https://api.nuget.org/v3/index.json

dotnet nuget push Karambolo.ReactiveMvvm.%PKGVER%.nupkg -k %APIKEY% -s %SOURCE%
IF %ERRORLEVEL% NEQ 0 goto:eof

dotnet nuget push Karambolo.ReactiveMvvm.Avalonia.%PKGVER%.nupkg -k %APIKEY% -s %SOURCE%
IF %ERRORLEVEL% NEQ 0 goto:eof

dotnet nuget push Karambolo.ReactiveMvvm.Uwp.%PKGVER%.nupkg -k %APIKEY% -s %SOURCE%
IF %ERRORLEVEL% NEQ 0 goto:eof

dotnet nuget push Karambolo.ReactiveMvvm.WinForms.%PKGVER%.nupkg -k %APIKEY% -s %SOURCE%
IF %ERRORLEVEL% NEQ 0 goto:eof

dotnet nuget push Karambolo.ReactiveMvvm.Wpf.%PKGVER%.nupkg -k %APIKEY% -s %SOURCE%
IF %ERRORLEVEL% NEQ 0 goto:eof

