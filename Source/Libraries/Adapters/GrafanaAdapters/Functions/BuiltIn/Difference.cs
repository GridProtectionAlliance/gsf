using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using System.Collections.Generic;
using System.Linq;
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
public abstract class Difference<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Difference<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the difference between consecutive values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Diff" };

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
        await foreach (T dataValue in GetDataSourceValues(parameters).Select(dataValue => TransposeCompute(dataValue, lastValue)).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (lastValue.Time > 0.0D)
                yield return dataValue;

            lastValue = dataValue;
        }
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Difference<DataSourceValue>
    {
        /// <inheritdoc />
        protected override DataSourceValue TransposeCompute(DataSourceValue currentValue, DataSourceValue lastValue) => currentValue with
        {
            Value = currentValue.Value - lastValue.Value
        };
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Difference<PhasorValue>
    {
        /// <inheritdoc />
        protected override PhasorValue TransposeCompute(PhasorValue currentValue, PhasorValue lastValue) => currentValue with
        {
            Magnitude = currentValue.Magnitude - lastValue.Magnitude,
            Angle = (currentValue.Angle + 180 - (lastValue.Angle + 180)) % 360 - 180
        };
    }
}