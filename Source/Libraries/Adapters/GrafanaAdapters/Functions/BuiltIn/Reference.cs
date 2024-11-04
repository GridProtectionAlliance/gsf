using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using GSF.Units;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a slice of angle differences to the first angle (i.e., the reference) for a series of angles. The <c>sliceTolerance</c>
/// parameter is a floating-point value that must be greater than or equal to 0.001 that represents the desired time tolerance,
/// in seconds, for the time slice. Parameter <c>adjustCoordinateMidPoint</c>, optional, is a boolean flag that determines if the
/// metadata of the coordinate system, i.e., longitude/latitude values, should be adjusted to the midpoint between reference and
/// the angle values in the slice - defaults to false. Parameter <c>applyWrapOps</c>, optional, is a boolean flag that determines
/// if angles should be unwrapped before computing differences then rewrapped - defaults to true. The <c>units</c>parameter,
/// optional, specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds
/// or AngularMil - defaults to Degrees.
/// </summary>
/// <remarks>
/// Signature: <c>Reference(sliceTolerance, [adjustCoordinateMidPoint = false], [applyWrapOps = true], [units = Degrees], expression)</c><br/>
/// Returns: Single value.<br/>
/// Example 1: <c>Reference(true, false, BROWNS_FERRY:BUS1.ANG; FILTER ActiveMeasurements WHERE SignalType='IPHA')</c><br/>
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
            Description = "A boolean flag that determines if the metadata for angles of the coordinate system should be adjusted to the midpoint between the angle and the reference.",
            Required = false
        },
        new ParameterDefinition<bool>
        {
            Name = "applyWrapOps",
            Default = true,
            Description = "A boolean flag that determines if angles should be unwrapped before computing differences then rewrapped.",
            Required = false
        },
        new ParameterDefinition<AngleUnit>
        {
            Name = "units",
            Default = AngleUnit.Degrees,
            Description = "Specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds or AngularMil.",
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
        bool adjustCoordinateMidPoint = parameters.Value<bool>(0);
        bool applyWrapOps = parameters.Value<bool>(1);
        AngleUnit units = parameters.Value<AngleUnit>(2);

        await using IAsyncEnumerator<T> enumerator = GetDataSourceValues(parameters).GetAsyncEnumerator(cancellationToken);

        // Get reference from first series
        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
            yield break;

        double reference = enumerator.Current.Value;

        // Each data source value in a slice is part of different series that will have their own metadata maps
        Dictionary<string, MetadataMap> metadataMaps = parameters.MetadataMaps;
        MetadataMap refCoordinates = null;
        double refLongitude = default, refLatitude = default;

        if (adjustCoordinateMidPoint)
        {
            // Store reference coordinates for later adjustment
            if (adjustCoordinateMidPoint = metadataMaps.TryGetValue(enumerator.Current.Target, out refCoordinates) && refCoordinates is not null)
            {
                adjustCoordinateMidPoint = tryParseCoordinates(refCoordinates, out refLongitude, out refLatitude);

                // Only need to adjust mid-point once for the series metadata, if key exists, mid-point has already been applied
                if (adjustCoordinateMidPoint && refCoordinates.ContainsKey("MidPointApplied"))
                    adjustCoordinateMidPoint = false;
            }
        }

        // Return first series, the reference, always zero
        yield return enumerator.Current with
        {
            Value = 0.0D
        };

        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            if (adjustCoordinateMidPoint)
            {
                if (metadataMaps.TryGetValue(enumerator.Current.Target, out MetadataMap currentCoordinates) && currentCoordinates is not null)
                {
                    // Adjust coordinate metadata for current value to midpoint between itself and the reference
                    if (tryParseCoordinates(currentCoordinates, out double currentLongitude, out double currentLatitude))
                    {
                        // NOTE: This initial implementation assumes geodesics are not required, i.e., distances
                        // between geographic locations are small enough that linear mid-point is sufficient
                        currentCoordinates["Longitude"] = ((refLongitude + currentLongitude) / 2.0D).ToString();
                        currentCoordinates["Latitude"] = ((refLatitude + currentLatitude) / 2.0D).ToString();
                        currentCoordinates["MidPointApplied"] = "true";
                    }
                    else
                    {
                        currentCoordinates["MidPointApplied"] = "false";
                    }
                }
            }

            if (applyWrapOps)
            {
                // Apply unwrap operations to angles converted to radians
                Angle[] angles = Angle.Unwrap([
                    Angle.ConvertFrom(enumerator.Current.Value, units), 
                    Angle.ConvertFrom(reference, units)
                ]).ToArray();

                yield return enumerator.Current with
                {
                    // Return difference between unwrapped angles, rewrapped and converted to original units
                    Value = (angles[0] - angles[1]).ToRange(-Math.PI, false).ConvertTo(units)
                };
            }
            else
            {
                yield return enumerator.Current with
                {
                    Value = enumerator.Current.Value - reference
                };
            }
        }

        // Track that mid-point calculations been applied to metadata using reference coordinates as a marker
        if (adjustCoordinateMidPoint)
            refCoordinates["MidPointApplied"] = "false"; // False because reference coordinates were not adjusted

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
