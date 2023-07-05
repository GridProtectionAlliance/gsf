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

namespace GrafanaAdapters.GrafanaFunctions
{
    internal class Functions
    {
        public static DataSourceValueGroup[] ParseFunction(string expression, GrafanaDataSourceBase dataSourceBase, QueryDataHolder queryData)
        {
            (IGrafanaFunction function, string parameterValue) = MatchFunction(expression);

            // Base Case
            if (function == null)
            {
                return GetDataSourceValue(parameterValue, dataSourceBase, queryData);
            }

            //Split Parameters
            string[] parsedParameters = ParseParameters(parameterValue);
            List<string> functionParameters = new List<string>();
            List<string> queryExpressions = new List<string>();
            int paramIndex = 0;

            // Seperate datapoints from params
            foreach (string parameter in parsedParameters)
            {
                // Datapoint
                if (function.Parameters[paramIndex] is IParameter<IEnumerable<DataSourceValue>>)
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

            List<DataSourceValueGroup[]> groupedDataValues = new List<DataSourceValueGroup[]>();
            foreach(string query in queryExpressions)
            {
                DataSourceValueGroup[] queryResult = ParseFunction(query ?? "", dataSourceBase, queryData);
                groupedDataValues.Add(queryResult);
            }

            List<DataSourceValueGroup[]> regroupedDataValues = RegroupDataValues(groupedDataValues);

            //if (parsedParameters.Length > 1)
            //    functionParameters = parsedParameters.Take(parsedParameters.Length - 1).ToArray();
            //else
            //    functionParameters = new string[0]; 

            //string functionQuery = parsedParameters.Last();

            // Recursive call to parse the nested function
            //CALL RECURSIVELY FOR TYPE DATA
            //SliceAdd(0.033,DATA!GPA_BIRMINGHAM:115KV_LINE1_IB_IB.MAG,DATA!GPA_BIRMINGHAM:115KV_LINE1_IB_IB.MAG)
            //tolerance -> default 0.033 and move to end
            //DataSourceValueGroup[] nestedResult = ParseFunction(functionQuery ?? "", dataSourceBase, queryData);

            // Apply the compute
            List<DataSourceValueGroup> res = new List<DataSourceValueGroup>();
            foreach (DataSourceValueGroup[] dataValues in regroupedDataValues)
            {
                List<IParameter> computeParameters = GenerateParameters(dataSourceBase, function, functionParameters, dataValues);
                IEnumerable<DataSourceValue> computedValues = function.Compute(computeParameters);

                //Take first 
                dataValues[0].Source = computedValues;
                res.Add(dataValues[0]);
            }

            // Return the result to the previous call
            return res.ToArray();
        }

        public static (IGrafanaFunction function, string parameterValue) MatchFunction(string expression)
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

        public static DataSourceValueGroup[] GetDataSourceValue(string expression, GrafanaDataSourceBase dataSourceBase, QueryDataHolder queryData)
        {
            if(expression == "" || expression == null)
            {
                return new DataSourceValueGroup[0];
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
            List<DataSourceValue> dataValues = dataSourceBase.QueryDataSourceValues(queryData.StartTime, queryData.StopTime, queryData.Interval, queryData.IncludePeaks, targetMap)
                .TakeWhile(_ => !queryData.CancellationToken.IsCancellationRequested).ToList();



            DataSourceValueGroup[] dataResult = new DataSourceValueGroup[targetMap.Count];
            int index = 0;

            foreach (KeyValuePair<ulong, string> target in targetMap)
            {
                DataSourceValueGroup dataSourceValueGroup = new DataSourceValueGroup
                {
                    Target = target.Value,
                    RootTarget = target.Value,
                    SourceTarget = queryData.SourceTarget,
                    Source = dataValues.Where(dataValue => dataValue.Target.Equals(target.Value)),
                    DropEmptySeries = queryData.DropEmptySeries,
                    refId = queryData.SourceTarget.refId,
                    metadata = GetMetadata(dataSourceBase, target.Value, queryData.MetadataSelection)
                };

                dataResult[index] = dataSourceValueGroup;
                index++;
            }

            
            return dataResult;
        }

        public static Dictionary<string, string> GetMetadata(GrafanaDataSourceBase dataSourceBase, string rootTarget, Dictionary<string, List<string>> metadataSelection)
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
                DataRow[] rows = dataSourceBase?.Metadata.Tables[table].Select($"PointTag = '{rootTarget}'") ?? new DataRow[0];

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


        public static List<IParameter> GenerateParameters(GrafanaDataSourceBase dataSourceBase, 
            IGrafanaFunction function, List<string> parsedParameters, DataSourceValueGroup[] dataValues)
        {
            List<IParameter> parameters = function.Parameters;
            int paramIndex = 0;
            int dataIndex = 0;
            foreach (IParameter parameter in parameters)
            {
                //Data
                if (parameter is Parameter<IEnumerable<DataSourceValue>> dataSourceValueParameter)
                {
                    //Not enough parameters 
                    if (dataIndex >= dataValues.Length)
                    {
                        dataSourceValueParameter.SetValue(dataSourceBase, null, null);
                    }
                    else
                    {
                        dataSourceValueParameter.SetValue(dataSourceBase, dataValues[dataIndex].Source, dataValues[dataIndex].Target);
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
                        parameter.SetValue(dataSourceBase, null, null); 
                    }
                    //Have a valid parameter
                    else
                    {
                        parameter.SetValue(dataSourceBase, parsedParameters[paramIndex], dataValues[0].Target);
                        paramIndex++;
                    }
                }
            }

            return parameters;
        }

        public static object[] GenerateParameters(IGrafanaFunction function, string[] parsedParameters, IEnumerable<DataSourceValue> dataPoints)
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

        public static List<DataSourceValueGroup[]> RegroupDataValues(List<DataSourceValueGroup[]> groupedDataValues)
        {
            List<DataSourceValueGroup[]> result = new List<DataSourceValueGroup[]>();

            //Empty list
            if (groupedDataValues.Count == 0)
            {
                return result;
            }

            int arrayLength = groupedDataValues.Any() ? groupedDataValues.Max(arr => arr.Length) : 0;
            int groupCount = groupedDataValues.Count;

            for (int i = 0; i < arrayLength; i++)
            {
                DataSourceValueGroup[] newGroup = new DataSourceValueGroup[groupCount];

                for (int j = 0; j < groupCount; j++)
                {
                    if (i < groupedDataValues[j].Length)
                    {
                        newGroup[j] = groupedDataValues[j][i];
                    }
                    //Out of bounds
                    else
                    { 
                        newGroup[j] = null;
                    }
                }

                result.Add(newGroup);
            }


            return result;
        }
    }

}
