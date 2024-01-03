using System.Collections.Generic;
using System.Threading;
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
    public override string Name => "Range";

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the range, i.e., <c>maximum - minimum</c>, of the values in the source series.";

    /// <inheritdoc />
    public class ComputeDataSourceValue : Range<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            DataSourceValue rangeMin = new() { Value = double.MaxValue };
            DataSourceValue rangeMax = new() { Value = double.MinValue };

            // Immediately enumerate values to find range
            foreach (DataSourceValue dataValue in GetDataSourceValues(parameters))
            {
                if (dataValue.Value <= rangeMin.Value)
                    rangeMin = dataValue;

                if (dataValue.Value >= rangeMax.Value)
                    rangeMax = dataValue;
            }

            // Do not return local default value
            if (rangeMin.Time > 0.0D && rangeMax.Time > 0.0D)
            {
                DataSourceValue result = rangeMax;
                result.Value = rangeMax.Value - rangeMin.Value;
                yield return result;
            }
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Range<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            PhasorValue rangeMin = new() { Magnitude = double.MaxValue };
            PhasorValue rangeMax = new() { Magnitude = double.MinValue };

            // Immediately enumerate values to find range - magnitude only
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters))
            {
                if (dataValue.Magnitude <= rangeMin.Magnitude)
                    rangeMin = dataValue;

                if (dataValue.Magnitude >= rangeMax.Magnitude)
                    rangeMax = dataValue;
            }

            // Do not return local default value
            if (rangeMin.Time > 0.0D && rangeMax.Time > 0.0D)
            {
                PhasorValue result = rangeMax;
                result.Magnitude = rangeMax.Magnitude - rangeMin.Magnitude;
                yield return result;
            }
        }
    }
}