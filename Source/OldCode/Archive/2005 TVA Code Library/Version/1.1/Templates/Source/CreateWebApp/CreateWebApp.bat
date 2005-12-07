@ECHO OFF
cscript.exe c:\inetpub\adminscripts\adsutil.vbs CREATE "W3SVC/1/ROOT/%1" "IIsWebDirectory"
cscript.exe c:\inetpub\adminscripts\adsutil.vbs APPCREATEINPROC "W3SVC/1/ROOT/%1"
cscript.exe c:\inetpub\adminscripts\adsutil.vbs SET "W3SVC/1/ROOT/%1/AuthAnonymous" False
cscript.exe c:\inetpub\adminscripts\adsutil.vbs SET "W3SVC/1/ROOT/%1/DefaultDoc" "FrameLoad.aspx"
