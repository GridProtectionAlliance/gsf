using System.Collections.Generic;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that is the minimum of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Minimum(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Minimum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Minimum, Min<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Minimum<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Minimum<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that is the minimum of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Min" };

    /// <inheritdoc />
    public override bool ResultIsSetTargetSeries => true;

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        T minValue = new() { Value = double.MaxValue };

        // Immediately enumerate values to find minimum
        foreach (T dataValue in GetDataSourceValues(parameters))
        {
            if (dataValue.Value <= minValue.Value)
                minValue = dataValue;
        }

        // Return computed results
        if (minValue.Time > 0.0D)
            yield return minValue;
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Minimum<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Minimum<PhasorValue>
    {
        // Operating on magnitude only
    }
}