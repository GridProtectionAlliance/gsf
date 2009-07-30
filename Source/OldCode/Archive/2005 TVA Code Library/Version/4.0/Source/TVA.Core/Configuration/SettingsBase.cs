//*******************************************************************************************************
//  SettingsBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/30/2009 - James R. Carroll
//       Generated original version of source code.
//  03/31/2009 - James R. Carroll
//       Added code to allow override of name used to serialize field or property to configuration file
//          by applying a SettingNameAttribute to the member.
//  04/01/2009 - James R. Carroll
//       Added code to optionally encrypt settings based on EncryptSettingAttribute and to pickup
//          DefaultValueAttribute value if provided and current value was uninitialized.
//
//*******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using TVA.Reflection;
using TVA.Security.Cryptography;

namespace TVA.Configuration
{
    /// <summary>
    /// Represents the base class for application settings that are synchronized with its configuration file.
    /// </summary>
    /// <remarks>
    /// In order to make custom types serializable for the configuration file, implement a <see cref="TypeConverter"/> for the type.<br/>
    /// See <a href="http://msdn.microsoft.com/en-us/library/ayybcxe5.aspx">MSDN</a> for details.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class SettingsBase : IDisposable, IEnumerable<string>, IEnumerable
    {
        #region [ Members ]

        // Constants

        // IMPORTANT! Never change the following constant or you will break backwards compatibility
        private const string InternalKey = "§g¥J2&5:0xDr;£l8fL?C¡dF4?6c«u%ª±n¤9µ%î]a4@³,ÜaD*{©[1P.¢7_~`4rËd®!5:1j=)Al6¦)a#2¦Ï3E?¤(^\\dz$|¶TÁ+";

        // Fields
        private BindingFlags m_memberAccessBindingFlags;
        private bool m_requireSerializeSettingAttribute;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="requireSerializeSettingAttribute">
        /// Assigns flag that determines if <see cref="SerializeSettingAttribute"/> is required
        /// to exist before a field or property is serialized to the configuration file.
        /// </param>
        protected SettingsBase(bool requireSerializeSettingAttribute)
        {
            m_requireSerializeSettingAttribute = requireSerializeSettingAttribute;
            m_memberAccessBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="CategorizedSettingsBase"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~SettingsBase()
        {
            // If user failed to dispose class, we make sure settings get saved...
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="BindingFlags"/> used to access fields and properties of dervied class.
        /// </summary>
        /// <remarks>
        /// Value defaults to <c><see cref="BindingFlags.Public"/> | <see cref="BindingFlags.Instance"/> | <see cref="BindingFlags.DeclaredOnly"/></c>.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual BindingFlags MemberAccessBindingFlags
        {
            get
            {
                return m_memberAccessBindingFlags;
            }
            set
            {
                m_memberAccessBindingFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="SerializeSettingAttribute"/> is
        /// required to exist before a field or property is serialized to the configuration
        /// file; defaults to False.
        /// </summary>
        public bool RequireSerializeSettingAttribute
        {
            get
            {
                return m_requireSerializeSettingAttribute;
            }
            set
            {
                m_requireSerializeSettingAttribute = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the specified field or property.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns>Value of setting.</returns>
        /// <remarks>This is the default member of this class.</remarks>
        public string this[string name]
        {
            get
            {
                return GetValue<string>(name);
            }
            set
            {
                SetValue(name, value);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="CategorizedSettingsBase"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="CategorizedSettingsBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // We'll make sure settings are saved when class is properly disposed...
                    if (disposing)
                        Save();
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Implementor should create setting in configuration file (or other location).
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="value">Setting value.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected abstract void CreateSetting(string name, string setting, string value);

        /// <summary>
        /// Implementor should retrieve setting from configuration file (or other location).
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <returns>Setting value.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected abstract string RetrieveSetting(string name, string setting);

        /// <summary>
        /// Implementor should store setting to configuration file (or other location).
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="value">Setting value.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected abstract void StoreSetting(string name, string setting, string value);

        /// <summary>
        /// Implementor should persist any pending changes to configuration file (or other location).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected abstract void PersistSettings();

        /// <summary>
        /// Gets setting name to use for specified field or property. 
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns><see cref="SettingNameAttribute.Name"/> applied to specified field or property; or <paramref name="name"/> if attribute does not exist.</returns>
        /// <remarks>
        /// Field or property name will be used for setting name unless user applied a <see cref="SettingNameAttribute"/>
        /// on the field or property to override name used to serialize value in configuration file.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or empty.</exception>
        public string GetSettingName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty");

            return GetAttributeValue<SettingNameAttribute, string>(name, name, attribute => attribute.Name).NotEmpty(name);
        }

        /// <summary>
        /// Gets the default value specified by <see cref="DefaultValueAttribute"/>, if any, applied to the specified field or property. 
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns>Default value applied to specified field or property; or null if one does not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or empty.</exception>
        public object GetDefaultValue(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty");

            return GetAttributeValue<DefaultValueAttribute, object>(name, null, attribute => attribute.Value);
        }

        /// <summary>
        /// Gets the encryption status specified by <see cref="EncryptSettingAttribute"/>, if any, applied to the specified field or property. 
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns>Encryption status applied to specified field or property; or <c>false</c> if one does not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or empty.</exception>
        public bool GetEncryptStatus(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty");

            return GetAttributeValue<EncryptSettingAttribute, bool>(name, false, attribute => attribute.Encrypt);
        }

        /// <summary>
        /// Gets the optional private encryption key specified by <see cref="EncryptSettingAttribute"/>, if any, applied to the specified field or property. 
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns>Encryption private key applied to specified field or property; or <c>null</c> if one does not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or empty.</exception>
        public string GetEncryptKey(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty");

            return GetAttributeValue<EncryptSettingAttribute, string>(name, null, attribute => attribute.PrivateKey);
        }

        /// <summary>
        /// Adds a setting to the application's configuration file, if it doesn't already exist.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <param name="value">Setting value.</param>
        /// <remarks>
        /// Use this function to ensure a setting exists, it will not override an existing value.
        /// </remarks>
        public void CreateValue(string name, object value)
        {
            string setting = GetSettingName(name);

            if (value == null)
                CreateSetting(name, setting, EncryptValue(name, setting, ""));
            else
                CreateSetting(name, setting, EncryptValue(name, setting, Common.TypeConvertToString(value)));
        }

        /// <summary>
        /// Copies the given value into the specified application setting.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <param name="value">Setting value.</param>
        public void SetValue(string name, object value)
        {
            string setting = GetSettingName(name);

            if (value == null)
                StoreSetting(name, setting, EncryptValue(name, setting, ""));
            else
                StoreSetting(name, setting, EncryptValue(name, setting, Common.TypeConvertToString(value)));
        }

        /// <summary>
        /// Gets the application's configuration file setting converted to the given type.
        /// </summary>
        /// <typeparam name="T">Type to use for setting conversion.</typeparam>
        /// <param name="name">Field or property name.</param>
        /// <returns>Value of specified configuration file setting converted to the given type.</returns>
        public T GetValue<T>(string name)
        {
            string setting = GetSettingName(name);

            return DecryptValue(name, setting, RetrieveSetting(name, setting)).ConvertToType<T>();
        }

        /// <summary>
        /// Gets the application's configuration file setting converted to the given type.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <param name="type">Setting type.</param>
        /// <returns>Value of specified configuration file setting converted to the given type.</returns>
        public object GetValue(string name, Type type)
        {
            string setting = GetSettingName(name);

            return DecryptValue(name, setting, RetrieveSetting(name, setting)).ConvertToType(type);
        }

        /// <summary>
        /// Copies the specified application setting into the given value.
        /// </summary>
        /// <typeparam name="T">Type to use for setting conversion.</typeparam>
        /// <param name="name">Field or property name.</param>
        /// <param name="value">Setting value.</param>
        public void GetValue<T>(string name, out T value)
        {
            string setting = GetSettingName(name);

            value = DecryptValue(name, setting, RetrieveSetting(name, setting)).ConvertToType<T>();
        }

        // Encrypt setting value and return a base64 encoded value
        private string EncryptValue(string name, string setting, string value)
        {
            // If encrypt attribute has been applied, encrypt value
            if (GetEncryptStatus(name))
                return value.Encrypt(GenerateEncryptionKey(name), CipherStrength.Level6);

            return value;
        }

        // Decrypt setting value
        private string DecryptValue(string name, string setting, string value)
        {
            // If encrypt attribute has been applied, decrypt value
            if (GetEncryptStatus(name))
                return value.Decrypt(GenerateEncryptionKey(name), CipherStrength.Level6);

            return value;
        }

        // Generate encryption key based on any applied private encryption key in field or property attributes plus internal key
        private string GenerateEncryptionKey(string name)
        {
            string internalKey = InternalKey;
            string encryptionKey = GetEncryptKey(name);

            if (encryptionKey == null)
                return internalKey;

            // We continue to further obfuscate key provided in attribute since this is easily reflected...
            StringBuilder generatedKey = new StringBuilder();
            char eKey, iKeyL, iKeyR;
            int keyLength = internalKey.Length;
            int index;

            for (int i = 0; i < encryptionKey.Length; i++)
            {
                eKey = encryptionKey[i];
                index = i % keyLength;
                iKeyL = internalKey[index];
                iKeyR = internalKey[keyLength - index - 1];

                switch ((int)eKey % 3)
                {
                    case 0:
                        generatedKey.Append(iKeyL);
                        generatedKey.Append(eKey);
                        generatedKey.Append(iKeyR);
                        break;
                    case 1:
                        generatedKey.Append(iKeyR);
                        generatedKey.Append(eKey);
                        generatedKey.Append(iKeyL);
                        break;
                    case 2:
                        generatedKey.Append(eKey);
                        generatedKey.Append(iKeyL);
                        generatedKey.Append(iKeyR);
                        break;
                }
            }

            return generatedKey.ToString();
        }

        /// <summary>
        /// Initializes configuration settings from derived class fields or properties.
        /// </summary>
        protected virtual void Initialize()
        {
            // Make sure all desired settings exist initialized with default values. Settings are
            // assumed to be public fields or public properties in derived class - so we enumerate
            // through of these making sure a setting exists for each field and property

            // Verify a configuration setting exists for each field
            ExecuteActionForFields(field => CreateValue(field.Name, DeriveDefaultValue(field.Name, field.GetValue(this))));

            // Verify a configuration setting exists for each property
            ExecuteActionForProperties(property => CreateValue(property.Name, DeriveDefaultValue(property.Name, property.GetValue(this, null))), BindingFlags.GetProperty);

            // If any new values were encountered, make sure they are flushed to config file
            PersistSettings();

            // Load current settings
            Load();
        }

        /// <summary>
        /// Attempts to get best default value for given member.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <param name="value">Current field or property value.</param>
        /// <remarks>
        /// If <paramref name="value"/> is equal to its default(type) value, then any value derived from <see cref="DefaultValueAttribute"/> will be used instead.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected object DeriveDefaultValue(string name, object value)
        {
            // See if value is equal to its default value (i.e., uninitialized)
            if (Common.IsDefaultValue(value))
            {
                // See if any value exists in a DefaultValueAttribute
                object defaultValue = GetDefaultValue(name);
                if (defaultValue != null)
                    return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Returns an enumerator based on <see cref="String"/> elements that iterates over the field and property names of this class
        /// that are targeted for serialization to the configuration file.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        public IEnumerator<string> GetEnumerator()
        {
            List<string> members = new List<string>();

            // Get names of fields
            ExecuteActionForFields(field => members.Add(field.Name));

            // Get names of properties
            ExecuteActionForProperties(property => members.Add(property.Name), BindingFlags.GetProperty);

            return members.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Loads configuration file into setting fields.
        /// </summary>
        public virtual void Load()
        {
            // Load configuration file settings into fields
            ExecuteActionForFields(field => field.SetValue(this, GetValue(field.Name, field.FieldType)));
            
            // Load configuration file settings into properties
            ExecuteActionForProperties(property => property.SetValue(this, GetValue(property.Name, property.PropertyType), null), BindingFlags.SetProperty);
        }

        /// <summary>
        /// Saves setting fields into configuration file.
        /// </summary>
        public virtual void Save()
        {
            // Saves setting fields into configuration file values
            ExecuteActionForFields(field => SetValue(field.Name, field.GetValue(this)));
            
            // Saves setting properties into configuration file values
            ExecuteActionForProperties(property => SetValue(property.Name, property.GetValue(this, null)), BindingFlags.GetProperty);

            // Make sure any changes are flushed to config file
            PersistSettings();
        }

        /// <summary>
        /// Executes specified action over all public dervied class member fields.
        /// </summary>
        /// <param name="fieldAction">Action to excute for all dervied class member fields.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void ExecuteActionForFields(Action<FieldInfo> fieldAction)
        {
            ExecuteActionForMembers(fieldAction, this.GetType().GetFields(m_memberAccessBindingFlags));
        }

        /// <summary>
        /// Executes specified action over all public dervied class properties.
        /// </summary>
        /// <param name="propertyAction">Action to execute for all properties.</param>
        /// <param name="isGetOrSet"><see cref="BindingFlags.GetProperty"/> or <see cref="BindingFlags.SetProperty"/>.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void ExecuteActionForProperties(Action<PropertyInfo> propertyAction, BindingFlags isGetOrSet)
        {
            // Make sure only non-indexer properties are used for settings
            ExecuteActionForMembers(property => { if (property.GetIndexParameters().Length == 0) propertyAction(property); }, this.GetType().GetProperties(m_memberAccessBindingFlags | isGetOrSet));
        }

        // Execute specified action over specified memembers
        private void ExecuteActionForMembers<T>(Action<T> memberAction, T[] members) where T : MemberInfo
        {
            SerializeSettingAttribute attribute;

            // Execute action for each member
            foreach (T member in members)
            {
                // See if serialize setting attribute exists
                if (member.TryGetAttribute(out attribute))
                {
                    // Found serialze setting attribute, perform action if setting is true
                    if (attribute.Serialize)
                        memberAction(member);
                }
                else if (!m_requireSerializeSettingAttribute)
                {
                    // Didn't find serialize setting attribute and it's not required, so we perform action
                    memberAction(member);
                }
            }
        }

        /// <summary>
        /// Attempts to find specified attribute and return specified value.
        /// </summary>
        /// <typeparam name="TAttribute">Type of <see cref="Attribute"/> to find.</typeparam>
        /// <typeparam name="TValue">Type of value attribute delegate returns.</typeparam>
        /// <param name="name">Name of field or property to search for attribute.</param>
        /// <param name="defaultValue">Default value to return if attribute doesn't exist.</param>
        /// <param name="attributeValue">Function delegate used to return desired attribute property.</param>
        /// <returns>Specified attribute value if it exists; otherwise default value.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected TValue GetAttributeValue<TAttribute, TValue>(string name, TValue defaultValue, Func<TAttribute, TValue> attributeValue) where TAttribute : Attribute
        {
            TAttribute attribute;

            // See if field exists with specified name
            FieldInfo field = this.GetType().GetField(name, m_memberAccessBindingFlags);

            if (field != null)
            {
                // See if attribute exists on field
                if (field.TryGetAttribute(out attribute))
                {
                    // Return value as specified by delegate
                    return attributeValue(attribute);
                }

                // Attribute wasn't found, return default value
                return defaultValue;
            }

            // See if property exists with specified name
            PropertyInfo property = this.GetType().GetProperty(name, m_memberAccessBindingFlags);

            if (property != null)
            {
                // See if attribute exists on property
                if (property.TryGetAttribute(out attribute))
                {
                    // Return value as specified by delegate
                    return attributeValue(attribute);
                }

                // Attribute wasn't found, return default value
                return defaultValue;
            }

            // Return default value
            return defaultValue;
        }

        #endregion
    }
}