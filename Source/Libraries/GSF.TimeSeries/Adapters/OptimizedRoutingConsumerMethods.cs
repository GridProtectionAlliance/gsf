//******************************************************************************************************
//  OptimizedRoutingConsumerMethods.cs - Gbtc
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

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// A set of methods that can be called to route measurements to an adapter that implements <see cref="IOptimizedRoutingConsumer"/>
    /// </summary>
    public class OptimizedRoutingConsumerMethods
    {
        /// <summary>
        /// Object to lock before calling any of the provided methods. 
        /// </summary> 
        public readonly object LockObject;
        /// <summary>
        /// Method to call before any measurements are passed to <see cref="ProcessMeasurement"/>
        /// </summary>
        public readonly Action BeginRouting;
        /// <summary>
        /// Where measurements are added when they are routed.
        /// </summary>
        public readonly Action<IMeasurement> ProcessMeasurement;
        /// <summary>
        /// Method to call after this round of routing has occurred.
        /// </summary>
        public readonly Action EndRouting;

        /// <summary>
        /// Creates a OptimizedRoutingConsumerMethods
        /// </summary>
        /// <param name="lockObject"></param>
        /// <param name="beginRouting"></param>
        /// <param name="processMeasurement"></param>
        /// <param name="endRouting"></param>
        public OptimizedRoutingConsumerMethods(object lockObject, Action beginRouting, Action<IMeasurement> processMeasurement, Action endRouting)
        {
            LockObject = lockObject ?? this;
            BeginRouting = beginRouting ?? CallNothing;
            ProcessMeasurement = processMeasurement ?? CallNothing;
            EndRouting = endRouting ?? CallNothing;
        }

        void CallNothing()
        {

        }

        void CallNothing(IMeasurement measurement)
        {

        }
    }
}