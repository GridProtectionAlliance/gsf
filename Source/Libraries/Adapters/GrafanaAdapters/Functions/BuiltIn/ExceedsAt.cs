using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values at which a value exceed the given threshold. The <c>threhsold</c> parameter
/// value is a floating-point numbers that represents the threshold to be exceeded. Second parameter optional,
/// is a boolean flag that determines if the time duration, in seconds, the value exceeds threshold should be
/// returned instead.
/// </summary>
/// <remarks>
/// Signature: <c>Exceeds(threshold, [includeDuration = false], expression)</c> -<br/>
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
            Name = "returnDurations",
            Default = false,
            Description = "A boolean flag that determines if the duration (in seconds) from where value exceeded threshold should be returned instead of the original value.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        double threshold = parameters.Value<double>(0);
        bool returnDurations = parameters.Value<bool>(1);

        T startValue = default;
        T lastValue = default;

        await foreach (T dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (double.IsNaN(dataValue.Value))
                continue;

            // While start value has a time, we are tracking time for threshold exceeded
            if (startValue.Time > 0.0D)
            {
                // If value continues to exceed threshold, keep tracking
                if (dataValue.Value > threshold)
                    continue;

                // If value drops below threshold, return duration
                if (returnDurations)
                {
                    if (lastValue.Time > 0.0D)
                    {
                        yield return startValue with
                        {
                            Value = TimeSpan.FromMilliseconds(lastValue.Time - startValue.Time).Seconds
                        };
                    }
                }
                else
                {
                    yield return startValue;
                }

                startValue = default;
            }
            else
            {
                if (dataValue.Value <= threshold)
                    continue;

                // If value exceeds threshold, start tracking time
                startValue = dataValue;
            }

            lastValue = dataValue;
        }

        if (startValue.Time == 0.0D)
            yield break;

        // Handle edge case where value exceeds threshold through end of series
        if (returnDurations)
        {
            if (lastValue.Time > 0.0D)
            {
                yield return startValue with
                {
                    Value = TimeSpan.FromMilliseconds(lastValue.Time - startValue.Time).Seconds
                };
            }
        }
        else
        {
            yield return startValue;
        }
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