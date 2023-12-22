//******************************************************************************************************
//  FunctionModelHelpers.cs - Gbtc
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
// ReSharper disable AccessToModifiedClosure
// ReSharper disable PossibleMultipleEnumeration

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GrafanaAdapters.GrafanaFunctionsCore;

internal static class FunctionModelHelpers
{
    private const string ExpressionFormat = @"^(?<GroupOp>Slice|Set)?{0}\s*\(\s*(?<Expression>.+)\s*\)";

    public const FunctionOperations DefaultFunctionOperations = FunctionOperations.Standard | FunctionOperations.Slice | FunctionOperations.Set;

    public static Regex GenerateFunctionRegex(params string[] functionNames)
    {
        if (functionNames.Length == 0)
            throw new ArgumentException("At least one function name must be specified.", nameof(functionNames));

        string allAliases = functionNames.Length == 1 ? functionNames[0] : $"({string.Join("|", functionNames)})";
        return new Regex(string.Format(ExpressionFormat, allAliases), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    public static readonly Parameter<IDataSourceValueGroup> InputDataPointValues = new()
    {
        Default = new DataSourceValueGroup<DataSourceValue>(),
        Description = "Input Data Points",
        Required = true
    };

    public static void InsertRequiredSliceParameter(this List<IParameter> parameters)
    {
        parameters.Insert(0, new Parameter<double>
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

    public static double ParseFloat(string parameter, IEnumerable<DataSourceValue> source = null, bool validateGTEZero = true, bool isSliceOperation = false)
    {
        double value;

        parameter = parameter.Trim();

        Tuple<bool, double> cache = TargetCache<Tuple<bool, double>>.GetOrAdd(parameter, () =>
        {
            bool success = double.TryParse(parameter, out double result);
            return new Tuple<bool, double>(success, result);
        });

        if (cache.Item1)
        {
            value = cache.Item2;
        }
        else
        {
            if (source is null)
                throw new FormatException($"Could not parse '{parameter}' as a floating-point value.");

            double defaultValue = 0.0D;
            bool hasDefaultValue = false;

            if (parameter.IndexOf(';') > -1)
            {
                string[] parts = parameter.Split(';');

                if (parts.Length >= 2)
                {
                    parameter = parts[0].Trim();
                    defaultValue = ParseFloat(parts[1], source, validateGTEZero, isSliceOperation);
                    hasDefaultValue = true;
                }
            }

            DataSourceValue result = source.FirstOrDefault(dataValue => dataValue.Target.Equals(parameter, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(result.Target))
            {
                // Slice operations may not have a target for a given slice - in this case function should use a default value and not fail
                if (isSliceOperation || hasDefaultValue)
                    result.Value = defaultValue;
                else
                    throw new FormatException($"Value target '{parameter}' could not be found in dataset nor parsed as a floating-point value.");
            }

            value = result.Value;
        }

        if (validateGTEZero && value < 0.0D)
            throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than zero.");

        return value;
    }
}