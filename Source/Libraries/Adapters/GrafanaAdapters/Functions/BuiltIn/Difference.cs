using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.TimeSeries;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the difference between consecutive values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Difference(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Difference(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Difference, Diff<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Difference<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Difference";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the difference between consecutive values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Diff" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Difference<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            DataSourceValue lastResult = new();

            // Transpose computed value
            DataSourceValue transposeCompute(DataSourceValue dataValue) => dataValue with
            {
                Value = dataValue.Value - lastResult.Value
            };

            // Return deferred enumeration of computed values
            foreach (DataSourceValue dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
            {
                if (lastResult.Time > 0.0D)
                    yield return dataValue;

                lastResult = dataValue;
            }
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Difference<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            PhasorValue lastResult = new();

            // Transpose computed value
            PhasorValue transposeCompute(PhasorValue dataValue) => dataValue with
            {
                Magnitude = dataValue.Magnitude - lastResult.Magnitude,
                Angle = (dataValue.Angle + 180 - (lastResult.Angle + 180)) % 360 - 180
            };

            // Return deferred enumeration of computed values
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
            {
                if (lastResult.Time > 0.0D)
                    yield return dataValue;

                lastResult = dataValue;
            }
        }
    }
}