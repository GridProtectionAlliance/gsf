@ECHO OFF
COPY %SUPPORT%SetBuildVersion.base /B + %SOURCE%Build.ver /B %SUPPORT%SetBuildVersion.bat
CALL %SUPPORT%SetBuildVersion.bat

COPY %SUPPORT%SetBuildVersion.bat %SUPPORT%SetRCBuildVersion.bat
%UTILITY%ReplaceInFiles %SUPPORT%SetRCBuildVersion.bat BUILDVER RCBUILDVER
%UTILITY%ReplaceInFiles %SUPPORT%SetRCBuildVersion.bat "." ","
CALL %SUPPORT%SetRCBuildVersion.bat

DEL %SUPPORT%SetBuildVersion.bat
DEL %SUPPORT%SetRCBuildVersion.bat