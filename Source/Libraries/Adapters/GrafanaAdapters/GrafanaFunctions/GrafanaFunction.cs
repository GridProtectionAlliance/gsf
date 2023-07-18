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
                    Flags = dataValue.Flags,
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
            // Get Values
            double value = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

            // Compute
            IEnumerable<PhasorValue> transformedDataSourceValues = phasorValues.Source.Select(phasorValue =>
                new PhasorValue
                {
                    Flags = phasorValue.Flags,
                    Magnitude = value + phasorValue.Magnitude,
                    Angle = phasorValue.Angle,
                    Time = phasorValue.Time,
                    MagnitudeTarget = phasorValue.MagnitudeTarget,
                    AngleTarget = phasorValue.AngleTarget,
                });

            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{value}+{labels[0]};{value}+{labels[1]}";
            phasorValues.Source = transformedDataSourceValues;

            return phasorValues;
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

            TimeSliceScanner<DataSourceValue> scanner = new(dataGroups, tolerance / SI.Milli);
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
            // Get Values
            double tolerance = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<PhasorValue> firstData = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            DataSourceValueGroup<PhasorValue> secondData = (DataSourceValueGroup<PhasorValue>)(parameters[2] as IParameter<IDataSourceValueGroup>).Value;

            List<DataSourceValueGroup<PhasorValue>> dataGroups = new List<DataSourceValueGroup<PhasorValue>>
            {
                firstData,
                secondData
            };

            TimeSliceScanner<PhasorValue> scanner = new(dataGroups, tolerance / SI.Milli);
            IEnumerable<PhasorValue> combinedValues = new List<PhasorValue>();
            while (!scanner.DataReadComplete)
            {
                IEnumerable<PhasorValue> datapointGroups = scanner.ReadNextTimeSlice();
                int numberDatapoints = datapointGroups.Count();
                if (datapointGroups.Count() != dataGroups.Count())
                {
                    continue;
                }

                double totalMag = 0;
                double totalAng = 0;
                double totalTime = 0;
                foreach (PhasorValue datapoint in datapointGroups)
                {
                    totalMag += datapoint.Magnitude;
                    totalAng += datapoint.Angle;
                    totalTime += datapoint.Time;
                }

                combinedValues = combinedValues.Append(new PhasorValue
                {
                    Magnitude = totalMag,
                    Angle = totalAng,
                    Time = totalTime / numberDatapoints,
                    MagnitudeTarget = datapointGroups.First().MagnitudeTarget,
                    AngleTarget = datapointGroups.First().AngleTarget,
                });
            }

            string[] firstNames = firstData.Target.Split(';');
            string[] secondNames = secondData.Target.Split(';');

            firstData.Target = $"{firstNames[0]}+{secondNames[0]};{firstNames[1]}+{secondNames[1]}";
            firstData.Source = combinedValues;

            return firstData;
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
                    Flags = dataValue.Flags,
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
            // Get Values
            DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            // Compute
            IEnumerable<PhasorValue> transformedDataSourceValues = phasorValues.Source.Select(phasorValue =>
                new PhasorValue
                {
                    Flags = phasorValue.Flags,
                    Magnitude = Math.Abs(phasorValue.Magnitude),
                    Angle = Math.Abs(phasorValue.Angle),
                    Time = phasorValue.Time,
                    MagnitudeTarget = phasorValue.MagnitudeTarget,
                    AngleTarget = phasorValue.AngleTarget,
                });

            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"Abs({labels[0]});Abs({labels[1]})";
            phasorValues.Source = transformedDataSourceValues;

            return phasorValues;
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
            phasorValues.Target = $"Round({labels[0]},{numberDecimals});Round({labels[1]},{numberDecimals})";
            phasorValues.Source = transformedDataSourceValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Round"/> class.
        /// </summary>
        public Round() { }
    }
}
