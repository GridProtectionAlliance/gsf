//******************************************************************************************************
//  Parameters.cs - Gbtc
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
//  01/02/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections;
using System.Collections.Generic;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Represents a collection of mutable parameters.
/// </summary>
public class Parameters : IList<IMutableParameter>
{
    private readonly List<IMutableParameter> m_parameters;

    /// <summary>
    /// Creates a new <see cref="Parameters"/> instance.
    /// </summary>
    public Parameters()
    {
        m_parameters = new List<IMutableParameter>();
    }

    /// <summary>
    /// Creates a new <see cref="Parameters"/> instance.
    /// </summary>
    /// <param name="parameters">Parameters to add to the collection.</param>
    public Parameters(IEnumerable<IMutableParameter> parameters)
    {
        m_parameters = new List<IMutableParameter>(parameters);
    }

    /// <inheritdoc />
    public int Count => m_parameters.Count;

    /// <summary>
    /// Gets or sets the number of parameters that have been parsed.
    /// </summary>
    public int ParsedCount { get; set; }

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public IMutableParameter this[int index]
    {
        get => m_parameters[index]; 
        set => m_parameters[index] = value;
    }

    /// <summary>
    /// Gets typed value of parameter at specified index.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="index">Index of parameter to get.</param>
    /// <returns>The typed value of parameter at specified index if can be cast to type; otherwise, default value.</returns>
    public T Value<T>(int index)
    {
        return m_parameters[index] is IMutableParameter<T> typedParameter ? 
            typedParameter.Value : 
            default;
    }

    /// <inheritdoc />
    public void Add(IMutableParameter item)
    {
        m_parameters.Add(item);
    }

    /// <inheritdoc />
    public void Clear()
    {
        m_parameters.Clear();
    }

    /// <inheritdoc />
    public bool Contains(IMutableParameter item)
    {
        return m_parameters.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(IMutableParameter[] array, int arrayIndex)
    {
        m_parameters.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public IEnumerator<IMutableParameter> GetEnumerator()
    {
        return m_parameters.GetEnumerator();
    }

    /// <inheritdoc />
    public int IndexOf(IMutableParameter item)
    {
        return m_parameters.IndexOf(item);
    }

    /// <inheritdoc />
    public void Insert(int index, IMutableParameter item)
    {
        m_parameters.Insert(index, item);
    }

    /// <inheritdoc />
    public bool Remove(IMutableParameter item)
    {
        return m_parameters.Remove(item);
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        m_parameters.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_parameters.GetEnumerator();
    }
}