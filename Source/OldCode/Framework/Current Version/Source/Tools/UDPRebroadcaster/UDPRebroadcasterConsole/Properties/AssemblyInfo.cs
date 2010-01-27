using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("1.0.68.40799")]

// Informational attributes.
[assembly: AssemblyCompany("TVA")]
[assembly: AssemblyCopyright("No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.")]
[assembly: AssemblyProduct("UDPRebroadcasterConsole")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif
[assembly: AssemblyDescription("Remote console application for windows service that can rebroadcast UDP datagrams to multiple destinations.")]
[assembly: AssemblyTitle("UDPRebroadcasterConsole")]

// Other configuration attributes.
[assembly: ComVisible(false)]
[assembly: Guid("079b7ca6-02df-4d17-a745-85d10ce25ef0")]
