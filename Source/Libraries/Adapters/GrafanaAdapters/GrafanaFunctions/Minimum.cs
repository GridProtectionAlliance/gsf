using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters.GrafanaFunctions;

/// <summary>
/// Returns a single value that is the minimum of the values in the source series.
/// </summary>
public class Minimum: GrafanaFunctionBase
{
    /// <inheritdoc />
    public override string Name => nameof(Minimum);

    /// <inheritdoc />
    public override string Description => "Returns a single value that is the minimum of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Min" };

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
        DataSourceValue minimumDataSourceValue = dataSourceValues.Source.OrderBy(dataValue => dataValue.Value).First();

        // Set Values
        dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
        dataSourceValues.Source = Enumerable.Repeat(minimumDataSourceValue, 1);

        return dataSourceValues;
    }

    /// <inheritdoc />
    public override DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
    {
        // Get Values
        DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            
        // Compute
        PhasorValue minimumPhasorValue = phasorValues.Source.OrderBy(dataValue => dataValue.Magnitude).First();

        // Set Values
        string[] labels = phasorValues.Target.Split(';');
        phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
        phasorValues.Source = Enumerable.Repeat(minimumPhasorValue, 1);

        return phasorValues;
    }
}