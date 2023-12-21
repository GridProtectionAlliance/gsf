//******************************************************************************************************
//  FunctionParser.cs - Gbtc
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GSF;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace GrafanaAdapters.GrafanaFunctionsCore;

internal static class FunctionParser
{
    private struct PhasorInfo
    {
        public string Label;
        public string Magnitude;
        public string Phase;
    };

    internal class ParsedTarget<T>
    {
        private readonly string[] PointTargets;

        public IGrafanaFunction Function { get; }
            
        public string[] DataTargets => Function is null ? PointTargets : TargetParameter.SelectMany(e => e.SelectMany(f => f.DataTargets)).ToArray();

        public List<ParsedTarget<T>[]> TargetParameter { get; }

        public List<string> BaseParameter { get; }

        public ParsedTarget(string expression)
        {
            TargetParameter = new List<ParsedTarget<T>[]>();
            BaseParameter = new List<string>();
            PointTargets = Array.Empty<string>();

            (Function, string parameterValue) = MatchFunction(expression);

            if (Function is null)
            {
                PointTargets = expression.Split(';');
                return;
            }

            int paramIndex = 0;

            // Separate data points from params
            foreach (string parameter in SplitWithParenthesis(parameterValue, ','))
            {
                if (Function.Parameters[paramIndex] is IParameter<IDataSourceValueGroup>)
                    TargetParameter.Add(ParseTargets(parameter)); // Data point
                else
                    BaseParameter.Add(parameter); // Other Parameter

                paramIndex++;
            }

        }

        /// <summary>
        /// Parses an expression like Func1(X,Y,Z);FUNC2(D,E,F) into separate ParsedTargets
        /// </summary>
        /// <param name="expression"></param>
        public static ParsedTarget<T>[] ParseTargets(string expression)
        {
            return string.IsNullOrWhiteSpace(expression) ? 
                Array.Empty<ParsedTarget<T>>() : 
                SplitWithParenthesis(expression, ';').Select(e => new ParsedTarget<T>(e)).ToArray();
        }
    }

    /// <summary>
    /// Parses an expression using the available Grafana Functions
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression">The expression to parse.</param>
    /// <param name="dataSourceBase">The <see cref="GrafanaDataSourceBase"/>.</param>
    /// <param name="queryData">The query information. </param>
    public static DataSourceValueGroup<T>[] Parse<T>(string expression, GrafanaDataSourceBase dataSourceBase, QueryDataHolder queryData)
    {
        if (queryData.CancellationToken.IsCancellationRequested) return null;

        // Take the initial expression apart
        IEnumerable<ParsedTarget<T>> targets = ParsedTarget<T>.ParseTargets(expression);

        // Get a dictionary of point tags and data
        Dictionary<string, DataSourceValueGroup<T>> pointData;

        if (queryData.IsPhasor)
            pointData = GetTargetPhasorData(targets.SelectMany(t => t.DataTargets).ToArray(), dataSourceBase, queryData) as Dictionary<string, DataSourceValueGroup<T>>;
        else
            pointData = GetTargetValueData(targets.SelectMany(t => t.DataTargets).ToArray(), dataSourceBase, queryData) as Dictionary<string, DataSourceValueGroup<T>>;

        // Apply the function
        List<DataSourceValueGroup<T>[]> result = new();

        // Initialize the list with placeholders to set capacity and enable index-based setting
        for (int i = 0; i < targets.Count(); i++)
            result.Add(Array.Empty<DataSourceValueGroup<T>>());

        // Walk through targets and save in order
        Parallel.ForEach(targets, (target, _, index) =>
        {
            result[(int)index] = ProcessTarget(target, pointData, dataSourceBase, queryData);
        });

        return result.SelectMany(e => e).ToArray();
    }

