using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using GSF.Drawing;
using GSF.Units;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values with their lattitude and longitude adjusted to avoid overlapping locations.
/// </summary>
/// <remarks>
/// Signature: <c>AdjustLocation(sliceTolerance, [adjustCoordinateMidPoint = false], [applyWrapOps = true], [units = Degrees], expression)</c><br/>
/// Returns: Single value.<br/>
/// Example 1: <c>AdjustLocation(true, false, BROWNS_FERRY:BUS1.ANG; FILTER ActiveMeasurements WHERE SignalType='IPHA')</c><br/>
/// Example 2: <c>AdjustLocation(FILTER PhasorValues WHERE SignalType='VPHM')</c><br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class AdjustLocation<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(AdjustLocation<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values with their lattitude and longitude adjusted to avoid overlapping locations.";

    /// <inheritdoc />
    public override string[] Aliases => new string[] { };

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "xOffset",
            Default = 0,
            Description = "should be adjusted to the midpoint between the angle and the reference.",
            Required = false
        },
        new ParameterDefinition<double>
        {
            Name = "yOffset",
            Default = 0,
            Description = " differences then rewrapped.",
            Required = false
        },
        new ParameterDefinition<double>
        {
            Name = "zoom",
            Default = 4,
            Description = "rcMinutes, ArcSeconds or AngularMil.",
            Required = true
        },
        new ParameterDefinition<double>
        {
            Name = "tolerance",
            Default = 0.000275,
            Description = "rcMinutes, ArcSeconds or AngularMil.",
            Required = true
        }
    };


    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {

        double xOffset = parameters.Value<double>(0);
        double yOffset = parameters.Value<double>(1);
        double zoom = parameters.Value<double>(2);
        double tolerance = parameters.Value<double>(3);


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



        Point translateSquare(Point point, int index, int count)
        {
            if (index == 0)
                return point;

            int ringIndex = ((int)Math.Sqrt(index) - 1) | 1;
            int ringDistance = (ringIndex + 1) / 2;
            int ringOffset = index - ringIndex * ringIndex;

            int[] xySignSequence = [1, -1, -1, 1, -1, 1, 1, -1];
            int xySign = xySignSequence[ringOffset % 8];
            int xyOffset = xySign * (ringOffset + 4) / 8;

            int yxSign = (ringOffset % 2 == 0) ? 1 : -1;
            int yxOffset = yxSign * ringDistance;

            int xTranslation = (ringOffset % 4) < 2 ? xyOffset : yxOffset;
            int yTranslation = (ringOffset % 4) < 2 ? yxOffset : xyOffset;
            double x = point.X + xOffset * xTranslation;
            double y = point.Y + yOffset * yTranslation;
            return new Point(x, y);
        }

        Point translateHorizontal(Point point, int index, int count)
        {
            double x = point.X;
            double y = point.Y;
            int distance = (index + 1) / 2;
            int direction = (index % 2) * 2 - 1;
            x += xOffset * distance * direction;
            return new Point(x, y);
        }

        Point translateVertical(Point point, int index, int count)
        {
            double x = point.X;
            double y = point.Y;
            int distance = (index + 1) / 2;
            int direction = (index % 2) * 2 - 1;
            y += yOffset * distance * direction;
            return new Point(x, y);
        }

        if (xOffset != 0 && yOffset != 0)
            return ProcessDistribution(queryValueGroups, translateSquare, zoom, tolerance, cancellationToken);

        if (yOffset == 0)
            return ProcessDistribution(queryValueGroups, translateHorizontal, zoom, tolerance, cancellationToken);

        if (xOffset == 0)
            return ProcessDistribution(queryValueGroups, translateVertical, zoom, tolerance, cancellationToken);

        throw new SyntaxErrorException("Square distribution either \"xOffset\" or \"yOffset\" needs to be specified.");

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
