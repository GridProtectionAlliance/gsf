using System.Collections.Generic;
using System.Linq;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of N, or N% of total, values from the end of the source series.
/// N, optional, is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
/// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.
/// N can either be constant value or a named target available from the expression. Any target values that fall between 0
/// and 1 will be treated as a percentage.
/// </summary>
/// <remarks>
/// Signature: <c>Last([N|N% = 1], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Last(150, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Last<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class Last<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Last<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of N, or N% of total, values from the end of the source series.";

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

        // Immediately load values in-memory only enumerating data source once
        T[] values = source.ToArray();
        int length = values.Length;

        if (length == 0)
            yield break;

        int valueN = ParseTotal(parameters.Value<string>(0), length);

        if (valueN > length)
            valueN = length;

        for (int i = 0; i < valueN; i++)
            yield return values[length - i - i];
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Last<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Last<PhasorValue>
    {
        // Operating on magnitude only
    }
}