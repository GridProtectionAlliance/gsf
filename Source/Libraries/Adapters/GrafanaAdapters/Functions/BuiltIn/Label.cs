using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Renames a series with the specified label value. If multiple series are targeted, labels will be indexed starting at one, e.g., if there are three
/// series in the target expression with a label value of "Max", series would be labeled as "Max 1", "Max 2" and "Max 3". Group operations on this
/// function will be ignored. The label parameter also supports substitutions when root target metadata can be resolved. For series values that directly
/// map to a point tag, metadata value substitutions for the tag can be used in the label value - for example: {ID}, {SignalID}, {PointTag}, {AlternateTag},
/// {SignalReference}, {Device}, {FramesPerSecond}, {Protocol}, {ProtocolType}, {SignalType}, {EngineeringUnits}, {PhasorType}, {Company}, {Description} -
/// where applicable, these substitutions can be used in any combination.
/// </summary>
/// <remarks>
/// Signature: <c>Label(value, expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example 1: <c>Label('AvgFreq', SetAvg(FILTER TOP 20 ActiveMeasurements WHERE SignalType='FREQ'))</c>
/// Example 2: <c>Label("{Alias} {EngineeringUnits}", Shelby=GPA_SHELBY:FREQ)</c>
/// Example 3: <c>Label({AlternateTag}, FILTER TOP 10 ActiveMeasurements WHERE SignalType LIKE '%PH%')</c>
/// Example 4: <c>Label('Shelby {ScadaTags.CircuitName} MW', FILTER ScadaTags WHERE SignalType='MW' AND Substation='SHELBY')</c>
/// Variants: Label, Name<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Label<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Label<T>);

    /// <inheritdoc />
    public override string Description => "Renames a series with the specified label value.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Name" };

    /// <inheritdoc />
    public override GroupOperations AllowedGroupOperations => GroupOperations.Standard;

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.Standard;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<string>
        {
            Name = "value",
            Default = "",
            Description = "A string expression representing a new label for a target series.",
            Required = true
        }
    };

    /// <inheritdoc />
    public override GroupOperations CheckAllowedGroupOperation(GroupOperations requestedOperation)
    {
        // Label function ignores any requested group operation instead of throwing an exception
        return GroupOperations.Standard;
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        // Label function performs no computation, it only renames series,
        // operation is handled as a special case by the base class
        return GetDataSourceValues(parameters);
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Label<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Label<PhasorValue>
    {
    }
}