using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series.
    /// </summary>
    /// <remarks>
    /// Signature: <c>Ceiling(expression)</c><br/>
    /// Returns: Series of values.<br/>
    /// Example: <c>Ceiling(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
    /// Variants: Ceiling, Ceil<br/>
    /// Execution: Deferred enumeration.
    /// </remarks>
    public class Ceiling : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Ceiling";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Ceiling);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(Ceiling|Ceil)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataSourceValue =>
            {
                DataSourceValue transformedValue = dataSourceValue;
                transformedValue.Value = Math.Ceiling(transformedValue.Value);

                return transformedValue;
            });

            // Set Values
            dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
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

            // Compute
            IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Select(phasorValue =>
            {
                PhasorValue transformedValue = phasorValue;
                transformedValue.Magnitude = Math.Ceiling(transformedValue.Magnitude);
                transformedValue.Angle = Math.Ceiling(transformedValue.Angle);

                return transformedValue;
            });

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = transformedPhasorValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ceiling"/> class.
        /// </summary>
        public Ceiling() { }
    }

}
