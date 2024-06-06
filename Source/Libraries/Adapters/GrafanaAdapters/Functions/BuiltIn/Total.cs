using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the sum of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Total(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Total(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Variants: Total, Add, Sum<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Total<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Total<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the sum of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Add", "Sum"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Scalar;

    /// <inheritdoc />
    public class ComputeMeasurementValue : Total<MeasurementValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<MeasurementValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            MeasurementValue lastValue = default;

            IAsyncEnumerable<double> trackedValues = GetDataSourceValues(parameters).Select(dataValue =>
            {
                lastValue = dataValue;
                return dataValue.Value;
            });

            // Immediately enumerate to compute values
            double sum = await trackedValues.SumAsync(cancellationToken).ConfigureAwait(false);

            // Return computed results
            if (lastValue.Time > 0.0D)
                yield return lastValue with { Value = sum };
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Total<PhasorValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<PhasorValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            PhasorValue lastValue = default;
            double magnitudeTotal = 0.0D;
            double angleTotal = 0.0D;

            // Immediately enumerate to compute values - only enumerate once
            await foreach (PhasorValue dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken).ConfigureAwait(false))
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