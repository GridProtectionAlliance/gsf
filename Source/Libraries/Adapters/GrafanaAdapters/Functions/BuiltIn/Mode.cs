using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the mode of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Mode(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Mode(FILTER TOP 5 ActiveMeasurements WHERE SignalType='DIGI')</c><br/>
/// Variants: Mode<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class Mode<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Mode";

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the mode of the values in the source series.";

    //private static double CalculateMode(IEnumerable<double> values)
    //{
    //    if (!values.Any())
    //        return 0.0;

    //    var groupedValues = values
    //        .GroupBy(v => v)
    //        .Select(group => new { Value = group.Key, Count = group.Count() })
    //        .ToList();

    //    int maxCount = groupedValues.Max(g => g.Count);

    //    return groupedValues.First(g => g.Count == maxCount).Value;
    //}

    /// <inheritdoc />
    public class ComputeDataSourceValue : Mode<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //DataSourceValue lastElement = dataSourceValues.Source.Last();
            //lastElement.Value = CalculateMode(dataSourceValues.Source.Select(pv => pv.Value));

            //// Set 
            //dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            //dataSourceValues.Source = Enumerable.Repeat(lastElement, 1);

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Mode<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //PhasorValue lastElement = phasorValues.Source.Last();
            //lastElement.Magnitude = CalculateMode(phasorValues.Source.Select(pv => pv.Magnitude));
            //lastElement.Angle = CalculateMode(phasorValues.Source.Select(pv => pv.Angle));


            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            //phasorValues.Source = Enumerable.Repeat(lastElement, 1);
            //;

            //return phasorValues;
            return null;
        }
    }
}