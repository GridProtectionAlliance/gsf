//******************************************************************************************************
//  QueryRequest.cs - Gbtc
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

namespace GrafanaAdapters.Model.Common;

/// <summary>
/// Defines a Grafana query request.
/// </summary>
public class QueryRequest
{
    /// <summary>
    /// Gets or sets panel ID of request.
    /// </summary>
    public int panelId { get; set; }

    /// <summary>
    /// Gets or sets panel ID of request.
    /// </summary>
    public int dashboardId { get; set; }

    /// <summary>
    /// Gets or sets request range.
    /// </summary>
    public Range range { get; set; }

    /// <summary>
    /// Gets or sets relative request range.
    /// </summary>
    public RangeRaw rangeRaw { get; set; }

    /// <summary>
    /// Gets or sets request interval.
    /// </summary>
    public string interval { get; set; }

    /// <summary>
    /// Gets or sets request interval, in milliseconds.
    /// </summary>
    public string intervalMs { get; set; }

    /// <summary>
    /// Gets or sets request format (typically json).
    /// </summary>
    public string format { get; set; }

    /// <summary>
    /// Gets or sets maximum data points to return.
    /// </summary>
    public int maxDataPoints { get; set; }

    /// <summary>
    /// Gets or sets request targets.
    /// </summary>
    public List<Target> targets { get; set; }

    /// <summary>
    /// Gets or sets ad-hoc filters to apply.
    /// </summary>
    public List<AdHocFilter> adhocFilters { get; set; }

    /// <summary>
    /// Gets or sets if request is in phasor mode.
    /// </summary>
    // TODO: JRC - suggest renaming this to 'dataType' and making it a string value, e.g., "PhasorValue" or "DataSourceValue", to allow for future expansion
    public bool isPhasor { get; set; }
}