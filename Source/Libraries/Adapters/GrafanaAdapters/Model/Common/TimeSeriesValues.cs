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
// ReSharper disable InconsistentNaming

using Newtonsoft.Json;
using System.Collections.Generic;

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
    public string refID;

    /// <summary>
    /// Gets or sets metadata attached to the <see cref="TimeSeriesValues"/>.
    /// </summary>
    public Dictionary<string, string> metadata;

    /// <summary>
    /// Gets or sets a flag that determines if empty series are produced -- non-serialized.
    /// </summary>
    [JsonIgnore] // Used internally for filtering empty series, not serialized
    public bool dropEmptySeries;

    /// <summary>
    /// Gets or sets a an error message that indicates a syntax error in the query request.
    /// </summary>
    public string syntaxError;

    /// <summary>
    /// Gets or sets a Grafana time-series value data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To ensure data will work with Grafana data source, all values should
    /// precede single time value. Time is always the last value in the array.
    /// Time value should be in Unix epoch milliseconds.
    /// </para>
    /// <para>
    /// JSON example:
    /// <code>
    /// "datapoints":[
    ///       [622,1450754160000],
    ///       [365,1450754220000]
    /// ]
    /// </code>
    /// </para>
    /// </remarks>
    public double[][] datapoints;


}