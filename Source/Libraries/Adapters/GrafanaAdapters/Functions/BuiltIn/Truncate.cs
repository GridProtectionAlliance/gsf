using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public class ComputeDataSourceValue : Truncate<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Transpose computed value
            DataSourceValue transposeCompute(DataSourceValue dataValue) => dataValue with
            {
                Value = Math.Truncate(dataValue.Value)
            };

            // Return deferred enumeration of computed values
            foreach (DataSourceValue dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
                yield return dataValue;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Truncate<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Transpose computed value
            PhasorValue transposeCompute(PhasorValue dataValue) => dataValue with
            {
                Magnitude = Math.Truncate(dataValue.Magnitude),
                Angle = Math.Truncate(dataValue.Angle),
            };

            // Return deferred enumeration of computed values
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
                yield return dataValue;
        }
    }
}