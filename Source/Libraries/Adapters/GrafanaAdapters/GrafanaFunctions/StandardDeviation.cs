using GSF;
using GSF.NumericalAnalysis;
using GSF.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a single value that represents the standard deviation of the values in the source series. Second parameter,
    /// optional, is a boolean flag representing if the sample based calculation should be used - defaults to false, which
    /// means the population based calculation should be used.
    /// </summary>
    /// <remarks>
    /// Signature: <c>StandardDeviation([useSampleCalc = false], expression)</c><br/>
    /// Returns: Single value.<br/>
    /// Example: <c>StandardDeviation(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
    /// Variants: StandardDeviation, StdDev<br/>
    /// Execution: Immediate in-memory array load.
    /// </remarks>
    public class StandardDeviation : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "StandardDeviation";

        /// <inheritdoc />
        public string Description { get; } = "Returns a single value that represents the standard deviation of the values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(StandardDeviation);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(StandardDeviation|StdDev)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true
                },
                new Parameter<bool>
                {
                    Default = false,
                    Description = "A boolean flag representing if the sample based calculation should be used.",
                    Required = false
                },
            };

        /// <inheritdoc />
        public DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            bool useSample = (parameters[1] as IParameter<bool>).Value;

            // Compute
            DataSourceValue lastElement = dataSourceValues.Source.Last();
            lastElement.Value = dataSourceValues.Source
                            .Select(dataValue => { return dataValue.Value; })
                            .StandardDeviation(useSample);

            // Set Values
            dataSourceValues.Target = $"{Name}({dataSourceValues.Target},{useSample})";
            dataSourceValues.Source = Enumerable.Repeat(lastElement, 1);

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
            bool useSample = (parameters[1] as IParameter<bool>).Value;

            // Compute
            PhasorValue lastElement = phasorValues.Source.Last();
            lastElement.Magnitude = phasorValues.Source
                            .Select(dataValue => { return dataValue.Magnitude; })
                            .StandardDeviation(useSample);

            lastElement.Angle = phasorValues.Source
                            .Select(dataValue => { return dataValue.Angle; })
                            .StandardDeviation(useSample);

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]},{useSample});{Name}({labels[1]},{useSample})";
            phasorValues.Source = Enumerable.Repeat(lastElement, 1);

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardDeviation"/> class.
        /// </summary>
        public StandardDeviation() { }
    }
}
