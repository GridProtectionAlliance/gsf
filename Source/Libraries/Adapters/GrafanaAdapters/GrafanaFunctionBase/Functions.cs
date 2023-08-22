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

namespace GrafanaAdapters.GrafanaFunctions
{
    internal class Functions
    {
        public static DataSourceValueGroup<T>[] ParseFunction<T>(string expression, GrafanaDataSourceBase dataSourceBase, QueryDataHolder queryData)
        {
            (IGrafanaFunction function, string parameterValue) = MatchFunction<T>(expression);

            // Base Case
            if (function == null)
            {
                if (queryData.IsPhasor)
                    return GetPhasor(parameterValue, dataSourceBase, queryData) as DataSourceValueGroup<T>[];
                else
                    return GetDataSource(parameterValue, dataSourceBase, queryData) as DataSourceValueGroup<T>[];
            }

            //Split Parameters
            string[] parsedParameters = ParseParameters(parameterValue);
            List<string> functionParameters = new List<string>(); //Other params
            List<string> queryExpressions = new List<string>(); //Datapoints
            int paramIndex = 0;

            // Seperate datapoints from params
            foreach (string parameter in parsedParameters)
            {
                // Datapoint
                if (function.Parameters[paramIndex] is IParameter<IDataSourceValueGroup>)
                {
                    queryExpressions.Add(parameter);
                }
                // Other Parameter
                else
                {
                    functionParameters.Add(parameter);
                }
                paramIndex++;
            }

            //Fetch datapoints query
            List<DataSourceValueGroup<T>[]> groupedDataValues = new List<DataSourceValueGroup<T>[]>();
            foreach(string query in queryExpressions)
            {
                DataSourceValueGroup<T>[] queryResult = ParseFunction<T>(query ?? "", dataSourceBase, queryData);
                groupedDataValues.Add(queryResult);
            }

            //Regroup datapoints to align for function
            //Ex: [A, B] & [C, D] -> [A, C] & [B, D]
            //If unequal number [A, B] & [C] fill remaining space with last element [A, C] & [B, C]
            List<DataSourceValueGroup<T>[]> regroupedDataValues = RegroupDataValues(groupedDataValues);

            // Apply the function
            List<DataSourceValueGroup<T>> res = new List<DataSourceValueGroup<T>>();
            foreach (DataSourceValueGroup<T>[] dataValues in regroupedDataValues)
            {
                List<IParameter> computeParameters = GenerateParameters(dataSourceBase, function, functionParameters, dataValues, queryData);
                DataSourceValueGroup<T> computedValues;
                if (typeof(T) == typeof(DataSourceValue))
                {
                    computedValues = (DataSourceValueGroup<T>)(object)function.Compute(computeParameters);
                }
                else if (typeof(T) == typeof(PhasorValue))
                {
                    computedValues = (DataSourceValueGroup<T>)(object)function.ComputePhasor(computeParameters);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported type parameter '{typeof(T)}' in Compute method");
                }

                res.Add(computedValues);
            }

            // Return the result to the previous call
            return res.ToArray();
        }

        public static (IGrafanaFunction function, string parameterValue) MatchFunction<T>(string expression)
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

