using GSF.Collections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a single value that represents the mode of the values in the source series.
    /// </summary>
    /// <remarks>
    /// Signature: <c>Mode(expression)</c><br/>
    /// Returns: Single value.<br/>
    /// Example: <c>Mode(FILTER TOP 5 ActiveMeasurements WHERE SignalType='DIGI')</c><br/>
    /// Variants: Mode<br/>
    /// Execution: Immediate in-memory array load.
    /// </remarks>
    public class Mode : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Mode";

        /// <inheritdoc />
        public string Description { get; } = "Returns a single value that represents the mode of the values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Mode);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "Mode"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

        private double CalculateMode(IEnumerable<double> values)
        {
            if(values.Count() == 0)
            {
                return 0.0;
            }
            var groupedValues = values
                .GroupBy(v => v)
                .Select(group => new { Value = group.Key, Count = group.Count() })
                .ToList();

            int maxCount = groupedValues.Max(g => g.Count);

            return groupedValues.First(g => g.Count == maxCount).Value;
        }

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
            lastElement.Value = CalculateMode(dataSourceValues.Source.Select(pv => pv.Value));

            // Set 
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
            lastElement.Magnitude = CalculateMode(phasorValues.Source.Select(pv => pv.Magnitude));
            lastElement.Angle = CalculateMode(phasorValues.Source.Select(pv => pv.Angle));


            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = Enumerable.Repeat(lastElement, 1); ;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mode"/> class.
        /// </summary>
        public Mode() { }
    }
}
