using GrafanaAdapters.GrafanaFunctionBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GSF.Units;

namespace GrafanaAdapters.GrafanaFunctions
{
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
    public class Derivative : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Derivative";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent the rate of change, per time units, for the difference between consecutive values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Derivative);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(Derivative|Der)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true,
                    ParameterTypeName = "data"
                },
                new Parameter<TargetTimeUnit>
                {
                    Default = new TargetTimeUnit
                    {
                        Unit = TimeUnit.Seconds
                    },
                    Description = "Specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, " +
                                  "Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or " +
                                  "AtomicUnitsOfTime - defaults to Seconds.",
                    Required = false,
                    ParameterTypeName = "time"
                }
            };

        /// <summary>
        /// Computes based on type DataSourceValue
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            TargetTimeUnit timeUnit = (parameters[1] as IParameter<TargetTimeUnit>).Value;

            // Compute
            DataSourceValue previousData = dataSourceValues.Source.First();
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source
                .Skip(1)
                .Select(dataSourceValue =>
                {
                    DataSourceValue transformedValue = dataSourceValue;

                    transformedValue.Value = (transformedValue.Value - previousData.Value) / TimeConversion.ToTimeUnits((transformedValue.Time - previousData.Time) * SI.Milli, timeUnit);

                    previousData = dataSourceValue;
                    return transformedValue;
                });

            // Set Values
            dataSourceValues.Target = $"{Name}({dataSourceValues.Target},{timeUnit.Unit})";
            dataSourceValues.Source = transformedDataSourceValues;

            return dataSourceValues;
        }

        /// <summary>
        /// Computes based on type PhasorValue
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
        {
            // Get Values
            DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            TargetTimeUnit timeUnit = (parameters[1] as IParameter<TargetTimeUnit>).Value;

            // Compute
            PhasorValue previousData = phasorValues.Source.First();
            IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source
                .Skip(1)
                .Select(phasorValue =>
                {
                    PhasorValue transformedValue = phasorValue;

                    transformedValue.Magnitude = (transformedValue.Magnitude - previousData.Magnitude) / TimeConversion.ToTimeUnits((transformedValue.Time - previousData.Time) * SI.Milli, timeUnit);
                    transformedValue.Angle = (transformedValue.Angle - previousData.Angle) / TimeConversion.ToTimeUnits((transformedValue.Time - previousData.Time) * SI.Milli, timeUnit);

                    previousData = phasorValue;
                    return transformedValue;
                });

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]},{timeUnit.Unit});{Name}({labels[1]},{timeUnit.Unit})";
            phasorValues.Source = transformedPhasorValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Derivative"/> class.
        /// </summary>
        public Derivative() { }
    }

}