    private static DataSourceValueGroup<T>[] ProcessTarget<T>(ParsedTarget<T> target, Dictionary<string, DataSourceValueGroup<T>> pointData, GrafanaDataSourceBase dataSourceBase, QueryDataHolder queryOptions)
    {
        if (target.Function is null)
        {
            return target.DataTargets.SelectMany(t => GetPointTags(t, dataSourceBase))
                .Select(t => pointData[t]).ToArray();
        }

        // Fetch data points query
        List<DataSourceValueGroup<T>[]> groupedDataValues = new();

        foreach (ParsedTarget<T>[] query in target.TargetParameter)
        {
            DataSourceValueGroup<T>[] queryResult = query.SelectMany(q => ProcessTarget(q, pointData, dataSourceBase, queryOptions)).ToArray();
            groupedDataValues.Add(queryResult);
        }

        // Regroup data points to align for function
        // Ex: [A, B] & [C, D] -> [A, C] & [B, D]
        // If unequal number [A, B] & [C] fill remaining space with last element [A, C] & [B, C]
        List<DataSourceValueGroup<T>[]> regroupedDataValues = RegroupDataValues(groupedDataValues);

        // Apply the function
        List<DataSourceValueGroup<T>> res = new();

        // Initialize the list with placeholders to set capacity and enable index-based setting.
        for (int i = 0; i < regroupedDataValues.Count; i++)
            res.Add(null);

        Parallel.ForEach(regroupedDataValues, (dataValues, _, index) =>
        {
            List<IParameter> computeParameters = GenerateParameters(dataSourceBase, target.Function, target.BaseParameter, dataValues, queryOptions);
            DataSourceValueGroup<T> computedValues;

            if (typeof(T) == typeof(DataSourceValue))
                computedValues = (DataSourceValueGroup<T>)(object)target.Function.Compute(computeParameters);
            else if (typeof(T) == typeof(PhasorValue))
                computedValues = (DataSourceValueGroup<T>)(object)target.Function.ComputePhasor(computeParameters);
            else
                throw new InvalidOperationException($"Unsupported type parameter '{typeof(T)}' in Compute method");

            // Use the index to ensure the order is maintained.
            res[(int)index] = computedValues;
        });

        return res.ToArray();
    }

    private static (IGrafanaFunction function, string parameterValue) MatchFunction(string expression)
    {
        List<IGrafanaFunction> grafanaFunctions = GetGrafanaFunctions();

        foreach (IGrafanaFunction function in grafanaFunctions)
        {
            // Check if the expression matches the current function's regex
            if (!function.Regex.IsMatch(expression))
                continue;

            // Get the matched groups from the regex
            GroupCollection groups = function.Regex.Match(expression).Groups;
            string parameterValue = groups[groups.Count - 1].Value;

            return (function, parameterValue);
        }

        // No match found
        return (null, expression);
    }

    /// <summary>
    /// Turns an expression Target into PointTags (e.g. Filter, PPA:123)
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="dataSourceBase"></param>
    /// <returns></returns>
    private static string[] GetPointTags(string expression, GrafanaDataSourceBase dataSourceBase)
    {
        DataSet Metadata = dataSourceBase.Metadata;
        MeasurementKey[] results = AdapterBase.ParseInputMeasurementKeys(Metadata, false, expression.SplitAlias(out string alias));

        if (!string.IsNullOrWhiteSpace(alias) && results.Length == 1)
            return new[] { $"{alias}={results[0].TagFromKey(Metadata)}" };

        return results.Select(key => key.TagFromKey(Metadata)).ToArray();
    }

    private static Dictionary<string, DataSourceValueGroup<DataSourceValue>> GetTargetValueData(string[] targets, GrafanaDataSourceBase dataSourceBase, QueryDataHolder queryData)
    {
        if (targets.Length == 0)
            return new Dictionary<string, DataSourceValueGroup<DataSourceValue>>();

        DataSet Metadata = dataSourceBase.Metadata;
        Dictionary<ulong, string> targetMap = new();

        // Expand target set to include point tags for all parsed inputs
        HashSet<string> targetSet = new();

        foreach (string target in targets)
        {
            targetSet.UnionWith(TargetCache<string[]>.GetOrAdd(target, () => GetPointTags(target, dataSourceBase)));
        }

        // Reduce all targets down to a dictionary of point ID's mapped to point tags
        foreach (string target in targetSet)
        {
            MeasurementKey key = TargetCache<MeasurementKey>.GetOrAdd(target, () => target.KeyFromTag(Metadata));

            if (key == MeasurementKey.Undefined)
            {
                Tuple<MeasurementKey, string> result = TargetCache<Tuple<MeasurementKey, string>>.GetOrAdd($"signalID@{target}", () => target.KeyAndTagFromSignalID(Metadata));

                key = result.Item1;
                string pointTag = result.Item2;

                if (key == MeasurementKey.Undefined)
                {
                    result = TargetCache<Tuple<MeasurementKey, string>>.GetOrAdd($"key@{target}", () =>
                    {
                        MeasurementKey.TryParse(target, out MeasurementKey parsedKey);

                        return new Tuple<MeasurementKey, string>(parsedKey, parsedKey.TagFromKey(Metadata));
                    });

                    key = result.Item1;
                    pointTag = result.Item2;

                    if (key != MeasurementKey.Undefined)
                        targetMap[key.ID] = pointTag;
                }
                else
                {
                    targetMap[key.ID] = pointTag;
                }
            }
            else
            {
                targetMap[key.ID] = target;
            }
        }

        // Query underlying data source for each target - to prevent parallel read from data source we enumerate immediately
        List<DataSourceValue> dataValues = dataSourceBase.QueryDataSourceValues(queryData.StartTime, queryData.StopTime, queryData.Interval, queryData.IncludePeaks, targetMap)
            .TakeWhile(_ => !queryData.CancellationToken.IsCancellationRequested).ToList();

        return targetMap.ToDictionary(kvp => kvp.Value, kvp => {
            IEnumerable<DataSourceValue> filteredValues = dataValues.Where(dataValue => dataValue.Target.Equals(kvp.Value));

            return new DataSourceValueGroup<DataSourceValue>
            {
                Target = kvp.Value,
                RootTarget = kvp.Value,
                SourceTarget = queryData.SourceTarget,
                Source = filteredValues,
                DropEmptySeries = queryData.DropEmptySeries,
                refId = queryData.SourceTarget.refId,
                metadata = GetMetadata(dataSourceBase, kvp.Value, queryData.MetadataSelection)
            };
        });            
    }

