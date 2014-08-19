::*******************************************************************************************************
::  BuildNightly.bat - Gbtc
::
::  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
::
::  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
::  the NOTICE file distributed with this work for additional information regarding copyright ownership.
::  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
::  not use this file except in compliance with the License. You may obtain a copy of the License at:
::
::      http://www.opensource.org/licenses/eclipse-1.0.php
::
::  Unless agreed to in writing, the subject software distributed under the License is distributed on an
::  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
::  License for the specific language governing permissions and limitations.
::
::  Code Modification History:
::  -----------------------------------------------------------------------------------------------------
::  10/20/2009 - Pinal C. Patel
::       Generated original version of source code.
::  09/14/2010 - Mihir Brahmbhatt
::		 Change Framework path from v3.5 to v4.0
::  10/03/2010 - Pinal C. Patel
::       Updated to use MSBuild 4.0.
::
::*******************************************************************************************************

echo off

setlocal EnableDelayedExpansion

set show_help=0
if [%1]==[] set show_help=1
if "%1"=="/?" set show_help=1

if %show_help%==1  (
	echo.
	echo.
	echo Help File:
	echo Explain Parameters
	echo.
	echo 	/u = TFS User Name
	echo 	/p = TFS Password
	echo 	/d = Build Deploy Location (required)
	echo 	/pkg = Nuget Package Location (required)
	echo 	/l = Logger
	echo.
	echo.
)

set tfsUser_name=""
set tfsPassword=""
set deploy=""
set package=""
set logger=""

set %%y=""
set item=""

set set_user=0
set set_pass=0
set set_deploy=0
set set_package=0
set set_log=0

for %%y in (%*) do (
	
	rem Set TFS User Name 
	if !set_user!==1 (
		set tfsUser_name=%%y
		set set_user=0
	)
	if %%y==/u (
		set set_user=1
	)
	
	rem Set TFS Password 
	if !set_pass!==1 (
		set tfsPassword=%%y
		set set_pass=0
	)
	if %%y==/p (
		set set_pass=1
	)
	
	rem Set Build Deploy Folder
		if !set_deploy!==1 (
		set deploy=%%y
		set set_deploy=0
	)
	if %%y==/d (
		set set_deploy=1
	)
	
	rem Set Nuget Package Location
		if !set_package!==1 (
		set package=%%y
		set package=%%y
		set set_package=0
	)
	if %%y==/pkg (
		set set_package=1
		)
		
	rem Set Logger
		if !set_log!==1 (
		set logger=%%y
		set set_log=0
	)
	if %%y==/l (
		set set_log=1
		)
)

C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\msbuild.exe GridSolutionsFramework.buildproj /p:BuildDeployFolder=%deploy% /p:NugetApiKey=915f11e2-62dc-4680-8500-ceb11116142a /p:NugetPackagesFolder=%package% /p:TfsUserName=%tfsUser_name% /p:TfsPassword=%tfsPassword% /p:ForceBuild=true /l:FileLogger,Microsoft.Build.Engine;logfile=%logger%  

endlocal
