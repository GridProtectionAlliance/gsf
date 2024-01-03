using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.Collections;
using GSF.TimeSeries;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the mode of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Mode(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Mode(FILTER TOP 5 ActiveMeasurements WHERE SignalType='DIGI')</c><br/>
/// Variants: Mode<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class Mode<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Mode";

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the mode of the values in the source series.";

    /// <inheritdoc />
    public class ComputeDataSourceValue : Mode<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Immediately load values in-memory only enumerating data source once
            DataSourceValue[] values = GetDataSourceValues(parameters).ToArray();
            yield return values.MajorityBy(values.Last(), dataValue => dataValue.Value, false);
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Mode<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            List<double> magnitudes = new();
            List<double> angles = new();

            double lastTime = 0.0D;
            string lastMagnitudeTarget = null;
            string lastAngleTarget = null;
            MeasurementStateFlags lastFlags = 0;

            // Immediately load values in-memory only enumerating data source once
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters))
            {
                magnitudes.Add(dataValue.Magnitude);
                angles.Add(dataValue.Angle);

                lastTime = dataValue.Time;
                lastMagnitudeTarget = dataValue.MagnitudeTarget;
                lastAngleTarget = dataValue.AngleTarget;
                lastFlags = dataValue.Flags;
            }

            if (magnitudes.Count == 0)
                yield break;

            double magnitudeMode = magnitudes.Majority(false);
            double angleMode = angles.Majority(false);

            // Return immediate enumeration of computed values
            yield return new PhasorValue()
            {
                Magnitude = magnitudeMode,
                Angle = angleMode,
                Time = lastTime,
                MagnitudeTarget = lastMagnitudeTarget,
                AngleTarget = lastAngleTarget,
                Flags = lastFlags
            };
        }
    }
}