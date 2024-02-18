//******************************************************************************************************
//  MeasurementValue.cs - Gbtc
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

namespace GrafanaAdapters.DataSourceValueTypes.BuiltIn;

/// <summary>
/// Represents a default target for a standard time-series value.
/// </summary>
/// <remarks>
/// This is the standard data source value structure for common time-series
/// values used by Grafana.
/// </remarks>
public partial struct MeasurementValue
{
    /// <summary>
    /// Defines the primary metadata table name for a <see cref="MeasurementValue"/>.
    /// </summary>
    public const string MetadataTableName = "ActiveMeasurements";

    /// <summary>
    /// Query target, e.g., a point-tag.
    /// </summary>
    public string Target;

    /// <summary>
    /// Queried measurement value.
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