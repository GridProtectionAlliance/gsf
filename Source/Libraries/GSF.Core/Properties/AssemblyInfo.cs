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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("2.3.195.0")]

[assembly: SuppressMessage("Microsoft.Usage", "CA2243:AttributeStringLiteralsShouldParseCorrectly")]
[assembly: AssemblyInformationalVersion("2.3.195-beta")]

// Informational attributes.
[assembly: AssemblyCompany("Grid Protection Alliance")]
[assembly: AssemblyCopyright("Copyright © GPA, 2013.  All Rights Reserved.")]
[assembly: AssemblyProduct("Grid Solutions Framework")]
// Open internals for unit tests.
[assembly: InternalsVisibleTo("GSF.Core.Tests")]
[assembly: InternalsVisibleTo("GSF.TestsSuite")]
// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif

[assembly: AssemblyDefaultAlias("GSF.Core")]
[assembly: AssemblyDescription("Library of .NET functions, extensions and components including asynchronous processing queues, configuration APIs, diagnostics, error handling, console functions, adapter framework, active directory and local account functions, checksum algorithms, unit conversion, binary parsing, cron-style task scheduler, Unix and NTP time classes, precision timer, 24-bit signed and unsigned integers, database extensions and abstraction layer, extensions for drawing, reflection, XML, buffers, chars, date/times, enumerations, strings, etc.")]
[assembly: AssemblyTitle("GSF.Core")]

// Other configuration attributes.
[assembly: ComVisible(false)]
[assembly: Guid("9448a8b5-35c1-4dc7-8c42-8712153ac08a")]
[assembly: NeutralResourcesLanguage("en-US")]
