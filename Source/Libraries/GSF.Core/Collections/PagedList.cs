//******************************************************************************************************
//  PagedList.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/17/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;

// ReSharper disable PossibleMultipleEnumeration
namespace GSF.Collections
{
    /// <summary>
    /// Defines a paged list for an enumeration.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="IEnumerable{T}"/> to paginate.</typeparam>
    /// <remarks>
    /// This class returns the elements for the specified page number for a given page size and
    /// also provides the calculated page count based on input parameters.
    /// </remarks>
    public class PagedList<T> : List<T>, IPagedList
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PagedList{T}"/>.
        /// </summary>
        /// <param name="source">Source enumeration to paginate.</param>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="count">Total records in source if known.</param>
        /// <remarks>
        /// If count is known or can be calculated early, specify the value in the
        /// <paramref name="count"/> parameter as an optimization to prevent a full
        /// enumeration on <paramref name="source"/> to get a count.
        /// </remarks>
        public PagedList(IEnumerable<T> source, int page, int pageSize, int count = -1)
        {
            // For many enumeration sources, Count() will map to Length or Count
            TotalCount = count > -1 ? count : source.Count(); 
            PageCount = CalculatePageCount(pageSize, TotalCount);
            Page = page < 1 ? 0 : page - 1;
            PageSize = pageSize;

            AddRange(source.Skip(Page * PageSize).Take(PageSize));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets total count of elements in enumeration.
        /// </summary>
        public int TotalCount
        {
            get;
        }

        /// <summary>
        /// Gets calculated page count based on page size and total items count.
        /// </summary>
        public int PageCount
        {
            get;
        }

        /// <summary>
        /// Gets current page number.
        /// </summary>
        public int Page
        {
            get;
        }

        /// <summary>
        /// Gets current page size.
        /// </summary>
        public int PageSize
        {
            get;
        }

        #endregion

        #region [ Methods ]

        // Calculate page count based on page size and total item count
        private int CalculatePageCount(int pageSize, int totalCount)
        {
            if (pageSize == 0)
                return 0;

            int remainder = totalCount % pageSize;
            return (totalCount / pageSize) + (remainder == 0 ? 0 : 1);
        }

        #endregion
    }
}
