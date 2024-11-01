using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using GSF;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a slice of angle differences to the first angle (i.e., the reference) for a series of angles. The <c>sliceTolerance</c>
/// parameter is a floating-point value that must be greater than or equal to 0.001 that represents the desired time tolerance,
/// in seconds, for the time slice. Parameter <c>adjustCoordinateMidPoint</c>, optional, is a boolean flag that determines if the
/// metadata of the coordinate system, i.e., longitude/latitude values, should be adjusted to the midpoint between the first and
/// second values in the slice, storing the updated result in the metadata of the first value - defaults to false.
/// </summary>
/// <remarks>
/// Signature: <c>Reference(sliceTolerance, [adjustCoordinateMidPoint = false], expression)</c><br/>
/// Returns: Single value.<br/>
/// Example 1: <c>Reference(true, BROWNS_FERRY:BUS1.ANG; FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Example 2: <c>Reference(BROWNS_FERRY:BUS1; FILTER PhasorValues WHERE SignalType='IPHM')</c><br/>
/// Variants: Reference, Ref<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Reference<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Reference<T>);

    /// <inheritdoc />
    public override string Description => "Returns a slice of angle differences to the first angle (i.e., the reference) for a series of angles.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Ref" };

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<bool>
        {
            Name = "adjustCoordinateMidPoint",
            Default = false,
            Description = "A boolean flag that determines if the metadata for the first value of the coordinate system should be adjusted to the midpoint between the first and second values in the slice.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override bool IsSliceSeriesEquivalent => false;

    /// <inheritdoc />
    // Function only operates on slices. Other group operations are ignored, see CheckAllowedGroupOperation.
    public override GroupOperations AllowedGroupOperations => GroupOperations.Slice;

    /// <inheritdoc />
    // Only non-group operation "Reference" is published instead of requiring "SliceReference".
    public override GroupOperations PublishedGroupOperations => GroupOperations.None;

    // No parameters other than slice tolerance are required for this function. Note that
    // required slice tolerance parameter added automatically by Grafana function handling.

    /// <inheritdoc />
    public override GroupOperations CheckAllowedGroupOperation(GroupOperations requestedOperation)
    {
        // Force group operation to be Slice as Reference only supports slice operations. This ignores
        // any requested group operation instead of throwing an exception based on what is allowed.
        return GroupOperations.Slice;
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Each data source value in a slice is part of different series that will have their own metadata maps
        Dictionary<string, MetadataMap> metadataMaps = parameters.MetadataMaps;
        MetadataMap firstCoordinates = null, secondCoordinates = null;
        bool adjustCoordinateMidPoint = parameters.Value<bool>(0);

        await using IAsyncEnumerator<T> enumerator = GetDataSourceValues(parameters).GetAsyncEnumerator(cancellationToken);

        // Get reference from first series
        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
            yield break;

        double reference = enumerator.Current.Value;

        // Store reference coordinates for later adjustment
        if (adjustCoordinateMidPoint)
        {
            if (adjustCoordinateMidPoint = metadataMaps.TryGetValue(enumerator.Current.Target, out firstCoordinates) && firstCoordinates is not null)
            {
                // Only need to adjust mid-point only for the series
                if (firstCoordinates.TryGetValue("MidPointApplied", out string applied) && applied.ParseBoolean())
                    adjustCoordinateMidPoint = false;
            }
        }

        // Return First Series
        yield return enumerator.Current with
        {
            Value = enumerator.Current.Value - reference
        };

        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            if (adjustCoordinateMidPoint && secondCoordinates is null)
            {
                // It's OK to adjust coordinate metadata for first value here after having already yielded its value
                // above as result set will be serialized together and thus resynchronized before return to Grafana
                if (metadataMaps.TryGetValue(enumerator.Current.Target, out secondCoordinates) && secondCoordinates is not null)
                {
                    // Adjust coordinate metadata for first value to midpoint between first and second values
                    if (tryParseCoordinates(firstCoordinates, out double firstLongitude, out double firstLatitude) && 
                        tryParseCoordinates(secondCoordinates, out double secondLongitude, out double secondLatitude))
                    {
                        // NOTE: This initial implementation assumes geodesics are not required, i.e., distances
                        // between geographic locations are small enough that linear mid-point is sufficient
                        firstCoordinates["Longitude"] = ((firstLongitude + secondLongitude) / 2.0D).ToString();
                        firstCoordinates["Latitude"] = ((firstLatitude + secondLatitude) / 2.0D).ToString();
                        firstCoordinates["MidPointApplied"] = "true";
                    }
                }
                else
                {
                    adjustCoordinateMidPoint = false;
                }
            }

            yield return enumerator.Current with
            {
                Value = enumerator.Current.Value - reference
            };
        }

        static bool tryParseCoordinate(MetadataMap metadataMap, string coordinate, out double value)
        {
            value = default;
            return metadataMap.TryGetValue(coordinate, out string setting) && double.TryParse(setting, out value);
        }

        static bool tryParseCoordinates(MetadataMap metadataMap, out double longitude, out double latitude)
        {
            latitude = default;
            return tryParseCoordinate(metadataMap, "Longitude", out longitude) && tryParseCoordinate(metadataMap, "Latitude", out latitude);
        }
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Reference<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Reference<PhasorValue>
    {
        /// <inheritdoc />
        protected override IAsyncEnumerable<PhasorValue> GetDataSourceValues(Parameters parameters)
        {
            // Update data source values to operate on angle components of phasor values, this
            // allows same base class logic to apply to both measurement and phasor values
            return base.GetDataSourceValues(parameters).Select(PhasorValue.AngleAsTarget);
        }
    }
}