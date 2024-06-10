using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable AccessToModifiedClosure

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the difference between consecutive values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Difference(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Difference(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Difference, Diff<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Difference<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Difference<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the difference between consecutive values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Diff"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override bool IsSliceSeriesEquivalent => false;

    /// <summary>
    /// Computes the difference between the current value and the last value.
    /// </summary>
    /// <param name="currentValue">Source value.</param>
    /// <param name="lastValue">Last result.</param>
    /// <returns>Calculated difference.</returns>
    protected abstract T TransposeCompute(T currentValue, T lastValue);

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        T lastValue = new();

        // Return deferred enumeration of computed values
        await foreach (T dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            // ReSharper disable once InlineTemporaryVariable
            T currentValue = dataValue;

            if (lastValue.Time > 0.0D)
                yield return TransposeCompute(dataValue, lastValue);

            lastValue = currentValue;
        }
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Difference<MeasurementValue>
    {
        /// <inheritdoc />
        protected override MeasurementValue TransposeCompute(MeasurementValue currentValue, MeasurementValue lastValue) => currentValue with
        {
            // To ensure current value is the source for metadata lookup on
            // this custom target, the spacing around the dash is important:
            Target = $"{currentValue.Target} - {lastValue.Target}",
            Value = currentValue.Value - lastValue.Value
        };
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Difference<PhasorValue>
    {
        /// <inheritdoc />
        protected override PhasorValue TransposeCompute(PhasorValue currentValue, PhasorValue lastValue) => currentValue with
        {
            // To ensure current value is the source for metadata lookup on
            // this custom target, the spacing around the dash is important:
            Target = $"{currentValue.Target} - {lastValue.Target}",
            Magnitude = currentValue.Magnitude - lastValue.Magnitude,
            Angle = (currentValue.Angle + 180 - (lastValue.Angle + 180)) % 360 - 180
        };
    }
}