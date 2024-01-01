using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.FunctionParsing;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Returns a single value that is the count of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Count(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Count(PPA:1; PPA:2; PPA:3)</c><br/>
/// Variants: Count<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Count<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Count";

    /// <inheritdoc />
    public override string Description => "Returns a single value that is the count of the values in the source series.";

    /// <inheritdoc />
    public class ComputeDataSourceValue : Count<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //DataSourceValue lastElement = dataSourceValues.Source.Last();
            //lastElement.Value = dataSourceValues.Source.Count();

            //// Set Values
            //dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            //dataSourceValues.Source = Enumerable.Repeat(lastElement, 1);

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Count<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //PhasorValue lastElement = phasorValues.Source.Last();
            //lastElement.Magnitude = phasorValues.Source.Count();
            //lastElement.Angle = phasorValues.Source.Count();

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            //phasorValues.Source = Enumerable.Repeat(lastElement, 1);

            //return phasorValues;
            return null;
        }
    }
}