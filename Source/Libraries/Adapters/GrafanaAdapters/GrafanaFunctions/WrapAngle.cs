using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GrafanaAdapters.GrafanaFunctionBase;
using GSF.Units;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a series of values that represent an adjusted set of angles that are wrapped, per specified angle units, so that angle values are consistently
    /// between -180 and +180 degrees. The units parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians,
    /// Grads, ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees.
    /// </summary>
    /// <remarks>
    /// Signature: <c>WrapAngle(expression, [units = Degrees])</c><br/>
    /// Returns: Series of values.<br/>
    /// Example: <c>WrapAngle(Radians, FILTER TOP 5 ActiveMeasurements WHERE SignalType LIKE '%PHA')</c><br/>
    /// Variants: WrapAngle, Wrap<br/>
    /// Execution: Deferred enumeration.
    /// </remarks>
    public class WrapAngle : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "WrapAngle";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent an adjusted set of angles that are wrapped.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(WrapAngle);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(WrapAngle|Wrap)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                new Parameter<AngleUnit>
                {
                    Default = AngleUnit.Degrees,
                    Description = "Specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds or AngularMil",
                    Required = false,
                    ParameterTypeName = "angle"
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

            // Compute
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataSourceValue =>
            {
                DataSourceValue transformedValue = dataSourceValue;
                transformedValue.Value = Angle.ConvertFrom(dataSourceValue.Value, angleUnit).ToRange(-Math.PI, false).ConvertTo(angleUnit);

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

            // Compute
            IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Select(phasorValue =>
            {
                PhasorValue transformedValue = phasorValue;
                transformedValue.Angle = Angle.ConvertFrom(phasorValue.Angle, angleUnit).ToRange(-Math.PI, false).ConvertTo(angleUnit);

                return transformedValue;
            });

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]},{angleUnit});{Name}({labels[1]},{angleUnit})";
            phasorValues.Source = transformedPhasorValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WrapAngle"/> class.
        /// </summary>
        public WrapAngle() { }
    }
}
