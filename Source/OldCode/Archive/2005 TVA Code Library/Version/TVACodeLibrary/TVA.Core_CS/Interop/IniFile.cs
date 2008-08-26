using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
//using TVA.Common;
using System.Runtime.InteropServices;

//*******************************************************************************************************
//  TVA.Interop.IniFile.vb - Old style Windows INI file manipulation class
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//  01/05/2006 - J. Ritchie Carroll
//       2.0 version of source code migrated from 1.1 source (TVA.Interop.Windows.IniFile)
//  01/05/2007 - J. Ritchie Carroll
//       Breaking change: Renamed "IniFileName" property to "FileName"
//       Updated "SectionNames" to use List(Of String) instead of ArrayList
//
//*******************************************************************************************************


namespace TVA
{
	namespace Interop
	{
		
		/// <summary>Old style Windows INI file manipulation class</summary>
		public class IniFile
		{
			
			
			[DllImport("kernel32",EntryPoint="GetPrivateProfileStringA", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
			private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
			
			[DllImport("kernel32",EntryPoint="WritePrivateProfileStringA", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
			private static extern int WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);
			
			[DllImport("kernel32",EntryPoint="GetPrivateProfileSectionNamesA", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
			private static extern int GetPrivateProfileSectionNames(byte[] lpszReturnBuffer, int nSize, string lpFileName);
			
			private const int BufferSize = 4096;
			
			private string m_fileName;
			
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
					StringBuilder buffer = new StringBuilder(BufferSize);
					int commentIndex;
					string value;
					
					if (defaultValue == null)
					{
						defaultValue = "";
					}
					GetPrivateProfileString(section, entry, defaultValue, buffer, BufferSize, m_fileName);
					
					// Remove any trailing comments from key value
					value = buffer.ToString().Trim();
					commentIndex = value.IndexOf(';');
					if (commentIndex > - 1)
					{
						value = value.Substring(0, commentIndex).Trim();
					}
					
					return value;
				}
			}
			
			/// <summary>Sets the value of the specified key</summary>
			/// <param name="section">Section key exists in</param>
			/// <param name="entry">Name of key</param>
			/// <value>The new key value to store in the INI file</value>
			/// <remarks>This is the default member of this class</remarks>
			public string this[string section, string entry]
			{
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
					byte[] buffer = TVA.Common.CreateArray<byte>(BufferSize);
					int readLength;
					int nullIndex;
					int startIndex;
					
					readLength = GetPrivateProfileSectionNames(buffer, BufferSize, m_fileName);
					
					if (readLength > 0)
					{
						while (startIndex < readLength)
						{
							nullIndex = Array.IndexOf(buffer, Convert.ToByte(0), startIndex);
							
							if (nullIndex > - 1)
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
			
		}
		
	}
	
}
