//******************************************************************************************************
//  FunctionParsing.cs - Gbtc
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

using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using GrafanaAdapters.Metadata;
using GSF;
using GSF.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GSF.Diagnostics;

namespace GrafanaAdapters.Functions;

internal static class FunctionParsing
{
    private static IGrafanaFunction[] s_grafanaFunctions;
    private static readonly object s_grafanaFunctionsLock = new();
    private static readonly LogPublisher s_log = Logger.CreatePublisher(typeof(FunctionParsing), MessageClass.Component);

    // Calls to this expensive match operation should be temporally cached by expression
    public static ParsedGrafanaFunction<T>[] MatchFunctions<T>(string expression) where T : struct, IDataSourceValue<T>
    {
        Regex functionsRegex = DataSourceValueCache<T>.FunctionsRegex;
        Dictionary<string, IGrafanaFunction<T>> functionMap = DataSourceValueCache<T>.FunctionMap;

        // Match all top-level functions in expression
        MatchCollection matches = functionsRegex.Matches(expression);

        List<ParsedGrafanaFunction<T>> parsedGrafanaFunctions = new();

        foreach (Match match in matches)
        {
            // Get the matched groups from the regex
            GroupCollection groups = match.Groups;

            // Lookup function by user provided name or alias
            if (!functionMap.TryGetValue(groups["Function"].Value, out IGrafanaFunction<T> function))
            {
            #if DEBUG
                Debug.Fail($"Unexpected failure to find function '{groups["Function"].Value}'.");
            #else
                continue;
            #endif
            }

            // Check if the function has a group operation prefix, e.g., slice or set
            Enum.TryParse(groups["GroupOp"].Value, true, out GroupOperations operation);

            // Verify that the function allows the requested group operation - this will
            // throw an exception if the function does not support the requested operation
            operation = function.CheckAllowedGroupOperation(operation);

            parsedGrafanaFunctions.Add(new ParsedGrafanaFunction<T>
            {
                Function = function,
                GroupOperation = operation,
                Expression = groups["Expression"].Value,
                MatchedValue = match.Value
            });
        }

        return parsedGrafanaFunctions.ToArray();
    }

    // Gets all the available Grafana functions.
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

            const string EventName = $"{nameof(FunctionParsing)} {nameof(IGrafanaFunction)} Type Load";

            try
            {
                s_log.Publish(MessageLevel.Info, EventName, $"Starting load for {nameof(IGrafanaFunction)} types...");
                long startTime = DateTime.UtcNow.Ticks;

                string grafanaFunctionsPath = FilePath.GetAbsolutePath("").EnsureEnd(Path.DirectorySeparatorChar);
                List<Type> implementationTypes = typeof(IGrafanaFunction).LoadImplementations(grafanaFunctionsPath, true, false);

                List<IGrafanaFunction> functions = new();

                foreach (Type type in implementationTypes.Where(type => type.GetConstructor(Type.EmptyTypes) is not null))
                {
                    (Type functionType, bool builtIn) = checkNestedType(type);

                    if (functionType is null)
                        continue;

                    IGrafanaFunction function = (IGrafanaFunction)Activator.CreateInstance(functionType);
                    functionType.GetProperty(nameof(IGrafanaFunction.Category))?.SetValue(function, builtIn ? Category.BuiltIn : Category.Custom);

                    functions.Add(function);
                }

                Interlocked.Exchange(ref s_grafanaFunctions, functions.ToArray());

                string elapsedTime = new TimeSpan(DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(3);
                s_log.Publish(MessageLevel.Info, EventName, $"Completed loading {nameof(IGrafanaFunction)} types: loaded {s_grafanaFunctions.Length:N0} types in {elapsedTime}.");
            }
            catch (Exception ex)
            {
                s_log.Publish(MessageLevel.Error, EventName, $"Failed while loading {nameof(IGrafanaFunction)} types: {ex.Message}", exception: ex);
            }

            return s_grafanaFunctions;
        }

