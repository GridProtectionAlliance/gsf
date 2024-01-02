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

using GrafanaAdapters.DataSources;
using System;
using System.Collections.Generic;
using System.Data;

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
    /// Gets flag that indicates if the parameter is required.
    /// </summary>
    bool Required { get; }

    /// <summary>
    /// Gets the type of the parameter.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Gets flag that indicates if parameter is a definition.
    /// </summary>
    /// <remarks>
    /// Definition parameters are used to define function parameters, not hold values for function evaluation.
    /// </remarks>
    bool IsDefinition { get; }

    /// <summary>
    /// Gets default value of the parameter.
    /// </summary>
    object Default { get; }
}

/// <summary>
/// Represents a typed parameter with a default value.
/// </summary>
/// <typeparam name="T">The type of the parameter.</typeparam>
public interface IParameter<out T> : IParameter
{
    /// <summary>
    /// Gets default value of the parameter.
    /// </summary>
    new T Default { get; }
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

    /// <summary>
    /// Converts parsed value to the mutable parameter type.
    /// </summary>
    /// <typeparam name="T">The type of the data source value.</typeparam>
    /// <param name="value">Parsed value to convert.</param>
    /// <param name="target">Associated target.</param>
    /// <param name="dataSourceValues">Data source values.</param>
    /// <param name="metadata">Metadata associated with the target.</param>
    /// <param name="metadataMap">Metadata map.</param>
    void ConvertParsedValue<T>(string value, string target, IEnumerable<T> dataSourceValues, DataSet metadata, Dictionary<string, string> metadataMap) where T : struct, IDataSourceValue<T>;
}

/// <summary>
/// Represents a typed parameter with a mutable value.
/// </summary>
/// <typeparam name="T">The type of the parameter.</typeparam>
public interface IMutableParameter<T> : IMutableParameter, IParameter<T>
{
    /// <summary>
    /// Gets or sets the actual value of the parameter.
    /// </summary>
    new T Value { get; set; }
}