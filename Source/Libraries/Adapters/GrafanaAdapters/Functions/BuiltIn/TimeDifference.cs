using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.Units;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the time difference, in time units, between consecutive values in the source series. The units
/// parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds,
/// Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or
/// AtomicUnitsOfTime - defaults to Seconds.
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
    public override string Name => "TimeDifference";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the time difference, in time units, between consecutive values in the source series.";


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
    public class ComputeDataSourceValue : TimeDifference<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //TargetTimeUnit timeUnit = (parameters[1] as IParameter<TargetTimeUnit>).Value;

            //// Compute
            //double previousTime = dataSourceValues.Source.First().Time;
            //IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Skip(1).Select(dataSourceValue =>
            //{
            //    DataSourceValue transformedValue = dataSourceValue;
            //    transformedValue.Value = TimeConversion.ToTimeUnits((dataSourceValue.Time - previousTime) * SI.Milli, timeUnit);

            //    previousTime = dataSourceValue.Time;
            //    return transformedValue;
            //});

            //// Set Values
            //dataSourceValues.Target = $"{Name}({dataSourceValues.Target},{timeUnit.Unit})";
            //dataSourceValues.Source = transformedDataSourceValues;

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : TimeDifference<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //TargetTimeUnit timeUnit = (parameters[1] as IParameter<TargetTimeUnit>).Value;

            //// Compute
            //double previousTime = phasorValues.Source.First().Time;
            //IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Skip(1).Select(phasorValue =>
            //{
            //    PhasorValue transformedValue = phasorValue;

            //    double timeDifference = TimeConversion.ToTimeUnits((phasorValue.Time - previousTime) * SI.Milli, timeUnit);
            //    transformedValue.Magnitude = timeDifference;
            //    transformedValue.Angle = timeDifference;

            //    previousTime = phasorValue.Time;
            //    return transformedValue;
            //});

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{Name}({labels[0]},{timeUnit.Unit});{Name}({labels[1]},{timeUnit.Unit})";
            //phasorValues.Source = transformedPhasorValues;

            //return phasorValues;
            return null;
        }
    }
}