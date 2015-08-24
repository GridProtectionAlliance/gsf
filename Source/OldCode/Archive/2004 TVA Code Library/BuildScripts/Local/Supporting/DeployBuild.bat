@ECHO OFF
ECHO ๚
ECHO ษออออออออออออออออออออออออออออออออออออออออออออป
ECHO บ      Removing Intermediate Files...        บ
ECHO ศออออออออออออออออออออออออออออออออออออออออออออผ
ECHO ๚

ECHO OneMustExist >%SOURCE%TEMP.ILK
ECHO OneMustExist >%SOURCE%TEMP.SBR
ECHO OneMustExist >%SOURCE%TEMP.OBJ
ECHO OneMustExist >%SOURCE%TEMP.COD
DEL %SOURCE%*.ILK /S >NUL
DEL %SOURCE%*.SBR /S >NUL
DEL %SOURCE%*.OBJ /S >NUL
DEL %SOURCE%*.COD /S >NUL

ECHO ๚
ECHO ษออออออออออออออออออออออออออออออออออออออออออออป
ECHO บ      Deploying TVACodeLibrary Build...     บ
ECHO ศออออออออออออออออออออออออออออออออออออออออออออผ
ECHO ๚

REM Determine proper deployment target
IF "%TARGET%"=="Debug" SET DEPLOYTARGET=Debug
IF "%TARGET%"=="Release" SET DEPLOYTARGET=Beta
IF "%DEPLOYTARGET%"=="" SET DEPLOYTARGET=Beta

REM Create a zip archive of the source code
REM %UTILITY%WZZIP.EXE -ex -o -p -r -u %ARCHIVEPATH%TVACodeLibrary%DEPLOYTARGET%.zip %SOURCE%*.*
%UTILITY%WZZIP.EXE -ex -o -p -r -u ..\..\TVACodeLibrary%DEPLOYTARGET%.zip %SOURCE%*.*

REM Make a raw backup of the source code...
REM XCOPY %SOURCE%*.* %ARCHIVEPATH%Backup\*.* /E /V /C /H /R /K /Y

REM Deploy build assemblies...
XCOPY %SOURCE%bin\*.* %DEPLOYPATH%Builds\%DEPLOYTARGET%\*.* /E /V /C /H /R /K /Y

IF "%1"=="" GOTO DOPAUSE
GOTO CONTINUE

:DOPAUSE
ECHO ๚
ECHO Build deployment complete.
ECHO ๚
PAUSE

:CONTINUE
