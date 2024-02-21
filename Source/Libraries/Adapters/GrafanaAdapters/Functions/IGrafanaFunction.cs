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

using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.Functions.BuiltIn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Flags that indicate the group operations that a Grafana function can perform.
/// </summary>
[Flags]
public enum GroupOperations
{
    /// <summary>
    /// The function has not defined any group operations.
    /// </summary>
    Undefined = 0x0,
    /// <summary>
    /// The function can perform standard, non-grouped, per-trend operations.
    /// </summary>
    None = 0x1, // Must not be zero to create a discernible value
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
/// Represents the return type of Grafana function.
/// </summary>
public enum ReturnType
{
    /// <summary>
    /// The function returns a single value.
    /// </summary>
    Scalar,
    /// <summary>
    /// The function returns a series of values.
    /// </summary>
    Series
}

/// <summary>
/// Represents a Grafana function category.
/// </summary>
public enum Category
{
    /// <summary>
    /// The function is a built-in function.
    /// </summary>
    BuiltIn,
    /// <summary>
    /// The function is a custom function.
    /// </summary>
    Custom
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
    /// Gets the return type of the Grafana function, i.e., scalar or series.
    /// </summary>
    ReturnType ReturnType { get; }

    /// <summary>
    /// Gets or sets a flag indicating whether the function behaves equivalently when processed as a series or as a
    /// slice. Value defaults to <c>true</c> for functions that return a series of values, i.e., the return type is
    /// <see cref="ReturnType.Series"/>, and the <see cref="AllowedGroupOperations"/> includes the flag for
    /// <see cref="GroupOperations.Slice"/> operations; otherwise, value defaults to <c>false</c> when the return
    /// type is a scalar value, i.e., <see cref="ReturnType.Scalar"/>, or the <see cref="AllowedGroupOperations"/>
    /// does not include the flag for <see cref="GroupOperations.Slice"/> operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When set to <c>true</c>, property indicates that the function, when dealing with time series data, produces
    /// the same result matrix whether processed horizontally (series-by-series) or vertically (slice-by-slice).
    /// This is typically applicable to functions that return a series of values and are designed to operate across
    /// time-aligned data series.
    /// </para>
    /// <para>
    /// Setting this property to <c>true</c> enables certain optimizations. For instance, the system can bypass
    /// slice-mode processing and use standard series processing instead, as both approaches yield equivalent results.
    /// This can lead to performance improvements, especially in data-intensive scenarios.
    /// </para>
    /// <para>
    /// It is important for users defining custom functions to accurately set this property. If the function's output
    /// does not depend on the specific mode of time series processing (series vs. slice), set this property to
    /// <c>true</c>. Conversely, if the function yields different results when processed by slice-by-slice as compared
    /// to when processed series-by-series, or it is a scalar function, this property should be set to <c>false</c>.
    /// </para>
    /// <para>
    /// Incorrectly setting this property may lead to unexpected behavior or suboptimal performance, as the system
    /// relies on this flag to determine the most efficient processing approach.
    /// </para>
    /// <para>
    /// Internally this function descriptor provides a hint to the Grafana data source adapter on how to handle series
    /// results for a function. For functions that return a scalar value this property is ignored, but is automatically
    /// set to <c>false</c> to preserve meaning. For slice operations on functions that return a series of values and
    /// produce the same result matrix whether processed horizontally or vertically, the slice operation is equivalent
    /// to its non-slice operation using the <see cref="Interval{T}"/> function over the same expression, for example,
    /// the following queries are equivalent:
    /// <list type="bullet">
    ///     <item>
    ///        <c>SliceShift(0.02, 1, FILTER TOP 10 ActiveMeasurements WHERE SignalType='FREQ')</c> -- <i>and</i> --<br/>
    ///        <c>Shift(1, Interval(0.02, FILTER TOP 10 ActiveMeasurements WHERE SignalType='FREQ'))</c>
    ///     </item>
    ///     <item>
    ///         <c>SliceRound(0.0333, 3, ACME-STAR:FREQ; ACME-PLUS:FREQ)</c> -- <i>and</i> --<br/>
    ///         <c>Round(3, Interval(0.0333, ACME-STAR:FREQ; ACME-PLUS:FREQ))</c>
    ///     </item>
    /// </list>
    /// As a result, slice operations that return a series of values and produce the same result matrix whether processed
    /// horizontally or vertically, i.e., is slice-series equivalent, are automatically are replaced with the equivalent
    /// non-slice operation and an <see cref="Interval{T}"/> function as an operational optimization.
    /// </para>
    /// </remarks>
    bool IsSliceSeriesEquivalent { get; }

    /// <summary>
    /// Gets flag that determines if function result is target series for set-based group operations.
    /// </summary>
    /// <remarks>
    /// For set-based group operations, there can also be data in which target series is selected,
    /// e.g., with <see cref="Minimum{T}"/> or <see cref="Maximum{T}"/> functions.
    /// </remarks>
    bool ResultIsSetTargetSeries { get; }

