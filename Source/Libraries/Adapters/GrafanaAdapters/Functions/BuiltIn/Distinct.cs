using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the unique set of values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Distinct(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Distinct(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Distinct, Unique<br/>
/// Execution: Deferred enumeration.<br/>
/// Group Operations: Slice, Set
/// </remarks>
public abstract class Distinct<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Distinct<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the unique set of values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Unique"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override bool IsSliceSeriesEquivalent => false;

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        // Return deferred enumeration of distinct -- this operates using IEqualityComparer<T> defined for T meaning
        // the IEquatable<T>.Equals(T) method as implemented by IDataSourceValueType<T> is used to determine equality
        return GetDataSourceValues(parameters).Distinct();
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Distinct<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Distinct<PhasorValue>
    {
        // Operating on magnitude only
    }
}