using GSF.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a single value that represents the Nth order percentile for the sorted values in the source series.
    /// N is a floating point value, representing a percentage, that must range from 0 to 100.
    /// </summary>
    /// <remarks>
    /// Signature: <c>Percentile(N[%], expression)</c><br/>
    /// Returns: Single value.<br/>
    /// Example: <c>Percentile(10%, FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
    /// Variants: Percentile, Pctl<br/>
    /// Execution: Immediate in-memory array load.
    /// </remarks>
    public class Percentile : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Percentile";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of N, or N% of total, values from the start of the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Percentile);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(Percentile|Pctl)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<string>
                {
                    Default = "100",
                    Description = "A floating point value, representing a percentage, that must range from 0 to 100.",
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
            };

        /// <summary>
        /// Used to convert value or percent to the number of points selected
        /// </summary>
        /// <param name="rawValue"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private double ConvertToValue(string rawValue)
        {
            try
            {
                //Percent
                if (rawValue.EndsWith("%"))
                    return Convert.ToDouble(rawValue.TrimEnd('%'));

                //Number
                return Convert.ToDouble(rawValue);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {rawValue} to {typeof(double)}.", ex);
            }
        }

        /// <inheritdoc />
        public DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            string rawValue = (parameters[0] as IParameter<string>).Value;
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            double percent = ConvertToValue(rawValue);

            // Compute
            DataSourceValue selectedElement = dataSourceValues.Source.Last();

            if (percent <= 0)
                selectedElement = dataSourceValues.Source.First();
            else
            {
                List<DataSourceValue> values = dataSourceValues.Source.ToList();
                double n = (dataSourceValues.Source.Count() - 1) * (percent / 100.0D) + 1.0D;
                int k = (int)n;
                if(k >= values.Count()) k = (values.Count() - 1);
                DataSourceValue kData = values[k];
                double d = n - k;
                double k0 = values[k - 1].Value;
                double k1 = kData.Value;

                selectedElement.Value = k0 + d * (k1 - k0);
                selectedElement.Time = kData.Time;
            }

            // Set Values
            dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            dataSourceValues.Source = Enumerable.Repeat(selectedElement, 1);

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
            string rawValue = (parameters[0] as IParameter<string>).Value;
            DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            double percent = ConvertToValue(rawValue);

            // Compute
            PhasorValue selectedElement = phasorValues.Source.Last();
            if (percent <= 0)
                selectedElement = phasorValues.Source.First();
            else
            {
                List<PhasorValue> values = phasorValues.Source.ToList();
                double n = (phasorValues.Source.Count() - 1) * (percent / 100.0D) + 1.0D;
                int k = (int)n;
                PhasorValue kData = values[k];
                double d = n - k;
                double k0 = values[k - 1].Magnitude;
                double k1 = kData.Magnitude;

                selectedElement.Magnitude = k0 + d * (k1 - k0);
                selectedElement.Angle = k0 + d * (k1 - k0);
                selectedElement.Time = kData.Time;
            }
            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = Enumerable.Repeat(selectedElement, 1);

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Percentile"/> class.
        /// </summary>
        public Percentile() { }
    }
}
