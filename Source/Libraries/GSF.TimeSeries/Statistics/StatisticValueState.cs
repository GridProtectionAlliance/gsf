//******************************************************************************************************
//  StatisticValueState.cs - Gbtc
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

namespace GSF.TimeSeries.Statistics
{
    /// <summary>
    /// Represents an object that can track the current and previous
    /// state of a statistic and get the difference between them.
    /// </summary>
    public class StatisticValueState : ObjectState<double>
    {
        /// <summary>
        /// Creates a new <see cref="StatisticValueState"/>.
        /// </summary>
        /// <param name="name">Name of statistic.</param>
        public StatisticValueState(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the statistical difference between current and previous statistic value.
        /// </summary>
        /// <returns>Difference from last cached statistic value.</returns>
        public double GetDifference()
        {
            if (CurrentState <= 0.0D)
                return 0.0D;

            double value = CurrentState - PreviousState;

            // If value is negative, statistics may have been reset by user
            if (value < 0.0D)
                value = CurrentState;

            // Track last value
            PreviousState = CurrentState;

            return value;

        }
    }
}
