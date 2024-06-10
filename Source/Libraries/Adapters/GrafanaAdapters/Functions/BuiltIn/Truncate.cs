using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the integral part of each of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Truncate(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Truncate(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Variants: Truncate, Trunc<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Truncate<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Truncate<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the integral part of each of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Trunc"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        return ExecuteFunction(Math.Truncate, parameters);
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Truncate<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Truncate<PhasorValue>
    {
        // Function computed for both magnitude and angle
    }
}