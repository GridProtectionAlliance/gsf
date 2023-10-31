using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a single value that represents the range, i.e., <c>maximum - minimum</c>, of the values in the source series.
    /// </summary>
    /// <remarks>
    /// Signature: <c>Range(expression)</c><br/>
    /// Returns: Single value.<br/>
    /// Example: <c>Range(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
    /// Variants: Range<br/>
    /// Execution: Immediate enumeration.
    /// </remarks>
    public class Range : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Range";

        /// <inheritdoc />
        public string Description { get; } = "Returns a single value that represents the range, i.e., <c>maximum - minimum</c>, of the values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Range);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "Range"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            IEnumerable<DataSourceValue> orderedValues = dataSourceValues.Source.OrderBy(dataValue => dataValue.Value);
            DataSourceValue lastElement = dataSourceValues.Source.Last();
            lastElement.Value = orderedValues.Last().Value - orderedValues.First().Value;

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
            IEnumerable<PhasorValue> orderedMag = phasorValues.Source.OrderBy(dataValue => dataValue.Magnitude);
            IEnumerable<PhasorValue> orderedAng = phasorValues.Source.OrderBy(dataValue => dataValue.Angle);

            PhasorValue lastElement = phasorValues.Source.Last();
            lastElement.Magnitude = orderedMag.Last().Magnitude - orderedMag.First().Magnitude;
            lastElement.Angle = orderedAng.Last().Angle - orderedAng.First().Angle;

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = Enumerable.Repeat(lastElement, 1);

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Range"/> class.
        /// </summary>
        public Range() { }
    }
}
