//******************************************************************************************************
//  GrafanaDataSourceBase_AncillaryOperations.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/25/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GrafanaAdapters.DataSources;
using GrafanaAdapters.Functions;
using GrafanaAdapters.Model.Common;
using GSF;
using GSF.Drawing;
using GSF.Geo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ProcessQueryRequestDelegate = System.Func<
    GrafanaAdapters.GrafanaDataSourceBase,
    GrafanaAdapters.Model.Common.QueryRequest,
    System.Threading.CancellationToken,
    System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<GrafanaAdapters.Model.Common.TimeSeriesValues>>>;

namespace GrafanaAdapters;

// Non-query specific Grafana functionality is defined here
partial class GrafanaDataSourceBase
{
    private static ProcessQueryRequestDelegate[] s_processQueryRequestFunctions;
    private static object s_processQueryRequestFunctionsLock = new();
    private static Regex s_selectExpression;

    // Gets array of functions used to process query requests per data source value type, each value in the array
    // is at the same index as the data source value type in the 'DataSourceCache.LoadedTypes' array
    private static ProcessQueryRequestDelegate[] ProcessQueryRequestFunctions
    {
        get
        {
            ProcessQueryRequestDelegate[] processQueryRequestFunctions = Interlocked.CompareExchange(ref s_processQueryRequestFunctions, null, null);

            if (processQueryRequestFunctions is not null)
                return processQueryRequestFunctions;

            lock (s_processQueryRequestFunctionsLock)
            {
                // Check if another thread already created functions
                if (s_processQueryRequestFunctions is not null)
                    return s_processQueryRequestFunctions;

                s_processQueryRequestFunctions = CreateProcessQueryRequestFunctions();
            }

            return s_processQueryRequestFunctions;
        }
    }

    /// <summary>
    /// Reloads data source value types cache.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This function is used to support dynamic data source value type loading.
    /// Function would only need to be called when a new data source value is added
    /// to Grafana at run-time and user wanted to use new installed data source
    /// value type without restarting host.
    /// </para>
    /// <para>
    /// Suggest making this option available via web-based endpoint for administrators.
    /// </para>
    /// </remarks>
    public void ReloadDataSourceValueTypes()
    {
        DataSourceValueCache.ReloadDataSourceValueTypes();

        lock (s_processQueryRequestFunctionsLock)
            Interlocked.Exchange(ref s_processQueryRequestFunctions, null);
    }

    /// <summary>
    /// Reloads Grafana functions cache.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This function is used to support dynamic loading for Grafana functions.
    /// Function would only need to be called when a new function is added to Grafana at
    /// run-time and user wanted to use new installed function without restarting host.
    /// </para>
    /// <para>
    /// Suggest making this option available via web-based endpoint for administrators.
    /// </para>
    /// </remarks>
    public void ReloadGrafanaFunctions()
    {
        FunctionParsing.ReloadGrafanaFunctions();
    }

