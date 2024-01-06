using System;
using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the rolling average of the values in the source series.
/// The windowSize parameter, optional, is a positive integer value representing a total number of windows
/// to use for the rolling average. If no windowSize is provided, the default value is the square root of
/// the total input values in the series. The windowSize can either be constant value or a named target
/// available from the expression. Function operates by producing a mean over each data window.
/// </summary>
/// <remarks>
/// Signature: <c>RollingAverage([windowSize = sqrt(len)], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>RollingAvg(150, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: RollingAverage, RollingAvg, RollingMean<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class RollingAverage<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(RollingAverage<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the rolling average of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "RollingAvg", "RollingMean" };

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<int>
        {
            Name = "windowSize",
            Default = -1,
            Description = "An integer value representing the total number of windows to use for the rolling average.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        IEnumerable<T> source = GetDataSourceValues(parameters);

        // Immediately load values in-memory only enumerating data source once
    #if NET
        ReadOnlySpan<T> values = source.ToArray();
    #else
        T[] values = source.ToArray();
    #endif
        int length = values.Length;

        if (length == 0)
            yield break;

        int windowSize = parameters.Value<int>(0);

        if (windowSize < 1)
            windowSize = (int)Math.Sqrt(length);

        if (windowSize > length)
            windowSize = length;

        // Calculate the moving average for each window
        for (int i = 0; i < length; i += windowSize) 
        {
            yield return values[i] with
            {
            #if NET
                Value = values[i..windowSize].Average(dataValue => dataValue.Value)
            #else
                Value = values.Skip(i).Take(windowSize).Average(dataValue => dataValue.Value)
            #endif
            };
        }
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : RollingAverage<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : RollingAverage<PhasorValue>
    {
        // Operating on magnitude only
    }
}