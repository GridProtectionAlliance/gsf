using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the mean of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Average(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Average(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Average, Avg, Mean<br/>
/// Execution: Immediate enumeration.<br/>
/// Group Operations: Slice, Set
/// </remarks>
public abstract class Average<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Average<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the mean of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Avg", "Mean"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Scalar;

    /// <inheritdoc />
    public class ComputeMeasurementValue : Average<MeasurementValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<MeasurementValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            MeasurementValue lastValue = default;
            double total = 0.0D;
            int count = 0;

            // Immediately enumerate to compute values enumerating data source once
            await foreach (MeasurementValue dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                lastValue = dataValue;
                total += dataValue.Value;
                count++;
            }

            // Return computed results
            if (lastValue.Time > 0.0D)
            {
                yield return lastValue with
                {
                    Value = total / count,
                };
            }
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Average<PhasorValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<PhasorValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            PhasorValue lastValue = default;
            double magnitudeTotal = 0.0D;
            double angleTotal = 0.0D;
            int count = 0;

            // Immediately enumerate to compute values enumerating data source once
            await foreach (PhasorValue dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken).ConfigureAwait(false))
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