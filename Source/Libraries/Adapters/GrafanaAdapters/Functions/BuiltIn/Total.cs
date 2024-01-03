using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.TimeSeries;

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
                Value = trackedValues.Sum(),
                Time = lastTime,
                Target = lastTarget,
                Flags = lastFlags
            };
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Total<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            double magnitudeTotal = 0.0D;
            double angleTotal = 0.0D;

            double lastTime = 0.0D;
            string lastMagnitudeTarget = null;
            string lastAngleTarget = null;
            MeasurementStateFlags lastFlags = 0;

            // Immediately enumerate to compute values - only enumerate once
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters))
            {
                magnitudeTotal += dataValue.Magnitude;
                angleTotal += dataValue.Angle + 180;

                lastTime = dataValue.Time;
                lastMagnitudeTarget = dataValue.MagnitudeTarget;
                lastAngleTarget = dataValue.AngleTarget;
                lastFlags = dataValue.Flags;
            }

            // Return computed results
            yield return new PhasorValue()
            {
                Magnitude = magnitudeTotal,
                Angle = angleTotal % 360 - 180, // TODO: JRC - should we leave angle as-is for total?
                Time = lastTime,
                MagnitudeTarget = lastMagnitudeTarget,
                AngleTarget = lastAngleTarget,
                Flags = lastFlags
            };
        }
    }
}