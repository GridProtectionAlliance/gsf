//******************************************************************************************************
//  AntiForgeryToken.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  02/15/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

// Derived from AspNetWebStack (https://github.com/aspnet/AspNetWebStack) 
// Copyright (c) .NET Foundation. All rights reserved.
// See NOTICE.txt file in Source folder for more information.

#endregion

namespace GSF.Web.Security.AntiCsrf
{
    // Represents the security token for the Anti-XSRF system.
    // The token is a random 128-bit value that correlates the session with the request body.
    internal sealed class AntiForgeryToken
    {
        internal const int SecurityTokenBitLength = 128;
        internal const int ClaimUidBitLength = 256;

        private string m_additionalData;
        private BinaryBlob m_securityToken;
        private string m_username;

        public string AdditionalData
        {
            get => m_additionalData ?? string.Empty;
            set => m_additionalData = value;
        }

        public bool IsSessionToken { get; set; }

        public BinaryBlob SecurityToken
        {
            get => m_securityToken ?? (m_securityToken = new BinaryBlob(SecurityTokenBitLength));
            set => m_securityToken = value;
        }

        public string Username
        {
            get => m_username ?? string.Empty;
            set => m_username = value;
        }
    }
}
