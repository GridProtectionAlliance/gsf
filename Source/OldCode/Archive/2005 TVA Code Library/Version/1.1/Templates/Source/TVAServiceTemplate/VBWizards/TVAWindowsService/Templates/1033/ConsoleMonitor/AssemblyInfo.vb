'***********************************************************************
'  AssemblyInfo.vb - TVA Service Template
'  Copyright © [!output CURR_YEAR] - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: [!output DEV_NAME]
'      Office: [!output DEV_OFFICE]
'       Phone: [!output DEV_PHONE]
'       Email: [!output DEV_EMAIL]
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  [!output CURR_DATE] - [!output USER_NAME]
'       Initial version of source generated for new Windows service
'       project "[!output PROJECT_NAME]".
'
'***********************************************************************

Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

<Assembly: AssemblyTitle("[!output PROJECT_NAME] Console Monitor")> 
<Assembly: AssemblyDescription("[!output PROJECT_NAME] Console Based Remote Service Monitor")> 
<Assembly: AssemblyCompany("TVA")> 
<Assembly: AssemblyProduct("TVA Windows Service Template [!output GEN_TIME]")> 
<Assembly: AssemblyCopyright("Copyright © 2004")> 
<Assembly: AssemblyTrademark("")> 
<Assembly: CLSCompliant(True)> 

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("[!output GUID_ASSEMBLY]")> 

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version 
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers 
' by using the '*' as shown below:

<Assembly: AssemblyVersion("1.0.*")> 