using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using GSF;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            Name = "returnDurations",
            Default = false,
            Description = "A boolean flag that indicates that the duration (in seconds) a value exceeded threshold be returned instead of the original value.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        double threshold = parameters.Value<double>(0);
        bool includeDuration = parameters.Value<bool>(1);
        Dictionary<string, MetadataMap> metadataMaps = parameters.MetadataMaps;

        T? start = null;

        await using IAsyncEnumerator<T> enumerator = GetDataSourceValues(parameters).GetAsyncEnumerator(cancellationToken);

        if (!double.IsNaN(enumerator.Current.Value) && enumerator.Current.Value > threshold)
        {
            start = enumerator.Current;
            
        }

        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            if (double.IsNaN(enumerator.Current.Value))
                continue;
                
            if (start is null && enumerator.Current.Value > threshold)
            {
                start = enumerator.Current;
            }
            else if (start is not null && enumerator.Current.Value <= threshold)
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

        if (start is not null)
        {
            if (includeDuration)
                yield return (T)start with
                {
                    Value = (((T)start).Time - enumerator.Current.Time) / GSF.Ticks.PerSecond
                };
            else
                yield return (T)start;
        }
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Exceeds<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Exceeds<PhasorValue>
    {
        // Operating on magnitude only
    }
}