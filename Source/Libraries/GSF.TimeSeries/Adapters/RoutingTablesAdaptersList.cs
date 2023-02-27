//******************************************************************************************************
//  RoutingTablesAdaptersList.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  06/29/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Indicates how the <see cref="RoutingTables"/> have changed from one route calculation to another.
    /// </summary>
    public class RoutingTablesAdaptersList
    {
        /// <summary>
        /// A list of all <see cref="IAdapter"/> that did NOT existed in the old route calculation, but do exist in the new route calculation. 
        /// </summary>
        public readonly List<IAdapter> NewAdapter;
        
        /// <summary>
        /// A list of all <see cref="IAdapter"/> that existed in the old route calculation, and also exist in the new route calculation. 
        /// </summary>
        public readonly List<IAdapter> ExistingAdapter;

        /// <summary>
        /// A list of all <see cref="IAdapter"/> that existed in the old route calculation, but do NOT exist in the new route calculation. 
        /// </summary>
        public readonly List<IAdapter> OldAdapter;

        /// <summary>
        /// A union of <see cref="NewAdapter"/> with <see cref="ExistingAdapter"/>.
        /// </summary>
        public readonly List<IAdapter> NewAndExistingAdapters;

        /// <summary>
        /// Creates a <see cref="RoutingTablesAdaptersList"/>
        /// </summary>
        /// <param name="previousAdapterList">A complete list of all the adapters that existed before.</param>
        /// <param name="currentAdapterList">A complete list of all the adapters that exist now</param>
        public RoutingTablesAdaptersList(HashSet<IAdapter> previousAdapterList, HashSet<IAdapter> currentAdapterList)
        {
            NewAdapter = new List<IAdapter>();
            ExistingAdapter = new List<IAdapter>();
            OldAdapter = new List<IAdapter>();
            NewAndExistingAdapters = new List<IAdapter>();

            NewAndExistingAdapters.AddRange(currentAdapterList);
            NewAdapter.AddRange(currentAdapterList.Where(adapter => !previousAdapterList.Contains(adapter)));
            OldAdapter.AddRange(previousAdapterList.Where(adapter => !currentAdapterList.Contains(adapter)));
            ExistingAdapter.AddRange(currentAdapterList.Where(previousAdapterList.Contains));
        }
    }
}