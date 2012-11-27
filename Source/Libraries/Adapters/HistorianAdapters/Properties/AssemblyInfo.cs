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
[assembly: AssemblyDefaultAlias("HistorianAdapters")]
[assembly: AssemblyDescription("Historian input and output adapters.")]
[assembly: AssemblyTitle("HistorianAdapters")]

// Other configuration attributes.
[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]
[assembly: Guid("30ef2e6b-7f7e-4fb9-b2e0-62a6b3982f51")]
