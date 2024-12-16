using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN.
/// Parameter <c>alsoFilterInfinity</c>, optional, is a boolean flag that determines if infinite values should also be excluded - defaults to true.
/// </summary>
/// <remarks>
/// Signature: <c>FilterNaN([alsoFilterInfinity = true], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>FilterNaN(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
/// Variants: FilterNaN<br/>
/// Execution: Deferred enumeration.<br/>
/// Group Operations: Slice, Set
/// </remarks> 
public abstract class FilterNaN<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(FilterNaN<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN.";

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<bool>
        {
            Name = "alsoFilterInfinity",
            Default = true,
            Description = "A boolean flag that determines if infinite values should also be excluded.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        bool alsoFilterInfinity = parameters.Value<bool>(0);

        bool filterNaN(T dataValue) =>
            !(double.IsNaN(dataValue.Value) || alsoFilterInfinity && double.IsInfinity(dataValue.Value));

        return GetDataSourceValues(parameters).Where(filterNaN);
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : FilterNaN<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : FilterNaN<PhasorValue>
    {
        // Operating on magnitude only
    }
}