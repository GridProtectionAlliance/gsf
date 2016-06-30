//******************************************************************************************************
//  IRouteMappingTables.cs - Gbtc
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

using System;
using System.Collections.Generic;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// An interface to provide a custom implementation of the routing functionality of the <see cref="RoutingTables"/>
    /// </summary>
    public interface IRouteMappingTables
    {
        /// <summary>
        /// Gets the number of routes in this routing table.
        /// </summary>
        int RouteCount { get; }

        /// <summary>
        /// Calculates new routes for the supplied list of producers and consumers.
        /// This new mapping table will replace the existing one when complete.
        /// </summary>
        /// <param name="previousMapping">the most recent mapping table that will get replaced by this one.</param>
        /// <param name="producerAdapters">all of the producers</param>
        /// <param name="consumerAdapters">all of the consumers</param>
        /// <returns>The new mapping table that will replace the old one.</returns>
        IRouteMappingTables CalculateNewRoutingTable(IRouteMappingTables previousMapping, RoutingTablesAdaptersList producerAdapters, RoutingTablesAdaptersList consumerAdapters);
    }
}