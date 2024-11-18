using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series with an extra MetaData Field.
/// </summary>
/// <remarks>
/// Signature: <c>AddMetaData(field, value, expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>AddMetaData('Company', 'GPA', FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class AddMetadata<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(AddMetadata<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values with an extra metadata field.";

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<string>
        {
            Name = "field",
            Default = "Field",
            Description = "A string containing the name of the field to be added.",
            Required = true
        },
        new ParameterDefinition<string>
        {
            Name = "value",
            Default = "Value",
            Description = "A string representing the value of the field to be added.",
            Required = true
        },
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Dictionary<string, MetadataMap> metadataMaps = parameters.MetadataMaps;
        string field = parameters.Value<string>(0);
        string value = parameters.Value<string>(1);

        await using IAsyncEnumerator<T> enumerator = GetDataSourceValues(parameters).GetAsyncEnumerator(cancellationToken);

        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            if (metadataMaps.TryGetValue(enumerator.Current.Target, out MetadataMap currentMetadata) && !currentMetadata.ContainsKey(field))
                currentMetadata[field] = value;

            yield return enumerator.Current;
        }
      
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : IncludeRange<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : IncludeRange<PhasorValue>
    {
        // Operating on magnitude only
    }
}