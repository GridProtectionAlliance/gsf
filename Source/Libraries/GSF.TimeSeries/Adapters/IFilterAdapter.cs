//******************************************************************************************************
//  IFilterAdapter.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  11/10/2017 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents a module that processes input measurements on
    /// the way by before routing the measurements to other adapters.
    /// </summary>
    public interface IFilterAdapter : IAdapter
    {
        /// <summary>
        /// Gets or sets the values that determines the order in which filter adapters are executed.
        /// </summary>
        int ExecutionOrder { get; set; }

        /// <summary>
        /// Handler for new measurements that have not yet been routed.
        /// </summary>
        /// <param name="measurements">Measurements that have not yet been routed.</param>
        void HandleNewMeasurements(ICollection<IMeasurement> measurements);
    }
}
