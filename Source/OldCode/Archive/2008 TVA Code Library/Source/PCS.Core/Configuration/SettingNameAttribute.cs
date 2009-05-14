//*******************************************************************************************************
//  SettingNameAttribute.cs
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
//  03/31/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Reflection;

namespace PCS.Configuration
{
    /// <summary>
    /// Represents an attribute that defines the setting name of a property or field in a class derived from
    /// <see cref="CategorizedSettingsBase"/> or <see cref="AppSettingsBase"/> when serializing the value
    /// to the configuration file.
    /// </summary>
    /// <remarks>
    /// This attribute allows consumers to override the name of the setting going into the configuration file,
    /// if the attribute doesn't exist the property or field name is used.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SettingNameAttribute : Attribute
    {
        #region [ Members ]

        // Fields
        private string m_name;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SettingNameAttribute"/> with the specified <paramref name="name"/> value.
        /// </summary>
        /// <param name="name">Assigns name used to serialize setting into config file.</param>
        public SettingNameAttribute(string name)
        {
            m_name = name;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the name used to serialize field or property into config file.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        #endregion
    }
}