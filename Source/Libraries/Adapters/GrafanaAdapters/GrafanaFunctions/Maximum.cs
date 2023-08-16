using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a single value that is the maximum of the values in the source series.
    /// </summary>
    /// <remarks>
    /// Signature: <c>Maximum(expression)</c><br/>
    /// Returns: Single value.<br/>
    /// Example: <c>Maximum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
    /// Variants: Maximum, Max<br/>
    /// Execution: Immediate enumeration.
    /// </remarks>
    public class Maximum : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Maximum";

        /// <inheritdoc />
        public string Description { get; } = " Returns a single value that is the maximum of the values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Maximum);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(Maximum|Max)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            DataSourceValue minimumDataSourceValue = dataSourceValues.Source.OrderBy(dataValue => dataValue.Value).Last();

            // Set Values
            dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            dataSourceValues.Source = Enumerable.Repeat(minimumDataSourceValue, 1);

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
            PhasorValue minimumPhasorValue = phasorValues.Source.OrderBy(dataValue => dataValue.Magnitude).Last();

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = Enumerable.Repeat(minimumPhasorValue, 1);

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Maximum"/> class.
        /// </summary>
        public Maximum() { }
    }
}
