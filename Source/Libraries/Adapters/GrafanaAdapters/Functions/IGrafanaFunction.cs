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
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Flags that indicate the group operations that a Grafana function can perform.
/// </summary>
[Flags]
public enum GroupOperations
{
    /// <summary>
    /// The function can perform standard, non-grouped, per-trend operations.
    /// </summary>
    Standard = 0x1,
    /// <summary>
    /// The function can perform slice-based group operations.
    /// </summary>
    Slice = 0x2,
    /// <summary>
    /// The function can perform set-based group operations.
    /// </summary>
    Set = 0x4
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
    /// Gets set of group operations that the Grafana function supports.
    /// </summary>
    GroupOperations SupportedGroupOperations { get; }

    /// <summary>
    /// Gets set of group operations that the Grafana function exposes publicly.
    /// </summary>
    GroupOperations PublishedGroupOperations { get; }

    /// <summary>
    /// Checks if function supports requested group operation against <see cref="SupportedGroupOperations"/>.
    /// </summary>
    /// <param name="requestedOperation">Requested operation.</param>
    /// <returns>Supported operation.</returns>
    /// <exception cref="InvalidOperationException">Function does not support the requested operation.</exception>
    /// <remarks>
    /// If the requested operation is <c>0</c>, function will default to <see cref="GroupOperations.Standard"/>.
    /// </remarks>
    GroupOperations CheckSupportedGroupOperation(GroupOperations requestedOperation);

    /// <summary>
    /// Gets the list of defined parameter definitions for the Grafana function.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These parameters are used to define the function signature and are normally constructed from
    /// read-only <see cref="ParameterDefinition{T}"/> instances.
    /// </para>
    /// <para>
    /// Parameter definitions are used to define function parameters, not hold values for function evaluation.
    /// Mutable function parameters, safe for evaluation, are defined by <see cref="Parameters"/> collection.
    /// </para>
    /// </remarks>
    ParameterDefinitions ParameterDefinitions { get; }

    /// <summary>
    /// Gets the number of required parameters, not including data source values expression, of the Grafana function.
    /// </summary>
    int RequiredParameterCount { get; }

    /// <summary>
    /// Gets the number of optional parameters of the Grafana function.
    /// </summary>
    int OptionalParameterCount { get; }

    /// <summary>
    /// Executes custom parameter parsing for the Grafana function.
    /// </summary>
    /// <param name="expression">Expression to parse.</param>
    /// <returns>
    /// Parameter values parsed from the expression; otherwise, <c>null</c> to use standard parsing.
    /// </returns>
    /// <remarks>
    /// This method is used to support custom parameter parsing for functions that may
    /// have special parameter parsing requirements. By default, this method will return
    /// <c>null</c> meaning that standard parameter parsing will be used.
    /// </remarks>
    List<string> ParseParameters(ref string expression);
}

/// <summary>
/// Defines a common interface for Grafana functions for a specific data source value type.
/// </summary>
public interface IGrafanaFunction<out T> : IGrafanaFunction where T : struct, IDataSourceValue<T>
{
    /// <summary>
    /// Gets a list of value mutable parameters for the Grafana function.
    /// </summary>
    /// <returns>New list of value mutable parameters based on the defined parameter definitions of the Grafana function.</returns>
    /// <remarks>
    /// This function creates a new list of value mutable parameters from the defined parameter definitions.
    /// </remarks>
    Parameters GetMutableParameters();

    /// <summary>
    /// Executes the computation for the Grafana function.
    /// </summary>
    /// <param name="parameters">The input parameters for the computation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A sequence of computed data source parameters.</returns>
    IEnumerable<T> Compute(Parameters parameters, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a custom slice computation for the Grafana function.
    /// </summary>
    /// <param name="parameters">The input parameters for the computation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A sequence of computed data source parameters.</returns>
    /// <remarks>
    /// This method is used to support custom slice computations for functions that
    /// need special handling for slice operations. By default, this method will call
    /// <see cref="Compute"/> to perform the computation.
    /// </remarks>
    IEnumerable<T> ComputeSlice(Parameters parameters, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a custom set computation for the Grafana function.
    /// </summary>
    /// <param name="parameters">The input parameters for the computation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A sequence of computed data source parameters.</returns>
    /// <remarks>
    /// This method is used to support custom set computations for functions that
    /// need special handling for set operations. By default, this method will call
    /// <see cref="Compute"/> to perform the computation.
    /// </remarks>
    IEnumerable<T> ComputeSet(Parameters parameters, CancellationToken cancellationToken);
}