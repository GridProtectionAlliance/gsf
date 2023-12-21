using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;
using GSF.Units;

namespace GrafanaAdapters.GrafanaFunctions;

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
public class TimeIntegration: GrafanaFunctionBase
{
    /// <inheritdoc />
    public override string Name => nameof(TimeIntegration);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the time-based integration.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "TimeInt" };

    /// <inheritdoc />
    public override List<IParameter> Parameters => new()
    {
        new Parameter<IDataSourceValueGroup>
        {
            Default = new DataSourceValueGroup<DataSourceValue>(),
            Description = "Data Points",
            Required = true
        },
        new Parameter<TargetTimeUnit>
        {
            Default = new TargetTimeUnit { Unit = TimeUnit.Hours },
            Description = "Specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, " +
                          "Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or " +
                          "AtomicUnitsOfTime - defaults to Seconds.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
    {
        // Get Values
        DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
        TargetTimeUnit timeUnit = (parameters[1] as IParameter<TargetTimeUnit>).Value;

        // Compute
        List<DataSourceValue> datasourceValuesList = dataSourceValues.Source.ToList();

        DataSourceValue lastElement = dataSourceValues.Source.First();
        double previousTime = dataSourceValues.Source.First().Time;

        for (int i = 1; i < datasourceValuesList.Count; i++) 
        {
            DataSourceValue dataSourceValue = datasourceValuesList[i];
            lastElement.Value += dataSourceValue.Value * TimeConversion.ToTimeUnits((dataSourceValue.Time - previousTime) * SI.Milli, timeUnit);

            previousTime = dataSourceValue.Time; 
        }

        // Set Values
        dataSourceValues.Target = $"{Name}({dataSourceValues.Target},{timeUnit.Unit})";
        dataSourceValues.Source = Enumerable.Repeat(lastElement, 1);

        return dataSourceValues;
    }

    /// <inheritdoc />
    public override DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
    {
        // Get Values
        DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
        TargetTimeUnit timeUnit = (parameters[1] as IParameter<TargetTimeUnit>).Value;

        // Compute
        List<PhasorValue> phasorValuesList = phasorValues.Source.ToList();

        PhasorValue lastElement = phasorValues.Source.First();
        lastElement.Magnitude = 0;
        lastElement.Angle = 0;
        double previousTime = phasorValues.Source.First().Time;

        for (int i = 1; i < phasorValuesList.Count; i++)
        {
            PhasorValue phasorValue = phasorValuesList[i];
            lastElement.Magnitude += phasorValue.Magnitude * TimeConversion.ToTimeUnits((phasorValue.Time - previousTime) * SI.Milli, timeUnit);
            lastElement.Angle += phasorValue.Angle * TimeConversion.ToTimeUnits((phasorValue.Time - previousTime) * SI.Milli, timeUnit);

            previousTime = phasorValue.Time;
        }

        // Set Values
        string[] labels = phasorValues.Target.Split(';');
        phasorValues.Target = $"{Name}({labels[0]},{timeUnit.Unit});{Name}({labels[1]},{timeUnit.Unit})";
        phasorValues.Source = Enumerable.Repeat(lastElement, 1);

        return phasorValues;
    }
}