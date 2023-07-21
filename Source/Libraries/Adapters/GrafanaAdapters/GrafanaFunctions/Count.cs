using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a single value that is the count of the values in the source series.
    /// </summary>
    public class Count : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Count";

        /// <inheritdoc />
        public string Description { get; } = "Returns a single value that is the count of the values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Count);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "Count"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            IEnumerable<DataSourceValue> orderedValues = dataSourceValues.Source.OrderBy(dataValue => dataValue.Value);

            DataSourceValue dataSourceValue = new()
            {
                Time = orderedValues.Last().Time,
                Value = orderedValues.Count()
            };

            dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            dataSourceValues.Source = Enumerable.Repeat(dataSourceValue, 1);

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
            IEnumerable<PhasorValue> orderedValues = phasorValues.Source.OrderBy(dataValue => dataValue.Magnitude);
            PhasorValue phasorValue = new()
            {
                Time = orderedValues.Last().Time,
                Magnitude = orderedValues.Count(),
                Angle = orderedValues.Count()
            };

            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = Enumerable.Repeat(phasorValue, 1);

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Count"/> class.
        /// </summary>
        public Count() { }
    }
}
