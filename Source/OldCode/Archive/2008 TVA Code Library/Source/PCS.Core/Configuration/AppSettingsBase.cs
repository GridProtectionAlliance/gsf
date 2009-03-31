//*******************************************************************************************************
//  AppSettingsBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/30/2009 - James R Carroll
//       Generated original version of source code.
//  03/31/2009 - James R Carroll
//       Made initialize during constructor optional for languages that do not initialize
//       member variables before call to constructor (e.g., Visual Basic.NET).
//
//*******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PCS.Configuration
{
    /// <summary>
    /// Represents the base class for application settings that are synchronized with the "appSettings" section in a configuration file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In order to make custom types serializable for the configuration file, implement a <see cref="System.ComponentModel.TypeConverter"/> for the type.<br/>
    /// See <a href="http://msdn.microsoft.com/en-us/library/ayybcxe5.aspx">MSDN</a> for details.
    /// </para>
    /// <example>
    /// Here is an example class derived from <see cref="AppSettingsBase"/> that automatically
    /// serializes its fields and properties to the configuration file.
    /// <code>
    ///    public enum MyEnum
    ///     {
    ///         One,
    ///         Two,
    ///         Three
    ///     }
    /// 
    ///     public class MySettings : AppSettingsBase
    ///     {
    ///         // Private property fields (private fields will not be serialized)
    ///         private double m_doubleVal = 1.159D;
    /// 
    ///         // Public settings fields
    ///         public bool BoolVal = true;
    ///         public int IntVal = 1;
    ///         public float FloatVal = 3.14F;
    ///         
    ///         [SettingName("StringValue")]
    ///         public string StrVal = "This is a test...";
    ///         public MyEnum EnumVal = MyEnum.Three;
    /// 
    ///         // Mark this field to not be serialized to configuration file...
    ///         [SerializeSetting(false)]
    ///         public decimal DecimalVal;
    /// 
    ///         public double DoubleVal
    ///         {
    ///             get
    ///             {
    ///                 return m_doubleVal;
    ///             }
    ///             set
    ///             {
    ///                 m_doubleVal = value;
    ///             }
    ///         }
    /// 
    ///         [SerializeSetting(false)]
    ///         public bool DontSerializeMe { get; set; }
    ///     }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class AppSettingsBase : SettingsBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AppSettingsBase"/> class for the application's configuration file.
        /// </summary>
        public AppSettingsBase()
            : this(ConfigurationFile.Current, false, true)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AppSettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="configFile">Configuration file used for accessing settings.</param>
        /// <param name="requireSerializeSettingAttribute">
        /// Assigns flag that determines if <see cref="SerializeSettingAttribute"/> is required
        /// to exist before a field or property is serialized to the configuration file.
        /// </param>
        /// <param name="initialize">Determines if <see cref="SettingsBase.Initialize"/> method should be called from constructor.</param>
        /// <remarks>
        /// Note that some .NET languages (e.g., Visual Basic) will not initialize member elements before call to constuctor,
        /// in this case <paramref name="initialize"/> should be set to <c>false</c>, then the <see cref="SettingsBase.Initialize"/>
        /// method should be called manually after all properties have been initialized.
        /// </remarks>
        public AppSettingsBase(ConfigurationFile configFile, bool requireSerializeSettingAttribute, bool initialize)
            : base(requireSerializeSettingAttribute)
        {
            ConfigFile = configFile;

            // Define delegates used to access and create settings in configuration file
            Getter = (name, setting) => ConfigFile.AppSettings.Settings[setting].Value;
            Setter = (name, setting, value) => ConfigFile.AppSettings.Settings[setting].Value = value;
            Creator = (name, setting, value) => { if (Getter(name, setting) == null) Setter(name, setting, value); };

            // Make sure settings exist and load current values
            if (initialize)
                Initialize();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns an enumerator that iterates through a collection of <see cref="KeyValueConfigurationElement"/> objects.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        public override IEnumerator GetEnumerator()
        {
            return ((IEnumerable)ConfigFile.AppSettings.Settings).GetEnumerator();
        }

        #endregion
    }
}
