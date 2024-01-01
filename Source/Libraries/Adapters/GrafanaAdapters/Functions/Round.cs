using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.FunctionParsing;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Returns a series of values that represent the rounded value, with N fractional digits, of each of the values in the source series.
/// N, optional, is a positive integer value representing the number of decimal places in the return value - defaults to 0.
/// </summary>
/// <remarks>
/// Signature: <c>Round([N = 0], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Round(3, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Round<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Round<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Round";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the rounded value, with N fractional digits, of each of the values in the source series.";

    /// <inheritdoc />
    public override ParameterDefinitions Parameters => new List<IParameter>
    {
        new ParameterDefinition<int>
        {
            Name = "N",
            Default = 0,
            Description = "A positive integer value representing the number of decimal places in the return value - defaults to 0.",
            Required = false
        }
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Round<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //int numberDecimals = (parameters[1] as IParameter<int>).Value;

            //// Compute
            //IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataSourceValue =>
            //{
            //    DataSourceValue transformedValue = dataSourceValue;
            //    transformedValue.Value = Math.Round(transformedValue.Value, numberDecimals);

            //    return transformedValue;
            //});

            //// Set Values
            //dataSourceValues.Target = $"{Name}({dataSourceValues.Target},{numberDecimals})";
            //dataSourceValues.Source = transformedDataSourceValues;

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Round<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //int numberDecimals = (parameters[1] as IParameter<int>).Value;

            //// Compute
            //IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Select(phasorValue =>
            //{
            //    PhasorValue transformedValue = phasorValue;
            //    transformedValue.Magnitude = Math.Round(transformedValue.Magnitude, numberDecimals);
            //    transformedValue.Angle = Math.Round(transformedValue.Angle, numberDecimals);

            //    return transformedValue;
            //});

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{Name}({labels[0]},{numberDecimals});{Name}({labels[1]},{numberDecimals})";
            //phasorValues.Source = transformedPhasorValues;

            //return phasorValues;
            return null;
        }
    }
}