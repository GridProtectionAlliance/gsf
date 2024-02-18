using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the smallest integral value that is less than or equal to each of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Floor(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Floor(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Variants: Floor<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Floor<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Floor<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the smallest integral value that is less than or equal to each of the values in the source series.";

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        return ExecuteFunction(Math.Floor, parameters);
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Floor<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Floor<PhasorValue>
    {
        // Function computed for both magnitude and angle
    }
}