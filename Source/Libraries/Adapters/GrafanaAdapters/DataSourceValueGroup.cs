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
/// Defines a class that represents an enumeration of T for a given target.
/// </summary>
/// <remarks>
/// This is a group construct keyed on <see cref="Target"/> for data source value enumerations.
/// </remarks>
public class DataSourceValueGroup<T> : IDataSourceValueGroup where T : IDataSourceValue
{
    /// <inheritdoc />
    public string Target { get; set; }

    /// <inheritdoc />
    public string RootTarget { get; set; }

    /// <inheritdoc />
    public Target SourceTarget { get; set; }

    /// <summary>
    /// Gets or sets data source value enumeration for the group.
    /// </summary>
    public IEnumerable<T> Source { get; set; }

    IEnumerable<IDataSourceValue> IDataSourceValueGroup.Source
    {
        get => Source as IEnumerable<IDataSourceValue>;
        set => Source = value as IEnumerable<T>;
    }

    /// <inheritdoc />
    public bool DropEmptySeries { get; set; }

    /// <inheritdoc />
    public string refId { get; set; }

    /// <inheritdoc />
    public Dictionary<string, string> metadata { get; set; }

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
            refId = refId,
            metadata = metadata
        };
    }
}