    private static Dictionary<string, DataSourceValueGroup<PhasorValue>> GetTargetPhasorData(string[] targets, GrafanaDataSourceBase dataSourceBase, QueryDataHolder queryData)
    {
        if (targets.Length == 0)
            return new Dictionary<string, DataSourceValueGroup<PhasorValue>>();

        DataSet metadata = dataSourceBase.Metadata;
        Dictionary<int, PhasorInfo> phasorTargets = new();
        Dictionary<ulong, string> targetMap = new();

        foreach (string targetLabel in targets)
        {
            if (targetLabel.StartsWith("FILTER ", StringComparison.OrdinalIgnoreCase) && AdapterBase.ParseFilterExpression(targetLabel.SplitAlias(out _), out string tableName, out string exp, out string sortField, out int takeCount))
            {
                foreach (DataRow row in dataSourceBase.Metadata.Tables[tableName].Select(exp, sortField).Take(takeCount))
                {
                    int targetId = Convert.ToInt32(row["ID"]);
                    phasorTargets[Convert.ToInt32(targetId)] = new PhasorInfo
                    {
                        Label = row["Label"].ToString(),
                        Phase = "",
                        Magnitude = ""
                    };
                }
                continue;
            }

            // Get phasor id
            DataRow[] phasorRows = metadata.Tables["Phasor"].Select($"Label = '{targetLabel}'");

            if (phasorRows.Length == 0)
                throw new Exception($"Unable to find label {targetLabel}");

            foreach (DataRow row in phasorRows)
            {
                int targetId = Convert.ToInt32(phasorRows[0]["ID"]);

                phasorTargets[targetId] = new PhasorInfo
                {
                    Label = row["Label"].ToString(),
                    Phase = "",
                    Magnitude = ""
                };

                DataRow[] measurementRows = metadata.Tables["ActiveMeasurements"].Select($"PhasorID = '{targetId}'");
                    
                if (measurementRows.Length < 2)
                    throw new Exception($"Did not locate both magnitude and phase for {phasorTargets[targetId].Label} with {targetId} id");
                    
                foreach (DataRow pointRow in measurementRows)
                {
                    ulong id = Convert.ToUInt64(pointRow["ID"].ToString().Split(':')[1]);
                    string pointTag = pointRow["PointTag"].ToString();

                    targetMap[id] = pointTag;

                    if (pointRow["SignalType"].ToString().EndsWith("PH")) 
                    {
                        phasorTargets[targetId] = new PhasorInfo
                        {
                            Label = phasorTargets[targetId].Label,
                            Phase = pointTag,
                            Magnitude = phasorTargets[targetId].Magnitude,
                        };
                    }

                    if (pointRow["SignalType"].ToString().EndsWith("PM"))
                    {
                        phasorTargets[targetId] = new PhasorInfo
                        {
                            Label = phasorTargets[targetId].Label,
                            Magnitude = pointTag,
                            Phase = phasorTargets[targetId].Phase,
                        };
                    }
                }
            }
        }

        List<DataSourceValue> dataValues = dataSourceBase.QueryDataSourceValues(queryData.StartTime, queryData.StopTime, queryData.Interval, queryData.IncludePeaks, targetMap)
            .TakeWhile(_ => !queryData.CancellationToken.IsCancellationRequested).ToList();

        return phasorTargets.ToDictionary((target) => target.Value.Label,(target) =>
        {
            IEnumerable<DataSourceValue> filteredMagnitudes = dataValues.Where(dataValue => dataValue.Target.Equals(target.Value.Magnitude));
            List<DataSourceValue> filteredPhases = dataValues.Where(dataValue => dataValue.Target.Equals(target.Value.Phase)).ToList();
            IEnumerable<PhasorValue> phasorValues = GeneratePhasorValues();

            List<PhasorValue> GeneratePhasorValues()
            {
                List<PhasorValue> values = new();
                int index = 0;
                    
                foreach (DataSourceValue mag in filteredMagnitudes)
                {
                    if (index >= filteredPhases.Count)
                        continue;

                    PhasorValue phasor = new()
                    {
                        MagnitudeTarget = target.Value.Magnitude,
                        AngleTarget = target.Value.Phase,
                        Flags = mag.Flags,
                        Time = mag.Time,
                        Magnitude = mag.Value,
                        Angle = filteredPhases[index].Value
                    };

                    index++;
                        
                    values.Add(phasor);
                }
                return values;
            }

            return new DataSourceValueGroup<PhasorValue>
            {
                Target = $"{target.Value.Magnitude};{target.Value.Phase}",
                RootTarget = target.Value.Label,
                SourceTarget = queryData.SourceTarget,
                Source = phasorValues,
                DropEmptySeries = queryData.DropEmptySeries,
                refId = queryData.SourceTarget.refId,
                metadata = GetMetadata(dataSourceBase, target.Value.Label, queryData.MetadataSelection, true)
            };
        });
    }
      
