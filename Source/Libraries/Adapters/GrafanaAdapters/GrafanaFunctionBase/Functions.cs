using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GrafanaAdapters.GrafanaFunctions;
using GSF.TimeSeries;
using GSF;
using GSF.IO;
using GSF.TimeSeries.Adapters;
using System.ServiceModel.Description;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
namespace GrafanaAdapters.GrafanaFunctions
{

    internal static class FunctionParser
    {
        internal class ParsedTarget<T>
        {
            private string[] PointTargets;

            public IGrafanaFunction Function { get; }
            public string[] DataTargets 
            { 
                get
                {
                    if (Function is null) return PointTargets;
                    return TargetParameter.SelectMany(e => e.SelectMany(f => f.DataTargets)).ToArray();
                } 
            }
            public List<ParsedTarget<T>[]> TargetParameter { get; }
            public List<string> BaseParameter { get; }

            public ParsedTarget(string expression)
            {
                TargetParameter = new List<ParsedTarget<T>[]>();
                BaseParameter = new List<string>();
                PointTargets = new string[0];

                (Function, string parameterValue) = FunctionParser.MatchFunction<T>(expression);

                if (Function is null)
                {
                    PointTargets = expression.Split(';');
                    return;
                }


                int paramIndex = 0;

                // Seperate datapoints from params
                foreach (string parameter in SplitWithPrenteses(parameterValue, ','))
                {
                    // Datapoint
                    if (Function.Parameters[paramIndex] is IParameter<IDataSourceValueGroup>)
                    {
                        TargetParameter.Add(ParseTargets(parameter));
                    }
                    // Other Parameter
                    else
                    {
                        BaseParameter.Add(parameter);
                    }
                    paramIndex++;
                }

            }

            /// <summary>
            /// Parses an expresion like Func1(X,Y,Z);FUNC2(D,E,F) into sepperate ParsedTargets
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            public static ParsedTarget<T>[] ParseTargets(string expression)
            {
                if (string.IsNullOrWhiteSpace(expression)) return new ParsedTarget<T>[0];
                return SplitWithPrenteses(expression, ';').Select(e => new ParsedTarget<T>(e)).ToArray();
            }
        }

