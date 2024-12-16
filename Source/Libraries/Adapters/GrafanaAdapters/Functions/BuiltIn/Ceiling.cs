using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Ceiling(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Ceiling(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Variants: Ceiling, Ceil<br/>
/// Execution: Deferred enumeration.<br/>
/// Group Operations: Slice, Set
/// </remarks>
public abstract class Ceiling<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Ceiling<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Ceil"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        return ExecuteFunction(Math.Ceiling, parameters);
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Ceiling<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Ceiling<PhasorValue>
    {
        // Function computed for both magnitude and angle
    }
}