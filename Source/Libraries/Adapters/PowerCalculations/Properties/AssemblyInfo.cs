using System;
using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("2.0.16.0")]

// Informational attributes.
[assembly: AssemblyCompany("Grid Protection Alliance")]
[assembly: AssemblyCopyright("Copyright © 2010.  All Rights Reserved.")]
[assembly: AssemblyProduct("openPDC")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif
[assembly: AssemblyDefaultAlias("PowerCalculations")]
[assembly: AssemblyDescription("Power calculation adapters.")]
[assembly: AssemblyTitle("PowerCalculations")]

// Other configuration attributes.
[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]
[assembly: Guid("784becf9-c3ff-4248-ac7a-67eab006cd4e")]
