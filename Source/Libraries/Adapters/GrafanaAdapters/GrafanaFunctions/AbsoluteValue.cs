using System;
using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters.GrafanaFunctions;

/// <summary>
/// Returns a series of values that represent the absolute value each of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>AbsoluteValue(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>AbsoluteValue(FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
/// Variants: AbsoluteValue, Abs<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public class AbsoluteValue : GrafanaFunctionBase
{
    /// <inheritdoc />
    public override string Name => nameof(AbsoluteValue);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the absolute value each of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Abs" };

    /// <inheritdoc />
    public override List<IParameter> Parameters => new()
    {
        InputDataPointValues
    };

    /// <inheritdoc />
    public override DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
    {
        // Get Values
        DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>)!.Value;

        // Compute
        IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataSourceValue =>
        {
            DataSourceValue transformedValue = dataSourceValue;
            transformedValue.Value = Math.Abs(transformedValue.Value);

            return transformedValue;
        });

        // Set Values
        dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
        dataSourceValues.Source = transformedDataSourceValues;

        return dataSourceValues;
    }

    /// <inheritdoc />
    public override DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
    {
        // Get Values
        DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>)!.Value;

        // Compute
        IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Select(phasorValue =>
        {
            PhasorValue transformedValue = phasorValue;
            transformedValue.Magnitude = Math.Abs(transformedValue.Magnitude);
            transformedValue.Angle = Math.Abs(transformedValue.Angle);

            return transformedValue;
        });

        // Set Values
        string[] labels = phasorValues.Target.Split(';');
        phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
        phasorValues.Source = transformedPhasorValues;

        return phasorValues;
    }
}