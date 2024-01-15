//******************************************************************************************************
//  QueryParameters.cs - Gbtc
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

using GrafanaAdapters.Model.Common;
using System;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Represents the parameters of a Grafana query.
/// </summary>
public class QueryParameters
{
    /// <summary>
    /// Gets or sets a reference to the original target that was the source of these results.
    /// </summary>
    public Target SourceTarget { get; set; }

    /// <summary>
    /// Gets or sets start time of the query.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets stop time of the query.
    /// </summary>
    public DateTime StopTime { get; set; }

    /// <summary>
    /// Gets or sets the interval of the query.
    /// </summary>
    public string Interval { get; set; }

    /// <summary>
    /// Gets or sets a flag that indicates whether to include the peaks of the query.
    /// </summary>
    public bool IncludePeaks { get; set; }

    /// <summary>
    /// Gets or sets a flag that indicates whether to include empty series in the query.
    /// </summary>
    public bool DropEmptySeries { get; set; }

    /// <summary>
    /// Gets or sets any defined eval imports defined in the query.
    /// </summary>
    public string Imports { get; set; }

    /// <summary>
    /// Gets or sets any defined radial distribution request parameters defined in the query.
    /// </summary>
    public string RadialDistribution { get; set; }

    /// <summary>
    /// Gets or sets metadata selections for the query.
    /// </summary>
    public (string tableName, string[] fieldNames)[] MetadataSelections { get; set; }
}