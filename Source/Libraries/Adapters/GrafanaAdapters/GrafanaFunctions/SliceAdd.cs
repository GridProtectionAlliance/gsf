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
}