    public static Dictionary<string, string> GetMetadata(GrafanaDataSourceBase dataSourceBase, string rootTarget, Dictionary<string, List<string>> metadataSelection, bool isPhasor = false)
    {
        // Create a new dictionary to hold the metadata values
        Dictionary<string, string> metadataDict = new();

        // Return an empty dictionary if metadataSelection is null or empty
        if (metadataSelection == null || metadataSelection.Count == 0)
        {
            return metadataDict;
        }

        //Iterate through selections
        foreach (KeyValuePair<string, List<string>> entry in metadataSelection)
        {
            string table = entry.Key;
            List<string> values = entry.Value;
            string selectQuery = isPhasor ? $"Label = '{rootTarget}'" : $"PointTag = '{rootTarget}'";
            DataRow[] rows = dataSourceBase?.Metadata.Tables[table].Select(selectQuery) ?? Array.Empty<DataRow>();

            // Populate the entry dictionary with the metadata values
            foreach (string value in values)
            {
                string metadataValue = string.Empty;

                if (rows.Length > 0)
                    metadataValue = rows[0][value].ToString();

                metadataDict[value] = metadataValue;
            }
        }

        return metadataDict;
    }


    private static List<IParameter> GenerateParameters<T>(GrafanaDataSourceBase dataSourceBase, IGrafanaFunction function, List<string> parsedParameters, DataSourceValueGroup<T>[] dataValues, QueryDataHolder queryData)
    {
        List<IParameter> parameters = function.Parameters;
        int paramIndex = 0;
        int dataIndex = 0;

        foreach (IParameter parameter in parameters)
        {
            if (parameter is Parameter<IDataSourceValueGroup> dataSourceValueParameter) // Data
            {
                // Not enough parameters 
                if (dataIndex >= dataValues.Length)
                {
                    dataSourceValueParameter.SetValue(dataSourceBase, null, null, null, queryData.IsPhasor);
                }
                else
                {
                    dataSourceValueParameter.SetValue(dataSourceBase, dataValues[dataIndex], dataValues[dataIndex].RootTarget, dataValues[dataIndex].metadata, queryData.IsPhasor);
                    dataIndex++;
                }
            }
            else // Parameter
            {
                if (paramIndex >= parsedParameters.Count)
                {
                    // Not enough parameters, if required throws error, else sets to default
                    parameter.SetValue(dataSourceBase, null, null, null, queryData.IsPhasor); 
                }
                else
                {
                    // Have a valid parameter, uses metadata from first 
                    parameter.SetValue(dataSourceBase, parsedParameters[paramIndex], dataValues[0]?.RootTarget, dataValues[0]?.metadata, queryData.IsPhasor);
                    paramIndex++;
                }
            }
        }

        return parameters;
    }

