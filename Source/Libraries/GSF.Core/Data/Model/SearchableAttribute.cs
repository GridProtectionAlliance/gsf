//******************************************************************************************************
//  SearchableAttribute.cs - Gbtc
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
//  08/20/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines an attribute that will mark a property as a searchable field for a modeled table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SearchableAttribute : Attribute
    {
        /// <summary>
        /// Gets flag that determines if search field should be queried with a SQL LIKE expression.
        /// </summary>
        /// <remarks>
        /// If flag is <c>true</c> a SQL LIKE expression will be used to search field; otherwise, field must match exactly.
        /// </remarks>
        public bool? UseLikeExpression
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="SearchableAttribute"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="UseLikeExpression"/> is assumed to be <c>true</c> for string fields.
        /// </remarks>
        public SearchableAttribute()
        {
        }

        /// <summary>
        /// Creates a new <see cref="SearchableAttribute"/> with specified use SQL LIKE expression state.
        /// </summary>
        /// <param name="useLikeExpression">Flag that determines if this searchable field should be queried with a SQL LIKE expression.</param>
        public SearchableAttribute(bool useLikeExpression)
        {
            UseLikeExpression = useLikeExpression;
        }
    }
}