using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that is selected based on the first series provided used as a 0 based index.
/// </summary>
/// <remarks>
/// Signature: <c>Switch(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Switch(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Variants: SELECT<T><br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Switch<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Switch<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value selected based on the first series provided as a 0-based index.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Select" };

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Scalar;

    /// <inheritdoc />
    public class ComputeDataSourceValue : Switch<DataSourceValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<DataSourceValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            DataSourceValue lastValue = default;

            IAsyncEnumerable<double> trackedValues = GetDataSourceValues(parameters).Select(dataValue =>
            {
                lastValue = dataValue;
                return dataValue.Value;
            });

            // Immediately enumerate to compute values
            double index = await trackedValues.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            double selection = await trackedValues.Skip((int)Math.Floor(index)).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            // Return computed results
            if (lastValue.Time > 0.0D)
                yield return lastValue with { Value = selection };
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Switch<PhasorValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<PhasorValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            PhasorValue lastValue = default;


            IAsyncEnumerable<PhasorValue> trackedValues = GetDataSourceValues(parameters).Select(dataValue =>
            {
                lastValue = dataValue;
                return dataValue;
            });

            double index = (await trackedValues.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)).Magnitude;
            PhasorValue selection = await trackedValues.Skip((int)Math.Floor(index)).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            // Return computed results
            if (lastValue.Time > 0.0D)
            {
                yield return lastValue with
                {
                    Magnitude = selection.Magnitude,
                    Angle = selection.Angle
                };
            }
        }
    }
}