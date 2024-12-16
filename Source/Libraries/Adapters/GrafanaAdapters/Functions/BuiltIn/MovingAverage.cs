using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the moving average of the values in the source series. The <c>windowSize</c> parameter,
/// optional, is a positive integer value representing a total number of windows to use for the moving average. If no <c>windowSize</c>
/// is provided, the default value is the square root of the total input values in the series. The <c>windowSize</c> can either be a
/// constant value or a named target available from the expression. Function operates using a simple moving average (SMA) algorithm.
/// </summary>
/// <remarks>
/// Signature: <c>MovingAverage([windowSize = sqrt(len)], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>MovingAvg(150, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: MovingAverage, MovingAvg, MovingMean, SimpleMovingAverage, SMA<br/>
/// Execution: Immediate in-memory array load.<br/>
/// Group Operations: Slice, Set
/// </remarks>
public abstract class MovingAverage<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(MovingAverage<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the simple moving average (SMA) of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["MovingAvg", "MovingMean", "SimpleMovingAverage", "SMA"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override bool IsSliceSeriesEquivalent => false;

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.None | GroupOperations.Set;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<int>
        {
            Name = "windowSize",
            Default = -1,
            Description = "An integer value representing the total number of windows to use for the moving average.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Immediately load values in-memory only enumerating data source once
        T[] values = await GetDataSourceValues(parameters).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        int length = values.Length;

        if (length == 0)
            yield break;

        int windowSize = parameters.Value<int>(0);

        if (windowSize < 1)
            windowSize = (int)Math.Sqrt(length);

        if (windowSize > length)
            windowSize = length;

        int windowCount = length - windowSize + 1;
        double windowSum = 0.0D;

        // Initialize the sum of the first window
        for (int i = 0; i < windowSize; i++)
            windowSum += values[i].Value;

        // Calculate the moving average for each window
        for (int i = 0; i < windowCount; i++)
        {
            yield return values[i] with { Value = windowSum / windowSize };

            // Update the sum to reflect the new window
            if (i + windowSize < values.Length)
                windowSum = windowSum - values[i].Value + values[i + windowSize].Value;
        }
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : MovingAverage<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : MovingAverage<PhasorValue>
    {
        // Operating on magnitude only
    }
}