    private static ProcessQueryRequestDelegate[] CreateProcessQueryRequestFunctions()
    {
        // One process query request function will be defined per data source value type
        ProcessQueryRequestDelegate[] processQueryRequestFunctions = new ProcessQueryRequestDelegate[DataSourceValueCache.LoadedTypes.Count];

        foreach (Type type in DataSourceValueCache.LoadedTypes)
        {
            string typeName = type.Name;

            // Get generic definition for ProcessQueryRequestAsync method
            MethodInfo genericMethod = typeof(GrafanaDataSourceBase).GetMethod(nameof(ProcessQueryRequestAsync), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(type);

            DynamicMethod dynamicMethod = new($"{nameof(ProcessQueryRequestAsync)}_{typeName}",
                typeof(Task<IEnumerable<TimeSeriesValues>>),
                new[] { typeof(GrafanaDataSourceBase), typeof(QueryRequest), typeof(CancellationToken) },
                typeof(GrafanaDataSourceBase));

            ILGenerator generator = dynamicMethod.GetILGenerator();

            // Load the first argument (GrafanaDataSourceBase) onto the stack
            generator.Emit(OpCodes.Ldarg_0);

            // Load the second argument (QueryRequest) onto the stack
            generator.Emit(OpCodes.Ldarg_1);

            // Load the third argument (CancellationToken) onto the stack
            generator.Emit(OpCodes.Ldarg_2);

            // Call the generic method
            generator.Emit(OpCodes.Call, genericMethod);

            // Return the result of the call
            generator.Emit(OpCodes.Ret);

            ProcessQueryRequestDelegate function = (ProcessQueryRequestDelegate)dynamicMethod.CreateDelegate(typeof(ProcessQueryRequestDelegate), null);

            // Make sure function index matches data source value type index
            int index = DataSourceValueCache.GetTypeIndex(typeName);

            Debug.Assert(index > -1, $"Failed to find data source value type index for \"{typeName}\"");

            processQueryRequestFunctions[index] = function;
        }

        return processQueryRequestFunctions;
    }

    private static Task ProcessRadialDistributionAsync<T>(List<DataSourceValueGroup<T>> queryValueGroups, QueryParameters queryParameters, CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        Dictionary<string, string> settings = queryParameters.RadialDistribution.ParseKeyValuePairs();

        // Gets radius of overlapping coordinate distribution
        if (!settings.TryGetValue("radius", out string setting))
            throw new SyntaxErrorException("Radial distribution \"radius\" setting is missing.");

        if (!double.TryParse(setting, out double radius) || radius <= 0.0D)
            throw new SyntaxErrorException("Radial distribution \"radius\" setting is negative, zero or not a valid number.");

        // Get zoom level
        if (!settings.TryGetValue("zoom", out setting))
            throw new SyntaxErrorException("Radial distribution \"zoom\" setting is missing.");

        if (!double.TryParse(setting, out double zoom) || zoom <= 0.0D)
            throw new SyntaxErrorException("Radial distribution \"zoom\" setting is negative, zero or not a valid number.");

        // For this use case, we consider coordinates to be the "same" if they are within 100 feet.
        //
        // As long as coordinates are not near the poles, the following formula works to calculate
        // the degree of difference to use for a default coordinate tolerance of roughly 100 feet:
        //
        //   1 degree of latitude (or longitude) at equator ≈ 111 kilometers, and
        //   1 foot ≈ 0.0003048 kilometers, so 100 feet ≈ 0.03048 kilometers, which means
        //   the degree of difference = 0.03048 kilometers / 111 kilometers/degree, or
        //   for 100 feet, the degree of difference is about 0.000275 degrees

        // Get optional tolerance, or use default (see above)
        if (!settings.TryGetValue("tolerance", out setting) || !double.TryParse(setting, out double tolerance) || tolerance <= double.Epsilon)
            tolerance = 0.000275;

        // Get metadata maps that contain valid longitude and latitude coordinates and are sorted by them
        MetadataMap[] metadataMaps = queryValueGroups
            .Select(group => group.MetadataMap)
            .Where(coordinatesAreValid)
            .OrderBy(metadataMap => numericValueOf(metadataMap["Longitude"]))
            .ThenBy(metadataMap => numericValueOf(metadataMap["Latitude"]))
            .ToArray();

        // No work to do if no metadata maps contain valid longitude and latitude values
        if (metadataMaps.Length == 0)
            return Task.CompletedTask;

        return Task.Factory.StartNew(() =>
        {
            List<MetadataMap[]> groupedMaps = new();
            List<MetadataMap> matchingMaps = new() { metadataMaps[0] };
            MetadataMap firstGroupMap = metadataMaps[0];

            // Organize metadata maps with overlapped coordinates into groups, this code
            // assumes that metadata maps are already sorted by longitude and latitude:
            for (int i = 1; i < metadataMaps.Length; i++)
            {
                MetadataMap currentMap = metadataMaps[i];

                if (coordinatesMatch(firstGroupMap, currentMap))
                {
                    matchingMaps.Add(currentMap);
                }
                else
                {
                    if (matchingMaps.Count > 1)
                        groupedMaps.Add(matchingMaps.ToArray());

                    matchingMaps = new List<MetadataMap> { currentMap };
                    firstGroupMap = currentMap;
                }
            }

            if (matchingMaps.Count > 1)
                groupedMaps.Add(matchingMaps.ToArray());

            // Create radial distribution for overlapped coordinates, leaving one item at center
            EPSG3857 coordinateReference = new();

            foreach (MetadataMap[] maps in groupedMaps)
            {
                int count = maps.Length;
                double interval = 2.0D * Math.PI / (count - 1);

                // Skip first map since it is the center
                for (int i = 1; i < count; i++)
                {
                    MetadataMap map = maps[i];
                    Point point = coordinateReference.Translate(new GeoCoordinate(double.Parse(map["Latitude"]), double.Parse(map["Longitude"])), zoom);

                    double theta = interval * i;
                    double x = point.X + radius * Math.Cos(theta);
                    double y = point.Y + radius * Math.Sin(theta);

                    GeoCoordinate coordinate = coordinateReference.Translate(new Point(x, y), zoom);

                    map["Longitude"] = $"{coordinate.Longitude}";
                    map["Latitude"] = $"{coordinate.Latitude}";
                }
            }
        }, cancellationToken);

        static double numericValueOf(string mapValue)
        {
            double.TryParse(mapValue, out double value);
            return value;
        }

        static bool tryParseCoordinate(MetadataMap metadataMap, string coordinate, out double value)
        {
            value = default;
            return metadataMap.TryGetValue(coordinate, out string setting) && double.TryParse(setting, out value);
        }

        static bool tryParseLongitude(MetadataMap metadataMap, out double longitude)
        {
            return tryParseCoordinate(metadataMap, "Longitude", out longitude);
        }

        static bool tryParseLatitude(MetadataMap metadataMap, out double latitude)
        {
            return tryParseCoordinate(metadataMap, "Latitude", out latitude);
        }

        static bool coordinatesAreValid(MetadataMap metadataMap)
        {
            return tryParseLongitude(metadataMap, out _) && tryParseLatitude(metadataMap, out _);
        }

        bool coordinatesMatch(MetadataMap first, MetadataMap current)
        {
            if (!tryParseLongitude(first, out double longitude1) || !tryParseLatitude(first, out double latitude1))
                return false;

            if (!tryParseLongitude(current, out double longitude2) || !tryParseLatitude(current, out double latitude2))
                return false;

            return Math.Abs(longitude1 - longitude2) < tolerance && Math.Abs(latitude1 - latitude2) < tolerance;
        }
    }
}