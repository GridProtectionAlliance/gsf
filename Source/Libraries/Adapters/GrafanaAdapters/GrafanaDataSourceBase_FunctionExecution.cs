//******************************************************************************************************
//  GrafanaDataSourceBase_FunctionExecution.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  09/25/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using GSF;
using GSF.Collections;
using GSF.NumericalAnalysis;
using GSF.Units;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable AccessToModifiedClosure
namespace GrafanaAdapters
{
    partial class GrafanaDataSourceBase
    {
        private class TargetTimeUnit
        {
            public TimeUnit Unit;
            public double Factor = double.NaN;

            public static bool TryParse(string value, out TargetTimeUnit targetTimeUnit)
            {
                if (Enum.TryParse(value, out TimeUnit timeUnit))
                {
                    targetTimeUnit = new TargetTimeUnit
                    {
                        Unit = timeUnit
                    };

                    return true;
                }

                switch (value?.ToLowerInvariant())
                {
                    case "milliseconds":
                        targetTimeUnit = new TargetTimeUnit
                        {
                            Unit = TimeUnit.Seconds,
                            Factor = SI.Milli
                        };

                        return true;
                    case "microseconds":
                        targetTimeUnit = new TargetTimeUnit
                        {
                            Unit = TimeUnit.Seconds,
                            Factor = SI.Micro
                        };

                        return true;
                    case "nanoseconds":
                        targetTimeUnit = new TargetTimeUnit
                        {
                            Unit = TimeUnit.Seconds,
                            Factor = SI.Nano
                        };

                        return true;
                }

                targetTimeUnit = null;
                return false;
            }
        }

