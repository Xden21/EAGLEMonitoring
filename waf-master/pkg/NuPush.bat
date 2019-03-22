@echo off
setlocal
set PkgDir=%~dp0
set PATH=%PATH%;%PkgDir%
set Version=5.0.0-alpha2

cd %PkgDir%\System.Waf\Release\%Version%

nuget Push System.Waf.Core.%Version%.nupkg -Source https://api.nuget.org/v3/index.json
nuget Push System.Waf.UnitTesting.Core.%Version%.nupkg -Source https://api.nuget.org/v3/index.json
nuget Push System.Waf.Wpf.%Version%.nupkg -Source https://api.nuget.org/v3/index.json
nuget Push System.Waf.UnitTesting.Wpf.%Version%.nupkg -Source https://api.nuget.org/v3/index.json

rem nuget Push System.Waf.Uwp.4.1.0-alpha8.nupkg -Source https://api.nuget.org/v3/index.json