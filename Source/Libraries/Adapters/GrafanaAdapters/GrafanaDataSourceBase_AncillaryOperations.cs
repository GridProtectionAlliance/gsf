//******************************************************************************************************
//  GrafanaDataSourceBase_AncillaryOperations.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  09/25/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable StaticMemberInGenericType

using GrafanaAdapters.DataSources;
using GrafanaAdapters.Functions;
using GrafanaAdapters.Model.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ProcessQueryRequestDelegate = System.Func<
    GrafanaAdapters.GrafanaDataSourceBase,
    GrafanaAdapters.Model.Common.QueryRequest,
    System.Threading.CancellationToken,
    System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<GrafanaAdapters.Model.Common.TimeSeriesValues>>>;


namespace GrafanaAdapters;

// Non-query specific Grafana functionality is defined here
partial class GrafanaDataSourceBase
{
    private static ProcessQueryRequestDelegate[] s_processQueryRequestFunctions;
    private static Regex s_selectExpression;

    // Gets array of functions used to process query requests per data source value type, each value in the array
    // is at the same index as the data source value type in the DataSourceCache.DataSourceValueTypes array
    private static ProcessQueryRequestDelegate[] ProcessQueryRequestFunctions =>
        s_processQueryRequestFunctions ??= CreateProcessQueryRequestFunctions();

    /// <summary>
    /// Reloads data source value types cache.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This function is used to support dynamic data source value type loading.
    /// Function would only need to be called when a new data source value is added
    /// to Grafana at run-time and user wanted to use new installed data source
    /// value type without restarting host.
    /// </para>
    /// <para>
    /// Suggest making this option available via web-based endpoint for administrators.
    /// </para>
    /// </remarks>
    public void ReloadDataSourceValueTypes()
    {
        DataSourceValueCache.ReloadDataSourceValueTypes();
        s_processQueryRequestFunctions = null;
    }

    /// <summary>
    /// Reloads Grafana functions cache.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This function is used to support dynamic loading for Grafana functions.
    /// Function would only need to be called when a new function is added to Grafana at
    /// run-time and user wanted to use new installed function without restarting host.
    /// </para>
    /// <para>
    /// Suggest making this option available via web-based endpoint for administrators.
    /// </para>
    /// </remarks>
    public void ReloadGrafanaFunctions()
    {
        FunctionParsing.ReloadGrafanaFunctions();
    }

    private static int GetDataSourceValueTypeIndex(string dataType) =>
        DataSourceValueCache.TypeIndexMap.TryGetValue(dataType, out int index) ? index : -1;

    private static ProcessQueryRequestDelegate[] CreateProcessQueryRequestFunctions()
    {
        ProcessQueryRequestDelegate[] functions = new ProcessQueryRequestDelegate[DataSourceValueCache.TypeCache.Count];

        foreach (Type type in DataSourceValueCache.TypeCache)
        {
            string typeName = type.Name;

            // Get generic definition for ProcessQueryRequestAsync method
            MethodInfo genericMethod = typeof(GrafanaDataSourceBase).GetMethod(nameof(ProcessQueryRequestAsync), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(type);

            DynamicMethod dynamicMethod = new($"{nameof(ProcessQueryRequestAsync)}_{typeName}",
                typeof(Task<IEnumerable<TimeSeriesValues>>),
                new[] { typeof(GrafanaDataSourceBase), typeof(QueryRequest), typeof(CancellationToken) },
                typeof(GrafanaDataSourceBase));

            ILGenerator generator = dynamicMethod.GetILGenerator();

            // Load the first argument (GrafanaDataSourceBase) onto the stack
            generator.Emit(OpCodes.Ldarg_0);

            // Load the second argument (QueryRequest) onto the stack
            generator.Emit(OpCodes.Ldarg_1);

            // Load the third argument (CancellationToken) onto the stack
            generator.Emit(OpCodes.Ldarg_2);

            // Call the generic method
            generator.Emit(OpCodes.Call, genericMethod);

            // Return the result of the call
            generator.Emit(OpCodes.Ret);

            ProcessQueryRequestDelegate function = (ProcessQueryRequestDelegate)dynamicMethod.CreateDelegate(typeof(ProcessQueryRequestDelegate), null);

            // Make sure function index matches data source value type index
            int index = DataSourceValueCache.GetTypeIndex(typeName);

            Debug.Assert(index > -1, $"Failed to find data source value type index for \"{typeName}\"");

            functions[index] = function;
        }

        return functions;
    }
}