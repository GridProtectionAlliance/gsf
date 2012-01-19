using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("4.0.3.36")]

// Informational attributes.
[assembly: AssemblyCompany("TVA")]
[assembly: AssemblyCopyright("No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.")]
[assembly: AssemblyProduct("TVA Code Library")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif
[assembly: AssemblyDefaultAlias("TVA.Core")]
[assembly: AssemblyDescription("Library of .NET extensions and components - adapter framework, process queue, configuration api, diagnostics, error handling, active directory, interop, checksums, ftp, mail, unit conversion, binary parsing, scheduler, ntp time, precision timer, int24, unit24, console extensions, database extensions, drawing extension, reflection extensions, xml extensions, bit extensions, buffer extensions, char extensions, data-time extensions, enum extensions, string extensions.")]
[assembly: AssemblyTitle("TVA.Core")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("9448a8b5-35c1-4dc7-8c42-8712153ac08a")]
[assembly: NeutralResourcesLanguage("en-US")]
