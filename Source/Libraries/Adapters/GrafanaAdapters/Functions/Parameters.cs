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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Represents a collection of mutable parameters.
/// </summary>
/// <remarks>
/// <para>
/// New instances of this class should be created by using the <see cref="ParameterDefinitions.CreateParameters"/> method.
/// </para>
/// <para>
/// This collection holds a distinct set of parameters generated for each function call, ensuring thread-safe operation
/// when multiple threads execute the same function simultaneously. The <see cref="ParameterDefinitions"/> class outlines
/// all possible parameters for a function, both required and optional, effectively determining its signature. Parameters
/// in this collection represent the values extracted from the user-provided function expression and, when presented to a
/// function, have already been validated and parsed by type. Additionally, the class provides access to the data source
/// values expression; function implementations can call <see cref="GrafanaFunctionBase{T}.GetDataSourceValues"/> to get
/// current data source values. The parameters in this collection are mutable, implying ownership by the function, and
/// can be safely modified as needed. To identify which optional parameters have been parsed and are available, refer to
/// the <see cref="ParsedCount"/> property.
/// </para>
/// </remarks>
public class Parameters : IList<IMutableParameter>
{
    private readonly List<IMutableParameter> m_parameters;
    private Dictionary<string, IMutableParameter> m_parameterNameMap;

    internal Parameters(IEnumerable<IMutableParameter> parameters)
    {
        m_parameters = [..parameters];
    }

    private Dictionary<string, IMutableParameter> ParameterNameMap =>
        m_parameterNameMap ??= this.ToDictionary(parameter => parameter.Name, StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public int Count => m_parameters.Count;

    /// <summary>
    /// Gets or sets the number of parameters that have been parsed.
    /// </summary>
    /// <remarks>
    /// The number of parameters in defined in the <see cref="Parameters"/> collection will always match the number of
    /// parameters defined in the function definition, optional or not, see <see cref="ParameterDefinitions"/>. This
    /// property is used to determine the count of required and optional parameters that were actually parsed from the
    /// user provided function expression. Note that the count does not include the data source values expression,
    /// which is always available as the last parameter. With this count, the function can determine which optional
    /// parameters were parsed and are thus available for use.
    /// </remarks>
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
    /// Gets or sets the parameter with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the parameter to get.</param>
    /// <returns>The parameter with the specified <paramref name="name"/>.</returns>
    /// <remarks>
    /// Parameter name lookup dictionary is lazy initialized. Using index-based lookups is more efficient.
    /// </remarks>
    /// <exception cref="KeyNotFoundException">Parameter name not found.</exception>
    public IMutableParameter this[string name]
    {
        get => ParameterNameMap[name];
        set => ParameterNameMap[name] = value;
    }

    /// <summary>
    /// Gets or sets collection of target user selected metadata associated with the data source values.
    /// </summary>
    /// <remarks>
    /// This property provides access to the set of outgoing metadata for each query result so that a
    /// function can adjust or augment the metadata sets if needed.
    /// </remarks>
    public Dictionary<string, MetadataMap> MetadataMaps { get; set; }

    /// <summary>
    /// Gets value of parameter at specified index, if the index is valid.
    /// </summary>
    /// <param name="index">Index of parameter to get.</param>
    /// <returns>
    /// The value of parameter at specified index if the index is valid;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This function will not throw an exception if the index is invalid.
    /// </remarks>
    public object Value(int index)
    {
        return index > -1 && index < m_parameters.Count ?
            m_parameters[index].Value : null;
    }

    /// <summary>
    /// Gets typed value of parameter at specified index, if the index is valid and the
    /// value can be cast as type.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="index">Index of parameter to get.</param>
    /// <returns>
    /// The typed value of parameter at specified index if the index is valid and the
    /// value can be cast to type; otherwise, default value.
    /// </returns>
    /// <remarks>
    /// This function will not throw an exception if the index is invalid.
    /// </remarks>
    public T Value<T>(int index)
    {
        return index > -1 && index < m_parameters.Count && m_parameters[index] is IMutableParameter<T> typedParameter ?
            typedParameter.Value : default;
    }

    /// <summary>
    /// Gets value of parameter with specified name, if name is found.
    /// </summary>
    /// <param name="name">Name of parameter to get.</param>
    /// <returns>
    /// The value of parameter with the specified name if the parameter name can be found;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// Parameter name lookup dictionary is lazy initialized. Using index-based lookups is more efficient.
    /// This function will not throw an exception if the name is not found.
    /// </remarks>
    public object Value(string name)
    {
        return ParameterNameMap.TryGetValue(name, out IMutableParameter parameter) ?
            parameter.Value : null;
    }

    /// <summary>
    /// Gets typed value of parameter with specified name, if name is found and the
    /// value can be cast as type.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="name">Name of parameter to get.</param>
    /// <returns>
    /// The typed value of parameter with the specified name if the parameter name can be found
    /// and the value can be cast to specified type; otherwise, default value.
    /// </returns>
    /// <remarks>
    /// Parameter name lookup dictionary is lazy initialized. Using index-based lookups is more efficient.
    /// This function will not throw an exception if the name is not found.
    /// </remarks>
    public T Value<T>(string name)
    {
        return ParameterNameMap.TryGetValue(name, out IMutableParameter parameter) && parameter is IMutableParameter<T> typedParameter ?
            typedParameter.Value : default;
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