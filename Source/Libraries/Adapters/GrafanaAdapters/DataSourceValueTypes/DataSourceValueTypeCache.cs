//******************************************************************************************************
//  MeasurementValueCache.cs - Gbtc
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

using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using GrafanaAdapters.Functions;
using GSF;
using GSF.Diagnostics;
using GSF.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace GrafanaAdapters.DataSourceValueTypes;

/// <summary>
/// Represents a cache of all defined data source value types and their default instances.
/// </summary>
public static class DataSourceValueTypeCache
{
    private static IDataSourceValueType[] s_defaultInstances;
    private static Type[] s_loadedTypes;
    private static Dictionary<string, int> s_typeIndexMap;
    private static readonly object s_defaultInstancesLock = new();
    private static readonly LogPublisher s_log = Logger.CreatePublisher(typeof(DataSourceValueTypeCache), MessageClass.Component);

    /// <summary>
    /// Gets default instance list of all the defined data source value type implementations.
    /// </summary>
    public static IDataSourceValueType[] DefaultInstances
    {
        get
        {
            // Caching default data source value types and instances so expensive assembly load with
            // type inspections and reflection-based instance creation of types are only done once.
            // If dynamic reload is needed at runtime, call ReloadMeasurementValueTypes() method.
            IDataSourceValueType[] defaultInstances = Interlocked.CompareExchange(ref s_defaultInstances, null, null);

            if (defaultInstances is not null)
                return defaultInstances;

            // If many external calls, e.g., web requests, are made to this function at the same time,
            // there will be an initial pause while the first thread loads the data source values
            lock (s_defaultInstancesLock)
            {
                // Check if another thread already created the data source values
                if (s_defaultInstances is not null)
                    return s_defaultInstances;

                const string EventName = $"{nameof(DataSourceValueTypeCache)} {nameof(IDataSourceValueType)} Type Load";

                try
                {
                    s_log.Publish(MessageLevel.Info, EventName, $"Starting load for {nameof(IDataSourceValueType)} types...");
                    long startTime = DateTime.UtcNow.Ticks;

                    // Load all data source value types from any assemblies in the current directory
                    string dataSourceValueTypesPath = FilePath.GetAbsolutePath("").EnsureEnd(Path.DirectorySeparatorChar);
                    List<Type> implementationTypes = typeof(IDataSourceValueType).LoadImplementations(dataSourceValueTypesPath, true, false);

                    // To maintain consistent order between runs, we sort the data source value types by load order and then by name
                    IDataSourceValueType[] instances = implementationTypes
                        .Select(type => (IDataSourceValueType)Activator.CreateInstance(type))
                        .OrderBy(dsv => dsv.LoadOrder)
                        .ThenBy(dsv => dsv.GetType().Name)
                        .ToArray();

                    Interlocked.Exchange(ref s_loadedTypes, instances.Select(dsv => dsv.GetType()).ToArray());
                    Interlocked.Exchange(ref s_defaultInstances, instances.ToArray());

                    string elapsedTime = new TimeSpan(DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(3);
                    s_log.Publish(MessageLevel.Info, EventName, $"Completed loading {nameof(IDataSourceValueType)} types: loaded {s_loadedTypes.Length:N0} types in {elapsedTime}.");
                }
                catch (Exception ex)
                {
                    s_log.Publish(MessageLevel.Error, EventName, $"Failed while loading {nameof(IDataSourceValueType)} types: {ex.Message}", exception: ex);
                }
            }

            return s_defaultInstances;
        }
    }

    /// <summary>
    /// Gets cache of defined data source value types loaded from local assemblies.
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
                // Get list of data source value instances, this establishes cache of loaded types
                _ = DefaultInstances;
                return s_loadedTypes;
            }
        }
    }

    /// <summary>
    /// Gets a default instance of the specified data source value type (by index).
    /// </summary>
    /// <param name="dataTypeIndex">Index of target <see cref="IDataSourceValueType"/> to lookup.</param>
    /// <returns>Default instance of the specified data source value type, found by index.</returns>
    /// <exception cref="IndexOutOfRangeException">Invalid data type index provided.</exception>
    /// <remarks>
    /// Use this method to provide a cleaner, more specific, error message to the user when a
    /// data source value type is specified that is not found.
    /// </remarks>
    public static IDataSourceValueType GetDefaultInstance(int dataTypeIndex)
    {
        if (dataTypeIndex < 0 || dataTypeIndex >= LoadedTypes.Count)
            throw new IndexOutOfRangeException($"Invalid data type index provided. Index must be between 0 and {LoadedTypes.Count - 1:N0}.");

        return DefaultInstances[dataTypeIndex];
    }

    /// <summary>
    /// Gets type index for the specified data source value type; or, -1 if not found
    /// </summary>
    /// <param name="dataTypeName">Type name of target <see cref="IDataSourceValueType"/> to lookup.</param>
    /// <returns>Index of specified data source value type; or, -1 if not found.</returns>
    public static int GetTypeIndex(string dataTypeName) => TypeIndexMap.TryGetValue(dataTypeName, out int index) ? index : -1;

    // Gets an index for the specified data source value type for the MeasurementValueTypes array
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
        const string InitializeMethodName = nameof(DataSourceValueTypeCache<MeasurementValue>.Initialize);

        // Just using reflection here since this method will only be called rarely
        foreach (Type type in LoadedTypes)
            typeof(DataSourceValueTypeCache<>).MakeGenericType(type).GetMethod(InitializeMethodName, BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
    }
}

// Caches functions, function map and functions regex for a specific data source value type
internal static class DataSourceValueTypeCache<T> where T : struct, IDataSourceValueType<T>
{
    public static IGrafanaFunction<T>[] Functions { get; private set; }

    public static Dictionary<string, IGrafanaFunction<T>> FunctionMap { get; private set; }

    public static Regex FunctionsRegex { get; private set; }

    public static readonly int DataTypeIndex = DataSourceValueTypeCache.TypeIndexMap[typeof(T).Name];

    static DataSourceValueTypeCache()
    {
        Initialize();
    }

    internal static void Initialize()
    {
        // This regex matches all functions and their parameters, critically, at top-level only - sub functions are part of parameter data expression,
        // see Expresso 'Documentation/GrafanaFunctionsRegex.xso' for development details on regex
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