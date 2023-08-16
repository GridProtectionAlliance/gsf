using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a single value that represents the mean of the values in the source series.
    /// </summary>
    /// <remarks>
    /// Signature: <c>Average(expression)</c><br/>
    /// Returns: Single value.<br/>
    /// Example: <c>Average(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
    /// Variants: Average, Avg, Mean<br/>
    /// Execution: Immediate enumeration.
    /// </remarks>
    public class Average : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Average";

        /// <inheritdoc />
        public string Description { get; } = "Returns a single value that represents the mean of the values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Average);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(Average|Avg|Mean)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

            // Compute
            DataSourceValue lastElement = dataSourceValues.Source.Last();
            lastElement.Value = dataSourceValues.Source
                            .Select(dataValue => { return dataValue.Value; })
                            .Average();

            // Set Values
            dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
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

            // Compute
            PhasorValue lastElement = phasorValues.Source.Last();
            lastElement.Magnitude = phasorValues.Source
                            .Select(dataValue => { return dataValue.Magnitude; })
                            .Average();
            lastElement.Angle = phasorValues.Source
                            .Select(dataValue => { return dataValue.Angle; })
                            .Average();

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = Enumerable.Repeat(lastElement, 1);

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Average"/> class.
        /// </summary>
        public Average() { }
    }
}
