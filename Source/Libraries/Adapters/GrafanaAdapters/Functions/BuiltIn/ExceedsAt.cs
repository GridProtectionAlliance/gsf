using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values at which a value exceeds the given threshold. The <c>threhsold</c> parameter value is a
/// floating-point numbers that represents the threshold to be exceeded. Second parameter, <c>fallsBelow</c>, optional,
/// is a boolean flag that determines if the value should be considered inversely as falling below the threshold instead
/// of exceeding. <c>returnDurations</c>, optional, is a boolean that determines if the duration (in seconds) from where
/// value exceeded threshold should be returned instead of the original value. Forth parameter, <c>reportEndMarker</c>,
/// is a boolean flag that determines if a value should be reported at the point when threshold stops being exceeding.
/// the threshold.
/// </summary>
/// <remarks>
/// Signature: <c>Exceeds(threshold, [fallsBelow = false], [returnDurations = false], [reportEndMarker = false], expression)</c> -<br/>
/// Returns: Series of values.<br/>
/// Example: <c>Exceeds(60.05, true, FILTER ActiveMeasurements WHERE SignalType LIKE '%FREQ')</c><br/>
/// Variants: ExceedsAt, Exceeds<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class ExceedsAt<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(ExceedsAt<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values at which a value exceed the given threshold.";

    /// <inheritdoc />
    public override string[] Aliases => ["Exceeds"];

    /// <inheritdoc />
    //  Function only operates on series data - slices and sets are not sensible for function usage.
    public override GroupOperations AllowedGroupOperations => GroupOperations.None;

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "threshold",
            Default = 1.0D,
            Description = "A floating point value representing the threshold.",
            Required = true
        },
        new ParameterDefinition<bool>
        {
            Name = "fallsBelow",
            Default = false,
            Description = "A boolean flag that determines if the value should be considered inversely as falling below the threshold instead of exceeding.",
            Required = false
        },
        new ParameterDefinition<bool>
        {
            Name = "returnDurations",
            Default = false,
            Description = "A boolean flag that determines if the duration (in seconds) from where value exceeded threshold should be returned instead of the original value.",
            Required = false
        },
        new ParameterDefinition<bool>
        {
            Name = "reportEndMarker",
            Default = false,
            Description = "A boolean flag that determines if a value should be reported at the point when threshold stops being exceeding.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        double threshold = parameters.Value<double>(0);
        bool fallsBelow = parameters.Value<bool>(1);
        bool returnDurations = parameters.Value<bool>(2);
        bool reportEndMarker = parameters.Value<bool>(3);

        T startValue = default;
        T lastValue = default;

        bool valueExceedsThreshold(T value)
        {
            // Invert threshold check when fallsBelow is true
            return fallsBelow ? value.Value < threshold : value.Value > threshold;
        }

        await foreach (T dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (double.IsNaN(dataValue.Value) || dataValue.Time == 0.0D)
                continue;

            lastValue = dataValue;

            // While start value has a time, we are tracking time for threshold exceeded
            if (startValue.Time > 0.0D)
            {
                // If value continues to exceed threshold, keep tracking
                if (valueExceedsThreshold(dataValue))
                    continue;

                // Value fell below threshold, produce start report
                yield return returnDurations ? startValue with { Value = (dataValue.Time - startValue.Time) / 1000.0D } : startValue;

                // If enabled, produce end report
                if (reportEndMarker)
                    yield return returnDurations ? dataValue with { Value = 0.0D } : dataValue;

                startValue = default;
            }
            else if (startValue.Time == 0.0D)
            {
                // If value does not exceed threshold, continue
                if (!valueExceedsThreshold(dataValue))
                    continue;

                // Value exceeded threshold, start tracking time
                startValue = dataValue;
            }
        }

        // Handle edge case for reporting end marker where value exceeds threshold through end of series
        if (startValue.Time == 0.0D || lastValue.Time == 0.0D)
            yield break;

        // Produce start report that exceeds threshold through end of series
        yield return returnDurations ? startValue with { Value = (lastValue.Time - startValue.Time) / 1000.0D } : startValue;

        // If enabled, produce end report
        if (reportEndMarker)
            yield return returnDurations ? lastValue with { Value = 0.0D } : lastValue;
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : ExceedsAt<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : ExceedsAt<PhasorValue>
    {
        // Operating on magnitude only
    }
}