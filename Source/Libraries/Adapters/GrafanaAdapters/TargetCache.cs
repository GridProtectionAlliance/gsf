//******************************************************************************************************
//  TargetCache.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  10/02/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;
using GSF.Threading;

namespace GrafanaAdapters;

/// <summary>
/// Exposes a method to reinitialize sliding memory caches used by Grafana data sources.
/// </summary>
public static class TargetCaches
{
    // References each type T instance TargetCache ResetCache function
    internal static readonly List<Action> ResetCacheFunctions;

    private static readonly ShortSynchronizedOperation s_resetAllCaches;

    static TargetCaches()
    {
        ResetCacheFunctions = new List<Action>();

        s_resetAllCaches = new ShortSynchronizedOperation(() =>
        {
            Action[] resetCacheFunctions;

            lock (ResetCacheFunctions)
                resetCacheFunctions = ResetCacheFunctions.ToArray();

            foreach (Action resetCache in resetCacheFunctions)
                resetCache();
        });
    }

    /// <summary>
    /// Resets all sliding memory caches used by Grafana data sources.
    /// </summary>
    public static void ResetAll()
    {
        s_resetAllCaches.RunOnceAsync();
    }
}

// Usage Note: Each type T should be unique unless cache can be safely shared
internal static class TargetCache<T>
{
    // Desired use case is one static MemoryCache per type T:
    // ReSharper disable StaticMemberInGenericType
    private static readonly string s_cacheName;
    private static MemoryCache s_targetCache;

    static TargetCache()
    {
        s_cacheName = $"GrafanaTargetCache-{typeof(T).Name}";
        s_targetCache = new MemoryCache(s_cacheName);

        lock (TargetCaches.ResetCacheFunctions)
            TargetCaches.ResetCacheFunctions.Add(ResetCache);
    }

    internal static T GetOrAdd(string target, Func<T> valueFactory)
    {
        Lazy<T> newValue = new(valueFactory);
        Lazy<T> oldValue;

        try
        {
            // Race condition exists here such that target cache being referenced may
            // be disposed between access and method invocation - hence the try/catch
            oldValue = s_targetCache.AddOrGetExisting(target, newValue, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(1.0D) }) as Lazy<T>;
        }
        catch
        {
            oldValue = null;
        }

        try
        {
            return (oldValue ?? newValue).Value;
        }
        catch
        {
            s_targetCache.Remove(target);
            throw;
        }
    }

    internal static void ResetCache()
    {
        Interlocked.Exchange(ref s_targetCache, new MemoryCache(s_cacheName)).Dispose();
    }
}