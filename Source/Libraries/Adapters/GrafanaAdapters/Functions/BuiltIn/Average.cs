using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the mean of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Average(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Average(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Average, Avg, Mean<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Average<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Average";

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the mean of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Avg", "Mean" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Average<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>)!.Value;

            //// Compute
            //DataSourceValue lastElement = dataSourceValues.Source.Last();
            //lastElement.Value = dataSourceValues.Source.Select(dataValue => dataValue.Value).Average();

            //// Set Values
            //dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            //dataSourceValues.Source = Enumerable.Repeat(lastElement, 1);

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Average<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>)!.Value;

            //// Compute
            //PhasorValue lastElement = phasorValues.Source.Last();

            //lastElement.Magnitude = phasorValues.Source.Select(dataValue => dataValue.Magnitude).Average();

            //lastElement.Angle = phasorValues.Source.Select(dataValue => dataValue.Angle).Average();

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            //phasorValues.Source = Enumerable.Repeat(lastElement, 1);

            //return phasorValues;
            return null;
        }
    }
}