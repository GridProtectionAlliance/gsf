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
                new Parameter<double>
                {
                    Default = 0,
                    Description = "A floating point value representing an additive offset to be applied to each value the source series.",
                    Required = true,
                    ParameterTypeName = "string"
                },
                new Parameter<IEnumerable<DataSourceValue>>
                {
                    Default = Enumerable.Empty<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true,
                    ParameterTypeName = "data"
                }
            };

        /// <inheritdoc />
        public IEnumerable<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            double value = (parameters[0] as IParameter<double>).Value;
            IEnumerable<DataSourceValue> dataSourceValues = (parameters[1] as IParameter<IEnumerable<DataSourceValue>>).Value;

            // Get Values
            //double value = double.Parse(values[0].ToString());
            //IEnumerable<DataSourceValue> dataSourceValues = (IEnumerable<DataSourceValue>)values[1];

            //// Compute
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
                    ParameterTypeName = "data"
                }
            };

        /// <inheritdoc />
        public IEnumerable<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            //IEnumerable<DataSourceValue> dataSourceValues = (IEnumerable<DataSourceValue>)values[0];
            IEnumerable<DataSourceValue> dataSourceValues = (parameters[0] as IParameter<IEnumerable<DataSourceValue>>).Value;
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


    /// <summary>
    /// Represents the "Round" function that rounds the value of a DataSourceValue.
    /// </summary>
    public class Round : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Round";

        /// <inheritdoc />
        public string Description { get; } = "Rounds the value of DataSourceValue";

        /// <inheritdoc />
        public Type Type { get; } = typeof(AbsoluteValue);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "Round"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<int>
                {
                    Default = 0,
                    Description = "A positive integer value representing the number of decimal places in the return value - defaults to 0.",
                    Required = false,
                    ParameterTypeName = "int"
                },
                new Parameter<IEnumerable<DataSourceValue>>
                {
                    Default = Enumerable.Empty<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true,
                    ParameterTypeName = "data"
                }
            };

        /// <inheritdoc />
        public IEnumerable<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            int numberDecimals = (parameters[0] as IParameter<int>).Value;
            IEnumerable<DataSourceValue> dataSourceValues = (parameters[1] as IParameter<IEnumerable<DataSourceValue>>).Value;

            // Compute
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Select(dataValue =>
                new DataSourceValue
                {
                    Value = Math.Round(dataValue.Value, numberDecimals),
                    Time = dataValue.Time,
                    Target = dataValue.Target
                });

            return transformedDataSourceValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbsoluteValue"/> class.
        /// </summary>
        public Round() { }
    }
}
