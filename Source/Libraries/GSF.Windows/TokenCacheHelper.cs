//******************************************************************************************************
//  TokenCacheHelper.cs - Gbtc
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
//  01/15/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using Microsoft.Identity.Client;
using System.IO;
using System.Security.Cryptography;
using GSF.IO;

namespace GSF.Windows
{
    /// <summary>
    /// Defines a helper class for token cache management.
    /// </summary>
    public static class TokenCacheHelper
    {
        static TokenCacheHelper()
        {
            CacheFilePath = Path.Combine(FilePath.GetApplicationDataFolder(), "msalv3.cache");
        }

        /// <summary>
        /// Path to the token cache.
        /// </summary>
        public static string CacheFilePath { get; }

        private static readonly object s_fileLock = new();

        private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            byte[] msal = null;
            
            if (File.Exists(CacheFilePath))
            {
                lock (s_fileLock)
                    msal = ProtectedData.Unprotect(File.ReadAllBytes(CacheFilePath), null, DataProtectionScope.CurrentUser);
            }

            args.TokenCache.DeserializeMsalV3(msal);
        }

        private static void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (!args.HasStateChanged)
                return;
            
            byte[] msal = ProtectedData.Protect(args.TokenCache.SerializeMsalV3(), null, DataProtectionScope.CurrentUser);

            lock (s_fileLock)
                File.WriteAllBytes(CacheFilePath, msal);
        }

        internal static void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }
    }
}
