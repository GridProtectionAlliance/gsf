@ECHO OFF

ECHO ๚
ECHO ษออออออออออออออออออออออออออออออออออออออออออออป
ECHO บ    Updating Timestamps of Assemblies...    บ
ECHO ศออออออออออออออออออออออออออออออออออออออออออออผ
ECHO ๚

COPY %SUPPORT%SetTimeStamp.base /B + %SOURCE%Build.ts /B %SUPPORT%SetTimeStamp.bat >NUL
CALL %SUPPORT%SetTimeStamp.bat >NUL
DEL %SUPPORT%SetTimeStamp.bat >NUL

%UTILITY%UpdateTimeStamp -c -v %SOURCE%bin\TVA*.* "%TIMESTAMP%"
%UTILITY%UpdateTimeStamp -c -v %SOURCE%bin\Xset*.* "%TIMESTAMP%"