Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Security.Permissions

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

<Assembly: AssemblyTitle("TVA .NET Code Library: Shared")> 
<Assembly: AssemblyDescription("Shared .NET Global Functions Code Library")> 
<Assembly: AssemblyCompany("TVA")> 
<Assembly: AssemblyProduct("Shared .NET Code Library for TVA")> 
<Assembly: AssemblyCopyright("Copyright © 2003, TVA - All rights reserved")> 
<Assembly: AssemblyTrademark("Author: James Ritchie Carroll")> 
<Assembly: CLSCompliant(True)> 

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("B70BF390-B162-447F-9308-7BA63592AF9E")> 

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version 
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers 
' by using the '*' as shown below:

<Assembly: AssemblyVersion("7.3.6.61900")> 

' This added so this assembly can impersonate other users
<Assembly: SecurityPermissionAttribute(SecurityAction.RequestMinimum, UnmanagedCode:=True)> 
<Assembly: PermissionSetAttribute(SecurityAction.RequestMinimum, Name:="FullTrust")> 