        /// <summary>
        /// Parses an expresion using the available Grafana Functions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"> the Expresion to Parse</param>
        /// <param name="dataSourceBase">The <see cref="GrafanaDataSourceBase"/></param>
        /// <param name="queryData"> The Query information. </param>
        /// <returns></returns>
        public static DataSourceValueGroup<T>[] Parse<T>(string expression, GrafanaDataSourceBase dataSourceBase, QueryDataHolder queryData)
        {
            if (queryData.CancellationToken.IsCancellationRequested) return null;

            // take the initial expression appart.... 
            IEnumerable<ParsedTarget<T>> targets = ParsedTarget<T>.ParseTargets(expression);

            //Get a Dictionary of PointTags and Data
            Dictionary<string, DataSourceValueGroup<T>> pointData;

            if (queryData.IsPhasor)
                pointData = GetTargetPhasorData(targets.SelectMany(t => t.DataTargets).ToArray(), dataSourceBase, queryData) as Dictionary<string, DataSourceValueGroup<T>>;
            else
                pointData = GetTargetValueData(targets.SelectMany(t => t.DataTargets).ToArray(), dataSourceBase, queryData) as Dictionary<string, DataSourceValueGroup<T>>;

            // Apply the function
            List<DataSourceValueGroup<T>[]> result = new List<DataSourceValueGroup<T>[]>();

            // Initialize the list with placeholders to set capacity and enable index-based setting.
            for (int i = 0; i < targets.Count(); i++)
                result.Add(new DataSourceValueGroup<T>[0]);

            // Walk through targets and save in order
            Parallel.ForEach(targets, (target, loopState, index) =>
            {
                result[(int)index] = ProcessTarget<T>(target, pointData, dataSourceBase, queryData);
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

            //Fetch datapoints query
            List<DataSourceValueGroup<T>[]> groupedDataValues = new List<DataSourceValueGroup<T>[]>();
            foreach (ParsedTarget<T>[] query in target.TargetParameter)
            {
                DataSourceValueGroup<T>[] queryResult = query.SelectMany(q => ProcessTarget<T>(q, pointData, dataSourceBase, queryOptions)).ToArray();
                groupedDataValues.Add(queryResult);
            }

            //Regroup datapoints to align for function
            //Ex: [A, B] & [C, D] -> [A, C] & [B, D]
            //If unequal number [A, B] & [C] fill remaining space with last element [A, C] & [B, C]
            List<DataSourceValueGroup<T>[]> regroupedDataValues = RegroupDataValues(groupedDataValues);

            // Apply the function
            List<DataSourceValueGroup<T>> res = new List<DataSourceValueGroup<T>>();

            // Initialize the list with placeholders to set capacity and enable index-based setting.
            for (int i = 0; i < regroupedDataValues.Count(); i++)
                res.Add(null);

            Parallel.ForEach(regroupedDataValues, (dataValues, loopState, index) =>
            {
                List<IParameter> computeParameters = GenerateParameters(dataSourceBase, target.Function, target.BaseParameter, dataValues, queryOptions);
                DataSourceValueGroup<T> computedValues;
                if (typeof(T) == typeof(DataSourceValue))
                {
                    computedValues = (DataSourceValueGroup<T>)(object)target.Function.Compute(computeParameters);
                }
                else if (typeof(T) == typeof(PhasorValue))
                {
                    computedValues = (DataSourceValueGroup<T>)(object)target.Function.ComputePhasor(computeParameters);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported type parameter '{typeof(T)}' in Compute method");
                }

                // Use the index to ensure the order is maintained.
                res[(int)index] = computedValues;
            });

            return res.ToArray();
        }
        private static (IGrafanaFunction function, string parameterValue) MatchFunction<T>(string expression)
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
            HashSet<string> targetSet = new HashSet<string>();

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
                IEnumerable<DataSourceValue> filteredValues = (IEnumerable<DataSourceValue>)dataValues.OfType<DataSourceValue>()
                            .Where(dataValue => dataValue.Target.Equals(kvp.Value));

                return new DataSourceValueGroup<DataSourceValue>
                {
                    Target = kvp.Value,
                    RootTarget = kvp.Value,
                    SourceTarget = queryData.SourceTarget,
                    Source = filteredValues,
                    DropEmptySeries = queryData.DropEmptySeries,
                    refId = queryData.SourceTarget.refId,
                    metadata = GetMetadata(dataSourceBase, kvp.Value, queryData.MetadataSelection, false)
                };
            });            
        }

        private struct PhasorInfo
        {
            public string Label;
            public string Magnitude;
            public string Phase;
        };