    public static object[] GenerateParameters(IGrafanaFunction function, string[] parsedParameters, IEnumerable<DataSourceValue> dataPoints)
    {
        object[] parameters = new object[function.Parameters.Count];
        int index = 0;
        int paramIndex = 0;

        foreach (IParameter parameter in function.Parameters)
        {
                
            if (parameter is IParameter<IEnumerable<DataSourceValue>>) // DataSourceValue
            {
                parameters[index] = dataPoints;
            }
            else // Other
            {
                if (paramIndex >= parsedParameters.Length) // Not enough parameters 
                {
                    // Required -> error
                    if (parameter.Required)
                        throw new ArgumentException($"Required parameter '{parameter.GetType()}' is missing.");

                    // Not required -> set to default
                    IParameter<object> genericParameter = parameter as IParameter<object>;
                    parameters[index] = genericParameter!.Default;
                }
                else // Have a valid parameter
                {
                    parameters[index] = parsedParameters[paramIndex];
                    paramIndex++;
                }
            }
                
            index++;
        }
            
        return parameters;
    }
        
    public static List<IGrafanaFunction> GetGrafanaFunctions()
    {
        List<Type> implementationTypes = typeof(IGrafanaFunction).LoadImplementations(FilePath.GetAbsolutePath("").EnsureEnd(Path.DirectorySeparatorChar), true, false).ToList();
        List<IGrafanaFunction> grafanaFunctions = new();

        foreach (Type type in implementationTypes)
        {
            if (type.GetConstructor(Type.EmptyTypes) != null) 
            {
                IGrafanaFunction instance = (IGrafanaFunction)Activator.CreateInstance(type);
                grafanaFunctions.Add(instance);
            }
        }

        return grafanaFunctions;
    }

    public static List<DataSourceValueGroup<T>[]> RegroupDataValues<T>(List<DataSourceValueGroup<T>[]> groupedDataValues)
    {
        List<DataSourceValueGroup<T>[]> result = new();

        // Empty list
        if (groupedDataValues.Count == 0)
            return result;

        int arrayLength = groupedDataValues.Any() ? groupedDataValues.Max(arr => arr.Length) : 0;
        int groupCount = groupedDataValues.Count;

        for (int i = 0; i < arrayLength; i++)
        {
            DataSourceValueGroup<T>[] newGroup = new DataSourceValueGroup<T>[groupCount];

            for (int j = 0; j < groupCount; j++)
            {
                if (i < groupedDataValues[j].Length)
                {
                    newGroup[j] = groupedDataValues[j][i];
                }
                else // Out of bounds
                {
                    // Check if the current group has elements
                    if (groupedDataValues[j].Length > 0)
                    {
                        // Take the last element
                        newGroup[j] = groupedDataValues[j][groupedDataValues[j].Length - 1];
                    }
                    else
                    {
                        // No elements in the current group, set to null
                        newGroup[j] = null;
                    }
                }
            }

            result.Add(newGroup);
        }

        return result;
    }

    /// <summary>
    /// Gets the <see cref="FunctionDescription"/> for all available Functions.
    /// </summary>
    /// <returns> a <see cref="IEnumerable{FunctionDescription}"/> </returns>
    public static IEnumerable<FunctionDescription> GetFunctionDescription()
    {
        return GetGrafanaFunctions().Select(type =>
        {
            return new FunctionDescription
            {
                Parameters = type.Parameters.Select(p => new ParameterDescription
                { 
                    Description = p.Description,
                    ParameterTypeName = p.ParameterTypeName,
                    Required = p.Required
                }).ToArray(),
                Name = type.Name,
                Description = type.Description
            };
        });
    }

    // Splits a string by a character accounting for parenthesis not being split up
    private static string[] SplitWithParenthesis(string expression, char splitChar)
    {
        List<string> result = new();
        StringBuilder currentParameter = new();
        int nestedParenthesesCount = 0;

        foreach (char currentChar in expression)
        {
            // Count parenthesis
            switch (currentChar)
            {
                case '(':
                    nestedParenthesesCount++;
                    break;
                case ')':
                    nestedParenthesesCount--;
                    break;
            }

            if (currentChar == splitChar && nestedParenthesesCount == 0) // Not inside (), store
            {
                result.Add(currentParameter.ToString().Trim());
                currentParameter.Clear();
            }
            else // Inside (), append for next part
            {
                currentParameter.Append(currentChar);
            }
        }

        if (currentParameter.Length > 0)
            result.Add(currentParameter.ToString().Trim());

        return result.ToArray();
    }
}