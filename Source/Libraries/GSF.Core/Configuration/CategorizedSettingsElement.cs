//******************************************************************************************************
//  CategorizedSettingsElement.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/11/2006 - Pinal C. Patel
//       Generated original version of source code.
//  05/25/2006 - J. Ritchie Carroll
//       Added Try/Catch safety wrapper around GetTypedValue implementation.
//  06/01/2006 - J. Ritchie Carroll
//       Added GetTypedValue overload to handle boolean types as a special case.
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//  09/17/2008 - Pinal C. Patel
//       Converted code to C#.
//  09/22/2008 - J. Ritchie Carroll
//       Made boolean types a special case (i.e., using ParseBoolean extension).
//  09/29/2008 - Pinal C. Patel
//       Reviewed code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  04/15/2010 - Pinal C. Patel
//       Modified property setters to update the internal property bag only if values have changed.
//  04/20/2010 - Pinal C. Patel
//       Added new Category property for the purpose of managing user scope setting.
//       Removed publicly accessible constructors for manageability.
//       Added Scope property as a way of identifying user scope settings.
//  04/21/2010 - Pinal C. Patel
//       Removed unnecessary overloads of Update() for manageability.
//  04/22/2010 - Pinal C. Patel
//       Fixed encryption related issue introduced when adding support for user scope settings.
//  12/05/2010 - Pinal C. Patel
//       Modified Update() and ValueAs() to specify CultureInfo for the conversion.
//  01/11/2011 - Pinal C. Patel
//       Added support for use of Eval() in Value to reference other setting values and static members
//       of .NET types.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header,
//
//******************************************************************************************************

using System;
using System.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;
using GSF.Security.Cryptography;

namespace GSF.Configuration
{
    #region [ Enumerations ]

    /// <summary>
    /// Specifies the scope of a setting represented by <see cref="CategorizedSettingsElement"/>.
    /// </summary>
    public enum SettingScope
    {
        /// <summary>
        /// Settings is intended for user specific use.
        /// </summary>
        User,
        /// <summary>
        /// Settings is intended for application wide use.
        /// </summary>
        Application
    }

    #endregion

    /// <summary>
    /// Represents a settings entry in the config file.
    /// </summary>
    public class CategorizedSettingsElement : ConfigurationElement
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="Value"/> property.
        /// </summary>
        public const string DefaultValue = "";

        /// <summary>
        /// Specifies the default value for the <see cref="Description"/> property.
        /// </summary>
        public const string DefaultDescription = "";

        /// <summary>
        /// Specifies the default value for the <see cref="Encrypted"/> property.
        /// </summary>
        public const bool DefaultEncrypted = false;

        /// <summary>
        /// Specifies the default value for the <see cref="Scope"/> property.
        /// </summary>
        public const SettingScope DefaultScope = SettingScope.Application;

        private const string DefaultCryptoKey = "0679d9ae-aca5-4702-a3f5-604415096987";
        private const string EvalRegex = @"Eval\((?<Container>(?<Accessor>[\w\d]+\.?)+)\.(?<Target>[\w\d]+)\)";
        private const string EnvRegex = @"Env\(((?<Target>Machine|Process|User)\:)?(?<Name>[\w\d]+)\)";

        // Fields
        private string m_cryptoKey;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Required by the configuration API and is for internal use only.
        /// </summary>
        internal CategorizedSettingsElement(CategorizedSettingsElementCollection category)
            : this(category, "")
        {
        }

