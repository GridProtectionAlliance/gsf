using System.Collections.Generic;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent each of the values in the source series subtracted by N.
/// N is a floating point value representing an subtractive offset to be applied to each value the source series.
/// N can either be constant value or a named target available from the expression.
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
    public override string Name => "Subtract";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent each of the values in the source series subtracted with N.";

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.Standard | GroupOperations.Set;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "N",
            Default = 0,
            Description = "A floating point value representing an subtractive offset to be applied to each value the source series.",
            Required = true
        }
    };

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
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
    }
}