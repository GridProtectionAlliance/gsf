//******************************************************************************************************
//  AssemblyInfo.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/09/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.UI;
using GSF.Web.Hosting;

// Assembly identity attributes.
[assembly: AssemblyVersion("2.4.124.0")]
[assembly: AssemblyInformationalVersion("2.4.124-beta")]

// Informational attributes.
[assembly: AssemblyCompany("Grid Protection Alliance")]
[assembly: AssemblyCopyright("Copyright © GPA, 2013.  All Rights Reserved.")]
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
[assembly: ComVisible(false)]
[assembly: Guid("eebbee45-2987-4bbd-9cc1-ff6d4dcf55f7")]

// Embed server-side controls.
[assembly: EmbeddedResourceFile("GSF.Web.Embedded.SecurityPortal.aspx", "GSF.Web.Embedded")]
[assembly: EmbeddedResourceFile("GSF.Web.Embedded.SecurityService.svc", "GSF.Web.Embedded")]

// Embed resources used by embedded controls.
[assembly: WebResource("GSF.Web.Embedded.Files.Help.pdf", "application/pdf")]
[assembly: WebResource("GSF.Web.Embedded.Images.Logo.png", "img/png")]
[assembly: WebResource("GSF.Web.Embedded.Images.Help.png", "img/png")]
[assembly: WebResource("GSF.Web.Embedded.Images.Warning.png", "img/png")]
[assembly: WebResource("GSF.Web.Embedded.Styles.SecurityPortal.css", "text/css")]
