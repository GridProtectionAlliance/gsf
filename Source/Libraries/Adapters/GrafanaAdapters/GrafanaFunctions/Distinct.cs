using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters.GrafanaFunctions;

/// <summary>
/// Returns a series of values that represent the unique set of values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Distinct(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Distinct(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Distinct, Unique<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public class Distinct: GrafanaFunctionBase
{
    /// <inheritdoc />
    public override string Name => nameof(Distinct);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the unique set of values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Unique" };

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
        IEnumerable<DataSourceValue> distinctValues = dataSourceValues.Source.GroupBy(dataValue => dataValue.Value).Select(group => group.First());

        // Set Values
        dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
        dataSourceValues.Source = distinctValues;

        return dataSourceValues;
    }

    /// <inheritdoc />
    public override DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
    {
        // Get Values
        DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

        // Compute
        IEnumerable<PhasorValue> distinctValues = phasorValues.Source.GroupBy(dataValue => dataValue.Magnitude).Select(group => group.First());

        // Set Values
        string[] labels = phasorValues.Target.Split(';');
        phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
        phasorValues.Source = distinctValues;

        return phasorValues;
    }
}