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
[assembly: AssemblyDefaultAlias("GSF.ServiceBus")]
[assembly: AssemblyDescription("Library of lightweight WCF-based Service Bus with queue and topic support for secure event-driven messaging between disjoint systems.")]
[assembly: AssemblyTitle("GSF.ServiceBus")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("055544a1-be9e-4727-9946-173e3548d2e0")]
