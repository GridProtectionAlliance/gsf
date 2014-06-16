//******************************************************************************************************
//  TimeSpanExtensions.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/14/2008 - F. Russell Robertson
//       Generated original version of source code.
//  05/30/2008 - J. Ritchie Carroll
//       Updated to use existing elapsed time string function of GSF.Units.Time.
//
//*******************************************************************************************************

using System;
using GSF.Units;

namespace GSF
{
    /// <summary>
    /// Extends the TimeSpan Class
    /// </summary>
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Converts the <see cref="TimeSpan"/> value into a textual representation of years, days, hours,
        /// minutes and seconds with the specified number of fractional digits.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> to process.</param>
        /// <param name="secondPrecision">Number of fractional digits to display for seconds. Defaults to 2.</param>
        /// <param name="minimumSubSecondResolution">Minimum sub-second resolution to display. Defaults to <see cref="SI.Milli"/>.</param>
        /// <remarks>Set second precision to -1 to suppress seconds display.</remarks>
        /// <returns>
        /// The string representation of the value of this <see cref="TimeSpan"/>, consisting of the number of
        /// years, days, hours, minutes and seconds represented by this value.
        /// </returns>
        /// <example>
        ///   DateTime g_start = DateTime.UtcNow;
        ///   DateTime EndTime = DateTime.UtcNow;
        ///   TimeSpan duration = EndTime.Subtract(g_start);
        ///   Console.WriteLine("Elapsed Time = " + duration.ToElapsedTimeString());
        /// </example>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="minimumSubSecondResolution"/> is not less than or equal to <see cref="SI.Milli"/> or
        /// <paramref name="minimumSubSecondResolution"/> is not defined in <see cref="SI.Factors"/> array.
        /// </exception>
        public static string ToElapsedTimeString(this TimeSpan value, int secondPrecision = 2, double minimumSubSecondResolution = SI.Milli)
        {
            return Time.ToElapsedTimeString(value.TotalSeconds, secondPrecision, null, minimumSubSecondResolution);
        }
    }
}