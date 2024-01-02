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

    public static int ParseInt(string parameter, bool includeZero = true)
    {
        parameter = parameter.Trim();

        if (!int.TryParse(parameter, out int value))
            throw new FormatException($"Could not parse '{parameter}' as an integer value.");

        if (includeZero)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than zero.");
        }
        else
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than or equal to zero.");
        }

        return value;
    }

    public static double ParseFloat(string parameter, bool validateGTEZero = true)
    {
        parameter = parameter.Trim();

        if (!double.TryParse(parameter, out double value))
            throw new FormatException($"Could not parse '{parameter}' as a floating-point value.");

        if (validateGTEZero && value < 0.0D)
            throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than zero.");

        return value;
    }
}