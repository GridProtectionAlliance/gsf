//******************************************************************************************************
//  ParameterDefinition.cs - Gbtc
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
//  12/31/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Represents a read-only parameter definition for a Grafana function.
/// </summary>
/// <typeparam name="T">The type of the parameter.</typeparam>
public readonly struct ParameterDefinition<T> : IParameter<T>
{
    /// <inheritdoc />
    public string Name { get; init; }

    /// <inheritdoc />
    public T Default { get; init; }

    object IParameter.Default => Default;

    /// <inheritdoc />
    public string Description { get; init; }

    /// <inheritdoc />
    public Type Type => typeof(T);

    /// <inheritdoc />
    public bool IsDefinition => true;

    /// <inheritdoc />
    public bool Required { get; init; }

    /// <inheritdoc />
    public bool Internal { get; init; }

    /// <inheritdoc />
    public Func<string, (T, bool)> Parse { get; init; }

    Func<string, (object, bool)> IParameter.Parse => null;

    /// <inheritdoc />
    public IMutableParameter<T> CreateParameter()
    {
        return new Parameter<T>(this);
    }

    IMutableParameter IParameter.CreateParameter()
    {
        return new Parameter<T>(this);
    }
}