using System;
using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("4.0.4.17")]

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
[assembly: AssemblyDefaultAlias("TVA.ServiceModel")]
[assembly: AssemblyDescription("Library of base WCF service with self-hosting capability, and WCF Authorization Policy and Host Factories for implementing role-based security.")]
[assembly: AssemblyTitle("TVA.ServiceModel")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("57b04fab-3c0f-4d28-ad88-47e4222ebbe0")]
