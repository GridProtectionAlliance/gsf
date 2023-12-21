using System;
using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters.GrafanaFunctions;

/// <summary>
/// Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Ceiling(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Ceiling(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Variants: Ceiling, Ceil<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public class Ceiling: GrafanaFunctionBase
{
    /// <inheritdoc />
    public override string Name => nameof(Ceiling);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Ceil" };

    /// <inheritdoc />
    public override List<IParameter> Parameters => new()
    {
        new Parameter<IDataSourceValueGroup>
        {
            Default = new DataSourceValueGroup<DataSourceValue>(),
            Description = "Data Points",
            Required = true
        }
    };

    /// <inheritdoc />
    public override DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
    {
        // Get Values
        DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

        // Compute
        IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataSourceValue =>
        {
            DataSourceValue transformedValue = dataSourceValue;
            transformedValue.Value = Math.Ceiling(transformedValue.Value);

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
        DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

        // Compute
        IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Select(phasorValue =>
        {
            PhasorValue transformedValue = phasorValue;
            transformedValue.Magnitude = Math.Ceiling(transformedValue.Magnitude);
            transformedValue.Angle = Math.Ceiling(transformedValue.Angle);

            return transformedValue;
        });

        // Set Values
        string[] labels = phasorValues.Target.Split(';');
        phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
        phasorValues.Source = transformedPhasorValues;

        return phasorValues;
    }
}