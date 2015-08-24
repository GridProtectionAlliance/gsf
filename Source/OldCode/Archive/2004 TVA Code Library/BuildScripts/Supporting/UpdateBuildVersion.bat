@ECHO OFF

ECHO ๚
ECHO ษออออออออออออออออออออออออออออออออออออออออออออป
ECHO บ   Updating Build Number in Source Files... บ
ECHO ศออออออออออออออออออออออออออออออออออออออออออออผ
ECHO ๚

%UTILITY%IncrementBuildNumber %SOURCE%Build.ver
CALL %SUPPORT%GetBuildVersion.bat >NUL
%UTILITY%ReplaceInFiles -r -i -x %SOURCE%AssemblyInfo.* "AssemblyVersion(Attribute)?\(&quot;((\*|\d+)\.)+(\*|\d+)&quot;\)" "AssemblyVersion(&quot;%BUILDVER%&quot;)"
%UTILITY%ReplaceInFiles -r -i -x %SOURCE%Config\VariableEvaluator.js "AssemblyVersion(Attribute)?\(&quot;((\*|\d+)\.)+(\*|\d+)&quot;\)" "AssemblyVersion(&quot;%BUILDVER%&quot;)"
%UTILITY%ReplaceInFiles -r -i -x %SOURCE%*.rc "(\d+\.)+\d+" "%BUILDVER%"
%UTILITY%ReplaceInFiles -r -i -x %SOURCE%*.rc "(\d+,)+\d+" "%RCBUILDVER%"