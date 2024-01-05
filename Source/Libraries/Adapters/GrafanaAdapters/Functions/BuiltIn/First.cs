using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of N, or N% of total, values from the start of the source series.
/// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
/// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.
/// N can either be constant value or a named target available from the expression. Any target values that fall between 0
/// and 1 will be treated as a percentage.
/// </summary>
/// <remarks>
/// Signature: <c>First([N|N% = 1], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>First(5%, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: First<br/>
/// Execution: Immediate in-memory array load for values of N greater than 1; otherwise, immediate enumeration of one.
/// </remarks>
public abstract class First<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "First";

    /// <inheritdoc />
    public override string Description => "Returns a series of N, or N% of total, values from the start of the source series.";

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
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        IEnumerable<T> source = GetDataSourceValues(parameters);

        if (parameters.ParsedCount == 0)
        {
            // Short cut for only getting first value
            using IEnumerator<T> enumerator = source.GetEnumerator();

            if (enumerator.MoveNext())
                yield return enumerator.Current;
        }
        else
        {
            // Immediately load values in-memory only enumerating data source once
            T[] values = source.ToArray();
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
    public class ComputeDataSourceValue : First<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : First<PhasorValue>
    {
        // Operating on magnitude only
    }
}