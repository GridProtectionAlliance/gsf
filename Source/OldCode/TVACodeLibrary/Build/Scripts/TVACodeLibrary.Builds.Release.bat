::*******************************************************************************************************
::  TVACodeLibrary.Builds.Beta.bat - Gbtc
::
::  Tennessee Valley Authority, 2009
::  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
::
::  This software is made freely available under the TVA Open Source Agreement (see below).
::
::  Code Modification History:
::  -----------------------------------------------------------------------------------------------------
::  06/05/2012 - Pinal C. Patel
::       Generated original version of source code.
::
::*******************************************************************************************************

@ECHO OFF
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\msbuild.exe TVACodeLibrary.buildproj /p:ForceBuild=true;PreRelease=false /l:FileLogger,Microsoft.Build.Engine;logfile=TVACodeLibrary.output
PAUSE
