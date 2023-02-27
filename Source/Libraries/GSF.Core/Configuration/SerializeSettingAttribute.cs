//******************************************************************************************************
//  SerializeSettingAttribute.cs - Gbtc
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
//  01/30/2009 - J. Ritchie Carroll
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
    /// <see cref="CategorizedSettingsBase"/> or <see cref="AppSettingsBase"/> should be serialized
    /// to the configuration file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SerializeSettingAttribute : Attribute
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SerializeSettingAttribute"/>; defaults to <c><see cref="Serialize"/> = true</c>.
        /// </summary>
        public SerializeSettingAttribute()
        {
            Serialize = true;
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
            Serialize = serialize;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines if the property or field this <see cref="SerializeSettingAttribute"/>
        /// modifies should be serialized to the configuration file.
        /// </summary>
        public bool Serialize { get; }

        #endregion
    }
}