        public static DataSourceValueGroup<PhasorValue>[] GetPhasor(string expression, GrafanaDataSourceBase dataSourceBase, QueryDataHolder queryData)
        {
            if (expression == "" || expression == null)
            {
                return new DataSourceValueGroup<PhasorValue>[0];
            }

            DataSet Metadata = dataSourceBase.Metadata;
            List<DataSourceValueGroup<PhasorValue>> dataSourceValueGroups = new List<DataSourceValueGroup<PhasorValue>>();
            string[] allTargets = expression.Split(';');
            HashSet<string> targetSet = new HashSet<string>();
            Dictionary<int, string> phasorTargets = new Dictionary<int, string>();

            //Loop through all targets
            foreach(string targetLabel in allTargets)
            {
                if (targetLabel.StartsWith("FILTER ", StringComparison.OrdinalIgnoreCase) && AdapterBase.ParseFilterExpression(targetLabel.SplitAlias(out string alias), out string tableName, out string exp, out string sortField, out int takeCount))
                {
                    foreach (DataRow row in dataSourceBase.Metadata.Tables[tableName].Select(exp, sortField).Take(takeCount))
                    {
                        int targetId = Convert.ToInt32(row["ID"]);
                        phasorTargets[Convert.ToInt32(targetId)] = targetLabel;
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
                    phasorTargets[targetId] = targetLabel;
                }
            }

            foreach (KeyValuePair<int, string> item in phasorTargets)
            {
                int phasorId = item.Key;
                string phasorLabel = item.Value;
                //Match phasor id to measurements
                DataRow[] measurementRows = Metadata.Tables["ActiveMeasurements"].Select($"PhasorID = '{phasorId}'");

                if (measurementRows.Length < 2)
                {
                    throw new Exception($"Did not locate both magnitude and longitude for {phasorLabel} with {phasorId} id");
                }

                //Get data
                List<DataSourceValue> magValues = new();
                List<DataSourceValue> angValues = new();
                string[] dataSourceNames = { "", "" };
                foreach (DataRow row in measurementRows)
                {
                    Dictionary<ulong, string> targetMap = new();
                    ulong id = Convert.ToUInt64(row["ID"].ToString().Split(':')[1]);
                    string pointTag = row["PointTag"].ToString();
                    targetMap[id] = pointTag;

                    List<DataSourceValue> dataValues = dataSourceBase.QueryDataSourceValues<DataSourceValue>(queryData.StartTime, queryData.StopTime, queryData.Interval, queryData.IncludePeaks, targetMap)
                        .TakeWhile(_ => !queryData.CancellationToken.IsCancellationRequested).ToList();

                    //Assign to proper values
                    if (pointTag.EndsWith(".MAG"))
                    {
                        magValues = dataValues;
                        dataSourceNames[0] = pointTag;
                    }
                    else if (pointTag.EndsWith(".ANG"))
                    {
                        angValues = dataValues;
                        dataSourceNames[1] = pointTag;
                    }
                    else
                        throw new Exception($"Point format for {pointTag} is neither .MAG nor .ANG");
                }

                //Error check
                //if (magValues.Count != angValues.Count)
                //    throw new Exception($"Number of data points for mag or ang values do not match for {dataSourceNames[0]} and {dataSourceNames[1]}");

                if (dataSourceNames[0] == "")
                    throw new Exception($"Unable to find magnitude name for {phasorLabel}");
                else if (dataSourceNames[1] == "")
                    throw new Exception($"Unable to find angle name for {phasorLabel}");

                //Convert datasource data to phasor data 
                IEnumerable<PhasorValue> phasorValues = GeneratePhasorValues();

                List<PhasorValue> GeneratePhasorValues()
                {
                    List<PhasorValue> phasorValues = new List<PhasorValue>();
                    int index = 0;
                    foreach (DataSourceValue mag in magValues)
                    {
                        if (index >= angValues.Count())
                            continue;

                        PhasorValue phasor = new()
                        {
                            MagnitudeTarget = dataSourceNames[0],
                            AngleTarget = dataSourceNames[1],
                            Flags = mag.Flags,
                            Time = mag.Time,
                            Magnitude = mag.Value,
                            Angle = angValues[index].Value
                        };

                        index++;
                        phasorValues.Add(phasor);
                    }
                    return phasorValues;
                }

                DataSourceValueGroup<PhasorValue> dataSourceValueGroup = new DataSourceValueGroup<PhasorValue>
                {
                    Target = $"{dataSourceNames[0]};{dataSourceNames[1]}",
                    RootTarget = phasorLabel,
                    SourceTarget = queryData.SourceTarget,
                    Source = phasorValues,
                    DropEmptySeries = queryData.DropEmptySeries,
                    refId = queryData.SourceTarget.refId,
                    metadata = GetMetadata(dataSourceBase, phasorLabel, queryData.MetadataSelection, true)
                };

                dataSourceValueGroups.Add(dataSourceValueGroup);
            }


            return dataSourceValueGroups.ToArray();
        }

        public static DataSourceValueGroup<DataSourceValue>[] GetDataSource(string expression, GrafanaDataSourceBase dataSourceBase, QueryDataHolder queryData)
        {
            if(expression == "" || expression == null)
            {
                return new DataSourceValueGroup<DataSourceValue>[0];
            }

            DataSet Metadata = dataSourceBase.Metadata;

            Dictionary<ulong, string> targetMap = new();
            string[] allTargets = expression.Split(';');

            // Expand target set to include point tags for all parsed inputs
            HashSet<string> targetSet = new HashSet<string>();
            foreach (string target in allTargets)
            {
                targetSet.UnionWith(TargetCache<string[]>.GetOrAdd(target, () =>
                {
                    MeasurementKey[] results = AdapterBase.ParseInputMeasurementKeys(Metadata, false, target.SplitAlias(out string alias));

                    if (!string.IsNullOrWhiteSpace(alias) && results.Length == 1)
                        return new[] { $"{alias}={results[0].TagFromKey(Metadata)}" };

                    return results.Select(key => key.TagFromKey(Metadata)).ToArray();
                }));
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
            List<DataSourceValue> dataValues = dataSourceBase.QueryDataSourceValues<DataSourceValue>(queryData.StartTime, queryData.StopTime, queryData.Interval, queryData.IncludePeaks, targetMap)
                .TakeWhile(_ => !queryData.CancellationToken.IsCancellationRequested).ToList();

            DataSourceValueGroup<DataSourceValue>[] dataResult = new DataSourceValueGroup<DataSourceValue>[targetMap.Count];
            int index = 0;

            foreach (KeyValuePair<ulong, string> target in targetMap)
            {
                IEnumerable<DataSourceValue> filteredValues = (IEnumerable<DataSourceValue>)dataValues.OfType<DataSourceValue>()
                        .Where(dataValue => dataValue.Target.Equals(target.Value));

                DataSourceValueGroup<DataSourceValue> dataSourceValueGroup = new DataSourceValueGroup<DataSourceValue>
                {
                    Target = target.Value,
                    RootTarget = target.Value,
                    SourceTarget = queryData.SourceTarget,
                    Source = filteredValues,
                    DropEmptySeries = queryData.DropEmptySeries,
                    refId = queryData.SourceTarget.refId,
                    metadata = GetMetadata(dataSourceBase, target.Value, queryData.MetadataSelection, false)
                };

                dataResult[index] = dataSourceValueGroup;
                index++;
            }

            
            return dataResult;
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


        public static List<IParameter> GenerateParameters<T>(GrafanaDataSourceBase dataSourceBase, IGrafanaFunction function, 
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

        public static string[] ParseParameters(string expression)
        {
            if (expression == null)
            {
                return new string[0];
            }

            List<string> parameters = new List<string>();
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
                if (currentChar == ',' && nestedParenthesesCount == 0)
                {
                    parameters.Add(currentParameter.ToString().Trim());
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
                parameters.Add(currentParameter.ToString().Trim());
            }

            return parameters.ToArray();
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

    }

}
