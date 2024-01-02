using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the difference between consecutive values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Difference(expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Difference(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Difference, Diff<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Difference<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Difference";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the difference between consecutive values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Diff" };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Difference<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //double previousValue = dataSourceValues.Source.First().Value;
            //IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Skip(1).Select(dataSourceValue =>
            //{
            //    DataSourceValue transformedValue = dataSourceValue;

            //    transformedValue.Value -= previousValue;

            //    previousValue = dataSourceValue.Value;
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
    public class ComputePhasorValue : Difference<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //double previousMag = phasorValues.Source.First().Magnitude;
            //double previousAng = phasorValues.Source.First().Angle;
            //IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Skip(1).Select(phasorValue =>
            //{
            //    PhasorValue transformedValue = phasorValue;

            //    transformedValue.Magnitude -= previousMag;
            //    transformedValue.Angle -= previousAng;

            //    previousMag = phasorValue.Magnitude;
            //    previousAng = phasorValue.Angle;
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