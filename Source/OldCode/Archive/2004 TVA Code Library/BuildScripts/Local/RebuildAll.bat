@ECHO OFF

SET TARGET=%1
IF "%TARGET%"=="" SET TARGET=Release

REM Batch scripts have to be run from a drive mapping :p
ECHO ๚
ECHO ษออออออออออออออออออออออออออออออออออออออออออออป
ECHO บ    TVA Code Library %TARGET% Build Script   บ
ECHO ศออออออออออออออออออออออออออออออออออออออออออออผ
ECHO ๚
ECHO ๚
ECHO This build script needs to run from a mapped drive.  If you did not start
ECHO this script from a mapped drive, cancel this script now
ECHO ๚

CALL SetPaths.bat

REM Download current source code
REM CALL %SUPPORT%DownloadToLocal.bat

REM Update build version
CALL %SUPPORT%UpdateBuildVersion.bat

REM Log this build
CALL %SUPPORT%UpdateBuildLog.bat

REM Delete all existing assemblies and temporary compiler files
CALL %SUPPORT%CleanAll.bat RebuildAll

REM Load Visual Studio 2003 Command Line Variables
CALL "c:\Program Files\Microsoft Visual Studio .NET 2003\Common7\Tools\vsvars32.bat" >NUL

REM Compile JScript.NET based VariableEvaluator.js
CALL %SOURCE%Config\RebuildVarEval.bat RebuildAll %TARGET%

ECHO ๚
ECHO ษออออออออออออออออออออออออออออออออออออออออออออป
ECHO บ Compiling TVA Code Library for .NET 1.1... บ
ECHO ศออออออออออออออออออออออออออออออออออออออออออออผ
ECHO ๚

REM Rebuild .NET 1.1 Project Components
devenv %SOURCE%TVA.sln /rebuild %TARGET%
DEL %SOURCE%bin\*.BAT >NUL
DEL %SOURCE%bin\*.XML >NUL

REM Update timestamps of new assemblies
CALL %SUPPORT%UpdateTimeStamps.bat

REM Deploy new build
CALL %SUPPORT%DeployBuild.bat RebuildAll
GOTO RebuildComplete

:NotDeployed
ECHO ๚
ECHO NOTE: Debug builds are not automatically deployed.

:RebuildComplete
ECHO ๚
ECHO ษออออออออออออออออออออออออออออออออออออออออออออป
ECHO บ  TVA Code Library %TARGET% Build Complete   บ
ECHO ศออออออออออออออออออออออออออออออออออออออออออออผ
ECHO ๚
PAUSE