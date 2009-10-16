using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly identity attributes.
[assembly: AssemblyVersion("1.0.12.31985")]

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
[assembly: AssemblyDefaultAlias("TVA.Web")]
[assembly: AssemblyDescription("Web application components of the openPDC Framework.")]
[assembly: AssemblyTitle("TVA.Web")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("eebbee45-2987-4bbd-9cc1-ff6d4dcf55f7")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]
