@ECHO OFF
cscript.exe c:\inetpub\adminscripts\adsutil.vbs APPDELETE "W3SVC/1/ROOT/%1"
cscript.exe c:\inetpub\adminscripts\adsutil.vbs DELETE "W3SVC/1/ROOT/%1"
