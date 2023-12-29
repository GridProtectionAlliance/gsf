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
using System.Threading;
using System.Threading.Tasks;
using GSF;
using GSF.Collections;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;

namespace GrafanaAdapters.GrafanaFunctionsCore;

internal static class FunctionParser
{
    internal class ParsedTarget<T> where T : IDataSourceValue
    {
        private readonly string[] m_pointTargets;

        public IGrafanaFunction<T> Function { get; }

        public FunctionOperations GroupOperation { get; }
        
        public string[] DataTargets => Function is null ? m_pointTargets : TargetParameter.SelectMany(targets => targets.SelectMany(target => target.DataTargets)).ToArray();

        public List<ParsedTarget<T>[]> TargetParameter { get; }

        public List<string> BaseParameter { get; }

        public ParsedTarget(string expression)
        {
            TargetParameter = new List<ParsedTarget<T>[]>();
            BaseParameter = new List<string>();
            m_pointTargets = Array.Empty<string>();

            (Function, GroupOperation, string parameterValue) = MatchFunction<T>(expression);

            if (Function is null)
            {
                m_pointTargets = expression.Split(';');
                return;
            }

            List<IParameter> parameters = new(Function.Parameters);

            if (GroupOperation == FunctionOperations.Slice)
                parameters.InsertRequiredSliceParameter();

            int paramIndex = 0;

            // Separate data points from params
            foreach (string parameter in SplitWithParenthesis(parameterValue, ','))
            {
                if (parameters[paramIndex] is IParameter<IDataSourceValueGroup>)
                    TargetParameter.Add(ParseTargets<T>(parameter)); // Data point
                else
                    BaseParameter.Add(parameter); // Other Parameter

                paramIndex++;
            }
        }

        public ParsedTarget(ParsedTarget<T> target, List<string> baseParameter)
        {
            Function = target.Function;
            GroupOperation = FunctionOperations.Standard;
            TargetParameter = target.TargetParameter;
            BaseParameter = baseParameter;
            m_pointTargets = target.m_pointTargets;
        }
    }

    private static IGrafanaFunction[] s_grafanaFunctions;
    private static readonly object s_grafanaFunctionsLock = new();

    /// <summary>
    /// Parses an expression like Func1(X,Y,Z);FUNC2(D,E,F) into separate ParsedTargets
    /// </summary>
    /// <param name="expression"></param>
    public static ParsedTarget<T>[] ParseTargets<T>(string expression) where T : IDataSourceValue
    {
        return string.IsNullOrWhiteSpace(expression) ?
            Array.Empty<ParsedTarget<T>>() :
            SplitWithParenthesis(expression, ';').Select(subExpression => new ParsedTarget<T>(subExpression)).ToArray();
    }

