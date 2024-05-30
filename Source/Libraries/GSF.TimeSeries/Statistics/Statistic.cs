//******************************************************************************************************
//  Statistic.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  02/22/2012 - Stephen C. Wills
//       Migrated from GSF.PhasorProtocols.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries.Statistics
{
    /// <summary>
    /// Method signature for function used to calculate a statistic for a given object.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="arguments">Any needed arguments for statistic calculation.</param>
    /// <returns>Actual calculated statistic.</returns>
    public delegate double StatisticCalculationFunction(object source, string arguments);
    
    /// <summary>
    /// Represents a statistic calculation.
    /// </summary>
    public class Statistic
    {
        /// <summary>
        /// Gets the key of the measurement associated with the statistic.
        /// </summary>
        public MeasurementKey Key { get; internal set; } = MeasurementKey.Undefined;

        /// <summary>
        /// The method to be called to calculate the statistic.
        /// </summary>
        public StatisticCalculationFunction Method { get; set; }

        /// <summary>
        /// The name of the source of the statistic.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The index of the signal associated with the statistic.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The arguments to be passed into the statistic calculation function.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Target data type of the statistic.
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// Gets the last calculated value of the statistic.
        /// </summary>
        public double Value { get; internal set; } = double.NaN;
    }
}
