using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a series of values that represent the integral part of each of the values in the source series.
    /// </summary>
    /// <remarks>
    /// Signature: <c>Truncate(expression)</c><br/>
    /// Returns: Series of values.<br/>
    /// Example: <c>Truncate(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
    /// Variants: Truncate, Trunc<br/>
    /// Execution: Deferred enumeration.
    /// </remarks>
    public class Truncate : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Truncate";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent the integral part of each of the values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Truncate);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(Truncate|Trunc)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                transformedValue.Value = Math.Truncate(transformedValue.Value);

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
                transformedValue.Magnitude = Math.Truncate(transformedValue.Magnitude);
                transformedValue.Angle = Math.Truncate(transformedValue.Angle);

                return transformedValue;
            });

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = transformedPhasorValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Truncate"/> class.
        /// </summary>
        public Truncate() { }
    }

}
