using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.FunctionParsing;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Ceiling(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Ceiling(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Variants: Ceiling, Ceil<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Ceiling<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Ceiling";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Ceil" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Ceiling<DataSourceValue>
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
            //    transformedValue.Value = Math.Ceiling(transformedValue.Value);

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
    public class ComputePhasorValue : Ceiling<PhasorValue>
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
            //    transformedValue.Magnitude = Math.Ceiling(transformedValue.Magnitude);
            //    transformedValue.Angle = Math.Ceiling(transformedValue.Angle);

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