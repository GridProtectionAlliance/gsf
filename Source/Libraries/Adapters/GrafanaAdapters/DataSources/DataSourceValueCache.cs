﻿//******************************************************************************************************
//  DataSourceValueCache.cs - Gbtc
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
// ReSharper disable StaticMemberInGenericType

using GrafanaAdapters.DataSources.BuiltIn;
using GrafanaAdapters.Functions;
using GSF;
using GSF.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace GrafanaAdapters.DataSources;

/// <summary>
/// Represents a cache of all defined data source value types and their default instances.
/// </summary>
public static class DataSourceValueCache
{
    private static IDataSourceValue[] s_defaultInstances;
    private static Type[] s_loadedTypes;
    private static Dictionary<string, int> s_typeIndexMap;
    private static readonly object s_defaultInstancesLock = new();

    /// <summary>
    /// Gets a default instance list of all the defined data source value type implementations.
    /// </summary>
    /// <returns>Default instance list of all the defined data source value type implementations.</returns>
    public static IDataSourceValue[] GetDefaultInstances()
    {
        // Caching default data source value types and instances so expensive assembly load with
        // type inspections and reflection-based instance creation of types are only done once.
        // If dynamic reload is needed at runtime, call ReloadDataSourceValueTypes() method.
        IDataSourceValue[] defaultInstances = Interlocked.CompareExchange(ref s_defaultInstances, null, null);

        if (defaultInstances is not null)
            return defaultInstances;

        // If many external calls, e.g., web requests, are made to this function at the same time,
        // there will be an initial pause while the first thread loads the data source values
        lock (s_defaultInstancesLock)
        {
            // Check if another thread already created the data source values
            if (s_defaultInstances is not null)
                return s_defaultInstances;

            // Load all data source value types from any assemblies in the current directory
            string dataSourceValuesPath = FilePath.GetAbsolutePath("").EnsureEnd(Path.DirectorySeparatorChar);
            List<Type> implementations = typeof(IDataSourceValue).LoadImplementations(dataSourceValuesPath, true, false);

            // To maintain consistent order between runs, we sort the data source value types by load order and then by name
            IDataSourceValue[] instances = implementations
                .Select(type => (IDataSourceValue)Activator.CreateInstance(type))
                .OrderBy(dsv => dsv.LoadOrder)
                .ThenBy(dsv => dsv.GetType().Name)
                .ToArray();

            Interlocked.Exchange(ref s_loadedTypes, instances.Select(dsv => dsv.GetType()).ToArray());
            Interlocked.Exchange(ref s_defaultInstances, instances.ToArray());
        }

        return s_defaultInstances;
    }

    /// <summary>
    /// Gets a list of all the cached data source value types.
    /// </summary>
    public static IReadOnlyCollection<Type> LoadedTypes
    {
        get
        {
            Type[] loadedTypes = Interlocked.CompareExchange(ref s_loadedTypes, null, null);

            if (loadedTypes is not null)
                return loadedTypes;

            // Initialize data source value type cache
            lock (s_defaultInstancesLock)
            {
                // Generate list of data source value instances which creates loaded type cache in the process
                GetDefaultInstances();
                return s_loadedTypes;
            }
        }
    }

    /// <summary>
    /// Gets a default instance of the specified data source value type (by index).
    /// </summary>
    /// <param name="dataTypeIndex">Index of target <see cref="IDataSourceValue"/> to lookup.</param>
    /// <returns>Default instance of the specified data source value type, found by index.</returns>
    /// <exception cref="IndexOutOfRangeException">Invalid data type index provided.</exception>
    public static IDataSourceValue GetDefaultInstance(int dataTypeIndex)
    {
        if (dataTypeIndex < 0 || dataTypeIndex >= LoadedTypes.Count)
            throw new IndexOutOfRangeException("Invalid data type index provided.");

        return GetDefaultInstances()[dataTypeIndex];
    }

    /// <summary>
    /// Gets type index for the specified data source value type; or, -1 if not found
    /// </summary>
    /// <param name="dataTypeName">Type name of target <see cref="IDataSourceValue"/> to lookup.</param>
    /// <returns>Index of specified data source value type; or, -1 if not found.</returns>
    public static int GetTypeIndex(string dataTypeName) => TypeIndexMap.TryGetValue(dataTypeName, out int index) ? index : -1;

    // Gets an index for the specified data source value type for the DataSourceValueTypes array
    // Lookup is case-insensitive and will match on type name or full type name
    internal static Dictionary<string, int> TypeIndexMap => s_typeIndexMap ??= CreateTypeIndexMap();

    // Gets an index map of all the defined data source value types by name and full name
    private static Dictionary<string, int> CreateTypeIndexMap()
    {
        Dictionary<string, int> typeIndexMap = new(StringComparer.OrdinalIgnoreCase);
        IReadOnlyCollection<Type> types = LoadedTypes.ToArray();

        for (int index = 0; index < types.Count; index++)
        {
            Type type = types.ElementAt(index);
            string typeName = type.Name;

            // For non-unique type names, we use full type name as key
            if (typeIndexMap.ContainsKey(typeName))
                typeName = type.FullName!;

            typeIndexMap[typeName] = index;

            if (!typeName.Equals(type.FullName))
                typeIndexMap[type.FullName!] = index;
        }

        return typeIndexMap;
    }

    // Reloads all data source value types and default instances
    internal static void ReloadDataSourceValueTypes()
    {
        lock (s_defaultInstancesLock)
        {
            Interlocked.Exchange(ref s_defaultInstances, null);
            Interlocked.Exchange(ref s_loadedTypes, null);
            Interlocked.Exchange(ref s_typeIndexMap, null);
        }
    }

    // Reinitializes all data source caches
    internal static void ReinitializeAll()
    {
        const string InitializeMethodName = nameof(DataSourceValueCache<DataSourceValue>.Initialize);

        // Just using reflection here since this method will only be called rarely
        foreach (Type type in LoadedTypes)
            typeof(DataSourceValueCache<>).MakeGenericType(type).GetMethod(InitializeMethodName, BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
    }
}

// Caches functions, function map and functions regex for a specific data source value type
internal static class DataSourceValueCache<T> where T : struct, IDataSourceValue<T>
{
    public static IGrafanaFunction<T>[] Functions { get; private set; }

    public static Dictionary<string, IGrafanaFunction<T>> FunctionMap { get; private set; }

    public static Regex FunctionsRegex { get; private set; }

    public static readonly int DataTypeIndex = DataSourceValueCache.TypeIndexMap[typeof(T).Name];

    static DataSourceValueCache()
    {
        Initialize();
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