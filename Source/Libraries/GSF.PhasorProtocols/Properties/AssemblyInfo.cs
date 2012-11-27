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
[assembly: AssemblyDefaultAlias("PhasorProtocols")]
[assembly: AssemblyDescription("Standard phasor protocol implementations.")]
[assembly: AssemblyTitle("PhasorProtocols")]

// Other configuration attributes.
[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]
[assembly: Guid("6d59b0ed-1991-4f12-a739-2cf8543dd9b2")]
