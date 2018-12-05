//******************************************************************************************************
//  SecurityProviderCache.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/25/2010 - Pinal C. Patel
//       Generated original version of source code.
//  01/27/2011 - Pinal C. Patel
//       Updated SetupPrincipal() to call AppDomain.SetThreadPrincipal() to set the thread principal
//       in addition to settings the current thread principal so the thread principal is set correctly 
//       inside WPF applications.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  04/27/2016 - J. Ritchie Carroll 
//      Added ValidateCurrentProvider method to simplify CurrentProvider usage patten; code clean-up.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using GSF.Collections;
using GSF.Configuration;
using GSF.Threading;
using Timer = System.Timers.Timer;

namespace GSF.Security
{
    /// <summary>
    /// A helper class that manages the caching of <see cref="ISecurityProvider"/>s.
    /// </summary>
    public static class SecurityProviderCache
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// A class that facilitates the caching of <see cref="ISecurityProvider"/>.
        /// </summary>
        private class CacheContext
        {
            #region [ Members ]

            // Fields
            private readonly ISecurityProvider m_provider;
            private readonly WeakReference<ISecurityProvider> m_weakProvider;
            private DateTime m_lastRefreshTime;
            private DateTime m_lastAccessTime;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Initializes a new instance of the <see cref="CacheContext"/> class.
            /// </summary>
            public CacheContext(ISecurityProvider provider)
                : this(provider, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CacheContext"/> class.
            /// </summary>
            public CacheContext(WeakReference<ISecurityProvider> weakProvider)
                : this(null, weakProvider)
            {
            }

            private CacheContext(ISecurityProvider provider, WeakReference<ISecurityProvider> weakProvider)
            {
                DateTime now = DateTime.UtcNow;

                m_provider = provider;
                m_weakProvider = weakProvider;
                m_lastRefreshTime = now;
                m_lastAccessTime = now;
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets the <see cref="ISecurityProvider"/> managed by this <see cref="CacheContext"/>.
            /// </summary>
            public ISecurityProvider Provider
            {
                get
                {
                    ISecurityProvider provider;

                    m_lastAccessTime = DateTime.UtcNow;

                    if ((object)m_provider != null)
                        return m_provider;

                    if ((object)m_weakProvider != null && m_weakProvider.TryGetTarget(out provider))
                        return provider;

                    return null;
                }
            }

            /// <summary>
            /// Gets the <see cref="DateTime"/> of when the <see cref="CacheContext"/> was last refreshed.
            /// </summary>
            public DateTime LastRefreshTime => m_lastRefreshTime;

            /// <summary>
            /// Gets the <see cref="DateTime"/> of when the <see cref="CacheContext"/> was last accessed.
            /// </summary>
            public DateTime LastAccessTime => m_lastAccessTime;

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Refreshes the provider managed by this <see cref="CacheContext"/>.
            /// </summary>
            public bool Refresh()
            {
                ISecurityProvider provider = Provider;

                if ((object)provider == null)
                    return false;

                provider.RefreshData();
                provider.Authenticate();
                m_lastRefreshTime = DateTime.UtcNow;

                return true;
            }

            #endregion
        }

        // Constants

        /// <summary>
        /// Number of minutes up to which <see cref="ISecurityProvider"/>s are to be cached.
        /// </summary>
        private const int DefaultUserCacheTimeout = 5;

        /// <summary>
        /// Number of milliseconds between calls to monitor the cache.
        /// </summary>
        private const int CacheMonitorTimerInterval = 60000;

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Dictionary<string, CacheContext> s_cache;
        private static readonly List<CacheContext> s_autoRefreshProviders;
        private static readonly int s_userCacheTimeout;
        private static readonly Action s_cacheMonitorAction;

        // Static Constructor
        static SecurityProviderCache()
        {
            // Load settings from the specified category
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SecurityProviderBase.DefaultSettingsCategory];
            settings.Add("UserCacheTimeout", DefaultUserCacheTimeout, "Defines the timeout, in whole minutes, for a user's provider cache. Any value less than 1 will cause cache reset every minute.");

            s_userCacheTimeout = settings["UserCacheTimeout"].ValueAs(DefaultUserCacheTimeout);

            // Initialize static variables
            s_cache = new Dictionary<string, CacheContext>();
            s_autoRefreshProviders = new List<CacheContext>();

            s_cacheMonitorAction = new Action(ManageCachedCredentials);
            s_cacheMonitorAction.DelayAndExecute(CacheMonitorTimerInterval);
        }

        // Static Methods

