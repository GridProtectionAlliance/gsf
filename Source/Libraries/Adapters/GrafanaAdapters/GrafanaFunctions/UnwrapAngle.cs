using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GrafanaAdapters.GrafanaFunctionBase;
using GSF.Units;
using Newtonsoft.Json.Linq;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a series of values that represent an adjusted set of angles that are unwrapped, per specified angle units, so that a comparable mathematical
    /// operation can be executed. For example, for angles that wrap between -180 and +180 degrees, this algorithm unwraps the values to make the values
    /// mathematically comparable. The units parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians, Grads,
    /// ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees.
    /// </summary>
    /// <remarks>
    /// Signature: <c>UnwrapAngle([units = Degrees], expression)</c><br/>
    /// Returns: Series of values.<br/>
    /// Example: <c>UnwrapAngle(FSX_PMU2-PA1:VH; REA_PMU3-PA2:VH)</c><br/>
    /// Variants: UnwrapAngle, Unwrap<br/>
    /// Execution: Immediate in-memory array load.
    /// </remarks>
    public class UnwrapAngle : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "UnwrapAngle";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent an adjusted set of angles that are unwrapped.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(UnwrapAngle);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(UnwrapAngle|Unwrap)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true,
                },
                new Parameter<AngleUnit>
                {
                    Default = AngleUnit.Degrees,
                    Description = "Specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds or AngularMil",
                    Required = false,
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
            AngleUnit angleUnit = (parameters[1] as IParameter<AngleUnit>).Value;

            List<DataSourceValue> dataValues = dataSourceValues.Source.ToList();

            // Compute
            IEnumerable<Angle> convertedAngles = dataValues.Select(dataValue => Angle.ConvertFrom(dataValue.Value, angleUnit)).ToList();
            List<Angle> unwrappedAngles = Angle.Unwrap(convertedAngles).ToList();

            IEnumerable<DataSourceValue> transformedDataSourceValues = unwrappedAngles.Select((angle, index) => 
            {
                DataSourceValue transformedValue = dataValues[index];
                transformedValue.Value = angle.ConvertTo(angleUnit);

                return transformedValue;
            });

            // Set Values
            dataSourceValues.Target = $"{Name}({dataSourceValues.Target},{angleUnit})";
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
            AngleUnit angleUnit = (parameters[1] as IParameter<AngleUnit>).Value;

            List<PhasorValue> phasorData = phasorValues.Source.ToList();

            // Compute
            IEnumerable<Angle> convertedAngles = phasorData.Select(phasorValue => Angle.ConvertFrom(phasorValue.Angle, angleUnit)).ToList();
            List<Angle> unwrappedAngles = Angle.Unwrap(convertedAngles).ToList();

            IEnumerable<PhasorValue> transformedPhasorValues = unwrappedAngles.Select((angle, index) =>
            {
                PhasorValue transformedValue = phasorData[index];
                transformedValue.Angle = angle.ConvertTo(angleUnit);

                return transformedValue;
            });

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]},{angleUnit});{Name}({labels[1]},{angleUnit})";
            phasorValues.Source = transformedPhasorValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnwrapAngle"/> class.
        /// </summary>
        public UnwrapAngle() { }
    }
}
