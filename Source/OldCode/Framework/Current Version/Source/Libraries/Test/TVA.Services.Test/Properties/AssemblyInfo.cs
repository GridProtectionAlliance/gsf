using System;
using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("1.0.35.37890")]

// Informational attributes.
[assembly: AssemblyCompany("TVA")]
[assembly: AssemblyCopyright("No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.")]
[assembly: AssemblyProduct("openPDC Framework")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif
[assembly: AssemblyDefaultAlias("TVA.Services.Test")]
[assembly: AssemblyDescription("Unit tests for windows service components of the openPDC Framework.")]
[assembly: AssemblyTitle("TVA.Services.Test")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("222621fa-2350-4a4e-b826-c4d60f6304b2")]
