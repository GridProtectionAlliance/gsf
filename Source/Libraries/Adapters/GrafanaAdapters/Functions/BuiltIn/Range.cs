using System.Collections.Generic;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

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
public abstract class Range<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Range<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the range, i.e., <c>maximum - minimum</c>, of the values in the source series.";

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        T rangeMin = new() { Value = double.MaxValue };
        T rangeMax = new() { Value = double.MinValue };

        // Immediately enumerate values to find range
        foreach (T dataValue in GetDataSourceValues(parameters))
        {
            if (dataValue.Value <= rangeMin.Value)
                rangeMin = dataValue;

            if (dataValue.Value >= rangeMax.Value)
                rangeMax = dataValue;
        }

        // Return computed results
        if (rangeMin.Time > 0.0D && rangeMax.Time > 0.0D)
            yield return rangeMax with { Value = rangeMax.Value - rangeMin.Value };
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Range<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Range<PhasorValue>
    {
        // Operating on magnitude only
    }
}