using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.UI;
using TVA.Web.Hosting;

// Assembly identity attributes.
[assembly: AssemblyVersion("4.0.4.49")]

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
[assembly: AssemblyDefaultAlias("TVA.Web")]
[assembly: AssemblyDescription("Library of ASP.NET web forms extensions, embedded resource hosting and HTTP Module for implementing role-based security.")]
[assembly: AssemblyTitle("TVA.Web")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("eebbee45-2987-4bbd-9cc1-ff6d4dcf55f7")]

// Embedd server-side controls.
[assembly: EmbeddedResourceFile("TVA.Web.Embedded.SecurityPortal.aspx", "TVA.Web.Embedded")]
[assembly: EmbeddedResourceFile("TVA.Web.Embedded.SecurityService.svc", "TVA.Web.Embedded")]

// Embedd resources used by embedded controls.
[assembly: WebResource("TVA.Web.Embedded.Files.Help.pdf", "application/pdf")]
[assembly: WebResource("TVA.Web.Embedded.Images.TVALogo.png", "img/png")]
[assembly: WebResource("TVA.Web.Embedded.Images.Help.png", "img/png")]
[assembly: WebResource("TVA.Web.Embedded.Images.Warning.png", "img/png")]
[assembly: WebResource("TVA.Web.Embedded.Styles.SecurityPortal.css", "text/css")]
