These instructions will install a Visual Studio add-in that will create the required common header for all code files within the project. 
Once defined you can insert the common header from within Visual Studio into any file by pressing "Ctrl+Alt+H". 

Installation steps follow:

* First, install the add-in:

  1) Close all instances of Visual Studio 2012 that are currently running.
  2) Find the Visual Studio directory to which Visual Studio add-ins are installed (typically "My Documents\Visual Studio 2012\Addins").
  3) Copy the files "GPACodeHeaderAddIn.AddIn" and "GPACodeHeaderAddIn.dll" into the folder found in step 1.
  4) Start Visual Studio 2012.

* Second, assign a shortcut key to the macro:

  1) Go to “View” / “Toolbars” / “Customize...”
  2) Click “Keyboard...” button in the bottom left corner
  3) Type “GPA” into “Show commands containing:” text box
  4) Select “GPACodeHeaderAddIn.Connect.InsertHeader”
  5) Select “Text Editor” from the “Use new shortcut in:” list
  6) Click in the “Press shortcut keys:” text box
  7) Press the “Ctrl”, “Alt” and “H” key (text box should say Ctrl+Alt+H)
  8) Click the “Assign” button, then click “OK”
  9) Click the “Close” button on the “Customize” window
 10) Open a C# code window, click anywhere in the file
 11) Press Ctrl+Alt+H, header will be inserted at top of document

Note all steps are executed from within Visual Studio. 
These instructions have been tested with Visual Studio 2012.
