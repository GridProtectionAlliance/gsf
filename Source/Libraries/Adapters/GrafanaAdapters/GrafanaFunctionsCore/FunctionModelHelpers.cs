//******************************************************************************************************
//  FunctionModelHelpers.cs - Gbtc
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
using System.Text.RegularExpressions;

namespace GrafanaAdapters.GrafanaFunctionsCore;

internal static class FunctionModelHelpers
{
    private const string ExpressionFormat = @"^{0}\s*\(\s*(?<Expression>.+)\s*\)";

    public const FunctionOperations DefaultFunctionOperations = FunctionOperations.Standard | FunctionOperations.Slice | FunctionOperations.Set;

    public static readonly Regex FunctionRegex = GenerateFunctionRegex(".*");

    public static Regex GenerateFunctionRegex(params string[] functionNames)
    {
        if (functionNames.Length == 0)
            throw new ArgumentException("At least one function name must be specified.", nameof(functionNames));

        string allAliases = functionNames.Length == 1 ? functionNames[0] :$"({string.Join("|", functionNames)})";
        return new Regex(string.Format(ExpressionFormat, allAliases), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}