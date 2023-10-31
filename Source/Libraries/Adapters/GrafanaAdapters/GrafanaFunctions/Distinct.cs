using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a series of values that represent the unique set of values in the source series.
    /// </summary>
    /// <remarks>
    /// Signature: <c>Distinct(expression)</c><br/>
    /// Returns: Series of values.<br/>
    /// Example: <c>Distinct(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
    /// Variants: Distinct, Unique<br/>
    /// Execution: Deferred enumeration.
    /// </remarks>
    public class Distinct : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Distinct";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent the unique set of values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Distinct);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(Distinct|Unique)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            IEnumerable<DataSourceValue> distinctValues = dataSourceValues.Source.GroupBy(dataValue => dataValue.Value).Select(group => group.First());

            // Set Values
            dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            dataSourceValues.Source = distinctValues;

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
            IEnumerable<PhasorValue> distinctValues = phasorValues.Source.GroupBy(dataValue => dataValue.Magnitude).Select(group => group.First());

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = distinctValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Distinct"/> class.
        /// </summary>
        public Distinct() { }
    }
}
