//******************************************************************************************************
//  VoltageLevel.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
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
//  12/28/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF.Units.EE;

// Attribute auto-generates "VoltageLevel" enum - see GSF.CodeGenerator.VoltageLevelEnumGenerator for details
[AttributeUsage(AttributeTargets.Class)]
internal sealed class GenerateVoltageLevelsAttribute(string enumName, params int[] voltageLevels) : Attribute
{
    public string EnumName { get; } = enumName;

    public int[] VoltageLevels { get; } = voltageLevels;
}

/// <summary>
/// Defines common transmission voltage levels.
/// </summary>
[GenerateVoltageLevels("VoltageLevel", 34, 44, 69, 115, 138, 161, 169, 230, 345, 500, 765, 1100)]
public static class CommonVoltageLevels
{
    /// <summary>
    /// Gets common transmission voltage level values.
    /// </summary>
    public static readonly string[] Values;

    static CommonVoltageLevels()
    {
        Values = VoltageLevelExtensions.VoltageLevelMap.Values
            .OrderByDescending(voltage => voltage)
            .Select(voltage => voltage.ToString()).ToArray();
    }
}

/// <summary>
/// Defines extension functions related to <see cref="VoltageLevel"/> enumeration.
/// </summary>
public static class VoltageLevelExtensions
{
    internal static readonly Dictionary<VoltageLevel, int> VoltageLevelMap;

    static VoltageLevelExtensions()
    {
        VoltageLevelMap = new Dictionary<VoltageLevel, int>();

        foreach (VoltageLevel value in Enum.GetValues(typeof(VoltageLevel)))
        {
            if (int.TryParse(value.GetDescription(), out int level))
                VoltageLevelMap[value] = level;
        }
    }

    /// <summary>
    /// Gets the voltage level for the specified <see cref="VoltageLevel"/> enum value.
    /// </summary>
    /// <param name="level">Target <see cref="VoltageLevel"/> enum value.</param>
    /// <returns>Voltage level for the specified <paramref name="level"/>.</returns>
    public static int Value(this VoltageLevel level)
    {
        return VoltageLevelMap.TryGetValue(level, out int value) ? value : 0;
    }

    /// <summary>
    /// Attempts to get the <see cref="VoltageLevel"/> enum value for the source kV <paramref name="value"/>.
    /// </summary>
    /// <param name="value">kV value to attempt to find.</param>
    /// <param name="level">Mapped <see cref="VoltageLevel"/> enum value, if found.</param>
    /// <returns>
    /// <c>true</c> if matching <see cref="VoltageLevel"/> enum value is found for specified kV
    /// <paramref name="value"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetVoltageLevel(this int value, out VoltageLevel level)
    {
        foreach (KeyValuePair<VoltageLevel, int> kvp in VoltageLevelMap)
        {
            if (kvp.Value != value)
                continue;

            level = kvp.Key;
            return true;
        }

        level = default;
        return false;
    }
}