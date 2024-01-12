//******************************************************************************************************
//  DataSourceCache.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
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
//  01/08/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using GrafanaAdapters.Functions;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using GSF.Threading;

// ReSharper disable StaticMemberInGenericType

namespace GrafanaAdapters.DataSources;

internal static class DataSourceCache
{
    public static IReadOnlyCollection<Type> Types => s_types;

    private static readonly HashSet<Type> s_types = new();

    internal static void AddType(Type type)
    {
        s_types.Add(type);
    }

    public static void ResetAll()
    {
        const string InitializeMethodName = nameof(DataSourceCache<DataSourceValue>.Initialize);

        foreach (Type type in s_types)
            typeof(DataSourceCache<>).MakeGenericType(type).GetMethod(InitializeMethodName, BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
    }

    public static void UpdateMetadata(DataSet metadata)
    {
        foreach (Type type in s_types)
        {
            MethodInfo updateMetadataMethod = typeof(DataSourceCache<>).MakeGenericType(type).GetMethod(nameof(UpdateMetadata), BindingFlags.NonPublic | BindingFlags.Static);
            updateMetadataMethod?.Invoke(null, new object[] { metadata });
        }
    }
}

internal static class DataSourceCache<T> where T : struct, IDataSourceValue<T>
{
    private static readonly ShortSynchronizedOperation s_updateMetadataOperation;

    public static IGrafanaFunction<T>[] Functions { get; private set; }

    public static Dictionary<string, IGrafanaFunction<T>> FunctionMap { get; private set; }

    public static Regex FunctionsRegex { get; private set; }

    public static DataSet Metadata { get; private set; }

    static DataSourceCache()
    {
        DataSourceCache.AddType(typeof(T));
        Initialize();
        s_updateMetadataOperation = new ShortSynchronizedOperation(() => default(T).UpdateMetadata(Metadata));
    }

    internal static void UpdateMetadata(DataSet metadata)
    {
        Metadata = metadata;
        s_updateMetadataOperation.RunOnce();
    }

    internal static void Initialize()
    {
        // This regex matches all functions and their parameters, critically, at top-level only - sub functions are part of parameter data expression
        const string GrafanaFunctionsExpression = @"(?<GroupOp>Slice|Set)?(?<Function>{0})\s*\((?<Expression>([^\(\)]|(?<counter>\()|(?<-counter>\)))*(?(counter)(?!)))\)";

        Functions = FunctionParsing.GetGrafanaFunctions().OfType<IGrafanaFunction<T>>().ToArray();
        FunctionMap = CreateFunctionMap();
        FunctionsRegex = new Regex(string.Format(GrafanaFunctionsExpression, string.Join("|", FunctionMap.Keys)), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    // Build and cache a data type specific lookup map for all functions by name and aliases
    private static Dictionary<string, IGrafanaFunction<T>> CreateFunctionMap()
    {
        Dictionary<string, IGrafanaFunction<T>> functionMap = new(StringComparer.OrdinalIgnoreCase);

        foreach (IGrafanaFunction<T> function in Functions)
        {
            functionMap[function.Name] = function;

            if (function.Aliases is null)
                continue;

            foreach (string alias in function.Aliases)
                functionMap[alias] = function;
        }

        return functionMap;
    }
}