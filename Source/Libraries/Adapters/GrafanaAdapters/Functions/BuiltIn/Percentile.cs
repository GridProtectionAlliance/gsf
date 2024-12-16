using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using static GrafanaAdapters.Functions.Common;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the <c>N</c>th order percentile for the sorted values in the source series.
/// <c>N</c> is a floating point value, representing a percentage, that must range from 0 to 100.
/// </summary>
/// <remarks>
/// Signature: <c>Percentile(N[%], expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Percentile(10%, FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
/// Variants: Percentile, Pctl<br/>
/// Execution: Immediate in-memory array load.<br/>
/// Group Operations: Slice, Set
/// </remarks>
public abstract class Percentile<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Percentile<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the Nth order percentile for the sorted values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Pctl"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Scalar;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<string>
        {
            Name = "N",
            Default = "100",
            Description = "A floating point value, representing a percentage, that must range from 0 to 100.",
            Required = true
        }
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Immediately load values in-memory only enumerating data source once
        T[] values = await GetDataSourceValues(parameters).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        int length = values.Length;

        if (length == 0)
            yield break;

        Array.Sort(values, (a, b) => a.Value < b.Value ? -1 : a.Value > b.Value ? 1 : 0);

        double valueN = ParsePercentage("N", parameters.Value<string>(0));

        switch (valueN)
        {
            case 0.0D:
                yield return values.First();
                break;
            case 100.0D:
                yield return values.Last();
                break;
            default:
                double n = (length - 1) * (valueN / 100.0D) + 1.0D;
                int k = (int)n;
                T kData = values[k];
                double d = n - k;
                double k0 = values[k - 1].Value;
                double k1 = kData.Value;
                yield return kData with { Value = k0 + d * (k1 - k0) };
                break;
        }
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Percentile<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Percentile<PhasorValue>
    {
        // Operating on magnitude only
    }
}