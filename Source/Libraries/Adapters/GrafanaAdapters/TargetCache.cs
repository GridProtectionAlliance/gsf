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
using System.Runtime.Caching;

namespace GrafanaAdapters
{
    // Usage Note: Each type T should be unique unless cache can be safely shared
    internal static class TargetCache<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly MemoryCache s_targetCache;

        static TargetCache()
        {
            s_targetCache = new MemoryCache($"GrafanaTargetCache-{typeof(T).Name}");
        }

        internal static T GetOrAdd(string target, Func<T> valueFactory)
        {
            Lazy<T> newValue = new Lazy<T>(valueFactory);
            Lazy<T> oldValue = s_targetCache.AddOrGetExisting(target, newValue, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(1.0D) }) as Lazy<T>;

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

        internal static bool GetLastAndUpdate(string target, out T oldValue, T newValue)
        {
            bool foundExisting = false;

            if (s_targetCache.Contains(target))
            {
                oldValue = (T)s_targetCache.Get(target);
                foundExisting = true;
            }
            else
            {
                oldValue = default(T);                
            }

            s_targetCache.Set(target, newValue, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(1.0D) });

            return foundExisting;
        }
    }
}
