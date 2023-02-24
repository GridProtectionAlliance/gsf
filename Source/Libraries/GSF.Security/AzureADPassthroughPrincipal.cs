//******************************************************************************************************
//  AzureADPassthroughPrincipal.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/24/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Security.Principal;

namespace GSF.Security;

internal class AzureADPassthroughPrincipal : GenericPrincipal
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureADPassthroughPrincipal" />
    /// class representing an Azure AD user with a passthrough principal.
    /// </summary>
    /// <param name="name">The name of the user on whose behalf the code is running.</param>
    public AzureADPassthroughPrincipal(string name) : 
        base(new GenericIdentity(name, nameof(AzureADPassthroughPrincipal)), Array.Empty<string>())
    {
    }
}