using System.Collections.Generic;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent each of the values in the source series added with N.
/// N is a floating point value representing an additive offset to be applied to each value the source series.
/// N can either be constant value or a named target available from the expression.
/// </summary>
/// <remarks>
/// Signature: <c>Add(N, expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Add(1.5, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
/// Variants: Add<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Add<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Add<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent each of the values in the source series added with N.";

    /// <inheritdoc />
    // Hiding slice operation since result matrix would be the same when tolerance matches data rate
    public override GroupOperations PublishedGroupOperations => GroupOperations.Standard | GroupOperations.Set;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "N",
            Default = 0.0D,
            Description = "A floating point value representing an additive offset to be applied to each value the source series.",
            Required = true
        }
    };

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        double valueN = parameters.Value<double>(0);
        return ExecuteFunction(value => value + valueN, parameters);
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Add<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Add<PhasorValue>
    {
        // Function computed for both magnitude and angle
    }
}