using System;
using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("4.0.2.52")]

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
[assembly: AssemblyDefaultAlias("TVA.Windows")]
[assembly: AssemblyDescription("Library of Windows Forms and WPF extensions and components.")]
[assembly: AssemblyTitle("TVA.Windows")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("85dfc906-a5f5-41ff-9a40-230ded5cd69d")]
