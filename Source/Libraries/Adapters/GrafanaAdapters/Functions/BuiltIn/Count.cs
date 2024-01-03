using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.TimeSeries;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that is the count of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Count(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Count(PPA:1; PPA:2; PPA:3)</c><br/>
/// Variants: Count<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Count<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Count";

    /// <inheritdoc />
    public override string Description => "Returns a single value that is the count of the values in the source series.";

    /// <inheritdoc />
    public class ComputeDataSourceValue : Count<DataSourceValue>
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
                Value = trackedValues.Count(),
                Time = lastTime,
                Target = lastTarget,
                Flags = lastFlags
            };
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Count<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            double lastTime = 0.0D;
            string lastMagnitudeTarget = null;
            string lastAngleTarget = null;
            MeasurementStateFlags lastFlags = 0;

            IEnumerable<double> trackedValues = GetDataSourceValues(parameters).Select(dataValue =>
            {
                lastTime = dataValue.Time;
                lastMagnitudeTarget = dataValue.MagnitudeTarget;
                lastAngleTarget = dataValue.AngleTarget;
                lastFlags = dataValue.Flags;
                return dataValue.Magnitude;
            });

            // Only count once
            int count = trackedValues.Count();

            // Return immediate enumeration of computed values
            yield return new PhasorValue()
            {
                Magnitude = count,
                Angle = count,
                Time = lastTime,
                MagnitudeTarget = lastMagnitudeTarget,
                AngleTarget = lastAngleTarget,
                Flags = lastFlags
            };
        }
    }
}