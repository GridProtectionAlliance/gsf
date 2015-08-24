using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Web.UI;


// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

// Review the values of the assembly attributes

[assembly:AssemblyTitle("TVA .NET Code Library: Security")]
[assembly:AssemblyDescription("Shared .NET Security Functions Library")]
[assembly:AssemblyCompany("TVA")]
[assembly:AssemblyProduct("Shared .NET Code Library for TVA")]
[assembly:AssemblyCopyright("Copyright © 2006, TVA - All rights reserved")]
[assembly:AssemblyTrademark("Authors: J. Ritchie Carroll, Pinal C. Patel")]

[assembly:ComVisible(false)]
[assembly:CLSCompliant(true)]
[assembly:SecurityPermission(SecurityAction.RequestMinimum, Execution=true)]

//The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly:Guid("645fac90-077e-4eda-9b30-024debf2819f")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// <Assembly: AssemblyVersion("3.0.116.286")>

[assembly:AssemblyVersion("3.0.116.286")]
[assembly:AssemblyFileVersion("3.0.116.286")]

// Entries for embedded resources used by the composite controls.
[assembly:WebResource("TVA.Security.Application.Controls.Help.gif", "img/gif")]
[assembly:WebResource("TVA.Security.Application.Controls.Help.png", "img/png")]
[assembly:WebResource("TVA.Security.Application.Controls.Help.pdf", "Application/pdf")]
[assembly:WebResource("TVA.Security.Application.Controls.StyleSheet.css", "text/css")]
[assembly:WebResource("TVA.Security.Application.Controls.LogoInternal.jpg", "img/jpeg")]
[assembly:WebResource("TVA.Security.Application.Controls.LogoExternal.jpg", "img/jpeg")]
