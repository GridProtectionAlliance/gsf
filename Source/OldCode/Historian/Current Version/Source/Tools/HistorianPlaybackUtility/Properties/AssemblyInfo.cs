using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly identity attributes.
[assembly: AssemblyVersion("1.1.8.42584")]

// Informational attributes.
[assembly: AssemblyCompany("TVA")]
[assembly: AssemblyCopyright("No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.")]
[assembly: AssemblyProduct("Historian Playback Utility")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif
[assembly: AssemblyDefaultAlias("HistorianPlaybackUtility")]
[assembly: AssemblyDescription("Utility for playing back data from historian data files.")]
[assembly: AssemblyTitle("HistorianPlaybackUtility")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("ddd2158e-a37b-425f-8be5-c86c5a99a31a")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]