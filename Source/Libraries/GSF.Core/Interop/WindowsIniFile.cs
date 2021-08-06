//******************************************************************************************************
//  WindowsIniFile.cs - Gbtc
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GSF.Interop
{
    /// <summary>
    /// Represents a Windows INI style configuration file.
    /// </summary>
    public class WindowsIniFile : IIniFile
    {
        #region [ Members ]

        // Constants
        private const int BufferSize = 32768;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="WindowsIniFile"/> using the specified INI file name.
        /// </summary>
        /// <param name="fileName">Specified INI file name to use.</param>
        public WindowsIniFile(string fileName) => 
            FileName = fileName;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// File name of the INI file.
        /// </summary>
        public string FileName { get; set; }

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
                StringBuilder buffer = new(BufferSize);
                GetPrivateProfileString(section, entry, defaultValue ?? "", buffer, BufferSize, FileName);
                return IniFile.RemoveComments(buffer.ToString());
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
            get => this[section, entry, null];
            set => WritePrivateProfileString(section, entry, value, FileName);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets an array of keys from the specified section in the INI file.
        /// </summary>
        /// <param name="section">Section to retrieve keys from.</param>
        /// <returns>Array of <see cref="string"/> keys from the specified section of the INI file.</returns>
        public string[] GetSectionKeys(string section)
        {
            List<string> keys = new();
            byte[] buffer = new byte[BufferSize];
            int startIndex = 0;

            int readLength = GetPrivateProfileSection(section, buffer, BufferSize, FileName);

            if (readLength > 0)
            {
                while (startIndex < readLength)
                {
                    int nullIndex = Array.IndexOf(buffer, (byte)0, startIndex);

                    if (nullIndex > -1)
                    {
                        if (buffer[startIndex] > 0)
                            keys.Add(IniFile.RemoveComments(Encoding.Default.GetString(buffer, startIndex, nullIndex - startIndex).RemoveCrLfs().Split('=')[0]));

                        startIndex = nullIndex + 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return keys.ToArray();
        }

        /// <summary>
        /// Gets an array of that section names that exist in the INI file.
        /// </summary>
        /// <returns>Array of <see cref="string"/> section names from the INI file.</returns>
        public string[] GetSectionNames()
        {
            List<string> sections = new();
            byte[] buffer = new byte[BufferSize];
            int startIndex = 0;

            int readLength = GetPrivateProfileSectionNames(buffer, BufferSize, FileName);

            if (readLength > 0)
            {
                while (startIndex < readLength)
                {
                    int nullIndex = Array.IndexOf(buffer, (byte)0, startIndex);

                    if (nullIndex > -1)
                    {
                        if (buffer[startIndex] > 0)
                            sections.Add(Encoding.Default.GetString(buffer, startIndex, nullIndex - startIndex).Trim());

                        startIndex = nullIndex + 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return sections.ToArray();
        }

        #endregion

        #region [ Static ]

        // Static Methods
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileString", BestFitMapping = false)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32", EntryPoint = "WritePrivateProfileString", BestFitMapping = false)]
        private static extern int WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("kernel32", EntryPoint = "GetPrivateProfileSection", BestFitMapping = false)]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        [DllImport("kernel32", EntryPoint = "GetPrivateProfileSectionNames", BestFitMapping = false)]
        private static extern int GetPrivateProfileSectionNames(byte[] lpszReturnBuffer, int nSize, string lpFileName);

        #endregion
    }
}