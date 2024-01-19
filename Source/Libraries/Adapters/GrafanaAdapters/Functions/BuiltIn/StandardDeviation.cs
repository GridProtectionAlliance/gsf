using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using GSF.NumericalAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the standard deviation of the values in the source series. Parameter <c>useSampleCalc</c>,
/// optional, is a boolean flag representing if the sample based calculation should be used - defaults to false, which means the
/// population based calculation should be used.
/// </summary>
/// <remarks>
/// Signature: <c>StandardDeviation([useSampleCalc = false], expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>StandardDeviation(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
/// Variants: StandardDeviation, StdDev<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class StandardDeviation<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(StandardDeviation<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the standard deviation of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "StdDev" };

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<bool>
        {
            Name = "useSampleCalc",
            Default = false,
            Description = "A boolean flag representing if the sample based calculation should be used.",
            Required = false
        }
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : StandardDeviation<DataSourceValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<DataSourceValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            bool useSampleCalc = parameters.Value<bool>(0);
            DataSourceValue lastValue = default;

            IAsyncEnumerable<double> trackedValues = GetDataSourceValues(parameters).Select(dataValue =>
            {
                lastValue = dataValue;
                return dataValue.Value;
            });

            // Immediately enumerate to array to compute values
            double stdDev = (await trackedValues.ToArrayAsync(cancellationToken).ConfigureAwait(false)).StandardDeviation(useSampleCalc);

            // Return computed results
            if (lastValue.Time > 0.0D)
                yield return lastValue with { Value = stdDev };
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : StandardDeviation<PhasorValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<PhasorValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            bool useSampleCalc = parameters.Value<bool>(0);

            List<double> magnitudes = new();
            List<double> angles = new();
            PhasorValue lastValue = default;

            // Immediately load values in-memory only enumerating data source once
            await foreach (PhasorValue dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                lastValue = dataValue;
                magnitudes.Add(dataValue.Magnitude);
                angles.Add(dataValue.Angle);
            }

            // Return computed results
            if (lastValue.Time > 0.0D)
            {
                yield return lastValue with
                {
                    Magnitude = magnitudes.StandardDeviation(useSampleCalc),
                    Angle = angles.StandardDeviation(useSampleCalc)
                };
            }
        }
    }
}