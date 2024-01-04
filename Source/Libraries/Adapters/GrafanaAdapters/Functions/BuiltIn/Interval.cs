using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.Units;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent a decimated set of the values in the source series based on the specified interval N, in time units.
/// N is a floating-point value that must be greater than or equal to zero that represents the desired time interval, in time units, for the returned
/// data. The units parameter, optional, specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds,
/// Milliseconds, Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals),
/// PlanckTime or AtomicUnitsOfTime - defaults to Seconds. Setting N value to zero will request non-decimated, full resolution data from the data
/// source. A zero N value will always produce the most accurate aggregation calculation results but will increase query burden for large time ranges.
/// N can either be constant value or a named target available from the expression.
/// </summary>
/// <remarks>
/// Signature: <c>Interval(N, [units = Seconds], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Sum(Interval(0, FILTER ActiveMeasurements WHERE SignalType LIKE '%PHM'))</c><br/>
/// Variants: Interval<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Interval<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Interval";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent a decimated set of the values in the source series based on the specified interval N, in time units.";

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "N",
            Default = 0,
            Description = "A floating-point value that must be greater than or equal to zero that represents the desired time interval",
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
                "AtomicUnitsOfTime - defaults to Seconds.",
            Required = false
        }
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Interval<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //double value = (parameters[0] as IParameter<double>).Value;
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            //TargetTimeUnit timeUnit = (parameters[2] as IParameter<TargetTimeUnit>).Value;

            //if (value < 0)
            //    throw new ArgumentException($"{value} must be greater than or equal to 0.");

            //// Compute
            //double timeValue = TimeConversion.FromTimeUnits(value, timeUnit) / SI.Milli;

            //double previousTime = dataSourceValues.Source.First().Time;
            //List<DataSourceValue> transformedDataSourceValues = new();
            //foreach (DataSourceValue dataSourceValue in dataSourceValues.Source.Skip(1))
            //{
            //    if (dataSourceValue.Time - previousTime > timeValue)
            //    {
            //        previousTime = dataSourceValue.Time;
            //        transformedDataSourceValues.Add(dataSourceValue);
            //    }
            //}

            //// Set Values
            //dataSourceValues.Target = $"{Name}({value},{dataSourceValues.Target},{timeUnit.Unit})";
            //dataSourceValues.Source = transformedDataSourceValues;

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Interval<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //double value = (parameters[0] as IParameter<double>).Value;
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            //TargetTimeUnit timeUnit = (parameters[2] as IParameter<TargetTimeUnit>).Value;

            //if (value < 0)
            //    throw new ArgumentException($"{value} must be greater than or equal to 0.");

            //// Compute
            //double timeValue = TimeConversion.FromTimeUnits(value, timeUnit) / SI.Milli;

            //double previousTime = phasorValues.Source.First().Time;
            //List<PhasorValue> transformedPhasorValues = new();
            //foreach (PhasorValue phasorValue in phasorValues.Source.Skip(1))
            //{
            //    if (phasorValue.Time - previousTime > timeValue)
            //    {
            //        previousTime = phasorValue.Time;
            //        transformedPhasorValues.Add(phasorValue);
            //    }
            //}

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{Name}({labels[0]},{timeUnit.Unit});{Name}({labels[1]},{timeUnit.Unit})";
            //phasorValues.Source = transformedPhasorValues;

            //return phasorValues;
            return null;
        }
    }
}