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
using System.Collections.Generic;

namespace GrafanaAdapters.GrafanaFunctionsCore;

/// <summary>
/// Defines a common interface for parameters of Grafana functions.
/// </summary>
public interface IParameter
{
    /// <summary>
    /// Gets or sets the description of the parameter.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the parameter is required.
    /// </summary>
    bool Required { get; }

    /// <summary>
    /// Gets the type of the parameter.
    /// </summary>
    Type ParameterType { get; }

    /// <summary>
    /// Gets or sets the type name of the parameter.
    /// </summary>
    string ParameterTypeName { get; }
    /// <summary>
    /// Sets the value of the parameter.
    /// </summary>
    void SetValue(GrafanaDataSourceBase dataSourceBase, object value, string target, Dictionary<string, string> metadata, bool isPhasor);
}

/// <summary>
/// Defines a common interface for parameters of a specific type.
/// </summary>
/// <typeparam name="T">The type of the parameter.</typeparam>
public interface IParameter<out T> : IParameter
{
    /// <summary>
    /// Gets default value of the parameter.
    /// </summary>
    T Default { get; }

    /// <summary>
    /// Gets or sets the actual value of the parameter.
    /// </summary>
    T Value { get; }
}