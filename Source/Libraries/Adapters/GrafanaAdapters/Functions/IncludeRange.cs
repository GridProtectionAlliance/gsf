using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.FunctionParsing;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Returns a series of values that represent a filtered set of the values in the source series where each value falls between the specified low and high.
/// The low and high parameter values are floating-point numbers that represent the range of values allowed in the return series. Third parameter, optional,
/// is a boolean flag that determines if range values are inclusive, i.e., allowed values are &gt;= low and &lt;= high - defaults to false, which means values
/// are exclusive, i.e., allowed values are &gt; low and &lt; high. Function allows a fourth optional parameter that is a boolean flag - when four parameters
/// are provided, third parameter determines if low value is inclusive and forth parameter determines if high value is inclusive. The low and high parameter
/// values can either be constant values or named targets available from the expression.
/// </summary>
/// <remarks>
/// Signature: <c>IncludeRange(low, high, [inclusive = false], expression) -or- IncludeRange(low, high, [lowInclusive = false], [highInclusive = false], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>IncludeRange(59.90, 60.10, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: IncludeRange, Include<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class IncludeRange<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "IncludeRange";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent a filtered set of the values in the source series where each value falls between the specified low and high.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Include" };

    /// <inheritdoc />
    public override ParameterDefinitions Parameters => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "low",
            Default = 0,
            Description = "A floating point value representing the low end of the range allowed in the return series.",
            Required = true
        },
        new ParameterDefinition<double>
        {
            Name = "high",
            Default = 0,
            Description = "A floating point value representing the high end of the range allowed in the return series.",
            Required = true
        },
        new ParameterDefinition<bool>
        {
            Name = "lowInclusive",
            Default = false,
            Description = "A boolean flag which determines if low value is inclusive.",
            Required = false
        },
        new ParameterDefinition<bool>
        {
            Name = "highInclusive",
            Default = false,
            Description = "A boolean flag which determines if high value is inclusive.",
            Required = false
        },
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : IncludeRange<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //double low = (parameters[0] as IParameter<double>).Value;
            //double high = (parameters[1] as IParameter<double>).Value;
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[2] as IParameter<IDataSourceValueGroup>).Value;
            //bool lowInclusive = (parameters[3] as IParameter<bool>).Value;
            //bool highInclusive = (parameters[4] as IParameter<bool>).Value;

            //// Compute
            //IEnumerable<DataSourceValue> selectedValues = dataSourceValues.Source.Where(dataValue => (lowInclusive ? dataValue.Value >= low : dataValue.Value > low) && (highInclusive ? dataValue.Value <= high : dataValue.Value < high));

            //// Set Values
            //dataSourceValues.Target = $"{low},{high},{dataSourceValues.Target},{lowInclusive},{highInclusive}";
            //dataSourceValues.Source = selectedValues;

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : IncludeRange<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //double low = (parameters[0] as IParameter<double>).Value;
            //double high = (parameters[1] as IParameter<double>).Value;
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[2] as IParameter<IDataSourceValueGroup>).Value;
            //bool lowInclusive = (parameters[3] as IParameter<bool>).Value;
            //bool highInclusive = (parameters[4] as IParameter<bool>).Value;

            //// Compute
            //IEnumerable<PhasorValue> selectedValues = phasorValues.Source.Where(phasorValue => (lowInclusive ? phasorValue.Magnitude >= low : phasorValue.Magnitude > low) && (highInclusive ? phasorValue.Magnitude <= high : phasorValue.Magnitude < high));

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{low},{high},{labels[0]},{lowInclusive},{highInclusive};{low},{high},{labels[1]},{lowInclusive},{highInclusive}";
            //phasorValues.Source = selectedValues;

            //return phasorValues;
            return null;
        }
    }
}