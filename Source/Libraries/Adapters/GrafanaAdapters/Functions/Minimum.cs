using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.FunctionParsing;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Returns a single value that is the minimum of the values in the source series.
/// </summary>
public abstract class Minimum<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Minimum";

    /// <inheritdoc />
    public override string Description => "Returns a single value that is the minimum of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Min" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Minimum<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //DataSourceValue minimumDataSourceValue = dataSourceValues.Source.OrderBy(dataValue => dataValue.Value).First();

            //// Set Values
            //dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            //dataSourceValues.Source = Enumerable.Repeat(minimumDataSourceValue, 1);

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Minimum<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //PhasorValue minimumPhasorValue = phasorValues.Source.OrderBy(dataValue => dataValue.Magnitude).First();

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            //phasorValues.Source = Enumerable.Repeat(minimumPhasorValue, 1);

            //return phasorValues;
            return null;
        }
    }
}