        //Cleanup Neccesarry
        private static Dictionary<string, DataSourceValueGroup<PhasorValue>> GetTargetPhasorData(string[] targets, GrafanaDataSourceBase dataSourceBase, QueryDataHolder queryData)
        {
            if (targets.Length == 0)
                return new Dictionary<string, DataSourceValueGroup<PhasorValue>>();

            DataSet Metadata = dataSourceBase.Metadata;

            

            HashSet<string> targetSet = new HashSet<string>();
            Dictionary<int, PhasorInfo> phasorTargets = new ();

            Dictionary<ulong, string> targetMap = new();

            foreach (string targetLabel in targets)
            {
                if (targetLabel.StartsWith("FILTER ", StringComparison.OrdinalIgnoreCase) && AdapterBase.ParseFilterExpression(targetLabel.SplitAlias(out string alias), out string tableName, out string exp, out string sortField, out int takeCount))
                {
                    foreach (DataRow row in dataSourceBase.Metadata.Tables[tableName].Select(exp, sortField).Take(takeCount))
                    {
                        int targetId = Convert.ToInt32(row["ID"]);
                        phasorTargets[Convert.ToInt32(targetId)] = new() 
                        {
                            Label = row["Label"].ToString(),
                            Phase = "",
                            Magnitude = ""
                        };
                    }
                    continue;
                }

                //Get phasor id
                DataRow[] phasorRows = Metadata.Tables["Phasor"].Select($"Label = '{targetLabel}'");
                if (phasorRows.Length == 0)
                    throw new Exception($"Unable to find label {targetLabel}");

                foreach (DataRow row in phasorRows)
                {
                    int targetId = Convert.ToInt32(phasorRows[0]["ID"]);
                    phasorTargets[targetId] = new()
                    {
                        Label = row["Label"].ToString(),
                        Phase = "",
                        Magnitude = ""
                    };
                    DataRow[] measurementRows = Metadata.Tables["ActiveMeasurements"].Select($"PhasorID = '{targetId}'");
                    if(measurementRows.Length < 2)
                    {
                        throw new Exception($"Did not locate both magnitude and phase for {phasorTargets[targetId].Label} with {targetId} id");
                    }
                    foreach (DataRow pointRow in measurementRows)
                    {

                        ulong id = Convert.ToUInt64(pointRow["ID"].ToString().Split(':')[1]);
                        string pointTag = pointRow["PointTag"].ToString();
                        targetMap[id] = pointTag;
                        if (pointRow["SignalType"].ToString().EndsWith("PH")) 
                        {
                            phasorTargets[targetId] = new()
                            {
                                Label = phasorTargets[targetId].Label,
                                Phase = pointTag,
                                Magnitude = phasorTargets[targetId].Magnitude,
                            };
                        }
                        if (pointRow["SignalType"].ToString().EndsWith("PM"))
                        {
                            phasorTargets[targetId] = new()
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

                IEnumerable<DataSourceValue> filteredMagnitudes = (IEnumerable<DataSourceValue>)dataValues.OfType<DataSourceValue>()
                            .Where(dataValue => dataValue.Target.Equals(target.Value.Magnitude));
                List<DataSourceValue> filteredPhases = dataValues.OfType<DataSourceValue>()
                            .Where(dataValue => dataValue.Target.Equals(target.Value.Phase)).ToList();

                IEnumerable<PhasorValue> phasorValues = GeneratePhasorValues();

                List<PhasorValue> GeneratePhasorValues()
                {
                    List<PhasorValue> phasorValues = new List<PhasorValue>();
                    int index = 0;
                    foreach (DataSourceValue mag in filteredMagnitudes)
                    {
                        if (index >= filteredPhases.Count())
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
                        phasorValues.Add(phasor);
                    }
                    return phasorValues;
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
      
        public static Dictionary<string, string> GetMetadata(GrafanaDataSourceBase dataSourceBase, 
            string rootTarget, Dictionary<string, List<string>> metadataSelection, bool isPhasor = false)
        {
            // Create a new dictionary to hold the metadata values
            var metadataDict = new Dictionary<string, string>();

            // Return an empty dictionary if metadataSelection is null or empty
            if (metadataSelection == null || metadataSelection.Count == 0)
            {
                return metadataDict;
            }

            //Iterate through selections
            foreach (var entry in metadataSelection)
            {
                string table = entry.Key;
                List<string> values = entry.Value;
                string selectQuery = isPhasor ? $"Label = '{rootTarget}'" : $"PointTag = '{rootTarget}'";
                DataRow[] rows = dataSourceBase?.Metadata.Tables[table].Select(selectQuery) ?? new DataRow[0];

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


        private static List<IParameter> GenerateParameters<T>(GrafanaDataSourceBase dataSourceBase, IGrafanaFunction function, 
            List<string> parsedParameters, DataSourceValueGroup<T>[] dataValues, QueryDataHolder queryData)
        {
            List<IParameter> parameters = function.Parameters;
            int paramIndex = 0;
            int dataIndex = 0;
            foreach (IParameter parameter in parameters)
            {
                //Data
                if (parameter is Parameter<IDataSourceValueGroup> dataSourceValueParameter)
                {
                    //Not enough parameters 
                    if (dataIndex >= dataValues.Length)
                    {
                        dataSourceValueParameter.SetValue(dataSourceBase, null, null, null, queryData.IsPhasor);
                    }
                    else
                    {
                        dataSourceValueParameter.SetValue(dataSourceBase, dataValues[dataIndex], 
                            dataValues[dataIndex].RootTarget, dataValues[dataIndex].metadata, queryData.IsPhasor);
                        dataIndex++;
                    }

                }
                //Parameter
                else
                {
                    //Not enough parameters 
                    if (paramIndex >= parsedParameters.Count)
                    {
                        //If required throws error. If not sets to default
                        parameter.SetValue(dataSourceBase, null, null, null, queryData.IsPhasor); 
                    }
                    //Have a valid parameter
                    else
                    {
                        //Uses metadata from first 
                        parameter.SetValue(dataSourceBase, parsedParameters[paramIndex], dataValues[0]?.RootTarget, dataValues[0]?.metadata, queryData.IsPhasor);
                        paramIndex++;
                    }
                }
            }

            return parameters;
        }

        public static object[] GenerateParameters<T>(IGrafanaFunction function, string[] parsedParameters, IEnumerable<DataSourceValue> dataPoints)
        {
            object[] parameters = new object[function.Parameters.Count()];
            int index = 0;
            int paramIndex = 0;
            foreach (IParameter parameter in function.Parameters)
            {
                //DataSourceValue
                if (parameter is IParameter<IEnumerable<DataSourceValue>>)
                {
                    parameters[index] = dataPoints;
                }

                //Other
                else
                {
                    //Not enough parameters 
                    if (paramIndex >= parsedParameters.Length)
                    {
                        //Required -> error
                        if (parameter.Required)
                        {
                            throw new ArgumentException($"Required parameter '{parameter.GetType().ToString()}' is missing.");
                        }
                        //Not required -> set to default
                        else
                        {
                            IParameter<object> genericParameter = parameter as IParameter<object>;
                            parameters[index] = genericParameter.Default;
                        }
                    }
                    //Have a valid parameter
                    else
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
            List<IGrafanaFunction> grafanaFunctions = new List<IGrafanaFunction>();

            foreach (Type type in implementationTypes)
            {
                if (type.GetConstructor(Type.EmptyTypes) != null) // Check if the type has a parameterless constructor
                {
                    IGrafanaFunction instance = (IGrafanaFunction)Activator.CreateInstance(type);
                    grafanaFunctions.Add(instance);
                }
            }

            return grafanaFunctions;
        }

        public static List<DataSourceValueGroup<T>[]> RegroupDataValues<T>(List<DataSourceValueGroup<T>[]> groupedDataValues)
        {
            List<DataSourceValueGroup<T>[]> result = new List<DataSourceValueGroup<T>[]>();

            // Empty list
            if (groupedDataValues.Count == 0)
            {
                return result;
            }

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
                    // Out of bounds
                    else
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
                return new FunctionDescription()
                {
                    Parameters = type.Parameters.Select(p => new ParameterDescription() { 
                        Description = p.Description,
                        ParameterTypeName = p.ParameterTypeName,
                        Required = p.Required
                    }).ToArray(),
                    Name = type.Name,
                    Description = type.Description
                };
            });
        }

        /// <summary>
        /// Splits a string by a character accounting for Prenteses not being split up
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="splitChar"></param>
        /// <returns></returns>
        private static string[] SplitWithPrenteses(string expression, char splitChar)
        {
            List<string> result = new List<string>();
            StringBuilder currentParameter = new StringBuilder();
            int nestedParenthesesCount = 0;

            for (int i = 0; i < expression.Length; i++)
            {
                char currentChar = expression[i];

                //Count parenthesis
                if (currentChar == '(')
                {
                    nestedParenthesesCount++;
                }
                else if (currentChar == ')')
                {
                    nestedParenthesesCount--;
                }

                //Not inside (), store
                if (currentChar == splitChar && nestedParenthesesCount == 0)
                {
                    result.Add(currentParameter.ToString().Trim());
                    currentParameter.Clear();
                }
                //Inside (), append for next part
                else
                {
                    currentParameter.Append(currentChar);
                }
            }

            if (currentParameter.Length > 0)
            {
                result.Add(currentParameter.ToString().Trim());
            }

            return result.ToArray();
        }
    }

}
