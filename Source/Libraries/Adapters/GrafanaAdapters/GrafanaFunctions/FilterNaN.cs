using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN.
    /// Second parameter, optional, is a boolean flag that determines if infinite values should also be excluded - defaults to true.
    /// </summary>
    /// <remarks>
    /// Signature: <c>FilterNaN(expression, [alsoFilterInfinity = true])</c><br/>
    /// Returns: Series of values.<br/>
    /// Example: <c>FilterNaN(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
    /// Variants: FilterNaN<br/>
    /// Execution: Deferred enumeration.
    /// </remarks> 
    public class FilterNaN : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "FilterNaN";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(FilterNaN);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "FilterNaN"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                },
                new Parameter<bool>
                {
                    Default = true,
                    Description = "A boolean flag that determines if infinite values should also be excluded - defaults to true",
                    Required = false,
                    ParameterTypeName = "boolean"
                },
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
            bool filterInfinity = (parameters[1] as IParameter<bool>).Value;

            // Compute
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Where(dataValue =>
                !(
                  double.IsNaN(dataValue.Value) ||
                  (filterInfinity && double.IsInfinity(dataValue.Value)
                )));

            // Set Values
            dataSourceValues.Target = $"{Name}({dataSourceValues.Target},{filterInfinity})";
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
            bool filterInfinity = (parameters[1] as IParameter<bool>).Value;

            // Compute
            IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Where(phasorValue =>
                !(
                  double.IsNaN(phasorValue.Magnitude) ||
                  double.IsNaN(phasorValue.Angle) ||
                  (filterInfinity && (double.IsInfinity(phasorValue.Magnitude) || double.IsInfinity(phasorValue.Angle)))
                ));

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]},{filterInfinity});{Name}({labels[1]},{filterInfinity})";
            phasorValues.Source = transformedPhasorValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterNaN"/> class.
        /// </summary>
        public FilterNaN() { }
    }

}