        private IEnumerable<DataSourceValueGroup> ExecuteSeriesFunction(Target sourceTarget, Tuple<SeriesFunction, string, GroupOperation> parsedFunction, DateTime startTime, DateTime stopTime, string interval, bool includePeaks, bool dropEmptySeries, CancellationToken cancellationToken)
        {
            SeriesFunction seriesFunction = parsedFunction.Item1;
            string expression = parsedFunction.Item2;
            GroupOperation groupOperation = parsedFunction.Item3;

            // Parse out function parameters and target expression
            Tuple<string[], string> expressionParameters = TargetCache<Tuple<string[], string>>.GetOrAdd(expression, () =>
            {
                List<string> parsedParameters = new List<string>();

                // Extract any required function parameters
                int requiredParameters = s_requiredParameters[seriesFunction]; // Safe: no lock needed since content doesn't change

                // Any slice operation adds one required parameter for time tolerance
                if (groupOperation == GroupOperation.Slice)
                    requiredParameters++;

                if (requiredParameters > 0)
                {
                    int index = 0;

                    for (int i = 0; i < requiredParameters && index > -1; i++)
                        index = expression.IndexOf(',', index + 1);

                    if (index > -1)
                        parsedParameters.AddRange(expression.Substring(0, index).Split(','));

                    if (parsedParameters.Count == requiredParameters)
                        expression = expression.Substring(index + 1).Trim();
                    else
                        throw new FormatException($"Expected {requiredParameters + 1} parameters, received {parsedParameters.Count + 1} in: {(groupOperation == GroupOperation.None ? "" : groupOperation.ToString())}{seriesFunction}({expression})");
                }

                // Extract any provided optional function parameters
                int optionalParameters = s_optionalParameters[seriesFunction]; // Safe: no lock needed since content doesn't change

                bool hasSubExpression(string target) => target.StartsWith("FILTER", StringComparison.OrdinalIgnoreCase) || target.Contains("(");

                if (optionalParameters > 0)
                {
                    int index = expression.IndexOf(',');
                    int lastIndex;

                    if (index > -1 && !hasSubExpression(expression.Substring(0, index)))
                    {
                        lastIndex = index;

                        for (int i = 1; i < optionalParameters && index > -1; i++)
                        {
                            index = expression.IndexOf(',', index + 1);

                            if (index > -1 && hasSubExpression(expression.Substring(lastIndex + 1, index - lastIndex - 1).Trim()))
                            {
                                index = lastIndex;
                                break;
                            }

                            lastIndex = index;
                        }

                        if (index > -1)
                        {
                            parsedParameters.AddRange(expression.Substring(0, index).Split(','));
                            expression = expression.Substring(index + 1).Trim();
                        }
                    }
                }

                return new Tuple<string[], string>(parsedParameters.ToArray(), expression);
            });

            string[] parameters = expressionParameters.Item1;
            string queryExpression = expressionParameters.Item2;   // Final function parameter is always target expression

            // When accurate calculation results are requested, query data source at full resolution
            if (seriesFunction == SeriesFunction.Interval && ParseFloat(parameters[0]) == 0.0D)
                includePeaks = false;

            // Query function expression to get series data
            IEnumerable<DataSourceValueGroup> dataset = QueryTarget(sourceTarget, queryExpression, startTime, stopTime, interval, includePeaks, dropEmptySeries, cancellationToken);

            // Handle label function as a special edge case - group operations on label are ignored
            if (seriesFunction == SeriesFunction.Label)
            {
                // Derive labels
                string label = parameters[0];

                if (label.StartsWith("\"") || label.StartsWith("'"))
                    label = label.Substring(1, label.Length - 2);

                DataSourceValueGroup[] valueGroups = dataset.ToArray();
                string[] seriesLabels = new string[valueGroups.Length];

                for (int i = 0; i < valueGroups.Length; i++)
                {
                    string target = valueGroups[i].RootTarget;

                    seriesLabels[i] = TargetCache<string>.GetOrAdd($"{label}@{target}", () =>
                    {
                        string table, derivedLabel;
                        string[] components = label.Split('.');

                        if (components.Length == 2)
                        {
                            table = components[0].Trim();
                            derivedLabel = components[1].Trim();
                        }
                        else
                        {
                            table = "ActiveMeasurements";
                            derivedLabel = label;
                        }

                        DataRow record = target.MetadataRecordFromTag(Metadata, table);

                        if (record != null && derivedLabel.IndexOf('{') >= 0)
                        {
                            foreach (string fieldName in record.Table.Columns.Cast<DataColumn>().Select(column => column.ColumnName))
                                derivedLabel = derivedLabel.ReplaceCaseInsensitive($"{{{fieldName}}}", record[fieldName].ToString());
                        }

                        // ReSharper disable once AccessToModifiedClosure
                        if (derivedLabel.Equals(label, StringComparison.Ordinal))
                            derivedLabel = $"{label}{(valueGroups.Length > 1 ? $" {i + 1}" : "")}";

                        return derivedLabel;
                    });
                }

                // Verify that all series labels are unique
                if (seriesLabels.Length > 1)
                {
                    HashSet<string> uniqueLabelSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    for (int i = 0; i < seriesLabels.Length; i++)
                    {
                        while (uniqueLabelSet.Contains(seriesLabels[i]))
                            seriesLabels[i] = $"{seriesLabels[i]}\u00A0"; // Suffixing with non-breaking space for label uniqueness

                        uniqueLabelSet.Add(seriesLabels[i]);
                    }
                }

                for (int i = 0; i < valueGroups.Length; i++)
                {
                    yield return new DataSourceValueGroup
                    {
                        Target = seriesLabels[i],
                        RootTarget = valueGroups[i].RootTarget,
                        SourceTarget = sourceTarget,
                        Source = valueGroups[i].Source,
                        DropEmptySeries = dropEmptySeries
                    };
                }
            }
            else
            {
                switch (groupOperation)
                {
                    case GroupOperation.Set:
                    {
                        // Flatten all series into a single enumerable
                        DataSourceValueGroup valueGroup = new DataSourceValueGroup
                        {
                            Target = $"Set{seriesFunction}({string.Join(", ", parameters)}{(parameters.Length > 0 ? ", " : "")}{queryExpression})",
                            RootTarget = queryExpression,
                            SourceTarget = sourceTarget,
                            Source = ExecuteSeriesFunctionOverSource(dataset.AsParallel().WithCancellation(cancellationToken).SelectMany(source => source.Source), seriesFunction, parameters),
                            DropEmptySeries = dropEmptySeries
                        };

                        // Handle edge-case set operations - for these functions there is data in the target series as well
                        if (seriesFunction == SeriesFunction.Minimum || seriesFunction == SeriesFunction.Maximum || seriesFunction == SeriesFunction.Median)
                        {
                            DataSourceValue dataValue = valueGroup.Source.First();
                            valueGroup.Target = $"Set{seriesFunction} = {dataValue.Target}";
                            valueGroup.RootTarget = dataValue.Target;
                        }

                        yield return valueGroup;
                        
                        break;
                    }
                    case GroupOperation.Slice:
                    {
                        TimeSliceScanner scanner = new TimeSliceScanner(dataset, ParseFloat(parameters[0]) / SI.Milli);
                        parameters = parameters.Skip(1).ToArray();

                        foreach (DataSourceValueGroup valueGroup in ExecuteSeriesFunctionOverTimeSlices(scanner, seriesFunction, parameters, cancellationToken))
                        {
                            yield return new DataSourceValueGroup
                            {
                                Target = $"Slice{seriesFunction}({string.Join(", ", parameters)}{(parameters.Length > 0 ? ", " : "")}{valueGroup.Target})",
                                RootTarget = valueGroup.RootTarget ?? valueGroup.Target,
                                SourceTarget = sourceTarget,
                                Source = valueGroup.Source,
                                DropEmptySeries = dropEmptySeries
                            };
                        }

                        break;
                    }
                    default:
                    {
                        foreach (DataSourceValueGroup valueGroup in dataset)
                        {
                            yield return new DataSourceValueGroup
                            {
                                Target = $"{seriesFunction}({string.Join(", ", parameters)}{(parameters.Length > 0 ? ", " : "")}{valueGroup.Target})",
                                RootTarget = valueGroup.RootTarget ?? valueGroup.Target,
                                SourceTarget = sourceTarget,
                                Source = ExecuteSeriesFunctionOverSource(valueGroup.Source, seriesFunction, parameters),
                                DropEmptySeries = dropEmptySeries
                            };
                        }

                        break;
                    }
                }
            }
        }

