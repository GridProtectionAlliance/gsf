using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.Collections;
using GSF.TimeSeries;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the median of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Median(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Median(FILTER ActiveMeasurements WHERE SignalType='ALOG')</c><br/>
/// Variants: Median, Med, Mid<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class Median<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Median";

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the median of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Med", "Mid" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Median<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Median uses immediate in-memory array load
            DataSourceValue[] values = GetDataSourceValues(parameters).OrderBy(dataValue => dataValue.Value).Median();

            if (values.Length == 0)
                yield break;

            // Median can return two values if there is an even number of values
            DataSourceValue result = values.Last();

            if (values.Length > 1)
                result.Value = values[0].Value + (values[1].Value - values[0].Value) / 2.0D;

            yield return result;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Median<PhasorValue>
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

            double[] magnitudeMedians = magnitudes.OrderBy(value => value).Median();
            double[] angleMedians = angles.OrderBy(value => value).Median();

            double magnitudeMedian = magnitudeMedians.Last();
            double angleMedian = angleMedians.Last();

            // Median can return two values if there is an even number of values
            if (magnitudeMedians.Length > 1)
                magnitudeMedian = magnitudeMedians[0] + (magnitudeMedians[1] - magnitudeMedians[0]) / 2.0D;

            if (angleMedians.Length > 1)
                angleMedian = angleMedians[0] + (angleMedians[1] - angleMedians[0]) / 2.0D;

            // Return immediate enumeration of computed values
            yield return new PhasorValue()
            {
                Magnitude = magnitudeMedian,
                Angle = angleMedian,
                Time = lastTime,
                MagnitudeTarget = lastMagnitudeTarget,
                AngleTarget = lastAngleTarget,
                Flags = lastFlags
            };
        }
    }
}