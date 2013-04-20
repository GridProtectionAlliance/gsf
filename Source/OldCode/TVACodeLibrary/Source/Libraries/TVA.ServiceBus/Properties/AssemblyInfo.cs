using System;
using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("4.0.4.59")]

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
[assembly: AssemblyDefaultAlias("TVA.ServiceBus")]
[assembly: AssemblyDescription("Library of lightweight WCF-based Service Bus with queue and topic support for secure event-driven messaging between disjoint systems.")]
[assembly: AssemblyTitle("TVA.ServiceBus")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("055544a1-be9e-4727-9946-173e3548d2e0")]
