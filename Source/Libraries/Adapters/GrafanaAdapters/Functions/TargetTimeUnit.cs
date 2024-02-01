//******************************************************************************************************
//  TargetTimeUnit.cs - Gbtc
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
using System;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Represents a time unit that can be targeted in OpenHistorian Grafana functions.
/// </summary>
/// <remarks>
/// This class is designed to handle various forms of time units and provides
/// a way to parse time units from strings.
/// </remarks>
public class TargetTimeUnit
{
    /// <summary>
    /// Gets or sets the base time unit.
    /// </summary>
    public TimeUnit Unit;

    /// <summary>
    /// Gets or sets the factor by which to scale the base time unit.
    /// </summary>
    public double Factor = double.NaN;

    /// <inheritdoc />
    public override string ToString()
    {
        if (Unit != TimeUnit.Seconds)
            return $"{Unit}";

        return Factor switch
        {
            double.NaN => "Seconds",
            SI.Milli => "Milliseconds",
            SI.Micro => "Microseconds",
            SI.Nano => "Nanoseconds",
            _ => $"{Unit}"
        };
    }

    /// <summary>
    /// Tries to parse a string representation of a time unit to a <see cref="TargetTimeUnit"/>.
    /// </summary>
    /// <param name="value">The string representation of the time unit to parse.</param>
    /// <returns>
    /// Tuple containing the <see cref="TargetTimeUnit"/> and a flag indicating if the parse was successful.
    /// </returns>
    /// <remarks>
    /// If this method succeeds, return value contains the <see cref="TargetTimeUnit"/> equivalent
    /// of the time unit contained in <paramref name="value"/>; otherwise, <c>null</c> if the
    /// conversion failed. The conversion fails if the <paramref name="value"/> is null or is not
    /// in the correct format.
    /// </remarks>
    public static (TargetTimeUnit, bool) Parse(string value)
    {
        if (Enum.TryParse(value, true, out TimeUnit units))
            return (new TargetTimeUnit { Unit = units }, true);

        return value?.ToLowerInvariant() switch
        {
            "milliseconds" => (new TargetTimeUnit { Unit = TimeUnit.Seconds, Factor = SI.Milli }, true),
            "microseconds" => (new TargetTimeUnit { Unit = TimeUnit.Seconds, Factor = SI.Micro }, true),
            "nanoseconds" => (new TargetTimeUnit { Unit = TimeUnit.Seconds, Factor = SI.Nano }, true),
            _ => (null, false)
        };
    }

    /// <summary>
    /// Scales the specified time, in the specified units, to a time in seconds.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <param name="units">The target time unit, which includes scaling factor, for the conversion.</param>
    /// <returns>A time, in seconds, that represents the converted value.</returns>
    public static double FromTimeUnits(double value, TargetTimeUnit units)
    {
        double time = Time.ConvertFrom(value, units.Unit);

        if (!double.IsNaN(units.Factor))
            time *= units.Factor;

        return time;
    }

    /// <summary>
    /// Scales a time, in seconds, to the specified units.
    /// </summary>
    /// <param name="value">The time, in seconds, to convert.</param>
    /// <param name="units">The target time units, which includes scaling factor, for the conversion.</param>
    /// <returns>A double value that represents the converted and scaled time, in the specified units.</returns>
    public static double ToTimeUnits(double value, TargetTimeUnit units)
    {
        double time = new Time(value).ConvertTo(units.Unit);

        if (!double.IsNaN(units.Factor))
            time /= units.Factor;

        return time;
    }
}