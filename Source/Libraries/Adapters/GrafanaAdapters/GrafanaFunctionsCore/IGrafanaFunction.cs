//******************************************************************************************************
//  IGrafanaFunction.cs - Gbtc
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
using System.Text.RegularExpressions;

namespace GrafanaAdapters.GrafanaFunctionsCore;

/// <summary>
/// Flags that indicate the operations that a Grafana function can perform.
/// </summary>
[Flags]
public enum FunctionOperations
{
    /// <summary>
    /// The function can perform standard per-trend operations.
    /// </summary>
    Standard,
    /// <summary>
    /// The function can perform slice-based group operations.
    /// </summary>
    Slice,
    /// <summary>
    /// The function can perform set-based group operations.
    /// </summary>
    Set
}

/// <summary>
/// Defines a common interface for Grafana functions.
/// </summary>
public interface IGrafanaFunction
{
    /// <summary>
    /// Gets the name of the Grafana function.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the Grafana function.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets any defined aliases for the Grafana function.
    /// </summary>
    string[] Aliases { get; }

    /// <summary>
    /// Gets the regular expression for matching the Grafana function.
    /// </summary>
    Regex Regex { get; }

    /// <summary>
    /// Gets set of operations that the Grafana function supports.
    /// </summary>
    FunctionOperations SupportedFunctionOperations { get; }

    /// <summary>
    /// Gets set of operations that the Grafana function exposes publicly.
    /// </summary>
    FunctionOperations PublishedFunctionOperations { get; }

    /// <summary>
    /// Gets the list of parameters of the Grafana function.
    /// </summary>
    List<IParameter> Parameters { get; }
}

/// <summary>
/// Defines a common interface for Grafana functions for a specific data source value type.
/// </summary>
public interface IGrafanaFunction<T> : IGrafanaFunction where T : IDataSourceValue
{
    /// <summary>
    /// Executes the computation for the Grafana function.
    /// </summary>
    /// <param name="parameters">The input parameters for the computation.</param>
    /// <returns>A sequence of computed data source parameters.</returns>
    DataSourceValueGroup<T> Compute(List<IParameter> parameters);

    /// <summary>
    /// Executes a custom slice computation for the Grafana function.
    /// </summary>
    /// <param name="parameters">The input parameters for the computation.</param>
    /// <returns>A sequence of computed data source parameters.</returns>
    /// <remarks>
    /// This method is used to support custom slice computations for functions that
    /// need special handling for slice operations. By default, this method will call
    /// <see cref="Compute"/> to perform the computation.
    /// </remarks>
    DataSourceValueGroup<T> ComputeSlice(List<IParameter> parameters);

    /// <summary>
    /// Executes a custom set computation for the Grafana function.
    /// </summary>
    /// <param name="parameters">The input parameters for the computation.</param>
    /// <returns>A sequence of computed data source parameters.</returns>
    /// <remarks>
    /// This method is used to support custom set computations for functions that
    /// need special handling for set operations. By default, this method will call
    /// <see cref="Compute"/> to perform the computation.
    /// </remarks>
    DataSourceValueGroup<T> ComputeSet(List<IParameter> parameters);
}