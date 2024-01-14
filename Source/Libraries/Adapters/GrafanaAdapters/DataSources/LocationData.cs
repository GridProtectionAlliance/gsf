//******************************************************************************************************
//  LocationData.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/05/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GrafanaAdapters.DataSources.BuiltIn;
using GrafanaAdapters.Model.Common;
using GSF.Drawing;
using GSF.Geo;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrafanaAdapters.DataSources;

// TODO: JRC - class is currently not referenced: think about where to locate/use "radial distribution for overlapped coordinates" code

/// <summary>
/// Defines location meta-data functions for Grafana controllers.
/// </summary>
public sealed class LocationData
{
    /// <summary>
    /// Gets or sets associated <see cref="GrafanaDataSourceBase"/> instance.
    /// </summary>
    public GrafanaDataSourceBase DataSource { get; set; }

    /// <summary>
    /// Queries Grafana data source for location data offsetting duplicate coordinates using a radial distribution.
    /// </summary>
    /// <param name="radius">Radius of overlapping coordinate distribution.</param>
    /// <param name="zoom">Zoom level.</param>
    /// <param name="request"> Query request.</param>
    /// <param name="cancellationToken">Propagates notification from client that operations should be canceled.</param>
    /// <returns>JSON serialized location metadata for specified targets.</returns>
    public Task<string> GetLocationData(double radius, double zoom, List<Target> request, CancellationToken cancellationToken)
    {
        if (double.IsNaN(radius) || radius <= 0.0D)
            return GetLocationData(request, cancellationToken);

        return Task.Factory.StartNew(() =>
            {
                // Get location data, sorted by longitude and latitude
                DataTable targetMeasurements = GetLocationDataTable(request, true);

                if (targetMeasurements.Rows.Count > 0)
                {
                    int longitude = targetMeasurements.Columns["Longitude"].Ordinal;
                    int latitude = targetMeasurements.Columns["Latitude"].Ordinal;

                    bool coordinateIsValid(DataRow row, int column)
                    {
                        return row[column] is decimal;
                    }

                    bool coordinatesAreValid(DataRow row)
                    {
                        return coordinateIsValid(row, longitude) && coordinateIsValid(row, latitude);
                    }

                    bool coordinateMatches(DataRow left, DataRow right, int column)
                    {
                        return left[column] is decimal leftValue && right[column] is decimal rightValue &&
                               leftValue.Equals(rightValue);
                    }

                    bool coordinatesMatch(DataRow first, DataRow current)
                    {
                        return coordinateMatches(first, current, longitude) &&
                               coordinateMatches(first, current, latitude);
                    }

                    List<DataRow[]> groupedRows = new();
                    List<DataRow> matchingRows = new() { targetMeasurements.Rows[0] };
                    DataRow firstGroupRow = matchingRows.First();
                    bool firstGroupRowValid = coordinatesAreValid(firstGroupRow);

                    // Organize metadata rows with overlapped coordinates into groups
                    for (int i = 1; i < targetMeasurements.Rows.Count; i++)
                    {
                        DataRow row = targetMeasurements.Rows[i];

                        if (firstGroupRowValid && coordinatesMatch(firstGroupRow, row))
                        {
                            matchingRows.Add(row);
                        }
                        else
                        {
                            if (matchingRows.Count > 1)
                                groupedRows.Add(matchingRows.ToArray());

                            matchingRows = new List<DataRow> { row };
                            firstGroupRow = matchingRows.First();
                            firstGroupRowValid = coordinatesAreValid(firstGroupRow);
                        }
                    }

                    if (matchingRows.Count > 1)
                        groupedRows.Add(matchingRows.ToArray());

                    // Create radial distribution for overlapped coordinates, leaving one item at center
                    EPSG3857 coordinateReference = new();

                    foreach (DataRow[] rows in groupedRows)
                    {
                        int count = rows.Length;
                        double interval = 2.0D * Math.PI / (count - 1);

                        for (int i = 1; i < count; i++)
                        {
                            DataRow row = rows[i];
                            Point point = coordinateReference.Translate(new GeoCoordinate((double)row.Field<decimal>(latitude), (double)row.Field<decimal>(longitude)), zoom);

                            double theta = interval * i;
                            double x = point.X + radius * Math.Cos(theta);
                            double y = point.Y + radius * Math.Sin(theta);

                            GeoCoordinate coordinate = coordinateReference.Translate(new Point(x, y), zoom);

                            row[longitude] = (decimal)coordinate.Longitude;
                            row[latitude] = (decimal)coordinate.Latitude;
                        }
                    }
                }

                return JsonConvert.SerializeObject(targetMeasurements);
            },
            cancellationToken);
    }

