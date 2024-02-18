using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using static GrafanaAdapters.Functions.Common;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of <c>N</c>, or <c>N%</c> of total, values from the start of the source series.
/// <c>N</c> is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
/// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.
/// <c>N</c> can either be constant value or a named target available from the expression. Any target values that fall between 0
/// and 1 will be treated as a percentage.
/// </summary>
/// <remarks>
/// Signature: <c>First([N|N% = 1], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>First(5%, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: First<br/>
/// Execution: Immediate in-memory array load, when <c>N</c> is defined; otherwise, immediate enumeration of one, i.e., first value.
/// </remarks>
public abstract class First<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(First<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of N, or N% of total, values from the start of the source series.";

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override bool IsSliceSeriesEquivalent => false;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<string>
        {
            Name = "N",
            Default = "1",
            Description = "A integer value or percent representing number or % of elements to take.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IAsyncEnumerable<T> source = GetDataSourceValues(parameters);

        if (parameters.ParsedCount == 0)
        {
            // Short cut for only getting first value
            await using IAsyncEnumerator<T> enumerator = source.GetAsyncEnumerator(cancellationToken);

            if (await enumerator.MoveNextAsync().ConfigureAwait(false))
                yield return enumerator.Current;
        }
        else
        {
            // Immediately load values in-memory only enumerating data source once
            T[] values = await source.ToArrayAsync(cancellationToken).ConfigureAwait(false);
            int length = values.Length;

            if (length == 0)
                yield break;

            int valueN = ParseTotal(parameters.Value<string>(0), length);

            if (valueN > length)
                valueN = length;

            for (int i = 0; i < valueN; i++)
                yield return values[i];
        }
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : First<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : First<PhasorValue>
    {
    }
}