using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using GSF.Units;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable AccessToModifiedClosure

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the time difference, in time units, between consecutive values in the source series.
/// The <c>units</c> parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds,
/// Microseconds, Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e.,
/// 100-nanosecond intervals), PlanckTime or AtomicUnitsOfTime - defaults to Seconds.
/// </summary>
/// <remarks>
/// Signature: <c>TimeDifference([units = Seconds], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>TimeDifference(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: TimeDifference, TimeDiff, Elapsed<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class TimeDifference<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(TimeDifference<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the time difference, in time units, between consecutive values in the source series.";

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    // Slice operation has no meaning for this time-focused function and Set operation will have an aberration between series,
    // so we override the exposed behaviors, i.e., use of Slice will produce an error and use of Set will be hidden:
    public override GroupOperations AllowedGroupOperations => GroupOperations.None | GroupOperations.Set;

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.None;

    /// <inheritdoc />
    public override string[] Aliases => new[] { "TimeDiff", "Elapsed" };

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<TargetTimeUnit>
        {
            Name = "units",
            Default = new TargetTimeUnit { Unit = TimeUnit.Seconds },
            Parse = TargetTimeUnit.Parse,
            Description =
                "Specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, " +
                "Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or " +
                "AtomicUnitsOfTime - defaults to Seconds.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        TargetTimeUnit units = parameters.Value<TargetTimeUnit>(0);
        double lastTime = new();

        // Transpose computed value
        T transposeCompute(T dataValue) => dataValue with
        {
            Value = TargetTimeUnit.ToTimeUnits((dataValue.Time - lastTime) * SI.Milli, units)
        };

        // Return deferred enumeration of computed values
        await foreach (T dataValue in GetDataSourceValues(parameters).Select(transposeCompute).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (lastTime > 0.0D)
                yield return dataValue;

            lastTime = dataValue.Time;
        }
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : TimeDifference<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : TimeDifference<PhasorValue>
    {
    }
}