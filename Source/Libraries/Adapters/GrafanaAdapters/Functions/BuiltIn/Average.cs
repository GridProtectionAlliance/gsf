using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.TimeSeries;

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
    public override string Name => "Average";

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the mean of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Avg", "Mean" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Average<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            double lastTime = 0.0D;
            string lastTarget = null;
            MeasurementStateFlags lastFlags = 0;

            IEnumerable<double> trackedValues = GetDataSourceValues(parameters).Select(dataValue =>
            {
                lastTime = dataValue.Time;
                lastTarget = dataValue.Target;
                lastFlags = dataValue.Flags;
                return dataValue.Value;
            });

            // Return immediate enumeration of computed values
            yield return new DataSourceValue()
            {
                Value = trackedValues.Average(),
                Time = lastTime,
                Target = lastTarget,
                Flags = lastFlags
            };
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Average<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            double magnitudeTotal = 0.0D;
            double angleTotal = 0.0D;
            int count = 0;
            
            double lastTime = 0.0D;
            string lastMagnitudeTarget = null;
            string lastAngleTarget = null;
            MeasurementStateFlags lastFlags = 0;

            // Immediately enumerate to compute values - only enumerate once
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters))
            {
                magnitudeTotal += dataValue.Magnitude;
                angleTotal += dataValue.Angle + 180;
                count++;

                lastTime = dataValue.Time;
                lastMagnitudeTarget = dataValue.MagnitudeTarget;
                lastAngleTarget = dataValue.AngleTarget;
                lastFlags = dataValue.Flags;
            }

            // Return computed results
            yield return new PhasorValue()
            {
                Magnitude = magnitudeTotal / count,
                Angle = angleTotal / count % 360 - 180,
                Time = lastTime,
                MagnitudeTarget = lastMagnitudeTarget,
                AngleTarget = lastAngleTarget,
                Flags = lastFlags
            };
        }
    }
}