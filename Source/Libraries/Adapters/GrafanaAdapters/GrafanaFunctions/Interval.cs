using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GrafanaAdapters.GrafanaFunctionBase;
using GSF.Units;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace GrafanaAdapters.GrafanaFunctions
{
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
    public class Interval : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Interval";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent a decimated set of the values in the source series based on the specified interval N, in time units.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Interval);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "Interval"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<double>
                {
                    Default = 0,
                    Description = "A floating-point value that must be greater than or equal to zero that represents the desired time interval",
                    Required = true
                },
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true
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
                    Required = false
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
            double value = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            TargetTimeUnit timeUnit = (parameters[2] as IParameter<TargetTimeUnit>).Value;

            if(value < 0)
                throw new ArgumentException($"{value} must be greater than or equal to 0.");

            // Compute
            double timeValue = TimeConversion.FromTimeUnits(value, timeUnit) / SI.Milli;

            double previousTime = dataSourceValues.Source.First().Time;
            List<DataSourceValue> transformedDataSourceValues = new();
            foreach (DataSourceValue dataSourceValue in dataSourceValues.Source.Skip(1))
            {
                if (dataSourceValue.Time - previousTime > timeValue)
                {
                    previousTime = dataSourceValue.Time;
                    transformedDataSourceValues.Add(dataSourceValue);
                }
            }

            // Set Values
            dataSourceValues.Target = $"{Name}({value},{dataSourceValues.Target},{timeUnit.Unit})";
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
            double value = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            TargetTimeUnit timeUnit = (parameters[2] as IParameter<TargetTimeUnit>).Value;
            
            if (value < 0)
                throw new ArgumentException($"{value} must be greater than or equal to 0.");

            // Compute
            double timeValue = TimeConversion.FromTimeUnits(value, timeUnit) / SI.Milli;

            double previousTime = phasorValues.Source.First().Time;
            List<PhasorValue> transformedPhasorValues = new();
            foreach (PhasorValue phasorValue in phasorValues.Source.Skip(1))
            {
                if (phasorValue.Time - previousTime > timeValue)
                {
                    previousTime = phasorValue.Time;
                    transformedPhasorValues.Add(phasorValue);
                }
            }

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]},{timeUnit.Unit});{Name}({labels[1]},{timeUnit.Unit})";
            phasorValues.Source = transformedPhasorValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Interval"/> class.
        /// </summary>
        public Interval() { }
    }
}
