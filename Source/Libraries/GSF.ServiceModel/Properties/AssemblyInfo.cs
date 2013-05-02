using System;
using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("2.0.45.0")]

// Informational attributes.
[assembly: AssemblyCompany("Grid Protection Alliance")]
[assembly: AssemblyCopyright("Copyright ©  2012. All Rights Reserved.")]
[assembly: AssemblyProduct("Grid Solutions Framework")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif
[assembly: AssemblyDefaultAlias("GSF.ServiceModel")]
[assembly: AssemblyDescription("Library of base WCF service with self-hosting capability, and WCF Authorization Policy and Host Factories for implementing role-based security.")]
[assembly: AssemblyTitle("GSF.ServiceModel")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("57b04fab-3c0f-4d28-ad88-47e4222ebbe0")]
