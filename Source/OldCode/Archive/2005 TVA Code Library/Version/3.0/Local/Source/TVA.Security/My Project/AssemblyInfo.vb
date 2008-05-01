Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Security.Permissions
Imports System.Web.UI

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

<Assembly: AssemblyTitle("TVA .NET Code Library: Security")> 
<Assembly: AssemblyDescription("Shared .NET Security Functions Library")> 
<Assembly: AssemblyCompany("TVA")> 
<Assembly: AssemblyProduct("Shared .NET Code Library for TVA")> 
<Assembly: AssemblyCopyright("Copyright © 2006, TVA - All rights reserved")> 
<Assembly: AssemblyTrademark("Authors: J. Ritchie Carroll, Pinal C. Patel")> 

<Assembly: ComVisible(False)> 
<Assembly: CLSCompliant(True)> 
<Assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution:=True)> 

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("645fac90-077e-4eda-9b30-024debf2819f")>

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version 
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers 
' by using the '*' as shown below:
' <Assembly: AssemblyVersion("2.2.1.57765")> 

<Assembly: AssemblyVersion("3.0.0.0")> 
<Assembly: AssemblyFileVersion("3.0.0.0")> 

' Entries for embedded resources used by the composite controls.
<Assembly: WebResource("TVA.Security.Application.Controls.Help.gif", "img/gif")> 
<Assembly: WebResource("TVA.Security.Application.Controls.Help.png", "img/png")> 
<Assembly: WebResource("TVA.Security.Application.Controls.Help.pdf", "Application/pdf")> 
<Assembly: WebResource("TVA.Security.Application.Controls.StyleSheet.css", "text/css")> 
<Assembly: WebResource("TVA.Security.Application.Controls.LogoInternal.jpg", "img/jpeg")> 
<Assembly: WebResource("TVA.Security.Application.Controls.LogoExternal.jpg", "img/jpeg")> 