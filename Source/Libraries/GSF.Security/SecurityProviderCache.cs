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
using System.Diagnostics.CodeAnalysis;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading;
using System.Timers;
using System.Web;
using GSF.Configuration;
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
            private readonly ISecurityProvider m_provider;
            private readonly DateTime m_cacheCreationTime;

            /// <summary>
            /// Initializes a new instance of the <see cref="CacheContext"/> class.
            /// </summary>
            public CacheContext(ISecurityProvider provider)
            {
                m_provider = provider;
                m_cacheCreationTime = DateTime.UtcNow;
            }

            /// <summary>
            /// Gets the <see cref="ISecurityProvider"/> managed by this <see cref="CacheContext"/>.
            /// </summary>
            public ISecurityProvider Provider => m_provider;

            /// <summary>
            /// Gets the <see cref="DateTime"/> of when the <see cref="CacheContext"/> was created.
            /// </summary>
            public DateTime CacheCreationTime => m_cacheCreationTime;
        }

        // Constants

        /// <summary>
        /// Number of minutes up to which <see cref="ISecurityProvider"/>s are to be cached.
        /// </summary>
        private const int DefaultUserCacheTimeout = 5;

        #endregion

        #region [ Static ]

        // Static Fields
        private static bool s_threadPolicySet;
        private static readonly IDictionary<string, CacheContext> s_cache;
        private static readonly int s_userCacheTimeout;

        // Static Constructor
        static SecurityProviderCache()
        {
            // Load settings from the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SecurityProviderBase.DefaultSettingsCategory];
            settings.Add("UserCacheTimeout", DefaultUserCacheTimeout, "Defines the timeout, in whole minutes, for a user's provider cache. Any value less than 1 will cause cache reset every minute.");

            s_userCacheTimeout = settings["UserCacheTimeout"].ValueAs(DefaultUserCacheTimeout);

            // Initialize static variables.
            s_cache = new Dictionary<string, CacheContext>(StringComparer.OrdinalIgnoreCase);

            Timer cacheMonitorTimer = new Timer(60000);
            cacheMonitorTimer.Elapsed += CacheMonitorTimer_Elapsed;
            cacheMonitorTimer.Start();
        }

        // Static Properties

        /// <summary>
        /// Gets or sets the <see cref="ISecurityProvider"/> of the current user.
        /// </summary>
        public static ISecurityProvider CurrentProvider
        {
            get
            {
                // Logic behind caching of the provider:
                // - A provider is cached to session state data if the runtime is ASP.NET and if the session state 
                //   data is accessible. This would essentially mean that we're dealing with web sites or web services
                //   that are either SOAP ASMX services or WCF services hosted in ASP.NET compatibility mode.
                // - A provider is cached to in-process static memory if we don't have access to session state data. 
                //   This would essentially mean that we're either dealing with windows based application or WCF 
                //   service hosted inside ASP.NET runtime without compatibility mode enabled.
                SecurityPrincipal principal = Thread.CurrentPrincipal as SecurityPrincipal;

                // The provider we're looking for is available to us via the current thread principal. This means
                // that the current thread principal has already been set by a call to Current property setter.
                if ((object)principal != null)
                    return ((SecurityIdentity)principal.Identity).Provider;

                // Since the provider is not available to us through the current thread principal, we check to see 
                // if it is available to us via one of the two caching mechanisms.
                if ((object)HttpContext.Current != null && (object)HttpContext.Current.Session != null)
                {
                    // Check session state.
                    ISecurityProvider provider = HttpContext.Current.Session[typeof(ISecurityProvider).Name] as ISecurityProvider;

                    if ((object)provider == null)
                        return null;

                    return SetupPrincipal(provider, false);
                }

                // Check in-process memory.
                CacheContext cache;

                lock (s_cache)
                    s_cache.TryGetValue(Thread.CurrentPrincipal.Identity.Name, out cache);

                if ((object)cache == null)
                    return null;

                return SetupPrincipal(cache.Provider, false);
            }
            set
            {
                if ((object)value != null)
                {
                    // Login - Setup security principal.
                    value.Initialize();

                    SetupPrincipal(value, false);

                    // Cache provider to session state.
                    if ((object)HttpContext.Current != null && (object)HttpContext.Current.Session != null)
                    {
                        HttpContext.Current.Session[typeof(ISecurityProvider).Name] = value;
                    }
                    else if (!string.IsNullOrEmpty(value.UserData.LoginID))
                    {
                        // Cache provider to in-process memory.
                        lock (s_cache)
                            s_cache[value.UserData.LoginID] = new CacheContext(value);
                    }
                }
                else
                {
                    // Logout - Restore original principal.
                    SecurityPrincipal principal = Thread.CurrentPrincipal as SecurityPrincipal;

                    if ((object)principal == null)
                        return;

                    SecurityIdentity identity = (SecurityIdentity)principal.Identity;
                    SetupPrincipal(identity.Provider, true);

                    if ((object)HttpContext.Current != null && (object)HttpContext.Current.Session != null)
                    {
                        // Remove previously cached provider from session state.
                        HttpContext.Current.Session[typeof(ISecurityProvider).Name] = null;
                    }
                    else
                    {
                        lock (s_cache)
                        {
                            // Remove previously cached provider from in-process memory if it already exists
                            if (s_cache.ContainsKey(identity.Provider.UserData.LoginID))
                                s_cache.Remove(identity.Provider.UserData.LoginID);
                        }
                    }
                }
            }
        }

        // Static Methods

        /// <summary>
        /// Validates that current provider is ready, creating it if necessary.
        /// </summary>
        /// <param name="username">User name of the user for whom the<see cref= "ISecurityProvider" /> is to be created; defaults to current user.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        public static void ValidateCurrentProvider(string username = null)
        {
            // Initialize the security principal from caller's windows identity if uninitialized, note that
            // simply by checking current provider any existing cached security principal will be restored,
            // if no current provider exists we create a new one
            if ((object)CurrentProvider == null)
            {
                lock (typeof(SecurityProviderCache))
                {
                    // Let's see if we won the race...
                    if ((object)CurrentProvider == null)
                        CurrentProvider = SecurityProviderUtility.CreateProvider(username);
                }
            }
        }

        /// <summary>
        /// Attempts to get cached <see cref="ISecurityProvider"/> for the given <paramref name="username"/>.
        /// </summary>
        /// <param name="username">Name of the user.</param>
        /// <param name="provider">Security provider to return.</param>
        /// <returns>True if provider is cached; false otherwise.</returns>
        public static bool TryGetCachedProvider(string username, out ISecurityProvider provider)
        {
            CacheContext cacheContext;
            bool result;

            lock (s_cache)
                result = s_cache.TryGetValue(username, out cacheContext);

            provider = result ? cacheContext.Provider : null;

            return result;
        }

        /// <summary>
        /// Attempts to reauthenticate the current thread principal
        /// after their provider has been removed from the cache.
        /// </summary>
        /// <returns>True if the user successfully reauthenticated; false otherwise.</returns>
        public static bool ReauthenticateCurrentPrincipal()
        {
            IPrincipal currentPrincipal;
            SecurityIdentity identity;
            ISecurityProvider provider = null;
            string password = null;
            bool authenticated;

            currentPrincipal = Thread.CurrentPrincipal;

            if ((object)currentPrincipal == null)
                return false;

            identity = currentPrincipal.Identity as SecurityIdentity;

            if ((object)identity != null)
                provider = identity.Provider;

            if ((object)provider != null)
                password = provider.Password;

            // Reset the current principal
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            Thread.CurrentPrincipal = new WindowsPrincipal(currentIdentity);

            // Create a new provider associated with current identity
            provider = SecurityProviderUtility.CreateProvider(currentPrincipal.Identity.Name);

            // Re-authenticate user
            authenticated = provider.Authenticate(password);

            // Re-cache current provider for user
            CurrentProvider = provider;

            return authenticated;
        }

        private static ISecurityProvider SetupPrincipal(ISecurityProvider provider, bool restore)
        {
            // Initialize the principal object.
            IPrincipal principal;

            if (restore)
            {
                // Set principal to anonymous WindowsPrincipal.
                principal = new WindowsPrincipal(WindowsIdentity.GetAnonymous());
            }
            else
            {
                // Set principal to SecurityPrincipal.
                principal = new SecurityPrincipal(new SecurityIdentity(provider));
            }

            // Setup the current thread principal.
            Thread.CurrentPrincipal = principal;

            if (!s_threadPolicySet)
            {
                try
                {
                    AppDomain.CurrentDomain.SetThreadPrincipal(Thread.CurrentPrincipal);
                }
                catch (PolicyException)
                {
                    // Can't set default domain thread principal twice
                }

                s_threadPolicySet = true;
            }

            // Setup ASP.NET remote user principal.
            if ((object)HttpContext.Current != null)
                HttpContext.Current.User = Thread.CurrentPrincipal;

            return provider;
        }

        private static void CacheMonitorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (s_cache)
            {
                List<string> cacheKeys = new List<string>(s_cache.Keys);

                foreach (string cacheKey in cacheKeys)
                {
                    CacheContext cache = s_cache[cacheKey];

                    if (DateTime.UtcNow.Subtract(cache.CacheCreationTime).TotalMinutes > s_userCacheTimeout)
                    {
                        if ((object)cache.Provider != null && (object)cache.Provider.UserData != null)
                            cache.Provider.UserData.IsAuthenticated = false;

                        s_cache.Remove(cacheKey);
                    }
                }
            }
        }

        #endregion
    }
}