        // Execute series function over a set of points from each series at the same time-slice
        private static IEnumerable<DataSourceValueGroup> ExecuteSeriesFunctionOverTimeSlices(TimeSliceScanner scanner, SeriesFunction seriesFunction, string[] parameters, CancellationToken cancellationToken)
        {
            IEnumerable<DataSourceValue> readSliceValues()
            {
                while (!scanner.DataReadComplete && !cancellationToken.IsCancellationRequested)
                {
                    foreach (DataSourceValue dataValue in ExecuteSeriesFunctionOverSource(scanner.ReadNextTimeSlice(), seriesFunction, parameters, true))
                        yield return dataValue;
                }
            }

            foreach (IGrouping<string, DataSourceValue> valueGroup in readSliceValues().GroupBy(dataValue => dataValue.Target))
            {
                yield return new DataSourceValueGroup
                {
                    Target = valueGroup.Key,
                    RootTarget = valueGroup.Key,
                    Source = valueGroup
                };
            }
        }

        // Design philosophy: whenever possible this function should delay source enumeration since source data sets could be very large.
        private static IEnumerable<DataSourceValue> ExecuteSeriesFunctionOverSource(IEnumerable<DataSourceValue> source, SeriesFunction seriesFunction, string[] parameters, bool isSliceOperation = false)
        {
            DataSourceValue[] values;
            DataSourceValue result = new DataSourceValue();
            double lastValue = double.NaN;
            double lastTime = 0.0D;
            string lastTarget = null;

            IEnumerable<double> trackedValues = source.Select(dataValue =>
            {
                lastTime = dataValue.Time;
                lastTarget = dataValue.Target;
                return dataValue.Value;
            });

            double baseTime, timeStep, value, low, high;
            bool normalizeTime, lowInclusive, highInclusive;
            int count;
            TargetTimeUnit timeUnit;
            AngleUnit angleUnit;

            switch (seriesFunction)
            {
                case SeriesFunction.Minimum:
                    DataSourceValue minValue = new DataSourceValue { Value = double.MaxValue };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (dataValue.Value <= minValue.Value)
                            minValue = dataValue;
                    }

                    if (minValue.Time > 0.0D)
                        yield return minValue;

                    break;
                case SeriesFunction.Maximum:
                    DataSourceValue maxValue = new DataSourceValue { Value = double.MinValue };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (dataValue.Value >= maxValue.Value)
                            maxValue = dataValue;
                    }

