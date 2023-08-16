using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GSF.Units;

namespace GrafanaAdapters.GrafanaFunctions
{

    /// <summary>
    /// Returns a series of values that represent each of the values in the source series added with another series of values.
    /// </summary>
    /// <remarks>
    /// Signature: <c>SliceAdd(N, expression)</c><br/>
    /// Returns: Series of values.<br/>
    /// Example: <c>SliceAdd(FILTER ActiveMeasurements WHERE SignalType='ABC', FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
    /// Variants: SliceAdd<br/>
    /// Execution: Deferred enumeration.
    /// </remarks>
    public class SliceAdd : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "SliceAdd";

        /// <inheritdoc />
        public string Description { get; } = "Returns one series by combining two other series within an offset N.";

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
            List<DataSourceValue> combinedValuesList = new();
            while (!scanner.DataReadComplete)
            {
                IEnumerable<DataSourceValue> datapointGroups = scanner.ReadNextTimeSlice();

                //Error check
                if (datapointGroups.Count() != dataGroups.Count())
                    continue;

                //Compute & Set Values
                DataSourceValue transformedValue = datapointGroups.Last();
                transformedValue.Value = datapointGroups.Select(dataValue => { return dataValue.Value; }).Sum();
                transformedValue.Time = datapointGroups.Select(dataValue => { return dataValue.Time; }).Average();
                combinedValuesList.Add(transformedValue);
            }

            IEnumerable<DataSourceValue> combinedValuesEnumerable = combinedValuesList;

            firstData.Target = $"{firstData.Target}+{secondData.Target}";
            firstData.Source = combinedValuesEnumerable;

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
            List<PhasorValue> combinedValuesList = new();
            while (!scanner.DataReadComplete)
            {
                IEnumerable<PhasorValue> datapointGroups = scanner.ReadNextTimeSlice();

                //Error check
                if (datapointGroups.Count() != dataGroups.Count())
                    continue;

                PhasorValue transformedValue = datapointGroups.Last();
                transformedValue.Magnitude = datapointGroups.Select(dataValue => { return dataValue.Magnitude; }).Sum();
                transformedValue.Angle = datapointGroups.Select(dataValue => { return dataValue.Angle; }).Average();
                transformedValue.Time = datapointGroups.Select(dataValue => { return dataValue.Time; }).Average();
                combinedValuesList.Add(transformedValue);
            }

            IEnumerable<PhasorValue> combinedValuesEnumerable = combinedValuesList;

            string[] firstNames = firstData.Target.Split(';');
            string[] secondNames = secondData.Target.Split(';');

            firstData.Target = $"{firstNames[0]}+{secondNames[0]};{firstNames[1]}+{secondNames[1]}";
            firstData.Source = combinedValuesEnumerable;

            return firstData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SliceAdd"/> class.
        /// </summary>
        public SliceAdd() { }
    }
}
