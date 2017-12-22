//******************************************************************************************************
//  AdoSecurityCache.cs - Gbtc
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
//  12/30/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using GSF.Configuration;
using GSF.Data;
using GSF.IO;
using GSF.Threading;

namespace GSF.Security
{
    /// <summary>
    /// Represents a secured inter-process cache for the security context needed by the <see cref="AdoSecurityProvider"/>.
    /// </summary>
    /// <remarks>
    /// This is a system cache that contains the last known security information loaded from the database related to users,
    /// roles and groups. This cache allows the <see cref="AdoSecurityProvider"/> to start-up without database access
    /// using latest cached security context. Even though this cache contains role information for all users, it does
    /// not overlap information in the <see cref="UserRoleCache"/> since the user role cache contains role assignments
    /// for a user at last login and is used for auditing.
    /// </remarks>
    public class AdoSecurityCache : InterprocessCache
    {
        #region [ Members ]

        // Constants

        // Default ADO security cache file name
        private const string DefaultCacheFileName = "AdoSecurityCache.bin";

        // Fields
        private DataSet m_dataSet;              // Internal ADO security data set
        private readonly object m_dataSetLock;  // Lock object for internal data set

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AdoSecurityCache"/> with the specified number of <paramref name="maximumConcurrentLocks"/>.
        /// </summary>
        /// <param name="maximumConcurrentLocks">Maximum concurrent reader locks to allow.</param>
        public AdoSecurityCache(int maximumConcurrentLocks = InterprocessReaderWriterLock.DefaultMaximumConcurrentLocks)
            : base(maximumConcurrentLocks)
        {
            m_dataSet = new DataSet("AdoSecurityContext");
            m_dataSetLock = new object();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the internal <see cref="DataSet"/>; returned value will be a copy of the internal.
        /// </summary>
        public DataSet DataSet
        {
            get
            {
                DataSet dataSet;

                // We wait until the data set cache is loaded before attempting to access it
                WaitForDataReady();

                // Wait for thread level lock on data set
                lock (m_dataSetLock)
                {
                    // Since user could changes, make a copy of the data set for external use
                    dataSet = m_dataSet.Copy();
                }

                return dataSet;
            }
            set
            {
                // Wait for thread level lock on data set
                lock (m_dataSetLock)
                {
                    // Assign new data set
                    m_dataSet = value;
                }

                // Queue up a serialization for this new data set
                Save();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initiates inter-process synchronized save of <see cref="DataSet"/>.
        /// </summary>
        public override void Save()
        {
            byte[] serializedDataSet;

            // Wait for thread level lock on data set
            lock (m_dataSetLock)
            {
                using (BlockAllocatedMemoryStream stream = new BlockAllocatedMemoryStream())
                {
                    m_dataSet.SerializeToStream(stream);
                    serializedDataSet = stream.ToArray();
                }
            }

            // File data is the serialized data set, assignment will initiate auto-save if needed
            FileData = serializedDataSet;
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
            // Encrypt data local to this machine (this way user cannot copy ADO security cache to another machine)
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
            byte[] serializedDataSet = ProtectedData.Unprotect(fileStream.ReadStream(), null, DataProtectionScope.LocalMachine);
            DataSet dataSet;

            using (MemoryStream stream = new MemoryStream(serializedDataSet))
            {
                dataSet = stream.DeserializeToDataSet();
            }

            // Wait for thread level lock on data set
            lock (m_dataSetLock)
            {
                m_dataSet = dataSet;
            }

            return serializedDataSet;
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
                throw new UnauthorizedAccessException("ADO security cache access failure: timeout while attempting to load last valid security context.", ex);
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Loads the <see cref="AdoSecurityCache"/> for the current local user.
        /// </summary>
        /// <returns>Loaded instance of the <see cref="AdoSecurityCache"/>.</returns>
        public static AdoSecurityCache GetCurrentCache()
        {
            AdoSecurityCache currentCache;
            AdoSecurityCache localSecurityCache = null;

            // Define default cache path
            string cachePath = null;

            try
            {
                // Attempt to retrieve configuration cache path as defined in the config file
                ConfigurationFile configFile = ConfigurationFile.Current;
                CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
                CategorizedSettingsElement configurationCachePathSetting = systemSettings["ConfigurationCachePath"];

                if ((object)configurationCachePathSetting != null)
                    cachePath = FilePath.GetAbsolutePath(systemSettings["ConfigurationCachePath"].Value);

                if (string.IsNullOrEmpty(cachePath))
                    cachePath = string.Format("{0}{1}ConfigurationCache{1}", FilePath.GetAbsolutePath(""), Path.DirectorySeparatorChar);
            }
            catch (ConfigurationErrorsException)
            {
                cachePath = string.Format("{0}{1}ConfigurationCache{1}", FilePath.GetAbsolutePath(""), Path.DirectorySeparatorChar);
            }

            string localCacheFileName = Path.Combine(cachePath, DefaultCacheFileName);

            try
            {
                // Make sure configuration cache path exists
                if (!Directory.Exists(cachePath))
                    Directory.CreateDirectory(cachePath);

                // Initialize local ADO security cache (application may only have read-only access to this cache)
                localSecurityCache = new AdoSecurityCache
                {
                    FileName = localCacheFileName,
                    ReloadOnChange = false,
                    AutoSave = false
                };

                // Load initial ADO security data set
                localSecurityCache.Load();

                // Validate that current user has write access to the local cache folder
                string tempFile = FilePath.GetDirectoryName(localCacheFileName) + Guid.NewGuid() + ".tmp";

                using (File.Create(tempFile))
                {
                }

                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                // No access issues exist, use local cache as the primary cache
                currentCache = localSecurityCache;
                currentCache.AutoSave = true;
                localSecurityCache = null;
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
                currentCache = new AdoSecurityCache
                {
                    FileName = userCacheFileName,
                    ReloadOnChange = false,
                    AutoSave = true
                };

                // Load initial ADO security data set
                currentCache.Load();

                // Update user located security cache if locally located security cache is newer
                if ((object)localSecurityCache != null && File.Exists(localCacheFileName) && File.Exists(userCacheFileName) && File.GetLastWriteTime(localCacheFileName) > File.GetLastWriteTime(userCacheFileName))
                    currentCache.DataSet = localSecurityCache.DataSet;
            }

            if ((object)localSecurityCache != null)
                localSecurityCache.Dispose();

            return currentCache;
        }

        #endregion
    }
}
