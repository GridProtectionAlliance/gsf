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
    /// Represents types of searches available for <see cref="SearchableAttribute"/>.
    /// </summary>
    public enum SearchType
    {
        /// <summary>
        /// Use default search type for field type.
        /// </summary>
        Default,
        /// <summary>
        /// Use SQL LIKE expression to match field.
        /// </summary>
        LikeExpression,
        /// <summary>
        /// Match full text of field.
        /// </summary>
        FullValueMatch,
    }

    /// <summary>
    /// Defines an attribute that will mark a property as a searchable field for a modeled table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SearchableAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets type of search that should be used for field queried.
        /// </summary>
        /// <remarks>
        /// Database query functions executed against modeled fields marked with both <see cref="SearchableAttribute"/> and
        /// <see cref="EncryptDataAttribute"/> will only be searched using <see cref="Model.SearchType.FullValueMatch"/>,
        /// regardless of specified value as encryption is handled locally by <see cref="TableOperations{T}"/>. However, the
        /// <see cref="TableOperations{T}.SearchRecords"/> function can be used to find data in encrypted fields that are
        /// marked for search with a <see cref="Model.SearchType.LikeExpression"/>.
        /// </remarks>
        public SearchType SearchType
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="SearchableAttribute"/>.
        /// </summary>
        public SearchableAttribute()
        {
            SearchType = SearchType.Default;
        }

        /// <summary>
        /// Creates a new <see cref="SearchableAttribute"/> with specified <paramref name="searchType"/>.
        /// </summary>
        /// <param name="searchType">Type of search that should be used for field queried.</param>
        /// <remarks>
        /// Database query functions executed against modeled fields marked with both <see cref="SearchableAttribute"/> and
        /// <see cref="EncryptDataAttribute"/> will only be searched using <see cref="Model.SearchType.FullValueMatch"/>,
        /// regardless of specified <paramref name="searchType"/> as encryption is handled locally by instances of the
        /// <see cref="TableOperations{T}"/>. However, the <see cref="TableOperations{T}.SearchRecords"/> function can be used
        /// to find data in encrypted fields that are marked for search with a <see cref="Model.SearchType.LikeExpression"/>.
        /// </remarks>
        public SearchableAttribute(SearchType searchType)
        {
            SearchType = searchType;
        }
    }
}