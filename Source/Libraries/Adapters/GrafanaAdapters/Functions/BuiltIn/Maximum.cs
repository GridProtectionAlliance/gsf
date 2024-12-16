using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that is the maximum of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Maximum(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Maximum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Maximum, Max<br/>
/// Execution: Immediate enumeration.<br/>
/// Group Operations: Slice, Set
/// </remarks>
public abstract class Maximum<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Maximum<T>);

    /// <inheritdoc />
    public override string Description => " Returns a single value that is the maximum of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Max"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Scalar;

    /// <inheritdoc />
    public override bool ResultIsSetTargetSeries => true;

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        T maxValue = new() { Value = double.MinValue };

        // Immediately enumerate values to find maximum
        await foreach (T dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (dataValue.Value >= maxValue.Value)
                maxValue = dataValue;
        }

        // Return computed results
        if (maxValue.Time > 0.0D)
            yield return maxValue;
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Maximum<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Maximum<PhasorValue>
    {
        // Operating on magnitude only
    }
}