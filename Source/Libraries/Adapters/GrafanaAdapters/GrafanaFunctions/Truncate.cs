using System;
using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters.GrafanaFunctions;

/// <summary>
/// Returns a series of values that represent the integral part of each of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Truncate(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Truncate(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Variants: Truncate, Trunc<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public class Truncate: GrafanaFunctionBase
{
    /// <inheritdoc />
    public override string Name => nameof(Truncate);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the integral part of each of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Trunc" };

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
            transformedValue.Value = Math.Truncate(transformedValue.Value);

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
            transformedValue.Magnitude = Math.Truncate(transformedValue.Magnitude);
            transformedValue.Angle = Math.Truncate(transformedValue.Angle);

            return transformedValue;
        });

        // Set Values
        string[] labels = phasorValues.Target.Split(';');
        phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
        phasorValues.Source = transformedPhasorValues;

        return phasorValues;
    }
}