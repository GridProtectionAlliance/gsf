﻿using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using System.Collections.Generic;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent each of the values in the source series subtracted by <c>N</c>.
/// <c>N</c> is a floating point value representing an subtractive offset to be applied to each value the source series.
/// <c>N</c> can either be constant value or a named target available from the expression.
/// </summary>
/// <remarks>
/// Signature: <c>Subtract(N, expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Subtract(2.2, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
/// Variants: Subtract<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Subtract<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Subtract<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent each of the values in the source series subtracted with N.";

    /// <inheritdoc />
    // Hiding slice operation since result matrix would be the same when tolerance matches data rate
    public override GroupOperations PublishedGroupOperations => GroupOperations.None | GroupOperations.Set;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "N",
            Default = 0.0D,
            Description = "A floating point value representing an subtractive offset to be applied to each value the source series.",
            Required = true
        }
    };

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        double valueN = parameters.Value<double>(0);
        return ExecuteFunction(value => value - valueN, parameters);
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Subtract<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Subtract<PhasorValue>
    {
        // Function computed for both magnitude and angle
    }
}