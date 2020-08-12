//******************************************************************************************************
//  StatHistorianReader.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  02/11/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GSF;
using GSF.Configuration;
using GSF.Diagnostics;
using GSF.IO;
using Microsoft.Win32;

// ReSharper disable UnusedMember.Global
namespace StatHistorianReportGenerator
{
    /// <summary>
    /// Searches the registry as well as well-known locations for openHistorian 1.0 archives.
    /// </summary>
    public class ArchiveLocator
    {
        #region [ Members ]

        // Fields
        private string m_archiveLocation;
        private string m_archiveLocationName;
        private string m_archiveName;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the archive location. Querying this property without
        /// setting it will initiate a search of the registry and well-known
        /// locations where the archive might be located.
        /// </summary>
        public string ArchiveLocation
        {
            get
            {
                if (string.IsNullOrEmpty(m_archiveLocation))
                    InitializeArchiveLocation();

                return m_archiveLocation;
            }
            set
            {
                m_archiveLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the folder containing the
        /// archive files to facilitate more accurate searching.
        /// </summary>
        public string ArchiveLocationName
        {
            get
            {
                return m_archiveLocationName;
            }
            set
            {
                m_archiveLocationName = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the archive to be searched for.
        /// </summary>
        public string ArchiveName
        {
            get
            {
                return m_archiveName;
            }
            set
            {
                m_archiveName = value;
            }
        }

        /// <summary>
        /// Gets the name of the archive file.
        /// </summary>
        public string ArchiveFileName
        {
            get
            {
                if ((object)m_archiveName == null)
                    return null;

                return m_archiveName + "_archive.d";
            }
        }

        /// <summary>
        /// Gets the name of the metadata file.
        /// </summary>
        public string MetadataFileName
        {
            get
            {
                if ((object)m_archiveName == null)
                    return null;

                return m_archiveName + "_dbase.dat";
            }
        }

        /// <summary>
        /// Gets the name of the state file.
        /// </summary>
        public string StateFileName
        {
            get
            {
                if ((object)m_archiveName == null)
                    return null;

                return m_archiveName + "_startup.dat";
            }
        }

        /// <summary>
        /// Gets the name of the intercom file.
        /// </summary>
        public string IntercomFileName
        {
            get
            {
                if ((object)m_archiveName == null)
                    return null;

                return "scratch.dat";
            }
        }

        /// <summary>
        /// Gets the full path to the archive file.
        /// </summary>
        public string ArchiveFilePath
        {
            get
            {
                return Path.Combine(ArchiveLocation, ArchiveFileName);
            }
        }

        /// <summary>
        /// Gets the full path to the metadata file.
        /// </summary>
        public string MetadataFilePath
        {
            get
            {
                return Path.Combine(ArchiveLocation, MetadataFileName);
            }
        }

        /// <summary>
        /// Gets the full path to the state file.
        /// </summary>
        public string StateFilePath
        {
            get
            {
                return Path.Combine(ArchiveLocation, StateFileName);
            }
        }

        /// <summary>
        /// Gets the full path to the intercom file.
        /// </summary>
        public string IntercomFilePath
        {
            get
            {
                return Path.Combine(ArchiveLocation, IntercomFileName);
            }
        }

        #endregion

        #region [ Methods ]

        // Initiates the search for the archive location.
        // This will check the configuration file setting before
        // resorting to the registry or searching well-known locations.
        private void InitializeArchiveLocation()
        {
            CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings["systemSettings"];
            CategorizedSettingsElement archiveLocationSetting = systemSettings["ArchiveLocation"];

            if ((object)archiveLocationSetting != null)
                m_archiveLocation = FilePath.GetAbsolutePath(archiveLocationSetting.Value);

            if (string.IsNullOrEmpty(m_archiveLocation) && !TryLocateArchive(out m_archiveLocation))
                throw new DirectoryNotFoundException($"Unable to locate {m_archiveName.ToNonNullString().ToUpper()} archive. Please check the configuration file to set the archive location.");
        }

        // Attempts to locate the archive by searching the registry and well-known locations.
        private bool TryLocateArchive(out string archiveLocation)
        {
            if (IsArchiveLocation(m_archiveLocationName))
            {
                archiveLocation = m_archiveLocationName;
                return true;
            }

            return SearchConfigFiles(out archiveLocation) || SearchRegistry(out archiveLocation) || SearchProgramFiles(out archiveLocation);
        }

        // Searches local config files to find the archive location.
        private bool SearchConfigFiles(out string archiveLocation)
        {
            try
            {
                XDocument serviceConfig = XDocument.Load(GetConfigurationFileName());

                string archiveFileName = serviceConfig
                    .Descendants($"{ArchiveName.ToLowerInvariant()}ArchiveFile")
                    .SelectMany(systemSettings => systemSettings.Elements("add"))
                    .Where(element => "FileName".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                    .Select(element => (string)element.Attribute("value"))
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(archiveFileName) && File.Exists(archiveFileName))
                {
                    archiveLocation = Path.GetDirectoryName(archiveFileName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Program.Log.Publish(MessageLevel.Warning, "SearchConfigFiles", "Failed search local config files for archive location.", exception: ex);
            }

            archiveLocation = null;
            return false;
        }

        // Searches the registry to find the archive location.
        private bool SearchRegistry(out string archiveLocation)
        {
            RegistryKey gpaKey;

            try
            {
                gpaKey = Registry.LocalMachine.OpenSubKey(@"Software\Grid Protection Alliance");

                if ((object)gpaKey != null)
                {
                    archiveLocation = gpaKey.GetSubKeyNames()
                        .Select(name => TryOpenSubKey(gpaKey, name))
                        .Where(productKey => (object)productKey != null)
                        .Select(productKey => productKey.GetValue("InstallPath") as string)
                        .Where(installPath => !string.IsNullOrEmpty(installPath))
                        .Select(installPath => Path.Combine(installPath, m_archiveLocationName))
                        .FirstOrDefault(IsArchiveLocation);

                    return !string.IsNullOrWhiteSpace(archiveLocation);
                }
            }
            catch (Exception ex)
            {
                Program.Log.Publish(MessageLevel.Warning, "SearchRegistry", "Failed search registry for archive location.", exception: ex);
            }

            archiveLocation = null;
            return false;
        }

        // Searches the Program Files folder for well-known, default statistics archive locations.
        private bool SearchProgramFiles(out string archiveLocation)
        {
            try
            {
                archiveLocation = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles))
                    .Select(programPath => Path.Combine(programPath, m_archiveLocationName))
                    .FirstOrDefault(IsArchiveLocation);

                return !string.IsNullOrWhiteSpace(archiveLocation);
            }
            catch (Exception ex)
            {
                Program.Log.Publish(MessageLevel.Warning, "SearchProgramFiles", "Failed search program files for archive location.", exception: ex);
            }

            archiveLocation = null;
            return false;
        }

        // Determines if the given path points to a valid archive location.
        private bool IsArchiveLocation(string path)
        {
            string fullPath;

            try
            {
                fullPath = FilePath.GetAbsolutePath(path);

                if (Directory.Exists(fullPath))
                {
                    return File.Exists(Path.Combine(fullPath, ArchiveFileName))
                        && File.Exists(Path.Combine(fullPath, MetadataFileName))
                        && File.Exists(Path.Combine(fullPath, StateFileName))
                        && File.Exists(Path.Combine(fullPath, IntercomFileName));
                }
            }
            catch (Exception ex)
            {
                Program.Log.Publish(MessageLevel.Warning, "IsArchiveLocation", "Failed test path for valid archive location.", exception: ex);
            }

            return false;
        }

        #endregion

        #region [ Static ]

        // Static Properties
        public static string HostConfigFile;

        // Static Methods

        // Attempts to open a sub key of the given registry key and returns null if it fails.
        private static RegistryKey TryOpenSubKey(RegistryKey key, string name)
        {
            try
            {
                return key.OpenSubKey(name);
            }
            catch
            {
                return null;
            }
        }

        public static string GetConfigurationFileName()
        {
            if (!string.IsNullOrEmpty(HostConfigFile) && File.Exists(HostConfigFile))
                return HostConfigFile;

            // Will be faster to load known config file, try a few common ones first
            string[] knownConfigurationFileNames =
            {
                "openPDC.exe.config",
                "SIEGate.exe.config",
                "openHistorian.exe.config",
                "substationSBG.exe.config",
                "openMIC.exe.config",
                "PDQTracker.exe.config",
                "openECA.exe.config"
            };

            // Search for the file name in the list of known configuration files
            foreach (string fileName in knownConfigurationFileNames)
            {
                string absolutePath = FilePath.GetAbsolutePath(fileName);

                if (File.Exists(absolutePath))
                    return absolutePath;
            }

            // Fall back on manual search
            foreach (string configFile in FilePath.GetFileList($"{FilePath.AddPathSuffix(FilePath.GetAbsolutePath(""))}*.exe.config"))
            {
                try
                {
                    XDocument serviceConfig = XDocument.Load(configFile);

                    string applicationName = serviceConfig
                        .Descendants("securityProvider")
                        .SelectMany(systemSettings => systemSettings.Elements("add"))
                        .Where(element => "ApplicationName".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                        .Select(element => (string)element.Attribute("value"))
                        .FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(applicationName))
                        continue;

                    if (applicationName.Trim().Equals(FilePath.GetFileNameWithoutExtension(FilePath.GetFileNameWithoutExtension(configFile)), StringComparison.OrdinalIgnoreCase))
                        return configFile;
                }
                catch (Exception ex)
                {
                    // Just move on to the next config file if XML parsing fails
                    Program.Log.Publish(MessageLevel.Warning, "GetConfigurationFileName", $"Failed parse config file \"{configFile}\".", exception: ex);
                }
            }

            return ConfigurationFile.Current.Configuration.FilePath;
        }

        #endregion
    }
}