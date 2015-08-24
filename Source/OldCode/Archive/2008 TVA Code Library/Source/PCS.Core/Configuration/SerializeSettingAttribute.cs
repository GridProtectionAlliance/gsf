//*******************************************************************************************************
//  SerializeSettingAttribute.cs
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
using System.Reflection;

namespace PCS.Configuration
{
    /// <summary>
    /// Represents an attribute that determines if a property or field in a class derived from
    /// <see cref="CategorizedSettingsBase"/> or <see cref="AppSettingsBase"/> should be serialized
    /// to the configuration file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SerializeSettingAttribute : Attribute
    {
        #region [ Members ]

        // Fields
        private bool m_serialize;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SerializeSettingAttribute"/>; defaults to <c><see cref="Serialize"/> = true</c>.
        /// </summary>
        public SerializeSettingAttribute()
        {
            m_serialize = true;
        }

        /// <summary>
        /// Creates a new <see cref="SerializeSettingAttribute"/> with the specified <paramref name="serialize"/> value.
        /// </summary>
        /// <param name="serialize">
        /// Assigns flag that determines if the property or field this <see cref="SerializeSettingAttribute"/>
        /// modifies should be serialized to the configuration file.
        /// </param>
        public SerializeSettingAttribute(bool serialize)
        {
            m_serialize = serialize;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines if the property or field this <see cref="SerializeSettingAttribute"/>
        /// modifies should be serialized to the configuration file.
        /// </summary>
        public bool Serialize
        {
            get
            {
                return m_serialize;
            }
            set
            {
                m_serialize = value;
            }
        }

        #endregion
    }
}