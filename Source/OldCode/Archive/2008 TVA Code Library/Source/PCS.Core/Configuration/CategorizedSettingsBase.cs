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
using System.Linq;
using System.Text;
using System.Reflection;
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
    /// See http://msdn.microsoft.com/en-us/library/ayybcxe5.aspx for details.
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
    ///             : base("GeneralSettings", true) {}
    /// 
    ///         [Category("OtherSettings")]
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
        bool m_useCategoryAttributes;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CategorizedSettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="categoryName">Name of default category to use to get and set settings from configuration file.</param>
        /// <param name="useCategoryAttributes">Determines if category attributes will be used for categroy names.</param>
        /// <remarks>
        /// If <paramref name="useCategoryAttributes"/> is false, all settings will be placed in section labeled by the
        /// <paramref name="categoryName"/> value; otherwise, if a <see cref="CategoryAttribute"/> exists on a field or
        /// property then the member value will serialized into the configuration file in a section labeled the same
        /// as the <see cref="CategoryAttribute.Category"/> value and if the attribute doesn't exist the member value
        /// will serialized into the section labeled by the <paramref name="categoryName"/> value.
        /// </remarks>
        public CategorizedSettingsBase(string categoryName, bool useCategoryAttributes)
            : this(ConfigurationFile.Current, categoryName, useCategoryAttributes, false)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CategorizedSettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="categoryName">Name of default category to use to get and set settings from configuration file.</param>
        /// <param name="useCategoryAttributes">Determines if category attributes will be used for categroy names.</param>
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
            : this(ConfigurationFile.Current, categoryName, useCategoryAttributes, requireSerializeSettingAttribute)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CategorizedSettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="configFile">Configuration file used for accessing settings.</param>
        /// <param name="categoryName">Name of default category to use to get and set settings from configuration file.</param>
        /// <param name="useCategoryAttributes">Determines if category attributes will be used for categroy names.</param>
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
        public CategorizedSettingsBase(ConfigurationFile configFile, string categoryName, bool useCategoryAttributes, bool requireSerializeSettingAttribute)
            : base(requireSerializeSettingAttribute)
        {
            m_categoryName = categoryName;
            m_useCategoryAttributes = useCategoryAttributes;
            ConfigFile = configFile;

            // Define delegates used to access and create settings in configuration file
            Getter = setting => ConfigFile.Settings[GetCategoryName(setting)][setting].Value;
            Setter = (setting, value) => ConfigFile.Settings[GetCategoryName(setting)][setting].Value = value;
            Creator = (setting, value) => ConfigFile.Settings[GetCategoryName(setting)].Add(setting, value);

            // Make sure settings exist and load current values
            Initialize();
        }

        #endregion

        #region [ Properties ]

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
        /// will be used for the configurtion section 
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

        // Lookup category name for field or property
        private string GetCategoryName(string name)
        {
            // If user wants to respect category attributes, we attempt to use those as configuration section names
            if (m_useCategoryAttributes)
            {
                CategoryAttribute attribute;

                // See if field exists with specified name
                FieldInfo field = this.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                if (field != null)
                {
                    // See if category attribute exists on field
                    if (field.TryGetAttribute(out attribute))
                    {
                        // Return category name as specified in attribute (base class will make string XML compliant)
                        return attribute.Category;
                    }

                    // Category attribute wasn't found, just use default category name
                    return m_categoryName;
                }

                // See if property exists with specified name
                PropertyInfo property = this.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                if (property != null)
                {
                    // See if category attribute exists on property
                    if (property.TryGetAttribute(out attribute))
                    {
                        // Return category name as specified in attribute (base class will make string XML compliant)
                        return attribute.Category;
                    }

                    // Category attribute wasn't found, just use default category name
                    return m_categoryName;
                }
            }

            // Return default category name
            return m_categoryName;
        }

        #endregion
    }
}
