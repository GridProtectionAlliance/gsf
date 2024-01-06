using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.DataSources;
using GSF.Units;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the time-based integration, i.e., the sum of <c>V(n) * (T(n) - T(n-1))</c> where time difference is
/// calculated in the specified time units, of the values in the source series. The units parameter, optional, specifies the type of time units
/// and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional
/// Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or AtomicUnitsOfTime - defaults to Hours.
/// </summary>
/// <remarks>
/// Signature: <c>TimeIntegration([units = Hours], expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>TimeIntegration(FILTER ActiveMeasurements WHERE SignalType='CALC' AND PointTag LIKE '%-MW:%')</c><br/>
/// Variants: TimeIntegration, TimeInt<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class TimeIntegration<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(TimeIntegration<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the time-based integration.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "TimeInt" };

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<TargetTimeUnit>
        {
            Name = "units",
            Default = new TargetTimeUnit { Unit = TimeUnit.Hours },
            Parse = TargetTimeUnit.Parse,
            Description =
                "Specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, " +
                "Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or " +
                "AtomicUnitsOfTime - defaults to Hours.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        TargetTimeUnit units = parameters.Value<TargetTimeUnit>(0);
        T lastResult = new();

        // Transpose computed value
        T transposeCompute(T dataValue) => dataValue with
        {
            Value = lastResult.Value + dataValue.Value * TargetTimeUnit.ToTimeUnits((dataValue.Time - lastResult.Time) * SI.Milli, units)
        };

        // Immediately enumerate to compute values - only enumerate once
        foreach (T dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
            lastResult = dataValue;

        // Return computed value
        if (lastResult.Time > 0.0D)
            yield return lastResult;
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : TimeIntegration<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : TimeIntegration<PhasorValue>
    {
        // Operating on magnitude only
    }
}