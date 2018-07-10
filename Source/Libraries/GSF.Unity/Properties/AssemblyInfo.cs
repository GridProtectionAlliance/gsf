using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("2.3.12.0")]
[assembly: AssemblyInformationalVersion("2.3.12-beta")]

// Informational attributes.
[assembly: AssemblyCompany("Grid Protection Alliance")]
[assembly: AssemblyCopyright("Copyright © 2012.  All Rights Reserved.")]
[assembly: AssemblyProduct("Grid Solutions Framework")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif
[assembly: AssemblyDefaultAlias("GSF.Unity")]
[assembly: AssemblyDescription("Library of GSF .NET adapters, extensions and components for Unity 3D platform.")]
[assembly: AssemblyTitle("GSF.Core")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("C6E788CA-F2AE-4FB1-A7EB-E0014EBAE0D2")]
[assembly: NeutralResourcesLanguage("en-US")]
