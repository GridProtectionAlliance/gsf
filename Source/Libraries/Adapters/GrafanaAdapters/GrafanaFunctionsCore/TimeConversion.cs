//******************************************************************************************************
//  TimeConversion.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  08/23/2023 - Timothy Liakh
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.Units;

namespace GrafanaAdapters.GrafanaFunctionsCore;

// Static functions for time unit conversion.
internal class TimeConversion
{
    /// <summary>
    /// Converts a double value from a specified time unit and scaling factor to a <see cref="Time"/> object.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <param name="target">The target time unit and scaling factor for the conversion.</param>
    /// <returns>A <see cref="Time"/> object that represents the converted value.</returns>
    public static Time FromTimeUnits(double value, TargetTimeUnit target)
    {
        double time = Time.ConvertFrom(value, target.Unit);

        if (!double.IsNaN(target.Factor))
            time *= target.Factor;

        return time;
    }

    /// <summary>
    /// Converts a <see cref="Time"/> object to a double value, scaled by a specified time unit and scaling factor.
    /// </summary>
    /// <param name="value">The <see cref="Time"/> object to convert.</param>
    /// <param name="target">The target time unit and scaling factor for the conversion.</param>
    /// <returns>A double value that represents the converted and scaled time.</returns>
    public static double ToTimeUnits(Time value, TargetTimeUnit target)
    {
        double time = value.ConvertTo(target.Unit);

        if (!double.IsNaN(target.Factor))
            time /= target.Factor;

        return time;
    }
}