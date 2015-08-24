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
//  03/31/2009 - James R Carroll
//       Made initialize during constructor optional for languages that do not initialize
//           member variables before call to constructor (e.g., Visual Basic.NET).
//       Updated class to pick up DesctiptionAttribute and apply value to settings.
//
//*******************************************************************************************************

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using PCS.Reflection;

namespace PCS.Configuration
{
    /// <summary>
    /// Represents the base class for application settings that are synchronized with a categorized section in a configuration file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In order to make custom types serializable for the configuration file, implement a <see cref="TypeConverter"/> for the type.<br/>
    /// See <a href="http://msdn.microsoft.com/en-us/library/ayybcxe5.aspx">MSDN</a> for details.
    /// </para>
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
    ///         private double m_doubleVal;
    /// 
    ///         // Public settings fields
    ///         public bool BoolVal = true;
    ///         public int IntVal = 1;
    ///         public float FloatVal = 3.14F;
    ///         public string StrVal = "This is a test...";
    ///         public MyEnum EnumVal = MyEnum.Three;
    ///         
    ///         [SettingName("UserOptions"), EncryptSetting()]
    ///         public string Password = "default";
    /// 
    ///         // Mark this field to not be serialized to configuration file...
    ///         [SerializeSetting(false)]
    ///         public decimal DecimalVal;
    /// 
    ///         public MySettings()
    ///             : base("GeneralSettings") {}
    /// 
    ///         [Category("OtherSettings"), Description("My double value setting description."), DefaultValue(1.159D)]
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
        private ConfigurationFile m_configFile;
        string m_categoryName;
        bool m_useCategoryAttributes;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CategorizedSettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="categoryName">Name of default category to use to get and set settings from configuration file.</param>
        public CategorizedSettingsBase(string categoryName)
            : this(ConfigurationFile.Current, categoryName, true, false, true)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CategorizedSettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="categoryName">Name of default category to use to get and set settings from configuration file.</param>
        /// <param name="useCategoryAttributes">Determines if category attributes will be used for category names.</param>
        /// <param name="requireSerializeSettingAttribute">
        /// Assigns flag that determines if <see cref="SerializeSettingAttribute"/> is required
        /// to exist before a field or property is serialized to the configuration file.
        /// </param>
        /// <remarks>
        /// If <paramref name="useCategoryAttributes"/> is false, all settings will be placed in section labeled by the
        /// <paramref name="categoryName"/> value; otherwise, if a <see cref="CategoryAttribute"/> exists on a field or
        /// property then the member value will serialized into the configuration file in a section labeled the same
        /// as the <see cref="CategoryAttribute.Category"/> value and if the attribute doesn't exist the member value
        /// will serialized into the section labeled by the <paramref name="categoryName"/> value.
        /// </remarks>
        public CategorizedSettingsBase(string categoryName, bool useCategoryAttributes, bool requireSerializeSettingAttribute)
            : this(ConfigurationFile.Current, categoryName, useCategoryAttributes, requireSerializeSettingAttribute, true)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CategorizedSettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="configFile">Configuration file used for accessing settings.</param>
        /// <param name="categoryName">Name of default category to use to get and set settings from configuration file.</param>
        /// <param name="useCategoryAttributes">Determines if category attributes will be used for category names.</param>
        /// <param name="requireSerializeSettingAttribute">
        /// Assigns flag that determines if <see cref="SerializeSettingAttribute"/> is required
        /// to exist before a field or property is serialized to the configuration file.
        /// </param>
        /// <param name="initialize">Determines if <see cref="SettingsBase.Initialize"/> method should be called from constructor.</param>
        /// <remarks>
        /// <para>
        /// If <paramref name="useCategoryAttributes"/> is false, all settings will be placed in section labeled by the
        /// <paramref name="categoryName"/> value; otherwise, if a <see cref="CategoryAttribute"/> exists on a field or
        /// property then the member value will serialized into the configuration file in a section labeled the same
        /// as the <see cref="CategoryAttribute.Category"/> value and if the attribute doesn't exist the member value
        /// will serialized into the section labeled by the <paramref name="categoryName"/> value.
        /// </para>
        /// <para>
        /// Note that some .NET languages (e.g., Visual Basic) will not initialize member elements before call to constuctor,
        /// in this case <paramref name="initialize"/> should be set to <c>false</c>, then the <see cref="SettingsBase.Initialize"/>
        /// method should be called manually after all properties have been initialized. Alternately, consider using the
        /// <see cref="DefaultValueAttribute"/> on the fields or properties and this will be used to initialize the values.
        /// </para>
        /// </remarks>
        public CategorizedSettingsBase(ConfigurationFile configFile, string categoryName, bool useCategoryAttributes, bool requireSerializeSettingAttribute, bool initialize)
            : base(requireSerializeSettingAttribute)
        {
            m_configFile = configFile;
            m_categoryName = categoryName;
            m_useCategoryAttributes = useCategoryAttributes;

            // Make sure settings exist and load current values
            if (initialize)
                Initialize();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets reference to working configuration file.
        /// </summary>
        /// <exception cref="NullReferenceException">value cannot be null.</exception>
        protected ConfigurationFile ConfigFile
        {
            get
            {
                return m_configFile;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value cannot be null");

                m_configFile = value;
            }
        }

        /// <summary>
        /// Gets or sets default category name of section used to access settings in configuration file.
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

        /// <summary>
        /// Gets or sets value that determines whether a <see cref="CategoryAttribute"/> applied to a field or property
        /// will be used for the category name.
        /// </summary>
        /// <remarks>
        /// If <see cref="UseCategoryAttributes"/> is false, all settings will be placed in section labeled by the
        /// <see cref="CategoryName"/> value; otherwise, if a <see cref="CategoryAttribute"/> exists on a field or
        /// property then the member value will serialized into the configuration file in a section labeled the same
        /// as the <see cref="CategoryAttribute.Category"/> value and if the attribute doesn't exist the member value
        /// will serialized into the section labeled by the <see cref="CategoryName"/> value.
        /// </remarks>
        public bool UseCategoryAttributes
        {
            get
            {
                return m_useCategoryAttributes;
            }
            set
            {
                m_useCategoryAttributes = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Create setting in configuration file if it doesn't already exist.
        /// This method is for internal use.
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="value">Setting value.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void CreateSetting(string name, string setting, string value)
        {
            m_configFile.Settings[GetCategoryName(name)].Add(setting, value, GetDescription(name), GetEncryptStatus(name));
        }

        /// <summary>
        /// Retrieves setting from configuration file.
        /// This method is for internal use.
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <returns>Setting value.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override string RetrieveSetting(string name, string setting)
        {
            return m_configFile.Settings[GetCategoryName(name)][setting].Value;
        }

        /// <summary>
        /// Stores setting to configuration file.
        /// This method is for internal use.
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="value">Setting value.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void StoreSetting(string name, string setting, string value)
        {
            m_configFile.Settings[GetCategoryName(name)][setting].Value = value;
        }

        /// <summary>
        /// Persist any pending changes to configuration file.
        /// This method is for internal use.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void PersistSettings()
        {
            m_configFile.Save();
        }

        /// <summary>
        /// Gets the category name to use for the specified field or property.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns><see cref="CategoryAttribute.Category"/> applied to specified field or property; or <see cref="CategoryName"/> if attribute does not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or empty.</exception>
        /// <remarks>
        /// <see cref="CategoryAttribute.Category"/> will only be returned if <see cref="UseCategoryAttributes"/> is <c>true</c>; otherwise
        /// <see cref="CategoryName"/> value will be returned.
        /// </remarks>
        public string GetCategoryName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty");

            // If user wants to respect category attributes, we attempt to use those as configuration section names
            if (m_useCategoryAttributes)
                return GetAttributeValue<CategoryAttribute, string>(name, m_categoryName, attribute => attribute.Category);

            // Otherwise return default category name
            return m_categoryName;
        }

        /// <summary>
        /// Gets the description specified by <see cref="DescriptionAttribute"/>, if any, applied to the specified field or property. 
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns>Description applied to specified field or property; or null if one does not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or empty.</exception>
        public string GetDescription(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty");

            return GetAttributeValue<DescriptionAttribute, string>(name, "", attribute => attribute.Description);
        }

        #endregion
    }
}