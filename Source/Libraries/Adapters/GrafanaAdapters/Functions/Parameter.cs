//******************************************************************************************************
//  Parameter.cs - Gbtc
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
// ReSharper disable PossibleMultipleEnumeration

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using GrafanaAdapters.DataSources;
using GSF;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Represents a mutable parameter of a Grafana function.
/// </summary>
/// <typeparam name="T">The type of the parameter.</typeparam>
internal class Parameter<T> : IMutableParameter<T>
{
    // Create a new mutable parameter from its definition.
    // Note: do not change signature of this constructor without also
    // modifying ParameterParsing.GetMutableParameterFactory operation.
    public Parameter(ParameterDefinition<T> definition)
    {
        Definition = definition;
        Value = definition.Default;
    }

    public string Name => Definition.Name;

    public ParameterDefinition<T> Definition { get; }

    public T Default => Definition.Default;

    object IParameter.Default => Default;

    // Only the value in this class is mutable.
    public T Value { get; set; }

    object IMutableParameter.Value
    {
        get => Value;
        set => Value = (T)value;
    }

    public string Description => Definition.Description;
        
    public bool Required => Definition.Required;
        
    public Type Type => Definition.Type;

    public bool IsDefinition => false;

    /// <summary>
    /// Converts parsed value to the parameter type.
    /// </summary>
    /// <remarks>
    /// This function is used to convert the parsed value to the parameter type.
    /// If the type of value provided and expected match, then it directly converts.
    /// If the types do not match, then it first searches through the provided metadata.
    /// If nothing is found, it looks through ActiveMeasurements for it.
    /// Finally, if none of the above work it throws an error.
    /// </remarks>
    public void ConvertParsedValue<TDataSourceValue>(string value, string target, IEnumerable<TDataSourceValue> dataSourceValues, DataSet metadata, Dictionary<string, string> metadataMap) where TDataSourceValue : struct, IDataSourceValue<TDataSourceValue>
    {
        // No value specified
        if (string.IsNullOrWhiteSpace(value))
        {
            // Required -> error
            if (Required)
                throw new ArgumentException($"Required '{Name}' parameter of type '{Type.Name}' is missing.");

            // Not required -> default
            Value = Default;
            return;
        }

        // Check if metadata is requested
        if (value.StartsWith("{") && value.EndsWith("}"))
        {
            value = value.Substring(1, value.Length - 2);
                
            (string value, bool success) lookup = LookupMetadata<TDataSourceValue>(value, target, metadata, metadataMap);
                
            if (lookup.success)
                value = lookup.value;
        }

        // String type
        if (typeof(T) == typeof(string))
        {
            Value = (T)(object)value;
            return;
        }

        // Time Unit - enum has a custom parse operation
        if (typeof(T) == typeof(TargetTimeUnit))
        {
            if (TargetTimeUnit.TryParse(value, out TargetTimeUnit timeUnit))
                Value = (T)(object)timeUnit;
            else
                Value = Default;

            return;
        }

        // Other types
        object result;

        // Attempt to convert to the parameter type
        if (value[0].IsNumeric())
        {
            try
            {
                // Convert.ChangeType is faster for numeric types
                result = Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                result = null;
            }
        }
        else
        {
            // ConvertToType uses a TypeConverter which works for most types, including enums
            result = value.ConvertToType(typeof(T));
        }

        // If conversion fails, check if value is a named target
        if (result is null && char.IsLetter(value[0]))
        {
            T defaultValue = default;
            bool hasDefaultValue = false;
            string[] targets = null;

            // Named target parameters can optionally specify multiple fall-back series and one final
            // default constant value each separated by a semi-colon to use when the named target series
            // is not available, e.g.: Top(T1;T2;5, T1;T2;T3)
            if (value.IndexOf(';') > -1)
            {
                string[] parts = value.Split(';');

                if (parts.Length >= 2)
                {
                    targets = new string[parts.Length - 1];
                    Array.Copy(parts, 0, targets, 0, targets.Length);
                    defaultValue = (T)parts[parts.Length - 1].ConvertToType(typeof(T));
                    hasDefaultValue = true;
                }
            }

            targets ??= new[] { value };

            foreach (string targetName in targets)
            {
                // Attempt to find named target in data source values
                TDataSourceValue sourceResult = dataSourceValues.FirstOrDefault(dataSourceValue => dataSourceValue.Target.Equals(targetName, StringComparison.OrdinalIgnoreCase));

                // Data source values are structs and cannot be null so an empty target means lookup failed
                if (string.IsNullOrEmpty(sourceResult.Target))
                    continue;

                double seriesValue = sourceResult.TimeSeriesValue[0];

                result = typeof(T).IsNumeric() ? 
                    Convert.ChangeType(seriesValue, typeof(T)) : 
                    seriesValue.ToString(CultureInfo.InvariantCulture).ConvertToType(typeof(T));

                break;
            }

            if (result is null && hasDefaultValue)
                result = defaultValue;
        }

        Value = result is null ? Default : (T)result;
    }

    private static (string value, bool success) LookupMetadata<TDataSourceValue>(string value, string target, DataSet metadata, Dictionary<string, string> metadataMap) where TDataSourceValue : struct, IDataSourceValue
    {
        // Attempt to value find in dictionary
        if (metadataMap.TryGetValue(value, out string metadataValue))
            return (metadataValue, true);

        // If not found in dictionary, lookup target in metadata
        DataRow[] rows = default(TDataSourceValue).LookupMetadata(metadata, target);

        if (rows.Length == 0 || !rows[0].Table.Columns.Contains(value))
            return (default, false);

        return (rows[0][value].ToString(), true);
    }
}