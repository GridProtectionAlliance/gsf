//******************************************************************************************************
//  IOptimizedRoutingConsumer.cs - Gbtc
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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Provides an alternative means of routing messages one at a time if an adapter
    /// is more efficient at receiving messages.
    /// </summary>
    public interface IOptimizedRoutingConsumer
    {
        /// <summary>
        /// Gets the alternative routing method callbacks for this adapter.
        /// </summary>
        /// <returns>The methods if the adapter supports them. Null if this method is not to be used.</returns>
        RoutingPassthroughMethod GetRoutingPassthroughMethods();
    }
}
