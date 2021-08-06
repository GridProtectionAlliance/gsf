//******************************************************************************************************
//  UnixIniFile.cs - Gbtc
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
//  08/25/2013 - J. Ritchie Carroll
//       Made INI file work for both Mono and Windows based implementations.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GSF.Interop
{
    /// <summary>
    /// Represents a Unix INI style configuration file.
    /// </summary>
    internal class UnixIniFile : IIniFile
    {
        #region [ Members ]

        // Fields
        private string m_fileName;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> m_iniData;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="UnixIniFile"/> using the specified INI file name.
        /// </summary>
        /// <param name="fileName">Specified INI file name to use.</param>
        public UnixIniFile(string fileName)
        {
            m_fileName = fileName;
            m_iniData = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>(StringComparer.CurrentCultureIgnoreCase);
            Load();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// File name of the INI file.
        /// </summary>
        public string FileName
        {
            get => m_fileName;
            set
            {
                m_fileName = value;
                Load();
            }
        }

        /// <summary>
        /// Gets the value of the specified key.
        /// </summary>
        /// <param name="section">Section key exists in.</param>
        /// <param name="entry">Name of key.</param>
        /// <param name="defaultValue">Default value of key.</param>
        /// <returns>Value of key.</returns>
        /// <remarks>This is the default member of this class.</remarks>
        public string this[string section, string entry, string defaultValue]
        {
            get
            {
                ConcurrentDictionary<string, string> iniSection = m_iniData.GetOrAdd(section, CreateNewSection);
                return iniSection.GetOrAdd(entry, defaultValue);
            }
        }

        /// <summary>
        /// Gets or sets the value of the specified key.
        /// </summary>
        /// <param name="section">Section key exists in.</param>
        /// <param name="entry">Name of key.</param>
        /// <value>The new key value to store in the INI file.</value>
        /// <returns>Value of key.</returns>
        /// <remarks>This is the default member of this class.</remarks>
        public string this[string section, string entry]
        {
            set
            {
                ConcurrentDictionary<string, string> iniSection = m_iniData.GetOrAdd(section, CreateNewSection);
                iniSection[entry] = value;
                Save();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets an array of keys from the specified section in the INI file.
        /// </summary>
        /// <param name="section">Section to retrieve keys from.</param>
        /// <returns>Array of <see cref="string"/> keys from the specified section of the INI file.</returns>
        public string[] GetSectionKeys(string section) =>
            m_iniData.TryGetValue(section, out ConcurrentDictionary<string, string> sectionEntries) ?
                sectionEntries.Keys.ToArray() :
                Array.Empty<string>();

        /// <summary>
        /// Gets an array of that section names that exist in the INI file.
        /// </summary>
        /// <returns>Array of <see cref="string"/> section names from the INI file.</returns>
        public string[] GetSectionNames() =>
            m_iniData.Keys.ToArray();

        private void Load()
        {
            m_iniData.Clear();

            if (!File.Exists(m_fileName))
                return;

            using StreamReader reader = new(m_fileName);
            string line = reader.ReadLine();
            ConcurrentDictionary<string, string> section = null;

            while (line is not null)
            {
                line = IniFile.RemoveComments(line);

                if (line.Length > 0)
                {
                    // Check for new section				
                    int startBracketIndex = line.IndexOf('[');

                    if (startBracketIndex == 0)
                    {
                        int endBracketIndex = line.IndexOf(']');

                        if (endBracketIndex > 1)
                        {
                            string sectionName = line.Substring(startBracketIndex + 1, endBracketIndex - 1);

                            if (!string.IsNullOrEmpty(sectionName))
                                section = m_iniData.GetOrAdd(sectionName, CreateNewSection);
                        }
                    }

                    if (section is null)
                        throw new InvalidOperationException("INI file did not begin with a [section]");

                    // Check for key/value pair
                    int equalsIndex = line.IndexOf("=", StringComparison.Ordinal);

                    if (equalsIndex > 0)
                    {
                        string key = line.Substring(0, equalsIndex).Trim();

                        if (!string.IsNullOrEmpty(key))
                            section[key] = line.Substring(equalsIndex + 1).Trim();
                    }
                }

                line = reader.ReadLine();
            }
        }

        private void Save()
        {
            // Saving INI file will strip comments - sorry :-(
            using StreamWriter writer = new(m_fileName);

            foreach (KeyValuePair<string, ConcurrentDictionary<string, string>> section in m_iniData)
            {
                writer.WriteLine("[{0}]", section.Key);

                foreach (KeyValuePair<string, string> entry in section.Value)
                    writer.WriteLine("{0} = {1}", entry.Key, entry.Value);

                writer.WriteLine();
            }
        }

        private ConcurrentDictionary<string, string> CreateNewSection(string sectionName) => 
            new(StringComparer.OrdinalIgnoreCase);

        #endregion
    }
}