                    if (maxValue.Time > 0.0D)
                        yield return maxValue;

                    break;
                case SeriesFunction.Average:
                    result.Value = trackedValues.Average();
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.Total:
                    result.Value = trackedValues.Sum();
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.Range:
                    DataSourceValue rangeMin = new DataSourceValue { Value = double.MaxValue };
                    DataSourceValue rangeMax = new DataSourceValue { Value = double.MinValue };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (dataValue.Value <= rangeMin.Value)
                            rangeMin = dataValue;

                        if (dataValue.Value >= rangeMax.Value)
                            rangeMax = dataValue;
                    }

                    if (rangeMin.Time > 0.0D && rangeMax.Time > 0.0D)
                    {
                        result = rangeMax;
                        result.Value = rangeMax.Value - rangeMin.Value;
                        yield return result;
                    }
                    break;
                case SeriesFunction.Count:
                    result.Value = trackedValues.Count();
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.Distinct:
                    foreach (DataSourceValue dataValue in source.DistinctBy(dataValue => dataValue.Value))
                        yield return dataValue;

                    break;
                case SeriesFunction.AbsoluteValue:
                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Abs(dataValue.Value), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Add:
                    value = ParseFloat(parameters[0], source, false, isSliceOperation);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = dataValue.Value + value, Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Subtract:
                    value = ParseFloat(parameters[0], source, false, isSliceOperation);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = dataValue.Value - value, Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Multiply:
                    value = ParseFloat(parameters[0], source, false, isSliceOperation);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = dataValue.Value * value, Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Divide:
                    value = ParseFloat(parameters[0], source, false, isSliceOperation);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = dataValue.Value / value, Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Round:
                    count = parameters.Length == 0 ? 0 : ParseInt(parameters[0]);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Round(dataValue.Value, count), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Floor:
                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Floor(dataValue.Value), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Ceiling:
                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Ceiling(dataValue.Value), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Truncate:
                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Truncate(dataValue.Value), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.StandardDeviation:
                    result.Value = trackedValues.StandardDeviation(parameters.Length > 0 && parameters[0].Trim().ParseBoolean());
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.Median:
                    values = source.Median();

                    if (values.Length == 0) //-V3080
                        yield break;

                    result = values.Last();

                    if (values.Length > 1)
                        result.Value = values.Select(dataValue => dataValue.Value).Average();

                    yield return result;
                    break;
                case SeriesFunction.Mode:
                    values = source.ToArray();
                    yield return values.MajorityBy(values.Last(), dataValue => dataValue.Value, false);
                    break;
                case SeriesFunction.Top:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = ParseCount(parameters[0], values, isSliceOperation);

                    if (count > values.Length)
                        count = values.Length;

                    normalizeTime = parameters.Length == 1 || parameters[1].Trim().ParseBoolean();
                    baseTime = values[0].Time;
                    timeStep = (values[values.Length - 1].Time - baseTime) / (count - 1).NotZero(1);
                    Array.Sort(values, (a, b) => a.Value < b.Value ? -1 : (a.Value > b.Value ? 1 : 0));

