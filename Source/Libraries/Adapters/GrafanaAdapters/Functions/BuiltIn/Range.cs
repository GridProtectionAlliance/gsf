using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the range, i.e., <c>maximum - minimum</c>, of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Range(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Range(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Range<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Range<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Range<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the range, i.e., <c>maximum - minimum</c>, of the values in the source series.";

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Scalar;

    /// <inheritdoc />
    public override bool ResultIsSetTargetSeries => true;

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        T rangeMin = new() { Value = double.MaxValue };
        T rangeMax = new() { Value = double.MinValue };

        // Immediately enumerate values to find range
        await foreach (T dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (dataValue.Value <= rangeMin.Value)
                rangeMin = dataValue;

            if (dataValue.Value >= rangeMax.Value)
                rangeMax = dataValue;
        }

        // Return computed results
        if (rangeMin.Time > 0.0D && rangeMax.Time > 0.0D)
            yield return rangeMax with { Value = rangeMax.Value - rangeMin.Value };
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Range<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Range<PhasorValue>
    {
        // Operating on magnitude only
    }
}