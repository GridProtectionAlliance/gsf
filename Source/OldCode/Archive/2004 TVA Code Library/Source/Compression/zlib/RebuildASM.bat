@ECHO OFF
IF "%1"=="" GOTO INITVARS
IF "%1"=="InitVars" GOTO INITVARS
GOTO COMPILE
:INITVARS
CALL "c:\Program Files\Microsoft Visual Studio .NET\Common7\Tools\vsvars32.bat" >NUL
:COMPILE
ECHO ๚
ECHO ษออออออออออออออออออออออออออออออออออออออออออออป
ECHO บ           Compiling gvmat32.asm...         บ
ECHO ศออออออออออออออออออออออออออออออออออออออออออออผ
ECHO ๚
ml /coff /Zi /c gvmat32.asm
IF "%1"=="" GOTO DOPAUSE
IF "%1"=="InitVars" GOTO DOPAUSE
GOTO CONTINUE
:DOPAUSE
ECHO ๚
ECHO If no errors appear, then compile was successful.
ECHO ๚
PAUSE
:CONTINUE
