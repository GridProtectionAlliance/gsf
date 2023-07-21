using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a series of values that represent the rounded value, with N fractional digits, of each of the values in the source series.
    /// N, optional, is a positive integer value representing the number of decimal places in the return value - defaults to 0.
    /// </summary>
    public class Round : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Round";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent the rounded value, with N fractional digits, of each of the values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(AbsoluteValue);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "Round"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                new Parameter<int>
                {
                    Default = 0,
                    Description = "A positive integer value representing the number of decimal places in the return value - defaults to 0.",
                    Required = false,
                    ParameterTypeName = "int"
                },
            };

        /// <inheritdoc />
        public DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            int numberDecimals = (parameters[1] as IParameter<int>).Value;

            // Compute
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataValue =>
                new DataSourceValue
                {
                    Flags = dataValue.Flags,
                    Value = Math.Round(dataValue.Value, numberDecimals),
                    Time = dataValue.Time,
                    Target = dataValue.Target
                });

            dataSourceValues.Target = $"Round({numberDecimals},{dataSourceValues.Target})";
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
            int numberDecimals = (parameters[1] as IParameter<int>).Value;

            // Compute
            IEnumerable<PhasorValue> transformedDataSourceValues = phasorValues.Source.Select(phasorValue =>
                new PhasorValue
                {
                    Flags = phasorValue.Flags,
                    Magnitude = Math.Round(phasorValue.Magnitude, numberDecimals),
                    Angle = Math.Round(phasorValue.Angle, numberDecimals),
                    Time = phasorValue.Time,
                    MagnitudeTarget = phasorValue.MagnitudeTarget,
                    AngleTarget = phasorValue.AngleTarget,
                });

            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]},{numberDecimals});{Name}({labels[1]},{numberDecimals})";
            phasorValues.Source = transformedDataSourceValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Round"/> class.
        /// </summary>
        public Round() { }
    }
}
