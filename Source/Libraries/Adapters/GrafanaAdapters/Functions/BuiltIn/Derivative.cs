using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.DataSources;
using GSF.Units;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the rate of change, per time units, for the difference between consecutive values in the source
/// series. The units parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds,
/// Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals),
/// PlanckTime or AtomicUnitsOfTime - defaults to Seconds.
/// </summary>
/// <remarks>
/// Signature: <c>Derivative([units = Seconds], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Derivative(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Derivative, Der<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Derivative<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Derivative";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the rate of change, per time units, for the difference between consecutive values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Der" };

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<TargetTimeUnit>
        {
            Name = "units",
            Default = new TargetTimeUnit { Unit = TimeUnit.Seconds },
            Parse = TargetTimeUnit.Parse,
            Description = "Specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, " + 
                          "Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or " + 
                          "AtomicUnitsOfTime - defaults to Seconds.",
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
            Value = (dataValue.Value - lastResult.Value) / TargetTimeUnit.ToTimeUnits((dataValue.Time - lastResult.Time) * SI.Milli, units)
        };

        // Return deferred enumeration of computed values
        foreach (T dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
        {
            if (lastResult.Time > 0.0D)
                yield return dataValue;

            lastResult = dataValue;
        }
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Derivative<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Derivative<PhasorValue>
    {
        // Operating on magnitude only
    }
}