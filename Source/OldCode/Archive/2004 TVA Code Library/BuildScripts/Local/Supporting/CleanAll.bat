@ECHO OFF
ECHO ๚
ECHO ษออออออออออออออออออออออออออออออออออออออออออออป
ECHO บ         Removing Temporary Files...        บ
ECHO ศออออออออออออออออออออออออออออออออออออออออออออผ
ECHO ๚

ECHO OneMustExist >%SOURCE%TVATEMP.PDB
ECHO OneMustExist >%SOURCE%TVATEMP.DLL
ECHO OneMustExist >%SOURCE%TVATEMP.EXE
ECHO OneMustExist >%SOURCE%TVATEMP.USER
DEL %SOURCE%TVA*.PDB /S >NUL
DEL %SOURCE%TVA*.DLL /A-R /S >NUL
DEL %SOURCE%TVA*.EXE >NUL
DEL %SOURCE%*.USER /S >NUL

IF "%1"=="" GOTO DOPAUSE
GOTO CONTINUE

:DOPAUSE
ECHO ๚
ECHO Temporary files removed.
ECHO ๚
PAUSE

:CONTINUE
