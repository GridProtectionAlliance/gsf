//******************************************************************************************************
//  DataSourceValue.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  02/13/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.TimeSeries;
using System;
using System.Collections.Generic;
using System.Data;

namespace GrafanaAdapters;

/// <summary>
/// Represents an individual time-series value from a data source.
/// </summary>
public struct DataSourceValue : IDataSourceValue
{
    /// <summary>
    /// Query target, e.g., a point-tag.
    /// </summary>
    public string Target;

    /// <summary>
    /// Queried value.
    /// </summary>
    public double Value;

    /// <summary>
    /// Timestamp, in Unix epoch milliseconds, of queried value.
    /// </summary>
    public double Time;

    /// <summary>
    /// Flags for queried value.
    /// </summary>
    public MeasurementStateFlags Flags;

    /// <inheritdoc />
    readonly double IDataSourceValue.Time => Time;

    /// <inheritdoc />
    readonly double[] IDataSourceValue.TimeSeriesValue => new[] { Value, Time };

    /// <inheritdoc />
    readonly MeasurementStateFlags IDataSourceValue.Flags => Flags;

    DataRow[] IDataSourceValue.LookupMetadata(DataSet metadata, string target)
    {
        return metadata?.Tables["ActiveMeasurements"].Select($"PointTag = '{target}'") ?? Array.Empty<DataRow>();
    }
}

/// <summary>
/// Helper class to compare two data sources
/// </summary>
public class DataSourceValueComparer : IComparer<DataSourceValue>
{
    /// <summary>
    /// Compare function
    /// </summary>
    public int Compare(DataSourceValue x, DataSourceValue y)
    {
        int result = x.Value.CompareTo(y.Value);
        return result != 0 ? result : x.Time.CompareTo(y.Time);
    }
}