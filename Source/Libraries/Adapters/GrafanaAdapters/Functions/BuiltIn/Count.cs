using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that is the count of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Count(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Count(PPA:1; PPA:2; PPA:3)</c><br/>
/// Variants: Count<br/>
/// Execution: Immediate enumeration.<br/>
/// Group Operations: Slice, Set
/// </remarks>
public abstract class Count<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Count<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that is the count of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Length"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Scalar;

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        T lastValue = default;

        IAsyncEnumerable<int> trackedValues = GetDataSourceValues(parameters).Select(dataValue =>
        {
            lastValue = dataValue;
            return 0;
        });

        // Immediately enumerate to compute values
        double count = await trackedValues.CountAsync(cancellationToken).ConfigureAwait(false);

        // Return computed results
        if (lastValue.Time > 0.0D)
            yield return lastValue with { Value = count };
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Count<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Count<PhasorValue>
    {
    }
}