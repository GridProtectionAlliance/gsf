//******************************************************************************************************
//  AssemblyInfo.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
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

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("2.0.205.0")]

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
[assembly: AssemblyDescription("Library of .NET extensions and components - adapter framework, process queue, configuration api, diagnostics, error handling, active directory, interop, checksums, ftp, mail, unit conversion, binary parsing, scheduler, ntp time, precision timer, int24, unit24, console extensions, database extensions, drawing extension, reflection extensions, xml extensions, bit extensions, buffer extensions, char extensions, data-time extensions, enum extensions, string extensions.")]
[assembly: AssemblyTitle("GSF.Core")]

// Other configuration attributes.
[assembly: ComVisible(false)]
[assembly: Guid("9448a8b5-35c1-4dc7-8c42-8712153ac08a")]
[assembly: NeutralResourcesLanguage("en-US")]
