using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using GSF.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

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
public abstract class Median<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Median<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the median of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Med", "Mid"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Scalar;

    /// <inheritdoc />
    public override bool ResultIsSetTargetSeries => true;

    /// <inheritdoc />
    public class ComputeMeasurementValue : Median<MeasurementValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<MeasurementValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // Median uses immediate in-memory array load
            List<MeasurementValue> values = await GetDataSourceValues(parameters)
                .OrderBy(dataValue => dataValue.Value)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Avoid multiple enumeration warnings; memory usage is small anyway
            List<MeasurementValue> median = values.Middle().ToList();

            if (median.Count == 0)
                yield break;

            MeasurementValue result = median.Last();
            result.Value = median.Average(mv => mv.Value);
            yield return result;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Median<PhasorValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<PhasorValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            PhasorValue lastValue = default;
            List<double> magnitudes = [];
            List<double> angles = [];

            // Immediately load values in-memory only enumerating data source once
            await foreach (PhasorValue dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                lastValue = dataValue;
                magnitudes.Add(dataValue.Magnitude);
                angles.Add(dataValue.Angle);
            }

            if (magnitudes.Count == 0)
                yield break;

            // Median can return two values if there is an even number of values
            double magnitudeMedian = magnitudes.Median().Average();
            double angleMedian = angles.Median().Average();

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