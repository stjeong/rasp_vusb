

REM In order to run this script, make /share directory in Raspberry PI "sudo mkdir -m 1777 /share"

IF '%1' == '' GOTO ERROR_ARG

SET PIADDR=%1
SET CURRENT_VERSION=v1_0_0_3

FOR /F %%I IN ("%0") DO SET CURRENTDIR=%%~dpI

pscp %CURRENTDIR%\*.* pi@%PIADDR%:/share
if errorlevel 1 GOTO ERROR_PSCP

pscp %CURRENTDIR%\..\bin\%CURRENT_VERSION%\rasp_vusb_server.out pi@%PIADDR%:/share
if errorlevel 1 GOTO ERROR_PSCP

GOTO END_OF_SCRIPT

:ERROR_ARG
echo deploy_with_pscp [pi_ip_address]
GOTO END_OF_SCRIPT

:ERROR_PSCP
echo Error occurred!

:END_OF_SCRIPT
