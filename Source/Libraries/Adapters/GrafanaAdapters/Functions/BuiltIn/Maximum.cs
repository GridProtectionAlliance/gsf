using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that is the maximum of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Maximum(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Maximum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Maximum, Max<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Maximum<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Maximum";

    /// <inheritdoc />
    public override string Description => " Returns a single value that is the maximum of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Max" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Maximum<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //DataSourceValue minimumDataSourceValue = dataSourceValues.Source.OrderBy(dataValue => dataValue.Value).Last();

            //// Set Values
            //dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            //dataSourceValues.Source = Enumerable.Repeat(minimumDataSourceValue, 1);

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Maximum<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //PhasorValue minimumPhasorValue = phasorValues.Source.OrderBy(dataValue => dataValue.Magnitude).Last();

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            //phasorValues.Source = Enumerable.Repeat(minimumPhasorValue, 1);

            //return phasorValues;
            return null;
        }
    }
}