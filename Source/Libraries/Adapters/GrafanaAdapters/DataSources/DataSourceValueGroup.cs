//******************************************************************************************************
//  DataSourceValueGroup.cs - Gbtc
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
//  02/14/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using GrafanaAdapters.Model.Common;

namespace GrafanaAdapters.DataSources;

/// <summary>
/// Defines a class that represents an enumeration of T for a given target.
/// </summary>
/// <remarks>
/// This is a group construct keyed on <see cref="Target"/> for data source value enumerations.
/// </remarks>
public class DataSourceValueGroup<T> where T : struct, IDataSourceValue
{
    /// <summary>
    /// Gets or sets target, e.g., a point-tag, representative of all <see cref="Source"/> values.
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// Gets or sets the root target expression, without any referenced series functions.
    /// </summary>
    public string RootTarget { get; set; }

    /// <summary>
    /// Gets or sets a reference to the original target that was the source of these results.
    /// </summary>
    public Target SourceTarget { get; init; }

    /// <summary>
    /// Gets or sets data source values enumerable.
    /// </summary>
    public IAsyncEnumerable<T> Source { get; set; }

    /// <summary>
    /// Gets or sets flag that determines if empty series are produced.
    /// </summary>
    public bool DropEmptySeries { get; init; }

    /// <summary>
    /// Gets or sets a refID for a specific Grafana Query.
    /// </summary>
    public string RefID { get; init; }

    /// <summary>
    /// TODO: Update this
    /// </summary>
    public Dictionary<string, string> MetadataMap { get; init; }

    /// <summary>
    /// Creates a new <see cref="DataSourceValueGroup{T}"/> from this instance.
    /// </summary>
    /// <returns></returns>
    public DataSourceValueGroup<T> Clone()
    {
        return new DataSourceValueGroup<T>
        {
            Target = Target,
            RootTarget = RootTarget,
            SourceTarget = SourceTarget,
            Source = Source,
            DropEmptySeries = DropEmptySeries,
            RefID = RefID,
            MetadataMap = MetadataMap
        };
    }
}