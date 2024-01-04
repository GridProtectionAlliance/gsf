using System.Collections.Generic;
using System.Threading;
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
    public class ComputeDataSourceValue : Derivative<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //TargetTimeUnit timeUnit = (parameters[1] as IParameter<TargetTimeUnit>).Value;

            //// Compute
            //DataSourceValue previousData = dataSourceValues.Source.First();
            //IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Skip(1).Select(dataSourceValue =>
            //{
            //    DataSourceValue transformedValue = dataSourceValue;

            //    transformedValue.Value = (transformedValue.Value - previousData.Value) / TimeConversion.ToTimeUnits((transformedValue.Time - previousData.Time) * SI.Milli, timeUnit);

            //    previousData = dataSourceValue;
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
    public class ComputePhasorValue : Derivative<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //TargetTimeUnit timeUnit = (parameters[1] as IParameter<TargetTimeUnit>).Value;

            //// Compute
            //PhasorValue previousData = phasorValues.Source.First();
            //IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Skip(1).Select(phasorValue =>
            //{
            //    PhasorValue transformedValue = phasorValue;

            //    transformedValue.Magnitude = (transformedValue.Magnitude - previousData.Magnitude) / TimeConversion.ToTimeUnits((transformedValue.Time - previousData.Time) * SI.Milli, timeUnit);
            //    transformedValue.Angle = (transformedValue.Angle - previousData.Angle) / TimeConversion.ToTimeUnits((transformedValue.Time - previousData.Time) * SI.Milli, timeUnit);

            //    previousData = phasorValue;
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