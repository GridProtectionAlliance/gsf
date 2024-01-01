using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.FunctionParsing;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Returns a series of values that represent each of the values in the source series modulo by N.
/// N is a floating point value representing a divisive factor to be applied to each value the source series.
/// N can either be constant value or a named target available from the expression.
/// </summary>
/// <remarks>
/// Signature: <c>Modulo(N, expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Mod(2, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
/// Variants: Modulo, Modulus, Mod<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Modulo<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Modulo";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent each of the values in the source series modulo by N.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Modulus", "Mod" };

    /// <inheritdoc />
    public override ParameterDefinitions Parameters => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "N",
            Default = 0,
            Description = "A floating point value representing a divisive factor to be applied to each value the source series.",
            Required = true
        },
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Modulo<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //double value = (parameters[0] as IParameter<double>).Value;
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Select(dataSourceValue =>
            //{
            //    DataSourceValue transformedValue = dataSourceValue;
            //    transformedValue.Value %= value;

            //    return transformedValue;
            //});

            //// Set Values
            //dataSourceValues.Target = $"{dataSourceValues.Target}/{value}";
            //dataSourceValues.Source = transformedDataSourceValues;

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Modulo<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //double value = (parameters[0] as IParameter<double>).Value;
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

            //// Compute
            //IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Select(phasorValue =>
            //{
            //    PhasorValue transformedValue = phasorValue;
            //    transformedValue.Magnitude %= value;

            //    return transformedValue;
            //});

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{labels[0]}%{value};{labels[1]}%{value}";
            //phasorValues.Source = transformedPhasorValues;

            //return phasorValues;
            return null;
        }
    }
}