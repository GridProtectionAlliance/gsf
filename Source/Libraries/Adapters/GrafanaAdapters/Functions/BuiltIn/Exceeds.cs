using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using GSF;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent The points at which the input series exceds the given threshold.
/// The <c>threhsold</c> parameter value is a floating-point numbers that represent the threshold to be exceedd.
/// Second parameter optional, is a boolean flag that determines if the Duration of exceeding is returned as value
/// </summary>
/// <remarks>
/// Signature: <c>Exceeds(threshold, [includeDuration = false], expression)</c> -<br/>
/// Returns: Series of values.<br/>
/// Example: <c>Exceeds(60.05, true, FILTER ActiveMeasurements WHERE SignalType LIKE '%FREQ')</c><br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Exceeds<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Exceeds<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent The points at which the input series exceds the given threshold..";

    /// <inheritdoc />
    public override string[] Aliases => [];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "threshold",
            Default = 1.0D,
            Description = "A floating point value representing the threshold.",
            Required = true
        },
        new ParameterDefinition<bool>
        {
            Name = "includeDuration",
            Default = false,
            Description = "A boolean flag which determines if duration is included as value (in seconds).",
            Required = false
        }
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        double threshold = parameters.Value<double>(0);
        bool includeDuration = parameters.Value<bool>(1);
        Dictionary<string, MetadataMap> metadataMaps = parameters.MetadataMaps;

        T? start = null;

        await using IAsyncEnumerator<T> enumerator = GetDataSourceValues(parameters).GetAsyncEnumerator(cancellationToken);

        if (enumerator.Current.Value > threshold)
        {
            start = enumerator.Current;
            
        }

        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            if (start is null && enumerator.Current.Value > threshold)
            {
                start = enumerator.Current;
            }
            else if (start is not null && enumerator.Current.Value < threshold)
            {
                if (includeDuration)
                    yield return (T)start with { 
                        Value = (enumerator.Current.Time - ((T)start).Time) /GSF.Ticks.PerSecond
                    };
                else
                    yield return (T)start;
                start = null;
            }
            
        }

        if (includeDuration)
            yield return (T)start with
            {
                Value = (enumerator.Current.Time - ((T)start).Time) / GSF.Ticks.PerSecond
            };
        else
            yield return (T)start;
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