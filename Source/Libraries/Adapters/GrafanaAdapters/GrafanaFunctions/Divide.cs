using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a series of values that represent each of the values in the source series divided by N.
    /// N is a floating point value representing a divisive factor to be applied to each value the source series.
    /// N can either be constant value or a named target available from the expression.
    /// </summary>
    /// <remarks>
    /// Signature: <c>Divide(N, expression)</c><br/>
    /// Returns: Series of values.<br/>
    /// Example: <c>Divide(1.732, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
    /// Variants: Divide<br/>
    /// Execution: Deferred enumeration.
    /// </remarks>
    public class Divide : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Divide";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent each of the values in the source series divided with N.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Divide);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "Divide"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<double>
                {
                    Default = 0,
                    Description = "A floating point value representing an divisive offset to be applied to each value the source series.",
                    Required = true
                },
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
            double value = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

            // Compute
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataSourceValue =>
            {
                DataSourceValue transformedValue = dataSourceValue;
                transformedValue.Value /= value;

                return transformedValue;
            });

            // Set Values
            dataSourceValues.Target = $"{dataSourceValues.Target}/{value}";
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
            double value = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

            // Compute
            IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Select(phasorValue =>
            {
                PhasorValue transformedValue = phasorValue;
                transformedValue.Magnitude /= value;

                return transformedValue;
            });

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{labels[0]}/{value};{labels[1]}/{value}";
            phasorValues.Source = transformedPhasorValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Divide"/> class.
        /// </summary>
        public Divide() { }
    }
}
