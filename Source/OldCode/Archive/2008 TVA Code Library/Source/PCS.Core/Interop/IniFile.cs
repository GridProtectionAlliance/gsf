//*******************************************************************************************************
//  IniFile.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//  01/05/2006 - J. Ritchie Carroll
//       2.0 version of source code migrated from 1.1 source (PCS.Interop.Windows.IniFile)
//  01/05/2007 - J. Ritchie Carroll
//       Breaking change: Renamed "IniFileName" property to "FileName"
//       Updated "SectionNames" to use List(Of String) instead of ArrayList
//  09/10/2008 - J. Ritchie Carroll
//      Converted to C#
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PCS.Interop
{
    /// <summary>Old style Windows INI file manipulation class</summary>
    public class IniFile
    {
        #region [ Members ]

        // Fields
        private string m_fileName;

        #endregion

        #region [ Constructors ]

        /// <summary>Creates a new instance of IniFile class</summary>
        /// <remarks>Ini file name defaults to "Win.ini" - change using FileName property</remarks>
        public IniFile()
        {
            m_fileName = "Win.ini";
        }

        /// <summary>Creates a new instance of IniFile class using the specified INI file name</summary>
        public IniFile(string fileName)
        {
            m_fileName = fileName;
        }

        #endregion

        #region [ Properties ]

        /// <summary>File name of the INI file</summary>
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

        /// <summary>Gets the value of the specified key</summary>
        /// <param name="section">Section key exists in</param>
        /// <param name="entry">Name of key</param>
        /// <param name="defaultValue">Default value of key</param>
        /// <returns>Value of key</returns>
        /// <remarks>This is the default member of this class</remarks>
        public string this[string section, string entry, string defaultValue]
        {
            get
            {
                const int BufferSize = 4096;
                StringBuilder buffer = new StringBuilder(BufferSize);
                int commentIndex;
                string keyValue;

                if (defaultValue == null) defaultValue = "";
                GetPrivateProfileString(section, entry, defaultValue, buffer, BufferSize, m_fileName);

                // Remove any trailing comments from key value
                keyValue = buffer.ToString().Trim();
                commentIndex = keyValue.IndexOf(';');
                if (commentIndex > -1) keyValue = keyValue.Substring(0, commentIndex).Trim();

                return keyValue;
            }
        }

        /// <summary>Gets or sets the value of the specified key</summary>
        /// <param name="section">Section key exists in</param>
        /// <param name="entry">Name of key</param>
        /// <value>The new key value to store in the INI file</value>
        /// <returns>Value of key</returns>
        /// <remarks>This is the default member of this class</remarks>
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

        /// <summary>Returns a string array of section names in the INI file</summary>
        public string[] SectionNames
        {
            get
            {
                const int BufferSize = 32768;
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
                        nullIndex = Array.IndexOf(buffer, Convert.ToByte(0), startIndex);

                        if (nullIndex > -1)
                        {
                            if (buffer[startIndex] > 0)
                            {
                                sections.Add(Encoding.Default.GetString(buffer, startIndex, nullIndex - startIndex).Trim());
                            }
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
        }

        #endregion

        #region [ Methods ]

        /// <summary>Gets the value of the specified key</summary>
        /// <param name="section">Section key exists in</param>
        /// <param name="entry">Name of key</param>
        /// <returns>Value of key</returns>
        public string GetKeyValue(string section, string entry)
        {
            return this[section, entry, null];
        }

        /// <summary>Gets the value of the specified key</summary>
        /// <param name="section">Section key exists in</param>
        /// <param name="entry">Name of key</param>
        /// <param name="defaultValue">Default value of key</param>
        /// <returns>Value of key</returns>
        public string GetKeyValue(string section, string entry, string defaultValue)
        {
            return this[section, entry, defaultValue];
        }

        /// <summary>Sets the value of the specified key</summary>
        /// <param name="section">Section key exists in</param>
        /// <param name="entry">Name of key</param>
        /// <param name="newValue">The new key value to store in the INI file</param>
        public void SetKeyValue(string section, string entry, string newValue)
        {
            this[section, entry] = newValue;
        }

        #endregion

        #region [ Static ]

        // Static Methods
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32", EntryPoint = "WritePrivateProfileStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("kernel32", EntryPoint = "GetPrivateProfileSectionNamesA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int GetPrivateProfileSectionNames(byte[] lpszReturnBuffer, int nSize, string lpFileName);

        #endregion
    }
}