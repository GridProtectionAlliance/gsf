//******************************************************************************************************
//  ConfigurationFrame.cs - Gbtc
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
//  05/05/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/20/2011 - J. Ritchie Carroll
//       Updated configuration caching algorithm to create multiple backup configurations.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Soap;
using GSF.Collections;
using GSF.Configuration;
using GSF.IO;
using GSF.IO.Checksums;
using GSF.Threading;

namespace GSF.PhasorProtocols.Anonymous
{
    /// <summary>
    /// Represents a protocol independent implementation of a <see cref="IConfigurationFrame"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class ConfigurationFrame : ConfigurationFrameBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from specified parameters.
        /// </summary>
        /// <param name="idCode">The ID code of this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="frameRate">The defined frame rate of this <see cref="ConfigurationFrame"/>.</param>
        public ConfigurationFrame(ushort idCode, Ticks timestamp, ushort frameRate)
            : base(idCode, new ConfigurationCellCollection(), timestamp, frameRate)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new ConfigurationCellCollection Cells
        {
            get
            {
                return base.Cells as ConfigurationCellCollection;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Calculates checksum of given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer image over which to calculate checksum.</param>
        /// <param name="offset">Start index into <paramref name="buffer"/> to calculate checksum.</param>
        /// <param name="length">Length of data within <paramref name="buffer"/> to calculate checksum.</param>
        /// <returns>Checksum over specified portion of <paramref name="buffer"/>.</returns>
        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            // Just returning calculated CRC-CCITT over given buffer as a default CRC
            return buffer.CrcCCITTChecksum(offset, length);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ProcessQueue<Tuple<IConfigurationFrame, Action<Exception>, string>> s_configurationCacheQueue;
        private static string s_configurationCachePath;
        private static int s_configurationBackups;

        // Static Constructor
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ConfigurationFrame()
        {
            s_configurationBackups = -1;
            s_configurationCacheQueue = ProcessQueue<Tuple<IConfigurationFrame, Action<Exception>, string>>.CreateRealTimeQueue(CacheConfigurationFile);
            s_configurationCacheQueue.SynchronizedOperationType = SynchronizedOperationType.LongBackground;
            s_configurationCacheQueue.Start();
        }

        // Static Properties

        /// <summary>
        /// Gets the path for storing serialized phasor protocol configurations.
        /// </summary>
        public static string ConfigurationCachePath
        {
            get
            {
                // This property will not change during system life-cycle so we cache if for future use
                if (string.IsNullOrEmpty(s_configurationCachePath))
                {
                    // Define default configuration cache directory relative to path of host application
                    s_configurationCachePath = string.Format("{0}{1}ConfigurationCache{1}", FilePath.GetAbsolutePath(""), Path.DirectorySeparatorChar);

                    // Make sure configuration cache path setting exists within system settings section of config file
                    ConfigurationFile configFile = ConfigurationFile.Current;
                    CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
                    systemSettings.Add("ConfigurationCachePath", s_configurationCachePath, "Defines the path used to cache serialized phasor protocol configurations");

                    // Retrieve configuration cache directory as defined in the config file
                    s_configurationCachePath = FilePath.AddPathSuffix(systemSettings["ConfigurationCachePath"].Value);

                    // Make sure configuration cache directory exists
                    if (!Directory.Exists(s_configurationCachePath))
                        Directory.CreateDirectory(s_configurationCachePath);
                }

                return s_configurationCachePath;
            }
        }

        /// <summary>
        /// Gets total number of configuration backups to keep when storing serialized phasor protocol configurations.
        /// </summary>
        public static int ConfigurationBackups
        {
            get
            {
                if (s_configurationBackups == -1)
                {
                    const int DefaultConfigurationBackups = 5;

                    // Make sure configuration backups setting exists within system settings section of config file
                    ConfigurationFile configFile = ConfigurationFile.Current;
                    CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
                    systemSettings.Add("ConfigurationBackups", DefaultConfigurationBackups, "Defines the total number of older backup configurations to maintain.");

                    // Retrieve configuration backups value as defined in the config file
                    s_configurationBackups = systemSettings["ConfigurationBackups"].ValueAs(DefaultConfigurationBackups);
                }

                return s_configurationBackups;
            }
        }

        // Static Methods

        /// <summary>
        /// Serializes configuration frame to cache folder on an independent thread for later use (if needed).
        /// </summary>
        /// <param name="configurationFrame">Reference to <see cref="IConfigurationFrame"/>.</param>
        /// <param name="exceptionHandler"><see cref="Action{T}"/> delegate to handle process exceptions.</param>
        /// <param name="configurationName"><see cref="string"/> representing the configuration name.</param>
        public static void Cache(IConfigurationFrame configurationFrame, Action<Exception> exceptionHandler, string configurationName)
        {
            Tuple<IConfigurationFrame, Action<Exception>, string> cacheState = new Tuple<IConfigurationFrame, Action<Exception>, string>(configurationFrame, exceptionHandler, configurationName);
            s_configurationCacheQueue.Add(cacheState);
        }

        // Cache configuration file
        private static void CacheConfigurationFile(Tuple<IConfigurationFrame, Action<Exception>, string> args)
        {
            if ((object)args != null)
            {
                FileStream configFile = null;
                IConfigurationFrame configurationFrame = args.Item1;
                Action<Exception> exceptionHandler = args.Item2;
                string configurationName = args.Item3;
                string configurationCacheFileName = GetConfigurationCacheFileName(configurationName);

                try
                {
                    // Create multiple backup configurations, if requested
                    for (int i = ConfigurationBackups; i > 0; i--)
                    {
                        string origConfigFile = configurationCacheFileName + ".backup" + (i == 1 ? "" : (i - 1).ToString());

                        if (File.Exists(origConfigFile))
                        {
                            string nextConfigFile = configurationCacheFileName + ".backup" + i;

                            if (File.Exists(nextConfigFile))
                                File.Delete(nextConfigFile);

                            File.Move(origConfigFile, nextConfigFile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptionHandler(new InvalidOperationException(string.Format("Failed to create extra backup serialized configuration frames due to exception: {0}", ex.Message)));
                }

                try
                {
                    if (ConfigurationBackups > 0)
                    {
                        // Back up current configuration file, if any
                        if (File.Exists(configurationCacheFileName))
                        {
                            string backupConfigFile = configurationCacheFileName + ".backup";

                            if (File.Exists(backupConfigFile))
                                File.Delete(backupConfigFile);

                            File.Move(configurationCacheFileName, backupConfigFile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptionHandler(new InvalidOperationException(string.Format("Failed to backup last serialized configuration frame due to exception: {0}", ex.Message)));
                }

                try
                {
                    // Serialize configuration frame to a file
                    SoapFormatter xmlSerializer = new SoapFormatter();
                    xmlSerializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                    xmlSerializer.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;

                    configFile = File.Create(configurationCacheFileName);
                    xmlSerializer.Serialize(configFile, configurationFrame);
                }
                catch (Exception ex)
                {
                    exceptionHandler(new InvalidOperationException(string.Format("Failed to serialize configuration frame: {0}", ex.Message), ex));
                }
                finally
                {
                    if ((object)configFile != null)
                        configFile.Close();
                }
            }
        }

        /// <summary>
        /// Gets the file name with path of the specified <paramref name="configurationName"/>.
        /// </summary>
        /// <param name="configurationName">Name of the configuration to get file name for.</param>
        /// <returns>File name with path of the specified <paramref name="configurationName"/>.</returns>
        public static string GetConfigurationCacheFileName(string configurationName)
        {
            return string.Format("{0}{1}.configuration.xml", ConfigurationCachePath, configurationName.ReplaceCharacters('_', c => Path.GetInvalidFileNameChars().Contains(c)));
        }

        /// <summary>
        /// Deletes the cached configuration, if defined.
        /// </summary>
        /// <param name="configurationName">Name of the configuration to delete.</param>
        public static void DeleteCachedConfiguration(string configurationName)
        {
            string configFileName = GetConfigurationCacheFileName(configurationName);

            if (File.Exists(configFileName))
                File.Delete(configFileName);
        }

        /// <summary>
        /// Deserializes cached configuration, if available.
        /// </summary>
        /// <param name="configurationName">Name of the configuration to get file name for.</param>
        /// <param name="fromCache">Set to True retrieve from cache, False to retrieve from specified file name in <paramref name="configurationName"/></param>
        /// <returns>Cached configuration frame, or null if not available.</returns>
        public static IConfigurationFrame GetCachedConfiguration(string configurationName, bool fromCache)
        {
            IConfigurationFrame configFrame = null;
            string configFileName;

            if (fromCache)
                configFileName = GetConfigurationCacheFileName(configurationName);
            else
                configFileName = configurationName;

            if (File.Exists(configFileName))
                configFrame = Common.DeserializeConfigurationFrame(configFileName);

            return configFrame;
        }

        #endregion
    }
}