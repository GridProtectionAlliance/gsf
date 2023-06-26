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
            string[] functionParameters;
            if (parsedParameters.Length > 1)
                functionParameters = parsedParameters.Take(parsedParameters.Length - 1).ToArray();
            else
                functionParameters = new string[0]; 

            string functionQuery = parsedParameters.Last();

            // Recursive call to parse the nested function
            DataSourceValueGroup[] nestedResult = ParseFunction(functionQuery ?? "", dataSourceBase, queryData);

            // Apply the compute function
            for (int i = 0; i < nestedResult.Length; i++)
            {
                object[] computeParameters = GenerateParameters(function, functionParameters, nestedResult[i].Source);
                IEnumerable<DataSourceValue> computedValues = function.Compute(computeParameters); 

                nestedResult[i].Source = computedValues;
            }

            // Return the result to the previous call
            return nestedResult;
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
                    refId = queryData.SourceTarget.refId
                };

                dataResult[index] = dataSourceValueGroup;
                index++;
            }

            return dataResult;
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

            int indexOpenBracket = expression.IndexOf('(');

            // If '(' doesn't exist, return split
            if (indexOpenBracket == -1)
            {
                return expression.Split(',');
            }

            string parametersSubstring = expression.Substring(0, indexOpenBracket);

            // Split the substring by comma
            string[] parameters = parametersSubstring.Split(',');

            if (parameters.Length > 0 && !string.IsNullOrWhiteSpace(parameters[parameters.Length - 1]))
            {
                // Last item is not empty or whitespace, append the remaining expression
                parameters[parameters.Length - 1] += expression.Substring(indexOpenBracket);
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
    }

}
