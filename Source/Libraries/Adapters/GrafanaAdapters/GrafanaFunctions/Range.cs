using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters.GrafanaFunctions;

/// <summary>
/// Returns a single value that represents the range, i.e., <c>maximum - minimum</c>, of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Range(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Range(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Range<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public class Range: GrafanaFunctionBase
{
    /// <inheritdoc />
    public override string Name => nameof(Range);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the range, i.e., <c>maximum - minimum</c>, of the values in the source series.";

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
        IEnumerable<DataSourceValue> orderedValues = dataSourceValues.Source.OrderBy(dataValue => dataValue.Value);
        DataSourceValue lastElement = dataSourceValues.Source.Last();
        lastElement.Value = orderedValues.Last().Value - orderedValues.First().Value;

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
        IEnumerable<PhasorValue> orderedMag = phasorValues.Source.OrderBy(dataValue => dataValue.Magnitude);
        IEnumerable<PhasorValue> orderedAng = phasorValues.Source.OrderBy(dataValue => dataValue.Angle);

        PhasorValue lastElement = phasorValues.Source.Last();
        lastElement.Magnitude = orderedMag.Last().Magnitude - orderedMag.First().Magnitude;
        lastElement.Angle = orderedAng.Last().Angle - orderedAng.First().Angle;

        // Set Values
        string[] labels = phasorValues.Target.Split(';');
        phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
        phasorValues.Source = Enumerable.Repeat(lastElement, 1);

        return phasorValues;
    }
}