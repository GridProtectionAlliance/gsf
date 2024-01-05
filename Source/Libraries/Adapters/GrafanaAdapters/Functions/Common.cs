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

using System;
using System.Collections.Generic;
using System.Linq;

namespace GrafanaAdapters.Functions;

internal static class Common
{
    public const GroupOperations DefaultGroupOperations = GroupOperations.Standard | GroupOperations.Slice | GroupOperations.Set;

    // This generates a standard data source values parameter definition - this is always the last parameter
    public static ParameterDefinition<IEnumerable<T>> DataSourceValuesParameterDefinition<T>()
    {
        return new ParameterDefinition<IEnumerable<T>>()
        {
            Name = "expression",
            Default = Array.Empty<T>(),
            Description = "Input Data Points",
            Required = true
        };
    }

    public static void InsertRequiredSliceParameter(this List<IParameter> parameters)
    {
        parameters.Insert(0, new ParameterDefinition<double>()
        {
            Default = 0.0333,
            Description = "A floating-point value that must be greater than or equal to zero that represents the desired time tolerance, in seconds, for the time slice",
            Required = true
        });
    }

    // Gets type for specified data source type name, assuming local namespace if needed.
    public static Type GetLocalType(string typeName)
    {
        if (typeName is null)
            throw new ArgumentNullException(nameof(typeName));

        if (!typeName.Contains('.'))
            typeName = $"{nameof(GrafanaAdapters)}.{typeName}";

        return Type.GetType(typeName);
    }

    // Parses a sting as positive (> 0) integer or a percentage value of total
    public static int ParseTotal(string parameter, int total)
    {
        int count;

        if (total == 0)
            return 0;

        parameter = parameter.Trim();

        if (parameter.EndsWith("%"))
        {
            try
            {
                double percent = ParsePercentage(parameter, false);
                count = (int)(total * (percent / 100.0D));

                if (count == 0)
                    count = 1;
            }
            catch
            {
                throw new ArgumentOutOfRangeException($"Could not parse '{parameter}' as a floating-point value or percentage is outside range of greater than 0 and less than or equal to 100.");
            }
        }
        else
        {
            if (parameter.Contains(".") && double.TryParse(parameter, out double result))
            {
                // Treat fractional numbers as a percentage of length
                if (result is > 0.0D and < 1.0D)
                    count = (int)(total * result);
                else
                    count = (int)result;
            }
            else
            {
                int.TryParse(parameter, out count);
            }
        }

        if (count < 1)
            throw new ArgumentOutOfRangeException($"Count '{count}' is less than one.");

        return count;
    }

    public static double ParsePercentage(string parameter, bool includeZero = true)
    {
        parameter = parameter.Trim();

        if (parameter.EndsWith("%"))
            parameter = parameter.Substring(0, parameter.Length - 1);

        if (!double.TryParse(parameter, out double percent))
            throw new FormatException($"Could not parse '{parameter}' as a floating-point value.");

        if (includeZero)
        {
            if (percent is < 0.0D or > 100.0D)
                throw new ArgumentOutOfRangeException($"Percentage '{parameter}' is outside range of 0 to 100, inclusive.");
        }
        else
        {
            if (percent is <= 0.0D or > 100.0D)
                throw new ArgumentOutOfRangeException($"Percentage '{parameter}' is outside range of greater than 0 and less than or equal to 100.");
        }

        return percent;
    }
}