        // Check for Grafana functions nested within abstract base class definition - 'BuiltIn' pattern
        static (Type functionType, bool builtIn) checkNestedType(Type type)
        {
            const string BuiltInNamespace = $"{nameof(GrafanaAdapters)}.{nameof(Functions)}.{nameof(BuiltIn)}";

            if (!type.ContainsGenericParameters)
                return (type, false);

            if (!type.IsNested || !type.DeclaringType!.IsGenericType)
                return (null, false);

            // Must contain at least one generic argument because IsGenericType is true
            Type[] constraints = type.DeclaringType.GetGenericArguments()[0].GetGenericParameterConstraints();

            // Look for any constraint based on IDataSourceValue, if found, assign a specific
            // type (any is fine) to generic parent class so nested type can be constructed
            return constraints.Any(constraint => constraint.GetInterfaces().Any(interfaceType => interfaceType == typeof(IDataSourceValue))) ?
                (type.MakeGenericType(typeof(DataSourceValue)), type.Namespace?.Equals(BuiltInNamespace) ?? false ) : 
                (null, false);
        }
    }

    // Gets the Grafana functions for a specific data source value type (by its index).
    public static IEnumerable<IGrafanaFunction> GetGrafanaFunctions(int dataTypeIndex)
    {
        return GetGrafanaFunctions().Where(function => function.DataTypeIndex == dataTypeIndex);
    }

    // Reloads the Grafana functions.
    public static void ReloadGrafanaFunctions()
    {
        lock (s_grafanaFunctionsLock)
        {
            Interlocked.Exchange(ref s_grafanaFunctions, null);
            TargetCaches.ResetAll();
        }

        // Reinitializing data source caches will reload all grafana functions per data source value type
        DataSourceValueCache.ReinitializeAll();
    }

    // Handle series rename operations for Grafana functions - this is a special case for handling the Label function
    public static async IAsyncEnumerable<DataSourceValueGroup<T>> RenameSeries<T>(this IAsyncEnumerable<DataSourceValueGroup<T>> dataset, QueryParameters queryParameters, DataSet metadata, string labelExpression, [EnumeratorCancellation] CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        if (labelExpression.StartsWith("\"") || labelExpression.StartsWith("'"))
            labelExpression = labelExpression.Substring(1, labelExpression.Length - 2);

        HashSet<string> uniqueLabelSet = new(StringComparer.OrdinalIgnoreCase);
        int index = 1;

        await foreach (DataSourceValueGroup<T> valueGroup in dataset.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            string rootTarget = valueGroup.RootTarget;

            string seriesLabel = TargetCache<string>.GetOrAdd($"{default(T).DataTypeIndex}:{labelExpression}@{rootTarget}", () =>
            {
                // If label expression does not contain any substitutions, just return the expression
                if (labelExpression.IndexOf('{') < 0)
                    return labelExpression;

                Dictionary<string, string> substitutions = new(StringComparer.OrdinalIgnoreCase);
                Regex fieldExpression = new(@"\{(?<Field>[^}]+)\}", RegexOptions.Compiled);

                // Handle substitutions for each tag defined in the rootTarget
                foreach (string item in rootTarget.Split(';'))
                {
                    // Load {alias} substitutions - alias values are for tags that have been assigned an alias, e.g.:
                    // for 'x=TAGNAME', the alias would be 'x' and the target would be 'TAGNAME'
                    string target = item.SplitAlias(out string alias);

                    if (substitutions.TryGetValue("alias", out string substitution))
                    {
                        // Pattern for multiple alias substitutions is: {alias}, {alias}, {alias}, ...
                        // This handles the case where multiple tags exist in the single root target
                        // and each one has am assigned alias
                        if (!string.IsNullOrWhiteSpace(alias))
                            substitutions["alias"] = string.IsNullOrWhiteSpace(substitution) ? alias : $"{substitution}, {alias}";
                    }
                    else
                    {
                        substitutions.Add("alias", alias ?? "");
                    }

                    // Check all substitution fields for table name specifications (ActiveMeasurements assumed)
                    HashSet<string> tableNames = new(new[] { DataSourceValue.MetadataTableName }, StringComparer.OrdinalIgnoreCase);
                    MatchCollection fields = fieldExpression.Matches(labelExpression);

                    foreach (Match match in fields)
                    {
                        string field = match.Result("${Field}");

                        // Check if specified field substitution has a table name prefix
                        string[] components = field.Split('.');

                        if (components.Length == 2)
                            tableNames.Add(components[0]);
                    }

                    // Load field substitutions for each table name from metadata
                    foreach (string tableName in tableNames)
                    {
                        // ActiveMeasurements view fields are added as non-prefixed field name substitutions
                        if (tableName.Equals(DataSourceValue.MetadataTableName, StringComparison.OrdinalIgnoreCase))
                            LoadFieldSubstitutions(metadata, substitutions, target, tableName, false);

                        // All other table fields are added with table name as the prefix {table.field}
                        LoadFieldSubstitutions(metadata, substitutions, target, tableName, true);
                    }
                }

                string derivedLabel = labelExpression;

                foreach (KeyValuePair<string, string> substitution in substitutions)
                    derivedLabel = derivedLabel.ReplaceCaseInsensitive($"{{{substitution.Key}}}", substitution.Value);

                if (derivedLabel.Equals(labelExpression, StringComparison.Ordinal))
                    derivedLabel = $"{labelExpression}{(index > 1 ? $" {index}" : "")}";

                index++;
                return derivedLabel;
            });

            // Verify that series label is unique
            while (uniqueLabelSet.Contains(seriesLabel))
            {
                Match match = s_uniqueSeriesRegex.Match(seriesLabel);

                if (match.Success)
                {
                    int count = int.Parse(match.Result("${Count}")) + 1;
                    seriesLabel = $"{match.Result("${Label}")} {count}";
                }
                else
                {
                    seriesLabel = $"{seriesLabel} 1";
                }
            }

            uniqueLabelSet.Add(seriesLabel);

            yield return new DataSourceValueGroup<T>
            {
                Target = seriesLabel,
                RootTarget = valueGroup.RootTarget,
                SourceTarget = queryParameters.SourceTarget,
                Source = valueGroup.Source,
                DropEmptySeries = queryParameters.DropEmptySeries,
                RefID = queryParameters.SourceTarget.refID,
                MetadataMap = valueGroup.MetadataMap
            };
        }
    }

    private static void LoadFieldSubstitutions(DataSet metadata, Dictionary<string, string> substitutions, string target, string tableName, bool usePrefix)
    {
        DataRow record = target.RecordFromTag(metadata, tableName);
        string prefix = usePrefix ? $"{tableName}." : "";

        if (record is null)
        {
            // Apply empty field substitutions when point tag metadata is not found
            foreach (string fieldName in metadata.Tables[tableName].Columns.Cast<DataColumn>().Select(column => column.ColumnName))
            {
                string columnName = $"{prefix}{fieldName}";

                if (columnName.Equals("PointTag", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!substitutions.ContainsKey(columnName))
                    substitutions.Add(columnName, "");
            }

            if (usePrefix)
                return;

            if (substitutions.TryGetValue("PointTag", out string substitution))
                substitutions["PointTag"] = $"{substitution}, {target}";
            else
                substitutions.Add("PointTag", target);
        }
        else
        {
            foreach (string fieldName in record.Table.Columns.Cast<DataColumn>().Select(column => column.ColumnName))
            {
                string columnName = $"{prefix}{fieldName}";
                string columnValue = record[fieldName].ToString();

                if (substitutions.TryGetValue(columnName, out string substitution))
                {
                    // Pattern for multiple field substitutions is: {field}, {field}, {field}, ...
                    // This handles the case where multiple tags exist in the single root target
                    if (!string.IsNullOrWhiteSpace(columnValue))
                        substitutions[columnName] = string.IsNullOrWhiteSpace(substitution) ? columnValue : $"{substitution}, {columnValue}";
                }
                else
                {
                    substitutions.Add(columnName, columnValue);
                }
            }
        }
    }

    private static readonly Regex s_uniqueSeriesRegex = new(@"(?<Label>.+) (?<Count>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
}