    /// <summary>
    /// Gets the category of the Grafana function, i.e., built-in or custom.
    /// </summary>
    /// <remarks>
    /// This property is automatically assigned by the system.
    /// </remarks>
    Category Category { get; init; }

    /// <summary>
    /// Gets set of group operations that the Grafana function allows.
    /// </summary>
    /// <remarks>
    /// Operations that are not allowed should be taken to mean that the use of the group operation for a function is an
    /// error. Implementors should carefully consider which group operations that a function exposes as not allowed since
    /// when a user selects a group operation that is not allowed, this results in an exception. Groups operations can
    /// be hidden from the user by overriding the <see cref="PublishedGroupOperations"/>. Additionally, a group operation
    /// that is not supported can be ignored, instead of throwing an exception, by forcing supported operations. This is
    /// handled by overriding the <see cref="CheckAllowedGroupOperation"/> method. See <see cref="Label{T}"/> function
    /// for an example of this.
    /// </remarks>
    GroupOperations AllowedGroupOperations { get; }

    /// <summary>
    /// Gets set of group operations that the Grafana function exposes publicly.
    /// </summary>
    /// <remarks>
    /// Normally, the published group operations should be a subset of the allowed group operations.
    /// </remarks>
    GroupOperations PublishedGroupOperations { get; }

    /// <summary>
    /// Checks if function allows requested group operation against <see cref="AllowedGroupOperations"/> property.
    /// </summary>
    /// <param name="requestedOperation">Requested operation.</param>
    /// <returns>Supported operation.</returns>
    /// <exception cref="SyntaxErrorException">Function does not support the requested operation.</exception>
    GroupOperations CheckAllowedGroupOperation(GroupOperations requestedOperation);

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
    /// Gets the number of internal parameters of the Grafana function.
    /// </summary>
    int InternalParameterCount { get; }

    /// <summary>
    /// Executes custom parameter parsing for the Grafana function.
    /// </summary>
    /// <param name="queryParameters">Query parameters.</param>
    /// <param name="queryExpression">Expression to parse.</param>
    /// <returns>
    /// A tuple of parsed parameters and any remaining query expression (typically the filter expression)
    /// after parsing parameters. Tuple of <c>(null, null)</c> should be returned to use standard parsing.
    /// </returns>
    /// <remarks>
    /// This method is used to support custom parameter parsing for functions that may have special parameter
    /// parsing requirements. By default, this method will return a tuple of <c>(null, null)</c> meaning that
    /// standard parameter parsing will be used.
    /// </remarks>
    (List<string> parsedParameters, string updatedQueryExpression) ParseParameters(QueryParameters queryParameters, string queryExpression);

    /// <summary>
    /// Gets a formatted target name for the Grafana function.
    /// </summary>
    /// <param name="groupOperation">Group operation from the format target name.</param>
    /// <param name="targetName">Target name to format.</param>
    /// <param name="parsedParameters">Parsed parameters.</param>
    /// <returns>
    /// Target name format for the Grafana function, typically in the form of: Name(Parameters,TargetName).
    /// </returns>
    string FormatTargetName(GroupOperations groupOperation, string targetName, string[] parsedParameters);

    /// <summary>
    /// Gets the data source value type index associated with the Grafana function.
    /// </summary>
    int DataTypeIndex { get; }
}

/// <summary>
/// Defines a common interface for Grafana functions for a specific data source value type.
/// </summary>
public interface IGrafanaFunction<out T> : IGrafanaFunction where T : struct, IDataSourceValueType<T>
{
    /// <summary>
    /// Executes the computation for the Grafana function.
    /// </summary>
    /// <param name="parameters">Input parameters for the computation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A sequence of computed data source parameters.</returns>
    IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a custom slice computation for the Grafana function.
    /// </summary>
    /// <param name="parameters">Input parameters for the computation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A sequence of computed data source parameters.</returns>
    /// <remarks>
    /// This method is used to support custom slice computations for functions that
    /// need special handling for slice operations. By default, this method will call
    /// <see cref="ComputeAsync"/> to perform the computation.
    /// </remarks>
    IAsyncEnumerable<T> ComputeSliceAsync(Parameters parameters, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a custom set computation for the Grafana function.
    /// </summary>
    /// <param name="parameters">Input parameters for the computation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A sequence of computed data source parameters.</returns>
    /// <remarks>
    /// This method is used to support custom set computations for functions that
    /// need special handling for set operations. By default, this method will call
    /// <see cref="ComputeAsync"/> to perform the computation.
    /// </remarks>
    IAsyncEnumerable<T> ComputeSetAsync(Parameters parameters, CancellationToken cancellationToken);
}