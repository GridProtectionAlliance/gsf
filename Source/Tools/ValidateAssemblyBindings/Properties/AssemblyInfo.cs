//******************************************************************************************************
//  AssemblyInfo.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  02/07/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Reflection;
using System.Runtime.InteropServices;

// Assembly identity attributes.
[assembly: AssemblyVersion("2.3.11.0")]
[assembly: AssemblyInformationalVersion("2.3.11-beta")]

// Informational attributes.
[assembly: AssemblyCompany("Grid Protection Alliance")]
[assembly: AssemblyCopyright("Copyright © GPA, 2018.  All Rights Reserved.")]
[assembly: AssemblyProduct("Grid Solutions Framework")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif

[assembly: AssemblyDescription("Utility for validating GSF.Web assembly bindings in app.config files.")]
[assembly: AssemblyTitle("ValidateAssemblyBindings")]

// Other configuration attributes.
[assembly: ComVisible(false)]
[assembly: Guid("835df9af-664b-4814-9654-5cb996d544f9")]
