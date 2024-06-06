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

using GrafanaAdapters.DataSourceValueTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Represents the base functionality for any Grafana function.
/// </summary>
public abstract class GrafanaFunctionBase<T> : IGrafanaFunction<T> where T : struct, IDataSourceValueType<T>
{
    private int? m_requiredParameterCount;
    private int? m_optionalParameterCount;
    private int? m_internalParameterCount;

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract string Description { get; }

    /// <inheritdoc />
    public virtual string[] Aliases => null;

    /// <inheritdoc />
    public abstract ReturnType ReturnType { get; }

    /// <inheritdoc />
    public Category Category { get; init; }

    /// <inheritdoc />
    public virtual GroupOperations AllowedGroupOperations { get; internal set; } = Common.DefaultGroupOperations;

    /// <inheritdoc />
    public virtual GroupOperations PublishedGroupOperations { get; internal set; } = Common.DefaultGroupOperations;

    /// <inheritdoc />
    public virtual GroupOperations CheckAllowedGroupOperation(GroupOperations requestedOperation)
    {
        // Assume no group operation is desired if requested operation is undefined
        if (requestedOperation == GroupOperations.Undefined)
            requestedOperation = GroupOperations.None;

        // Verify that the function supports the requested operation
        if (!AllowedGroupOperations.HasFlag(requestedOperation))
            throw new SyntaxErrorException($"Function '{Name}' does not support a '{requestedOperation}' group operation.");

        return requestedOperation;
    }

    /// <inheritdoc />
    public virtual bool IsSliceSeriesEquivalent => ReturnType == ReturnType.Series && AllowedGroupOperations.HasFlag(GroupOperations.Slice);

    /// <inheritdoc />
    public virtual bool ResultIsSetTargetSeries => false;

    /// <inheritdoc />
    public virtual ParameterDefinitions ParameterDefinitions => [];

    /// <inheritdoc />
    public virtual int RequiredParameterCount => m_requiredParameterCount ??= ParameterDefinitions.Count(parameter => parameter.Required) - 1;

    /// <inheritdoc />
    public virtual int OptionalParameterCount => m_optionalParameterCount ??= ParameterDefinitions.Count(parameter => !parameter.Required);

    /// <inheritdoc />
    public virtual int InternalParameterCount => m_internalParameterCount ??= ParameterDefinitions.Count(parameter => parameter.Internal);

    /// <inheritdoc />
    public virtual (List<string> parsedParameters, string updatedQueryExpression) ParseParameters(QueryParameters queryParameters, string queryExpression)
    {
        return (null, null);
    }

    /// <inheritdoc />
    public virtual string FormatTargetName(GroupOperations groupOperation, string targetName, string[] parsedParameters)
    {
        string groupName = groupOperation <= GroupOperations.None ? string.Empty : $"{groupOperation}";
        return $"{groupName}{Name}({this.FormatParameters(parsedParameters)}{targetName})";
    }

    /// <inheritdoc />
    public int DataTypeIndex => DataSourceValueTypeCache<T>.DataTypeIndex;

    /// <inheritdoc />
    public abstract IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual IAsyncEnumerable<T> ComputeSliceAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        return ComputeAsync(parameters, cancellationToken);
    }

    /// <inheritdoc />
    public virtual IAsyncEnumerable<T> ComputeSetAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        return ComputeAsync(parameters, cancellationToken);
    }

    /// <summary>
    /// Gets data source values enumeration found in the provided parameters.
    /// </summary>
    /// <param name="parameters">Input parameters.</param>
    /// <returns>Data source values from provided parameters.</returns>
    /// <exception cref="InvalidOperationException">Last parameter is not a data source value of type <see cref="IAsyncEnumerable{T}"/>.</exception>
    protected virtual IAsyncEnumerable<T> GetDataSourceValues(Parameters parameters)
    {
        return (parameters.LastOrDefault() as IMutableParameter<IAsyncEnumerable<T>>)?.Value ??
            throw new InvalidOperationException($"Last parameter is not a data source value of type '{typeof(IAsyncEnumerable<T>).Name}'.");
    }

    /// <summary>
    /// Executes specified function against data source values enumeration using provided parameters.
    /// </summary>
    /// <param name="function">Function to execute.</param>
    /// <param name="parameters">Input parameters.</param>
    /// <returns>Deferred enumeration of computed values.</returns>
    /// <remarks>
    /// This method uses the <see cref="IDataSourceValueType{T}.TransposeCompute"/> method to execute the specified
    /// function against each data source value in the provided enumeration operating on all the values in the
    /// target data source value type. For example, if the target data source value type is a phasor, this method
    /// will execute the function against both the magnitude and angle of each phasor value.
    /// </remarks>
    protected virtual IAsyncEnumerable<T> ExecuteFunction(Func<double, double> function, Parameters parameters)
    {
        return GetDataSourceValues(parameters).Select(dataValue => dataValue.TransposeCompute(function));
    }
}