//*******************************************************************************************************
//  RegistrySettingsBase.cs
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
//  04/02/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using Microsoft.Win32;

namespace TVA.Configuration
{
    /// <summary>
    /// Represents the base class for application settings that are synchronized to the registry.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In order to make custom types serializable for the registry, implement a <see cref="TypeConverter"/> for the type.<br/>
    /// See <a href="http://msdn.microsoft.com/en-us/library/ayybcxe5.aspx">MSDN</a> for details.
    /// </para>
    /// <example>
    /// Here is an example class derived from <see cref="RegistrySettingsBase"/> that automatically
    /// serializes its fields and properties to the registry.
    /// <code>
    ///    public enum MyEnum
    ///     {
    ///         One,
    ///         Two,
    ///         Three
    ///     }
    /// 
    ///     public class MySettings : RegistrySettingsBase
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
    ///         // Mark this field to not be serialized to registry...
    ///         [SerializeSetting(false)]
    ///         public decimal DecimalVal;
    /// 
    ///         public MySettings()
    ///             : base("HKEY_CURRENT_USER\\Software\\My Company\\My Product\\", "General Settings") {}
    /// 
    ///         [Category("OtherSettings"), DefaultValue(1.159D)]
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
    public abstract class RegistrySettingsBase : SettingsBase
    {
        #region [ Members ]

        // Fields
        string m_rootPath;
        string m_keyName;
        bool m_useCategoryAttributes;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RegistrySettingsBase"/> class for the application's registry based settings.
        /// </summary>
        /// <param name="rootPath">Defines the root registry path used to access settings in the registry (e.g., "HKEY_CURRENT_USER\\Software\\My Company\\My Product\\").</param>
        /// <param name="keyName">Defines the name of default key used to access settings in the registry (e.g., "General Settings").</param>
        public RegistrySettingsBase(string rootPath, string keyName)
            : this(rootPath, keyName, true, false, true)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RegistrySettingsBase"/> class for the application's registry based settings.
        /// </summary>
        /// <param name="rootPath">Defines the root registry path used to access settings in the registry (e.g., "HKEY_CURRENT_USER\\Software\\My Company\\My Product\\").</param>
        /// <param name="keyName">Defines the name of default key used to access settings in the registry (e.g., "General Settings").</param>
        /// <param name="useCategoryAttributes">Determines if category attributes will be used for the registry key names.</param>
        /// <param name="requireSerializeSettingAttribute">
        /// Assigns flag that determines if <see cref="SerializeSettingAttribute"/> is required
        /// to exist before a field or property is serialized to the registry.
        /// </param>
        /// <param name="initialize">Determines if <see cref="SettingsBase.Initialize"/> method should be called from constructor.</param>
        /// <remarks>
        /// <para>
        /// If <paramref name="useCategoryAttributes"/> is false, all settings will be placed in section labeled by the
        /// <paramref name="keyName"/> value; otherwise, if a <see cref="CategoryAttribute"/> exists on a field or
        /// property then the member value will serialized into the registry in a section labeled the same
        /// as the <see cref="CategoryAttribute.Category"/> value and if the attribute doesn't exist the member value
        /// will serialized into the section labeled by the <paramref name="keyName"/> value.
        /// </para>
        /// <para>
        /// Note that some .NET languages (e.g., Visual Basic) will not initialize member elements before call to constuctor,
        /// in this case <paramref name="initialize"/> should be set to <c>false</c>, then the <see cref="SettingsBase.Initialize"/>
        /// method should be called manually after all properties have been initialized. Alternately, consider using the
        /// <see cref="DefaultValueAttribute"/> on the fields or properties and this will be used to initialize the values.
        /// </para>
        /// </remarks>
        public RegistrySettingsBase(string rootPath, string keyName, bool useCategoryAttributes, bool requireSerializeSettingAttribute, bool initialize)
            : base(requireSerializeSettingAttribute)
        {
            m_rootPath = rootPath;
            m_keyName = keyName;
            m_useCategoryAttributes = useCategoryAttributes;

            // Make sure settings exist and load current values
            if (initialize)
                Initialize();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets root registry path used to access settings in the registry (e.g., "HKEY_CURRENT_USER\\Software\\My Company\\My Product\\").
        /// </summary>
        public string RootPath
        {
            get
            {
                return m_rootPath;
            }
            set
            {
                m_rootPath = value;
            }
        }

        /// <summary>
        /// Gets or sets name of default key used to access settings in the registry (e.g., "General Settings").
        /// </summary>
        public string KeyName
        {
            get
            {
                return m_keyName;
            }
            set
            {
                m_keyName = value;
            }
        }

        /// <summary>
        /// Gets or sets value that determines whether a <see cref="CategoryAttribute"/> applied to a field or property
        /// will be used for the registry key names.
        /// </summary>
        /// <remarks>
        /// If <see cref="UseCategoryAttributes"/> is false, all settings will be placed in section labeled by the
        /// <see cref="KeyName"/> value; otherwise, if a <see cref="CategoryAttribute"/> exists on a field or
        /// property then the member value will serialized into the registry in a section labeled the same
        /// as the <see cref="CategoryAttribute.Category"/> value and if the attribute doesn't exist the member value
        /// will stored in the registry key identified by the <see cref="KeyName"/> value.
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
        /// Create setting in registry if it doesn't already exist.
        /// This method is for internal use.
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="value">Setting value.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void CreateSetting(string name, string setting, string value)
        {
            string keyName = GetKeyName(name);

            if (Registry.GetValue(keyName, setting, null) == null)
                Registry.SetValue(keyName, setting, value, RegistryValueKind.String);
        }

        /// <summary>
        /// Retrieves setting from registry.
        /// This method is for internal use.
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <returns>Setting value.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override string RetrieveSetting(string name, string setting)
        {
            return (string)Registry.GetValue(GetKeyName(name), setting, "");
        }

        /// <summary>
        /// Stores setting to registry.
        /// This method is for internal use.
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="value">Setting value.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void StoreSetting(string name, string setting, string value)
        {
            Registry.SetValue(GetKeyName(name), setting, value, RegistryValueKind.String);
        }

        /// <summary>
        /// Persist any pending changes to registry.
        /// This method is for internal use.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void PersistSettings()
        {
            // Registry API's flush at every update...
        }

        /// <summary>
        /// Gets the key name to use for storing the specified field or property in the registry.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns><see cref="CategoryAttribute.Category"/> applied to specified field or property; or <see cref="KeyName"/> if attribute does not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or empty.</exception>
        /// <remarks>
        /// <see cref="CategoryAttribute.Category"/> will only be returned if <see cref="UseCategoryAttributes"/> is <c>true</c>; otherwise
        /// <see cref="KeyName"/> value will be returned.
        /// </remarks>
        public string GetKeyName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty");

            // If user wants to respect category attributes, we attempt to use those as configuration section names
            if (m_useCategoryAttributes)
                return m_rootPath + GetAttributeValue<CategoryAttribute, string>(name, m_keyName, attribute => attribute.Category);

            // Otherwise return default category name
            return m_rootPath + m_keyName;
        }

        #endregion
    }
}