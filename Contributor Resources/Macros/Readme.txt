These instructions will create a Visual Studio macro that will create the required common header for all files included within the openPDC project. Once defined you can insert the common header from within Visual Studio into any file by pressing "Ctrl+Alt+H". Installation steps follow:

First, install the project macro:

1) Go to “Tools” / “Macros” / “Macros IDE”
2) Right click “MyMacros” and select "Add" / “Add Module”
3) Enter name “ProjectMacros”
3) Select all default code by pressing "Ctrl+A"
5) Press the "delete" key to remove all existing code
4) Paste in code from the "ProjectMacros.vb" found in this folder
5) Right click “References” in the Project Explorer and select “Add Reference”
6) Select “System.DirectoryServices.dll”
7) Select “File” / “Save MyMacros”
8) Close “Macros IDE”

Second, assign a shortcut key to the macro:

1) Go to “View” / “Toolbars” / “Customize...”
2) Click “Keyboard...” button in the bottom left corner
3) Type “MyMacros” into “Show commands containing:” text box
4) Select “Macros.MyMacros.ProjectMacros.InsertHeader”
5) Select “Text Editor” from the “Use new shortcut in:” list
6) Click in the “Press shortcut keys:” text box
7) Press the “Ctrl”, “Alt” and “H” key (text box should say Ctrl+Alt+H)
8) Click the “Assign” button, then click “OK”
9) Click the “Close” button on the “Customize” window
10) Open a C# code window, click anywhere in the file
11) Press Ctrl+Alt+H, header will be inserted at top of document

Note all steps are executed from within Visual Studio. These instructions have only been tested with Visual Studio 2008.
