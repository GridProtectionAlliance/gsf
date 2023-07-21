using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a single value that represents the mean of the values in the source series.
    /// </summary>
    public class Average : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Average";

        /// <inheritdoc />
        public string Description { get; } = "Returns a single value that represents the mean of the values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Average);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(Average|Avg|Mean)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            double sumOfValues = dataSourceValues.Source.Sum(dataValue => dataValue.Value);
            DataSourceValue averageDataSourceValue = new()
            {
                Time = dataSourceValues.Source.Last().Time,
                Value = sumOfValues / dataSourceValues.Source.Count()
            };

            dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            dataSourceValues.Source = Enumerable.Repeat(averageDataSourceValue, 1);

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
            double sumOfMags = phasorValues.Source.Sum(dataValue => dataValue.Magnitude);
            double sumOfAngs = phasorValues.Source.Sum(dataValue => dataValue.Angle);
            PhasorValue averagePhasorValue = new()
            {
                Time = phasorValues.Source.Last().Time,
                Magnitude = sumOfMags / phasorValues.Source.Count(),
                Angle = sumOfMags / phasorValues.Source.Count()
            };


            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = Enumerable.Repeat(averagePhasorValue, 1);

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Average"/> class.
        /// </summary>
        public Average() { }
    }
}
