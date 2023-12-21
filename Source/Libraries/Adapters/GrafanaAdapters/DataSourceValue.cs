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
using System.Collections.Generic;

namespace GrafanaAdapters;

/// <summary>
/// Defines a structure that represents an individual time-series value from a data source.
/// </summary>
public struct DataSourceValue
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

        if (result != 0)
            return result;

        return x.Time.CompareTo(y.Time);
    }
}