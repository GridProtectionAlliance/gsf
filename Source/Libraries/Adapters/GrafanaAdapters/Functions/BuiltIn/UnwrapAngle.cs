using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.Units;

namespace GrafanaAdapters.Functions.BuiltIn;

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
public abstract class UnwrapAngle<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "UnwrapAngle";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent an adjusted set of angles that are unwrapped.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Unwrap" };

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<AngleUnit>
        {
            Name = "units",
            Default = AngleUnit.Degrees,
            Description = "Specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds or AngularMil",
            Required = false,
        }
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : UnwrapAngle<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //AngleUnit angleUnit = (parameters[1] as IParameter<AngleUnit>).Value;

            //List<DataSourceValue> dataValues = dataSourceValues.Source.ToList();

            //// Compute
            //IEnumerable<Angle> convertedAngles = dataValues.Select(dataValue => Angle.ConvertFrom(dataValue.Value, angleUnit)).ToList();
            //List<Angle> unwrappedAngles = Angle.Unwrap(convertedAngles).ToList();

            //IEnumerable<DataSourceValue> transformedDataSourceValues = unwrappedAngles.Select((angle, index) =>
            //{
            //    DataSourceValue transformedValue = dataValues[index];
            //    transformedValue.Value = angle.ConvertTo(angleUnit);

            //    return transformedValue;
            //});

            //// Set Values
            //DataSourceValueGroup<DataSourceValue> result = dataSourceValues.Clone();
            //result.Target = $"{Name}({dataSourceValues.Target},{angleUnit})";
            //result.Source = transformedDataSourceValues;

            //return result;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : UnwrapAngle<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //AngleUnit angleUnit = (parameters[1] as IParameter<AngleUnit>).Value;

            //List<PhasorValue> phasorData = phasorValues.Source.ToList();

            //// Compute
            //IEnumerable<Angle> convertedAngles = phasorData.Select(phasorValue => Angle.ConvertFrom(phasorValue.Angle, angleUnit)).ToList();
            //List<Angle> unwrappedAngles = Angle.Unwrap(convertedAngles).ToList();

            //IEnumerable<PhasorValue> transformedPhasorValues = unwrappedAngles.Select((angle, index) =>
            //{
            //    PhasorValue transformedValue = phasorData[index];
            //    transformedValue.Angle = angle.ConvertTo(angleUnit);

            //    return transformedValue;
            //});

            //// Set Values
            //DataSourceValueGroup<PhasorValue> result = phasorValues.Clone();
            //result.Target = $"{Name}({phasorValues.Target},{angleUnit})";
            //result.Source = transformedPhasorValues;

            //return result;
            return null;
        }
    }
}