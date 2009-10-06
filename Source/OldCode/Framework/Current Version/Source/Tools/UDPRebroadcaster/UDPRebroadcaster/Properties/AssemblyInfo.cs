using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("1.0.12.31177")]

// Informational attributes.
[assembly: AssemblyCompany("TVA")]
[assembly: AssemblyCopyright("No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.")]
[assembly: AssemblyProduct("UDPRebroadcaster")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif
[assembly: AssemblyDescription("Windows service that can rebroadcast UDP datagrams to multiple destinations.")]
[assembly: AssemblyTitle("UDPRebroadcaster")]

// Other configuration attributes.
[assembly: ComVisible(false)]
[assembly: Guid("0ef0b9ad-9030-40ec-9dad-05b778299e29")]