    /// <summary>
    /// Queries Grafana data source for location data.
    /// </summary>
    /// <param name="request"> Query request.</param>
    /// <param name="cancellationToken">Propagates notification from client that operations should be canceled.</param>
    /// <returns>JSON serialized location metadata for specified targets.</returns>
    public Task<string> GetLocationData(List<Target> request, CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(() => JsonConvert.SerializeObject(GetLocationDataTable(request, false)), cancellationToken);
    }

    private DataTable GetLocationDataTable(List<Target> request, bool orderByCoordinates)
    {
        DataTable activeMeasurements = DataSource?.Metadata?.Tables[DataSourceValue.MetadataTableName];

        if (activeMeasurements is null)
            return new DataTable();

        // Create a hash set of desired targets for quick contains-based lookup
        IEnumerable<string> targets = request.Select(target => target.target).Where(value => !string.IsNullOrEmpty(value));
        HashSet<string> pointTags = new(targets, StringComparer.OrdinalIgnoreCase);

        DataTable targetMeasurements = new("LocationMetadata");

        // Reduce metadata to return only needed fields
        targetMeasurements.Columns.Add(new DataColumn("PointTag", typeof(string)));
        targetMeasurements.Columns.Add(new DataColumn("Device", typeof(string)));
        targetMeasurements.Columns.Add(new DataColumn("DeviceID", typeof(int)));
        targetMeasurements.Columns.Add(new DataColumn("Longitude", typeof(decimal)));
        targetMeasurements.Columns.Add(new DataColumn("Latitude", typeof(decimal)));

        Dictionary<int, int> columnMap = new();

        // Map ordinal indexes of target measurement columns to those in active measurements
        foreach (DataColumn targetColumn in targetMeasurements.Columns)
        {
            DataColumn sourceColumn = activeMeasurements.Columns[targetColumn.ColumnName];
            columnMap.Add(targetColumn.Ordinal, sourceColumn.Ordinal);
        }

        ConcurrentBag<DataRow> matchingRows = new();

        foreach (DataRow row in activeMeasurements.AsEnumerable())
        {
            if (!pointTags.Contains(row["PointTag"].ToString()))
                continue;

            DataRow newRow = targetMeasurements.NewRow();

            for (int x = 0; x < targetMeasurements.Columns.Count; x++)
                newRow[x] = row[columnMap[x]];

            matchingRows.Add(newRow);
        }

        if (orderByCoordinates)
        {
            int longitude = targetMeasurements.Columns["Longitude"].Ordinal;
            int latitude = targetMeasurements.Columns["Latitude"].Ordinal;

            decimal getCoordinate(DataRow row, int column)
            {
                return row[column] is decimal coordinate ? coordinate : 0.0M;
            }

            decimal getLongitude(DataRow row)
            {
                return getCoordinate(row, longitude);
            }

            decimal getLatitude(DataRow row)
            {
                return getCoordinate(row, latitude);
            }

            foreach (DataRow row in matchingRows.OrderBy(getLongitude).ThenBy(getLatitude))
                targetMeasurements.Rows.Add(row);
        }
        else
        {
            foreach (DataRow row in matchingRows)
                targetMeasurements.Rows.Add(row);
        }

        return targetMeasurements;
    }
}