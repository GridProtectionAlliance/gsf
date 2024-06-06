using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent each of the values in the source series modulo by <c>N</c>.
/// <c>N</c> is a floating point value representing a divisive factor to be applied to each value the source series.
/// <c>N</c> can either be constant value or a named target available from the expression.
/// </summary>
/// <remarks>
/// Signature: <c>Modulo(N, expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Mod(2, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
/// Variants: Modulo, Modulus, Mod<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Modulo<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Modulo<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent each of the values in the source series modulo by N.";

    /// <inheritdoc />
    public override string[] Aliases => ["Modulus", "Mod"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "N",
            Default = 1.0D,
            Description = "A floating point value representing a divisive factor to be applied to each value the source series.",
            Required = true
        }
    };

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        double valueN = parameters.Value<double>(0);
        return ExecuteFunction(value => value % valueN, parameters);
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Modulo<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Modulo<PhasorValue>
    {
        // Function computed for both magnitude and angle
    }
}