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

using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Identity.Client;
using GSF.IO;
using GSF.Security.Cryptography;
using Logger = GSF.Diagnostics.Logger;

namespace GSF.Security
{
    /// <summary>
    /// Defines a helper class for token cache management.
    /// </summary>
    public static class TokenCacheHelper
    {
        static TokenCacheHelper()
        {
            string appDataFolder = FilePath.GetApplicationDataFolder();

            try
            {
                if (!Directory.Exists(appDataFolder))
                    Directory.CreateDirectory(appDataFolder);
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }

            CacheFilePath = Path.Combine(appDataFolder, "msalv3.cache");
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
                // Even though this cache will be for the current system user, we cannot guarantee that the 
                // current user context will match the user system context, e.g., when using an Azure AD
                // account or database user, so we will use the local machine to ensure consistent access.
                lock (s_fileLock)
                    msal = DataProtection.Unprotect(File.ReadAllBytes(CacheFilePath), null, DataProtectionScope.LocalMachine);
            }

            args.TokenCache.DeserializeMsalV3(msal);
        }

        private static void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (!args.HasStateChanged)
                return;
            
            // Even though this cache will be for the current system user, we cannot guarantee that the 
            // current user context will match the user system context, e.g., when using an Azure AD
            // account or database user, so we will use the local machine to ensure consistent access.
            byte[] msal = DataProtection.Protect(args.TokenCache.SerializeMsalV3(), null, DataProtectionScope.LocalMachine);

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
