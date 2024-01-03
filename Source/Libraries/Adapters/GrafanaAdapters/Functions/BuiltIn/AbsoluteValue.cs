using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public class ComputeDataSourceValue : AbsoluteValue<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Transpose computed value
            DataSourceValue transposeCompute(DataSourceValue dataValue) => dataValue with
            {
                Value = Math.Abs(dataValue.Value)
            };

            // Return deferred enumeration of computed values
            foreach (DataSourceValue dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
                yield return dataValue;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : AbsoluteValue<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Transpose computed value
            PhasorValue transposeCompute(PhasorValue dataValue) => dataValue with
            {
                Magnitude = Math.Abs(dataValue.Magnitude),
                Angle = Math.Abs(dataValue.Angle),
            };

            // Return deferred enumeration of computed values
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
                yield return dataValue;
        }
    }
}
