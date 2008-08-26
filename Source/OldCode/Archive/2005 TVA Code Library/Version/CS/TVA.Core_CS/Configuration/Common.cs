using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Configuration.Common.vb - Common Configuration Functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/13/2006 - Pinal C. Patel
//       Generated original version of source code.
//  05/19/2006 - J. Ritchie Carroll
//       Added type-safe "native type" settings shortcut functions.
//  09/01/2006 - J. Ritchie Carroll
//       Added set property to shortcut functions that didn't take default value parameter.
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************

namespace TVA
{
	namespace Configuration
	{
		
		/// <summary>
		/// Defines common shared configuration file related functions.
		/// </summary>
		public sealed class Common
		{
			
			
			private Common()
			{
				
				// This class contains only global functions and is not meant to be instantiated.
				
			}
			
			private static ConfigurationFile m_defaultConfigFile;
			
			/// <summary>
			/// Gets or sets the TVA.Configuration.ConfigurationFile object that represents the configuration
			/// file of the currently executing Windows or Web application.
			/// </summary>
			/// <returns>
			/// The TVA.Configuration.ConfigurationFile object that represents the configuration file of the
			/// currently executing Windows or Web application.
			/// </returns>
			/// <remarks>
			/// <para>
			/// Use this property to access the settings saved under the "appSettings" and "connectionStrings"
			/// sections provided by the .Net framework in addition to the "categorizedSettings" sections.
			/// </para>
			/// <para>
			/// Example:
			/// <code>
			/// With DefaultConfigFile()
			///     ' Adds setting to the "appSettings" section of the config file.
			///     .AppSettings.Settings.Add("SaveSettingsOnExit", "1")
			///
			///     ' Adds setting to the "connectionStrings" section of the config file.
			///     .ConnectionStrings.ConnectionStrings.Add(New ConnectionStringSettings("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True"))
			///
			///     ' Adds settings to the "categorizedSettings" section of the config file.
			///     .CategorizedSettings.General.Add("SaveSettingsOnExit", "1")
			///     .CategorizedSettings("Development").Add("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True", True)
			///     .CategorizedSettings("Production").Add("DbConnectString", "Server=OPDATSQL;Database=DB;Trusted_Connection=True", True)
			///
			///     ' Saves modified settings to the config file.
			///     .Save()
			/// End With
			/// </code>
			/// </para>
			/// </remarks>
			public static ConfigurationFile DefaultConfigFile
			{
				get
				{
					if (m_defaultConfigFile == null)
					{
						m_defaultConfigFile = new ConfigurationFile();
					}
					return m_defaultConfigFile;
				}
				set
				{
					m_defaultConfigFile = value;
				}
			}
			
			/// <summary>
			/// Gets the TVA.Configuration.ConfigurationFile object that represent the specified configuration
			/// file that belongs a Windows or Web application.
			/// </summary>
			/// <param name="filePath">Path of the configuration file that belongs to a Windows or Web application.</param>
			/// <remarks>
			/// <para>
			/// Use this property for accessing the configuration files of other Windows or Web applications.
			/// This will also give access to settings saved under the "appSettings" and "connectionStrings"
			/// sections provided by the .Net framework in addition to the "categorizedSettings" sections.
			/// </para>
			/// <para>
			/// Example:
			/// <code>
			/// ' Manupulating the configuration file of another Windows application.
			/// With CustomConfigFile("C:\Projects\WindowsApplication1\bin\Debug\WindowsApplication1.exe.config")
			///     ' Adds setting to the "appSettings" section of the config file.
			///     .AppSettings.Settings.Add("SaveSettingsOnExit", "1")
			///
			///     ' Adds setting to the "connectionStrings" section of the config file.
			///     .ConnectionStrings.ConnectionStrings.Add(New ConnectionStringSettings("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True"))
			///
			///     ' Adds settings to the "categorizedSettings" section of the config file.
			///     .CategorizedSettings.General.Add("SaveSettingsOnExit", "1")
			///     .CategorizedSettings("Development").Add("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True", True)
			///     .CategorizedSettings("Production").Add("DbConnectString", "Server=OPDATSQL;Database=DB;Trusted_Connection=True", True)
			///
			///     ' Saves modified settings to the config file.
			///     .Save()
			/// End With
			///
			/// ' Manupulating the configuration file of another Web application.
			/// With CustomConfigFile("/WebApplication1/web.config")
			///     ' Adds setting to the "appSettings" section of the config file.
			///     .AppSettings.Settings.Add("SaveSettingsOnExit", "1")
			///
			///     ' Adds setting to the "connectionStrings" section of the config file.
			///     .ConnectionStrings.ConnectionStrings.Add(New ConnectionStringSettings("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True"))
			///
			///     ' Adds settings to the "categorizedSettings" section of the config file.
			///     .CategorizedSettings.General.Add("SaveSettingsOnExit", "1")
			///     .CategorizedSettings("Development").Add("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True", True)
			///     .CategorizedSettings("Production").Add("DbConnectString", "Server=OPDATSQL;Database=DB;Trusted_Connection=True", True)
			///
			///     ' Saves modified settings to the config file.
			///     .Save()
			/// End With
			/// </code>
			/// </para>
			/// </remarks>
			public static ConfigurationFile CustomConfigFile(string filePath)
			{
				return new ConfigurationFile(filePath);
			}
			
