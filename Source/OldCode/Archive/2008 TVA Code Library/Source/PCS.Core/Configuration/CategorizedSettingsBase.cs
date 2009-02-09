//*******************************************************************************************************
//  CategorizedSettingsBase.cs
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
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCS.Configuration
{
    /// <summary>
    /// Represents the base class for application settings that are synchronized with a categorized section in a configuration file.
    /// </summary>
    /// <remarks>
    /// <example>
    /// Here is an example class derived from <see cref="CategorizedSettingsBase"/> that automatically
    /// serializes its fields and properties to the configuration file.
    /// <code>
    ///    public enum MyEnum
    ///     {
    ///         One,
    ///         Two,
    ///         Three
    ///     }
    /// 
    ///     public class MySettings : CategorizedSettingsBase
    ///     {
    ///         // Private property fields (private fields will not be serialized)
    ///         private double m_doubleVal = 1.159D;
    /// 
    ///         // Public settings fields
    ///         public bool BoolVal = true;
    ///         public int IntVal = 1;
    ///         public float FloatVal = 3.14F;
    ///         public string StrVal = "This is a test...";
    ///         public MyEnum EnumVal = MyEnum.Three;
    /// 
    ///         // Mark this field to not be serialized to configuration file...
    ///         [SerializeSetting(false)]
    ///         public decimal DecimalVal;
    /// 
    ///         public MySettings()
    ///             : base("GeneralSettings") {}
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
    public abstract class CategorizedSettingsBase : SettingsBase
    {
        #region [ Members ]

        // Fields
        string m_categoryName;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CategorizedSettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="categoryName">Name of category used to get and set settings from configuration file.</param>
        public CategorizedSettingsBase(string categoryName)
            : this(ConfigurationFile.Current, categoryName, false)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CategorizedSettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="categoryName">Name of category used to get and set settings from configuration file.</param>
        /// <param name="requireSerializeSettingAttribute">
        /// Assigns flag that determines if <see cref="SerializeSettingAttribute"/> is required
        /// to exist before a field or property is serialized to the configuration file.
        /// </param>
        public CategorizedSettingsBase(string categoryName, bool requireSerializeSettingAttribute)
            : this(ConfigurationFile.Current, categoryName, requireSerializeSettingAttribute)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CategorizedSettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="configFile">Configuration file used for accessing settings.</param>
        /// <param name="categoryName">Name of category used to get and set settings from configuration file.</param>
        /// <param name="requireSerializeSettingAttribute">
        /// Assigns flag that determines if <see cref="SerializeSettingAttribute"/> is required
        /// to exist before a field or property is serialized to the configuration file.
        /// </param>
        public CategorizedSettingsBase(ConfigurationFile configFile, string categoryName, bool requireSerializeSettingAttribute)
            : base(requireSerializeSettingAttribute)
        {
            m_categoryName = categoryName;
            ConfigFile = configFile;

            // Define delegates used to access and create settings in configuration file
            Getter = setting => ConfigFile.Settings[m_categoryName][setting].Value;
            Setter = (setting, value) => ConfigFile.Settings[m_categoryName][setting].Value = value;
            Creator = (setting, value) => ConfigFile.Settings[m_categoryName].Add(setting, value);

            // Make sure settings exist and load current values
            Initialize();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Category name of section used to access settings in configuration file.
        /// </summary>
        public string CategoryName
        {
            get
            {
                return m_categoryName;
            }
            set
            {
                m_categoryName = value;
            }
        }

        #endregion
    }
}
