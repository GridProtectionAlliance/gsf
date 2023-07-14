using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.Units;

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
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataValue =>
                new DataSourceValue
                {
                    Value = value + dataValue.Value,
                    Time = dataValue.Time,
                    Target = dataValue.Target
                });

            dataSourceValues.Target = $"{value}+{dataSourceValues.Target}";
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
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Add"/> class.
        /// </summary>
        public Add() { }
    }

    /// <summary>
    /// Represents the "SliceAdd" function that adds two DataSourceValues.
    /// </summary>
    public class SliceAdd : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "SliceAdd";

        /// <inheritdoc />
        public string Description { get; } = "Adds two DataSourceValue together to a certain degree of tolerance";

        /// <inheritdoc />
        public Type Type { get; } = typeof(SliceAdd);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "SliceAdd"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<double>
                {
                    Default = 0.033,
                    Description = "The level of tolerance.",
                    Required = true,
                    ParameterTypeName = "string"
                },
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "First datapoint",
                    Required = true,
                    ParameterTypeName = "data"
                },
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "Second datapoint",
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
            double tolerance = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<DataSourceValue> firstData = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            DataSourceValueGroup<DataSourceValue> secondData = (DataSourceValueGroup<DataSourceValue>)(parameters[2] as IParameter<IDataSourceValueGroup>).Value;

            List<DataSourceValueGroup<DataSourceValue>> dataGroups = new List<DataSourceValueGroup<DataSourceValue>>
            {
                firstData,
                secondData
            };

            TimeSliceScanner scanner = new(dataGroups, tolerance / SI.Milli);
            IEnumerable<DataSourceValue> combinedValues = new List<DataSourceValue>();
            while (!scanner.DataReadComplete)
            {
                IEnumerable<DataSourceValue> datapointGroups = scanner.ReadNextTimeSlice();
                int numberDatapoints = datapointGroups.Count();
                if (datapointGroups.Count() != dataGroups.Count())
                {
                    continue;
                }

                double totalValue = 0;
                double totalTime = 0;
                foreach (DataSourceValue datapoint in datapointGroups)
                {
                    totalValue += datapoint.Value;
                    totalTime += datapoint.Time;
                }

                combinedValues = combinedValues.Append(new DataSourceValue
                {
                    Value = totalValue,
                    Time = totalTime / numberDatapoints,
                    Target = datapointGroups.First().Target
                });
            }

            firstData.Target = $"{firstData.Target}+{secondData.Target}";
            firstData.Source = combinedValues;

            return firstData;
        }

        /// <summary>
        /// Computes based on type PhasorValue
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
        {
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SliceAdd"/> class.
        /// </summary>
        public SliceAdd() { }
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
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataValue =>
                new DataSourceValue
                {
                    Value = Math.Abs(dataValue.Value),
                    Time = dataValue.Time,
                    Target = dataValue.Target
                });

            dataSourceValues.Target = $"Abs({dataSourceValues.Target})";
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
            return null;
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
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Round"/> class.
        /// </summary>
        public Round() { }
    }
}
