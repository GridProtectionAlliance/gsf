using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the rounded value, with specified fractional digits, of each of the values in the source series.
/// Parameter <c>digits</c>, optional, is a positive integer value representing the number of decimal places in the return value - defaults to 0.
/// </summary>
/// <remarks>
/// Signature: <c>Round([digits = 0], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Round(3, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Round<br/>
/// Execution: Deferred enumeration.<br/>
/// Group Operations: Slice, Set
/// </remarks>
public abstract class Round<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Round<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the rounded value, with N fractional digits, of each of the values in the source series.";

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<int>
        {
            Name = "digits",
            Default = 0,
            Description = "A positive integer value representing the number of decimal places in the return value.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        int digits = parameters.Value<int>(0);
        return ExecuteFunction(value => Math.Round(value, digits), parameters);
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Round<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Round<PhasorValue>
    {
        // Function computed for both magnitude and angle
    }
}