                    foreach (DataSourceValue dataValue in values.Take(count).Select((dataValue, i) => new DataSourceValue { Value = dataValue.Value, Time = normalizeTime ? baseTime + i * timeStep : dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Bottom:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = ParseCount(parameters[0], values, isSliceOperation);

                    if (count > values.Length)
                        count = values.Length;

                    normalizeTime = parameters.Length == 1 || parameters[1].Trim().ParseBoolean();
                    baseTime = values[0].Time;
                    timeStep = (values[values.Length - 1].Time - baseTime) / (count - 1).NotZero(1);
                    Array.Sort(values, (a, b) => a.Value > b.Value ? -1 : (a.Value < b.Value ? 1 : 0));

                    foreach (DataSourceValue dataValue in values.Take(count).Select((dataValue, i) => new DataSourceValue { Value = dataValue.Value, Time = normalizeTime ? baseTime + i * timeStep : dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Random:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = ParseCount(parameters[0], values, isSliceOperation);

                    if (count > values.Length)
                        count = values.Length;

                    normalizeTime = parameters.Length == 1 || parameters[1].Trim().ParseBoolean();
                    baseTime = values[0].Time;
                    timeStep = (values[values.Length - 1].Time - baseTime) / (count - 1).NotZero(1);
                    List<int> indexes = new List<int>(Enumerable.Range(0, values.Length));
                    indexes.Scramble();

                    foreach (DataSourceValue dataValue in indexes.Take(count).Select((index, i) => new DataSourceValue { Value = values[index].Value, Time = normalizeTime ? baseTime + i * timeStep : values[index].Time, Target = values[index].Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.First:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = parameters.Length == 0 ? 1 : ParseCount(parameters[0], values, isSliceOperation);

                    if (count > values.Length)
                        count = values.Length;

                    for (int i = 0; i < count; i++)
                        yield return values[i];

                    break;
                case SeriesFunction.Last:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = parameters.Length == 0 ? 1 : ParseCount(parameters[0], values, isSliceOperation);

                    if (count > values.Length)
                        count = values.Length;

                    for (int i = 0; i < count; i++)
                        yield return values[values.Length - 1 - i];

                    break;
                case SeriesFunction.Percentile:
                    double percent = ParsePercentage(parameters[0]);
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    Array.Sort(values, (a, b) => a.Value < b.Value ? -1 : (a.Value > b.Value ? 1 : 0));
                    count = values.Length;

                    if (percent == 0.0D)
                    {
                        yield return values.First();
                    }
                    else if (percent == 100.0D)
                    {
                        yield return values.Last();
                    }
                    else
                    {
                        double n = (count - 1) * (percent / 100.0D) + 1.0D;
                        int k = (int)n;
                        DataSourceValue kData = values[k];
                        double d = n - k;
                        double k0 = values[k - 1].Value;
                        double k1 = kData.Value;

                        result.Value = k0 + d * (k1 - k0);
                        result.Time = kData.Time;
                        result.Target = kData.Target;
                        yield return result;
                    }
                    break;
                case SeriesFunction.Difference:
                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                            yield return new DataSourceValue { Value = dataValue.Value - lastValue, Time = dataValue.Time, Target = lastTarget };

                        lastValue = dataValue.Value;
                        lastTime = dataValue.Time;
                        lastTarget = dataValue.Target;
                    }
                    break;
                case SeriesFunction.TimeDifference:
                    if (parameters.Length == 0 || !TargetTimeUnit.TryParse(parameters[0], out timeUnit))
                        timeUnit = new TargetTimeUnit { Unit = TimeUnit.Seconds };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                            yield return new DataSourceValue { Value = ToTimeUnits((dataValue.Time - lastTime) * SI.Milli, timeUnit), Time = dataValue.Time, Target = lastTarget };

                        lastTime = dataValue.Time;
                        lastTarget = dataValue.Target;
                    }
                    break;
                case SeriesFunction.Derivative:
                    if (parameters.Length == 0 || !TargetTimeUnit.TryParse(parameters[0], out timeUnit))
                        timeUnit = new TargetTimeUnit { Unit = TimeUnit.Seconds };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                            yield return new DataSourceValue { Value = (dataValue.Value - lastValue) / ToTimeUnits((dataValue.Time - lastTime) * SI.Milli, timeUnit), Time = dataValue.Time, Target = lastTarget };

                        lastValue = dataValue.Value;
                        lastTime = dataValue.Time;
                        lastTarget = dataValue.Target;
                    }
                    break;
                case SeriesFunction.TimeIntegration:
                    if (parameters.Length == 0 || !TargetTimeUnit.TryParse(parameters[0], out timeUnit))
                        timeUnit = new TargetTimeUnit { Unit = TimeUnit.Hours };

                    result.Value = 0.0D;

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                            result.Value += dataValue.Value * ToTimeUnits((dataValue.Time - lastTime) * SI.Milli, timeUnit);

                        lastTime = dataValue.Time;
                        lastTarget = dataValue.Target;
                    }

                    if (lastTime > 0.0D)
                    {
                        result.Time = lastTime;
                        result.Target = lastTarget;
                        yield return result;
                    }
                    break;
                case SeriesFunction.Interval:
                    if (parameters.Length == 1 || !TargetTimeUnit.TryParse(parameters[1], out timeUnit))
                        timeUnit = new TargetTimeUnit { Unit = TimeUnit.Seconds };

                    value = FromTimeUnits(ParseFloat(parameters[0], source, true, isSliceOperation), timeUnit) / SI.Milli;

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                        {
                            if (dataValue.Time - lastTime > value)
                            {
                                lastTime = dataValue.Time;
                                yield return dataValue;
                            }
                        }
                        else
                        {
                            lastTime = dataValue.Time;
                            yield return dataValue;
                        }
                    }
                    break;
                case SeriesFunction.IncludeRange:
                    low = ParseFloat(parameters[0], source, false, isSliceOperation);
                    high = ParseFloat(parameters[1], source, false, isSliceOperation);
                    lowInclusive = parameters.Length > 2 && parameters[2].Trim().ParseBoolean();
                    highInclusive = parameters.Length > 3 ? parameters[3].Trim().ParseBoolean() : lowInclusive;

                    foreach (DataSourceValue dataValue in source.Where(dataValue => (lowInclusive ? dataValue.Value >= low : dataValue.Value > low) && (highInclusive ? dataValue.Value <= high : dataValue.Value < high)))
                        yield return dataValue;

                    break;
                case SeriesFunction.ExcludeRange:
                    low = ParseFloat(parameters[0], source, false, isSliceOperation);
                    high = ParseFloat(parameters[1], source, false, isSliceOperation);
                    lowInclusive = parameters.Length > 2 && parameters[2].Trim().ParseBoolean();
                    highInclusive = parameters.Length > 3 ? parameters[3].Trim().ParseBoolean() : lowInclusive;

                    foreach (DataSourceValue dataValue in source.Where(dataValue => (lowInclusive ? dataValue.Value <= low : dataValue.Value < low) || (highInclusive ? dataValue.Value >= high : dataValue.Value > high)))
                        yield return dataValue;

                    break;
                case SeriesFunction.FilterNaN:
                    bool alsoFilterInifinity = parameters.Length == 0 || parameters[0].Trim().ParseBoolean();

                    foreach (DataSourceValue dataValue in source.Where(dataValue => !(double.IsNaN(dataValue.Value) || alsoFilterInifinity && double.IsInfinity(dataValue.Value)))) //-V3130
                        yield return dataValue;

                    break;
                case SeriesFunction.UnwrapAngle:
                    if (parameters.Length == 0 || !Enum.TryParse(parameters[0], true, out angleUnit))
                        angleUnit = AngleUnit.Degrees;

                    values = source.ToArray();

                    foreach (DataSourceValue dataValue in Angle.Unwrap(values.Select(dataValue => Angle.ConvertFrom(dataValue.Value, angleUnit))).Select((angle, index) => new DataSourceValue { Value = angle.ConvertTo(angleUnit), Time = values[index].Time, Target = values[index].Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.WrapAngle:
                    if (parameters.Length == 0 || !Enum.TryParse(parameters[0], true, out angleUnit))
                        angleUnit = AngleUnit.Degrees;

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Angle.ConvertFrom(dataValue.Value, angleUnit).ToRange(-Math.PI, false).ConvertTo(angleUnit), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
            }
        }

        private static Time FromTimeUnits(double value, TargetTimeUnit target)
        {
            double time = Time.ConvertFrom(value, target.Unit);

            if (!double.IsNaN(target.Factor))
                time *= target.Factor;

            return time;
        }

        private static double ToTimeUnits(Time value, TargetTimeUnit target)
        {
            double time = value.ConvertTo(target.Unit);

            if (!double.IsNaN(target.Factor))
                time /= target.Factor;

            return time;
        }

        private static int ParseInt(string parameter, bool includeZero = true)
        {
            parameter = parameter.Trim();

            if (!int.TryParse(parameter, out int value))
                throw new FormatException($"Could not parse '{parameter}' as an integer value.");

            if (includeZero)
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than zero.");
            }
            else
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than or equal to zero.");
            }

            return value;
        }

        private static double ParseFloat(string parameter, IEnumerable<DataSourceValue> source = null, bool validateGTEZero = true, bool isSliceOperation = false)
        {
            double value;

            parameter = parameter.Trim();

            Tuple<bool, double> cache = TargetCache<Tuple<bool, double>>.GetOrAdd(parameter, () =>
            {
                bool success = double.TryParse(parameter, out double result);
                return new Tuple<bool, double>(success, result);
            });

            if (cache.Item1)
            {
                value = cache.Item2;
            }
            else
            {
                if (source == null)
                    throw new FormatException($"Could not parse '{parameter}' as a floating-point value.");

                double defaultValue = 0.0D;
                bool hasDefaultValue = false;

                if (parameter.IndexOf(';') > -1)
                {
                    string[] parts = parameter.Split(';');

                    if (parts.Length >= 2)
                    {
                        parameter = parts[0].Trim();
                        defaultValue = ParseFloat(parts[1], source, validateGTEZero, isSliceOperation);
                        hasDefaultValue = true;
                    }
                }

                DataSourceValue result = source.FirstOrDefault(dataValue => dataValue.Target.Equals(parameter, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(result.Target))
                {
                    // Slice operations may not have a target for a given slice - in this case function should use a default value and not fail
                    if (isSliceOperation || hasDefaultValue)
                        result.Value = defaultValue;
                    else
                        throw new FormatException($"Value target '{parameter}' could not be found in dataset nor parsed as a floating-point value.");
                }

                value = result.Value;
            }

            if (validateGTEZero)
            {
                if (value < 0.0D)
                    throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than zero.");
            }

            return value;
        }

        private static int ParseCount(string parameter, DataSourceValue[] values, bool isSliceOperation)
        {
            int length = values.Length;
            int count;

            if (length == 0)
                return 0;

            parameter = parameter.Trim();

            Tuple<bool, int> cache = TargetCache<Tuple<bool, int>>.GetOrAdd(parameter, () =>
            {
                bool success = true;
                int result;

                if (parameter.EndsWith("%"))
                {
                    try
                    {
                        double percent = ParsePercentage(parameter, false);
                        result = (int)(length * (percent / 100.0D));

                        if (result == 0)
                            result = 1;
                    }
                    catch
                    {
                        success = false;
                        result = 0;
                    }
                }
                else
                {
                    success = int.TryParse(parameter, out result);
                }

                return new Tuple<bool, int>(success, result);
            });

            if (cache.Item1)
            {
                count = cache.Item2;
            }
            else
            {
                if (parameter.EndsWith("%"))
                    throw new ArgumentOutOfRangeException($"Could not parse '{parameter}' as a floating-point value or percentage is outside range of greater than 0 and less than or equal to 100.");

                double defaultValue = 1.0D;
                bool hasDefaultValue = false;

                if (parameter.IndexOf(';') > -1)
                {
                    string[] parts = parameter.Split(';');

                    if (parts.Length >= 2)
                    {
                        parameter = parts[0].Trim();
                        defaultValue = ParseCount(parts[1], values, isSliceOperation);
                        hasDefaultValue = true;
                    }
                }

                DataSourceValue result = values.FirstOrDefault(dataValue => dataValue.Target.Equals(parameter, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(result.Target))
                {
                    // Slice operations may not have a target for a given slice - in this case function should use a default value and not fail
                    if (isSliceOperation || hasDefaultValue)
                        result.Value = defaultValue;
                    else
                        throw new FormatException($"Value target '{parameter}' could not be found in dataset nor parsed as an integer value.");
                }

                // Treat fractional numbers as a percentage of length
                if (result.Value > 0.0D && result.Value < 1.0D)
                    count = (int)(length * result.Value);
                else
                    count = (int)result.Value;
            }

            if (count < 1)
                throw new ArgumentOutOfRangeException($"Count '{count}' is less than one.");

            return count;
        }

        private static double ParsePercentage(string parameter, bool includeZero = true)
        {
            parameter = parameter.Trim();

            if (parameter.EndsWith("%"))
                parameter = parameter.Substring(0, parameter.Length - 1);

            if (!double.TryParse(parameter, out double percent))
                throw new FormatException($"Could not parse '{parameter}' as a floating-point value.");

            if (includeZero)
            {
                if (percent < 0.0D || percent > 100.0D)
                    throw new ArgumentOutOfRangeException($"Percentage '{parameter}' is outside range of 0 to 100, inclusive.");
            }
            else
            {
                if (percent <= 0.0D || percent > 100.0D)
                    throw new ArgumentOutOfRangeException($"Percentage '{parameter}' is outside range of greater than 0 and less than or equal to 100.");
            }

            return percent;
        }

    }
}
