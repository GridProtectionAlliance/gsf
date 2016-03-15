//******************************************************************************************************
//  PrimaryKeyAttribute.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/30/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines an attribute that will mark a property as a primary key field for a modeled table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// Gets flag that determines if primary key field is an identity field.
        /// </summary>
        public bool IsIdentity
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="PrimaryKeyAttribute"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="IsIdentity"/> is assumed to be <c>false</c>.
        /// </remarks>
        public PrimaryKeyAttribute()
        {
            IsIdentity = false;
        }

        /// <summary>
        /// Creates a new <see cref="PrimaryKeyAttribute"/> with specified identity state.
        /// </summary>
        /// <param name="isIdentity">Flag that determines if this primary key field is an identity field.</param>
        public PrimaryKeyAttribute(bool isIdentity)
        {
            IsIdentity = isIdentity;
        }
    }
}