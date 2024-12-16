using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using static GrafanaAdapters.Functions.Common;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of <c>N</c>, or <c>N%</c> of total, values from the end of the source series.
/// <c>N</c>, optional, is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
/// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.
/// <c>N</c> can either be constant value or a named target available from the expression. Any target values that fall between 0
/// and 1 will be treated as a percentage.
/// </summary>
/// <remarks>
/// Signature: <c>Last([N|N% = 1], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Last(150, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Last<br/>
/// Execution: Immediate in-memory array load.<br/>
/// Group Operations: Slice, Set
/// </remarks>
public abstract class Last<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Last<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of N, or N% of total, values from the end of the source series.";

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
        // Immediately load values in-memory only enumerating data source once
        T[] values = await GetDataSourceValues(parameters).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        int length = values.Length;

        if (length == 0)
            yield break;

        int valueN = ParseTotal("N", parameters.Value<string>(0), length);

        if (valueN > length)
            valueN = length;

        for (int i = 0; i < valueN; i++)
            yield return values[length - 1 - i];
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Last<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Last<PhasorValue>
    {
    }
}