			#region " General Settings Shortcuts "
			
			/// <summary>
			/// Gets the TVA.Configuration.CategorizedSettingsCollection representing the settings under "general"
			/// category of the "categorizedSettings" section within the default configuration file.
			/// </summary>
			/// <returns>
			/// The TVA.Configuration.CategorizedSettingsCollection representing the settings under "general"
			/// category of the "categorizedSettings" section.
			/// </returns>
			/// <remarks>
			/// <para>
			/// This property is meant to be a shortcut for accessing the settings under the "general" category of
			/// "categorizedSettings" section within the configuration file. In order to access the settings under
			/// "appSettings" and "connectionStrings" sections, use either DefaultConfigFile() or CustomConfigFile()
			/// property.
			/// </para>
			/// <para>
			/// Example:
			/// <code>
			/// Settings.Add("SaveSettingsOnExit", "1") ' Add a new setting to the "general" category.
			/// SaveSettings() ' Propogate the changes to the config file.
			///
			/// Settings("SaveSettingsOnExit").Value() ' Read an existing setting from the "general" category.
			/// </code>
			/// </para>
			/// </remarks>
			public static CategorizedSettingsElementCollection Settings
			{
				get
				{
					return DefaultConfigFile.CategorizedSettings.General;
				}
			}
			
			/// <summary>
			/// Gets the TVA.Configuration.CategorizedSettingsCollection representing the settings under the specified
			/// category of the "categorizedSettings" section within the default configuration file.
			/// </summary>
			/// <param name="category">The name of the category whose settings are to be retreived.</param>
			/// <returns>
			/// The TVA.Configuration.CategorizedSettingsCollection representing the settings under the specified
			/// category of the "categorizedSettings" section.
			/// </returns>
			/// <remarks>
			/// <para>
			/// This property is meant to be a shortcut for accessing the settings under the various categories,
			/// including "general", of "categorizedSettings" section within the configuration file. In order to access
			/// the settings under "appSettings" and "connectionStrings" sections, use either DefaultConfigFile() or
			/// CustomConfigFile() property.
			/// </para>
			/// <para>
			/// Example:
			/// <code>
			/// CategorizedSettings("Development").Add("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True", True) ' Add a new setting to the "development" category.
			/// SaveSettings() ' Propogate the changes to the config file.
			///
			/// CategorizedSettings("Development")("DbConnectString").Value() ' Read an existing setting from the "development" category.
			/// </code>
			/// </para>
			/// </remarks>
			public static CategorizedSettingsElementCollection CategorizedSettings(string category)
			{
				return DefaultConfigFile.CategorizedSettings[category];
			}
			
			/// <summary>
			/// Writes the modified configuration settings to the default configuration file.
			/// </summary>
			public static void SaveSettings()
			{
				DefaultConfigFile.Save();
			}
			
			#endregion
			
			#region " Coerced Native Type Setting Access Shortcuts "
			
