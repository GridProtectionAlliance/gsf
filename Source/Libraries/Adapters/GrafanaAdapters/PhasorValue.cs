//******************************************************************************************************
//  PhasorValue.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  08/23/2023 - Timothy Liakh
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.TimeSeries;
using System;
using System.Collections.Generic;
using System.Data;

namespace GrafanaAdapters;

/// <summary>
/// Represents an individual time-series phasor value from a data source.
/// </summary>
public struct PhasorValue : IDataSourceValue
{
    /// <summary>
    /// Query magnitude target, e.g., a point-tag.
    /// </summary>
    public string MagnitudeTarget;

    /// <summary>
    /// Query angle target, e.g., a point-tag.
    /// </summary>
    public string AngleTarget;

    /// <summary>
    /// Queried magnitude value.
    /// </summary>
    public double Magnitude;

    /// <summary>
    /// Queried angle value.
    /// </summary>
    public double Angle;

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
    readonly double[] IDataSourceValue.TimeSeriesValue => new[] { Magnitude, Angle, Time };

    /// <inheritdoc />
    readonly MeasurementStateFlags IDataSourceValue.Flags => Flags;

    DataRow[] IDataSourceValue.LookupMetadata(DataSet metadata, string target)
    {
        return metadata?.Tables["Phasor"].Select($"Label = '{target}'") ?? Array.Empty<DataRow>();
    }
}

/// <summary>
/// Helper class to compare two phasors.
/// </summary>
public class PhasorValueComparer : IComparer<PhasorValue>
{
    /// <summary>
    /// Compare function
    /// </summary>
    public int Compare(PhasorValue x, PhasorValue y)
    {
        int result = x.Magnitude.CompareTo(y.Magnitude);

        if (result != 0)
            return result;

        result = x.Angle.CompareTo(y.Angle);

        if (result != 0)
            return result;

        return x.Time.CompareTo(y.Time);
    }
}