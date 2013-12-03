//******************************************************************************************************
//  EncryptSettingAttribute.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/01/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.Configuration
{
    /// <summary>
    /// Represents an attribute that determines if a property or field in a class derived from
    /// <see cref="CategorizedSettingsBase"/> or <see cref="AppSettingsBase"/> should be encrypted
    /// when it is serialized to the configuration file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class EncryptSettingAttribute : Attribute
    {
        #region [ Members ]

        // Fields
        private readonly bool m_encrypt;
        private string m_privateKey;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="EncryptSettingAttribute"/>; defaults to <c><see cref="Encrypt"/> = true</c>.
        /// </summary>
        public EncryptSettingAttribute()
        {
            m_encrypt = true;
        }

        /// <summary>
        /// Creates a new <see cref="EncryptSettingAttribute"/> with the specified <paramref name="encrypt"/> value.
        /// </summary>
        /// <param name="encrypt">
        /// Assigns flag that determines if the property or field this <see cref="EncryptSettingAttribute"/>
        /// modifies should be encrypted when serialized to the configuration file.
        /// </param>
        public EncryptSettingAttribute(bool encrypt)
        {
            m_encrypt = encrypt;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines if the property or field this <see cref="EncryptSettingAttribute"/>
        /// modifies should be encrypted when serialized to the configuration file.
        /// </summary>
        public bool Encrypt
        {
            get
            {
                return m_encrypt;
            }
        }

        /// <summary>
        /// Gets or sets optional encryption key that will be used on a setting for added security.
        /// </summary>
        /// <remarks>
        /// This key is not the actual key used for encryption, it is used for hash lookup of the actual AES key.
        /// If this key is not specified, the property name will be used for the hash lookup.
        /// </remarks>
        public string PrivateKey
        {
            get
            {
                return m_privateKey;
            }
            set
            {
                m_privateKey = value;
            }
        }

        #endregion
    }
}