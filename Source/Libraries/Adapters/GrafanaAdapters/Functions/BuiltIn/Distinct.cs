using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
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
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Distinct<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Distinct<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the unique set of values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Unique" };

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        // Return deferred enumeration of distinct values
        return GetDataSourceValues(parameters).Distinct();
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Distinct<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Distinct<PhasorValue>
    {
        // Operating on magnitude only
    }
}