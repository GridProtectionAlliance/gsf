//******************************************************************************************************
//  Parameter.cs - Gbtc
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
// ReSharper disable PossibleMultipleEnumeration

using System;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Represents a mutable parameter of a Grafana function.
/// </summary>
/// <typeparam name="T">The type of the parameter.</typeparam>
internal class Parameter<T> : IMutableParameter<T>
{
    private readonly Func<string, (object, bool)> m_parse;

    // Create a new mutable parameter from its definition.
    public Parameter(ParameterDefinition<T> definition)
    {
        Definition = definition;
        Value = definition.Default;

        if (Definition.Parse is not null)
            m_parse = value => Definition.Parse(value);
    }

    public string Name => Definition.Name;

    public ParameterDefinition<T> Definition { get; }

    public T Default => Definition.Default;

    object IParameter.Default => Default;

    // Only the value in this class is mutable.
    public T Value { get; set; }

    object IMutableParameter.Value
    {
        get => Value;
        set => Value = (T)value;
    }

    public string Description => Definition.Description;

    public Type Type => Definition.Type;

    public bool IsDefinition => false;

    public bool Required => Definition.Required;

    public bool Internal => Definition.Internal;

    public Func<string, (T, bool)> Parse => Definition.Parse;

    Func<string, (object, bool)> IParameter.Parse => m_parse;

    IMutableParameter<T> IParameter<T>.CreateParameter()
    {
        return MemberwiseClone() as IMutableParameter<T>;
    }

    IMutableParameter IParameter.CreateParameter()
    {
        return MemberwiseClone() as IMutableParameter;
    }
}