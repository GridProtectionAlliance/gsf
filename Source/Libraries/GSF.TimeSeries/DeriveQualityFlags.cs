//******************************************************************************************************
//  DeriveQualityFlags.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  02/27/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents functions to derive <see cref="MeasurementStateFlags"/> related to value and time quality.
    /// </summary>
    public static class DeriveQualityFlags
    {
        /// <summary>
        /// Gets derived quality flags from a set of value and time quality vectors.
        /// </summary>
        /// <param name="valueQualities">Boolean vector where flag determines if value quality is good.</param>
        /// <param name="timeQualities">Boolean vector where flag determines if time quality is good.</param>
        /// <returns>Derived quality flags.</returns>
        public static MeasurementStateFlags From(IEnumerable<bool> valueQualities, IEnumerable<bool> timeQualities)
        {
            MeasurementStateFlags derivedQuality = MeasurementStateFlags.Normal;

            bool qualityIsBad(bool qualityIsGood) => !qualityIsGood;

            if (valueQualities.Any(qualityIsBad))
                derivedQuality |= MeasurementStateFlags.BadData;

            if (timeQualities.Any(qualityIsBad))
                derivedQuality |= MeasurementStateFlags.BadTime;

            return derivedQuality;
        }

        /// <summary>
        /// Gets derived quality flags from specified value and time quality.
        /// </summary>
        /// <param name="valueQualityIsGood">Flag that determines if value quality is good.</param>
        /// <param name="timeQualityIsGood">Flag that determines if time quality is good.</param>
        /// <returns>Derived quality flags.</returns>
        public static MeasurementStateFlags From(bool valueQualityIsGood, bool timeQualityIsGood)
        {
            MeasurementStateFlags derivedQuality = MeasurementStateFlags.Normal;

            if (!valueQualityIsGood)
                derivedQuality |= MeasurementStateFlags.BadData;

            if (!timeQualityIsGood)
                derivedQuality |= MeasurementStateFlags.BadTime;

            return derivedQuality;
        }
    }
}