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
SET source1="\\GPAWEB\NightlyBuilds\TVACodeLibrary\Beta\Libraries\*.*"
SET target1="..\..\Source\Dependencies\TVA"
SET source2="\\GPAWEB\NightlyBuilds\TimeSeriesFramework\Beta\Libraries\TimeSeriesFramework.*"
SET target2="..\..\Source\Dependencies\TimeSeriesFramework"
SET solution="..\..\Source\Historian.sln"
SET /p checkin=Check-in updates (Y or N)? 

ECHO.
ECHO Getting latest version...
%tfs% get %target1% /version:T /force /recursive /noprompt
%tfs% get %target2% /version:T /force /recursive /noprompt

ECHO.
ECHO Checking out dependencies...
%tfs% checkout %target1% /recursive /noprompt
%tfs% checkout %target2% /recursive /noprompt

ECHO.
ECHO Updating dependencies...
XCOPY %source1% %target1% /Y
XCOPY %source2% %target2% /Y

ECHO.
ECHO Building solution...
%vs% %solution% /Build "Release|Any CPU"

IF /I "%checkin%" == "Y" GOTO Checkin
GOTO Finalize

:Checkin
ECHO.
ECHO Checking in dependencies...
%tfs% checkin %target1% /noprompt /recursive /comment:"Historian: Updated code library dependencies."
%tfs% checkin %target2% /noprompt /recursive /comment:"Historian: Updated time-series framework dependencies."

:Finalize
ECHO.
ECHO Update complete