using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Configuration;
//using TVA.Common;
//using TVA.Text.Common;
using TVA.Security.Cryptography;
//using TVA.Security.Cryptography.Common;

//*******************************************************************************************************
//  TVA.Configuration.CategorizedSettingsElement.vb - Categorized Settings Element
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
//  04/11/2006 - Pinal C. Patel
//       Generated original version of source code.
//  05/25/2006 - J. Ritchie Carroll
//       Added Try/Catch safety wrapper around GetTypedValue implementation.
//  06/01/2006 - J. Ritchie Carroll
//       Added GetTypedValue overload to handle boolean types as a special case.
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************


namespace TVA
{
    namespace Configuration
    {

        /// <summary>
        /// Represents a configuration element under the categories of categorizedSettings section within
        /// a configuration file.
        /// </summary>
        public class CategorizedSettingsElement : ConfigurationElement
        {


            private const string CryptoKey = "0679d9ae-aca5-4702-a3f5-604415096987";

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
            /// Initializes a new instance of TVA.Configuration.CategorizedSettingsElement with the specified
            /// name and value information.
            /// </summary>
            /// <param name="name">The identifier string of the element.</param>
            /// <param name="value">The value string of the element.</param>
            public CategorizedSettingsElement(string name, string value)
                : this(name, value, "")
            {
            }

            /// <summary>
            /// Initializes a new instance of TVA.Configuration.CategorizedSettingsElement with the specified
            /// name and value information.
            /// </summary>
            /// <param name="name">The identifier string of the element.</param>
            /// <param name="value">The value string of the element.</param>
            /// <param name="description">The description string of the element.</param>
            public CategorizedSettingsElement(string name, string value, string description)
                : this(name, value, description, false)
            {
            }

            /// <summary>
            /// Initializes a new instance of TVA.Configuration.CategorizedSettingsElement with the specified
            /// name and value information.
            /// </summary>
            /// <param name="name">The identifier string of the element.</param>
            /// <param name="value">The value string of the element.</param>
            /// <param name="description">The description string of the element.</param>
            /// <param name="encrypted">True, if the value string of the element is to be encrypted; otherwise false.</param>
            public CategorizedSettingsElement(string name, string value, string description, bool encrypted)
            {
                this.Name = name;
                this.Value = value;
                this.Description = description;
                this.Encrypted = encrypted;
            }

            /// <summary>
            /// Gets or sets the identifier string of the element.
            /// </summary>
            /// <returns>The identifier string of the element.</returns>
            [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
            public string Name
            {
                get
                {
                    return Convert.ToString(base.Item("name"));
                }
                set
                {
                    base.Item("name") = value;
                }
            }

            /// <summary>
            /// Gets or sets the value string of the element.
            /// </summary>
            /// <returns>The value string of the element.</returns>
            [ConfigurationProperty("value", IsRequired = true)]
            public string Value
            {
                get
                {
                    return DecryptValue(Convert.ToString(base.Item("value")));
                }
                set
                {
                    base.Item("value") = EncryptValue(value);
                }
            }

            /// <summary>
            /// Gets the element value as the specified type.
            /// </summary>
            /// <typeparam name="T">Type to which the value string is to be converted.</typeparam>
            /// <param name="defaultValue">The default value to return if the value string is empty.</param>
            /// <returns>The type-coerced value of the referenced setting.</returns>
            /// <remarks>If this function fails to properly coerce value to specified type, the default value is
            /// returned.</remarks>
            public T GetTypedValue<T>(T defaultValue)
            {

                try
                {
                    string stringValue = Value();

                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        // Converts the element's value string, if present, to the proper type.
                        if (typeof(T).IsEnum)
                        {
                            // Parses the string to the equivalent enumeration.
                            return ((T)(@Enum.Parse(typeof(T), stringValue)));
                        }
                        else
                        {
                            // Casts the string to the specified type.
                            return ((T)((object)stringValue));
                        }
                    }
                    else
                    {
                        // If the element's value string is not present, uses the default.
                        return defaultValue;
                    }
                }
                catch
                {
                    return defaultValue;
                }

            }

            /// <summary>
            /// Gets or sets the description string of the element.
            /// </summary>
            /// <returns>The description string of the element.</returns>
            [ConfigurationProperty("description", IsRequired = true)]
            public string Description
            {
                get
                {
                    return Convert.ToString(base.Item("description"));
                }
                set
                {
                    base.Item("description") = value;
                }
            }

            /// <summary>
            /// Gets or sets a boolean indicating whether the value string of the element is to be encrypted.
            /// </summary>
            /// <returns>True, if the value string of the element is to be encrypted; otherwise, false.</returns>
            [ConfigurationProperty("encrypted", IsRequired = true)]
            public bool Encrypted
            {
                get
                {
                    return Convert.ToBoolean(base.Item("encrypted"));
                }
                set
                {
                    string elementValue = this.Value; // Gets the decrypted value if encrypted.
                    base.Item("encrypted") = value;
                    this.Value = elementValue; // Setting the value again will cause encryption to be performed,
                    // if required.
                }
            }

            private string EncryptValue(string value)
            {

                string encryptedValue = value;
                if ((base.Item("encrypted") != null) && Convert.ToBoolean(base.Item("encrypted")))
                {
                    // Encrypts the element's value.
                    encryptedValue = TVA.Security.Cryptography.Common.Encrypt(value, CryptoKey, EncryptLevel.Level4);
                }
                return encryptedValue;

            }

            private string DecryptValue(string value)
            {

                string decryptedValue = value;
                if ((base.Item("encrypted") != null) && Convert.ToBoolean(base.Item("encrypted")))
                {
                    // Decrypts the element's value.
                    decryptedValue = TVA.Security.Cryptography.Common.Decrypt(value, CryptoKey, EncryptLevel.Level4);
                }
                return decryptedValue;

            }

        }

    }

}
