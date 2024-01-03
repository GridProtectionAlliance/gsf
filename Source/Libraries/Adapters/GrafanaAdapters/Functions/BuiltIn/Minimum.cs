using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that is the minimum of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Minimum(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Minimum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Minimum, Min<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Minimum<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Minimum";

    /// <inheritdoc />
    public override string Description => "Returns a single value that is the minimum of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Min" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Minimum<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            DataSourceValue minValue = new() { Value = double.MaxValue };

            // Immediately enumerate values to find minimum
            foreach (DataSourceValue dataValue in GetDataSourceValues(parameters))
            {
                if (dataValue.Value <= minValue.Value)
                    minValue = dataValue;
            }

            // Do not return local default value
            if (minValue.Time > 0.0D)
                yield return minValue;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Minimum<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            PhasorValue minValue = new() { Magnitude = double.MaxValue };

            // Immediately enumerate values to find minimum - magnitude only
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters))
            {
                if (dataValue.Magnitude <= minValue.Magnitude)
                    minValue = dataValue;
            }

            // Do not return local default value
            if (minValue.Time > 0.0D)
                yield return minValue;
        }
    }
}