using System;
using System.Collections.Generic;
using GrafanaAdapters.DataSources;

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
public abstract class AbsoluteValue<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "AbsoluteValue";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the absolute value each of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Abs" };

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.Standard | GroupOperations.Set;

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        return ExecuteFunction(Math.Abs, parameters);
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : AbsoluteValue<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : AbsoluteValue<PhasorValue>
    {
    }
}
