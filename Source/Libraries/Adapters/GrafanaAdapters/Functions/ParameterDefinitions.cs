//******************************************************************************************************
//  ParameterDefinitions.cs - Gbtc
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
//  12/31/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GrafanaAdapters.DataSourceValueTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Represents a readonly collection of <see cref="IParameter"/> definitions.
/// </summary>
public class ParameterDefinitions : IReadOnlyList<IParameter>
{
    private readonly List<IParameter> m_parameters;
    private readonly Dictionary<string, IParameter> m_parameterNameMap;

    /// <summary>
    /// Creates a new <see cref="ParameterDefinitions"/> instance.
    /// </summary>
    public ParameterDefinitions() : this(Array.Empty<IParameter>())
    {
    }

    /// <summary>
    /// Creates a new <see cref="ParameterDefinitions"/> instance.
    /// </summary>
    /// <param name="parameters">Parameters to include in the definitions.</param>
    /// <exception cref="ArgumentException">
    /// Parameter is not a definition type -- 'IsDefinition' property must be true -or-
    /// Parameter has no defined name -- 'Name' property cannot be null, empty or whitespace -or-
    /// Parameter name is not unique -- parameter with the same name is already defined.
    /// </exception>
    public ParameterDefinitions(IEnumerable<IParameter> parameters)
    {
        m_parameters = [];
        m_parameterNameMap = new Dictionary<string, IParameter>(StringComparer.OrdinalIgnoreCase);

        foreach (IParameter parameter in parameters)
            Add(parameter);

        // Data source parameter is always the last parameter
        Add(ParameterDefinitions<IDataSourceValueType>.DataSourceValues);
    }

    private void Add(IParameter parameter)
    {
        if (!parameter.IsDefinition)
            throw new ArgumentException($"Non-definition parameter: cannot add parameter \"{parameter.Name}\" to parameter definitions since it is not a definition type -- 'IsDefinition' property must be true.");

        if (string.IsNullOrWhiteSpace(parameter.Name))
            throw new ArgumentException($"Empty parameter name: cannot add parameter of type \"{parameter.Type.Name}\" to parameter definitions since it has no defined name.");

        if (m_parameterNameMap.ContainsKey(parameter.Name))
            throw new ArgumentException($"Duplicate parameter name: cannot add parameter \"{parameter.Name}\" to parameter definitions since a parameter with the same name is already defined.");

        m_parameters.Add(parameter);
        m_parameterNameMap.Add(parameter.Name, parameter);
    }

    /// <summary>
    /// Gets the parameter at the specified index.</summary>
    /// <param name="index">The zero-based index of the parameter to get.</param>
    /// <returns>The parameter at the specified index.</returns>
    public IParameter this[int index] => m_parameters[index];

    /// <summary>
    /// Gets the parameter with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the parameter to get.</param>
    /// <returns>The parameter with the specified <paramref name="name"/>.</returns>
    public IParameter this[string name] => m_parameterNameMap[name];

    /// <inheritdoc />
    public int Count => m_parameters.Count;

    /// <inheritdoc />
    public IEnumerator<IParameter> GetEnumerator() => m_parameters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Searches for the parameter with the specified <paramref name="name"/> and returns its zero-based index.
    /// </summary>
    /// <param name="name">The name of the parameter to locate in the <see cref="ParameterDefinitions" />.</param>
    /// <returns>
    /// The zero-based index of the parameter with the specified <paramref name="name" />, if found; otherwise, -1.
    /// </returns>
    public int IndexOf(string name)
    {
        return m_parameters.FindIndex(parameter => parameter.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether the <see cref="ParameterDefinitions" /> contains a parameter with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the parameter to locate in the <see cref="ParameterDefinitions" />.</param>
    /// <returns>
    /// <c>true</c> if the <see cref="ParameterDefinitions" /> contains a parameter with the specified name; otherwise, <c>false</c>.
    /// </returns>
    public bool Contains(string name)
    {
        return m_parameterNameMap.ContainsKey(name);
    }

    /// <summary>
    /// Copies the entire <see cref="ParameterDefinitions" /> to a compatible one-dimensional array, starting at the specified index of the target array.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="Array" /> that is the destination of the elements copied from <see cref="ParameterDefinitions" />. Array must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    public void CopyTo(IParameter[] array, int arrayIndex)
    {
        m_parameters.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Implicitly converts a <see cref="List{IParmeter}"/> to a <see cref="ParameterDefinitions"/>.
    /// </summary>
    /// <param name="parameters">List of parameters to convert.</param>
    /// <returns>New <see cref="ParameterDefinitions"/> instance built from specified <paramref name="parameters"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Parameter is not a definition type -- 'IsDefinition' property must be true -or-
    /// Parameter has no defined name -- 'Name' property cannot be null, empty or whitespace -or-
    /// Parameter name is not unique -- parameter with the same name is already defined.
    /// </exception>
    public static implicit operator ParameterDefinitions(List<IParameter> parameters)
    {
        return new ParameterDefinitions(parameters);
    }

    /// <summary>
    /// Creates a set of mutable parameters from the parameter definitions.
    /// </summary>
    /// <returns>New set of mutable parameters based on the parameter definitions.</returns>
    public Parameters CreateParameters()
    {
        return new Parameters(this.Select(parameterDefinition => parameterDefinition.CreateParameter()));
    }

    // Gets parameter definitions with inserted slice parameter
    internal IReadOnlyList<IParameter> WithRequiredSliceParameter =>
        new List<IParameter>(this).InsertRequiredSliceParameter();
}

internal static class ParameterDefinitions<T>
{
    // This generates a standard data source values parameter definition - this is always the last parameter
    public static ParameterDefinition<IAsyncEnumerable<T>> DataSourceValues = new()
    {
        Name = "expression",
        Default = AsyncEnumerable.Empty<T>(),
        Description = "Target expression that produces a series of values representing input data for the function.",
        Required = true
    };
}