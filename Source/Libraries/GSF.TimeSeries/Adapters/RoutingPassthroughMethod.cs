//******************************************************************************************************
//  RoutingPassthroughMethod.cs - Gbtc
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
//  11/15/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// A set of methods that can be called to route measurements to an adapter that implements <see cref="IOptimizedRoutingConsumer"/>
    /// Note, this method will be called within a synchronized context.
    /// </summary>
    public class RoutingPassthroughMethod
    {
        /// <summary>
        /// Measurements can be directly passed through to this method without the need for routing/filtering.
        /// </summary>
        public readonly Action<List<IMeasurement>> ProcessMeasurementList;

        /// <summary>
        /// Creates <see cref="RoutingPassthroughMethod"/>.
        /// </summary>
        /// <param name="processMeasurementList"></param>
        public RoutingPassthroughMethod(Action<List<IMeasurement>> processMeasurementList) => 
            ProcessMeasurementList = processMeasurementList ?? CallNothing;

        private void CallNothing(List<IMeasurement> measurement)
        {
        }
    }
}