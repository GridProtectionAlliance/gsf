//******************************************************************************************************
//  SettingNameAttribute.cs - Gbtc
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
//  03/31/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Linq;

namespace GSF.Configuration
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
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SettingNameAttribute"/> with the specified <paramref name="names"/> value.
        /// </summary>
        /// <param name="names">Assigns name(s) used to serialize setting into config file.</param>
        public SettingNameAttribute(params string[] names)
        {
            Names = names;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the name used to serialize field or property into config file.
        /// </summary>
        public string Name => Names.First();

        /// <summary>
        /// Gets the names used to serialize field or property into config file.
        /// </summary>
        public string[] Names { get; }

        #endregion
    }
}