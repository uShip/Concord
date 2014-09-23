@echo off
for /f %%i in ('dir C:\inetpub\wwwroot\uShipDeploy\uShip.RestTests\packages\concord* /s /b /ad') do set concordDir=%%i
echo %concordDir%

set SOURCEPATH=.\concord\bin\Debug\
set TARGETPATH=%concordDir%\tools\

copy %SOURCEPATH%concord.exe %TARGETPATH%
if not errorlevel 0 pause
copy %SOURCEPATH%concord.pdb %TARGETPATH%
if not errorlevel 0 pause

xcopy %SOURCEPATH%*.* %TARGETPATH% /Y
