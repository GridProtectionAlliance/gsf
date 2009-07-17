//*******************************************************************************************************
//  CategorizedSettingsElement.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR 2W-C
//       Phone: 423-751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/11/2006 - Pinal C. Patel
//       Generated original version of source code.
//  05/25/2006 - James R. Carroll
//       Added Try/Catch safety wrapper around GetTypedValue implementation.
//  06/01/2006 - James R. Carroll
//       Added GetTypedValue overload to handle boolean types as a special case.
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//  09/17/2008 - Pinal C. Patel
//       Converted code to C#.
//  09/22/2008 - James R. Carroll
//       Made boolean types a special case (i.e., using ParseBoolean extension).
//  09/29/2008 - Pinal C. Patel
//       Reviewed code comments.
//
//*******************************************************************************************************

using System;
using System.Configuration;
using TVA.Security.Cryptography;

namespace TVA.Configuration
{
    /// <summary>
    /// Represents a settings entry in the config file.
    /// </summary>
    public class CategorizedSettingsElement : ConfigurationElement
    {
        #region [ Members ]

        // Constants
        private const string DefaultCryptoKey = "0679d9ae-aca5-4702-a3f5-604415096987";

        // Fields
        private string m_cryptoKey;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Required by the configuration API and is for internal use only.
        /// </summary>
        internal CategorizedSettingsElement()
            : this("")
        {
        }

