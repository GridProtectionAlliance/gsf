using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.DataSources;
using GSF.Collections;

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
    public override string Name => nameof(Median<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the median of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Med", "Mid" };

    /// <inheritdoc />
    public override bool ResultIsSetTargetSeries => true;

    /// <inheritdoc />
    public class ComputeDataSourceValue : Median<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters)
        {
            // Median uses immediate in-memory array load
            DataSourceValue[] values = GetDataSourceValues(parameters).OrderBy(dataValue => dataValue.Value).Median();
            int length = values.Length;

            if (length == 0)
                yield break;

            // Median can return two values if there is an even number of values
            DataSourceValue result = values.Last();

            if (length > 1)
                result.Value = values[0].Value + (values[1].Value - values[0].Value) / 2.0D;

            yield return result;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Median<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters)
        {
            PhasorValue lastValue = default;
            List<double> magnitudes = new();
            List<double> angles = new();

            // Immediately load values in-memory only enumerating data source once
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters))
            {
                lastValue = dataValue;
                magnitudes.Add(dataValue.Magnitude);
                angles.Add(dataValue.Angle);
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

            // Return computed results
            if (lastValue.Time > 0.0D)
            {
                yield return lastValue with
                {
                    Magnitude = magnitudeMedian,
                    Angle = angleMedian
                };
            }
        }
    }
}