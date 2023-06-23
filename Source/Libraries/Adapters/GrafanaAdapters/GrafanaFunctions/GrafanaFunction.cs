using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Represents the "Add" function that adds a decimal number to a DataSourceValue.
    /// </summary>
    public class Add : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Add";

        /// <inheritdoc />
        public string Description { get; } = "Adds a decimal number to DataSourceValue";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Add);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "Add"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<decimal>
                {
                    Default = 0,
                    Description = "Decimal number to add",
                    Required = true,
                },
                new Parameter<IEnumerable<DataSourceValue>>
                {
                    Default = Enumerable.Empty<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true,
                }
            };

        /// <inheritdoc />
        public IEnumerable<DataSourceValue> Compute(object[] values)
        {
            // Verify Values
            //FunctionsModelHelper.CheckValuesIntegrity(this, values);

            // Get Values
            double value = double.Parse(values[0].ToString());
            IEnumerable<DataSourceValue> dataSourceValues = (IEnumerable<DataSourceValue>)values[1];

            // Compute
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Select(dataValue =>
                new DataSourceValue
                {
                    Value = value + dataValue.Value,
                    Time = dataValue.Time,
                    Target = dataValue.Target
                });

            return transformedDataSourceValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Add"/> class.
        /// </summary>
        public Add() { }
    }

    /// <summary>
    /// Represents the "AbsoluteValue" function that takes the absolute value of a DataSourceValue.
    /// </summary>
    public class AbsoluteValue : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "AbsoluteValue";

        /// <inheritdoc />
        public string Description { get; } = "Takes absolute value of DataSourceValue";

        /// <inheritdoc />
        public Type Type { get; } = typeof(AbsoluteValue);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(AbsoluteValue|Abs)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<IEnumerable<DataSourceValue>>
                {
                    Default = Enumerable.Empty<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true,
                }
            };

        /// <inheritdoc />
        public IEnumerable<DataSourceValue> Compute(object[] values)
        {
            // Verify Values
            //FunctionsModelHelper.CheckValuesIntegrity(this, values);

            // Get Values
            IEnumerable<DataSourceValue> dataSourceValues = (IEnumerable<DataSourceValue>)values[0];

            // Compute
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Select(dataValue =>
                new DataSourceValue
                {
                    Value = Math.Abs(dataValue.Value),
                    Time = dataValue.Time,
                    Target = dataValue.Target
                });

            return transformedDataSourceValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbsoluteValue"/> class.
        /// </summary>
        public AbsoluteValue() { }
    }
}
