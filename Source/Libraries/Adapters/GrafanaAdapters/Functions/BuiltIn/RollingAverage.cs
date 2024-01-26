using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the rolling average of the values in the source series. The <c>windowSize</c> parameter, optional,
/// is a positive integer value representing a total number of data points to use for each of the values in the rolling average results. If no
/// <c>windowSize</c> is provided, the default value is the square root of the total input values in the series. The <c>windowSize</c> can either
/// be constant value or a named target available from the expression. Function operates by producing a mean over each data window.
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
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ResultsLength ResultsLength => ResultsLength.Reduced;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<int>
        {
            Name = "windowSize",
            Default = -1,
            Description = "An integer value representing the window size, in number of points, to use for each rolling average value.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Immediately load values in-memory only enumerating data source once
#if NET
        ReadOnlySpan<T> values = await GetDataSourceValues(parameters).ToArrayAsync(cancellationToken).ConfigureAwait(false);
#else
        T[] values = await GetDataSourceValues(parameters).ToArrayAsync(cancellationToken).ConfigureAwait(false);
#endif
        int length = values.Length;

        if (length == 0)
            yield break;

        int windowSize = parameters.Value<int>(0);

        if (windowSize < 1)
            windowSize = (int)Math.Sqrt(length);

        if (windowSize > length)
            windowSize = length;

        // Calculate the rolling average for each window
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