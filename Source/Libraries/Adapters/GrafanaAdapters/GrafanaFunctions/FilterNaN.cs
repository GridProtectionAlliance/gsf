using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters.GrafanaFunctions;

/// <summary>
/// Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN.
/// Second parameter, optional, is a boolean flag that determines if infinite values should also be excluded - defaults to true.
/// </summary>
/// <remarks>
/// Signature: <c>FilterNaN(expression, [alsoFilterInfinity = true])</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>FilterNaN(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
/// Variants: FilterNaN<br/>
/// Execution: Deferred enumeration.
/// </remarks> 
public class FilterNaN: GrafanaFunctionBase
{
    /// <inheritdoc />
    public override string Name => nameof(FilterNaN);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN.";

    /// <inheritdoc />
    public override List<IParameter> Parameters => new()
    {
        InputDataPointValues,

        new Parameter<bool>
        {
            Default = true,
            Description = "A boolean flag that determines if infinite values should also be excluded - defaults to true",
            Required = false
        },
    };

    /// <inheritdoc />
    public override DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
    {
        // Get Values
        DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
        bool filterInfinity = (parameters[1] as IParameter<bool>).Value;

        // Compute
        IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Where(dataValue =>
            !(
                double.IsNaN(dataValue.Value) ||
                (filterInfinity && double.IsInfinity(dataValue.Value)
                )));

        // Set Values
        dataSourceValues.Target = $"{Name}({dataSourceValues.Target},{filterInfinity})";
        dataSourceValues.Source = transformedDataSourceValues;

        return dataSourceValues;
    }

    /// <inheritdoc />
    public override DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
    {
        // Get Values
        DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
        bool filterInfinity = (parameters[1] as IParameter<bool>).Value;

        // Compute
        IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Where(phasorValue =>
            !(
                double.IsNaN(phasorValue.Magnitude) ||
                double.IsNaN(phasorValue.Angle) ||
                (filterInfinity && (double.IsInfinity(phasorValue.Magnitude) || double.IsInfinity(phasorValue.Angle)))
            ));

        // Set Values
        string[] labels = phasorValues.Target.Split(';');
        phasorValues.Target = $"{Name}({labels[0]},{filterInfinity});{Name}({labels[1]},{filterInfinity})";
        phasorValues.Source = transformedPhasorValues;

        return phasorValues;
    }
}