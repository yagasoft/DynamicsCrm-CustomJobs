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
@ECHO ! Installing service ...
SC CREATE "CustomJobService" binpath= "%~dp0Yagasoft.CustomJobs.Service.exe" displayname= "Yagasoft Dynamics Custom Jobs" start= delayed-auto error= normal
@ECHO:
@ECHO ! Setting service description ...
SC DESCRIPTION "CustomJobService" "Trigger Custom Jobs in Dynamics CRM. Created by Ahmed Elsawalhy (Yagasoft.com)."
@ECHO:
@ECHO:
PAUSE
