using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the mean of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Average(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Average(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Average, Avg, Mean<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Average<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Average<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the mean of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Avg", "Mean" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Average<DataSourceValue>
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
            double average = trackedValues.Average();

            // Return computed results
            if (lastValue.Time > 0.0D)
                yield return lastValue with { Value = average };
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Average<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters)
        {
            PhasorValue lastValue = default;
            double magnitudeTotal = 0.0D;
            double angleTotal = 0.0D;
            int count = 0;

            // Immediately enumerate to compute values enumerating data source once
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters))
            {
                lastValue = dataValue;
                magnitudeTotal += dataValue.Magnitude;
                angleTotal += dataValue.Angle + 180;
                count++;
            }

            // Return computed results
            if (lastValue.Time > 0.0D)
            {
                yield return lastValue with
                {
                    Magnitude = magnitudeTotal / count,
                    Angle = angleTotal / count % 360 - 180
                };
            }
        }
    }
}