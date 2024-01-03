using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.Collections;

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
    public override string Name => "Distinct";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the unique set of values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Unique" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Distinct<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Return deferred enumeration of distinct values
            return GetDataSourceValues(parameters).DistinctBy(dataValue => dataValue.Value);
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Distinct<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Return deferred enumeration of distinct magnitudes
            return GetDataSourceValues(parameters).DistinctBy(dataValue => dataValue.Magnitude);
        }
    }
}