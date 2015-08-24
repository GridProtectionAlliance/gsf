You have opened the production TVA code library source code archive.  Do *not* make any changes to
code here without coordinating with the ESO Dot Net Reusability Committee.

Please make sure you leave this file open in Visual Studio before you close the solution so that anyone
who opens the solution from the shared location we see this message.

============================================================================================================

To start using the code library, reference the latest assemblies in the following folder:

	\\opdat\opdat\CtrlSys\CodeLibrary\Builds\Release\Current
 
Note that many of the assemblies contain designable components that can be added to your component list in
Visual Studio.  The compiled assemblies found in the current release build folder will always be tested and
considered reliable.  The absolute latest build can be found in the beta build folder:

	\\opdat\opdat\CtrlSys\CodeLibrary\Builds\Beta

Note that the beta build will likely be untested code and you should only use this code if you absolutely
need a need a feature or bug fix that this code provides.

Also, debug builds can built as needed - these are not "official" releases and should only be used if you
suspect there is a bug in the code library and you need to step through the code.  Debug builds will be
located in the following folder:

	\\opdat\opdat\CtrlSys\CodeLibrary\Builds\Debug

============================================================================================================

Build Notes:

Code library builds are not compiled from within Visual Studio - they are compiled from build scripts
located in the \\opdat\opdat\CtrlSys\CodeLibrary\BuildScripts folder.  They only time you would compile
a build from within Visual Studio is to debug the build process (for example, you added a new project
to the library solution and you want to make sure it compiles).  Never reference assemblies directly
from the Source\bin folder, instead, always run the desired build script and reference assemblies from
the predefined build destinations.

Note: in order to successfully run the build script you will need to install the WinZip command line
utility.  This can be downloaded from http://www.winzip.com/downcl.htm


Compiling a DEBUG build of the code library

	Run the following build script: RebuildAllDebug.bat
	Reference assemblies from the Builds\Debug folder

Compiling a BETA build of the code library

	Run the following build script: RebuildAll.bat
	Reference assemblies from the Builds\Beta folder

Promoting a BETA build to PRODUCTION

	Execute all available test scripts in the code library test bed:

		Run test bed application from within VS (Testing\TestBed.sln) to
		make sure latest BETA build files are being tested

	If all tests were successful, then given

		(M) = the new major version number of this build and
		(N) = the new minor version number of this build

		perform the following steps:

		1) Copy TVACodeLibraryBeta.zip found in folder Builds\Archive\ to TVACodeLbrary(M).(N).zip

		2) Set the "Read Only" attribute on the new TVACodeLbrary(M).(N).zip archive file

		3) Copy the Builds\Beta folder into the Builds\Release folder so that you have a new
			sub folder in Builds\Release called Beta (i.e., Builds\Release\Beta)

		4) Rename the Builds\Release\Beta folder to Builds\Release\(M).(N)

		5) Delete the file called "README!.TXT" from the new Builds\Release\(M).(N) folder

		6) Set the "Read Only" attribute on the new Builds\Release\(M).(N) folder, when prompted
			make sure to apply attribute changes to folder, subfolder and files

		7) Copy all the new assemblies found in the new Builds\Release\(M).(N) folder to the
			current release folder (Builds\Release\Current)

		8) Send a notification to all interested parties about the new build making sure to
			inform them that they may need to restart VS because of production updates.
			Typically we include a detailed summary of notable changes in this e-mail so
			it's best to have this e-mail ready in advance

		9) Increment the major and/or minor build numbers in the file called Build.ver found in
			the Source folder so that any new builds will have a new build version

		10) Define a new timestamp for builds that matches the new major and minor build
			numbers setup in step 9 in the file called Build.ts also found in the
			Source folder

============================================================================================================

Ritchie
