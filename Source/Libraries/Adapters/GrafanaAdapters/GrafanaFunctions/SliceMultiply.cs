using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;
using GSF.Units;

namespace GrafanaAdapters.GrafanaFunctions;

    /// <summary>
    /// Returns a series of values that represent each of the values in the source series multiplied with another series of values.
    /// </summary>
    /// <remarks>
    /// Signature: <c>SliceMultiply(N, expression)</c><br/>
    /// Returns: Series of values.<br/>
    /// Example: <c>SliceMultiply(0.033, FILTER ActiveMeasurements WHERE SignalType='ABC', FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
    /// Variants: SliceMultiply<br/>
    /// Execution: Deferred enumeration.
    /// </remarks>
    public abstract class SliceMultiply<T> : GrafanaFunctionBase<T> where T : IDataSourceValue
    {
        /// <inheritdoc />
        public override string Name => "SliceMultiply";

        /// <inheritdoc />
        public override string Description =>
            "Returns a series of values that represent each of the values in the source series multiplied with another series of values.";

        /// <inheritdoc />
        public override List<IParameter> Parameters =>
            new()
            {
                new Parameter<double>
                {
                    Default = 0.033,
                    Description = "The level of tolerance.",
                    Required = true
                },
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "First datapoint",
                    Required = true
                },
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "Second datapoint",
                    Required = true
                }
            };

        /// <inheritdoc />
        public override DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            double tolerance = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<DataSourceValue> firstData =
                (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            DataSourceValueGroup<DataSourceValue> secondData =
                (DataSourceValueGroup<DataSourceValue>)(parameters[2] as IParameter<IDataSourceValueGroup>).Value;

            List<DataSourceValueGroup<DataSourceValue>> dataGroups = new()
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
                if (datapointGroups.Count() != dataGroups.Count)
                    continue;

                //Compute & Set Values
                DataSourceValue transformedValue = datapointGroups.Last();

                List<double> values = datapointGroups.Select(dataValue => { return dataValue.Value; }).ToList();
                transformedValue.Value = values[0] * values[1];
                transformedValue.Time = datapointGroups.Select(dataValue => { return dataValue.Time; }).Average();
                combinedValuesList.Add(transformedValue);
            }

            IEnumerable<DataSourceValue> combinedValuesEnumerable = combinedValuesList;

            firstData.Target = $"{firstData.Target}+{secondData.Target}";
            firstData.Source = combinedValuesEnumerable;

            return firstData;
        }

        /// <inheritdoc />
        public override DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
        {
            // Get Values
            double tolerance = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<PhasorValue> firstData =
                (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            DataSourceValueGroup<PhasorValue> secondData =
                (DataSourceValueGroup<PhasorValue>)(parameters[2] as IParameter<IDataSourceValueGroup>).Value;

            List<DataSourceValueGroup<PhasorValue>> dataGroups = new()
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
                if (datapointGroups.Count() != dataGroups.Count)
                    continue;

                PhasorValue transformedValue = datapointGroups.Last();
                List<double> magnitudes = datapointGroups.Select(dataValue => { return dataValue.Magnitude; }).ToList();
                List<double> angles = datapointGroups.Select(dataValue => { return dataValue.Angle; }).ToList();

                transformedValue.Magnitude = magnitudes[0] * magnitudes[1];
                transformedValue.Angle = angles[0] * angles[1];
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
    }
}