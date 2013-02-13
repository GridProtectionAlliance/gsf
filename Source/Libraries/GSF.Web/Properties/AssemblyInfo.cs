using GSF.Web.Hosting;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.UI;

// Assembly identity attributes.
[assembly: AssemblyVersion("2.0.28.0")]

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
[assembly: AssemblyDefaultAlias("GSF.Web")]
[assembly: AssemblyDescription("Library of ASP.NET web forms extensions, embedded resource hosting and HTTP Module for implementing role-based security.")]
[assembly: AssemblyTitle("GSF.Web")]

// Other configuration attributes.
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("eebbee45-2987-4bbd-9cc1-ff6d4dcf55f7")]

// Embedd server-side controls.
[assembly: EmbeddedResourceFile("GSF.Web.Embedded.SecurityPortal.aspx", "GSF.Web.Embedded")]
[assembly: EmbeddedResourceFile("GSF.Web.Embedded.SecurityService.svc", "GSF.Web.Embedded")]

// Embedd resources used by embedded controls.
[assembly: WebResource("GSF.Web.Embedded.Files.Help.pdf", "application/pdf")]
[assembly: WebResource("GSF.Web.Embedded.Images.GSFLogo.png", "img/png")]
[assembly: WebResource("GSF.Web.Embedded.Images.Help.png", "img/png")]
[assembly: WebResource("GSF.Web.Embedded.Images.Warning.png", "img/png")]
[assembly: WebResource("GSf.Web.Embedded.Styles.SecurityPortal.css", "text/css")]
