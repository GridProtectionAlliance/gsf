using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("1.1.12.42748")]

// Informational attributes.
[assembly: AssemblyCompany("TVA")]
[assembly: AssemblyCopyright("No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.")]
[assembly: AssemblyProduct("ConfigCrypter")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif
[assembly: AssemblyDescription("Utility for encrypting and decrypting configuration file values.")]
[assembly: AssemblyTitle("ConfigCrypter")]

// Other configuration attributes.
[assembly: ComVisible(false)]
[assembly: Guid("3be9bc3a-666f-40d6-abe5-588593129c95")]
