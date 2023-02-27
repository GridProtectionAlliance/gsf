//******************************************************************************************************
//  UserRoleCache.cs - Gbtc
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
//  05/07/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using GSF.Collections;
using GSF.IO;
using GSF.Security.Cryptography;
using GSF.Threading;

namespace GSF.Security
{
    /// <summary>
    /// Represents a secured inter-process cache for a <see cref="Dictionary{TKey,TValue}"/> of serialized user role information.
    /// </summary>
    /// <remarks>
    /// This is a system cache that contains the role assignments for each user that has logged in successfully. This cache is used
    /// to check for changes in role assignments for a user - that is, a role change in the database that may now be different than
    /// what is in the current cache. Any kind of role changes are logged as security events in the Windows event log for auditing.
    /// Note that this is kept as a separate cache from the <see cref="AdoSecurityCache"/> since the user role cache is used for
    /// auditing and contains information relative to a user's roles at last login.
    /// </remarks>
    public class UserRoleCache : InterprocessCache
    {
        #region [ Members ]

        // Constants

        // Default user role cache file name
        private const string DefaultCacheFileName = "UserRoleCache.bin";

        // Fields
        private Dictionary<string, string[]> m_userRoles;   // Internal dictionary of serialized user roles
        private readonly object m_userRolesLock;            // Lock object for internal dictionary

        #endregion

        #region [ Constructors ]
        