    /// <summary>
    /// Gets all the data targets from a parsed target.
    /// </summary>
    /// <param name="targets">Parsed target array</param>
    /// <returns></returns>
    public static string[] GetDataTargets<T>(this ParsedTarget<T>[] targets) where T : IDataSourceValue
    {
        return new HashSet<string>(targets.SelectMany(target => target.DataTargets), StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>
    /// Executes targets with queried data using the available Grafana functions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targets">The targets.</param>
    /// <param name="pointData">The point data.</param>
    /// <param name="metadata">The data source metadata.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static IEnumerable<DataSourceValueGroup<T>> ExecuteSeriesFunctions<T>(ParsedTarget<T>[] targets, Dictionary<string, DataSourceValueGroup<T>> pointData, DataSet metadata, CancellationToken cancellationToken) where T : IDataSourceValue, new()
    {
        if (cancellationToken.IsCancellationRequested)
            return null;

        // Initialize the list with target capacity to enable index-based result assignment to maintain order
        List<List<DataSourceValueGroup<T>>> results = new(new List<DataSourceValueGroup<T>>[targets.Length]);

        // Apply the function for each targets
        Parallel.ForEach(targets, (target, _, index) =>
        {
            results[(int)index] = ExecuteSeriesFunction(target, pointData, metadata, cancellationToken);
        });

        // ToArray here complete evaluations of any final deferred enumerations
        return results.SelectMany(enumerable => enumerable);
    }

    private static List<DataSourceValueGroup<T>> ExecuteSeriesFunction<T>(ParsedTarget<T> target, Dictionary<string, DataSourceValueGroup<T>> pointData, DataSet metadata, CancellationToken cancellationToken) where T : IDataSourceValue, new()
    {
        if (target.Function is null)
            return target.DataTargets.SelectMany(valueGroup => GetPointTags(valueGroup, metadata)).Where(pointData.ContainsKey).Select(key => pointData[key]).ToList();

        string functionName = target.Function.Name;

        switch (target.GroupOperation)
        {
            case FunctionOperations.Slice:
            {
                // Execute all series functions over time slices
                TimeSliceScanner scanner = new(pointData.Values, ParseFloat(target.BaseParameter[0]) / SI.Milli);
                ParsedTarget<T> sliceTarget = new(target, target.BaseParameter.Skip(1).ToList());
                List<DataSourceValueGroup<T>> sliceResults = new();

                foreach (DataSourceValueGroup<T> valueGroup in ExecuteSeriesFunctionOverTimeSlices(scanner, sliceTarget, metadata, cancellationToken))
                {
                    sliceResults.Add(new DataSourceValueGroup<T>
                    {
                        Target = $"Slice{functionName}({string.Join(", ", sliceTarget.BaseParameter)}{(sliceTarget.BaseParameter.Count > 0 ? ", " : "")}{valueGroup.Target})",
                        RootTarget = valueGroup.RootTarget ?? valueGroup.Target,
                        SourceTarget = valueGroup.SourceTarget,
                        Source = valueGroup.Source,
                        DropEmptySeries = valueGroup.DropEmptySeries
                    });
                }

                return sliceResults;
            }
            case FunctionOperations.Set:
            {
                Dictionary<string, DataSourceValueGroup<T>>.ValueCollection values = pointData.Values;
                DataSourceValueGroup<T> first = values.First();

                // Flatten all series into a single enumerable
                first.Source = values.AsParallel().WithCancellation(cancellationToken).SelectMany(source => source.Source);

                DataSourceValueGroup<T> valueGroup = ExecuteSeriesFunction(target, new Dictionary<string, DataSourceValueGroup<T>> { { first.Target, first } }, metadata, cancellationToken).First();

                valueGroup.Target = $"Set{target.Function.Name}({string.Join(", ", target.BaseParameter)}{(target.BaseParameter.Count > 0 ? ", " : "")}{first.RootTarget})";
                
                // Handle edge-case set operations - for these functions there is data in the target series as well
                if (functionName is "Minimum" or "Maximum" or "Median")
                {
                    T dataValue = valueGroup.Source.First();
                    valueGroup.Target = $"Set{functionName} = {dataValue.Target}";
                    valueGroup.RootTarget = dataValue.Target;
                }

                return new List<DataSourceValueGroup<T>>(new[] { valueGroup });
            }
            default:
            {
                List<DataSourceValueGroup<T>[]> groupedDataValues = new();

                foreach (ParsedTarget<T>[] query in target.TargetParameter)
                {
                    DataSourceValueGroup<T>[] queryResult = query.SelectMany(subTarget => ExecuteSeriesFunction(subTarget, pointData, metadata, cancellationToken)).ToArray();
                    groupedDataValues.Add(queryResult);
                }

                // Initialize the list with placeholders to set capacity and enable index-based assignment to maintain order
                List<DataSourceValueGroup<T>> results = new(new DataSourceValueGroup<T>[groupedDataValues.Count]);

                // Apply the function, index is used to ensure the order is maintained
                Parallel.ForEach(groupedDataValues, (dataValues, _, index) =>
                {
                    List<IParameter> computeParameters = GenerateParameters(metadata, target.Function, target.BaseParameter, dataValues);
                    results[(int)index] = ComputeFunction(target.Function, target.GroupOperation, computeParameters, cancellationToken);
                });

                return results;
            }
        }
    }

    // Execute series function over a set of points from each series at the same time-slice
    private static IEnumerable<DataSourceValueGroup<T>> ExecuteSeriesFunctionOverTimeSlices<T>(TimeSliceScanner scanner, ParsedTarget<T> target, DataSet metadata, CancellationToken cancellationToken) where T : IDataSourceValue, new()
    {
        Dictionary<string, DataSourceValueGroup<T>> pointData = new();

        while (!scanner.DataReadComplete && !cancellationToken.IsCancellationRequested)
        {
            foreach (IGrouping<string, IDataSourceValue> valueGroup in scanner.ReadNextTimeSlice().GroupBy(dataValue => dataValue.Target))
            {
                DataSourceValueGroup<T> groupPoints = pointData.GetOrAdd(valueGroup.Key, _ => new DataSourceValueGroup<T>
                {
                    Target = valueGroup.Key,
                    RootTarget = valueGroup.Key,
                    Source = Enumerable.Empty<T>()
                });

                groupPoints.Source = groupPoints.Source.Concat(valueGroup.Cast<T>());
            }
        }

        foreach (DataSourceValueGroup<T> dataValueGroup in ExecuteSeriesFunction(target, pointData, metadata, cancellationToken))
            yield return dataValueGroup;
    }

    private static DataSourceValueGroup<T> ComputeFunction<T>(IGrafanaFunction<T> function, FunctionOperations groupOperation, List<IParameter> parameters, CancellationToken cancellationToken) where T : IDataSourceValue
    {
        return groupOperation switch
        {
            FunctionOperations.Slice => function.ComputeSlice(parameters, cancellationToken),
            FunctionOperations.Set => function.ComputeSet(parameters, cancellationToken),
            _ => function.Compute(parameters, cancellationToken)
        };
    }

    private static (IGrafanaFunction<T> function, FunctionOperations groupOperation, string parameterValue) MatchFunction<T>(string expression) where T: IDataSourceValue
    {
        IGrafanaFunction<T>[] grafanaFunctions = GetGrafanaFunctions<T>();

        foreach (IGrafanaFunction<T> function in grafanaFunctions)
        {
            // Check if the expression matches the current function's regex
            if (!function.Regex.IsMatch(expression))
                continue;

            // Get the matched groups from the regex
            GroupCollection groups = function.Regex.Match(expression).Groups;

            // Check if the function has a group operation prefix
            Enum.TryParse(groups["GroupOp"].Value, true, out FunctionOperations operation);

            // Default to standard
            if (operation == 0)
                operation = FunctionOperations.Standard;

            // Verify that the function supports the requested operation
            if (!function.SupportedFunctionOperations.HasFlag(operation))
                throw new InvalidOperationException($"Function '{function.Name}' does not support '{operation}' function operations.");

            return (function, operation, groups["Expression"].Value);
        }

        // No match found
        return (null, FunctionOperations.Standard, expression);
    }

    /// <summary>
    /// Turns an expression Target into PointTags (e.g. Filter, PPA:123)
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="dataSourceMetadata">Metadata of the data source</param>
    /// <returns>Point tags from expression.</returns>
    public static string[] GetPointTags(string expression, DataSet dataSourceMetadata)
    {
        MeasurementKey[] results = AdapterBase.ParseInputMeasurementKeys(dataSourceMetadata, false, expression.SplitAlias(out string alias));

        if (!string.IsNullOrWhiteSpace(alias) && results.Length == 1)
            return new[] { $"{alias}={results[0].TagFromKey(dataSourceMetadata)}" };

        return results.Select(key => key.TagFromKey(dataSourceMetadata)).ToArray();
    }

    /// <summary>
    /// Gets metadata map for the specified target and selections.
    /// </summary>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="rootTarget">Root target to use for metadata lookup.</param>
    /// <param name="metadataSelection">Metadata selections.</param>
    /// <returns>Mapped metadata for the specified target and selections.</returns>
    public static Dictionary<string, string> GetMetadata<T>(DataSet metadata, string rootTarget, Dictionary<string, List<string>> metadataSelection) where T : IDataSourceValue, new()
    {
        // Create a new dictionary to hold the metadata values
        Dictionary<string, string> metadataMap = new();

        // Return an empty dictionary if metadataSelection is null or empty
        if (metadataSelection is null || metadataSelection.Count == 0)
            return metadataMap;

        // Iterate through selections
        foreach (KeyValuePair<string, List<string>> entry in metadataSelection)
        {
            List<string> values = entry.Value;
            DataRow[] rows = DataSourceValue.Default<T>().LookupMetadata(metadata, rootTarget);

            // Populate the entry dictionary with the metadata values
            foreach (string value in values)
            {
                string metadataValue = string.Empty;

                if (rows.Length > 0)
                    metadataValue = rows[0][value].ToString();

                metadataMap[value] = metadataValue;
            }
        }

        return metadataMap;
    }

    private static List<IParameter> GenerateParameters<T>(DataSet metadata, IGrafanaFunction<T> function, List<string> parsedParameters, DataSourceValueGroup<T>[] dataValues) where T : IDataSourceValue, new()
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
                    dataSourceValueParameter.SetValue<T>(metadata, null, null, null);
                }
                else
                {
                    dataSourceValueParameter.SetValue<T>(metadata, dataValues[dataIndex], dataValues[dataIndex].RootTarget, dataValues[dataIndex].MetadataMap);
                    dataIndex++;
                }
            }
            else // Parameter
            {
                if (paramIndex >= parsedParameters.Count)
                {
                    // Not enough parameters, if required throws error, else sets to default
                    parameter.SetValue<T>(metadata, null, null, null); 
                }
                else
                {
                    // Have a valid parameter, uses metadata from first 
                    parameter.SetValue<T>(metadata, parsedParameters[paramIndex], dataValues[0]?.RootTarget, dataValues[0]?.MetadataMap);
                    paramIndex++;
                }
            }
        }

        return parameters;
    }

    /// <summary>
    /// Gets all the available Grafana functions.
    /// </summary>
    /// <returns>List of Grafana functions.</returns>
    public static IGrafanaFunction[] GetGrafanaFunctions()
    {
        // Caching default grafana functions so expensive assembly load with type inspections
        // and reflection-based instance creation of types are only done once. If dynamic
        // reload is needed at runtime, call ReloadGrafanaFunctions() method.
        IGrafanaFunction[] grafanaFunctions = Interlocked.CompareExchange(ref s_grafanaFunctions, null, null);
        
        if (grafanaFunctions is not null)
            return grafanaFunctions;

        // If many external calls, e.g., web requests, are made to this function at the same time,
        // there will be an initial pause while the first thread loads the Grafana functions
        lock (s_grafanaFunctionsLock)
        {
            // Check if another thread already created the Grafana functions
            if (s_grafanaFunctions is not null)
                return s_grafanaFunctions;

            string grafanaFunctionsPath = FilePath.GetAbsolutePath("").EnsureEnd(Path.DirectorySeparatorChar);
            List<Type> implementationTypes = typeof(IGrafanaFunction).LoadImplementations(grafanaFunctionsPath, true, false);

            List<IGrafanaFunction> list = new();

            foreach (Type type in implementationTypes.Where(type => type.GetConstructor(Type.EmptyTypes) is not null))
            {
                IGrafanaFunction function;

                // Check for Grafana functions nested within abstract base class definition
                if (type.ContainsGenericParameters)
                {
                    if (!type.IsNested || !type.DeclaringType!.IsGenericType)
                        continue;

                    // Must contain at least one generic argument because IsGenericType is true
                    Type[] constraints = type.DeclaringType.GetGenericArguments()[0].GetGenericParameterConstraints();

                    if (constraints.Length != 1 || constraints[0] != typeof(IDataSourceValue))
                        continue;

                    // Assign a specific type to abstract parent class so nested type can be constructed
                    function = (IGrafanaFunction)Activator.CreateInstance(type.MakeGenericType(typeof(DataSourceValue)));
                }
                else
                {
                    function = (IGrafanaFunction)Activator.CreateInstance(type);
                }

                list.Add(function);
            }

            Interlocked.Exchange(ref s_grafanaFunctions, list.ToArray());

            return s_grafanaFunctions;
        }
    }

    /// <summary>
    /// Gets all the available Grafana functions for a specific data source value type.
    /// </summary>
    /// <typeparam name="T">Data source value type.</typeparam>
    /// <returns>List of Grafana functions for a specific data source value type.</returns>
    public static IGrafanaFunction<T>[] GetGrafanaFunctions<T>() where T : IDataSourceValue
    {
        return TargetCache<IGrafanaFunction<T>[]>.GetOrAdd($"TypedGrafanaFunctions-{typeof(T).FullName}", () => 
            GetGrafanaFunctions().OfType<IGrafanaFunction<T>>().ToArray());
    }

    /// <summary>
    /// Gets all the available Grafana functions for a specific data source value type.
    /// </summary>
    /// <param name="dataType">Data source value type.</param>
    /// <returns>List of Grafana functions for a specific data source value type.</returns>
    public static IGrafanaFunction[] GetGrafanaFunctions(string dataType)
    {
        if (dataType is null)
            throw new ArgumentNullException(nameof(dataType));

        return TargetCache<IGrafanaFunction[]>.GetOrAdd($"NamedTypeGrafanaFunctions-{dataType}", () =>
        {
            Type type = GetLocalType(dataType);
            return GetGrafanaFunctions().Where(function => function.GetType() == type).ToArray();
        });
    }

    /// <summary>
    /// Reloads the Grafana functions.
    /// </summary>
    /// <remarks>
    /// This function should be called to clear Grafana functions cache when new
    /// local assemblies have been added that contain new Grafana functions.
    /// </remarks>
    public static void ReloadGrafanaFunctions()
    {
        Interlocked.Exchange(ref s_grafanaFunctions, null);
    }

    /// <summary>
    /// Gets the <see cref="FunctionDescription"/> for all available functions.
    /// </summary>
    /// <returns> a <see cref="IEnumerable{FunctionDescription}"/> </returns>
    public static IEnumerable<FunctionDescription> GetFunctionDescriptions()
    {
        // TODO: JRC - getting unique function names for the moment - this should be changed to filter by data source value type
        // since technically there could be functions that are unique to a specific data source value types
        return new HashSet<FunctionDescription>(GetGrafanaFunctions().SelectMany(function =>
        {
            List<FunctionDescription> descriptions = new();

            if (function.PublishedFunctionOperations.HasFlag(FunctionOperations.Standard))
            {
                descriptions.Add(new FunctionDescription
                {
                    Parameters = function.Parameters.Select(parameter => new ParameterDescription
                    {
                        Description = parameter.Description,
                        ParameterTypeName = parameter.ParameterTypeName,
                        Required = parameter.Required
                    }).ToArray(),
                    Name = function.Name,
                    Description = function.Description
                });
            }

            if (function.PublishedFunctionOperations.HasFlag(FunctionOperations.Slice))
            {
                List<IParameter> parameters = new(function.Parameters);
                parameters.InsertRequiredSliceParameter();

                descriptions.Add(new FunctionDescription
                {
                    Parameters = parameters.Select(parameter => new ParameterDescription
                    {
                        Description = parameter.Description,
                        ParameterTypeName = parameter.ParameterTypeName,
                        Required = parameter.Required
                    }).ToArray(),
                    Name = $"Slice{function.Name}",
                    Description = function.Description
                });
            }

            if (function.PublishedFunctionOperations.HasFlag(FunctionOperations.Set))
            {
                descriptions.Add(new FunctionDescription
                {
                    Parameters = function.Parameters.Select(parameter => new ParameterDescription
                    {
                        Description = parameter.Description,
                        ParameterTypeName = parameter.ParameterTypeName,
                        Required = parameter.Required
                    }).ToArray(),
                    Name = $"Set{function.Name}",
                    Description = function.Description
                });
            }

            return descriptions;
        }));
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