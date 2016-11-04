//******************************************************************************************************
//  UserDataCache.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  04/06/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  04/14/2011 - Pinal C. Patel
//       Updated to use new serialization and deserialization methods in GSF.Serialization class.
//  06/10/2011 - Pinal C. Patel
//       Renamed RetryDelayInterval and MaximumRetryAttempts settings persisted to the config file 
//       to CacheRetryDelayInterval and CacheMaximumRetryAttempts for clarity.
//  08/12/2011 - J. Ritchie Carroll
//       Modified static GetCurrentCache to accept settings category of host security provider
//       implementation in case the category has been changed from the default value by the consumer.
//  08/16/2011 - Pinal C. Patel
//       Modified GetCurrentCache() to just set the FileName property and not the RetryDelayInterval, 
//       MaximumRetryAttempts, ReloadOnChange and AutoSave properties.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GSF.Collections;
using GSF.IO;
using GSF.Security.Cryptography;
using GSF.Threading;

namespace GSF.Security
{
    /// <summary>
    /// Represents a secured inter-process cache for a <see cref="Dictionary{TKey,TValue}"/> of serialized <see cref="UserData"/>.
    /// </summary>
    /// <remarks>
    /// This is a personal user data cache that only contains basic LDAP information for the user. It is used to load the local and
    /// Active Directory groups a user is associated with when the user no longer has access to its domain server. This can happen
    /// when a laptop that is normally connected to the Active Directory domain gets shutdown then restarted without access to the
    /// domain, for example, on an airplane - in this mode the user can successfully still login to the laptop to their using domain
    /// account cached by Windows but the groups the user is in will no longer be accessible. If role based security happens to be
    /// based on Active Directory groups, this cache will make sure the user can still have needed role based access even when the
    /// domain is unavailable. This cache is maintained as a separate user cache from the system level <see cref="AdoSecurityCache"/>
    /// since the user data cache only contains group information and is used by the <see cref="LdapSecurityProvider"/> which can be
    /// used independently of the <see cref="AdoSecurityProvider"/>.
    /// </remarks>
    public class UserDataCache : InterprocessCache
    {
        #region [ Members ]

        // Constants

        // Default user AD group data cache file name
        private const string DefaultCacheFileName = "UserDataCache.bin";

        // Expected cache header bytes
        private static readonly byte[] CacheHeaderBytes = { (byte)0x55, (byte)0x44, (byte)0x43 };

        // Fields
        private Dictionary<string, UserData> m_userDataTable;   // Internal dictionary of serialized user data
        private readonly object m_userDataTableLock;            // Lock object for internal dictionary
        private int m_providerID;                               // Unique provider ID used to distinguish cached user data that may be different based on provider

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="UserDataCache"/>.
        /// </summary>
        /// <param name="providerID">Unique provider ID used to distinguish cached user data that may be different based on provider.</param>
        public UserDataCache(int providerID = LdapSecurityProvider.ProviderID)
            : this(providerID, InterprocessReaderWriterLock.DefaultMaximumConcurrentLocks)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UserDataCache"/> with the specified number of <paramref name="maximumConcurrentLocks"/>.
        /// </summary>
        /// <param name="maximumConcurrentLocks">Maximum concurrent reader locks to allow.</param>
        /// <param name="providerID">Unique provider ID used to distinguish cached user data that may be different based on provider.</param>
        public UserDataCache(int providerID, int maximumConcurrentLocks)
            : base(maximumConcurrentLocks)
        {
            m_providerID = providerID;
            m_userDataTable = new Dictionary<string, UserData>(StringComparer.OrdinalIgnoreCase);
            m_userDataTableLock = new object();
        }

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

        /// <summary>
        /// Gets or sets unique provider ID used to distinguish cached user data that may be different based on provider.
        /// </summary>
        public int ProviderID
        {
            get
            {
                return m_providerID;
            }
            set
            {
                m_providerID = value;
            }
        }

        #endregion

        #region [ Methods ]

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
            WaitForDataReady();

            // Wait for thread level lock on user data table
            lock (m_userDataTableLock)
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

            // We wait until the cache is loaded before attempting to access it
            WaitForDataReady();

            // Wait for thread level lock on user data table
            lock (m_userDataTableLock)
            {
                // Assign new user information to user data table
                m_userDataTable[hash] = userData;
            }

            // Queue up a serialization for this new user information
            Save();
        }

