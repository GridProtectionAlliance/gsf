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

using System;
using System.Collections.Generic;
using System.Data;
using GSF.Units;

namespace GrafanaAdapters.GrafanaFunctionsCore;

internal class Parameter<T> : IParameter<T>
{
    public T Default { get; set; }
        
    public T Value { get; set; }
        
    public string Description { get; set; }
        
    public bool Required { get; set; }
        
    public Type ParameterType => typeof(T);

    public string ParameterTypeName => ParameterType.Name;

    private (T value, bool success) LookupMetadata<TDataSourceValue>(DataSet metadata, Dictionary<string, string> metadataMap, string value, string target) where TDataSourceValue : IDataSourceValue, new()
    {
        // Attempt to find in dictionary
        if (metadataMap.Count != 0 && metadataMap.TryGetValue(value, out string metaValue))
        {
            try
            {
                // Found, attempt to convert
                return ((T)Convert.ChangeType(metaValue, typeof(T)), true);
            }
            catch 
            {
                return (default, false);
            }
        }

        // Not found, lookup in metadata
        DataRow[] rows = DataSourceValue.Default<TDataSourceValue>().LookupMetadata(metadata, target);

        // Not valid
        if (!(rows.Length > 0 && rows[0].Table.Columns.Contains(value)))
            return (default, false);

        // Found, attempt to convert
        string foundValue = rows[0][value].ToString();

        try
        {
            return ((T)Convert.ChangeType(foundValue, typeof(T)), true);
        }
        catch 
        {
            return (default, false);
        }
    }

    /// <summary>
    /// Converts the value to the proper type.
    /// </summary>
    /// <remarks>
    /// This function is used to convert the value to the proper type
    /// If the type of value provided and expected match, then it directly converts
    /// If the types do not match, then it first searches through the provided metadata.
    /// If nothing is found, it looks through ActiveMeasurements for it.
    /// Finally, if none of the above work it throws an error.
    /// </remarks>
    public void SetValue<TDataSourceValue>(DataSet metadata, object value, string target, Dictionary<string, string> metadataMap) where TDataSourceValue : IDataSourceValue, new()
    {
        // No value specified
        if (value is null)
        {
            // Required -> error
            if (Required)
                throw new ArgumentException($"Required parameter '{GetType()}' is missing.");
                
            // Not required -> default
            Value = Default;
            return;
        }

        // Data
        if(typeof(T) == typeof(IDataSourceValueGroup))
        {
            Value = (T)value;
            return;
        }

        // Check if requested metadata
        string strValue = value.ToString();

        if (strValue.StartsWith("{") && strValue.EndsWith("}"))
        {
            strValue = strValue.Substring(1, strValue.Length - 2);
                
            (T value, bool success) result = LookupMetadata<TDataSourceValue>(metadata, metadataMap, strValue, target);
                
            if (result.success)
                strValue = result.value.ToString();
        }

        // Time Unit
        if (typeof(T) == typeof(TargetTimeUnit))
        {
            if (TargetTimeUnit.TryParse(strValue, out TargetTimeUnit timeUnit))
                Value = (T)(object)timeUnit;
            else
                Value = Default;

            return;
        }

        // Angle Unit
        if (typeof(T) == typeof(AngleUnit))
        {
            if (Enum.TryParse(strValue, out AngleUnit angleUnit))
                Value = (T)(object)angleUnit;
            else
                Value = Default;

            return;
        }

        // String
        if (typeof(T) == typeof(string))
        {
            Value = (T)(object)strValue;
            return;
        }
            
        try
        {
            // Attempt to convert
            Value = (T)Convert.ChangeType(strValue, typeof(T));
        }
        catch (Exception)
        {
            // Not proper type, check metadata
            (T value, bool success) result = LookupMetadata<TDataSourceValue>(metadata, metadataMap, strValue, target);

            if (result.success)
                Value = result.value;
            else
                throw new Exception($"Unable convert or find corresponding metadata for {strValue}");
        }
    }
}