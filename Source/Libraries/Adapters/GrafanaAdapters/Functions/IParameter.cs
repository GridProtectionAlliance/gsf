//******************************************************************************************************
//  IParameter.cs - Gbtc
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

using System;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Represents a parameter for Grafana functions.
/// </summary>
public interface IParameter
{
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the parameter.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the type of the parameter.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Gets flag that indicates if parameter is a definition.
    /// </summary>
    /// <remarks>
    /// Definition parameters are used to define function parameters,
    /// not hold values for function evaluation.
    /// </remarks>
    bool IsDefinition { get; }

    /// <summary>
    /// Gets flag that indicates if the parameter is required.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Required parameters (i.e., Required = <c>true</c>) must precede
    /// optional parameters (i.e., Required = <c>false</c>) in the
    /// parameter list.
    /// </para>
    /// <para>
    /// Note that the data source values parameter, i.e., the 'expression', is technically
    /// a required parameter but always exists as the last parameter after any defined
    /// optional or internal parameters. This parameter is automatically added to the
    /// parameter list by the <see cref="ParameterDefinitions"/> class.
    /// </para>
    /// </remarks>
    bool Required { get; }

    /// <summary>
    /// Gets flag that indicates if parameter is internal.
    /// </summary>
    /// <remarks>
    /// Internal parameters are not exposed to the user and should
    /// always be defined at the end of the parameter list.
    /// </remarks>
    public bool Internal { get; }

    /// <summary>
    /// Gets default value of the parameter.
    /// </summary>
    object Default { get; }

    /// <summary>
    /// Gets a custom parsing function that converts string into target type.
    /// </summary>
    /// <remarks>
    /// When defined, this function is used to override default parsing behavior.
    /// </remarks>
    public Func<string, (object, bool)> Parse { get; }

    /// <summary>
    /// Creates a new mutable parameter from its definition.
    /// </summary>
    /// <returns>New mutable parameter.</returns>
    IMutableParameter CreateParameter();
}

/// <summary>
/// Represents a typed parameter with a default value.
/// </summary>
/// <typeparam name="T">The type of the parameter.</typeparam>
public interface IParameter<T> : IParameter
{
    /// <summary>
    /// Gets default typed value of the parameter.
    /// </summary>
    new T Default { get; }

    /// <summary>
    /// Gets a custom parsing function that converts string into target type.
    /// </summary>
    /// <remarks>
    /// When defined, this function is used to override default parsing behavior.
    /// </remarks>
    new Func<string, (T, bool)> Parse { get; }

    /// <summary>
    /// Creates a new typed mutable parameter from its definition.
    /// </summary>
    /// <returns>New typed mutable parameter.</returns>
    new IMutableParameter<T> CreateParameter();
}

/// <summary>
/// Represents a parameter with a mutable value.
/// </summary>
public interface IMutableParameter : IParameter
{
    /// <summary>
    /// Gets or sets the actual value of the parameter.
    /// </summary>
    object Value { get; set; }
}

/// <summary>
/// Represents a typed parameter with a mutable value.
/// </summary>
/// <typeparam name="T">The type of the parameter.</typeparam>
public interface IMutableParameter<T> : IMutableParameter, IParameter<T>
{
    /// <summary>
    /// Gets or sets the actual typed value of the parameter.
    /// </summary>
    new T Value { get; set; }
}