//******************************************************************************************************
//  CryptoSystem.cs - Gbtc
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

using System.Security.Cryptography;
using System.Web;
using Random = GSF.Security.Cryptography.Random;

// ReSharper disable AssignNullToNotNullAttribute
namespace GSF.Web.Security.AntiCsrf
{
    internal static class CryptoSystem
    {
        private static readonly byte[] s_entropy = new byte[16];

        static CryptoSystem()
        {
            // Added entropy makes tokens unique to AppDomain instance of this class
            Random.GetBytes(s_entropy);
        }

        public static string Protect(byte[] data)
        {
            byte[] rawProtectedBytes = ProtectedData.Protect(data, s_entropy, DataProtectionScope.LocalMachine);
            return HttpServerUtility.UrlTokenEncode(rawProtectedBytes);
        }

        public static byte[] Unprotect(string protectedData)
        {
            byte[] rawProtectedBytes = HttpServerUtility.UrlTokenDecode(protectedData);
            return ProtectedData.Unprotect(rawProtectedBytes, s_entropy, DataProtectionScope.LocalMachine);
        }
    }
}