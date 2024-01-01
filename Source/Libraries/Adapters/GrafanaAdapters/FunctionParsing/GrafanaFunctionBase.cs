//******************************************************************************************************
//  GrafanaFunctionBase.cs - Gbtc
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
//  11/19/2023 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.FunctionParsing;

/// <summary>
/// Represents the base functionality for any Grafana function.
/// </summary>
public abstract class GrafanaFunctionBase<T> : IGrafanaFunction<T> where T : struct, IDataSourceValue<T>
{
    private int? m_requiredParameterCount;
    private int? m_optionalParameterCount;
    private int? m_dataSourceParameterIndex;

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract string Description { get; }

    /// <inheritdoc />
    public virtual string[] Aliases => null;

    /// <inheritdoc />
    public virtual GroupOperations SupportedGroupOperations => DefaultGroupOperations;

    /// <inheritdoc />
    public virtual GroupOperations PublishedGroupOperations => DefaultGroupOperations;

    /// <inheritdoc />
    public virtual GroupOperations CheckSupportedGroupOperation(GroupOperations requestedOperation)
    {
        // Default to standard
        if (requestedOperation == 0)
            requestedOperation = GroupOperations.Standard;

        // Verify that the function supports the requested operation
        if (!SupportedGroupOperations.HasFlag(requestedOperation))
            throw new InvalidOperationException($"Function '{Name}' does not support '{requestedOperation}' function operations.");

        return requestedOperation;
    }

    /// <inheritdoc />
    public virtual ParameterDefinitions Parameters => new();

    /// <inheritdoc />
    public virtual List<IParameter> GetValueMutableParameters()
    {
        // Cache lookups for all parameter factory functions per derived function type for faster performance
        (Func<IParameter, IParameter> factoryFunction, IParameter parameterDefinition)[] factoryFunctionCache = TargetCache<(Func<IParameter, IParameter>, IParameter)[]>.GetOrAdd(GetType().FullName, () =>
        {
            return Parameters.Select(parameterDefinition => (GetValueMutableParameterFactoryFunction(parameterDefinition.Type), parameterDefinition)).ToArray();
        });

        // The following operates like 'Parameters.Select(parameterDefinition => new ValueMutableParameter<T>(parameterDefinition)).ToList()',
        // if type 'T' was known at compile-time. Even if type 'T' was known at compile time, the above would not work since type 'T' is
        // variable per each parameter definition.
        return factoryFunctionCache.Select(item => item.factoryFunction(item.parameterDefinition)).ToList();
    }

    /// <inheritdoc />
    public virtual int RequiredParameterCount => m_requiredParameterCount ??= Parameters.Count(parameter => parameter.Required);

    /// <inheritdoc />
    public virtual int OptionalParameterCount => m_optionalParameterCount ??= Parameters.Count(parameter => !parameter.Required);

    /// <inheritdoc />
    public virtual int DataSourceParameterIndex => m_dataSourceParameterIndex ??= Parameters.Select((parameter, index) => (parameter, index))
        .First(item => item.parameter.Type == typeof(IEnumerable<IDataSourceValue>)).index;

    /// <inheritdoc />
    public virtual List<string> ParseParameters(ref string expression)
    {
        return null;
    }

    /// <inheritdoc />
    public abstract IEnumerable<T> Compute(List<IParameter> parameters, CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual IEnumerable<T> ComputeSlice(List<IParameter> parameters, CancellationToken cancellationToken)
    {
        return Compute(parameters, cancellationToken);
    }

    /// <inheritdoc />
    public virtual IEnumerable<T> ComputeSet(List<IParameter> parameters, CancellationToken cancellationToken)
    {
        return Compute(parameters, cancellationToken);
    }
}