//*******************************************************************************************************
//  IniFile.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - James R. Carroll
//       Initial version of source generated
//  01/05/2006 - James R. Carroll
//       2.0 version of source code migrated from 1.1 source (TVA.Interop.Windows.IniFile)
//  01/05/2007 - James R. Carroll
//       Breaking change: Renamed "IniFileName" property to "FileName"
//       Updated "SectionNames" to use List(Of String) instead of ArrayList
//  09/10/2008 - James R. Carroll
//       Converted to C#
//  04/01/2009 - James R. Carroll
//       Added "GetSectionKeys" to enumerate keys of a specified section.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TVA.Interop
{
    /// <summary>
    /// Represents a Windows INI style configuration file.
    /// </summary>
    public class IniFile
    {
        #region [ Members ]

        // Constants
        private const int BufferSize = 32768;

        // Fields
        private string m_fileName;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="IniFile"/>.
        /// </summary>
        /// <remarks>INI file name defaults to "Win.ini" - change using FileName property.</remarks>
        public IniFile()
        {
            m_fileName = "Win.ini";
        }

        /// <summary>
        /// Creates a new <see cref="IniFile"/> using the specified INI file name.
        /// </summary>
        public IniFile(string fileName)
        {
            m_fileName = fileName;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// File name of the INI file.
        /// </summary>
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                m_fileName = value;
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
                const int BufferSize = 4096;
                StringBuilder buffer = new StringBuilder(BufferSize);

                if (defaultValue == null)
                    defaultValue = "";

                GetPrivateProfileString(section, entry, defaultValue, buffer, BufferSize, m_fileName);

                return RemoveComments(buffer.ToString());
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
            get
            {
                return this[section, entry, null];
            }
            set
            {
                WritePrivateProfileString(section, entry, value, m_fileName);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the value of the specified key.
        /// </summary>
        /// <param name="section">Section key exists in.</param>
        /// <param name="entry">Name of key.</param>
        /// <returns>Value of key.</returns>
        public string GetKeyValue(string section, string entry)
        {
            return this[section, entry, null];
        }

        /// <summary>
        /// Gets the value of the specified key.
        /// </summary>
        /// <param name="section">Section key exists in.</param>
        /// <param name="entry">Name of key.</param>
        /// <param name="defaultValue">Default value of key.</param>
        /// <returns>Value of key.</returns>
        public string GetKeyValue(string section, string entry, string defaultValue)
        {
            return this[section, entry, defaultValue];
        }

        /// <summary>
        /// Sets the value of the specified key.
        /// </summary>
        /// <param name="section">Section key exists in.</param>
        /// <param name="entry">Name of key.</param>
        /// <param name="newValue">The new key value to store in the INI file.</param>
        public void SetKeyValue(string section, string entry, string newValue)
        {
            this[section, entry] = newValue;
        }

        /// <summary>
        /// Gets an array of keys from the specified section in the INI file.
        /// </summary>
        /// <param name="section">Section to retrieve keys from.</param>
        public string[] GetSectionKeys(string section)
        {
            List<string> keys = new List<string>();
            byte[] buffer = new byte[BufferSize];
            int startIndex = 0;
            int readLength;
            int nullIndex;

            readLength = GetPrivateProfileSection(section, buffer, BufferSize, m_fileName);

            if (readLength > 0)
            {
                while (startIndex < readLength)
                {
                    nullIndex = Array.IndexOf(buffer, (byte)0, startIndex);

                    if (nullIndex > -1)
                    {
                        if (buffer[startIndex] > 0)
                            keys.Add(RemoveComments(Encoding.Default.GetString(buffer, startIndex, nullIndex - startIndex).RemoveCrLfs().Split('=')[0]));

                        startIndex = nullIndex + 1;
                    }
                    else
                        break;
                }
            }

            return keys.ToArray();
        }

        /// <summary>
        /// Gets an array of that section names that exist in the INI file.
        /// </summary>
        public string[] GetSectionNames()
        {
            List<string> sections = new List<string>();
            byte[] buffer = new byte[BufferSize];
            int startIndex = 0;
            int readLength;
            int nullIndex;

            readLength = GetPrivateProfileSectionNames(buffer, BufferSize, m_fileName);

            if (readLength > 0)
            {
                while (startIndex < readLength)
                {
                    nullIndex = Array.IndexOf(buffer, (byte)0, startIndex);

                    if (nullIndex > -1)
                    {
                        if (buffer[startIndex] > 0)
                            sections.Add(Encoding.Default.GetString(buffer, startIndex, nullIndex - startIndex).Trim());

                        startIndex = nullIndex + 1;
                    }
                    else
                        break;
                }
            }

            return sections.ToArray();
        }

        #endregion

        #region [ Static ]

        // Static Methods
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileStringW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32", EntryPoint = "WritePrivateProfileStringW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("kernel32", EntryPoint = "GetPrivateProfileSectionW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        [DllImport("kernel32", EntryPoint = "GetPrivateProfileSectionNamesW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetPrivateProfileSectionNames(byte[] lpszReturnBuffer, int nSize, string lpFileName);

        // Remove any comments from key value string
        private static string RemoveComments(string keyValue)
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