        /// <summary>
        /// Required by the configuration API and is for internal use only.
        /// </summary>
        internal CategorizedSettingsElement(string name)
            : this(name, "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategorizedSettingsElement"/> class.
        /// </summary>
        /// <param name="name">The identifier of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        public CategorizedSettingsElement(string name, string value)
            : this(name, value, "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategorizedSettingsElement"/> class.
        /// </summary>
        /// <param name="name">The identifier of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        /// <param name="description">The description of the setting.</param>
        public CategorizedSettingsElement(string name, string value, string description)
            : this(name, value, description, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategorizedSettingsElement"/> class.
        /// </summary>
        /// <param name="name">The identifier of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        /// <param name="description">The description of the setting.</param>
        /// <param name="encrypted">true if the setting value is to be encrypted; otherwise false.</param>
        public CategorizedSettingsElement(string name, string value, string description, bool encrypted)
        {
            m_cryptoKey = DefaultCryptoKey;
            this.Name = name;
            Update(value, description, encrypted);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the identifier of the setting.
        /// </summary>
        /// <returns>The identifier of the setting.</returns>
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the setting.
        /// </summary>
        /// <returns>The value of the setting.</returns>
        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get
            {
                return DecryptValue((string)base["value"]);
            }
            set
            {
                base["value"] = EncryptValue(value);
            }
        }

        /// <summary>
        /// Gets or sets the description of the setting.
        /// </summary>
        /// <returns>The description of the setting.</returns>
        [ConfigurationProperty("description", IsRequired = true)]
        public string Description
        {
            get
            {
                return (string)base["description"];
            }
            set
            {
                base["description"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the setting value is to be encrypted.
        /// </summary>
        /// <returns>true, if the setting value is to be encrypted; otherwise false.</returns>
        [ConfigurationProperty("encrypted", IsRequired = true)]
        public bool Encrypted
        {
            get
            {
                return (bool)base["encrypted"];
            }
            set
            {
                string elementValue = this.Value;   // Gets the decrypted value if encrypted.
                base["encrypted"] = value;
                this.Value = elementValue;          // This will cause encryption to be performed if required.
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
            if (!string.IsNullOrEmpty(cryptoKey))
            {
                // Re-encrypt the existing value with the new key. This is done because the value gets encrypted,
                // if specified, with the default crypto key when the value is set during instantiation.
                string decryptedValue = Value;
                m_cryptoKey = cryptoKey;
                Value = decryptedValue;
            }
        }

        /// <summary>
        /// Updates setting information.
        /// </summary>
        /// <param name="value">New setting value.</param>
        /// <param name="description">New setting description.</param>
        public void Update(object value, string description)
        {
            Update(value.ToString(), description);
        }

        /// <summary>
        /// Updates setting information.
        /// </summary>
        /// <param name="value">New setting value.</param>
        /// <param name="description">New setting description.</param>
        public void Update(string value, string description)
        {
            Update(value, description, Encrypted);
        }

        /// <summary>
        /// Updates setting information.
        /// </summary>
        /// <param name="value">New setting value.</param>
        /// <param name="description">New setting description.</param>
        /// <param name="encrypted">A boolean value that indicated whether the new setting value is to be encrypted.</param>
        public void Update(object value, string description, bool encrypted)
        {
            Update(value.ToString(), description, encrypted);
        }

        /// <summary>
        /// Updates setting information.
        /// </summary>
        /// <param name="value">New setting value.</param>
        /// <param name="description">New setting description.</param>
        /// <param name="encrypted">A boolean value that indicated whether the new setting value is to be encrypted.</param>
        public void Update(string value, string description, bool encrypted)
        {
            this.Value = value;
            this.Description = description;
            this.Encrypted = encrypted;
        }

        /// <summary>
        /// Gets the setting value as the specified type.
        /// </summary>
        /// <typeparam name="T">Type to which the setting value is to be converted.</typeparam>
        /// <returns>The type-coerced value of the setting.</returns>
        /// <remarks>
        /// If this function fails to properly coerce value to specified type, the default value is returned.
        /// </remarks>
        public T ValueAs<T>()
        {
            return this.ValueAs<T>(default(T));
        }

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
                if (string.IsNullOrEmpty(Value))
                    // Value is an empty string - use default value.
                    return defaultValue;
                else
                    // Value is not empty string - convert to target type.
                    return Value.ConvertToType<T>();
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
        public string ValueAsString()
        {
            return ValueAsString("");
        }

        /// <summary>
        /// Gets the setting value as a string.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as string.</returns>
        public string ValueAsString(string defaultValue)
        {
            string setting = Value;

            if (string.IsNullOrEmpty(setting))
                return defaultValue;
            else
                return setting;
        }

        /// <summary>
        /// Gets the setting value as a boolean.
        /// </summary>
        /// <returns>Value as boolean.</returns>
        public bool ValueAsBoolean()
        {
            return ValueAsBoolean(default(bool));
        }

        /// <summary>
        /// Gets the setting value as a boolean.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as boolean.</returns>
        public bool ValueAsBoolean(bool defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as a byte.
        /// </summary>
        /// <returns>Value as byte.</returns>
        public byte ValueAsByte()
        {
            return ValueAsByte(default(byte));
        }

        /// <summary>
        /// Gets the setting value as a byte.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as byte.</returns>
        public byte ValueAsByte(byte defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as a signed byte.
        /// </summary>
        /// <returns>Value as signed byte.</returns>
        [CLSCompliant(false)]
        public sbyte ValueAsSByte()
        {
            return ValueAsSByte(default(sbyte));
        }

        /// <summary>
        /// Gets the setting value as a signed byte.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as signed byte.</returns>
        [CLSCompliant(false)]
        public sbyte ValueAsSByte(sbyte defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as a char.
        /// </summary>
        /// <returns>Value as char.</returns>
        public char ValueAsChar()
        {
            return ValueAsChar(default(char));
        }

        /// <summary>
        /// Gets the setting value as a char.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as char.</returns>
        public char ValueAsChar(char defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as a short.
        /// </summary>
        /// <returns>Value as short.</returns>
        public short ValueAsInt16()
        {
            return ValueAsInt16(default(short));
        }

        /// <summary>
        /// Gets the setting value as a short.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as short.</returns>
        public short ValueAsInt16(short defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as an int.
        /// </summary>
        /// <returns>Value as int.</returns>
        public int ValueAsInt32()
        {
            return ValueAsInt32(default(int));
        }

        /// <summary>
        /// Gets the setting value as an int.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as int.</returns>
        public int ValueAsInt32(int defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as a long.
        /// </summary>
        /// <returns>Value as long.</returns>
        public long ValueAsInt64()
        {
            return ValueAsInt64(default(long));
        }

        /// <summary>
        /// Gets the setting value as a long.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as long.</returns>
        public long ValueAsInt64(long defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as an unsigned short.
        /// </summary>
        /// <returns>Value as unsigned short.</returns>
        [CLSCompliant(false)]
        public ushort ValueAsUInt16()
        {
            return ValueAsUInt16(default(ushort));
        }

        /// <summary>
        /// Gets the setting value as an unsigned short.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as unsigned short.</returns>
        [CLSCompliant(false)]
        public ushort ValueAsUInt16(ushort defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as an unsigned int.
        /// </summary>
        /// <returns>Value as unsigned int.</returns>
        [CLSCompliant(false)]
        public uint ValueAsUInt32()
        {
            return ValueAsUInt32(default(uint));
        }

        /// <summary>
        /// Gets the setting value as an unsigned int.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as unsigned int.</returns>
        [CLSCompliant(false)]
        public uint ValueAsUInt32(uint defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as an unsigned long.
        /// </summary>
        /// <returns>Value as unsigned long.</returns>
        [CLSCompliant(false)]
        public ulong ValueAsUInt64()
        {
            return ValueAsUInt64(default(ulong));
        }

        /// <summary>
        /// Gets the setting value as an unsigned long.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as unsigned long.</returns>
        [CLSCompliant(false)]
        public ulong ValueAsUInt64(ulong defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as a float.
        /// </summary>
        /// <returns>Value as float.</returns>
        public float ValueAsSingle()
        {
            return ValueAsSingle(default(float));
        }

        /// <summary>
        /// Gets the setting value as a float.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as float.</returns>
        public float ValueAsSingle(float defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as a double.
        /// </summary>
        /// <returns>Value as double.</returns>
        public double ValueAsDouble()
        {
            return ValueAsDouble(default(double));
        }

        /// <summary>
        /// Gets the setting value as a double.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as double.</returns>
        public double ValueAsDouble(double defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as a decimal.
        /// </summary>
        /// <returns>Value as decimal.</returns>
        public decimal ValueAsDecimal()
        {
            return ValueAsDecimal(default(decimal));
        }

        /// <summary>
        /// Gets the setting value as a decimal.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as decimal.</returns>
        public decimal ValueAsDecimal(decimal defaultValue)
        {
            return ValueAs(defaultValue);
        }

        /// <summary>
        /// Gets the setting value as DateTime.
        /// </summary>
        /// <returns>Value as DateTime.</returns>
        public DateTime ValueAsDateTime()
        {
            return ValueAsDateTime(default(DateTime));
        }

        /// <summary>
        /// Gets the setting value as DateTime.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the setting value is empty.</param>
        /// <returns>Value as DateTime.</returns>
        public DateTime ValueAsDateTime(DateTime defaultValue)
        {
            return ValueAs(defaultValue);
        }

        private string EncryptValue(string value)
        {
            if ((base["encrypted"] != null) && ((bool)base["encrypted"]))
            {
                // Encrypts the element's value.
                value = value.Encrypt(m_cryptoKey, CipherStrength.Level4);

            }
            return value;
        }

        private string DecryptValue(string value)
        {
            if ((base["encrypted"] != null) && ((bool)base["encrypted"]))
            {
                // Decrypts the element's value.
                return value.Decrypt(m_cryptoKey, CipherStrength.Level4);
            }

            return value;
        }

        #endregion
    }
}
