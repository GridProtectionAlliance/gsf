using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that is the maximum of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Maximum(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Maximum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Maximum, Max<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Maximum<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Maximum";

    /// <inheritdoc />
    public override string Description => " Returns a single value that is the maximum of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Max" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Maximum<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            DataSourceValue maxValue = new() { Value = double.MinValue };

            // Immediately enumerate values to find maximum
            foreach (DataSourceValue dataValue in GetDataSourceValues(parameters))
            {
                if (dataValue.Value >= maxValue.Value)
                    maxValue = dataValue;
            }

            // Do not return local default value
            if (maxValue.Time > 0.0D)
                yield return maxValue;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Maximum<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            PhasorValue maxValue = new() { Magnitude = double.MinValue };

            // Immediately enumerate values to find maximum - magnitude only
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters))
            {
                if (dataValue.Magnitude >= maxValue.Magnitude)
                    maxValue = dataValue;
            }

            // Do not return local default value
            if (maxValue.Time > 0.0D)
                yield return maxValue;
        }
    }
}