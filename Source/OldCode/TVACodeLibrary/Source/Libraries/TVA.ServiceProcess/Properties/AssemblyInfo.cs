using System;
using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("4.0.2.63")]

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
[assembly: AssemblyDefaultAlias("TVA.ServiceProcess")]
[assembly: AssemblyDescription("Library of components that facilitate remote interaction with a Windows Service from Console, Windows and Web application.")]
[assembly: AssemblyTitle("TVA.ServiceProcess")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("eebbee45-2987-4bbd-9cc1-ff6d4dcf55f7")]
