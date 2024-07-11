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

[assembly: SuppressMessage("Microsoft.Usage", "CA2243:AttributeStringLiteralsShouldParseCorrectly")]

// Open internals for unit tests.
[assembly: InternalsVisibleTo("GSF.Core.Tests")]
[assembly: InternalsVisibleTo("GSF.TestsSuite")]
[assembly: InternalsVisibleTo("LibraryTester")]

// Assembly identity attributes.
[assembly: AssemblyVersion("2.4.189.0")]
[assembly: AssemblyInformationalVersion("2.4.189-beta")]

// Assembly manifest attributes.
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release Build")]
#endif

[assembly: AssemblyDefaultAlias("GSF.Core")]

// Other configuration attributes.
[assembly: ComVisible(false)]
[assembly: Guid("9448a8b5-35c1-4dc7-8c42-8712153ac08a")]