        /// <summary>
        /// Creates a new instance of the <see cref="UserRoleCache"/> with the specified number of <paramref name="maximumConcurrentLocks"/>.
        /// </summary>
        /// <param name="maximumConcurrentLocks">Maximum concurrent reader locks to allow.</param>
        public UserRoleCache(int maximumConcurrentLocks = InterprocessReaderWriterLock.DefaultMaximumConcurrentLocks)
            : base(maximumConcurrentLocks)
        {
            m_userRoles = new Dictionary<string, string[]>();
            m_userRolesLock = new object();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a copy of the internal user role dictionary.
        /// </summary>
        public Dictionary<string, string[]> UserRoles
        {
            get
            {
                Dictionary<string, string[]> userRoles;

                // We wait until the user roles cache is loaded before attempting to access it
                WaitForDataReady();

                // Wait for thread level lock on dictionary
                lock (m_userRolesLock)
                {
                    // Make a copy of the user role dictionary for external use
                    userRoles = new Dictionary<string, string[]>(m_userRoles);
                }

                return userRoles;
            }
        }

        /// <summary>
        /// Gets or sets access roles for given <paramref name="userName"/>.
        /// </summary>
        /// <param name="userName">User name for associated access role to load or save.</param>
        /// <returns>Access roles for given <paramref name="userName"/> if found; otherwise <c>null</c>.</returns>
        public string[] this[string userName]
        {
            get
            {
                TryGetUserRole(userName, out string[] roles);
                return roles;
            }
            set => SaveUserRole(userName, value);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Attempts to retrieve access role for given <paramref name="userName"/>.
        /// </summary>
        /// <param name="userName">User name associated with access role to retrieve.</param>
        /// <param name="roles">Access roles to populate if found.</param>
        /// <returns><c>true</c> if access roles for given <paramref name="userName"/> were retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetUserRole(string userName, out string[] roles)
        {
            string hash = HashLoginID(userName);
            bool result;

            // We wait until the cache is loaded before attempting to access it
            WaitForDataReady();

            // Wait for thread level lock on user info dictionary
            lock (m_userRolesLock)
            {
                // Attempt to lookup persisted access role based on hash of user name
                result = m_userRoles.TryGetValue(hash, out roles);
            }

            return result;
        }

        /// <summary>
        /// Serializes the <paramref name="roles"/> for the given <paramref name="userName"/> into the <see cref="UserRoleCache"/>.
        /// </summary>
        /// <param name="userName">User name associated with access role to retrieve.</param>
        /// <param name="roles">Access roles to update or populate.</param>
        /// <remarks>
        /// <para>
        /// This will add an entry into the user roles cache for <paramref name="userName"/> if it doesn't exist;
        /// otherwise existing entry will be updated.
        /// </para>
        /// <para>
        /// Updates are automatically queued up for serialization so user does not need to call <see cref="Save"/>.
        /// </para>
        /// </remarks>
        public void SaveUserRole(string userName, string[] roles)
        {
            string hash = HashLoginID(userName);

            // We wait until the cache is loaded before attempting to access it
            WaitForDataReady();

            // Wait for thread level lock on user data dictionary
            lock (m_userRolesLock)
            {
                // Assign new user information to user data dictionary
                m_userRoles[hash] = roles;
            }

            // Queue up a serialization for this new user information
            Save();
        }

        /// <summary>
        /// Merge user roles from another <see cref="UserRoleCache"/>, local cache taking precedence.
        /// </summary>
        /// <param name="other">Other <see cref="UserRoleCache"/> to merge with.</param>
        public void MergeLeft(UserRoleCache other)
        {
            // Merge other roles into local ones
            Dictionary<string, string[]> mergedUserRoles = UserRoles.Merge(other.UserRoles);

            // Wait for thread level lock on dictionary
            lock (m_userRolesLock)
            {
                // Replace local user roles dictionary with merged roles
                m_userRoles = mergedUserRoles;
            }

            // Queue up a serialization for any newly added roles
            Save();
        }

        /// <summary>
        /// Merge user roles from another <see cref="UserRoleCache"/>, other cache taking precedence.
        /// </summary>
        /// <param name="other">Other <see cref="UserRoleCache"/> to merge with.</param>
        public void MergeRight(UserRoleCache other)
        {
            // Merge other roles into local ones
            Dictionary<string, string[]> mergedUserRoles = other.UserRoles.Merge(UserRoles);

            // Wait for thread level lock on dictionary
            lock (m_userRolesLock)
            {
                // Replace local user roles dictionary with merged roles
                m_userRoles = mergedUserRoles;
            }

            // Queue up a serialization for any newly added roles
            Save();
        }

        /// <summary>
        /// Initiates inter-process synchronized save of user role cache.
        /// </summary>
        public override void Save()
        {
            byte[] serializedUserDataTable;

            // Wait for thread level lock on dictionary
            lock (m_userRolesLock)
            {
                serializedUserDataTable = Serialization.Serialize(m_userRoles, SerializationFormat.Binary);
            }

            // File data is the serialized user roles dictionary, assignment will initiate auto-save if needed
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
            // Encrypt data local to this machine (this way user cannot copy user role cache to another machine)
            base.SaveFileData(fileStream, DataProtection.Protect(fileData, null, DataProtectionScope.LocalMachine));
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
            byte[] serializedUserRoles = DataProtection.Unprotect(fileStream.ReadStream(), null, DataProtectionScope.LocalMachine);
            Dictionary<string, string[]> userRoles = Serialization.Deserialize<Dictionary<string, string[]>>(serializedUserRoles, SerializationFormat.Binary);

            // Wait for thread level lock on user role dictionary
            lock (m_userRolesLock)
            {
                // Merge new and existing dictionaries since new user roles may have been queued for serialization, but not saved yet
                m_userRoles = userRoles.Merge(m_userRoles);
            }

            return serializedUserRoles;
        }

        /// <summary>
        /// Calculates the hash of the <paramref name="userName"/> used as the key for the user roles dictionary.
        /// </summary>
        /// <param name="userName">User name to hash.</param>
        /// <returns>The Base64 encoded calculated SHA-2 hash of the <paramref name="userName"/> used as the key for the user roles dictionary.</returns>
        /// <remarks>
        /// For added security, a hash of the <paramref name="userName"/> is used as the key for accessing roles
        /// in the user roles cache instead of the actual <paramref name="userName"/>. This method allows the
        /// consumer to properly calculate this hash when directly using the user data cache.
        /// </remarks>
        protected string HashLoginID(string userName)
        {
            return Cipher.GetPasswordHash(userName.ToLower(), 128);
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
                throw new UnauthorizedAccessException("User role cache access failure: timeout while attempting to load user role cache.", ex);
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Loads the <see cref="UserRoleCache"/> for the current local user.
        /// </summary>
        /// <returns>Loaded instance of the <see cref="UserRoleCache"/>.</returns>
        public static UserRoleCache GetCurrentCache()
        {
            UserRoleCache currentCache;
            string localCacheFileName = FilePath.GetAbsolutePath(DefaultCacheFileName);

            // Initialize local user role cache (application may only have read-only access to this cache)
            UserRoleCache localUserRoleCache = new()
            {
                FileName = localCacheFileName,
                ReloadOnChange = false,
                AutoSave = false
            };

            // Load initial user roles
            localUserRoleCache.Load();

            try
            {
                // Validate that user has write access to the local cache folder
                string tempFile = FilePath.GetDirectoryName(localCacheFileName) + Guid.NewGuid() + ".tmp";

                using (File.Create(tempFile))
                {
                }

                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                // No access issues exist, use local cache as the primary cache
                currentCache = localUserRoleCache;
                currentCache.AutoSave = true;
                localUserRoleCache = null;
            }
            catch (UnauthorizedAccessException)
            {
                // User does not have needed serialization access to common cache folder,
                // use a path where user will have rights
                string userCacheFolder = FilePath.AddPathSuffix(FilePath.GetApplicationDataFolder());
                string userCacheFileName = userCacheFolder + FilePath.GetFileName(localCacheFileName);

                // Make sure user directory exists
                if (!Directory.Exists(userCacheFolder))
                    Directory.CreateDirectory(userCacheFolder);

                // Copy existing common cache if none exists
                if (File.Exists(localCacheFileName) && !File.Exists(userCacheFileName))
                    File.Copy(localCacheFileName, userCacheFileName);

                // Initialize primary cache within user folder
                currentCache = new UserRoleCache
                {
                    FileName = userCacheFileName,
                    ReloadOnChange = false,
                    AutoSave = true
                };

                // Load initial roles
                currentCache.Load();

                // Merge new or updated roles, protected folder roles taking precedence over user folder roles
                currentCache.MergeRight(localUserRoleCache);
            }

            localUserRoleCache?.Dispose();

            return currentCache;
        }

        #endregion
    }
}
