using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly identity attributes.
[assembly: AssemblyVersion("2.0.32.0")]

// Informational attributes.
[assembly: AssemblyCompany("Grid Protection Alliance")]
[assembly: AssemblyCopyright("Copyright © 2012.  All Rights Reserved.")]
[assembly: AssemblyProduct("openPDC Historian")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif
[assembly: AssemblyDefaultAlias("GSF.Historian")]
[assembly: AssemblyDescription("Core historian specific components.")]
[assembly: AssemblyTitle("GSF.Historian")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("b058b8eb-e73e-414d-a5b2-461181db0de2")]
