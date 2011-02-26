::*******************************************************************************************************
::  UpdateDependencies.bat - Gbtc
::
::  Tennessee Valley Authority, 2009
::  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
::
::  This software is made freely available under the TVA Open Source Agreement (see below).
::
::  Code Modification History:
::  -----------------------------------------------------------------------------------------------------
::  02/26/2011 - Pinal C. Patel
::       Generated original version of source code.
::
::*******************************************************************************************************

@ECHO OFF

SET vs="%VS100COMNTOOLS%\..\IDE\devenv.com"
SET tfs="%VS100COMNTOOLS%\..\IDE\tf.exe"
SET source="\\GPAWEB\NightlyBuilds\TVACodeLibrary\Beta\Libraries\*.*"
SET target="..\..\Source\Dependencies\TVA"
SET solution="..\..\Source\TimeSeriesFramework.sln"
SET checkinComment="Updated dependencies."
SET /p checkin=Check-in updates (Y or N)? 

ECHO.
ECHO Getting latest version...
%tfs% get %target% /version:T /force /recursive /noprompt

ECHO.
ECHO Checking out dependencies...
%tfs% checkout %target% /recursive /noprompt

ECHO.
ECHO Updating dependencies...
XCOPY %source% %target% /Y

ECHO.
ECHO Building solution...
%vs% %solution% /Build "Release|Any CPU"

IF /I "%checkin%" == "Y" GOTO Checkin
GOTO Finalize

:Checkin
ECHO.
ECHO Checking in dependencies...
%tfs% checkin %target% /noprompt /recursive /comment:%checkinComment%

:Finalize
ECHO.
ECHO Update complete