        /// <summary>
        /// Creates a new provider from data cached by the <see cref="SecurityProviderCache"/>.
        /// </summary>
        /// <param name="username">The username of the user for which to create a new provider.</param>
        /// <returns>A new provider initialized from cached data.</returns>
        public static ISecurityProvider CreateProvider(string username)
        {
            CacheContext cacheContext;

            lock (s_cache)
            {
                cacheContext = s_cache.GetOrAdd(username, name => new CacheContext(SecurityProviderUtility.CreateProvider(username)));
            }

            ISecurityProvider provider = SecurityProviderUtility.CreateProvider(cacheContext.Provider.UserData);

            AutoRefresh(provider);

            return provider;
        }

        /// <summary>
        /// Removes any cached information about the user with the given username.
        /// </summary>
        /// <param name="username">The username of the user to be flushed from the cache.</param>
        public static void Flush(string username)
        {
            lock (s_cache)
            {
                s_cache.Remove(username);
            }
        }

        /// <summary>
        /// Adds the given provider to the collection of providers being automatically refreshed on the user cache timeout interval.
        /// </summary>
        /// <param name="provider">The security provider to be cached.</param>
        public static void AutoRefresh(ISecurityProvider provider)
        {
            lock (s_autoRefreshProviders)
            {
                WeakReference<ISecurityProvider> weakProvider = new WeakReference<ISecurityProvider>(provider);
                CacheContext cacheContext = new CacheContext(weakProvider);
                s_autoRefreshProviders.Add(cacheContext);
            }
        }

        /// <summary>
        /// Removes the given provider from the collection of providers being automatically refreshed.
        /// </summary>
        /// <param name="provider">The provider to be removed.</param>
        public static void DisableAutoRefresh(ISecurityProvider provider)
        {
            lock (s_autoRefreshProviders)
            {
                s_autoRefreshProviders.RemoveAll(cacheContext => provider.Equals(cacheContext.Provider));
            }
        }

        /// <summary>
        /// Forces all cached providers to refresh state.
        /// </summary>
        public static void RefreshAll()
        {
            lock (s_cache)
            {
                s_cache.Clear();
            }

            List<CacheContext> refreshedContexts = new List<CacheContext>();

            lock (s_autoRefreshProviders)
            {
                for (int i = s_autoRefreshProviders.Count - 1; i >= 0; i--)
                {
                    CacheContext cacheContext = s_autoRefreshProviders[i];

                    if ((object)cacheContext.Provider == null)
                    {
                        s_autoRefreshProviders[i] = s_autoRefreshProviders[s_autoRefreshProviders.Count - 1];
                        s_autoRefreshProviders.RemoveAt(s_autoRefreshProviders.Count - 1);
                    }
                    else
                    {
                        refreshedContexts.Add(cacheContext);
                    }
                }
            }

            // It's important to avoid calling refresh
            // inside the lock for performance reasons
            foreach (CacheContext cacheContext in refreshedContexts)
                cacheContext.Refresh();
        }

        private static void ManageCachedCredentials()
        {
            HashSet<CacheContext> refreshedContexts = new HashSet<CacheContext>();

            lock (s_cache)
            {
                foreach (string username in s_cache.Keys.ToList())
                {
                    CacheContext cacheContext = s_cache[username];

                    if (DateTime.UtcNow.Subtract(cacheContext.LastAccessTime).TotalMinutes > s_userCacheTimeout)
                        s_cache.Remove(username);
                    else
                        refreshedContexts.Add(s_cache[username]);
                }
            }

            lock (s_autoRefreshProviders)
            {
                for (int i = s_autoRefreshProviders.Count - 1; i >= 0; i--)
                {
                    CacheContext cacheContext = s_autoRefreshProviders[i];

                    if (DateTime.UtcNow.Subtract(cacheContext.LastRefreshTime).TotalMinutes > s_userCacheTimeout)
                    {
                        if ((object)cacheContext.Provider == null)
                        {
                            s_autoRefreshProviders[i] = s_autoRefreshProviders[s_autoRefreshProviders.Count - 1];
                            s_autoRefreshProviders.RemoveAt(s_autoRefreshProviders.Count - 1);
                        }
                        else
                        {
                            refreshedContexts.Add(cacheContext);
                        }
                    }
                }
            }

            // It's important to avoid calling refresh
            // inside the lock for performance reasons
            foreach (CacheContext cacheContext in refreshedContexts)
                cacheContext.Refresh();

            // The refresh could take several minutes so we should
            // wait to kick off the timer until after we are finished
            s_cacheMonitorAction.DelayAndExecute(CacheMonitorTimerInterval);
        }

        #endregion
    }
}
