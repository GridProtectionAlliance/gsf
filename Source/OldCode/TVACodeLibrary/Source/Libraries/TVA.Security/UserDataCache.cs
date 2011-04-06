//******************************************************************************************************
//  UserDataCache.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  04/06/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using TVA.Collections;
using TVA.Configuration;
using TVA.IO;
using TVA.Security.Cryptography;

namespace TVA.Security
{
    /// <summary>
    /// Represents a secured interprocess cache for a <see cref="Dictionary{T1,T2}"/> of serialized <see cref="UserData"/>.
    /// </summary>
    public class UserDataCache : InterprocessCache
    {
        #region [ Members ]

        // Constants

        // Default key and initialization vector cache file name
        private const string DefaultCacheFileName = "UserDataCache.bin";

        // Fields

        // Internal dictionary of serialized user data
        private Dictionary<string, UserData> m_userDataTable = new Dictionary<string, UserData>();

        // Wait handle used so that system will wait for user data cache load before any pending dictionary access
        private ManualResetEventSlim m_userDataTableIsReady = new ManualResetEventSlim(false);

        // Class disposed flag
        private bool m_disposed;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="UserData"/> for given <paramref name="loginID"/>.
        /// </summary>
        /// <param name="loginID">Login ID of associated <see cref="UserData"/> to load or save.</param>
        /// <returns>Reference to <see cref="UserData"/> for given <paramref name="loginID"/> if found; otherwise <c>null</c>.</returns>
        public UserData this[string loginID]
        {
            get
            {
                UserData userData;
                TryGetUserData(loginID, out userData);
                return userData;
            }
            set
            {
                SaveUserData(loginID, value);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="UserDataCache"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_userDataTableIsReady != null)
                            m_userDataTableIsReady.Dispose();

                        m_userDataTableIsReady = null;
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Attempts to retrieve <see cref="UserData"/> for given <paramref name="loginID"/>.
        /// </summary>
        /// <param name="loginID">Login ID of associated <see cref="UserData"/> to retrieve.</param>
        /// <param name="userData">Reference to <see cref="UserData"/> object to populate if found.</param>
        /// <returns><c>true</c> if <see cref="UserData"/> for given <paramref name="loginID"/> was retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetUserData(string loginID, out UserData userData)
        {
            string hash = HashLoginID(loginID);
            bool result;

            // We wait until the cache is loaded before attempting to access it
            if (!m_userDataTableIsReady.IsSet && !m_userDataTableIsReady.Wait((int)(RetryDelayInterval * MaximumRetryAttempts)))
                throw new UnauthorizedAccessException("User data access failure: timeout while attempting to load user data cache.");

            // Wait for thread level lock on user data table
            lock (m_userDataTable)
            {
                // Attempt to lookup persisted user data based on hash of login ID
                result = m_userDataTable.TryGetValue(hash, out userData);
            }

            return result;
        }

        /// <summary>
        /// Serializes the <paramref name="userData"/> for the given <paramref name="loginID"/> into the <see cref="UserDataCache"/>.
        /// </summary>
        /// <param name="loginID">Login ID of associated <see cref="UserData"/> to retrieve.</param>
        /// <param name="userData">Reference to <see cref="UserData"/> object to serialize into <see cref="UserDataCache"/>.</param>
        /// <remarks>
        /// <para>
        /// This will add an entry into the user data cache for <paramref name="loginID"/> if it doesn't exist;
        /// otherwise existing entry will be updated.
        /// </para>
        /// <para>
        /// Updates are automatically queued up for serialization so user does not need to call <see cref="Save"/>.
        /// </para>
        /// </remarks>
        public void SaveUserData(string loginID, UserData userData)
        {
            string hash = HashLoginID(loginID);

            // Wait for thread level lock on user data table
            lock (m_userDataTable)
            {
                // Assign new user information to user data table
                m_userDataTable[hash] = userData;
            }

            // Queue up a serialization for this new user information
            Save();
        }

        /// <summary>
        /// Initiates interprocess synchronized save of user data cache.
        /// </summary>
        public override void Save()
        {
            byte[] serializedUserDataTable;

            // Wait for thread level lock on key table
            lock (m_userDataTable)
            {
                serializedUserDataTable = Serialization.GetBytes(m_userDataTable);
            }

            // File data is the serialized user data table, assigmnent will initiate auto-save if needed
            FileData = serializedUserDataTable;
        }

        /// <summary>
        /// Initiates interprocess synchronized load of user data cache.
        /// </summary>
        public override void Load()
        {
            // Hold any threads needing user information
            m_userDataTableIsReady.Reset();

            base.Load();
        }

        /// <summary>
        /// Handles serialization of file to disk; virtual method allows customization (e.g., pre-save encryption and/or data merge).
        /// </summary>
        /// <param name="fileStream"><see cref="FileStream"/> used to serialize data.</param>
        /// <param name="fileData">File data to be serialized.</param>
        /// <remarks>
        /// Consumers overriding this method should not directly call <see cref="InterprocessCache.FileData"/> property to avoid potential dead-locks.
        /// </remarks>
        protected override void SaveFileData(FileStream fileStream, byte[] fileData)
        {
            // Encrypt data local to this machine (this way user cannot copy user data cache to another machine)
            base.SaveFileData(fileStream, ProtectedData.Protect(fileData, null, DataProtectionScope.LocalMachine));
        }

        /// <summary>
        /// Handles deserialization of file from disk; virtual method allows customization (e.g., pre-load decryption and/or data merge).
        /// </summary>
        /// <param name="fileStream"><see cref="FileStream"/> used to deserialize data.</param>
        /// <returns>Deserialized file data.</returns>
        /// <remarks>
        /// Consumers overriding this method should not directly call <see cref="InterprocessCache.FileData"/> property to avoid potential dead-locks.
        /// </remarks>
        protected override byte[] LoadFileData(FileStream fileStream)
        {
            // Decrypt data that was encrypted local to this machine
            byte[] serializedUserDataTable = ProtectedData.Unprotect(fileStream.ReadStream(), null, DataProtectionScope.LocalMachine);
            Dictionary<string, UserData> userDataTable = Serialization.GetObject<Dictionary<string, UserData>>(serializedUserDataTable);

            // Wait for thread level lock on user data table
            lock (m_userDataTable)
            {
                // Merge new and existing key tables since new user information may have been queued for serialization, but not saved yet
                m_userDataTable = userDataTable.Merge(m_userDataTable);
            }

            // Release any threads waiting for user information
            m_userDataTableIsReady.Set();

            return serializedUserDataTable;
        }

        /// <summary>
        /// Calculates the hash of the <paramref name="loginID"/> used as the key for the user data cache.
        /// </summary>
        /// <param name="loginID">Login ID to hash.</param>
        /// <returns>The Base64 encoded calculated SHA-2 hash of the <paramref name="loginID"/> used as the key for the user data cache.</returns>
        /// <remarks>
        /// For added security, a hash of the <paramref name="loginID"/> is used as the key for <see cref="UserData"/> in the
        /// user data cache instead of the actual <paramref name="loginID"/>. This method allows the
        /// consumer to properly calculate this hash when directly using the user data cache.
        /// </remarks>
        protected string HashLoginID(string loginID)
        {
            return Cipher.GetPasswordHash(loginID, 0);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Loads the <see cref="UserDataCache"/> for the current local user.
        /// </summary>
        /// <returns>Loaded instance of the <see cref="UserDataCache"/>.</returns>
        public static UserDataCache GetCurrentCache()
        {
            // By default user data cache is stored in a path where user will have rights
            UserDataCache userDataCache;
            string userCacheFolder = FilePath.AddPathSuffix(FilePath.GetApplicationDataFolder());
            string userCacheFileName = userCacheFolder + FilePath.GetFileName(DefaultCacheFileName);
            double retryDelayInterval = DefaultRetryDelayInterval;
            int maximumRetryAttempts = DefaultMaximumRetryAttempts;

            // Load user data cache settings
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SecurityProviderBase.DefaultSettingsCategory];

            settings.Add("UserDataCache", userCacheFileName, "Path and file name of user data cache.");
            settings.Add("RetryDelayInterval", retryDelayInterval, "Wait interval, in milliseconds, before retrying load of user data cache.");
            settings.Add("MaximumRetryAttempts", maximumRetryAttempts, "Maximum retry attempts allowed for loading user data cache.");

            userCacheFileName = FilePath.GetAbsolutePath(settings["UserDataCache"].ValueAs(userCacheFileName));
            retryDelayInterval = settings["RetryDelayInterval"].ValueAs(retryDelayInterval);
            maximumRetryAttempts = settings["MaximumRetryAttempts"].ValueAs(maximumRetryAttempts);

            // Make sure user directory exists
            if (!Directory.Exists(userCacheFolder))
                Directory.CreateDirectory(userCacheFolder);

            // Initialize user data cache for current local user
            userDataCache = new UserDataCache()
            {
                FileName = userCacheFileName,
                RetryDelayInterval = retryDelayInterval,
                MaximumRetryAttempts = maximumRetryAttempts,
                ReloadOnChange = true,
                AutoSave = true
            };

            // Load initial user data
            userDataCache.Load();

            return userDataCache;
        }

        #endregion
        
    }
}
