using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("4.0.2.42")]

// Informational attributes.
[assembly: AssemblyCompany("TVA")]
[assembly: AssemblyCopyright("No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.")]
[assembly: AssemblyProduct("ConfigurationEditor")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif
[assembly: AssemblyDescription("Utility for editing application configuration files.")]
[assembly: AssemblyTitle("ConfigurationEditor")]

// Other configuration attributes.
[assembly: ComVisible(false)]
[assembly: Guid("c4ba78b7-92b0-4125-a5b0-3840ab9a6d72")]
