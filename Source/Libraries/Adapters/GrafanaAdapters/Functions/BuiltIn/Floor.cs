using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public override string Name => "Floor";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the smallest integral value that is less than or equal to each of the values in the source series.";

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.Standard | GroupOperations.Set;

    /// <inheritdoc />
    public class ComputeDataSourceValue : Floor<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Transpose computed value
            DataSourceValue transposeCompute(DataSourceValue dataValue) => dataValue with
            {
                Value = Math.Floor(dataValue.Value)
            };

            // Return deferred enumeration of computed values
            foreach (DataSourceValue dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
                yield return dataValue;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Floor<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Transpose computed value
            PhasorValue transposeCompute(PhasorValue dataValue) => dataValue with
            {
                Magnitude = Math.Floor(dataValue.Magnitude),
                Angle = Math.Floor(dataValue.Angle),
            };

            // Return deferred enumeration of computed values
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
                yield return dataValue;
        }
    }
}