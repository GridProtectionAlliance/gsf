using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.FunctionParsing;
using GSF.Units;

namespace GrafanaAdapters.Functions;

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
public abstract class WrapAngle<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "WrapAngle";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent an adjusted set of angles that are wrapped.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Wrap" };

    /// <inheritdoc />
    public override ParameterDefinitions Parameters => new List<IParameter>
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
    public class ComputeDataSourceValue : WrapAngle<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //AngleUnit angleUnit = (parameters[1] as IParameter<AngleUnit>).Value;

            //// Compute
            //IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataSourceValue =>
            //{
            //    DataSourceValue transformedValue = dataSourceValue;
            //    transformedValue.Value = Angle.ConvertFrom(dataSourceValue.Value, angleUnit).ToRange(-Math.PI, false).ConvertTo(angleUnit);

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
    public class ComputePhasorValue : WrapAngle<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //AngleUnit angleUnit = (parameters[1] as IParameter<AngleUnit>).Value;

            //// Compute
            //IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Select(phasorValue =>
            //{
            //    PhasorValue transformedValue = phasorValue;
            //    transformedValue.Angle = Angle.ConvertFrom(phasorValue.Angle, angleUnit).ToRange(-Math.PI, false).ConvertTo(angleUnit);

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