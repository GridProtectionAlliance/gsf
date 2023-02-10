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
using System.Security.Principal;
using GSF.Collections;
using GSF.Configuration;
using GSF.Threading;

// ReSharper disable MethodOverloadWithOptionalParameter
namespace GSF.Security
{
    /// <summary>
    /// A helper class that manages the caching of <see cref="ISecurityProvider"/>s.
    /// </summary>
    public class SecurityProviderCache
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
            private bool m_disposed;

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
                LastRefreshTime = now;
                LastAccessTime = now;
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
                    LastAccessTime = DateTime.UtcNow;
                    return InternalProvider;
                }
            }

            /// <summary>
            /// Gets the <see cref="DateTime"/> of when the <see cref="CacheContext"/> was last refreshed.
            /// </summary>
            public DateTime LastRefreshTime { get; private set; }

            /// <summary>
            /// Gets the <see cref="DateTime"/> of when the <see cref="CacheContext"/> was last accessed.
            /// </summary>
            public DateTime LastAccessTime { get; private set; }

            // Gets the provider without updating LastAccessTime.
            private ISecurityProvider InternalProvider
            {
                get
                {
                    if (m_disposed)
                        return null;

                    if (m_provider is not null)
                        return m_provider;

                    if (m_weakProvider is not null && m_weakProvider.TryGetTarget(out ISecurityProvider provider))
                        return provider;

                    return null;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Refreshes the provider managed by this <see cref="CacheContext"/>.
            /// </summary>
            public bool Refresh()
            {
                try
                {
                    ISecurityProvider provider = InternalProvider;

                    if (provider is null)
                        return false;

                    if (provider.CanRefreshData)
                        provider.RefreshData();

                    provider.Authenticate();
                    LastRefreshTime = DateTime.UtcNow;

                    return true;
                }
                catch (ObjectDisposedException)
                {
                    m_disposed = true;
                    return false;
                }
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

        // Fields
        private readonly Dictionary<string, CacheContext> m_cache;
        private readonly List<CacheContext> m_autoRefreshProviders;
        private readonly int m_userCacheTimeout;
        private readonly Action m_cacheMonitorAction;
        private readonly string m_settingsCategory;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates a new <see cref="SecurityProviderCache"/>.
        /// </summary>
        /// <param name="settingsCategory">Settings category.</param>
        public SecurityProviderCache(string settingsCategory)
        {
            // Load settings from the specified category
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[settingsCategory];
            settings.Add("UserCacheTimeout", DefaultUserCacheTimeout, "Defines the timeout, in whole minutes, for a user's provider cache. Any value less than 1 will cause cache reset every minute.");

            m_userCacheTimeout = settings["UserCacheTimeout"].ValueAs(DefaultUserCacheTimeout);

            // Initialize static variables
            m_cache = new Dictionary<string, CacheContext>();
            m_autoRefreshProviders = new List<CacheContext>();

            m_cacheMonitorAction = ManageCachedCredentials;
            m_cacheMonitorAction.DelayAndExecute(CacheMonitorTimerInterval);

            m_settingsCategory = settingsCategory;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a new provider from data cached by the <see cref="SecurityProviderCache"/>.
        /// </summary>
        /// <param name="username">The username of the user for which to create a new provider.</param>
        /// <param name="passthroughPrincipal"><see cref="IPrincipal"/> obtained through alternative authentication mechanisms to provide authentication for the <see cref="ISecurityProvider"/>.</param>
        /// <param name="autoRefresh">Indicates whether the provider should be automatically refreshed on a timer.</param>
        /// <returns>A new provider initialized from cached data.</returns>
        public ISecurityProvider CreateProvider(string username, IPrincipal passthroughPrincipal = null, bool autoRefresh = true)
        {
            CacheContext cacheContext;

            lock (m_cache)
                cacheContext = m_cache.GetOrAdd(username, _ => new CacheContext(SecurityProviderUtility.CreateProvider(username, passthroughPrincipal, m_settingsCategory)));

            ISecurityProvider provider = SecurityProviderUtility.CreateProvider(cacheContext.Provider.UserData, m_settingsCategory);
            provider.PassthroughPrincipal = passthroughPrincipal;

            if (autoRefresh)
                AutoRefresh(provider);

            return provider;
        }

        /// <summary>
        /// Removes any cached information about the user with the given username.
        /// </summary>
        /// <param name="username">The username of the user to be flushed from the cache.</param>
        public void Flush(string username)
        {
            lock (m_cache)
                m_cache.Remove(username);
        }

        /// <summary>
        /// Adds the given provider to the collection of providers being automatically refreshed on the user cache timeout interval.
        /// </summary>
        /// <param name="provider">The security provider to be cached.</param>
        public void AutoRefresh(ISecurityProvider provider)
        {
            lock (m_autoRefreshProviders)
            {
                WeakReference<ISecurityProvider> weakProvider = new(provider);
                CacheContext cacheContext = new(weakProvider);
                m_autoRefreshProviders.Add(cacheContext);
            }
        }

        /// <summary>
        /// Removes the given provider from the collection of providers being automatically refreshed.
        /// </summary>
        /// <param name="provider">The provider to be removed.</param>
        public void DisableAutoRefresh(ISecurityProvider provider)
        {
            lock (m_autoRefreshProviders)
                m_autoRefreshProviders.RemoveAll(cacheContext => provider.Equals(cacheContext.Provider));
        }

        /// <summary>
        /// Forces all cached providers to refresh state.
        /// </summary>
        public void RefreshAll()
        {
            lock (m_cache)
                m_cache.Clear();

            List<CacheContext> refreshedContexts = new();

            lock (m_autoRefreshProviders)
            {
                for (int i = m_autoRefreshProviders.Count - 1; i >= 0; i--)
                {
                    CacheContext cacheContext = m_autoRefreshProviders[i];

                    if (cacheContext.Provider is null)
                    {
                        m_autoRefreshProviders[i] = m_autoRefreshProviders[m_autoRefreshProviders.Count - 1];
                        m_autoRefreshProviders.RemoveAt(m_autoRefreshProviders.Count - 1);
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

        private void ManageCachedCredentials()
        {
            DateTime now = DateTime.UtcNow;

            void refreshCache()
            {
                // ReSharper disable once RedundantAssignment
                var cachedProviders = Enumerable.Empty<object>()
                    .Select(_ => new { Username = "", Context = (CacheContext)null })
                    .ToList();

                lock (m_cache)
                {
                    foreach (KeyValuePair<string, CacheContext> kvp in m_cache.ToList())
                    {
                        string username = kvp.Key;
                        CacheContext context = kvp.Value;

                        // Because CacheContext.LastAccessTime is used to purge
                        // records from s_cache, it is important here not to invoke
                        // the CacheContext.Provider property on any contexts in s_cache
                        if (now.Subtract(context.LastAccessTime).TotalMinutes >= m_userCacheTimeout)
                            m_cache.Remove(username);
                    }

                    cachedProviders = m_cache
                        .Select(kvp => new { Username = kvp.Key, Context = kvp.Value })
                        .Where(obj => now.Subtract(obj.Context.LastRefreshTime).TotalMinutes >= m_userCacheTimeout)
                        .ToList();
                }

                // It is important to avoid calling CacheContext.Refresh()
                // inside a lock for performance reasons
                var purgedProviders = cachedProviders
                    .Where(obj => !obj.Context.Refresh())
                    .ToList();

                lock (m_cache)
                {
                    // CacheContext.Refresh() failed due to an ObjectDisposedException, so it
                    // needs to be purged from the cache to prevent cached data from growing stale
                    foreach (var obj in purgedProviders)
                    {
                        if (m_cache.TryGetValue(obj.Username, out CacheContext cachedContext) && ReferenceEquals(obj.Context, cachedContext))
                            m_cache.Remove(obj.Username);
                    }
                }
            }

            void refreshAutoRefreshProviders()
            {
                List<CacheContext> autoRefreshProviders;

                lock (m_autoRefreshProviders)
                {
                    // It is okay to access CacheContext.Provider here because
                    // CacheContext.LastAccessTime is not used to purge auto refresh providers
                    bool shouldRemove(CacheContext context) =>
                        now.Subtract(context.LastRefreshTime).TotalMinutes >= m_userCacheTimeout &&
                        context.Provider is null;

                    m_autoRefreshProviders.RemoveWhere(shouldRemove);

                    autoRefreshProviders = m_autoRefreshProviders
                        .Where(context => now.Subtract(context.LastRefreshTime).TotalMinutes >= m_userCacheTimeout)
                        .ToList();
                }

                // It is important to avoid calling CacheContext.Refresh()
                // inside a lock for performance reasons; also, no need to
                // check the return value of CacheContext.Refresh() since the
                // logic above checks whether CacheContext.Provider is null
                foreach (CacheContext context in autoRefreshProviders)
                    context.Refresh();
            }

            refreshCache();
            refreshAutoRefreshProviders();

            // The refresh could take several minutes so we should
            // wait to kick off the timer until after we are finished
            m_cacheMonitorAction.DelayAndExecute(CacheMonitorTimerInterval);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly SecurityProviderCache s_primarySecurityProvider;
        private static readonly SecurityProviderCache s_alternateSecurityProvider;

        /// <summary>
        /// Specifies the default value for the SettingsCategory property for the AlternateSecurityProvider.
        /// </summary>
        public const string DefaultAlternateSettingsCategory = "AlternateSecurityProvider";

        // Static Constructor
        static SecurityProviderCache()
        {
            // Load Primary SecurityProvider
            s_primarySecurityProvider = new SecurityProviderCache(SecurityProviderBase.DefaultSettingsCategory);

            // Load Alternate Security Provider
            try
            {
                s_alternateSecurityProvider = new SecurityProviderCache(DefaultAlternateSettingsCategory);
            }
            catch
            {
                s_alternateSecurityProvider = null;
            }    
            
        }

        // Static Methods

        /// <summary>
        /// Creates a new provider from data cached by the <see cref="SecurityProviderCache"/>.
        /// </summary>
        /// <param name="username">The username of the user for which to create a new provider.</param>
        /// <param name="passthroughPrincipal"><see cref="IPrincipal"/> obtained through alternative authentication mechanisms to provide authentication for the <see cref="ISecurityProvider"/>.</param>
        /// <param name="autoRefresh">Indicates whether the provider should be automatically refreshed on a timer.</param>
        /// <param name="useAlternate">Indicates whether the alternate <see cref="ISecurityProvider"/> should be used.</param>
        /// <returns>A new provider initialized from cached data.</returns>
        public static ISecurityProvider CreateProvider(string username, IPrincipal passthroughPrincipal = null, bool autoRefresh = true, bool useAlternate = false) =>
            useAlternate && s_alternateSecurityProvider is not null ? 
                s_alternateSecurityProvider.CreateProvider(username, passthroughPrincipal, autoRefresh) : 
                s_primarySecurityProvider.CreateProvider(username, passthroughPrincipal, autoRefresh);

        /// <summary>
        /// Removes any cached information about the user with the given username.
        /// </summary>
        /// <param name="username">The username of the user to be flushed from the cache.</param>
        /// <param name="useAlternate">Indicates whether the alternate <see cref="ISecurityProvider"/> should be used.</param>
        public static void Flush(string username, bool useAlternate = false)
        {
            if (useAlternate && s_alternateSecurityProvider is not null)
                s_alternateSecurityProvider.Flush(username);
            else
                s_primarySecurityProvider.Flush(username);
        }

        /// <summary>
        /// Adds the given provider to the collection of providers being automatically refreshed on the user cache timeout interval.
        /// </summary>
        /// <param name="provider">The security provider to be cached.</param>
        /// <param name="useAlternate">Indicates whether the alternate <see cref="ISecurityProvider"/> should be used.</param>
        public static void AutoRefresh(ISecurityProvider provider, bool useAlternate = false)
        {
            if (useAlternate && s_alternateSecurityProvider is not null)
                s_alternateSecurityProvider.AutoRefresh(provider);
            else
                s_primarySecurityProvider.AutoRefresh(provider);
        }

        /// <summary>
        /// Removes the given provider from the collection of providers being automatically refreshed.
        /// </summary>
        /// <param name="provider">The provider to be removed.</param>
        /// <param name="useAlternate">Indicates whether the alternate <see cref="ISecurityProvider"/> should be used.</param>
        public static void DisableAutoRefresh(ISecurityProvider provider, bool useAlternate = false)
        {
            if (useAlternate && s_alternateSecurityProvider is not null)
                s_alternateSecurityProvider.DisableAutoRefresh(provider);
            else
                s_primarySecurityProvider.DisableAutoRefresh(provider);
        }

        /// <summary>
        /// Forces all cached providers to refresh state.
        /// </summary>
        /// <param name="useAlternate">Indicates whether the alternate <see cref="ISecurityProvider"/> should be used.</param>
        public static void RefreshAll(bool useAlternate = false)
        {
            if (useAlternate && s_alternateSecurityProvider is not null)
                s_alternateSecurityProvider.RefreshAll();
            else
                s_primarySecurityProvider.RefreshAll();              
        }

        #endregion
    }
}
