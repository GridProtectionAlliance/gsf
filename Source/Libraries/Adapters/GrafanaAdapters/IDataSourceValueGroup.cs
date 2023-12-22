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

namespace GrafanaAdapters;

/// <summary>
/// Defines a interface that represents an enumeration of T for a given target.
/// </summary>
/// <remarks>
/// This is a group construct keyed on <see cref="Target"/> for data source value enumerations.
/// </remarks>
public interface IDataSourceValueGroup
{
    /// <summary>
    /// Gets or sets target, e.g., a point-tag, representative of all <see cref="Source"/> values.
    /// </summary>
    string Target { get; set; }

    /// <summary>
    /// Gets or sets the root target expression, without any referenced series functions.
    /// </summary>
    string RootTarget { get; set; }

    /// <summary>
    /// Gets or sets a reference to the original target that was the source of these results.
    /// </summary>
    Target SourceTarget { get; set; }

    /// <summary>
    /// Gets or sets data source values enumerable.
    /// </summary>
    public IEnumerable<IDataSourceValue> Source { get; set; }

    /// <summary>
    /// Gets or sets flag that determines if empty series are produced.
    /// </summary>
    bool DropEmptySeries { get; set; }

    /// <summary>
    /// Gets or sets a refID for a specific Grafana Query.
    /// </summary>
    string refId { get; set; }

    /// <summary>
    /// Gets or sets a refID for a specific Grafana Query.
    /// </summary>
    Dictionary<string, string> metadata { get; set; }
}