			public static bool BooleanSetting(string name)
			{
				return BooleanSetting(name, false);
			}
			public static void SetBooleanSetting(string name, bool value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static bool BooleanSetting(string name, bool defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static byte ByteSetting(string name)
			{
				return ByteSetting(name, byte.MinValue);
			}
			public static void SetByteSetting(string name, byte value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static byte ByteSetting(string name, byte defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static SByte SByteSetting(string name)
			{
				return SByteSetting(name, SByte.MinValue);
			}
			public static void SetSByteSetting(string name, SByte value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static SByte SByteSetting(string name, SByte defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static char CharSetting(string name)
			{
				return CharSetting(name, char.MinValue);
			}
			public static void SetCharSetting(string name, char value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static char CharSetting(string name, char defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static int IntegerSetting(string name)
			{
				return IntegerSetting(name, int.MinValue);
			}
			public static void SetIntegerSetting(string name, int value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static int IntegerSetting(string name, int defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static UInt32 UIntegerSetting(string name)
			{
				return UIntegerSetting(name, UInt32.MinValue);
			}
			public static void SetUIntegerSetting(string name, UInt32 value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static UInt32 UIntegerSetting(string name, UInt32 defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static long LongSetting(string name)
			{
				return LongSetting(name, long.MinValue);
			}
			public static void SetLongSetting(string name, long value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static long LongSetting(string name, long defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static UInt64 ULongSetting(string name)
			{
				return ULongSetting(name, UInt64.MinValue);
			}
			public static void SetULongSetting(string name, UInt64 value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static UInt64 ULongSetting(string name, UInt64 defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static short Int16Setting(string name)
			{
				return Int16Setting(name, short.MinValue);
			}
			public static void SetInt16Setting(string name, short value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static short Int16Setting(string name, short defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static int Int32Setting(string name)
			{
				return Int32Setting(name, int.MinValue);
			}
			public static void SetInt32Setting(string name, int value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static int Int32Setting(string name, int defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static long Int64Setting(string name)
			{
				return Int64Setting(name, long.MinValue);
			}
			public static void SetInt64Setting(string name, long value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static long Int64Setting(string name, long defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static UInt16 UInt16Setting(string name)
			{
				return UInt16Setting(name, UInt16.MinValue);
			}
			public static void SetUInt16Setting(string name, UInt16 value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static UInt16 UInt16Setting(string name, UInt16 defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static UInt32 UInt32Setting(string name)
			{
				return UInt32Setting(name, UInt32.MinValue);
			}
			public static void SetUInt32Setting(string name, UInt32 value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static UInt32 UInt32Setting(string name, UInt32 defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static UInt64 UInt64Setting(string name)
			{
				return UInt64Setting(name, UInt64.MinValue);
			}
			public static void SetUInt64Setting(string name, UInt64 value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static UInt64 UInt64Setting(string name, UInt64 defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static float SingleSetting(string name)
			{
				return SingleSetting(name, float.MinValue);
			}
			public static void SetSingleSetting(string name, float value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static float SingleSetting(string name, float defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static double DoubleSetting(string name)
			{
				return DoubleSetting(name, double.MinValue);
			}
			public static void SetDoubleSetting(string name, double value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static double DoubleSetting(string name, double defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static decimal DecimalSetting(string name)
			{
				return DecimalSetting(name, decimal.MinValue);
			}
			public static void SetDecimalSetting(string name, decimal value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static decimal DecimalSetting(string name, decimal defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static DateTime DateTimeSetting(string name)
			{
				return DateTimeSetting(name, DateTime.MinValue);
			}
			public static void SetDateTimeSetting(string name, DateTime value)
			{
				Settings[name].Value = value.ToString();
			}
			
			public static DateTime DateTimeSetting(string name, DateTime defaultValue)
			{
				return Settings[name].GetTypedValue(defaultValue);
			}
			
			public static string StringSetting(string name)
			{
				return StringSetting(name, "");
			}
			public static void SetStringSetting(string name, string value)
			{
				Settings[name].Value = value;
			}
			
			public static string StringSetting(string name, string defaultValue)
			{
				string setting = Settings[name].Value;
				
				if (string.IsNullOrEmpty(setting))
				{
					return defaultValue;
				}
				else
				{
					return setting;
				}
			}
			
			public static bool CategorizedBooleanSetting(string category, string name)
			{
				return CategorizedBooleanSetting(category, name, false);
			}
			public static void SetCategorizedBooleanSetting(string category, string name, bool value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static bool CategorizedBooleanSetting(string category, string name, bool defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static byte CategorizedByteSetting(string category, string name)
			{
				return CategorizedByteSetting(category, name, byte.MinValue);
			}
			public static void SetCategorizedByteSetting(string category, string name, byte value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static byte CategorizedByteSetting(string category, string name, byte defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static SByte CategorizedSByteSetting(string category, string name)
			{
				return CategorizedSByteSetting(category, name, SByte.MinValue);
			}
			public static void SetCategorizedSByteSetting(string category, string name, SByte value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static SByte CategorizedSByteSetting(string category, string name, SByte defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static char CategorizedCharSetting(string category, string name)
			{
				return CategorizedCharSetting(category, name, char.MinValue);
			}
			public static void SetCategorizedCharSetting(string category, string name, char value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static char CategorizedCharSetting(string category, string name, char defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static int CategorizedIntegerSetting(string category, string name)
			{
				return CategorizedIntegerSetting(category, name, int.MinValue);
			}
			public static void SetCategorizedIntegerSetting(string category, string name, int value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static int CategorizedIntegerSetting(string category, string name, int defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static UInt32 CategorizedUIntegerSetting(string category, string name)
			{
				return CategorizedUIntegerSetting(category, name, UInt32.MinValue);
			}
			public static void SetCategorizedUIntegerSetting(string category, string name, UInt32 value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static UInt32 CategorizedUIntegerSetting(string category, string name, UInt32 defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static long CategorizedLongSetting(string category, string name)
			{
				return CategorizedLongSetting(category, name, long.MinValue);
			}
			public static void SetCategorizedLongSetting(string category, string name, long value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static long CategorizedLongSetting(string category, string name, long defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static UInt64 CategorizedULongSetting(string category, string name)
			{
				return CategorizedULongSetting(category, name, UInt64.MinValue);
			}
			public static void SetCategorizedULongSetting(string category, string name, UInt64 value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static UInt64 CategorizedULongSetting(string category, string name, UInt64 defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static short CategorizedInt16Setting(string category, string name)
			{
				return CategorizedInt16Setting(category, name, short.MinValue);
			}
			public static void SetCategorizedInt16Setting(string category, string name, short value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static short CategorizedInt16Setting(string category, string name, short defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static int CategorizedInt32Setting(string category, string name)
			{
				return CategorizedInt32Setting(category, name, int.MinValue);
			}
			public static void SetCategorizedInt32Setting(string category, string name, int value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static int CategorizedInt32Setting(string category, string name, int defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static long CategorizedInt64Setting(string category, string name)
			{
				return CategorizedInt64Setting(category, name, long.MinValue);
			}
			public static void SetCategorizedInt64Setting(string category, string name, long value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static long CategorizedInt64Setting(string category, string name, long defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static UInt16 CategorizedUInt16Setting(string category, string name)
			{
				return CategorizedUInt16Setting(category, name, UInt16.MinValue);
			}
			public static void SetCategorizedUInt16Setting(string category, string name, UInt16 value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static UInt16 CategorizedUInt16Setting(string category, string name, UInt16 defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static UInt32 CategorizedUInt32Setting(string category, string name)
			{
				return CategorizedUInt32Setting(category, name, UInt32.MinValue);
			}
			public static void SetCategorizedUInt32Setting(string category, string name, UInt32 value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static UInt32 CategorizedUInt32Setting(string category, string name, UInt32 defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static UInt64 CategorizedUInt64Setting(string category, string name)
			{
				return CategorizedUInt64Setting(category, name, UInt64.MinValue);
			}
			public static void SetCategorizedUInt64Setting(string category, string name, UInt64 value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static UInt64 CategorizedUInt64Setting(string category, string name, UInt64 defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static float CategorizedSingleSetting(string category, string name)
			{
				return CategorizedSingleSetting(category, name, float.MinValue);
			}
			public static void SetCategorizedSingleSetting(string category, string name, float value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static float CategorizedSingleSetting(string category, string name, float defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static double CategorizedDoubleSetting(string category, string name)
			{
				return CategorizedDoubleSetting(category, name, double.MinValue);
			}
			public static void SetCategorizedDoubleSetting(string category, string name, double value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static double CategorizedDoubleSetting(string category, string name, double defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static decimal CategorizedDecimalSetting(string category, string name)
			{
				return CategorizedDecimalSetting(category, name, decimal.MinValue);
			}
			public static void SetCategorizedDecimalSetting(string category, string name, decimal value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static decimal CategorizedDecimalSetting(string category, string name, decimal defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static DateTime CategorizedDateTimeSetting(string category, string name)
			{
				return CategorizedDateTimeSetting(category, name, DateTime.MinValue);
			}
			public static void SetCategorizedDateTimeSetting(string category, string name, DateTime value)
			{
				CategorizedSettings(category)[name].Value = value.ToString();
			}
			
			public static DateTime CategorizedDateTimeSetting(string category, string name, DateTime defaultValue)
			{
				return CategorizedSettings(category)[name].GetTypedValue(defaultValue);
			}
			
			public static string CategorizedStringSetting(string category, string name)
			{
				return CategorizedStringSetting(category, name, "");
			}
			public static void SetCategorizedStringSetting(string category, string name, string value)
			{
				CategorizedSettings(category)[name].Value = value;
			}
			
			public static string CategorizedStringSetting(string category, string name, string defaultValue)
			{
				string setting = CategorizedSettings(category)[name].Value;
				
				if (string.IsNullOrEmpty(setting))
				{
					return defaultValue;
				}
				else
				{
					return setting;
				}
			}
			
			#endregion
			
		}
		
	}
	
}