        /// <summary>
        /// Required by the configuration API and is for internal use only.
        /// </summary>
        internal CategorizedSettingsElement(CategorizedSettingsElementCollection category, string name)
        {
            Category = category;
            Name = name;
            m_cryptoKey = DefaultCryptoKey;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="CategorizedSettingsElementCollection"/> to which this <see cref="CategorizedSettingsElement"/> belongs.
        /// </summary>
        public CategorizedSettingsElementCollection Category { get; internal set; }

        /// <summary>
        /// Gets or sets the identifier of the setting.
        /// </summary>
        /// <returns>The identifier of the setting.</returns>
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get => (string)base["name"];
            set
            {
                base["name"] = value;
                Modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the value of the setting.
        /// </summary>
        /// <returns>The value of the setting.</returns>
        /// <remarks>
        /// <see cref="Value"/> can reference the value of another setting using <b>Eval([Section].[Setting])</b> syntax or the value of a known .NET 
        /// type's static member (field, non-indexed property or parameterless method) using <b>Eval([Type].[Member])</b> syntax as shown in the example below:
        /// <code>
        /// <![CDATA[
        /// <?xml version="1.0"?>
        /// <configuration>
        ///   <configSections>
        ///     <section name="categorizedSettings" type="GSF.Configuration.CategorizedSettingsSection, GSF.Core" />
        ///   </configSections>
        ///   <categorizedSettings>
        ///     <database>
        ///       <add name="SettingsTable" value="dbo.Settings" description="Table that contains the settings." 
        ///         encrypted="false" />
        ///       <add name="AdminEmail" value="SELECT * FROM Eval(Database.SettingsTable) WHERE Name = 'AdminEmail'" 
        ///         description="Email address of the administrator." encrypted="false" />
        ///       <add name="AuditQuery" value="SELECT * FROM dbo.Log WHERE EntryTime < 'Eval(System.DateTime.UtcNow)'" 
        ///         description="Query for retrieving audit records." encrypted="false" />
        ///     </database>
        ///   </categorizedSettings>
        /// </configuration>
        /// ]]>
        /// </code>
        /// </remarks>
        [ConfigurationProperty("value", IsRequired = true, DefaultValue = DefaultValue)]
        public string Value
        {
            get => EvaluateValue(GetEnvValue(DecryptValue(GetRawValue())));
            set
            {
                // Continue only if values are different.
                string currentValue;

                try
                {
                    currentValue = DecryptValue(GetRawValue());
                }
                catch (ConfigurationErrorsException)
                {
                    // Clear value if it fails to decrypt, updating anyway
                    currentValue = string.Empty;
                }

                if (value.ToNonNullString().Equals(currentValue))
                    return;

                // Ensure only Eval() can replace Eval().
                if (s_evalRegex.IsMatch(currentValue) && !s_evalRegex.IsMatch(value))
                    return;

                // Ensure only Env() can replace Env().
                if (s_envRegex.IsMatch(currentValue) && !s_envRegex.IsMatch(value))
                    return;

                value = EncryptValue(value);

                if (Scope == SettingScope.Application || Category[Name] is null)
                {
                    // Setting is application wide or is being added for the first time.
                    base["value"] = value;
                    Modified = true;
                }
                else
                {
                    // Setting is user specific so update setting in user settings store.
                    Category.Section.File.UserSettings.Write(Category.Name, Name, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the description of the setting.
        /// </summary>
        /// <returns>The description of the setting.</returns>
        [ConfigurationProperty("description", IsRequired = true, DefaultValue = DefaultDescription)]
        public string Description
        {
            get => (string)base["description"];
            set
            {
                // Continue only if values are different.
                if (value.ToNonNullString().Equals(Description))
                    return;

                base["description"] = value;
                Modified = true;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the setting value is to be encrypted.
        /// </summary>
        /// <returns>true, if the setting value is to be encrypted; otherwise false.</returns>
        [ConfigurationProperty("encrypted", IsRequired = true, DefaultValue = DefaultEncrypted)]
        public bool Encrypted
        {
            get => (bool)base["encrypted"];
            set
            {
                // Continue only if values are different.
                if (value.Equals(Encrypted))
                    return;

                base["encrypted"] = value;
                Modified = true;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SettingScope"/>.
        /// </summary>
        [ConfigurationProperty("scope", IsRequired = false, DefaultValue = DefaultScope)]
        public SettingScope Scope
        {
            get => (SettingScope)base["scope"];
            set
            {
                // Continue only if values are different.
                if (value.Equals(Scope))
                    return;

                base["scope"] = value;
                Modified = true;
            }
        }

        /// <summary>
        /// Gets value that will actually be serialized to the configuration file.
        /// </summary>
        public string SerializedValue => EncryptValue((string)base["value"]);

        internal bool Modified
        {
            set
            {
                if (Category is not null)
                    Category.Modified = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Sets the key to be used for encrypting and decrypting the <see cref="Value"/>.
        /// </summary>
        /// <param name="cryptoKey">New crypto key.</param>
        public void SetCryptoKey(string cryptoKey)
        {
            if (string.IsNullOrEmpty(cryptoKey))
                return;

            // Re-encrypt the existing value with the new key. This is done because the value gets encrypted,
            // if specified, with the default crypto key when the value is set during instantiation.
            string decryptedValue;

            try
            {
                decryptedValue = Value;
            }
            catch (ConfigurationErrorsException)
            {
                // Clear value if it fails to decrypt
                decryptedValue = string.Empty;
            }

            m_cryptoKey = cryptoKey;
            Value = decryptedValue;
            Modified = true;
        }

        /// <summary>
        /// Updates setting information.
        /// </summary>
        /// <param name="value">New setting value.</param>
        public void Update(object value) => 
            Update(value, Description, Encrypted, Scope);

        /// <summary>
        /// Updates setting information.
        /// </summary>
        /// <param name="value">New setting value.</param>
        /// <param name="description">New setting description.</param>
        public void Update(object value, string description) => 
            Update(value, description, Encrypted, Scope);
        
        /// <summary>
        /// Updates setting information.
        /// </summary>
        /// <param name="value">New setting value.</param>
        /// <param name="description">New setting description.</param>
        /// <param name="encrypted">A boolean value that indicated whether the new setting value is to be encrypted.</param>
        public void Update(object value, string description, bool encrypted) => 
            Update(value, description, encrypted, Scope);

        /// <summary>
        /// Updates setting information.
        /// </summary>
        /// <param name="value">New setting value.</param>
        /// <param name="description">New setting description.</param>
        /// <param name="encrypted">A boolean value that indicated whether the new setting value is to be encrypted.</param>
        /// <param name="scope">One of the <see cref="SettingScope"/> values.</param>
        public void Update(object value, string description, bool encrypted, SettingScope scope)
        {
            Scope = scope;
            Encrypted = encrypted;
            Value = Common.TypeConvertToString(value, Category.Section.File.Culture);
            Description = description;
        }

        /// <summary>
        /// Gets the setting value as the specified type.
        /// </summary>
        /// <typeparam name="T">Type to which the setting value is to be converted.</typeparam>
        /// <returns>The type-coerced value of the setting.</returns>
        /// <remarks>
        /// If this function fails to properly coerce value to specified type, the default value is returned.
        /// </remarks>
        public T ValueAs<T>() => 
            ValueAs(default(T));

        /// <summary>
        /// Gets the setting value as the specified type.
        /// </summary>
        /// <typeparam name="T">Type to which the setting value is to be converted.</typeparam>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>The type-coerced value of the setting.</returns>
        /// <remarks>
        /// If this function fails to properly coerce value to specified type, the default value is returned.
        /// </remarks>
        public T ValueAs<T>(T defaultValue)
        {
            try
            {
                string value = Value;

                // If value is an empty string, use default value.
                return string.IsNullOrEmpty(value) ? 
                    defaultValue : 
                    value.ConvertToType<T>(Category.Section.File.Culture);
            }
            catch
            {
                // Conversion to target type failed so use the default value.
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the setting value as a string.
        /// </summary>
        /// <returns>Value as string.</returns>
        public string ValueAsString() => 
            ValueAsString(string.Empty);

        /// <summary>
        /// Gets the setting value as a string.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as string.</returns>
        public string ValueAsString(string defaultValue) => 
            ValueAs(defaultValue);

        /// <summary>
        /// Gets the setting value as a boolean.
        /// </summary>
        /// <returns>Value as boolean.</returns>
        public bool ValueAsBoolean() => 
            ValueAsBoolean(default);

        /// <summary>
        /// Gets the setting value as a boolean.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as boolean.</returns>
        public bool ValueAsBoolean(bool defaultValue)
        {
            try
            {
                string value = Value;

                // Value is an empty string - use default value.
                if (string.IsNullOrEmpty(value))
                    return defaultValue;

                // Value is not empty string - convert to boolean.
                return value.ParseBoolean();
            }
            catch
            {
                // Conversion to target type failed so use the default value.
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the setting value as a byte.
        /// </summary>
        /// <returns>Value as byte.</returns>
        public byte ValueAsByte() => 
            ValueAsByte(default);

        /// <summary>
        /// Gets the setting value as a byte.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as byte.</returns>
        public byte ValueAsByte(byte defaultValue) => 
            ValueAs(defaultValue);
        
        /// <summary>
        /// Gets the setting value as a signed byte.
        /// </summary>
        /// <returns>Value as signed byte.</returns>
        public sbyte ValueAsSByte() => 
            ValueAsSByte(default);

        /// <summary>
        /// Gets the setting value as a signed byte.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as signed byte.</returns>
        public sbyte ValueAsSByte(sbyte defaultValue) => 
            ValueAs(defaultValue);
        
        /// <summary>
        /// Gets the setting value as a char.
        /// </summary>
        /// <returns>Value as char.</returns>
        public char ValueAsChar() => 
            ValueAsChar(default);

        /// <summary>
        /// Gets the setting value as a char.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as char.</returns>
        public char ValueAsChar(char defaultValue) => 
            ValueAs(defaultValue);

        /// <summary>
        /// Gets the setting value as a short.
        /// </summary>
        /// <returns>Value as short.</returns>
        public short ValueAsInt16() => 
            ValueAsInt16(default);

        /// <summary>
        /// Gets the setting value as a short.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as short.</returns>
        public short ValueAsInt16(short defaultValue) => 
            ValueAs(defaultValue);

        /// <summary>
        /// Gets the setting value as an int.
        /// </summary>
        /// <returns>Value as int.</returns>
        public int ValueAsInt32() => 
            ValueAsInt32(default);

        /// <summary>
        /// Gets the setting value as an int.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as int.</returns>
        public int ValueAsInt32(int defaultValue) => 
            ValueAs(defaultValue);

        /// <summary>
        /// Gets the setting value as a long.
        /// </summary>
        /// <returns>Value as long.</returns>
        public long ValueAsInt64() => 
            ValueAsInt64(default);

        /// <summary>
        /// Gets the setting value as a long.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as long.</returns>
        public long ValueAsInt64(long defaultValue) => 
            ValueAs(defaultValue);

        /// <summary>
        /// Gets the setting value as an unsigned short.
        /// </summary>
        /// <returns>Value as unsigned short.</returns>
        public ushort ValueAsUInt16() => 
            ValueAsUInt16(default);

        /// <summary>
        /// Gets the setting value as an unsigned short.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as unsigned short.</returns>
        public ushort ValueAsUInt16(ushort defaultValue) => 
            ValueAs(defaultValue);

        /// <summary>
        /// Gets the setting value as an unsigned int.
        /// </summary>
        /// <returns>Value as unsigned int.</returns>
        public uint ValueAsUInt32() => 
            ValueAsUInt32(default);

        /// <summary>
        /// Gets the setting value as an unsigned int.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as unsigned int.</returns>
        public uint ValueAsUInt32(uint defaultValue) => 
            ValueAs(defaultValue);

        /// <summary>
        /// Gets the setting value as an unsigned long.
        /// </summary>
        /// <returns>Value as unsigned long.</returns>
        public ulong ValueAsUInt64() => 
            ValueAsUInt64(default);

        /// <summary>
        /// Gets the setting value as an unsigned long.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as unsigned long.</returns>
        public ulong ValueAsUInt64(ulong defaultValue) => 
            ValueAs(defaultValue);

        /// <summary>
        /// Gets the setting value as a float.
        /// </summary>
        /// <returns>Value as float.</returns>
        public float ValueAsSingle() => 
            ValueAsSingle(default);

        /// <summary>
        /// Gets the setting value as a float.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as float.</returns>
        public float ValueAsSingle(float defaultValue) => 
            ValueAs(defaultValue);

        /// <summary>
        /// Gets the setting value as a double.
        /// </summary>
        /// <returns>Value as double.</returns>
        public double ValueAsDouble() => 
            ValueAsDouble(default);

        /// <summary>
        /// Gets the setting value as a double.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as double.</returns>
        public double ValueAsDouble(double defaultValue) => 
            ValueAs(defaultValue);

        /// <summary>
        /// Gets the setting value as a decimal.
        /// </summary>
        /// <returns>Value as decimal.</returns>
        public decimal ValueAsDecimal() => 
            ValueAsDecimal(default);

        /// <summary>
        /// Gets the setting value as a decimal.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as decimal.</returns>
        public decimal ValueAsDecimal(decimal defaultValue) => 
            ValueAs(defaultValue);

        /// <summary>
        /// Gets the setting value as DateTime.
        /// </summary>
        /// <returns>Value as DateTime.</returns>
        public DateTime ValueAsDateTime() => 
            ValueAsDateTime(default);
        
        /// <summary>
        /// Gets the setting value as DateTime.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as DateTime.</returns>
        public DateTime ValueAsDateTime(DateTime defaultValue) => 
            ValueAs(defaultValue);

        private string GetRawValue()
        {
            string value = (string)base["value"];

            // If setting is user specific, retrieve value from user settings store.
            if (Scope == SettingScope.User)
                value = Category.Section.File.UserSettings.Read(Category.Name, Name, value);

            return value;
        }

        private string EncryptValue(string value)
        {
            if (!Encrypted || string.IsNullOrEmpty(value))
                return value;

            try
            {
                // Encrypts the element's value.
                value = value.Encrypt(m_cryptoKey, CipherStrength.Aes256);
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException($"Failed to encrypt '{value}'", ex);
            }

            return value;
        }

        private string DecryptValue(string value)
        {
            if (!Encrypted || string.IsNullOrEmpty(value))
                return value;

            try
            {
                // Decrypts the element's value.
                value = value.Decrypt(m_cryptoKey, CipherStrength.Aes256);
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException($"Failed to decrypt '{value}'", ex);
            }

            return value;
        }

        private string EvaluateValue(string value)
        {
            ConfigurationFile config = Category.Section.File;

            foreach (Match match in s_evalRegex.Matches(value))
            {
                string containerValue = match.Groups["Container"].Value;
                string targetValue = match.Groups["Target"].Value;

                // Try replacing Eval() with the actual value.
                CategorizedSettingsElement setting = config.Settings[containerValue][targetValue];

                if (setting is not null)
                {
                    // Replacement is the value of another setting.
                    value = value.Replace(match.Value, setting.Value);
                }
                else
                {
                    // Replacement could be the value of .NET type's static member.
                    Type target = Type.GetType(containerValue);

                    if (target is null)
                        continue;

                    // Specified .NET type is found.
                    MemberInfo[] members = target.GetMember(targetValue, BindingFlags.Public | BindingFlags.Static);

                    if (members.Length > 0)
                    {
                        // Specified target member is found in the .NET type.
                        value = members[0].MemberType switch
                        {
                            MemberTypes.Field =>
                                // Member is a static field.
                                value.Replace(match.Value, ((FieldInfo)members[0]).GetValue(null).ToNonNullString()),
                            MemberTypes.Method =>
                                // Member is a static method.
                                value.Replace(match.Value, ((MethodInfo)members[0]).Invoke(null, Array.Empty<object>()).ToNonNullString()),
                            MemberTypes.Property =>
                                // Member is a static property.
                                value.Replace(match.Value, ((PropertyInfo)members[0]).GetValue(null, Array.Empty<object>()).ToNonNullString()),
                            _ => value
                        };
                    }
                }
            }

            return value;
        }

        private static string GetEnvValue(string value)
        {
            foreach (Match match in s_envRegex.Matches(value))
            {
                Group targetValue = match.Groups["Target"];

                if (!targetValue.Success || !Enum.TryParse(targetValue.Value, true, out EnvironmentVariableTarget target))
                    target = EnvironmentVariableTarget.Machine;

                // Try replacing Env() with the actual value.
                value = value.Replace(match.Value, Environment.GetEnvironmentVariable(match.Groups["Name"].Value, target));
            }

            return value;
        }

        #endregion

        #region [ Static ]

        private static readonly Regex s_evalRegex;
        private static readonly Regex s_envRegex;

        static CategorizedSettingsElement()
        {
            s_evalRegex = new Regex(EvalRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            s_envRegex = new Regex(EnvRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        #endregion
    }
}
