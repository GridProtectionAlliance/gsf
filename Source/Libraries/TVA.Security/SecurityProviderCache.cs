//******************************************************************************************************
//  SecurityProviderCache.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
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
//  10/8/2012 - Danyelle Gilliam
//        Modified Header
//
//******************************************************************************************************


using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Web;

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
            private ISecurityProvider m_provider;
            private DateTime m_lastAccessed;

            /// <summary>
            /// Initializes a new instance of the <see cref="CacheContext"/> class.
            /// </summary>
            public CacheContext(ISecurityProvider provider)
            {
                m_provider = provider;
                m_lastAccessed = DateTime.Now;
            }

            /// <summary>
            /// Gets the <see cref="ISecurityProvider"/> managed by this <see cref="CacheContext"/>.
            /// </summary>
            public ISecurityProvider Provider
            {
                get
                {
                    m_lastAccessed = DateTime.Now;
                    return m_provider;
                }
            }

            /// <summary>
            /// Gets the <see cref="DateTime"/> of when the <see cref="Provider"/> was last accessed.
            /// </summary>
            public DateTime LastAccessed
            {
                get
                {
                    return m_lastAccessed;
                }
            }
        }

        // Constants

        /// <summary>
        /// Number of minutes upto which <see cref="ISecurityProvider"/>s are to be cached.
        /// </summary>
        private const int CachingTimeout = 20;

        #endregion

        #region [ Static ]

        // Static Fields
        private static bool s_threadPolicySet;
        private static IDictionary<string, CacheContext> s_cache;
        private static System.Timers.Timer s_cacheMonitorTimer;

        // Static Constructor
        static SecurityProviderCache()
        {
            // Initialize static variables.
            s_cache = new Dictionary<string, CacheContext>(StringComparer.CurrentCultureIgnoreCase);
            s_cacheMonitorTimer = new System.Timers.Timer(60000);
            s_cacheMonitorTimer.Elapsed += CacheMonitorTimer_Elapsed;
            s_cacheMonitorTimer.Start();
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
                if (principal != null)
                {
                    // The provider we're looking for is available to us via the current thread principal. This means
                    // that the current thread principal has already been set by a call to Current property setter.
                    return ((SecurityIdentity)principal.Identity).Provider;
                }
                else
                {
                    // Since the provider is not available to us through the current thread principal, we check to see 
                    // if it is available to us via one of the two caching mechanisms.
                    if (HttpContext.Current != null && HttpContext.Current.Session != null)
                    {
                        // Check session state.
                        ISecurityProvider provider = HttpContext.Current.Session[typeof(ISecurityProvider).Name] as ISecurityProvider;
                        if (provider == null)
                            return null;
                        else
                            return SetupPrincipal(provider, false);
                    }
                    else
                    {
                        // Check in-process memory.
                        CacheContext cache;
                        lock (s_cache)
                        {
                            s_cache.TryGetValue(Thread.CurrentPrincipal.Identity.Name, out cache);
                        }

                        if (cache == null)
                            return null;
                        else
                            return SetupPrincipal(cache.Provider, false);
                    }
                }
            }
            set
            {
                if (value != null)
                {
                    // Login - Setup security principal.
                    value.Initialize();
                    SetupPrincipal(value, false);
                    if (HttpContext.Current != null && HttpContext.Current.Session != null)
                        // Cache provider to session state.
                        HttpContext.Current.Session[typeof(ISecurityProvider).Name] = value;
                    else if (!string.IsNullOrEmpty(value.UserData.LoginID))
                        // Cache provider to in-process memory.
                        lock (s_cache)
                        {
                            s_cache[value.UserData.LoginID] = new CacheContext(value);
                        }
                }
                else
                {
                    // Logout - Restore original principal.
                    SecurityPrincipal principal = Thread.CurrentPrincipal as SecurityPrincipal;
                    if (principal == null)
                        return;

                    SecurityIdentity identity = (SecurityIdentity)principal.Identity;
                    SetupPrincipal(identity.Provider, true);

                    if (HttpContext.Current != null && HttpContext.Current.Session != null)
                        // Remove previously cached provider from session state.
                        HttpContext.Current.Session[typeof(ISecurityProvider).Name] = null;
                    else if (s_cache.ContainsKey(identity.Provider.UserData.LoginID))
                        // Remove previously cached provider from in-process memory.
                        lock (s_cache)
                        {
                            s_cache.Remove(identity.Provider.UserData.LoginID);
                        }
                }
            }
        }

        // Static Methods

        private static ISecurityProvider SetupPrincipal(ISecurityProvider provider, bool restore)
        {
            // Initialize the principal object.
            IPrincipal principal;
            if (restore)
                // Set principal to anonymous WindowsPrincipal.
                principal = new WindowsPrincipal(WindowsIdentity.GetAnonymous());
            else
                // Set principal to SecurityPrincipal.
                principal = new SecurityPrincipal(new SecurityIdentity(provider));

            // Setup the current thread principal.
            Thread.CurrentPrincipal = principal;
            if (!s_threadPolicySet)
            {
                AppDomain.CurrentDomain.SetThreadPrincipal(Thread.CurrentPrincipal);
                s_threadPolicySet = true;
            }

            // Setup ASP.NET remote user principal.
            if (HttpContext.Current != null)
                HttpContext.Current.User = Thread.CurrentPrincipal;

            return provider;
        }

        private static void CacheMonitorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (s_cache)
            {
                List<string> cacheKeys = new List<string>(s_cache.Keys);
                foreach (string cacheKey in cacheKeys)
                {
                    if (DateTime.Now.Subtract(s_cache[cacheKey].LastAccessed).TotalMinutes > CachingTimeout)
                        s_cache.Remove(cacheKey);
                }
            }
        }

        #endregion
    }
}
