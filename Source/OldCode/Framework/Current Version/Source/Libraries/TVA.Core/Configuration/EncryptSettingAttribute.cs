//*******************************************************************************************************
//  EncryptSettingAttribute.cs
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
//  04/01/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Configuration
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
        private bool m_encrypt;
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