        /// <summary>
        /// Initiates inter-process synchronized save of user data cache.
        /// </summary>
        public override void Save()
        {
            byte[] serializedUserDataTable;

            // Wait for thread level lock on key table
            lock (m_userDataTableLock)
            {
                serializedUserDataTable = SerializeCache(m_userDataTable);
            }

            // File data is the serialized user data table, assignment will initiate auto-save if needed
            FileData = serializedUserDataTable;
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
            Dictionary<string, UserData> userDataTable = DeserializeCache(serializedUserDataTable);

            // Wait for thread level lock on user data table
            lock (m_userDataTableLock)
            {
                // Merge new and existing key tables since new user information may have been queued for serialization, but not saved yet
                m_userDataTable = userDataTable.Merge(m_userDataTable);
            }

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
            return Cipher.GetPasswordHash(loginID.ToLower(), m_providerID);
        }

        // Waits until the cache is loaded before attempting to access it
        private void WaitForDataReady()
        {
            try
            {
                // Just wrapping this method to provide a more detailed exception message if there is an issue loading cache
                WaitForLoad();
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("User data access failure: timeout while attempting to load user data cache.", ex);
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Loads the <see cref="UserDataCache"/> for the current local user.
        /// </summary>
        /// <param name="providerID">Unique security provider ID used to distinguish cached user data that may be different based on provider.</param>
        /// <returns>Loaded instance of the <see cref="UserDataCache"/>.</returns>
        public static UserDataCache GetCurrentCache(int providerID)
        {
            // By default user data cache is stored in a path where user will have rights
            UserDataCache userDataCache;
            string userCacheFolder = FilePath.GetApplicationDataFolder();
            string userCacheFileName = Path.Combine(userCacheFolder, FilePath.GetFileName(DefaultCacheFileName));

            // Make sure user directory exists
            if (!Directory.Exists(userCacheFolder))
                Directory.CreateDirectory(userCacheFolder);

            // Initialize user data cache for current local user
            userDataCache = new UserDataCache(providerID);
            userDataCache.FileName = userCacheFileName;

            return userDataCache;
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private static byte[] SerializeCache(Dictionary<string, UserData> cache)
        {
            using (BlockAllocatedMemoryStream stream = new BlockAllocatedMemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Default))
            {
                writer.Write(CacheHeaderBytes);
                writer.Write(cache.Count);

                foreach (KeyValuePair<string, UserData> data in cache)
                {
                    UserData userData = data.Value;

                    writer.Write(data.Key);
                    writer.Write(userData.Username);
                    writer.Write(userData.FirstName);
                    writer.Write(userData.LastName);
                    writer.Write(userData.CompanyName);
                    writer.Write(userData.PhoneNumber);
                    writer.Write(userData.EmailAddress);
                    writer.Write(userData.IsLockedOut);
                    writer.Write(userData.IsDisabled);
                    writer.Write(userData.PasswordChangeDateTime.Ticks);
                    writer.Write(userData.AccountCreatedDateTime.Ticks);

                    writer.Write(userData.Roles.Count);

                    foreach (string role in userData.Roles)
                        writer.Write(role);

                    writer.Write(userData.Groups.Count);

                    foreach (string group in userData.Groups)
                        writer.Write(group);
                }

                return stream.ToArray();
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private Dictionary<string, UserData> DeserializeCache(byte[] data)
        {
            Dictionary<string, UserData> cache = new Dictionary<string, UserData>(StringComparer.OrdinalIgnoreCase);

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream, Encoding.Default))
            {
                if (reader.ReadBytes(CacheHeaderBytes.Length).CompareTo(CacheHeaderBytes) != 0)
                    throw new InvalidDataException("Unexpected data read from UserDataCache - file possibly corrupted.");

                int listLength, cacheLength = reader.ReadInt32();

                for (int i = 0; i < cacheLength; i++)
                {
                    UserData userData = new UserData();
                    string loginID;

                    loginID = reader.ReadString();
                    userData.Username = reader.ReadString();
                    userData.FirstName = reader.ReadString();
                    userData.LastName = reader.ReadString();
                    userData.CompanyName = reader.ReadString();
                    userData.PhoneNumber = reader.ReadString();
                    userData.EmailAddress = reader.ReadString();
                    userData.IsLockedOut = reader.ReadBoolean();
                    userData.IsDisabled = reader.ReadBoolean();
                    userData.PasswordChangeDateTime = new DateTime(reader.ReadInt64());
                    userData.AccountCreatedDateTime = new DateTime(reader.ReadInt64());

                    userData.Roles = new List<string>();
                    listLength = reader.ReadInt32();

                    for (int j = 0; j < listLength; j++)
                        userData.Roles.Add(reader.ReadString());

                    userData.Groups = new List<string>();
                    listLength = reader.ReadInt32();

                    for (int j = 0; j < listLength; j++)
                        userData.Groups.Add(reader.ReadString());

                    cache.Add(loginID, userData);
                }
            }

            return cache;
        }

        #endregion
    }
}
