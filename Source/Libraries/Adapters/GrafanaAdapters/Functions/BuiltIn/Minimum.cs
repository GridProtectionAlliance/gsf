using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that is the minimum of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Minimum(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Minimum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Minimum, Min<br/>
/// Execution: Immediate enumeration.<br/>
/// Group Operations: Slice, Set
/// </remarks>
public abstract class Minimum<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Minimum<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that is the minimum of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Min"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Scalar;

    /// <inheritdoc />
    public override bool ResultIsSetTargetSeries => true;

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        T minValue = new() { Value = double.MaxValue };

        // Immediately enumerate values to find minimum
        await foreach (T dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (dataValue.Value <= minValue.Value)
                minValue = dataValue;
        }

        // Return computed results
        if (minValue.Time > 0.0D)
            yield return minValue;
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Minimum<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Minimum<PhasorValue>
    {
        // Operating on magnitude only
    }
}