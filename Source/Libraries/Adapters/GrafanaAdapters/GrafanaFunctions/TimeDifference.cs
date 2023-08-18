using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GrafanaAdapters.GrafanaFunctionBase;
using GSF.Units;


namespace GrafanaAdapters.GrafanaFunctions
{
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
    public class TimeDifference : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "TimeDifference";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent the time difference, in time units, between consecutive values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(TimeDifference);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(TimeDifference|TimeDiff|Elapsed)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                        Unit = TimeUnit.Seconds,
                        Factor = SI.Nano
                    },
                    Description = "Specifies the type of time units and must be one of the following: Seconds, Nanoseconds, Microseconds, Milliseconds, " +
                                  "Minutes, Hours, Days, Weeks, Ke (i.e., traditional Chinese unit of decimal time), Ticks (i.e., 100-nanosecond intervals), PlanckTime or " +
                                  "AtomicUnitsOfTime - defaults to Seconds.",
                    Required = false,
                    ParameterTypeName = "time"
                }
            };

        private static double ToTimeUnits(Time value, TargetTimeUnit target)
        {
            double time = value.ConvertTo(target.Unit);

            if (!double.IsNaN(target.Factor))
                time /= target.Factor;

            return time;
        }

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
            double previousTime = dataSourceValues.Source.First().Time;
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source
                .Skip(1)
                .Select(dataSourceValue =>
                {
                    DataSourceValue transformedValue = dataSourceValue;
                    transformedValue.Value = ToTimeUnits((dataSourceValue.Time - previousTime) * SI.Milli, timeUnit);

                    previousTime = dataSourceValue.Time;
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
            double previousTime = phasorValues.Source.First().Time;
            IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source
                .Skip(1)
                .Select(phasorValue =>
                {
                    PhasorValue transformedValue = phasorValue;

                    double timeDifference = ToTimeUnits((phasorValue.Time - previousTime) * SI.Milli, timeUnit);
                    transformedValue.Magnitude = timeDifference;
                    transformedValue.Angle = timeDifference;

                    previousTime = phasorValue.Time;
                    return transformedValue;
                });

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]},{timeUnit.Unit});{Name}({labels[1]},{timeUnit.Unit})";
            phasorValues.Source = transformedPhasorValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeDifference"/> class.
        /// </summary>
        public TimeDifference() { }
    }
}
