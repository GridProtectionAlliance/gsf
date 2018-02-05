These instructions will install a Visual Studio extension that will create the required common header for all code files within the project. 
Once defined you can insert the common header from within Visual Studio into any file by pressing "Ctrl+Alt+H". 

Installation steps follow:

* First, install the extension:

  1) Close all instances of Visual Studio that are currently running
  2) Double-click the GPAVSExtension#VER#.vsix file to begin the installation
  3) Click the "Install" button to install the extension
  4) Start Visual Studio 2015

* Second, assign a shortcut key to the InsertHeader command:

  1) Go to “View” / “Toolbars” / “Customize...”
  2) Click “Keyboard...” button in the bottom left corner
  3) Type “GPA” into “Show commands containing:” text box
  4) Select “EditorContextMenus.CodeWindow.GPA.InsertHeader”
  5) Select “Text Editor” from the “Use new shortcut in:” list
  6) Click in the “Press shortcut keys:” text box
  7) Press the “Ctrl”, “Alt” and “H” key (text box should say Ctrl+Alt+H)
  8) Click the “Assign” button, then click “OK”
  9) Click the “Close” button on the “Customize” window
 10) Open a C# code window, click anywhere in the file
 11) Press Ctrl+Alt+H, header will be inserted at top of document

These instructions have been tested with Visual Studio 2015.
