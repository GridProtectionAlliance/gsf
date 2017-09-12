//******************************************************************************************************
//  AuthorizationCache.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  03/11/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using GSF.Data;
using GSF.Diagnostics;
using GSF.Identity;

namespace GSF.Web.Security
{
    /// <summary>
    /// Defines authorization cache.
    /// </summary>
    public class AuthorizationCache
    {
        /// <summary>
        /// Gets the current user ID cache.
        /// </summary>
        public static readonly ConcurrentDictionary<string, Guid> UserIDs = new ConcurrentDictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        private static readonly LogPublisher s_log = Logger.CreatePublisher(typeof(AuthorizationCache), MessageClass.Framework);

        /// <summary>
        /// Caches authorized user.
        /// </summary>
        /// <param name="userName">User name to cache.</param>
        /// <param name="securitySettingsCategory">Settings category used to lookup security connection for user data context.</param>
        public static void CacheAuthorization(string userName, string securitySettingsCategory)
        {
            // Make sure current user ID is cached
            if (UserIDs.ContainsKey(userName))
                return;

            try
            {
                using (AdoDataConnection connection = new AdoDataConnection(securitySettingsCategory))
                {
                    Guid? userID = connection.ExecuteScalar<Guid?>("SELECT ID FROM UserAccount WHERE Name={0}", UserInfo.UserNameToSID(userName));

                    if ((object)userID != null)
                        UserIDs.TryAdd(userName, userID.GetValueOrDefault());
                }
            }
            catch (Exception ex)
            {
                s_log.Publish(MessageLevel.Warning, "Authorization Cache Exception", exception: ex);
            }
        }
    }
}