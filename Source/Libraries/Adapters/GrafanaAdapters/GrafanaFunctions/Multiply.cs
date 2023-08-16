using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a series of values that represent each of the values in the source series multiplied by N.
    /// N is a floating point value representing a multiplicative factor to be applied to each value the source series.
    /// N can either be constant value or a named target available from the expression.
    /// </summary>
    /// <remarks>
    /// Signature: <c>Multiply(N, expression)</c><br/>
    /// Returns: Series of values.<br/>
    /// Example: <c>Multiply(1.5, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
    /// Variants: Multiply<br/>
    /// Execution: Deferred enumeration.
    /// </remarks>
    public class Multiply : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Multiply";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent each of the values in the source series multiplied by N.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Multiply);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "Multiply"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<double>
                {
                    Default = 0,
                    Description = "A floating point value representing an multiplicative offset to be applied to each value the source series.",
                    Required = true,
                    ParameterTypeName = "string"
                },
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true,
                    ParameterTypeName = "data"
                }
            };

        /// <inheritdoc />
        public DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            double value = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

            // Compute
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataSourceValue =>
            {
                DataSourceValue transformedValue = dataSourceValue;
                transformedValue.Value *= value;

                return transformedValue;
            });

            // Set Values
            dataSourceValues.Target = $"{value}*{dataSourceValues.Target}";
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
                transformedValue.Magnitude *= value;

                return transformedValue;
            });

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{value}*{labels[0]};{value}*{labels[1]}";
            phasorValues.Source = transformedPhasorValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Multiply"/> class.
        /// </summary>
        public Multiply() { }
    }
}
