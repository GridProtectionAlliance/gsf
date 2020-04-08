//******************************************************************************************************
//  WindowsUserInfo.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  08/27/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.Configuration;
using GSF.Diagnostics;
using GSF.Interop;
using GSF.IO;
using Microsoft.Win32;

#if !MONO
using System.DirectoryServices.AccountManagement;
#endif

namespace GSF.Identity
{
    // Windows implementation of key UserInfo class elements
    internal sealed class WindowsUserInfo : IUserInfo
    {
        #region [ Members ]

        // Constants
        private const string LogonDomainRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
        private const string LogonDomainRegistryValue = "DefaultDomainName";

        // Fields
        private readonly UserInfo m_parent;
        private DirectoryEntry m_userEntry;
        private bool m_domainRespondsForUser;
        private bool m_isLocalAccount;
        private bool m_useLegacyGroupLookups;
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        public WindowsUserInfo(UserInfo parent)
        {
            m_parent = parent;

            // Attempt to attach to power mode changed system event
            try
            {
                SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            }
            catch
            {
                // This event will not be raised when running from a Windows service since
                // a service does not establish a message loop.
            }

            // Attempt to get configuration flag for using legacy group lookups
            try
            {
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_parent.SettingsCategory];
                settings.Add("UseLegacyGroupLookups", m_useLegacyGroupLookups, "Flag that determines if group based lookups for local users should use legacy algorithm. Enabling may speed up authentication when using local accounts.");
                m_useLegacyGroupLookups = settings["UseLegacyGroupLookups"].ValueAs(m_useLegacyGroupLookups);
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex, "Failed while checking configuration for legacy group lookups");
            }
        }

        ~WindowsUserInfo()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        public bool DomainRespondsForUser
        {
            get
            {
                if (!m_initialized)
                {
                    // Attempt to initialize
                    try
                    {
                        Initialize();
                    }
                    catch (InitializationException)
                    {
                        // Initialization failures are due to domain not responding,
                        // member flag is set in Initialize() method.
                    }
                }

                return m_domainRespondsForUser;
            }
        }

        public bool Exists
        {
            get
            {
                bool exists = false;

                if (!m_initialized)
                {
                    // Attempt to initialize
                    try
                    {
                        Initialize();
                    }
                    catch (InitializationException)
                    {
                        // User could not be found - this could simply mean that ActiveDirectory is unavailable (e.g., laptop disconnected from the domain).
                        // In this case, if user logged in with cached credentials they are at least authenticated so we can assume that the user exists...
                        IPrincipal principal = m_parent.PassthroughPrincipal;
                        IIdentity identity = principal?.Identity ?? WindowsIdentity.GetCurrent();

                        exists =
                            (object)identity != null &&
                            !string.IsNullOrEmpty(m_parent.LoginID) &&
                            identity.Name.Equals(m_parent.LoginID, StringComparison.OrdinalIgnoreCase) &&
                            identity.IsAuthenticated;
                    }
                }

                if (!exists)
                {
                    if (m_isLocalAccount)
                    {
                        try
                        {
                            exists = DirectoryEntry.Exists("WinNT://" + m_parent.Domain + "/" + m_parent.UserName);
                        }
                        catch
                        {
                            exists = false;
                        }
                    }
                    else
                    {
                        exists = ((object)m_userEntry != null);
                    }
                }

                return exists;
            }
        }

        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        public DateTime LastLogon
        {
            get
            {
                if (m_enabled)
                {
                    try
                    {
                        if (m_isLocalAccount)
                            return DateTime.Parse(GetUserPropertyValue("lastLogin"));

                        return DateTime.FromFileTime(ConvertToLong(GetUserPropertyValueCollection("lastLogon").Value));
                    }
                    catch
                    {
                        return DateTime.MinValue;
                    }
                }

                return DateTime.MinValue;
            }
        }

        public DateTime AccountCreationDate
        {
            get
            {
                if (m_enabled)
                {
                    try
                    {
                        if (m_isLocalAccount)
                        {
                            string profilePath = GetUserPropertyValue("profile");

                            if (string.IsNullOrEmpty(profilePath) || !Directory.Exists(profilePath))
                            {
                                // Remove any trailing directory separator character from the file path.
                                string rootFolder = FilePath.AddPathSuffix(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                                string userFolder = FilePath.GetLastDirectoryName(rootFolder);
                                int folderLocation = rootFolder.LastIndexOf(userFolder, StringComparison.OrdinalIgnoreCase);

                                // Remove user profile name for current user (this class may be for user other than owner of current thread)                            
                                rootFolder = FilePath.AddPathSuffix(rootFolder.Substring(0, folderLocation));

                                // Create profile path for user referenced in this UserInfo class
                                profilePath = FilePath.AddPathSuffix(rootFolder + m_parent.UserName);
                            }

                            return Directory.GetCreationTime(profilePath);
                        }

                        return Convert.ToDateTime(GetUserPropertyValue("whenCreated"));
                    }
                    catch
                    {
                        return DateTime.MinValue;
                    }
                }

                return DateTime.MinValue;
            }
        }

        public DateTime NextPasswordChangeDate
        {
            get
            {
                DateTime passwordChangeDate = DateTime.MaxValue;

                if (m_enabled && !m_parent.PasswordCannotChange && !m_parent.PasswordDoesNotExpire)
                {
                    try
                    {
                        if (m_isLocalAccount)
                        {
                            Ticks maxPasswordTicksAge = MaximumPasswordAge;

                            if (maxPasswordTicksAge >= 0)
                            {
                                // WinNT properties are in seconds, not ticks
                                long passwordAge = long.Parse(GetUserPropertyValue("passwordAge"));
                                long maxPasswordAge = (long)maxPasswordTicksAge.ToSeconds();

                                if (passwordAge > maxPasswordAge || GetUserPropertyValue("passwordExpired").ParseBoolean())
                                {
                                    // User must change password on next logon.
                                    passwordChangeDate = DateTime.UtcNow;
                                }
                                else
                                {
                                    // User must change password periodically.
                                    passwordChangeDate = DateTime.UtcNow.AddSeconds(maxPasswordAge - passwordAge);
                                }
                            }
                        }
                        else
                        {
                            long passwordSetOn = ConvertToLong(GetUserPropertyValueCollection("pwdLastSet").Value);

                            if (passwordSetOn == 0)
                            {
                                // User must change password on next logon.
                                passwordChangeDate = DateTime.UtcNow;
                            }
                            else
                            {
                                // User must change password periodically.
                                long maxPasswordAge = MaximumPasswordAge;

                                // Ignore extremes
                                if (maxPasswordAge != long.MaxValue && maxPasswordAge > 0)
                                    passwordChangeDate = DateTime.FromFileTime(passwordSetOn).AddDays(TimeSpan.FromTicks(maxPasswordAge).Duration().Days);
                            }
                        }
                    }
                    catch
                    {
                        return DateTime.MaxValue;
                    }
                }

                return passwordChangeDate;
            }
        }

        public int LocalUserAccountControl
        {
            get
            {
                int userAccountControl = -1;
                string userPropertyValue;

                if (m_enabled && m_isLocalAccount)
                {
                    userPropertyValue = GetUserPropertyValue("userFlags");

                    if (string.IsNullOrEmpty(userPropertyValue))
                        throw new SecurityException(string.Format(UserInfo.SecurityExceptionFormat, "Local", UserInfo.CurrentUserID, "local machine accounts"));

                    if (!int.TryParse(userPropertyValue, out userAccountControl))
                        throw new InvalidOperationException(string.Format(UserInfo.UnknownErrorFormat, m_parent.LoginID, userPropertyValue));
                }

                return userAccountControl;
            }
        }

        public Ticks MaximumPasswordAge
        {
            get
            {
                if (m_enabled)
                {
                    string maxAgePropertyValue = string.Empty;

                    if (m_isLocalAccount)
                    {
                        maxAgePropertyValue = GetUserPropertyValue("maxPasswordAge");
                    }
                    else
                    {
                        WindowsImpersonationContext currentContext = null;
                        try
                        {
                            currentContext = m_parent.ImpersonatePrivilegedAccount();
                            using (DirectorySearcher searcher = CreateDirectorySearcher())
                            {
                                SearchResult searchResult = searcher.FindOne();
                                if ((object)searchResult != null && searchResult.Properties.Contains("maxPwdAge"))
                                    maxAgePropertyValue = searchResult.Properties["maxPwdAge"][0].ToString();
                            }
                        }
                        finally
                        {
                            UserInfo.EndImpersonation(currentContext);
                        }
                    }

                    if (string.IsNullOrEmpty(maxAgePropertyValue))
                        return -1;

                    long maxPasswordAge = long.Parse(maxAgePropertyValue);

                    if (m_isLocalAccount)
                        return Ticks.FromSeconds(maxPasswordAge);

                    return maxPasswordAge;
                }

                return -1;
            }
        }

        public string[] Groups
        {
            get
            {
            #if !MONO
                if (!m_enabled)
                    return Array.Empty<string>();
                
                if (m_isLocalAccount && m_useLegacyGroupLookups)
                    return OldGetGroups();

                try
                {
                    using (PrincipalContext context = m_isLocalAccount ? new PrincipalContext(ContextType.Machine) : new PrincipalContext(ContextType.Domain, m_parent.Domain))
                    using (UserPrincipal principal = UserPrincipal.FindByIdentity(context, m_parent.UserName))
                    {
                        return principal.GetAuthorizationGroups()
                            .Select(groupPrincipal => groupPrincipal.Sid.ToString())
                            .Select(SIDToAccountName)
                            .ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Log.Publish(MessageLevel.Error, "Get Groups", ex.Message, null, ex);

                    // TODO: Fix random, inexplicable errors when looking up
                    // identities via System.DirectoryServices.AccountManagement
                    return OldGetGroups();
                }
            #else
                return OldGetGroups();
            #endif
            }
        }

        public string[] LocalGroups
        {
            get
            {
            #if !MONO
                if (!m_enabled)
                    return Array.Empty<string>();

                if (m_isLocalAccount && m_useLegacyGroupLookups)
                    return OldGetLocalGroups();

                try
                {
                    using (PrincipalContext context = m_isLocalAccount ? new PrincipalContext(ContextType.Machine) : new PrincipalContext(ContextType.Domain, m_parent.Domain))
                    using (UserPrincipal principal = UserPrincipal.FindByIdentity(context, m_parent.UserName))
                    {
                        return principal.GetAuthorizationGroups()
                            .Cast<GroupPrincipal>()
                            .Where(groupPrincipal => groupPrincipal.GroupScope == GroupScope.Local)
                            .Select(groupPrincipal => groupPrincipal.Sid.ToString())
                            .Select(SIDToAccountName)
                            .ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Log.Publish(MessageLevel.Error, "Get LocalGroups", ex.Message, null, ex);

                    // TODO: Fix random, inexplicable errors when looking up
                    // identities via System.DirectoryServices.AccountManagement
                    return OldGetLocalGroups();
                }
            #else
                return OldGetLocalGroups();
            #endif
            }
        }

        public string FullLocalUserName
        {
            get
            {
                string name = GetUserPropertyValue("fullName");

                if (string.IsNullOrEmpty(name))
                    name = GetUserPropertyValue("Name");

                return name;
            }
        }

        public bool IsLocalAccount
        {
            get
            {
                return m_isLocalAccount;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="WindowsUserInfo"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="WindowsUserInfo"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        if ((object)m_userEntry != null)
                            m_userEntry.Dispose();

                        m_userEntry = null;

                        // Attempt to detach from power mode changed system event
                        try
                        {
                            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
                        }
                        catch
                        {
                            // This event is not raised when running from a Windows service since
                            // a service does not establish a message loop.
                        }
                    }
                }
                finally
                {
                    m_enabled = false;  // Mark as disabled.
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        public bool Initialize()
        {
            if (!m_initialized)
            {
                // Load settings from config file.
                m_parent.LoadSettings();

                // Handle initialization
                m_enabled = false;

                // Attempt do derive the domain if one is not specified
                if (string.IsNullOrEmpty(m_parent.Domain))
                {
                    if (!string.IsNullOrEmpty(m_parent.m_privilegedDomain))
                    {
                        // Use domain specified for privileged account
                        m_parent.Domain = m_parent.m_privilegedDomain;
                    }
                    else
                    {
                        // Attempt to use the default logon domain of the host machine. Note that this key will not exist on machines
                        // that do not connect to a domain and the Environment.UserDomainName property will return the machine name.
                        m_parent.Domain = Registry.GetValue(LogonDomainRegistryKey, LogonDomainRegistryValue, Environment.UserDomainName).ToString();
                    }
                }

                // Set the domain as the local machine if one is not specified
                if (string.IsNullOrEmpty(m_parent.Domain))
                    m_parent.Domain = Environment.MachineName;

                // WinNT directory entries will only resolve "BUILTIN" local prefixes with a "."
                if (m_parent.Domain.Equals("BUILTIN", StringComparison.OrdinalIgnoreCase))
                    m_parent.Domain = ".";

                // Determine if "domain" is for local machine or active directory
                if (UserInfo.IsLocalDomain(m_parent.Domain))
                {
                    try
                    {
                        // Initialize the directory entry object used to retrieve local account information
                        m_userEntry = new DirectoryEntry("WinNT://" + m_parent.Domain + "/" + m_parent.UserName);
                        m_isLocalAccount = true;
                        m_enabled = true;
                        m_parent.UserAccountControl = -1;

                        if (m_parent.Domain.Equals("NT SERVICE", StringComparison.OrdinalIgnoreCase) || m_parent.Domain.Equals("NT AUTHORITY", StringComparison.OrdinalIgnoreCase))
                            m_domainRespondsForUser = true;
                        else
                            m_domainRespondsForUser = DirectoryEntry.Exists("WinNT://" + m_parent.Domain + "/" + m_parent.UserName);
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        if ((object)m_userEntry != null)
                            m_userEntry.Dispose();

                        m_userEntry = null;
                        m_domainRespondsForUser = false;

                        throw new InitializationException(string.Format("Failed to initialize directory entry for user '{0}'", m_parent.LoginID), ex);
                    }
                }
                else
                {
                    // Initialize the directory entry object used to retrieve active directory information
                    WindowsImpersonationContext currentContext = null;

                    try
                    {
                        // Impersonate to the privileged account if specified
                        currentContext = m_parent.ImpersonatePrivilegedAccount();

                        // Initialize the Active Directory searcher object
                        using (DirectorySearcher searcher = CreateDirectorySearcher())
                        {
                            searcher.Filter = "(SAMAccountName=" + m_parent.UserName + ")";
                            SearchResult result = searcher.FindOne();
                            if ((object)result != null)
                                m_userEntry = result.GetDirectoryEntry();
                        }

                        m_isLocalAccount = false;
                        m_enabled = true;
                        m_domainRespondsForUser = true;
                        m_parent.UserAccountControl = -1;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        if ((object)m_userEntry != null)
                            m_userEntry.Dispose();

                        m_userEntry = null;
                        m_domainRespondsForUser = false;

                        throw new InitializationException(string.Format("Failed to initialize directory entry for domain user '{0}'", m_parent.LoginID), ex);
                    }
                    finally
                    {
                        // Undo impersonation if it was performed
                        UserInfo.EndImpersonation(currentContext);
                    }
                }

                // Initialize user information only once
                m_initialized = true;
            }

            return m_initialized;
        }

        // If the system is waking back up from a sleep state, this class should reinitialize in case
        // user is no longer connected to a domain
        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                m_initialized = false;
        }

        public void ChangePassword(string oldPassword, string newPassword)
        {
            m_userEntry.Invoke("ChangePassword", oldPassword, newPassword);

            // Commit changes (required for non-local accounts)
            if (!m_isLocalAccount)
                m_userEntry.CommitChanges();
        }

        public PropertyValueCollection GetUserPropertyValueCollection(string propertyName)
        {
            WindowsImpersonationContext currentContext = null;

            try
            {
                // Initialize if uninitialized
                Initialize();

                // Quit if disabled
                if (!m_enabled)
                    return null;

                if ((object)m_userEntry != null)
                {
                    // Impersonate to the privileged account if specified
                    currentContext = m_parent.ImpersonatePrivilegedAccount();

                    // Return requested Active Directory property value
                    return m_userEntry.Properties[propertyName];
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                // Undo impersonation if it was performed
                UserInfo.EndImpersonation(currentContext);
            }
        }

        public string GetUserPropertyValue(string propertyName)
        {
            PropertyValueCollection value = GetUserPropertyValueCollection(propertyName);

            if ((object)value != null && value.Count > 0)
                return value[0].ToString().Replace("  ", " ").Trim();

            return string.Empty;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private DirectorySearcher CreateDirectorySearcher()
        {
            DirectorySearcher searcher;

            if (string.IsNullOrEmpty(m_parent.LdapPath))
                searcher = new DirectorySearcher();
            else
                searcher = new DirectorySearcher(new DirectoryEntry(m_parent.LdapPath));

            return searcher;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private long ConvertToLong(object largeInteger)
        {
            Type type = largeInteger.GetType();

            int highPart = (int)type.InvokeMember("HighPart", BindingFlags.GetProperty, null, largeInteger, null);
            int lowPart = (int)type.InvokeMember("LowPart", BindingFlags.GetProperty, null, largeInteger, null);

            return (long)highPart << 32 | (uint)lowPart;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly string[] s_builtInLocalGroups;

        // Static constructor
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static WindowsUserInfo()
        {
            // Determine built-in group list - this is not expected to change so it is statically cached
            List<string> builtInGroups = new List<string>();

            WellKnownSidType[] builtInSids =
            {
                WellKnownSidType.BuiltinAccountOperatorsSid,
                WellKnownSidType.BuiltinAdministratorsSid,
                WellKnownSidType.BuiltinAuthorizationAccessSid,
                WellKnownSidType.BuiltinBackupOperatorsSid,
                WellKnownSidType.BuiltinDomainSid,
                WellKnownSidType.BuiltinGuestsSid,
                WellKnownSidType.BuiltinIncomingForestTrustBuildersSid,
                WellKnownSidType.BuiltinNetworkConfigurationOperatorsSid,
                WellKnownSidType.BuiltinPerformanceLoggingUsersSid,
                WellKnownSidType.BuiltinPerformanceMonitoringUsersSid,
                WellKnownSidType.BuiltinPowerUsersSid,
                WellKnownSidType.BuiltinPreWindows2000CompatibleAccessSid,
                WellKnownSidType.BuiltinPrintOperatorsSid,
                WellKnownSidType.BuiltinRemoteDesktopUsersSid,
                WellKnownSidType.BuiltinReplicatorSid,
                WellKnownSidType.BuiltinSystemOperatorsSid,
                WellKnownSidType.BuiltinUsersSid
            };

            SecurityIdentifier securityIdentifier;
            NTAccount groupAccount;

            foreach (WellKnownSidType builtInSid in builtInSids)
            {
                try
                {
                    // Attempt to translate well-known SID to a local NT group - if this fails, local group is not defined
                    securityIdentifier = new SecurityIdentifier(builtInSid, null);
                    groupAccount = (NTAccount)securityIdentifier.Translate(typeof(NTAccount));

                    // Don't include "BUILTIN\" prefix for group names so they are easily comparable
                    builtInGroups.Add(groupAccount.ToString().Substring(8));
                }
                catch (IdentityNotMappedException ex)
                {
                    Logger.SwallowException(ex, "WindowsUserInfo.cs Static Constructor: Failed to lookup identity", builtInSid.ToString());
                }
            }

            // Sort list so binary search can be used
            builtInGroups.Sort(StringComparer.OrdinalIgnoreCase);

            s_builtInLocalGroups = builtInGroups.ToArray();
        }

        // Static Properties
        public static bool MachineIsJoinedToDomain
        {
            get
            {
                using (ManagementObject wmi = new ManagementObject(string.Format("Win32_ComputerSystem.Name='{0}'", Environment.MachineName)))
                {
                    wmi.Get();
                    return (bool)wmi["PartOfDomain"];
                }
            }
        }

        // Static Methods
        public static string[] GetBuiltInLocalGroups()
        {
            return s_builtInLocalGroups;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static IPrincipal AuthenticateUser(string domain, string userName, string password, out string errorMessage)
        {
            errorMessage = null;
            IntPtr tokenHandle = IntPtr.Zero;

            try
            {
                // Call Win32 LogonUser method.
                if (WindowsApi.LogonUser(userName, domain, password, WindowsApi.LOGON32_LOGON_NETWORK, WindowsApi.LOGON32_PROVIDER_DEFAULT, out tokenHandle))
                {
                    // Create a windows principal of the authenticated user.
                    return new WindowsPrincipal(new WindowsIdentity(tokenHandle));
                }
                else
                {
                    // Get the error encountered when authenticating the user.
                    errorMessage = WindowsApi.GetLastErrorMessage();
                    return null;
                }
            }
            finally
            {
                // Free the token.
                if (tokenHandle != IntPtr.Zero)
                    WindowsApi.CloseHandle(tokenHandle);
            }

            #region [ Alternate User Authentication Method ]

            // This requires reference to System.DirectoryServices.AccountManagement.
            // Note that these account management methods are not enabled under Mono...

            //// Attempt do derive the domain if one is not specified
            //if (string.IsNullOrEmpty(domain))
            //{
            //    // Attempt to use the default logon domain of the host machine. Note that this key will not exist on machines
            //    // that do not connect to a domain and the Environment.UserDomainName property will return the machine name.
            //    domain = Registry.GetValue(LogonDomainRegistryKey, LogonDomainRegistryValue, Environment.UserDomainName).ToString();
            //}

            //// Set the domain as the local machine if one is not specified
            //if (string.IsNullOrEmpty(domain))
            //    domain = Environment.MachineName;

            //using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domain))
            //{
            //    if (context.ValidateCredentials(username, password, ContextOptions.Negotiate))
            //    {
            //        using (UserPrincipal principal = new UserPrincipal(context, username, password, true))
            //        {
            //            return new WindowsPrincipal(new WindowsIdentity(principal.UserPrincipalName));
            //        }
            //    }

            //    errorMessage = string.Format("Failed to authenticate {0}\\{1}", domain, username);
            //}

            //return null;

            #endregion
        }

        public static WindowsImpersonationContext ImpersonateUser(string domain, string userName, string password)
        {
            WindowsImpersonationContext impersonatedUser;

            IntPtr userTokenHandle = IntPtr.Zero;
            IntPtr duplicateTokenHandle = IntPtr.Zero;

            try
            {
                // Calls LogonUser to obtain a handle to an access token.
                if (!WindowsApi.LogonUser(userName, domain, password, WindowsApi.LOGON32_LOGON_INTERACTIVE, WindowsApi.LOGON32_PROVIDER_DEFAULT, out userTokenHandle))
                    throw new InvalidOperationException(string.Format("Failed to impersonate user \"{0}\\{1}\": {2}", domain, userName, WindowsApi.GetLastErrorMessage()));

                if (!WindowsApi.DuplicateToken(userTokenHandle, WindowsApi.SECURITY_IMPERSONATION, ref duplicateTokenHandle))
                    throw new InvalidOperationException(string.Format("Failed to impersonate user \"{0}\\{1}\" - exception thrown while trying to duplicate token: {2}", domain, userName, WindowsApi.GetLastErrorMessage()));

                // The token that is passed into WindowsIdentity must be a primary token in order to use it for impersonation
                impersonatedUser = WindowsIdentity.Impersonate(duplicateTokenHandle);
            }
            finally
            {
                // Free the tokens
                if (userTokenHandle != IntPtr.Zero)
                    WindowsApi.CloseHandle(userTokenHandle);

                if (duplicateTokenHandle != IntPtr.Zero)
                    WindowsApi.CloseHandle(duplicateTokenHandle);
            }

            return impersonatedUser;
        }

        // Determines if local account (e.g., user or group) exists using an existing directory entry
        private static bool LocalAccountExists(DirectoryEntry localMachine, string accountName, string schemaType, bool allowActiveDirectoryAccount, out DirectoryEntry accountEntry)
        {
            if (allowActiveDirectoryAccount && accountName.Contains("\\"))
            {
                string[] accountParts = accountName.Split('\\');

                if (accountParts.Length == 2)
                {
                    string domain = accountParts[0].Trim();
                    string account = accountParts[1].Trim();

                    // Check for non-local domain name (don't check for those like NT SERVICE, WinNT:// works with those)
                    if (!string.IsNullOrEmpty(domain) &&
                        string.Compare(domain, ".", StringComparison.OrdinalIgnoreCase) != 0 &&
                        string.Compare(domain, Environment.MachineName, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        // AD accounts that exist in local groups can be referenced by name
                        accountEntry = new DirectoryEntry(string.Format("WinNT://{0}/{1}", domain, account));
                        return true;
                    }

                    // Remove domain prefix for local accounts
                    accountName = account;
                }
            }

            try
            {
                accountEntry = localMachine.Children.Find(accountName, schemaType);
                return true;
            }
            catch (COMException)
            {
                accountEntry = null;
                return false;
            }
        }

        public static bool LocalUserExists(string userName)
        {
            // Create a directory entry for the local machine
            using (DirectoryEntry localMachine = new DirectoryEntry("WinNT://.,computer", null, null, AuthenticationTypes.Secure))
            {
                DirectoryEntry userEntry;

                // Determine if local user exists
                bool userExists = LocalAccountExists(localMachine, userName, "user", false, out userEntry);

                if ((object)userEntry != null)
                    userEntry.Dispose();

                return userExists;
            }
        }

        public static bool CreateLocalUser(string userName, string password, string userDescription)
        {
            // Create a directory entry for the local machine
            using (DirectoryEntry localMachine = new DirectoryEntry("WinNT://.,computer", null, null, AuthenticationTypes.Secure))
            {
                DirectoryEntry userEntry = null;

                try
                {
                    // Determine if local user exists
                    if (!LocalAccountExists(localMachine, userName, "user", false, out userEntry))
                    {
                        using (DirectoryEntry newUserEntry = localMachine.Children.Add(userName, "user"))
                        {
                            newUserEntry.Invoke("SetPassword", new object[] { password });
                            newUserEntry.Invoke("Put", new object[] { "Description", userDescription ?? "Local account for " + userName });
                            newUserEntry.CommitChanges();
                        }
                        return true;
                    }
                }
                catch (COMException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot create local user \"{0}\": {1}", userName, ex.Message), ex);
                }
                catch (TargetInvocationException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot create local user \"{0}\": {1}", userName, ex.InnerException.Message), ex.InnerException);
                }
                finally
                {
                    if ((object)userEntry != null)
                        userEntry.Dispose();
                }
            }

            return false;
        }

        public static void SetLocalUserPassword(string userName, string password)
        {
            // Create a directory entry for the local machine
            using (DirectoryEntry localMachine = new DirectoryEntry("WinNT://.,computer", null, null, AuthenticationTypes.Secure))
            {
                DirectoryEntry userEntry = null;

                try
                {
                    // Determine if local user exists
                    if (!LocalAccountExists(localMachine, userName, "user", false, out userEntry))
                        throw new InvalidOperationException(string.Format("Cannot set password for local user \"{0}\": user does not exist.", userName));

                    userEntry.Invoke("SetPassword", new object[] { password });
                    userEntry.CommitChanges();
                }
                catch (COMException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot set password for local user \"{0}\": {1}", userName, ex.Message), ex);
                }
                catch (TargetInvocationException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot set password for local user \"{0}\": {1}", userName, ex.InnerException.Message), ex.InnerException);
                }
                finally
                {
                    if ((object)userEntry != null)
                        userEntry.Dispose();
                }
            }
        }

        public static bool RemoveLocalUser(string userName)
        {
            // Create a directory entry for the local machine
            using (DirectoryEntry localMachine = new DirectoryEntry("WinNT://.,computer", null, null, AuthenticationTypes.Secure))
            {
                DirectoryEntry userEntry = null;

                try
                {
                    // Determine if local user exists
                    if (LocalAccountExists(localMachine, userName, "user", false, out userEntry))
                    {
                        localMachine.Children.Remove(userEntry);
                        return true;
                    }
                }
                catch (COMException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot remove local user \"{0}\": {1}", userName, ex.Message), ex);
                }
                finally
                {
                    if ((object)userEntry != null)
                        userEntry.Dispose();
                }
            }

            return false;
        }

        public static bool LocalGroupExists(string groupName)
        {
            // Create a directory entry for the local machine
            using (DirectoryEntry localMachine = new DirectoryEntry("WinNT://.,computer", null, null, AuthenticationTypes.Secure))
            {
                DirectoryEntry groupEntry;

                // Determine if local group exists
                bool groupExists = LocalAccountExists(localMachine, groupName, "group", false, out groupEntry);

                if ((object)groupEntry != null)
                    groupEntry.Dispose();

                return groupExists;
            }
        }

        public static bool CreateLocalGroup(string groupName, string groupDescription)
        {
            // Create a directory entry for the local machine
            using (DirectoryEntry localMachine = new DirectoryEntry("WinNT://.,computer", null, null, AuthenticationTypes.Secure))
            {
                DirectoryEntry groupEntry = null;

                try
                {
                    // Determine if local group exists
                    if (!LocalAccountExists(localMachine, groupName, "group", false, out groupEntry))
                    {
                        using (DirectoryEntry newGroupEntry = localMachine.Children.Add(groupName, "group"))
                        {
                            newGroupEntry.Invoke("Put", new object[] { "Description", groupDescription ?? groupName + " group." });
                            newGroupEntry.CommitChanges();
                        }
                        return true;
                    }
                }
                catch (COMException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot create local group \"{0}\": {1}", groupName, ex.Message), ex);
                }
                catch (TargetInvocationException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot create local group \"{0}\": {1}", groupName, ex.InnerException.Message), ex.InnerException);
                }
                finally
                {
                    if ((object)groupEntry != null)
                        groupEntry.Dispose();
                }
            }

            return false;
        }

        public static bool RemoveLocalGroup(string groupName)
        {
            // Create a directory entry for the local machine
            using (DirectoryEntry localMachine = new DirectoryEntry("WinNT://.,computer", null, null, AuthenticationTypes.Secure))
            {
                DirectoryEntry groupEntry = null;

                try
                {
                    // Determine if local group exists
                    if (LocalAccountExists(localMachine, groupName, "group", false, out groupEntry))
                    {
                        localMachine.Children.Remove(groupEntry);
                        return true;
                    }
                }
                catch (COMException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot remove local group \"{0}\": {1}", groupName, ex.Message), ex);
                }
                finally
                {
                    if ((object)groupEntry != null)
                        groupEntry.Dispose();
                }
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.String.Compare(System.String,System.String,System.StringComparison)")]
        public static bool UserIsInLocalGroup(string groupName, string userName)
        {
            // Create a directory entry for the local machine
            using (DirectoryEntry localMachine = new DirectoryEntry("WinNT://.,computer", null, null, AuthenticationTypes.Secure))
            {
                DirectoryEntry groupEntry = null;
                DirectoryEntry userEntry = null;
                string userPath;

                try
                {
                    // Determine if local group exists
                    if (!LocalAccountExists(localMachine, groupName, "group", false, out groupEntry))
                        throw new InvalidOperationException(string.Format("Cannot determine if user \"{0}\" is in local group \"{1}\": group does not exist.", userName, groupName));

                    // Determine if local user exists
                    if (!LocalAccountExists(localMachine, userName, "user", true, out userEntry))
                        throw new InvalidOperationException(string.Format("Cannot determine if user \"{0}\" is in local group \"{1}\": user does not exist.", userName, groupName));

                    userPath = userEntry.Path;

                    // See if user is in group
                    foreach (object adsUser in (IEnumerable)groupEntry.Invoke("Members"))
                    {
                        using (DirectoryEntry groupUserEntry = new DirectoryEntry(adsUser))
                        {
                            if (string.Compare(groupUserEntry.Path, userPath, StringComparison.OrdinalIgnoreCase) == 0)
                                return true;
                        }
                    }
                }
                catch (COMException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot determine if user \"{0}\" is in local group \"{1}\": {2}", userName, groupName, ex.Message), ex);
                }
                catch (TargetInvocationException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot determine if user \"{0}\" is in local group \"{1}\": {2}", userName, groupName, ex.InnerException.Message), ex.InnerException);
                }
                finally
                {
                    if ((object)groupEntry != null)
                        groupEntry.Dispose();

                    if ((object)userEntry != null)
                        userEntry.Dispose();
                }
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.String.Compare(System.String,System.String,System.StringComparison)")]
        public static bool AddUserToLocalGroup(string groupName, string userName)
        {
            // Create a directory entry for the local machine
            using (DirectoryEntry localMachine = new DirectoryEntry("WinNT://.,computer", null, null, AuthenticationTypes.Secure))
            {
                DirectoryEntry groupEntry = null;
                DirectoryEntry userEntry = null;
                string userPath;

                try
                {
                    // Determine if local group exists
                    if (!LocalAccountExists(localMachine, groupName, "group", false, out groupEntry))
                        throw new InvalidOperationException(string.Format("Cannot add user \"{0}\" to local group \"{1}\": group does not exist.", userName, groupName));

                    // Determine if user exists
                    if (!LocalAccountExists(localMachine, userName, "user", true, out userEntry))
                        throw new InvalidOperationException(string.Format("Cannot add user \"{0}\" to local group \"{1}\": user does not exist.", userName, groupName));

                    userPath = userEntry.Path;

                    // See if user is already in group
                    foreach (object adsUser in (IEnumerable)groupEntry.Invoke("Members"))
                    {
                        using (DirectoryEntry groupUserEntry = new DirectoryEntry(adsUser))
                        {
                            // If user already exists in group, exit and return false
                            if (string.Compare(groupUserEntry.Path, userPath, StringComparison.OrdinalIgnoreCase) == 0)
                                return false;
                        }
                    }

                    // Add new user to group
                    groupEntry.Invoke("Add", new object[] { userPath });
                    return true;
                }
                catch (COMException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot add user \"{0}\" to local group \"{1}\": {2}", userName, groupName, ex.Message), ex);
                }
                catch (TargetInvocationException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot add user \"{0}\" to local group \"{1}\": {2}", userName, groupName, ex.InnerException.Message), ex.InnerException);
                }
                finally
                {
                    if ((object)groupEntry != null)
                        groupEntry.Dispose();

                    if ((object)userEntry != null)
                        userEntry.Dispose();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison", MessageId = "System.String.Compare(System.String,System.String,System.StringComparison)")]
        public static bool RemoveUserFromLocalGroup(string groupName, string userName)
        {
            // Create a directory entry for the local machine
            using (DirectoryEntry localMachine = new DirectoryEntry("WinNT://.,computer", null, null, AuthenticationTypes.Secure))
            {
                DirectoryEntry groupEntry = null;
                DirectoryEntry userEntry = null;
                string userPath;

                try
                {
                    // Determine if local group exists
                    if (!LocalAccountExists(localMachine, groupName, "group", false, out groupEntry))
                        throw new InvalidOperationException(string.Format("Cannot remove user \"{0}\" from local group \"{1}\": group does not exist.", userName, groupName));

                    // Determine if user exists
                    if (!LocalAccountExists(localMachine, userName, "user", true, out userEntry))
                        throw new InvalidOperationException(string.Format("Cannot remove user \"{0}\" from local group \"{1}\": user does not exist.", userName, groupName));

                    userPath = userEntry.Path;

                    // See if user is in group
                    foreach (object adsUser in (IEnumerable)groupEntry.Invoke("Members"))
                    {
                        using (DirectoryEntry groupUserEntry = new DirectoryEntry(adsUser))
                        {
                            // If user exists in group, remove user and return true
                            if (string.Compare(groupUserEntry.Path, userPath, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                groupEntry.Invoke("Remove", new object[] { userPath });
                                return true;
                            }
                        }
                    }
                }
                catch (COMException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot remove user \"{0}\" from local group \"{1}\": {2}", userName, groupName, ex.Message), ex);
                }
                catch (TargetInvocationException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot remove user \"{0}\" from local group \"{1}\": {2}", userName, groupName, ex.InnerException.Message), ex.InnerException);
                }
                finally
                {
                    if ((object)groupEntry != null)
                        groupEntry.Dispose();

                    if ((object)userEntry != null)
                        userEntry.Dispose();
                }
            }

            return false;
        }

        public static string[] GetLocalGroupUserList(string groupName)
        {
            List<string> userList = new List<string>();

            // Create a directory entry for the local machine
            using (DirectoryEntry localMachine = new DirectoryEntry("WinNT://.,computer", null, null, AuthenticationTypes.Secure))
            {
                DirectoryEntry groupEntry = null;

                try
                {
                    // Determine if local group exists
                    if (!LocalAccountExists(localMachine, groupName, "group", false, out groupEntry))
                        throw new InvalidOperationException(string.Format("Cannot get members for local group \"{0}\": group does not exist.", groupName));

                    // See if user is in group
                    foreach (object adsUser in (IEnumerable)groupEntry.Invoke("Members"))
                    {
                        using (DirectoryEntry groupUserEntry = new DirectoryEntry(adsUser))
                        {
                            userList.Add(groupUserEntry.Name);
                        }
                    }
                }
                catch (COMException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot get members for local group \"{0}\": {1}", groupName, ex.Message), ex);
                }
                catch (TargetInvocationException ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot get members for local group \"{0}\": {1}", groupName, ex.InnerException.Message), ex.InnerException);
                }
                finally
                {
                    if ((object)groupEntry != null)
                        groupEntry.Dispose();
                }
            }

            return userList.ToArray();
        }

        public static string AccountNameToSID(string accountName)
        {
            string[] accountParts;
            NTAccount account;
            SecurityIdentifier securityIdentifier;

            string machineAlias;
            string machineDomain;
            string accountAlias;
            string accountAliasSID;

            if ((object)accountName == null)
                throw new ArgumentNullException(nameof(accountName));

            try
            {
                accountParts = accountName.Split('\\');

                if (accountParts.Length == 2)
                    account = new NTAccount(accountParts[0], accountParts[1]);
                else
                    account = new NTAccount(accountName);

                securityIdentifier = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));

                return securityIdentifier.ToString();
            }
            catch (IdentityNotMappedException ex)
            {
                Logger.SwallowException(ex, "WindowsUserInfo.cs AccountNameToSID: Error in mapping");
                machineAlias = @".\";
                machineDomain = Environment.MachineName + @"\";

                if (accountName.StartsWith(machineAlias, StringComparison.OrdinalIgnoreCase))
                {
                    // If a '.' is specified as the domain, treat it as an alias for the machine domain and attempt the lookup again
                    accountAlias = Regex.Replace(accountName, "^" + Regex.Escape(machineAlias), machineDomain, RegexOptions.IgnoreCase);
                    accountAliasSID = AccountNameToSID(accountAlias);

                    if (!ReferenceEquals(accountAlias, accountAliasSID))
                        return accountAliasSID;
                }
                else if (accountName.StartsWith(machineDomain, StringComparison.OrdinalIgnoreCase))
                {
                    // If the machine domain is specified, attempt the lookup again using the BUILTIN domain instead
                    accountAlias = Regex.Replace(accountName, "^" + Regex.Escape(machineDomain), @"BUILTIN\", RegexOptions.IgnoreCase);
                    accountAliasSID = AccountNameToSID(accountAlias);

                    if (!ReferenceEquals(accountAlias, accountAliasSID))
                        return accountAliasSID;
                }
            }
            catch (SystemException)
            {
            }

            return accountName;
        }

        public static string SIDToAccountName(string sid)
        {
            try
            {
                SecurityIdentifier securityIdentifier;
                NTAccount account;

                if ((object)sid == null)
                    throw new ArgumentNullException(nameof(sid));

                securityIdentifier = new SecurityIdentifier(CleanSid(sid));
                account = (NTAccount)securityIdentifier.Translate(typeof(NTAccount));

                return account.ToString();
            }
            catch (IdentityNotMappedException)
            {
            }
            catch (SystemException)
            {
            }

            return sid;
        }

        public static bool IsUserSID(string sid)
        {
            return IsSchemaSID(sid, "User");
        }

        public static bool IsGroupSID(string sid)
        {
            return IsSchemaSID(sid, "Group");
        }

        private static bool IsSchemaSID(string sid, string schemaClassName)
        {
            try
            {
                string accountName;

                if ((object)sid == null)
                    throw new ArgumentNullException(nameof(sid));

                sid = CleanSid(sid);

                if (!sid.StartsWith("S-", StringComparison.OrdinalIgnoreCase))
                    return false;

                accountName = SIDToAccountName(sid);

                using (DirectoryEntry entry = new DirectoryEntry(string.Format("WinNT://{0}", UserInfo.ValidateGroupName(accountName).Replace('\\', '/'))))
                {
                    return entry.SchemaClassName.Equals(schemaClassName, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (COMException)
            {
                return false;
            }
        }

        private static string CleanSid(string sid)
        {
            // When the same security database is being used in both Windows and Unix environments, SIDs could
            // have "user:" or "group:" prefix as needed by UnixUserInfo, under Windows we will ignore this
            if (sid.StartsWith("user:", StringComparison.OrdinalIgnoreCase) || sid.StartsWith("group:", StringComparison.OrdinalIgnoreCase))
                return sid.Substring(sid.IndexOf(':') + 1);

            return sid;
        }

        #region [ Old Group Lookup Functions ]

        private string[] OldGetGroups()
        {
            HashSet<string> groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string groupName;

            if (m_enabled)
            {
                if (m_isLocalAccount)
                {
                    // Get fixed list of BUILTIN local groups
                    string[] builtInGroups = UserInfo.GetBuiltInLocalGroups();

                    // Get local groups that local user is a member of
                    object localGroups = m_userEntry.Invoke("Groups");

                    foreach (object localGroup in (IEnumerable)localGroups)
                    {
                        using (DirectoryEntry groupEntry = new DirectoryEntry(localGroup))
                        {
                            groupName = groupEntry.Name;

                            if (Array.BinarySearch(builtInGroups, groupName, StringComparer.OrdinalIgnoreCase) < 0)
                                groups.Add(Environment.MachineName + "\\" + groupName);
                            else
                                groups.Add("BUILTIN\\" + groupName);
                        }
                    }

                    // Union this with a manual scan of local groups since "Groups" call will not derive
                    // "NT AUTHORITY\Authenticated Users" which will miss "BUILTIN\Users" for authenticated
                    // users. This will also catch other groups that may have been missed by "Groups" call.
                    groups.UnionWith(LocalGroups);
                }
                else
                {
                    // Get active directory groups that active directory user is a member of
                    m_userEntry.RefreshCache(new[] { "TokenGroups" });

                    foreach (byte[] sid in m_userEntry.Properties["TokenGroups"])
                    {
                        try
                        {
                            groupName = new SecurityIdentifier(sid, 0).Translate(typeof(NTAccount)).ToString();
                            groups.Add(groupName);
                        }
                        catch (IdentityNotMappedException)
                        {
                            // This might happen when AD server is not accessible.
                        }
                        catch (SystemException)
                        {
                            // Ignoring group SID's that fail to translate to an active AD group, for whatever reason
                        }
                    }

                    // Union this with local groups that active directory user is a member of. The
                    // "TokenGroups" call doesn't get all of these :-p, generally this call only
                    // returns some of the common "BUILTIN\*" groups.
                    groups.UnionWith(LocalGroups);
                }
            }

            return groups.ToArray();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private string[] OldGetLocalGroups()
        {
            List<string> groups = new List<string>();

            // Get local groups that user is a member of
            DirectoryEntry root = new DirectoryEntry("WinNT://.,computer", null, null, AuthenticationTypes.Secure);
            string userPath = string.Format("WinNT://{0}/{1}", m_parent.Domain, m_parent.UserName);
            string groupName;

            string[] builtInGroups = UserInfo.GetBuiltInLocalGroups();

            // Only enumerate groups
            root.Children.SchemaFilter.Add("Group");

            // Have to scan each local group for the AD user...
            foreach (DirectoryEntry groupEntry in root.Children)
            {
                if ((bool)groupEntry.Invoke("IsMember", new object[] { userPath }))
                {
                    groupName = groupEntry.Name;

                    if (Array.BinarySearch(builtInGroups, groupName, StringComparer.OrdinalIgnoreCase) < 0)
                        groups.Add(Environment.MachineName + "\\" + groupName);
                    else
                        groups.Add("BUILTIN\\" + groupName);
                }
            }

            return groups.ToArray();
        }

        private static readonly LogPublisher Log = Logger.CreatePublisher(typeof(WindowsUserInfo), MessageClass.Component);

        #endregion

        #endregion
    }
}
