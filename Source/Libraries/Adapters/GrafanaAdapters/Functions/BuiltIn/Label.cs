using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Renames a series with the specified label <c>value</c>. If multiple series are targeted, labels will be indexed starting at one, e.g., if there are three series
/// in the target expression with a label value of "Max", series would be labeled as "Max 1", "Max 2" and "Max 3". Group operations on this function will be ignored.
/// Label <c>value</c>parameter can be optionally quoted with single or double quotes.<br/><br/>
/// The label parameter also supports substitutions when root target metadata can be resolved. For series values that directly map to a point tag, metadata value
/// substitutions for the tag can be used in the label value - for example: <c>{Alias}</c>, <c>{ID}</c>, <c>{SignalID}</c>, <c>{PointTag}</c>, <c>{AlternateTag}</c>,
/// <c>{SignalReference}</c>, <c>{Device}</c>, <c>{FramesPerSecond}</c>, <c>{Protocol}</c>, <c>{ProtocolType}</c>, <c>{SignalType}</c>, <c>{EngineeringUnits}</c>,
/// <c>{PhasorType}</c>, <c>{PhasorLabel}</c>, <c>{BaseKV}</c>, <c>{Company}</c>, <c>{Longitude}</c>, <c>{Latitude}</c>, <c>{Description}</c>, etc. Each of these
/// fields come from the "ActiveMeasurements" metadata source, as defined in the "ConfigurationEntity" table. Where applicable, substitutions can be used along with
/// fixed label text in any combination, e.g.: <c>'Series {ID} [{PointTag}]'</c>.<br/><br/>
/// Other metadata sources that target time-series measurements can also be used for substitutions so long the source is defined in the "ConfigurationEntity" table
/// and the metadata columns include a "PointTag" field that can be matched to the target Grafana series name. To use any field from another defined metadata source,
/// use the following substitution parameter format: <c>{TableName.FieldName}</c>.
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
public abstract class Label<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Label<T>);

    /// <inheritdoc />
    public override string Description => "Renames a series with the specified label value.";

    /// <inheritdoc />
    public override string[] Aliases => ["Name"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override GroupOperations AllowedGroupOperations => GroupOperations.None;

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.None;

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
        // Label function ignores any requested group operation instead of throwing an exception:
        return GroupOperations.None;
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        // Label function performs no computation, it only renames series, so inputs are returned as-is,
        // actual rename operation is handled as a special case by the base class
        return GetDataSourceValues(parameters);
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Label<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Label<PhasorValue>
    {
    }
}