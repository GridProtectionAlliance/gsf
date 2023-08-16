using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a single value that is the minimum of the values in the source series.
    /// </summary>
    public class Minimum : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Minimum";

        /// <inheritdoc />
        public string Description { get; } = "Returns a single value that is the minimum of the values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Minimum);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(Minimum|Min)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            DataSourceValue minimumDataSourceValue = dataSourceValues.Source.OrderBy(dataValue => dataValue.Value).First();

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
            PhasorValue minimumPhasorValue = phasorValues.Source.OrderBy(dataValue => dataValue.Magnitude).First();

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = Enumerable.Repeat(minimumPhasorValue, 1);

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Minimum"/> class.
        /// </summary>
        public Minimum() { }
    }
}
