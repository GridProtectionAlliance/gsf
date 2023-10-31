using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a single value that represents the sum of the values in the source series.
    /// </summary>
    /// <remarks>
    /// Signature: <c>Total(expression)</c><br/>
    /// Returns: Single value.<br/>
    /// Example: <c>Total(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
    /// Variants: Total, Sum<br/>
    /// Execution: Immediate enumeration.
    /// </remarks>
    public class Total : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Total";

        /// <inheritdoc />
        public string Description { get; } = "Returns a single value that represents the sum of the values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Total);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(Total|Sum)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true
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
                            .Sum();

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
                            .Sum();

            lastElement.Angle = phasorValues.Source
                            .Select(dataValue => { return dataValue.Angle; })
                            .Sum();

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = Enumerable.Repeat(lastElement, 1);

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Total"/> class.
        /// </summary>
        public Total() { }
    }
}
