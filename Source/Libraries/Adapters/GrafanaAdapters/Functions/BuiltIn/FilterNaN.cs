using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN.
/// First parameter, optional, is a boolean flag that determines if infinite values should also be excluded - defaults to true.
/// </summary>
/// <remarks>
/// Signature: <c>FilterNaN([alsoFilterInfinity = true], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>FilterNaN(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
/// Variants: FilterNaN<br/>
/// Execution: Deferred enumeration.
/// </remarks> 
public abstract class FilterNaN<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(FilterNaN<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN.";

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<bool>
        {
            Name = "alsoFilterInfinity",
            Default = true,
            Description = "A boolean flag that determines if infinite values should also be excluded - defaults to true",
            Required = false
        }
    };

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        bool alsoFilterInfinity = parameters.Value<bool>(0);

        bool filterNaN(T dataValue) => 
            !(double.IsNaN(dataValue.Value) || alsoFilterInfinity && double.IsInfinity(dataValue.Value));

        return GetDataSourceValues(parameters).Where(filterNaN);
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : FilterNaN<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : FilterNaN<PhasorValue>
    {
        // Operating on magnitude only
    }
}