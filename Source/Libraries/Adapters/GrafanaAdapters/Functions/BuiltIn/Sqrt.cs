using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the square root each of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Sqrt(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Sqrt(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Variants: Sqrt<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Sqrt<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Sqrt<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the square root each of the values in the source series.";

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        return ExecuteFunction(Math.Sqrt, parameters);
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Sqrt<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Sqrt<PhasorValue>
    {
        // Function computed for both magnitude and angle
    }
}