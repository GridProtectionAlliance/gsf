using System;
using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("2.0.16.0")]

// Informational attributes.
[assembly: AssemblyCompany("Grid Protection Alliance")]
[assembly: AssemblyCopyright("Copyright © 2012.  All Rights Reserved.")]
[assembly: AssemblyProduct("Grid Solutions Frameowrk")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif

[assembly: AssemblyDefaultAlias("GSF.Communication")]
[assembly: AssemblyDescription("Library of components for TCP, UDP, serial and file-based communication in an abstract fashion.")]
[assembly: AssemblyTitle("GSF.Communication")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("fa2223ea-9782-49f1-8757-2a333a1633c2")]
