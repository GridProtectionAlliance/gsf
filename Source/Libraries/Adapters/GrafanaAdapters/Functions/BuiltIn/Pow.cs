using System;
using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent each of the values in the source series raised to the power of N.
/// N is a floating point value representing an exponent used to raise each value of the source series to the specified power.
/// N can either be constant value or a named target available from the expression.
/// </summary>
/// <remarks>
/// Signature: <c>Pow(N, expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Pow(2, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
/// Variants: Pow<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Pow<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Pow<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent each of the values in the source series raised to the power of N.";

    /// <inheritdoc />
    // Hiding slice operation since result matrix would be the same when tolerance matches data rate
    public override GroupOperations PublishedGroupOperations => GroupOperations.Standard | GroupOperations.Set;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "N",
            Default = 1.0D,
            Description = "A floating point value representing an exponent used to raise each value of the source series to the specified power.",
            Required = true
        }
    };

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        double valueN = parameters.Value<double>(0);
        return ExecuteFunction(value => Math.Pow(value, valueN), parameters);
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Pow<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Pow<PhasorValue>
    {
        // Function computed for both magnitude and angle
    }
}