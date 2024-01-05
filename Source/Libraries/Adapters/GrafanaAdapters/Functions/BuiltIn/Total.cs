using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the sum of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Total(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Total(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Variants: Total, Sum<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Total<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Total";

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the sum of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Sum" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Total<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters)
        {
            DataSourceValue lastValue = default;

            IEnumerable<double> trackedValues = GetDataSourceValues(parameters).Select(dataValue =>
            {
                lastValue = dataValue;
                return dataValue.Value;
            });

            // Immediately enumerate to compute values
            double sum = trackedValues.Sum();

            // Return computed results
            if (lastValue.Time > 0.0D)
                yield return lastValue with { Value = sum };
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Total<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters)
        {
            PhasorValue lastValue = default;
            double magnitudeTotal = 0.0D;
            double angleTotal = 0.0D;

            // Immediately enumerate to compute values - only enumerate once
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters))
            {
                lastValue = dataValue;
                magnitudeTotal += dataValue.Magnitude;
                angleTotal += dataValue.Angle + 180;
            }

            // Return computed results
            if (lastValue.Time > 0.0D)
            {
                yield return lastValue with
                {
                    Magnitude = magnitudeTotal,
                    Angle = angleTotal % 360 - 180
                };
            }
        }
    }
}