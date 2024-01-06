using System;
using System.Collections.Generic;
using GrafanaAdapters.DataSources;

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
public abstract class Floor<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Floor<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the smallest integral value that is less than or equal to each of the values in the source series.";

    /// <inheritdoc />
    // Hiding slice operation since result matrix would be the same when tolerance matches data rate
    public override GroupOperations PublishedGroupOperations => GroupOperations.Standard | GroupOperations.Set;

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        return ExecuteFunction(Math.Floor, parameters);
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Floor<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Floor<PhasorValue>
    {
        // Function computed for both magnitude and angle
    }
}