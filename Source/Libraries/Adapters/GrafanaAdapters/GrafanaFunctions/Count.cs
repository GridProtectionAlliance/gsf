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
    /// <remarks>
    /// Signature: <c>Count(expression)</c><br/>
    /// Returns: Single value.<br/>
    /// Example: <c>Count(PPA:1; PPA:2; PPA:3)</c><br/>
    /// Variants: Count<br/>
    /// Execution: Immediate enumeration.
    /// </remarks>
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
            DataSourceValue lastElement = dataSourceValues.Source.Last();
            lastElement.Value = dataSourceValues.Source
                            .Count();

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
            lastElement.Magnitude = phasorValues.Source.Count();
            lastElement.Angle = phasorValues.Source.Count();

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = Enumerable.Repeat(lastElement, 1);

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Count"/> class.
        /// </summary>
        public Count() { }
    }
}
