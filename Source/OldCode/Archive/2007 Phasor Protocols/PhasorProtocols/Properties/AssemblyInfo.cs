using System;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("NERC PCS Phasor Protocols")]
[assembly: AssemblyDescription("Standard Phasor Protocol Implementations")]
[assembly: AssemblyCompany("TVA")]
[assembly: AssemblyProduct("NERC Phasor Concentration System")]
[assembly: AssemblyCopyright("Copyright © 2009, TVA")]
[assembly: AssemblyTrademark("Author: J. Ritchie Carroll, Gbtc")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Most all classes in phasor protocols use an unsigned type, instead of marking
// each individual class as non-CLS compliant, we just mark the entire assembly.
[assembly: CLSCompliant(false)]

//The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6d59b0ed-1991-4f12-a739-2cf8543dd9b2")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
