using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GrafanaAdapters.GrafanaFunctions;
using GSF.TimeSeries;

namespace GrafanaAdapters.GrafanaFunctions
{
    internal class Functions
    {
        public static DataSourceValueGroup[] ParseFunction(string expression, GrafanaDataSourceBase dataSourceBase, QueryDataHolder queryData)
        {
            (IFunctionsModel function, string parameterValue) = MatchFunction(expression);

            // Base Case
            if (function == null)
            {
                return GetDataSourceValue(parameterValue, dataSourceBase, queryData);
            }

            string[] parsedParameters = ParseParameters(parameterValue);

            // Recursive call to parse the nested function
            DataSourceValueGroup[] nestedResult = ParseFunction(parsedParameters?.LastOrDefault() ?? "", dataSourceBase, queryData);

            // Apply the compute function
            for (int i = 0; i < nestedResult.Length; i++)
            {
                IEnumerable<DataSourceValue> computedValues = function.Compute(parsedParameters, nestedResult[i].Source);

                nestedResult[i].Source = computedValues;
            }

            // Return the result to the previous call
            return nestedResult;
        }



        public static (IFunctionsModel function, string parameterValue) MatchFunction(string expression)
        {
            foreach (IFunctionsModel function in FunctionsBase.GrafanaFunctions)
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

            // Reduce all targets down to a dictionary of point ID's mapped to point tags
            foreach (string target in allTargets)
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

        public static string[] ParseParameters(string expression)
        {
            if (expression == null)
            {
                return new string[0];
            }

            int indexOpenBracket = expression.IndexOf('(');
            int indexComma = expression.IndexOf(',');

            // If '(' doesn't exist or is after ',' then split by comma
            if ((indexOpenBracket == -1 || indexComma < indexOpenBracket) && indexComma != -1)
            {
                return expression.Split(',');
            }

            // Otherwise return the whole string as a single item array
            return new string[] { expression };
        }
    }

}
