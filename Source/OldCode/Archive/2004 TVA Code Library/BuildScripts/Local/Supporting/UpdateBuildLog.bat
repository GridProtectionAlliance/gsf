@ECHO OFF

ECHO ๚
ECHO ษออออออออออออออออออออออออออออออออออออออออออออป
ECHO บ            Updating Build Log...           บ
ECHO ศออออออออออออออออออออออออออออออออออออออออออออผ
ECHO ๚

COPY /Y %DEPLOYPATH%BuildScripts\Build.log Build.log

ECHO Build %BUILDVER% compiled at %DATE% %TIME% by %USERDOMAIN%\%USERNAME% from %COMPUTERNAME% >>Build.log

COPY /Y Build.log %DEPLOYPATH%BuildScripts\Build.log
