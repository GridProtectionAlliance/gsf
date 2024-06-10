using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent a filtered set of the values in the source series where each value falls outside the specified low and high.
/// The <c>low</c> and <c>high</c> parameter values are floating-point numbers that represent the range of values excluded in the return series. Third parameter,
/// optional, is a boolean flag that determines if range values are inclusive, i.e., excluded values are &lt;= low or &gt;= high - defaults to false, which means
/// values are exclusive, i.e., excluded values are &lt; low or &gt; high. Function allows a fourth optional parameter that is a boolean flag - when four parameters
/// are provided, third parameter determines if low value is inclusive and forth parameter determines if high value is inclusive. The <c>low</c> and <c>high</c>
/// parameter values can either be constant values or named targets available from the expression.
/// </summary>
/// <remarks>
/// Signature: <c>ExcludeRange(low, high, [inclusive = false], expression)</c> -or- <c>ExcludeRange(low, high, [lowInclusive = false], [highInclusive = false], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>ExcludeRange(-180.0, 180.0, true, false, FILTER ActiveMeasurements WHERE SignalType LIKE '%PHA')</c><br/>
/// Variants: ExcludeRange, Exclude<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class ExcludeRange<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(ExcludeRange<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent a filtered set of the values in the source series where each value falls outside the specified low and high.";

    /// <inheritdoc />
    public override string[] Aliases => ["Exclude"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "low",
            Default = 0.0D,
            Description = "A floating point value representing the low end of the range allowed in the return series.",
            Required = true
        },
        new ParameterDefinition<double>
        {
            Name = "high",
            Default = 0.0D,
            Description = "A floating point value representing the high end of the range allowed in the return series.",
            Required = true
        },
        new ParameterDefinition<bool>
        {
            Name = "lowInclusive",
            Default = false,
            Description = "A boolean flag which determines if low value is inclusive.",
            Required = false
        },
        new ParameterDefinition<bool>
        {
            Name = "highInclusive",
            Default = false,
            Description = "A boolean flag which determines if high value is inclusive.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        double low = parameters.Value<double>(0);
        double high = parameters.Value<double>(1);
        bool lowInclusive = parameters.Value<bool>(2);
        bool highInclusive = parameters.ParsedCount > 3 ? parameters.Value<bool>(3) : lowInclusive;

        bool filterRange(T dataValue) =>
            (lowInclusive ? dataValue.Value <= low : dataValue.Value < low) ||
            (highInclusive ? dataValue.Value >= high : dataValue.Value > high);

        return GetDataSourceValues(parameters).Where(filterRange);
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : ExcludeRange<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : ExcludeRange<PhasorValue>
    {
        // Operating on magnitude only
    }
}