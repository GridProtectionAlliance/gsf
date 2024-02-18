using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent each of the values in the source series scaled by <c>N</c>.
/// <c>N</c> is a floating point value representing a scaling factor (multiplier or reciprocal) to be applied to each value the source series.
/// <c>N</c> can either be constant value or a named target available from the expression. The <c>asReciprocal</c> is a boolean parameter that,
/// when <c>true</c>, requests that <c>N</c> be treated as a reciprocal, i.e., 1 / <c>N</c>, thus resulting in a division operation instead of
/// multiplication - defaults to <c>false</c>.
/// </summary>
/// <remarks>
/// Signature: <c>Scale(N, [asReciprocal = false], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example 1: <c>Scale(1.5, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
/// Example 2: <c>Scale(0.5, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Example 3: <c>Scale(60, true, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Scale<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Scale<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Scale<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent each of the values in the source series scaled by N.";

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "N",
            Default = 1.0D,
            Description = "A floating point value representing a scaling factor (multiplier or reciprocal) to be applied to each value the source series.",
            Required = true
        },
        new ParameterDefinition<bool>
        {
            Name = "asReciprocal",
            Default = false,
            Description = "A boolean value indicating if N should treated as a reciprocal, i.e., 1 / N, thus requesting a division operation instead of multiplication.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        double valueN = parameters.Value<double>(0);

        if (parameters.Value<bool>(1))
            valueN = 1.0D / valueN;

        return ExecuteFunction(value => value * valueN, parameters);
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Scale<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Scale<PhasorValue>
    {
        // Function computed for both magnitude and angle
    }
}