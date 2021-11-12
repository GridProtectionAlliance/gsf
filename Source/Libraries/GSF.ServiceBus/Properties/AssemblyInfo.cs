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

// Assembly identity attributes.
[assembly: AssemblyVersion("2.3.512.0")]
[assembly: AssemblyInformationalVersion("2.3.512-beta")]

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

[assembly: AssemblyDefaultAlias("GSF.ServiceBus")]
[assembly: AssemblyDescription("Library of lightweight WCF-based service bus with queue and topic support for secure event-driven messaging between disjointed systems.")]
[assembly: AssemblyTitle("GSF.ServiceBus")]

// Other configuration attributes.
[assembly: ComVisible(false)]
[assembly: Guid("055544a1-be9e-4727-9946-173e3548d2e0")]
