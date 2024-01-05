using System;
using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.DataSources;
using GSF;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of N, or N% of total, values that are the largest in the source series.
/// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
/// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100.
/// Third parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.
/// N can either be constant value or a named target available from the expression. Any target values that fall between 0
/// and 1 will be treated as a percentage.
/// </summary>
/// <remarks>
/// Signature: <c>Top(N|N%, [normalizeTime = true], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Top(50%, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Top, Largest<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class Top<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Top";

    /// <inheritdoc />
    public override string Description => "Returns a series of N, or N% of total, values that are the largest in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Largest" };

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<string>
        {
            Name = "N",
            Default = "1",
            Description = "A integer value or percent representing number or % of elements to take.",
            Required = true
        },
        new ParameterDefinition<bool>
        {
            Name = "normalizeTime",
            Default = true,
            Description = "A boolean flag which representing if time in dataset should be normalized.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        // Immediately load values in-memory only enumerating data source once
        T[] values = GetDataSourceValues(parameters).ToArray();

        if (values.Length == 0)
            yield break;

        int valueN = ParseTotal(parameters.Value<string>(0), values.Length);

        if (valueN > values.Length)
            valueN = values.Length;

        bool normalizeTime = parameters.Value<bool>(1);
        double baseTime = values[0].Time;
        double timeStep = (values[values.Length - 1].Time - baseTime) / (valueN - 1).NotZero(1);
        Array.Sort(values, (a, b) => a.Value < b.Value ? -1 : a.Value > b.Value ? 1 : 0);

        T transposeTime(T dataValue, int index) => dataValue with
        {
            Time = normalizeTime ? baseTime + index * timeStep : dataValue.Time
        };

        // Return immediate enumeration of computed values
        foreach (T dataValue in values.Take(valueN).Select(transposeTime))
            yield return dataValue;
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Top<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Top<PhasorValue>
    {
    }
}