@ECHO OFF
@ECHO ################################################
@ECHO ## Dynamics 365 Custom Jobs Service Installer ##
@ECHO ##       Ahmed Elsawalhy (Yagasoft.com)       ##
@ECHO ################################################
@ECHO:
@ECHO:
@ECHO !!! Make sure to run this script as admin. !!!
@ECHO:
@ECHO:
@ECHO:
@ECHO ! Removing existing installation ...
SC DELETE "CustomJobService"
@ECHO:
@ECHO:
PAUSE
