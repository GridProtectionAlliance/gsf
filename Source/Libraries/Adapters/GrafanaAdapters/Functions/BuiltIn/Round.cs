using System;
using System.Collections.Generic;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the rounded value, with N fractional digits, of each of the values in the source series.
/// N, optional, is a positive integer value representing the number of decimal places in the return value - defaults to 0.
/// </summary>
/// <remarks>
/// Signature: <c>Round([digits = 0], expression)</c><br/>
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
    public override GroupOperations PublishedGroupOperations => GroupOperations.Standard | GroupOperations.Set;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<int>
        {
            Name = "digits",
            Default = 0,
            Description = "A positive integer value representing the number of decimal places in the return value - defaults to 0.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override IEnumerable<T> Compute(Parameters parameters)
    {
        int digits = parameters.Value<int>(0);
        return ExecuteFunction(value => Math.Round(value, digits), parameters);
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Round<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Round<PhasorValue>
    {
    }
}