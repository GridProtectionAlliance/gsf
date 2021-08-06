//******************************************************************************************************
//  IniFile.cs - Gbtc
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
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated.
//  01/05/2006 - J. Ritchie Carroll
//       2.0 version of source code migrated from 1.1 source (GSF.Interop.Windows.IniFile).
//  01/05/2007 - J. Ritchie Carroll
//       Breaking change: Renamed "IniFileName" property to "FileName".
//       Updated "SectionNames" to use List(Of String) instead of ArrayList.
//  09/10/2008 - J. Ritchie Carroll
//       Converted to C#.
//  04/01/2009 - J. Ritchie Carroll
//       Added "GetSectionKeys" to enumerate keys of a specified section.
//  08/05/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/21/2011 - J. Ritchie Carroll
//       Excluded class from Mono deployments due to P/Invoke requirements.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  08/25/2013 - J. Ritchie Carroll
//       Made INI file work for both Mono and Windows based implementations.
//
//******************************************************************************************************

namespace GSF.Interop
{
    /// <summary>
    /// Represents a Windows INI style configuration file.
    /// </summary>
    public class IniFile
    {
        #region [ Members ]

        // Fields
        private readonly IIniFile m_iniFile;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="IniFile"/>.
        /// </summary>
        /// <remarks>INI file name defaults to "Win.ini" - change using FileName property.</remarks>
        public IniFile()
            : this("Win.ini")
        {
        }

        /// <summary>
        /// Creates a new <see cref="IniFile"/> using the specified INI file name.
        /// </summary>
        /// <param name="fileName">Specified INI file name to use.</param>
        public IniFile(string fileName)
        {
            m_iniFile = Common.IsPosixEnvironment ? 
                new UnixIniFile(fileName) : 
                new WindowsIniFile(fileName);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// File name of the INI file.
        /// </summary>
        public string FileName
        {
            get => m_iniFile.FileName;
            set => m_iniFile.FileName = value;
        }

        /// <summary>
        /// Gets the value of the specified key.
        /// </summary>
        /// <param name="section">Section key exists in.</param>
        /// <param name="entry">Name of key.</param>
        /// <param name="defaultValue">Default value of key.</param>
        /// <returns>Value of key.</returns>
        /// <remarks>This is the default member of this class.</remarks>
        public string this[string section, string entry, string defaultValue] => m_iniFile[section, entry, defaultValue];

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
            get => this[section, entry, null];
            set => m_iniFile[section, entry] = value;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the value of the specified key.
        /// </summary>
        /// <param name="section">Section key exists in.</param>
        /// <param name="entry">Name of key.</param>
        /// <returns>Value of key.</returns>
        public string GetKeyValue(string section, string entry) => 
            this[section, entry, null];

        /// <summary>
        /// Gets the value of the specified key.
        /// </summary>
        /// <param name="section">Section key exists in.</param>
        /// <param name="entry">Name of key.</param>
        /// <param name="defaultValue">Default value of key.</param>
        /// <returns>Value of key.</returns>
        public string GetKeyValue(string section, string entry, string defaultValue) => 
            this[section, entry, defaultValue];

        /// <summary>
        /// Sets the value of the specified key.
        /// </summary>
        /// <param name="section">Section key exists in.</param>
        /// <param name="entry">Name of key.</param>
        /// <param name="newValue">The new key value to store in the INI file.</param>
        public void SetKeyValue(string section, string entry, string newValue) => 
            this[section, entry] = newValue;

        /// <summary>
        /// Gets an array of keys from the specified section in the INI file.
        /// </summary>
        /// <param name="section">Section to retrieve keys from.</param>
        /// <returns>Array of <see cref="string"/> keys from the specified section of the INI file.</returns>
        public string[] GetSectionKeys(string section) => 
            m_iniFile.GetSectionKeys(section);

        /// <summary>
        /// Gets an array of that section names that exist in the INI file.
        /// </summary>
        /// <returns>Array of <see cref="string"/> section names from the INI file.</returns>
        public string[] GetSectionNames() => 
            m_iniFile.GetSectionNames();

        #endregion

        #region [ Static ]

        // Remove any comments from key value string
        internal static string RemoveComments(string keyValue)
        {
            // Remove any trailing comments from key value
            keyValue = keyValue.Trim();

            int commentIndex = keyValue.IndexOf(';');

            if (commentIndex > -1)
                keyValue = keyValue.Substring(0, commentIndex).Trim();

            return keyValue;
        }

        #endregion
    }
}