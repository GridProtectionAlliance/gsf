using GSF.Collections;
using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters.GrafanaFunctions;

/// <summary>
/// Returns a single value that represents the median of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Median(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Median(FILTER ActiveMeasurements WHERE SignalType='ALOG')</c><br/>
/// Variants: Median, Med, Mid<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public class Median: GrafanaFunctionBase
{
    /// <inheritdoc />
    public override string Name => nameof(Median);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the median of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Med", "Mid" };

    /// <inheritdoc />
    public override List<IParameter> Parameters => new()
    {
        InputDataPointValues
    };

    /// <inheritdoc />
    public override DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
    {
        // Get Values
        DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

        // Compute
        DataSourceValue lastElement = dataSourceValues.Source.Last();
        lastElement.Value = dataSourceValues.Source
            .Select(dataValue => { return dataValue.Value; })
            .Median()
            .Average();

        // Set Values
        dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
        dataSourceValues.Source = Enumerable.Repeat(lastElement, 1);

        return dataSourceValues;
    }

    /// <inheritdoc />
    public override DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
    {
        // Get Values
        DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

        // Compute
        PhasorValue lastElement = phasorValues.Source.Last();
        lastElement.Magnitude = phasorValues.Source
            .Select(dataValue => { return dataValue.Magnitude; })
            .Median()
            .Average();
        lastElement.Angle = phasorValues.Source
            .Select(dataValue => { return dataValue.Angle; })
            .Median()
            .Average();

        // Set Values
        string[] labels = phasorValues.Target.Split(';');
        phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
        phasorValues.Source = Enumerable.Repeat(lastElement, 1);

        return phasorValues;
    }
}