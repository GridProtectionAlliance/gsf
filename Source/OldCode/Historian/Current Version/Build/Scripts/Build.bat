::*******************************************************************************************************
::  Build.bat - Gbtc
::
::  Tennessee Valley Authority, 2009
::  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
::
::  This software is made freely available under the TVA Open Source Agreement (see below).
::
::  Code Modification History:
::  -----------------------------------------------------------------------------------------------------
::  09/05/2009 - Pinal C. Patel
::       Generated original version of source code.
::
::*******************************************************************************************************

@ECHO OFF
:: Passing in "false" for argument #1 will cause the build to take place in unattended mode.
C:\WINDOWS\Microsoft.NET\Framework\v3.5\msbuild.exe Historian.buildproj /p:BuildInteractive=%1 /l:FileLogger,Microsoft.Build.Engine;logfile=Historian.output
IF NOT "%1" == "false" PAUSE