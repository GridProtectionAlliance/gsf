using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters.GrafanaFunctions;

/// <summary>
/// Returns a series of values that represent each of the values in the source series multiplied by N.
/// N is a floating point value representing a multiplicative factor to be applied to each value the source series.
/// N can either be constant value or a named target available from the expression.
/// </summary>
/// <remarks>
/// Signature: <c>Multiply(N, expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Multiply(1.5, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
/// Variants: Multiply<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Multiply<T> : GrafanaFunctionBase<T> where T : IDataSourceValue
{
    /// <inheritdoc />
    public override string Name => "Multiply";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent each of the values in the source series multiplied by N.";

    /// <inheritdoc />
    public override List<IParameter> Parameters => new()
    {
        new Parameter<double>
        {
            Default = 0,
            Description = "A floating point value representing an multiplicative offset to be applied to each value the source series.",
            Required = true
        },

        InputDataPointValues
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Multiply<DataSourceValue>
    {
        /// <inheritdoc />
        public override DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            // Get Values
            double value = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

            // Compute
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataSourceValue =>
            {
                DataSourceValue transformedValue = dataSourceValue;
                transformedValue.Value *= value;

                return transformedValue;
            });

            // Set Values
            dataSourceValues.Target = $"{value}*{dataSourceValues.Target}";
            dataSourceValues.Source = transformedDataSourceValues;

            return dataSourceValues;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Multiply<PhasorValue>
    {
        /// <inheritdoc />
        public override DataSourceValueGroup<PhasorValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            // Get Values
            double value = (parameters[0] as IParameter<double>).Value;
            DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

            // Compute
            IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Select(phasorValue =>
            {
                PhasorValue transformedValue = phasorValue;
                transformedValue.Magnitude *= value;

                return transformedValue;
            });

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{value}*{labels[0]};{value}*{labels[1]}";
            phasorValues.Source = transformedPhasorValues;

            return phasorValues;
        }
    }
}