@echo off
:: The available verbosity levels are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]
set verbosity=minimal
set buildfile=deploy.proj

if (%configuration% == '') set configuration=Debug

set msbuild=dotnet msbuild
set framework=netframework

:start
cls
echo.
echo            ************************************************************
echo            *                     Build Prompt                         *
echo            ************************************************************
echo.
echo            Build and Copy Server Binaries To Deploy Folder
echo            Configuration: %configuration%
echo            Buildfile:     %buildfile%
echo.
echo            1.  Loadbalancing (Master+Game+NameServer+Plugins)
echo            2   Nameserver
echo.
echo            0.  Exit
echo.

:begin
IF NOT EXIST .\log\ MD .\log
set eof=
set choice=
set target=
set os=win

set /p choice=Enter option (1-0)
if not '%choice%'=='' set choice=%choice:~0,1%
if '%choice%'=='1' set target=loadbalancing
if '%choice%'=='2' set target=nameserver
if '%choice%'=='9' set target=buildall
if '%choice%'=='0' goto eof
if NOT '%target%'=='' goto %target%
if '%eof%'=='' ECHO "%choice%" is not valid please try again
if '%eof%'=='' goto begin
pause
goto start


:loadbalancing
@echo Building LoadBalancing for framework: %framework%
@echo .................................................
%msbuild% %buildfile% /verbosity:%verbosity% /fl /flp:"logfile=log\LoadbalancingBuild.log;verbosity=%verbosity%;performancesummary" /p:Configuration="%configuration%";Framework=%framework%;OS="%os%" /t:%target%
@echo Building LoadBalancing for framework: %framework%
pause
goto start

:nameserver
::build
@echo Building NameServer for framework: %framework%
@echo .................................................
%msbuild% %buildfile% /verbosity:%verbosity% /fl /flp:"logfile=log\%rootpath%Build.log;verbosity=%verbosity%;performancesummary" /p:Configuration="%configuration%";OS="%os%" /t:%target%

@echo Building NameServer for framework: %framework%
pause
goto start

:done
:eof
set eof=1
