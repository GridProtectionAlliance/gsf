::*******************************************************************************************************
::  UpdateDebugDependencies.bat - Gbtc
::
::  Tennessee Valley Authority, 2009
::  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
::
::  This software is made freely available under the TVA Open Source Agreement (see below).
::
::  Code Modification History:
::  -----------------------------------------------------------------------------------------------------
::  11/06/2009 - J. Ritchie Carroll
::       Generated original version of source code.
::
::*******************************************************************************************************

@ECHO OFF
COPY "..\..\..\..\Framework\Current Version\Build\Output\Debug\Libraries\TVA.Communication.*" "..\..\Source\Dependencies\TVA\"
COPY "..\..\..\..\Framework\Current Version\Build\Output\Debug\Libraries\TVA.Core.*" "..\..\Source\Dependencies\TVA\"
COPY "..\..\..\..\Framework\Current Version\Build\Output\Debug\Libraries\TVA.Web.*" "..\..\Source\Dependencies\TVA\"
PAUSE