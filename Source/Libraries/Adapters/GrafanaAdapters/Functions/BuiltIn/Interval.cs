using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using GSF.Units;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent a decimated set of the values in the source series based on the specified interval <c>N</c>, in time units.
/// <c>N</c> is a floating-point value that must be greater than or equal to zero that represents the desired time interval, in time units, for the returned
/// data. The <c>units</c>parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds,
/// Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or
/// AtomicUnitsOfTime - defaults to Seconds. <c>N</c> can either be constant value or a named target available from the expression.
/// </summary>
/// <remarks>
/// Signature: <c>Interval(N, [units = Seconds], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Sum(Interval(5, FILTER ActiveMeasurements WHERE SignalType LIKE '%PHM'))</c><br/>
/// Variants: Interval<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Interval<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Interval<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent a decimated set of the values in the source series based on the specified interval N, in time units.";

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    // Slice operation has no meaning for this time-focused function and Set operation will have an aberration between series,
    // so we override the exposed behaviors, i.e., use of Slice will produce an error and use of Set will be hidden:
    public override GroupOperations AllowedGroupOperations => GroupOperations.None | GroupOperations.Set;

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.None;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "N",
            Default = 1.0D,
            Description = "A floating-point value that must be greater than or equal to zero that represents the desired time interval.",
            Required = true
        },
        new ParameterDefinition<TargetTimeUnit>
        {
            Name = "units",
            Default = new TargetTimeUnit { Unit = TimeUnit.Seconds },
            Parse = TargetTimeUnit.Parse,
            Description =
                "Specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, " +
                "Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or " +
                "AtomicUnitsOfTime.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        TargetTimeUnit units = parameters.Value<TargetTimeUnit>(1);
        double valueN = TargetTimeUnit.FromTimeUnits(parameters.Value<double>(0), units) / SI.Milli;
        double lastTime = 0.0D;

        await foreach (T dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (lastTime > 0.0D)
            {
                if (dataValue.Time - lastTime >= valueN)
                {
                    lastTime = dataValue.Time;
                    yield return dataValue;
                }
            }
            else
            {
                lastTime = dataValue.Time;
                yield return dataValue;
            }
        }
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Interval<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Interval<PhasorValue>
    {
    }

}