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

using System;
using GSF.Units;

namespace GrafanaAdapters.FunctionParsing;

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

    /// <summary>
    /// Tries to parse a string representation of a time unit to a <see cref="TargetTimeUnit"/>.
    /// </summary>
    /// <param name="value">The string representation of the time unit to parse.</param>
    /// <param name="targetTimeUnit">
    /// When this method returns, contains the <see cref="TargetTimeUnit"/> equivalent
    /// of the time unit contained in <paramref name="value"/>, if the conversion succeeded,
    /// or null if the conversion failed. The conversion fails if the <paramref name="value"/>
    /// is null or is not of the correct format. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <c>true</c> if <paramref name="value"/> was converted successfully; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryParse(string value, out TargetTimeUnit targetTimeUnit)
    {
        if (Enum.TryParse(value, true, out TimeUnit timeUnit))
        {
            targetTimeUnit = new TargetTimeUnit { Unit = timeUnit };
            return true;
        }

        switch (value?.ToLowerInvariant())
        {
            case "milliseconds":
                targetTimeUnit = new TargetTimeUnit { Unit = TimeUnit.Seconds, Factor = SI.Milli };
                return true;
            case "microseconds":
                targetTimeUnit = new TargetTimeUnit { Unit = TimeUnit.Seconds, Factor = SI.Micro };
                return true;
            case "nanoseconds":
                targetTimeUnit = new TargetTimeUnit { Unit = TimeUnit.Seconds, Factor = SI.Nano };
                return true;
        }

        targetTimeUnit = null;
        return false;
    }
}