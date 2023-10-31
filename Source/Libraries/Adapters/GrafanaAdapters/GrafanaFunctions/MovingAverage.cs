using GrafanaAdapters.GrafanaFunctionBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GSF.Units;
using GSF.Drawing;

namespace GrafanaAdapters.GrafanaFunctions
{
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
    public class MovingAverage : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "MovingAverage";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of value that represent the moving average of the initial data.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(MovingAverage);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(MovingAverage|RollingAverage|MovingAvg|RollingAvg|MovingMean|RollingMean)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<double>
                {
                    Default = 0,
                    Description = "A floating point value representing the time interval to average.",
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
            double timeValue = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            TargetTimeUnit timeUnit = (parameters[2] as IParameter<TargetTimeUnit>).Value;
            
            Time timeObj = TimeConversion.FromTimeUnits(timeValue, timeUnit);
            double tolerance = timeObj.ConvertTo(TimeUnit.Seconds);

            List<DataSourceValue> dataSourceValuesList = dataSourceValues.Source.ToList();
            List<DataSourceValue> movingAverageList = new();

            // Compute
            TimeSliceScanner<DataSourceValue> scanner = new(new List<DataSourceValueGroup<DataSourceValue>> { dataSourceValues }, tolerance / SI.Milli);
            while (!scanner.DataReadComplete)
            {
                IEnumerable<DataSourceValue> dataPointGroups = scanner.ReadNextTimeSlice();
                if (dataPointGroups.Count() == 0)
                    continue;

                DataSourceValue transformedValue = dataPointGroups.Last();
                transformedValue.Value = dataPointGroups.Select(dataValue => { return dataValue.Value; }).Average();
                transformedValue.Time = dataPointGroups.Select(dataValue => { return dataValue.Time; }).Average();

                movingAverageList.Add(transformedValue);
            }

            // Set Values
            dataSourceValues.Target = $"{Name}({dataSourceValues.Target},{timeUnit.Unit})";
            dataSourceValues.Source = movingAverageList;

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
            double timeValue = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            TargetTimeUnit timeUnit = (parameters[2] as IParameter<TargetTimeUnit>).Value;

            Time timeObj = TimeConversion.FromTimeUnits(timeValue, timeUnit);
            double tolerance = timeObj.ConvertTo(TimeUnit.Seconds);

            List<PhasorValue> phasorSourceValuesList = phasorValues.Source.ToList();
            List<PhasorValue> movingAverageList = new();

            // Compute
            TimeSliceScanner<PhasorValue> scanner = new(new List<DataSourceValueGroup<PhasorValue>> { phasorValues }, tolerance / SI.Milli);
            while (!scanner.DataReadComplete)
            {
                IEnumerable<PhasorValue> dataPointGroups = scanner.ReadNextTimeSlice();
                if (dataPointGroups.Count() == 0)
                    continue;

                PhasorValue transformedValue = dataPointGroups.Last();
                transformedValue.Magnitude = dataPointGroups.Select(dataValue => { return dataValue.Magnitude; }).Average();
                transformedValue.Angle = dataPointGroups.Select(dataValue => { return dataValue.Angle; }).Average();
                transformedValue.Time = dataPointGroups.Select(dataValue => { return dataValue.Time; }).Average();

                movingAverageList.Add(transformedValue);
            }

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]},{timeUnit.Unit});{Name}({labels[1]},{timeUnit.Unit})";
            phasorValues.Source = movingAverageList;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MovingAverage"/> class.
        /// </summary>
        public MovingAverage() { }
    }

}
