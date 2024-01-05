using System;
using System.Collections.Generic;
using GrafanaAdapters.DataSources;

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
public abstract class Truncate<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Truncate";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the integral part of each of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Trunc" };

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.Standard | GroupOperations.Set;

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        return ExecuteFunction(Math.Truncate, parameters);
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Truncate<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Truncate<PhasorValue>
    {
    }
}