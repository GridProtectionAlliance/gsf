//******************************************************************************************************
//  DataSourceValue.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
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
//  02/18/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.TimeSeries;

namespace GrafanaAdapters.Model.Common;

/// <summary>
/// Represents an individual time-series value as queried from a data source.
/// </summary>
public struct DataSourceValue
{
    /// <summary>
    /// Point-tag and target ID for the query.
    /// </summary>
    /// <remarks>
    /// <c>pointTag</c> is a specific name for a data point to be queried
    /// from the data source. <c>target</c> is a more general name that
    /// can represent multiple point-tags, e.g., a tuple of point-tags.
    /// </remarks>
    public (string pointTag, string target) ID;

    /// <summary>
    /// Queried data source value.
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