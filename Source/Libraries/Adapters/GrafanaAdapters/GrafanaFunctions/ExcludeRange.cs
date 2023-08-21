using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a series of values that represent a filtered set of the values in the source series where each value falls outside the specified low and high.
    /// The low and high parameter values are floating-point numbers that represent the range of values excluded in the return series. Fourth parameter, optional,
    /// is a boolean flag that determines if range values are inclusive, i.e., excluded values are &lt;= low or &gt;= high - defaults to false, which means
    /// values are exclusive, i.e., excluded values are &lt; low or &gt; high. Function allows a fifth optional parameter that is a boolean flag - when four
    /// parameters are provided, fourth parameter determines if low value is inclusive and fifth parameter determines if high value is inclusive.
    /// The low and high parameter values can either be constant values or named targets available from the expression.
    /// </summary>
    /// <remarks>
    /// Signature: <c>ExcludeRange(low, high, expression, [lowInclusive = false], [highInclusive = false])</c><br/>
    /// Returns: Series of values.<br/>
    /// Example: <c>ExcludeRange(-180.0, 180.0, true, false, FILTER ActiveMeasurements WHERE SignalType LIKE '%PHA')</c><br/>
    /// Variants: ExcludeRange, Exclude<br/>
    /// Execution: Deferred enumeration.
    /// </remarks>
    public class ExcludeRange : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "ExcludeRange";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent a filtered set of the values in the source series where each value falls outside the specified low and high.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(ExcludeRange);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(ExcludeRange|Exclude)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<double>
                {
                    Default = 0,
                    Description = "A floating point value representing the low end of the range allowed in the return series.",
                    Required = true,
                    ParameterTypeName = "string"
                },
                new Parameter<double>
                {
                    Default = 0,
                    Description = "A floating point value representing the high end of the range allowed in the return series.",
                    Required = true,
                    ParameterTypeName = "string"
                },
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true,
                    ParameterTypeName = "data"
                },
                new Parameter<bool>
                {
                    Default = false,
                    Description = "A boolean flag which determines if low value is inclusive.",
                    Required = false,
                    ParameterTypeName = "boolean"
                },
                new Parameter<bool>
                {
                    Default = false,
                    Description = "A boolean flag which determines if high value is inclusive.",
                    Required = false,
                    ParameterTypeName = "boolean"
                },
            };

        /// <inheritdoc />
        public DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            double low = (parameters[0] as IParameter<double>).Value;
            double high = (parameters[1] as IParameter<double>).Value;
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[2] as IParameter<IDataSourceValueGroup>).Value;
            bool lowInclusive = (parameters[3] as IParameter<bool>).Value;
            bool highInclusive = (parameters[4] as IParameter<bool>).Value;

            // Compute
            IEnumerable<DataSourceValue> selectedValues =
                dataSourceValues.Source.Where(dataValue => 
                (lowInclusive ? dataValue.Value <= low : dataValue.Value < low) || 
                (highInclusive ? dataValue.Value >= high : dataValue.Value > high));

            // Set Values
            dataSourceValues.Target = $"{low},{high},{dataSourceValues.Target},{lowInclusive},{highInclusive}";
            dataSourceValues.Source = selectedValues;

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
            double low = (parameters[0] as IParameter<double>).Value;
            double high = (parameters[1] as IParameter<double>).Value;
            DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[2] as IParameter<IDataSourceValueGroup>).Value;
            bool lowInclusive = (parameters[3] as IParameter<bool>).Value;
            bool highInclusive = (parameters[4] as IParameter<bool>).Value;

            // Compute
            IEnumerable<PhasorValue> selectedValues =
                phasorValues.Source.Where(phasorValue =>
                (lowInclusive ? phasorValue.Magnitude <= low : phasorValue.Magnitude < low) ||
                (highInclusive ? phasorValue.Magnitude >= high : phasorValue.Magnitude > high));

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{low},{high},{labels[0]},{lowInclusive},{highInclusive};{low},{high},{labels[1]},{lowInclusive},{highInclusive}";
            phasorValues.Source = selectedValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludeRange"/> class.
        /// </summary>
        public ExcludeRange() { }
    }
}
