//******************************************************************************************************
//  ParsedGrafanaFunction.cs - Gbtc
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
//  12/30/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GrafanaAdapters.DataSourceValueTypes;

namespace GrafanaAdapters.Functions;

/// <summary>
/// Represents a parsed Grafana function.
/// </summary>
/// <typeparam name="T">Data source value type.</typeparam>
public class ParsedGrafanaFunction<T> where T : struct, IDataSourceValueType<T>
{
    /// <summary>
    /// Parsed Grafana function.
    /// </summary>
    public IGrafanaFunction<T> Function;

    /// <summary>
    /// Defined group operation for parsed function.
    /// </summary>
    public GroupOperations GroupOperation;

    /// <summary>
    /// Parsed function expression, e.g., parameters.
    /// </summary>
    public string Expression;

    /// <summary>
    /// Defines the match from the RegEx for the function.
    /// </summary>
    public string MatchedValue;
}