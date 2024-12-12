//******************************************************************************************************
//  Common.cs - Gbtc
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

using System.Collections.Generic;
using System.Data;

namespace GrafanaAdapters.Functions;

internal static class Common
{
    public const GroupOperations DefaultGroupOperations = GroupOperations.None | GroupOperations.Slice | GroupOperations.Set;

    // Default slice tolerance parameter
    public static readonly ParameterDefinition<double> DefaultSliceTolerance = new()
    {
        Name = "sliceTolerance",
        Default = 1.0D,
        Description = "A floating-point value that must be greater than or equal to 0.001 that represents the desired time tolerance, in seconds, for the time slice.",
        Required = true
    };

    public static List<IParameter> InsertRequiredSliceParameter(this List<IParameter> parameters)
    {
        parameters.Insert(0, DefaultSliceTolerance);
        return parameters;
    }

    /// <summary>
    /// Parses a string as a positive (> 0) integer or a percentage value of total.
    /// </summary>
    /// <param name="parameterName">Parameter name for error reporting.</param>
    /// <param name="value">Value to parse as a specific total or a percentage of total.</param>
    /// <param name="total">Total value to use for percentage calculations.</param>
    /// <returns>Integer total value as parsed from <paramref name="value"/>.</returns>
    public static int ParseTotal(string parameterName, string value, int total)
    {
        int count;

        if (total == 0)
            return 0;

        value = value.Trim();

        if (value.EndsWith("%"))
        {
            try
            {
                double percent = ParsePercentage(parameterName, value, false);
                count = (int)(total * (percent / 100.0D));

                if (count == 0)
                    count = 1;
            }
            catch
            {
                throw new SyntaxErrorException($"Could not parse parameter '{parameterName}' total value '{value}' as a floating-point number or percentage is outside range of greater than 0 and less than or equal to 100.");
            }
        }
        else
        {
            if (value.Contains(".") && double.TryParse(value, out double result))
            {
                // Treat fractional numbers as a percentage of length
                if (result is > 0.0D and < 1.0D)
                    count = (int)(total * result);
                else
                    count = (int)result;
            }
            else
            {
                int.TryParse(value, out count);
            }
        }

        if (count < 1)
            throw new SyntaxErrorException($"Parameter '{parameterName}' total value '{count}' is less than one.");

        return count;
    }

    /// <summary>
    /// Parses a string as a percentage value. If value is suffixed with '%' it is treated as a percentage expected to be in
    /// the range of 0 to 100, otherwise as a floating-point number expected to be in the range of 0 to 1. Inclusivity for
    /// extremes is controlled by <paramref name="include0"/> and <paramref name="include100"/>. Return value is always in
    /// the range of 0 to 100.
    /// </summary>
    /// <param name="parameterName">Parameter name for error reporting.</param>
    /// <param name="value">Value to parse as a percentage.</param>
    /// <param name="include0">Boolean flag to include 0 in range.</param>
    /// <param name="include100">Boolean flag to include 100 in range.</param>
    /// <returns>Parsed percentage value in the range of 0 to 100.</returns>
    public static double ParsePercentage(string parameterName, string value, bool include0 = true, bool include100 = true)
    {
        double multiplier = 1.0D;
        value = value.Trim();

        if (value.EndsWith("%"))
            value = value.Substring(0, value.Length - 1);
        else
            multiplier = 100.0D;

        if (!double.TryParse(value, out double percent))
            throw new SyntaxErrorException($"Could not parse parameter '{parameterName}' value '{value}' as a floating-point number.");

        percent *= multiplier;

        if (include100)
        {
            if (include0)
            {
                if (percent is < 0.0D or > 100.0D)
                    throw new SyntaxErrorException($"Parameter '{parameterName}' percentage '{value}' is outside range of 0 to 100, inclusive.");
            }
            else
            {
                if (percent is <= 0.0D or > 100.0D)
                    throw new SyntaxErrorException($"Parameter '{parameterName}' percentage '{value}' is outside range of greater than 0 and less than or equal to 100.");
            }
        }
        else
        {
            if (include0)
            {
                if (percent is < 0.0D or >= 100.0D)
                    throw new SyntaxErrorException($"Parameter '{parameterName}' percentage '{value}' is outside range of greater than or equal to 0 and less than 100.");
            }
            else
            {
                if (percent is <= 0.0D or >= 100.0D)
                    throw new SyntaxErrorException($"Parameter '{parameterName}' percentage '{value}' is outside range of 0 to 100, exclusive.");
            }
        }

        return percent;
    }
}