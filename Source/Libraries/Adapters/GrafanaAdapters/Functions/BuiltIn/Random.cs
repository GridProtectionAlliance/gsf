using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using GSF;
using GSF.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using static GrafanaAdapters.Functions.Common;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of <c>N</c>, or <c>N%</c> of total, values that are a random sample of the values in the source series.
/// <c>N</c> is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
/// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100.
/// Third parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.
/// <c>N</c> can either be constant value or a named target available from the expression. Any target values that fall between 0
/// and 1 will be treated as a percentage.
/// </summary>
/// <remarks>
/// Signature: <c>Random(N|N%, [normalizeTime = true], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Random(25%, FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
/// Variants: Random, Rand, Sample<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class Random<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Random<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of N, or N% of total, values that are a random sample of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Rand", "Sample" };

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override bool IsSliceSeriesEquivalent => false;

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.None | GroupOperations.Set;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<string>
        {
            Name = "N",
            Default = "1",
            Description = "A integer value or percent representing number or % of elements to take.",
            Required = true
        },
        new ParameterDefinition<bool>
        {
            Name = "normalizeTime",
            Default = true,
            Description = "A boolean flag which representing if time in dataset should be normalized.",
            Required = false
        }
    };

    /// <summary>
    /// Transposes order of values in array.
    /// </summary>
    /// <param name="currentValue">Source value.</param>
    /// <param name="values">Array of values.</param>
    /// <param name="index">Index of current value.</param>
    /// <returns>The transposed value.</returns>
    protected abstract T TransposeCompute(T currentValue, T[] values, int index);

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Immediately load values in-memory only enumerating data source once
        T[] values = await GetDataSourceValues(parameters).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        int length = values.Length;

        if (length == 0)
            yield break;

        int valueN = ParseTotal(parameters.Value<string>(0), length);

        if (valueN > length)
            valueN = length;

        bool normalizeTime = parameters.Value<bool>(1);
        double baseTime = values[0].Time;
        double timeStep = (values[length - 1].Time - baseTime) / (valueN - 1).NotZero(1);
        List<int> indexes = new(Enumerable.Range(0, length));
        indexes.Scramble();

        T transposeOrder(T dataValue, int index) => TransposeCompute(dataValue, values, index) with
        {
            Time = normalizeTime ? baseTime + index * timeStep : values[index].Time,
        };

        // Return immediate enumeration of computed values
        foreach (T dataValue in values.Take(valueN).Select(transposeOrder))
            yield return dataValue;
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Random<DataSourceValue>
    {
        /// <inheritdoc />
        protected override DataSourceValue TransposeCompute(DataSourceValue currentValue, DataSourceValue[] values, int index) => currentValue with
        {
            Value = values[index].Value,
            Target = values[index].Target,
            Flags = values[index].Flags
        };
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Random<PhasorValue>
    {
        /// <inheritdoc />
        protected override PhasorValue TransposeCompute(PhasorValue currentValue, PhasorValue[] values, int index) => currentValue with
        {
            Magnitude = values[index].Magnitude,
            Angle = values[index].Angle,
            MagnitudeTarget = values[index].MagnitudeTarget,
            AngleTarget = values[index].AngleTarget,
            Flags = values[index].Flags
        };
    }
}