//*******************************************************************************************************
//  IniSettingsBase.cs
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
//  04/01/2009 - James R Carroll
//       Generated original version of source code.
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
using PCS.Interop;

namespace PCS.Configuration
{
    /// <summary>
    /// Represents the base class for application settings that are synchronized to an INI file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In order to make custom types serializable for the INI file, implement a <see cref="TypeConverter"/> for the type.<br/>
    /// See <a href="http://msdn.microsoft.com/en-us/library/ayybcxe5.aspx">MSDN</a> for details.
    /// </para>
    /// <example>
    /// Here is an example class derived from <see cref="IniSettingsBase"/> that automatically
    /// serializes its fields and properties to the INI file.
    /// <code>
    ///    public enum MyEnum
    ///     {
    ///         One,
    ///         Two,
    ///         Three
    ///     }
    /// 
    ///     public class MySettings : IniSettingsBase
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
    ///         // Mark this field to not be serialized to INI file...
    ///         [SerializeSetting(false)]
    ///         public decimal DecimalVal;
    /// 
    ///         public MySettings()
    ///             : base(FilePath.GetAbsolutePath("MySettings.ini"), "GeneralSettings") {}
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
    public abstract class IniSettingsBase : SettingsBase
    {
        #region [ Members ]

        // Fields
        private IniFile m_iniFile;
        string m_categoryName;
        bool m_useCategoryAttributes;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="IniSettingsBase"/> class for the application's INI file.
        /// </summary>
        /// <param name="iniFileName">Name of INI file to use for accessing settings.</param>
        /// <param name="categoryName">Name of default category to use to get and set settings from INI file.</param>
        public IniSettingsBase(string iniFileName, string categoryName)
            : this(new IniFile(iniFileName), categoryName, true, false, true)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IniSettingsBase"/> class for the application's INI file.
        /// </summary>
        /// <param name="iniFileName">Name of INI file to use for accessing settings.</param>
        /// <param name="categoryName">Name of default category to use to get and set settings from INI file.</param>
        /// <param name="useCategoryAttributes">Determines if category attributes will be used for category names.</param>
        /// <param name="requireSerializeSettingAttribute">
        /// Assigns flag that determines if <see cref="SerializeSettingAttribute"/> is required
        /// to exist before a field or property is serialized to the INI file.
        /// </param>
        /// <remarks>
        /// If <paramref name="useCategoryAttributes"/> is false, all settings will be placed in section labeled by the
        /// <paramref name="categoryName"/> value; otherwise, if a <see cref="CategoryAttribute"/> exists on a field or
        /// property then the member value will serialized into the INI file in a section labeled the same
        /// as the <see cref="CategoryAttribute.Category"/> value and if the attribute doesn't exist the member value
        /// will serialized into the section labeled by the <paramref name="categoryName"/> value.
        /// </remarks>
        public IniSettingsBase(string iniFileName, string categoryName, bool useCategoryAttributes, bool requireSerializeSettingAttribute)
            : this(new IniFile(iniFileName), categoryName, useCategoryAttributes, requireSerializeSettingAttribute, true)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IniSettingsBase"/> class for the application's INI file.
        /// </summary>
        /// <param name="iniFile">INI file to use for accessing settings.</param>
        /// <param name="categoryName">Name of default category to use to get and set settings from INI file.</param>
        /// <param name="useCategoryAttributes">Determines if category attributes will be used for category names.</param>
        /// <param name="requireSerializeSettingAttribute">
        /// Assigns flag that determines if <see cref="SerializeSettingAttribute"/> is required
        /// to exist before a field or property is serialized to the INI file.
        /// </param>
        /// <param name="initialize">Determines if <see cref="SettingsBase.Initialize"/> method should be called from constructor.</param>
        /// <remarks>
        /// <para>
        /// If <paramref name="useCategoryAttributes"/> is false, all settings will be placed in section labeled by the
        /// <paramref name="categoryName"/> value; otherwise, if a <see cref="CategoryAttribute"/> exists on a field or
        /// property then the member value will serialized into the INI file in a section labeled the same
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
        public IniSettingsBase(IniFile iniFile, string categoryName, bool useCategoryAttributes, bool requireSerializeSettingAttribute, bool initialize)
            : base(requireSerializeSettingAttribute)
        {
            m_iniFile = iniFile;
            m_categoryName = categoryName;
            m_useCategoryAttributes = useCategoryAttributes;

            // Make sure settings exist and load current values
            if (initialize)
                Initialize();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets reference to working INI file.
        /// </summary>
        /// <exception cref="NullReferenceException">value cannot be null.</exception>
        protected IniFile IniFile
        {
            get
            {
                return m_iniFile;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value cannot be null");

                m_iniFile = value;
            }
        }

        /// <summary>
        /// Gets or sets default category name of section used to access settings in INI file.
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
        /// property then the member value will serialized into the INI file in a section labeled the same
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
        /// Create setting in INI file if it doesn't already exist.
        /// This method is for internal use.
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="value">Setting value.</param>
        internal override void CreateSetting(string name, string setting, string value)
        {
            if (string.IsNullOrEmpty(m_iniFile[GetCategoryName(name), setting]))
                m_iniFile[GetCategoryName(name), setting] = value;
        }

        /// <summary>
        /// Retrieves setting from INI file.
        /// This method is for internal use.
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <returns>Setting value.</returns>
        internal override string RetrieveSetting(string name, string setting)
        {
            return m_iniFile[GetCategoryName(name), setting];
        }

        /// <summary>
        /// Stores setting to INI file.
        /// This method is for internal use.
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="value">Setting value.</param>
        internal override void StoreSetting(string name, string setting, string value)
        {
            m_iniFile[GetCategoryName(name), setting] = value;
        }

        /// <summary>
        /// Persist any pending changes to INI file.
        /// This method is for internal use.
        /// </summary>
        internal override void PersistSettings()
        {
            // INI files flush at every update...
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

        ///// <summary>
        ///// Returns an enumerator that iterates through a collection of <see cref="String"/> objects.
        ///// </summary>
        ///// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        ///// <remarks>
        ///// This enumerator only enumerates settings from the default category.
        ///// </remarks>
        //public override IEnumerator GetEnumerator()
        //{
        //    return GetEnumerator(m_categoryName);
        //}

        ///// <summary>
        ///// Returns an enumerator that iterates through a collection of <see cref="String"/> objects.
        ///// </summary>
        ///// <param name="category">Category name to enumerate.</param>
        ///// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        //public IEnumerator GetEnumerator(string category)
        //{
        //    return ((IEnumerable)m_iniFile.GetSectionKeys(category)).GetEnumerator();
        //}

        #endregion
    }
}