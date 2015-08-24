@ECHO OFF

REM Handle optional command line parameters
SET TARGET=%1
IF "%TARGET%"=="" SET TARGET=Release

SET BUILDDOCS=%2
IF "%BUILDDOCS%"=="" SET BUILDDOCS=Docs
IF NOT "%TARGET%"=="Release" SET BUILDDOCS=NoDocs

SET CHANGEVER=%3
IF "%CHANGEVER%"=="" SET CHANGEVER=Change

ECHO ๚
ECHO ษออออออออออออออออออออออออออออออออออออออออออออออออออออป
ECHO บ     TVA Code Library 2.0 %TARGET% Build Script      บ
ECHO ศออออออออออออออออออออออออออออออออออออออออออออออออออออผ
ECHO ๚

REM Initialize needed deployment and source paths and other environmental variables
CALL SetPaths.bat

REM Checkout version files and increment build version number
IF "%CHANGEVER%"=="Change" CALL %SUPPORT%IncrementBuildVersion.bat

REM Get the latest source code from SourceSafe
CALL %SUPPORT%GetLatestCode.bat

REM Load Visual Studio 2005 Command Line Variables
CALL "C:\Program Files\Microsoft Visual Studio 8\Common7\Tools\vsvars32.bat" >NUL

REM Compile signed assemblies
CALL %SUPPORT%CompileSignedAssemblies.bat

REM Compile unsigned assemblies
CALL %SUPPORT%CompileUnsignedAssemblies.bat

REM Deploy new build
CALL %SUPPORT%DeployBuild.bat RebuildAll

REM Compile code comments into formal documentation
IF "%BUILDDOCS%"=="Docs" CALL %SUPPORT%CompileDocumentation.bat

ECHO ๚
ECHO ษออออออออออออออออออออออออออออออออออออออออออออออออออออป
ECHO บ     TVA Code Library 2.0 %TARGET% Build Complete    บ
ECHO ศออออออออออออออออออออออออออออออออออออออออออออออออออออผ
ECHO ๚
PAUSE