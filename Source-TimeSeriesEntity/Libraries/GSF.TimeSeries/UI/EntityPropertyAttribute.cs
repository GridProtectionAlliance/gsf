//******************************************************************************************************
//  EntityPropertyAttribute.cs - Gbtc
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
//  04/08/2011 - Ritchie
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Represents an attribute that determines if a property in a class derived from
    /// <see cref="DataModelBase"/> should be included as a field in the data model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityPropertyAttribute : Attribute
    {
        #region [ Members ]

        // Fields
        private readonly bool m_include;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="EntityPropertyAttribute"/>; defaults to <c><see cref="Include"/> = true</c>.
        /// </summary>
        public EntityPropertyAttribute()
        {
            m_include = true;
        }

        /// <summary>
        /// Creates a new <see cref="EntityPropertyAttribute"/> with the specified <paramref name="include"/> value.
        /// </summary>
        /// <param name="include">
        /// Assigns flag that determines if the property this <see cref="EntityPropertyAttribute"/>
        /// modifies should be included as a field in the data model.
        /// </param>
        public EntityPropertyAttribute(bool include)
        {
            m_include = include;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines if the property this <see cref="EntityPropertyAttribute"/>
        /// modifies should be included as a field in the data model.
        /// </summary>
        public bool Include
        {
            get
            {
                return m_include;
            }
        }

        #endregion
    }
}