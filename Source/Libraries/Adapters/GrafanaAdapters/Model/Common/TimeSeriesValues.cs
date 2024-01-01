//******************************************************************************************************
//  TimeSeriesValues.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  09/12/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using Newtonsoft.Json;

namespace GrafanaAdapters.Model.Common;

/// <summary>
/// Defines a Grafana time-series values.
/// </summary>
/// <remarks>
/// This structure is serialized and returned to Grafana via JSON.
/// </remarks>
public class TimeSeriesValues
{
    /// <summary>
    /// Data point index for value.
    /// </summary>
    public const int Value = 0;

    /// <summary>
    /// Data point index for time.
    /// </summary>
    public const int Time = 1;

    /// <summary>
    /// Gets or sets a Grafana time-series value point source.
    /// </summary>
    public string target;

    /// <summary>
    /// Gets or sets a Grafana time-series underlying point tag.
    /// </summary>
    public string rootTarget;

    /// <summary>
    /// Gets or sets a Grafana time-series refId to reference a specific query.
    /// </summary>
    public string refId;

    /// <summary>
    /// Gets or sets metadata attached to the <see cref="TimeSeriesValues"/>.
    /// </summary>
    public Dictionary<string, string> meta;

    /// <summary>
    /// Gets or sets a flag that determines if empty series are produced -- non-serialized.
    /// </summary>
    [JsonIgnore]
    public bool dropEmptySeries;

    /// <summary>
    /// Gets or sets a Grafana time-series value data.
    /// </summary>
    /// <remarks>
    /// "datapoints":[
    ///       [622,1450754160000],
    ///       [365,1450754220000]
    /// ]
    /// </remarks>
    public List<double[]> datapoints;
}

// TODO: JRC - these classes are no longer used, I think they should be removed

///// <summary>
///// This represents the MetaData Attached to a <see cref="TimeSeriesValues"/>
///// </summary>
///// <remarks>
///// This structure is serialized and returned to Grafana via JSON.
///// It needs to match QueryResultMeta in Grafana to avoid Grafana removing any MetaData
///// </remarks>
//public class MetaData
//{
//    /// <summary>
//    /// Defines custom historian metadata.
//    /// </summary>
//    public HistorianMetaData custom;
//}

///// <summary>
///// Represents a custom MetaData Record created by the openHistorian
///// </summary>
//public class HistorianMetaData
//{
//    /// <summary>
//    /// The Longitude of the original Signal
//    /// </summary>
//    public double Longitude;

//    /// <summary>
//    /// The Latitude of the original Signal
//    /// </summary>
//    public double Latitude;

//    /// <summary>
//    /// The name of the PMU
//    /// </summary>
//    public string Device;
//}