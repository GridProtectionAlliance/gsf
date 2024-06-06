using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the absolute value each of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>AbsoluteValue(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>AbsoluteValue(FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
/// Variants: AbsoluteValue, Abs<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class AbsoluteValue<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(AbsoluteValue<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the absolute value each of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Abs"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        return ExecuteFunction(Math.Abs, parameters);
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : AbsoluteValue<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : AbsoluteValue<PhasorValue>
    {
        // Function computed for both magnitude and angle
    }
}
