using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.FunctionParsing;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Returns a series of values that represent the smallest integral value that is less than or equal to each of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Floor(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Floor(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Variants: Floor<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Floor<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Floor";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the smallest integral value that is less than or equal to each of the values in the source series.";

    /// <inheritdoc />
    public class ComputeDataSourceValue : Floor<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataSourceValue =>
            //{
            //    DataSourceValue transformedValue = dataSourceValue;
            //    transformedValue.Value = Math.Floor(transformedValue.Value);

            //    return transformedValue;
            //});

            //// Set Values
            //dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            //dataSourceValues.Source = transformedDataSourceValues;

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Floor<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Select(phasorValue =>
            //{
            //    PhasorValue transformedValue = phasorValue;
            //    transformedValue.Magnitude = Math.Floor(transformedValue.Magnitude);
            //    transformedValue.Angle = Math.Floor(transformedValue.Angle);

            //    return transformedValue;
            //});

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            //phasorValues.Source = transformedPhasorValues;

            //return phasorValues;
            return null;
        }
    }
}