//******************************************************************************************************
//  FunctionDescription.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/17/2023 - C. Lackner
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

namespace GrafanaAdapters.Model.Functions;

/// <summary>
/// Defines a Grafana function.
/// </summary>
public class FunctionDescription
{
    /// <summary>
    /// Gets or sets the name of the function.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Gets or sets the description of the function.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// Gets or sets other names for the function.
    /// </summary>
    public string[] aliases { get; set; }

    /// <summary>
    /// Gets or sets the return type of the function, either "Scalar" or "Series".
    /// </summary>
    public string returnType { get; set; }

    /// <summary>
    /// Gets or sets the category of the function, either "BuiltIn" or "Custom".
    /// </summary>
    public string category { get; set; }

    /// <summary>
    /// Gets or sets allowed group operations for the function.
    /// </summary>
    public string allowedGroupOperations { get; set; }

    /// <summary>
    /// Gets or sets published group operations for the function.
    /// </summary>
    public string publishedGroupOperations { get; set; }

    /// <summary>
    /// Gets or sets the parameter descriptions for the function.
    /// </summary>
    public ParameterDescription[] parameters { get; set; }
}