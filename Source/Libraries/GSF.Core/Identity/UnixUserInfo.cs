//******************************************************************************************************
//  UnixUserInfo.cs - Gbtc
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

// Define USE_SHARED_OBJECT to use GSF.POSIX.so shared object library for unmanaged functions
// Undefine USE_SHARED_OBJECT to use internally linked unmanaged functions (e.g., Mono hosted gsf service)

#define USE_SHARED_OBJECT
// #undef USE_SHARED_OBJECT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using GSF.Annotations;
using GSF.Collections;
using GSF.Configuration;
using GSF.Console;
using GSF.Units;
using Novell.Directory.Ldap;

#pragma warning disable 0649

namespace GSF.Identity
{
    // Unix implementation of key UserInfo class elements
    internal sealed class UnixUserInfo : IUserInfo
    {
        #region [ Members ]

        // Nested Types

        // ReSharper disable UnusedMember.Local
        [Serializable]
        private class UnixIdentity : WindowsIdentity
        {
            #region [ Members ]

            // Fields
            private readonly string m_domain;
            private readonly string m_userName;
            private LdapConnection m_connection;
            private string m_providerType;
            private string m_ldapRoot;
            private readonly bool m_loadedUserPasswordInformation;
            private readonly UserPasswordInformation m_userPasswordInformation;
            private readonly AccountStatus m_accountStatus;

            #endregion

            #region [ Constructors ]

            public UnixIdentity(string domain, string userName, LdapConnection connection = null)
                : base(GetUserIDAsToken(domain, userName), null, WindowsAccountType.Normal, true)
            {
                m_domain = domain;
                m_userName = userName;
                Connection = connection;

                if (UserInfo.IsLocalDomain(domain))
                {
                    // Cache shadow information for local user before possible reduction in privileges
                    if (GetLocalUserPasswordInformation(userName, out m_userPasswordInformation, out m_accountStatus) == 0)
                        m_loadedUserPasswordInformation = true;
                    else
                        m_accountStatus = AccountStatus.Disabled;
                }
            }

            #endregion

            #region [ Properties ]

            public string Domain
            {
                get
                {
                    return m_domain;
                }
            }

            public string UserName
            {
                get
                {
                    return m_userName;
                }
            }

            public string LoginID
            {
                get
                {
                    return string.Format("{0}\\{1}", m_domain, m_userName);
                }
            }

            public string ProviderType
            {
                get
                {
                    return m_providerType;
                }
            }

            public string LdapRoot
            {
                get
                {
                    return m_ldapRoot;
                }
            }

            public LdapConnection Connection
            {
                get
                {
                    return m_connection;
                }
                set
                {
                    m_connection = value;
                    m_providerType = (object)m_connection == null ? "PAM_LOCAL" : "PAM_LDAP";

                    if ((object)m_connection != null)
                    {
                        // Extract LDAP search root distinguished name
                        StringBuilder ldapRoot = new StringBuilder();
                        string[] elements = m_connection.GetSchemaDN().Split(',');

                        for (int i = 0; i < elements.Length; i++)
                        {
                            string element = elements[i].Trim();

                            if (element.StartsWith("DC", StringComparison.OrdinalIgnoreCase))
                            {
                                if (ldapRoot.Length > 0)
                                    ldapRoot.Append(',');

                                ldapRoot.Append(element);
                            }
                        }

                        m_ldapRoot = ldapRoot.ToString();
                    }
                    else
                    {
                        m_ldapRoot = "";
                    }
                }
            }

            public bool LoadedUserPasswordInformation
            {
                get
                {
                    return m_loadedUserPasswordInformation;
                }
            }

            public UserPasswordInformation UserPasswordInformation
            {
                get
                {
                    return m_userPasswordInformation;
                }
            }

            public AccountStatus AccountStatus
            {
                get
                {
                    return m_accountStatus;
                }
            }

            #endregion

            #region [ Static ]

            private static IntPtr GetUserIDAsToken(string domain, string userName)
            {
                uint userID;

                if (!UserInfo.IsLocalDomain(domain))
                    userName = string.Format("{0}\\{1}", domain, userName);

                return GetLocalUserID(userName, out userID) == 0 ? new IntPtr(userID) : IntPtr.Zero;
            }

            #endregion
        }

        private struct UserPasswordInformation
        {
            [UsedImplicitly]
            public long lastChangeDate;
            [UsedImplicitly]
            public long minDaysForChange;
            [UsedImplicitly]
            public long maxDaysForChange;
            [UsedImplicitly]
            public long warningDays;
            [UsedImplicitly]
            public long inactivityDays;
            [UsedImplicitly]
            public long accountExpirationDate;
        }

        private struct UserPasswordInformation32
        {
            [UsedImplicitly]
            public int lastChangeDate;
            [UsedImplicitly]
            public int minDaysForChange;
            [UsedImplicitly]
            public int maxDaysForChange;
            [UsedImplicitly]
            public int warningDays;
            [UsedImplicitly]
            public int inactivityDays;
            [UsedImplicitly]
            public int accountExpirationDate;
        }

        private enum AccountStatus
        {
            [UsedImplicitly]
            Normal,     // 0: Normal - encrypted password
            [UsedImplicitly]
            Disabled,   // 1: Password is *
            [UsedImplicitly]
            LockedOut,  // 2: Password starts with !
            [UsedImplicitly]
            NoPassword  // 3: Password is not defined (blank)
        }

        private enum PAMResponseCode
        {
            PAM_SYSTEM_ERR = 4,
            PAM_BUF_ERR = 5,
            PAM_PERM_DENIED = 7,
            PAM_MAXTRIES = 8,
            PAM_AUTH_ERR = 9,
            PAM_CRED_INSUFFICIENT = 11,
            PAM_AUTHINFO_UNAVAIL = 12,
            PAM_USER_UNKNOWN = 13,
            PAM_AUTHTOK_ERR = 20,
            PAM_AUTHTOK_RECOVERY_ERR = 21,
            PAM_AUTHTOK_LOCK_BUSY = 22,
            PAM_AUTHTOK_DISABLE_AGING = 23,
            PAM_ABORT = 26,
            PAM_TRY_AGAIN = 27
        }

        // Constants
        private const int MaxAccountNameLength = 256;

        // DllImport code is in GSF.POSIX.c
#if USE_SHARED_OBJECT
        private const string ImportFileName = "./GSF.POSIX.so";
#else
        private const string ImportFileName = "__Internal";
#endif

        // Fields
        private readonly UserInfo m_parent;
        private LdapConnection m_connection;
        private LdapEntry m_userEntry;
        private string m_ldapRoot;
        private bool m_domainRespondsForUser;
        private bool m_isLocalAccount;
        private bool m_enabled;
        private bool m_initialized;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public UnixUserInfo(UserInfo parent)
        {
            m_parent = parent;
        }

        ~UnixUserInfo()
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
                    if (IsLocalAccount)
                    {
                        try
                        {
                            exists = LocalUserExists(m_parent.UserName);
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
                DateTime lastLogon = DateTime.MinValue;

                if (m_enabled)
                {
                    try
                    {
                        CommandResponse response = Command.Execute("lastlog", "-u " + EncodeAccountName(m_isLocalAccount ? m_parent.UserName : m_parent.LoginID));

                        if (response.ExitCode == 0)
                        {
                            string[] lines = response.StandardOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                            if (lines.Length < 2 || lines[1].Length < 44 || !DateTime.TryParseExact(lines[1].Substring(43), "ddd MMM d HH:mm:ss zzff yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowInnerWhite, out lastLogon))
                                lastLogon = DateTime.MinValue;
                        }
                        else
                        {
                            if (!m_isLocalAccount)
                                lastLogon = DateTime.FromFileTime(long.Parse(GetUserPropertyValue("lastLogon")));
                        }
                    }
                    catch
                    {
                        lastLogon = DateTime.MinValue;
                    }
                }

                return lastLogon;
            }
        }

        public DateTime AccountCreationDate
        {
            get
            {
                DateTime creationDate = DateTime.MinValue;

                if (m_enabled)
                {
                    try
                    {
                        creationDate = Directory.GetCreationTime(Environment.GetEnvironmentVariable("HOME") ?? EncodeAccountName("/home/" + m_parent.UserName));
                    }
                    catch
                    {
                        if (!m_isLocalAccount || !DateTime.TryParseExact(GetUserPropertyValue("whenCreated"), "yyyyMMddHHmmss.fZ", CultureInfo.InvariantCulture, DateTimeStyles.AllowInnerWhite | DateTimeStyles.AssumeUniversal, out creationDate))
                            creationDate = DateTime.MinValue;
                    }
                }

                return creationDate;
            }
        }

        public DateTime NextPasswordChangeDate
        {
            get
            {
                DateTime passwordChangeDate = DateTime.MaxValue;

                if (m_enabled)
                {
                    if (IsLocalAccount)
                    {
                        UserPasswordInformation userPasswordInfo;
                        AccountStatus status;

                        if (GetCachedLocalUserPasswordInformation(m_parent.PassthroughPrincipal, m_isLocalAccount ? m_parent.UserName : m_parent.LoginID, out userPasswordInfo, out status) == 0)
                        {
                            if (userPasswordInfo.maxDaysForChange >= 99999)
                            {
                                passwordChangeDate = DateTime.MaxValue;
                            }
                            else
                            {
                                // From chage.c source:
                                //   The password expiration date is determined from the last change
                                //   date plus the number of days the password is valid for.
                                UnixTimeTag expirationDate = new UnixTimeTag((uint)(userPasswordInfo.lastChangeDate + userPasswordInfo.maxDaysForChange));
                                passwordChangeDate = expirationDate.ToDateTime();
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            long passwordSetOn = long.Parse(GetUserPropertyValue("pwdLastSet"));

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
                        catch
                        {
                            passwordChangeDate = DateTime.MaxValue;
                        }
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

                if (m_enabled && IsLocalAccount)
                {
                    UserPasswordInformation userPasswordInfo;
                    AccountStatus status;

                    if (GetCachedLocalUserPasswordInformation(m_parent.PassthroughPrincipal, m_parent.UserName, out userPasswordInfo, out status) == 0)
                    {
                        userAccountControl = 0;

                        // Assuming Linux account "disabled" when the password has expired
                        if (status == AccountStatus.Disabled)
                            userAccountControl |= UserInfo.ACCOUNTDISABLED;

                        // Assuming disabled accounts are effectively locked out as well
                        if (status == AccountStatus.LockedOut || status == AccountStatus.Disabled)
                            userAccountControl |= UserInfo.LOCKED;

                        if (userPasswordInfo.accountExpirationDate < 0 || userPasswordInfo.maxDaysForChange >= 99999)
                            userAccountControl |= UserInfo.DONT_EXPIRE_PASSWORD;
                    }
                }

                return userAccountControl;
            }
        }

        public Ticks MaximumPasswordAge
        {
            get
            {
                long maxPasswordAge = -1;

                if (m_enabled)
                {
                    if (IsLocalAccount)
                    {
                        UserPasswordInformation userPasswordInfo;
                        AccountStatus status;

                        if (GetCachedLocalUserPasswordInformation(m_parent.PassthroughPrincipal, m_parent.UserName, out userPasswordInfo, out status) == 0 && userPasswordInfo.maxDaysForChange < 99999)
                            maxPasswordAge = Ticks.FromSeconds(userPasswordInfo.maxDaysForChange * Time.SecondsPerDay);
                    }
                    else
                    {
                        try
                        {
                            maxPasswordAge = long.Parse(GetUserPropertyValue("maxPwdAge"));
                        }
                        catch
                        {
                            maxPasswordAge = -1;
                        }
                    }

                }

                return maxPasswordAge;
            }
        }

        public string[] Groups
        {
            get
            {
                if (!m_isLocalAccount)
                {
                    // Domain accounts are not case sensitive even though local account names are
                    HashSet<string> groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    string[] dnGroups = GetUserPropertyValueCollection("memberOf");

                    if ((object)dnGroups != null && dnGroups.Length > 0)
                    {
                        string ldapRoot, cn, dc;

                        if (string.IsNullOrEmpty(m_ldapRoot))
                            ldapRoot = m_parent.Domain;
                        else
                            ldapRoot = ParseDNTokens(m_ldapRoot, "DC");

                        // Convert LDAP distinguished names to AD style group names
                        foreach (string dnGroup in dnGroups)
                        {
                            try
                            {
                                // Lookup group entry by its distinguished name
                                LdapEntry groupEntry = m_connection.Read(dnGroup);

                                // Read the SAMAccountName attribute value
                                string group = groupEntry.getAttributeSet().getAttribute("sAMAccountName").StringValue;

                                if (!string.IsNullOrEmpty(group))
                                    groups.Add(string.Format("{0}\\{1}", m_parent.Domain, group));
                            }
                            catch
                            {
                                // This option is not as accurate
                                cn = ParseDNTokens(dnGroup, "CN");
                                dc = ParseDNTokens(dnGroup, "DC");

                                // If group domain matches LDAP root, assume this is a valid group
                                if (dc.Equals(ldapRoot, StringComparison.OrdinalIgnoreCase))
                                    groups.Add(string.Format("{0}\\{1}", m_parent.Domain, cn));
                            }
                        }
                    }

                    groups.UnionWith(LocalGroups);

                    return groups.ToArray();
                }

                return LocalGroups;
            }
        }

        public string[] LocalGroups
        {
            get
            {
                return GetLocalUserGroups(m_isLocalAccount ? m_parent.UserName : m_parent.LoginID);

                #region [ Possible Alternate Implementation ]

                //CommandResponse response = Command.Execute("groups", EncodeAccountName(m_isLocalAccount ? m_parent.UserName : m_parent.LoginID));

                //if (response.ExitCode == 0)
                //    return response.StandardOutput.Substring(m_parent.UserName.Length + 3).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                //return new string[0];

                #endregion
            }
        }

        public string FullLocalUserName
        {
            get
            {
                string userName = PtrToString(GetLocalUserGecos(m_isLocalAccount ? m_parent.UserName : m_parent.LoginID));

                if (string.IsNullOrWhiteSpace(userName))
                    return m_parent.UserName;

                if (userName.Contains(","))
                    userName = userName.Split(',')[0];

                return userName;
            }
        }

        public bool IsLocalAccount
        {
            get
            {
                return (m_isLocalAccount || (object)m_userEntry == null);
            }
        }

        #endregion

        #region [ Methods ]

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        m_userEntry = null;
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

                UnixIdentity unixIdentity = GetUnixIdentity();

                // Set the domain as the local machine if one is not specified
                if (string.IsNullOrEmpty(m_parent.Domain))
                    m_parent.Domain = Environment.MachineName;

                // Determine if "domain" is for local machine or active directory
                if (UserInfo.IsLocalDomain(m_parent.Domain))
                {
                    uint userID;

                    // Determine if local user exists
                    if (GetLocalUserID(m_parent.UserName, out userID) == 0)
                    {
                        m_isLocalAccount = true;
                        m_enabled = true;
                        m_domainRespondsForUser = true;
                        m_parent.UserAccountControl = -1;
                    }
                    else
                    {
                        m_domainRespondsForUser = false;
                        throw new InitializationException(string.Format("Failed to retrieve local user info for '{0}'", m_parent.UserName));
                    }
                }
                else
                {
                    WindowsImpersonationContext currentContext = null;

                    // Initialize the LdapEntry object used to retrieve LDAP user attributes
                    try
                    {
                        // Impersonate to the privileged account if specified
                        currentContext = m_parent.ImpersonatePrivilegedAccount();

                        // If domain user has already been authenticated, we should already have an active LDAP connection
                        if ((object)unixIdentity != null && unixIdentity.LoginID.Equals(m_parent.LoginID, StringComparison.OrdinalIgnoreCase))
                        {
                            m_connection = unixIdentity.Connection;
                            m_ldapRoot = unixIdentity.LdapRoot ?? m_parent.Domain;
                        }

                        // If no LDAP connection is available, attempt anonymous binding - note that this has to be enabled on AD as it is not enabled by default
                        if ((object)m_connection == null)
                            unixIdentity = AttemptAnonymousBinding(unixIdentity);

                        if ((object)m_connection != null)
                        {
                            // Search for user by account name starting at root and moving through hierarchy recursively
                            LdapSearchResults results = m_connection.Search(
                                m_ldapRoot,
                                LdapConnection.SCOPE_SUB,
                                string.Format("(&(objectCategory=person)(objectClass=user)(sAMAccountName={0}))", m_parent.UserName),
                                null,
                                false);

                            if (results.hasMore())
                            {
                                m_userEntry = results.next();
                                m_isLocalAccount = false;
                                m_enabled = true;
                                m_domainRespondsForUser = true;
                                m_parent.UserAccountControl = -1;
                            }
                        }
                        else
                        {
                            // If PAM authentication succeeded but no LDAP connection can be found, we will attempt to only use PAM
                            if ((object)unixIdentity != null && unixIdentity.LoginID.Equals(m_parent.LoginID, StringComparison.OrdinalIgnoreCase))
                            {
                                m_isLocalAccount = false;
                                m_enabled = true;
                                m_domainRespondsForUser = true; // PAM may be enough...
                                m_parent.UserAccountControl = -1;
                            }
                            else
                            {
                                // See if initialization is for current user
                                WindowsIdentity identity = WindowsIdentity.GetCurrent();

                                if ((object)identity != null && identity.IsAuthenticated && (identity.Name.Equals(m_parent.LoginID, StringComparison.OrdinalIgnoreCase) || identity.Name.Equals(m_parent.UserName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    m_isLocalAccount = !identity.Name.Contains('\\');
                                    m_enabled = true;
                                    m_domainRespondsForUser = true;
                                    m_parent.UserAccountControl = -1;
                                }
                                else
                                {
                                    throw new InvalidOperationException("No valid LDAP connection was found or user is not authenticated");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        m_userEntry = null;
                        m_domainRespondsForUser = false;

                        throw new InitializationException(string.Format("Failed to initialize LDAP entry for domain user '{0}': {1}", m_parent.LoginID, ex.Message), ex);
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

        private UnixIdentity GetUnixIdentity()
        {
            // Attempt to pick up current user or impersonated user principal
            WindowsPrincipal principal = m_parent.PassthroughPrincipal as WindowsPrincipal;
            UnixIdentity unixIdentity = null;

            if ((object)principal != null)
                unixIdentity = principal.Identity as UnixIdentity;

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
                    if ((object)unixIdentity != null)
                    {
                        // Use domain of user authenticated on current thread
                        m_parent.Domain = unixIdentity.Domain;
                    }
                    else
                    {
                        // Attempt to use the current user's logon domain
                        WindowsIdentity identity = WindowsIdentity.GetCurrent();

                        if ((object)identity != null)
                        {
                            string accountName = identity.Name;

                            if (accountName.Contains('\\'))
                            {
                                string[] accountParts = accountName.Split('\\');

                                // User name is specified in 'domain\accountname' format
                                if (accountParts.Length == 2)
                                    m_parent.Domain = accountParts[0];
                            }
                            else if (accountName.Contains('@'))
                            {
                                string[] accountParts = accountName.Split('@');

                                // User name is specified in 'accountname@domain' format
                                if (accountParts.Length == 2)
                                    m_parent.Domain = accountParts[1];
                            }
                        }
                    }
                }
            }

            return unixIdentity;
        }

        private UnixIdentity AttemptAnonymousBinding(UnixIdentity unixIdentity)
        {
            try
            {
                // Try really hard to find a configured LDAP host
                string ldapHost = string.IsNullOrEmpty(m_parent.m_ldapPath) ? GetLdapHost() : m_parent.m_ldapPath;

                // If LDAP host cannot be determined, no LdapConnection can be established
                if (!string.IsNullOrEmpty(ldapHost))
                {
                    // Attempt LDAP account authentication                    
                    LdapConnection connection = new LdapConnection();

                    if (ldapHost.StartsWith("LDAP", StringComparison.OrdinalIgnoreCase))
                    {
                        Uri ldapURI = new Uri(ldapHost);
                        ldapHost = ldapURI.Host + (ldapURI.Port == 0 ? "" : ":" + ldapURI.Port);
                    }

                    // If host LDAP path contains suffixed port number (e.g., host:port), this will be preferred over specified 389 default
                    connection.Connect(ldapHost, 389);
                    connection.Bind(null, null);

                    if ((object)unixIdentity == null)
                    {
                        unixIdentity = new UnixIdentity(m_parent.Domain, m_parent.UserName, connection);
                        m_parent.PassthroughPrincipal = new WindowsPrincipal(unixIdentity);
                    }
                    else
                    {
                        unixIdentity.Connection = connection;
                    }

                    m_connection = connection;
                    m_ldapRoot = unixIdentity.LdapRoot ?? m_parent.Domain;

                    if (string.IsNullOrEmpty(m_parent.m_ldapPath))
                        m_parent.m_ldapPath = ldapHost;
                }
            }
            catch
            {
                m_connection = null;
            }

            return unixIdentity;
        }

        public void ChangePassword(string oldPassword, string newPassword)
        {
            int responseCode = ChangeUserPassword(m_isLocalAccount ? m_parent.UserName : m_parent.LoginID, oldPassword, newPassword);

            if (responseCode != 0)
                throw new SecurityException(string.Format("Failed to change password for user \"{0}\": {1}", m_parent.UserName, GetPAMErrorMessage(responseCode)));
        }

        public string[] GetUserPropertyValueCollection(string propertyName)
        {
            try
            {
                // Initialize if uninitialized
                Initialize();

                // Quit if disabled
                if (!m_enabled)
                    return null;

                // Return requested LDAP property value
                if ((object)m_userEntry != null)
                    return m_userEntry.getAttributeSet().getAttribute(propertyName).StringValueArray;
            }
            catch
            {
                return null;
            }

            return null;
        }

        public string GetUserPropertyValue(string propertyName)
        {
            string[] value = GetUserPropertyValueCollection(propertyName);

            if ((object)value != null && value.Length > 0)
                return value[0].Replace("  ", " ").Trim();

            return string.Empty;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly string[] s_builtInLocalGroups;

        // Static Constructor
        static UnixUserInfo()
        {
            List<string> builtInGroups = new List<string>();

            foreach (string builtInGroup in new[] { "root", "sys", "tty", "lp", "man", "wheel" })
            {
                if (LocalGroupExists(builtInGroup))
                    builtInGroups.Add(builtInGroup);
            }

            s_builtInLocalGroups = builtInGroups.ToArray();
        }

        // Static Properties
        public static bool MachineIsJoinedToDomain
        {
            get
            {
                int retval;

                try
                {
                    // Check Winbind / Samba
                    retval = Command.Execute("wbinfo", "-t").ExitCode;
                }
                catch
                {
                    retval = 1;
                }

                if (retval != 0)
                {
                    try
                    {
                        // Check Centrify DirectControl Express
                        retval = Command.Execute("adinfo").ExitCode;
                    }
                    catch
                    {
                        retval = 1;
                    }
                }

                if (retval != 0)
                {
                    try
                    {
                        // Check LikewiseOpen / Beyond Trust
                        retval = Command.Execute("lw-get-status").ExitCode;
                    }
                    catch
                    {
                        retval = 1;
                    }
                }

                return retval == 0;
            }
        }

        // Static Methods
        public static string[] GetBuiltInLocalGroups()
        {
            return s_builtInLocalGroups;
        }

        public static IPrincipal AuthenticateUser(string domain, string userName, string password, out string errorMessage)
        {
            WindowsPrincipal principal = null;
            int responseCode;
            errorMessage = null;

            if (UserInfo.IsLocalDomain(domain))
            {
                // Set the domain as the local machine if one is not specified
                if (string.IsNullOrEmpty(domain))
                    domain = Environment.MachineName;

                responseCode = AuthenticateUser(userName, password);

                if (responseCode == 0)
                    principal = new WindowsPrincipal(new UnixIdentity(domain, userName));
                else
                    errorMessage = string.Format("Failed to authenticate \"{0}\": {1}", userName, GetPAMErrorMessage(responseCode));
            }
            else
            {
                // Attempt PAM based authentication first - if configured, this will be the best option
                string domainUserName = string.Format("{0}\\{1}", domain, userName);

                responseCode = AuthenticateUser(domainUserName, password);

                if (responseCode == 0)
                    principal = new WindowsPrincipal(new UnixIdentity(domain, userName));

                // Try really hard to find a configured LDAP host
                string ldapHost = GetLdapHost();

                // If LDAP host cannot be determined, no LdapConnection can be established - if authentication
                // succeeded, user will be treated as a local user
                if ((object)ldapHost == null)
                {
                    if ((object)principal == null)
                        errorMessage = string.Format("Failed to authenticate \"{0}\": {1}", domainUserName, GetPAMErrorMessage(responseCode));
                    else
                        errorMessage = string.Format("User authentication succeeded, but no LDAP path could be derived.");
                }
                else
                {
                    try
                    {
                        // Attempt LDAP account authentication                    
                        LdapConnection connection = new LdapConnection();

                        if (ldapHost.StartsWith("LDAP", StringComparison.OrdinalIgnoreCase))
                        {
                            Uri ldapURI = new Uri(ldapHost);
                            ldapHost = ldapURI.Host + (ldapURI.Port == 0 ? "" : ":" + ldapURI.Port);
                        }

                        // If host LDAP path contains suffixed port number (e.g., host:port), this will be preferred over specified 389 default
                        connection.Connect(ldapHost, 389);
                        connection.Bind(string.Format("{0}@{1}", userName, domain), password);

                        if ((object)principal == null)
                            principal = new WindowsPrincipal(new UnixIdentity(domain, userName, connection));
                        else
                            ((UnixIdentity)principal.Identity).Connection = connection;
                    }
                    catch (Exception ex)
                    {
                        if (responseCode == 0)
                            errorMessage = string.Format("User authentication succeeded, but LDAP connection failed. LDAP response: {0}", ex.Message);
                        else
                            errorMessage = string.Format("LDAP response: {0}{1}PAM response: {2}", ex.Message, Environment.NewLine, GetPAMErrorMessage(responseCode));
                    }
                }
            }

            return principal;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static string GetLdapHost()
        {
            string ldapHost = null;

            // Attempt to derive an LDAP host from a Samba configuration @ /etc/samba/smb.conf looking for key "realm"
            try
            {
                const string smbConfFileName = "/etc/samba/smb.conf";

                if (File.Exists(smbConfFileName))
                {
                    string line;

                    using (StreamReader smbConf = File.OpenText(smbConfFileName))
                    {
                        do
                        {
                            line = smbConf.ReadLine();

                            if (!string.IsNullOrEmpty(line))
                            {
                                line = line.Trim();

                                if (line.StartsWith("realm", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] parts = line.Split('=');

                                    if (parts.Length > 1)
                                        ldapHost = parts[1].Trim();

                                    break;
                                }
                            }
                        }
                        while ((object)line != null);
                    }
                }
            }
            catch
            {
                ldapHost = null;
            }

            // If LDAP host has not been derived yet, attempt to derive an LDAP host from an OpenLDAP configuration @ /etc/openldap/ldap.conf looking for key "URI"
            if (string.IsNullOrEmpty(ldapHost))
            {
                try
                {
                    const string ldapConfFileName = "/etc/openldap/ldap.conf";

                    if (File.Exists(ldapConfFileName))
                    {
                        string line;

                        using (StreamReader ldapConf = File.OpenText(ldapConfFileName))
                        {
                            do
                            {
                                line = ldapConf.ReadLine();

                                if (!string.IsNullOrEmpty(line))
                                {
                                    line = line.Trim();

                                    if (line.StartsWith("URI", StringComparison.OrdinalIgnoreCase) && line.Length > 3)
                                    {
                                        ldapHost = line.Substring(3).Trim();
                                        break;
                                    }
                                }
                            }
                            while ((object)line != null);
                        }
                    }
                }
                catch
                {
                    ldapHost = null;
                }
            }

            // If LDAP host has not been derived yet, attempt to derive an LDAP host from a Centrify DirectControl Express adinfo call looking for key "Joined to domain:"
            if (string.IsNullOrEmpty(ldapHost))
            {
                try
                {
                    string line;

                    MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(Command.Execute("adinfo").StandardOutput));

                    using (StreamReader ldapConf = new StreamReader(stream))
                    {
                        do
                        {
                            line = ldapConf.ReadLine();

                            if (!string.IsNullOrEmpty(line))
                            {
                                line = line.Trim();

                                if (line.StartsWith("Joined to domain:", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] parts = line.Split(':');

                                    if (parts.Length > 1)
                                        ldapHost = parts[1].Trim();

                                    break;
                                }
                            }
                        }
                        while ((object)line != null);
                    }
                }
                catch
                {
                    ldapHost = null;
                }
            }

            // If LDAP host has not been derived yet, attempt to derive an LDAP host from a LikewiseOpen / Beyond Trust lw-get-status call looking for key "Domain:"
            if (string.IsNullOrEmpty(ldapHost))
            {
                try
                {
                    string line;

                    MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(Command.Execute("lw-get-status").StandardOutput));

                    using (StreamReader ldapConf = new StreamReader(stream))
                    {
                        do
                        {
                            line = ldapConf.ReadLine();

                            if (!string.IsNullOrEmpty(line))
                            {
                                line = line.Trim();

                                if (line.StartsWith("Domain:", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] parts = line.Split(':');

                                    if (parts.Length > 1)
                                        ldapHost = parts[1].Trim();

                                    break;
                                }
                            }
                        }
                        while ((object)line != null);
                    }
                }
                catch
                {
                    ldapHost = null;
                }
            }

            // If LDAP host has not been derived yet, attempt to derive an LDAP host from the configuration for known GSF security providers
            if (string.IsNullOrEmpty(ldapHost))
            {
                // TODO: If defined, should these values take precedence over derived LDAP host?
                try
                {
                    ConfigurationFile config = ConfigurationFile.Current;
                    CategorizedSettingsElementCollection settings = config.Settings["SecurityProvider"];

                    // Attempt to get LDAP path defined by AdoSecurityProvider
                    try
                    {
                        // In AdoSecurityProvider the ConnectionString setting is used for database connection
                        // to load role based security -- so it adds a new setting for actual LdapPath
                        ldapHost = settings["LdapPath"].Value;
                    }
                    catch
                    {
                        ldapHost = null;
                    }

                    // Otherwise, attempt to get LDAP path defined by LdapSecurityProvider
                    if ((object)ldapHost == null)
                    {
                        string ldapConnectionString = settings["ConnectionString"].Value;

                        if (ldapConnectionString.StartsWith("LDAP://", StringComparison.OrdinalIgnoreCase) ||
                            ldapConnectionString.StartsWith("LDAPS://", StringComparison.OrdinalIgnoreCase))
                            ldapHost = ldapConnectionString;

                        foreach (KeyValuePair<string, string> pair in ldapConnectionString.ParseKeyValuePairs())
                        {
                            if (pair.Value.StartsWith("LDAP://", StringComparison.OrdinalIgnoreCase) ||
                                pair.Value.StartsWith("LDAPS://", StringComparison.OrdinalIgnoreCase))
                                ldapHost = pair.Value;
                        }
                    }
                }
                catch
                {
                    ldapHost = null;
                }
            }

            return ldapHost;
        }

        public static WindowsImpersonationContext ImpersonateUser(string domain, string userName, string password)
        {
            WindowsImpersonationContext context = null;

            string errorMessage;
            IPrincipal principal = AuthenticateUser(domain, userName, password, out errorMessage);

            if ((object)principal != null)
            {
                try
                {
                    uint userID;

                    if (!UserInfo.IsLocalDomain(domain))
                        userName = string.Format("{0}\\{1}", domain, userName);

                    if (GetLocalUserID(userName, out userID) == 0)
                    {
                        // This requires that initial program load has root privileges
                        context = WindowsIdentity.Impersonate(new IntPtr(userID));
                    }
                    else
                    {
                        // If we can't derive local user ID, we will attempt to impersonate ourselves
                        // as this should be allowed for any user, this way an impersonation context
                        // object will at least exist:
                        WindowsIdentity current = WindowsIdentity.GetCurrent();

                        if ((object)current != null)
                            context = WindowsIdentity.Impersonate(current.Token);
                    }
                }
                catch
                {
                    context = null;
                }
            }

            return context;
        }

        public static bool LocalUserExists(string userName)
        {
            uint userID;
            return GetLocalUserID(ValidateAccountName(userName), out userID) == 0;
        }

        public static bool CreateLocalUser(string userName, string password, string userDescription)
        {
            // Determine if local user exists
            if (!LocalUserExists(userName))
            {
                try
                {
                    Command.Execute("useradd", string.Format("-c \"{0}\" -m -U -p {1} {2}",
                        userDescription ?? "Local account for " + userName,
                        PtrToString(GetPasswordHash(password, GetRandomSalt())),
                        EncodeAccountName(userName)));

                    return true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot create local user \"{0}\": {1}", userName, ex.Message), ex);
                }
            }

            return false;
        }

        public static void SetLocalUserPassword(string userName, string password)
        {
            // Determine if local user exists
            if (!LocalUserExists(userName))
                throw new InvalidOperationException(string.Format("Cannot set password for local user \"{0}\": user does not exist.", userName));

            int response = SetLocalUserPassword(ValidateAccountName(userName), password, GetRandomSalt());

            if (response != 0)
                throw new InvalidOperationException(string.Format("Cannot set password for local user \"{0}\": {1}", userName, response));
        }

        private static string GetRandomSalt()
        {
            char[] salt = new char[16];

            for (int i = 0; i < salt.Length; i++)
            {
                switch (Security.Cryptography.Random.Int32Between(0, 3))
                {
                    case 0:
                        salt[i] = (char)((int)'A' + Security.Cryptography.Random.Int32Between(0, 26));
                        break;
                    case 1:
                        salt[i] = (char)((int)'a' + Security.Cryptography.Random.Int32Between(0, 26));
                        break;
                    case 2:
                        salt[i] = (char)((int)'.' + Security.Cryptography.Random.Int32Between(0, 12));
                        break;
                }
            }

            return "$6$" + new string(salt) + "$";
        }

        public static bool RemoveLocalUser(string userName)
        {
            if (LocalUserExists(userName))
            {
                try
                {
                    Command.Execute("userdel", string.Format("-f -r -Z {0}", EncodeAccountName(userName)));
                    return true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot remove local user \"{0}\": {1}", userName, ex.Message), ex);
                }
            }

            return false;
        }

        public static bool LocalGroupExists(string groupName)
        {
            uint groupID;
            return GetLocalGroupID(ValidateAccountName(groupName), out groupID) == 0;
        }

        public static bool CreateLocalGroup(string groupName)
        {
            if (!LocalGroupExists(groupName))
            {
                try
                {
                    Command.Execute("groupadd", EncodeAccountName(groupName));
                    return true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot create local group \"{0}\": {1}", groupName, ex.Message), ex);
                }
            }

            return false;
        }

        public static bool RemoveLocalGroup(string groupName)
        {
            if (LocalGroupExists(groupName))
            {
                try
                {
                    Command.Execute("groupdel", EncodeAccountName(groupName));
                    return true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(string.Format("Cannot remove local group \"{0}\": {1}", groupName, ex.Message), ex);
                }
            }

            return false;
        }

        public static bool UserIsInLocalGroup(string groupName, string userName)
        {
            // Determine if local group exists
            if (!LocalGroupExists(groupName))
                throw new InvalidOperationException(string.Format("Cannot determine if user \"{0}\" is in local group \"{1}\": group does not exist.", userName, groupName));

            // Determine if local user exists
            if (!LocalUserExists(userName))
                throw new InvalidOperationException(string.Format("Cannot determine if user \"{0}\" is in local group \"{1}\": user does not exist.", userName, groupName));

            try
            {
                groupName = ValidateAccountName(groupName);
                userName = ValidateAccountName(userName);

                // See if user is in group
                return GetLocalGroupUserSet(groupName).Contains(userName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Cannot determine if user \"{0}\" is in local group \"{1}\": {2}", userName, groupName, ex.Message), ex);
            }
        }

        public static bool AddUserToLocalGroup(string groupName, string userName)
        {
            // Determine if local group exists
            if (!LocalGroupExists(groupName))
                throw new InvalidOperationException(string.Format("Cannot add user \"{0}\" to local group \"{1}\": group does not exist.", userName, groupName));

            // Determine if user exists
            if (!LocalUserExists(userName))
                throw new InvalidOperationException(string.Format("Cannot add user \"{0}\" to local group \"{1}\": user does not exist.", userName, groupName));

            try
            {
                groupName = ValidateAccountName(groupName);
                userName = ValidateAccountName(userName);

                // If user already exists in group, exit and return false
                if (GetLocalGroupUserSet(groupName).Contains(userName))
                    return false;

                // Add new user to group
                Command.Execute("gpasswd ", string.Format("-a {0} {1}", EncodeAccountName(userName), EncodeAccountName(groupName)));
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Cannot add user \"{0}\" to local group \"{1}\": {2}", userName, groupName, ex.Message), ex);
            }
        }

        public static bool RemoveUserFromLocalGroup(string groupName, string userName)
        {
            // Determine if local group exists
            if (!LocalGroupExists(groupName))
                throw new InvalidOperationException(string.Format("Cannot remove user \"{0}\" from local group \"{1}\": group does not exist.", userName, groupName));

            // Determine if user exists
            if (!LocalUserExists(userName))
                throw new InvalidOperationException(string.Format("Cannot remove user \"{0}\" from local group \"{1}\": user does not exist.", userName, groupName));

            try
            {
                groupName = ValidateAccountName(groupName);
                userName = ValidateAccountName(userName);

                // If user exists in group, remove user and return true
                if (GetLocalGroupUserSet(groupName).Contains(userName))
                {
                    Command.Execute("gpasswd ", string.Format("-d {0} {1}", EncodeAccountName(userName), EncodeAccountName(groupName)));
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Cannot remove user \"{0}\" from local group \"{1}\": {2}", userName, groupName, ex.Message), ex);
            }

            return false;
        }

        public static string[] GetLocalGroupUserList(string groupName)
        {
            // Determine if local group exists
            if (!LocalGroupExists(groupName))
                throw new InvalidOperationException(string.Format("Cannot get members for local group \"{0}\": group does not exist.", groupName));

            return GetLocalGroupUserSet(ValidateAccountName(groupName)).Select(DecodeAccountName).ToArray();
        }

        // A HashSet is used to ensure a unique list since there can be membership overlap in primary and secondary groups
        private static HashSet<string> GetLocalGroupUserSet(string groupName)
        {
            IntPtr groupMembers;

            if (GetLocalGroupMembers(groupName, out groupMembers) == 0)
            {
                try
                {
                    return new HashSet<string>(PtrToStringArray(groupMembers), StringComparer.Ordinal);
                }
                finally
                {
                    FreeLocalGroupMembers(groupMembers);
                }
            }

            return new HashSet<string>();
        }

        public static string UserNameToSID(string userName)
        {
            uint userID;

            if ((object)userName == null)
                throw new ArgumentNullException(nameof(userName));

            if (GetLocalUserID(ValidateAccountName(userName), out userID) == 0)
                return "user:" + userID;

            // Do not prefix unknown accounts, could be an account name for another platform 
            return userName;
        }

        public static string GroupNameToSID(string groupName)
        {
            uint groupID;

            if ((object)groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            if (GetLocalGroupID(ValidateAccountName(groupName), out groupID) == 0)
                return "group:" + groupID;

            // Do not prefix unknown accounts, could be an account name for another platform 
            return groupName;
        }

        public static string SIDToAccountName(string sid)
        {
            StringBuilder accountName = new StringBuilder(MaxAccountNameLength);
            uint accountID;

            if ((object)side == null)
                return null;

            if ((IsUserSID(sid) && TryExtractAccountID(sid, out accountID) && GetLocalUserName(accountID, accountName) == 0) ||
                (IsGroupSID(sid) && TryExtractAccountID(sid, out accountID) && GetLocalGroupName(accountID, accountName) == 0))
                return DecodeAccountName(accountName.ToString());

            return sid;
        }

        public static bool IsUserSID(string sid)
        {
            return sid.StartsWith("user:", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsGroupSID(string sid)
        {
            return sid.StartsWith("group:", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryExtractAccountID(string sid, out uint accountID)
        {
            return uint.TryParse(sid.Substring(sid.IndexOf(':') + 1), out accountID);
        }

        // User name expected to be pre-validated
        private static string[] GetLocalUserGroups(string userName)
        {
            List<string> groups = new List<string>();
            int groupCount = GetLocalUserGroupCount(userName);

            if (groupCount > 0)
            {
                uint[] groupIDs = new uint[groupCount];

                if (GetLocalUserGroupIDs(userName, groupCount, ref groupIDs) == 0)
                {
                    foreach (uint groupID in groupIDs)
                    {
                        StringBuilder groupName = new StringBuilder(MaxAccountNameLength);

                        if (GetLocalGroupName(groupID, groupName) == 0)
                            groups.Add(DecodeAccountName(groupName.ToString()));
                    }
                }
            }

            return groups.ToArray();
        }

        private static int GetCachedLocalUserPasswordInformation(IPrincipal passthroughPrincipal, string userName, out UserPasswordInformation userPasswordInfo, out AccountStatus accountStatus)
        {
            // Attempt to retrieve Unix user principal identity in case shadow information has already been parsed, in most
            // cases we will have already reduced rights needed to read this information so we pick up pre-parsed info
            WindowsPrincipal principal = passthroughPrincipal as WindowsPrincipal;

            if ((object)principal != null)
            {
                UnixIdentity identity = principal.Identity as UnixIdentity;

                // If user has already been authenticated, we can load pre-parsed shadow information
                if ((object)identity != null && identity.LoadedUserPasswordInformation && identity.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                {
                    userPasswordInfo = identity.UserPasswordInformation;
                    accountStatus = identity.AccountStatus;
                    return 0;
                }
            }

            return GetLocalUserPasswordInformation(userName, out userPasswordInfo, out accountStatus);
        }

        // Bit-size/platform inter-mediator for getting user password information
        private static int GetLocalUserPasswordInformation(string userName, out UserPasswordInformation userPasswordInfo, out AccountStatus accountStatus)
        {
            userPasswordInfo = new UserPasswordInformation();

            if (Common.GetOSPlatformID() == PlatformID.MacOSX)
            {
                int lastChangeDate, maxDaysForChange, accountExpirationDate;

                accountStatus = AccountStatus.Normal;

                // Mac OS X call
                if (GetLocalUserPasswordInformationMac(userName, out lastChangeDate, out maxDaysForChange, out accountExpirationDate) == 0)
                {
                    userPasswordInfo.lastChangeDate = lastChangeDate;
                    userPasswordInfo.maxDaysForChange = maxDaysForChange;
                    userPasswordInfo.accountExpirationDate = accountExpirationDate;
                    return 0;
                }
            }
            else
            {
                if (IntPtr.Size == 4)
                {
                    // 32-bit OS call
                    UserPasswordInformation32 userPasswordInfo32 = new UserPasswordInformation32();

                    if (GetLocalUserPasswordInformation32(userName, ref userPasswordInfo32, out accountStatus) == 0)
                    {
                        userPasswordInfo.lastChangeDate = userPasswordInfo32.lastChangeDate;
                        userPasswordInfo.minDaysForChange = userPasswordInfo32.minDaysForChange;
                        userPasswordInfo.maxDaysForChange = userPasswordInfo32.maxDaysForChange;
                        userPasswordInfo.warningDays = userPasswordInfo32.warningDays;
                        userPasswordInfo.inactivityDays = userPasswordInfo32.inactivityDays;
                        userPasswordInfo.accountExpirationDate = userPasswordInfo32.accountExpirationDate;
                        return 0;
                    }
                }
                else
                {
                    // 64-bit OS call
                    return GetLocalUserPasswordInformation64(userName, ref userPasswordInfo, out accountStatus);
                }
            }

            return 1;
        }

        private static string GetPAMErrorMessage(int responseCode)
        {
            if (Enum.IsDefined(typeof(PAMResponseCode), responseCode))
            {
                switch ((PAMResponseCode)responseCode)
                {
                    case PAMResponseCode.PAM_SYSTEM_ERR:
                        return "System error, for example a NULL pointer was submitted instead of a pointer to data.";
                    case PAMResponseCode.PAM_BUF_ERR:
                        return "Memory buffer error.";
                    case PAMResponseCode.PAM_MAXTRIES:
                        return "One or more of the authentication modules has reached its limit of tries authenticating the user. Do not try again.";
                    case PAMResponseCode.PAM_AUTH_ERR:
                        return "The user was not authenticated.";
                    case PAMResponseCode.PAM_CRED_INSUFFICIENT:
                        return "For some reason the application does not have sufficient credentials to authenticate the user.";
                    case PAMResponseCode.PAM_AUTHINFO_UNAVAIL:
                        return "The modules were not able to access the authentication information. This might be due to a network or hardware failure etc.";
                    case PAMResponseCode.PAM_USER_UNKNOWN:
                        return "User unknown to authentication service.";
                    case PAMResponseCode.PAM_ABORT:
                        return "General failure.";
                    case PAMResponseCode.PAM_AUTHTOK_ERR:
                        return "A module was unable to obtain the new authentication token.";
                    case PAMResponseCode.PAM_AUTHTOK_RECOVERY_ERR:
                        return "A module was unable to obtain the old authentication token.";
                    case PAMResponseCode.PAM_AUTHTOK_LOCK_BUSY:
                        return "One or more of the modules was unable to change the authentication token since it is currently locked.";
                    case PAMResponseCode.PAM_AUTHTOK_DISABLE_AGING:
                        return "Authentication token aging has been disabled for at least one of the modules.";
                    case PAMResponseCode.PAM_PERM_DENIED:
                        return "Permission denied. Validate user credentials.";
                    case PAMResponseCode.PAM_TRY_AGAIN:
                        return "Not all of the modules were in a position to update the authentication token(s). None of the user's authentication tokens were updated.";
                    default:
                        return responseCode.ToString();
                }
            }

            return responseCode.ToString();
        }

        // Returns a decoded account name for use by API user
        private static string DecodeAccountName(string accountName)
        {
            accountName = accountName.ShellDecode().Replace('^', ' ');

            if (!accountName.Contains('\\'))
                accountName = Environment.MachineName + "\\" + accountName;

            return accountName;
        }

        // Return an encoded account name valid for shell commands
        private static string EncodeAccountName(string accountName)
        {
            return accountName.ShellEncode().Replace(' ', '^');
        }

        // Return valid account name for machine name prefixed local accounts
        private static string ValidateAccountName(string accountName)
        {
            if (accountName.Contains('\\'))
            {
                // POSIX functions do not recognize machinename\accountname - but UserInfo allows this format for local accounts
                string[] accountParts = accountName.Split('\\');

                if (accountParts.Length == 2)
                {
                    // groupName is specified in 'domain\accountname' format
                    string domain = accountParts[0];
                    accountName = accountParts[1];

                    // For local users we just want accountname; otherwise, domain\accountname
                    if (!UserInfo.IsLocalDomain(domain))
                        accountName = string.Format("{0}\\{1}", domain, accountName);
                }
            }
            else if (accountName.Contains('@'))
            {
                // POSIX functions do not recognize accountname@machinename - but UserInfo allows this format for local accounts
                string[] accountParts = accountName.Split('@');

                if (accountParts.Length == 2)
                {
                    // groupName is specified in 'accountname@domain' format
                    string domain = accountParts[1];
                    accountName = accountParts[0];

                    // For local users we just want accountname; otherwise, domain\accountname
                    if (!UserInfo.IsLocalDomain(domain))
                        accountName = string.Format("{0}\\{1}", domain, accountName);
                }
            }

            return accountName;
        }

        // Parse LDAP distinguished name tokens
        private static string ParseDNTokens(string dn, string token, char delimiter = '.')
        {
            List<string> tokens = new List<string>();
            string[] elements = dn.Split(',');

            for (int i = 0; i < elements.Length; i++)
            {
                string element = elements[i].Trim();

                if (element.StartsWith(token, StringComparison.OrdinalIgnoreCase))
                {
                    string[] parts = element.Split('=');

                    if (parts.Length == 2)
                        tokens.Add(parts[1].Trim());
                }
            }

            return tokens.ToDelimitedString(delimiter);
        }

        #region [ String Marshaling Functions ]

        private static string PtrToString(IntPtr p)
        {
            if (p == IntPtr.Zero)
                return null;

            return Marshal.PtrToStringAnsi(p);
        }

        private static string[] PtrToStringArray(IntPtr stringArray)
        {
            if (stringArray == IntPtr.Zero)
                return new string[] { };

            return PtrToStringArray(CountStrings(stringArray), stringArray);
        }

        private static int CountStrings(IntPtr stringArray)
        {
            int count = 0;

            while (Marshal.ReadIntPtr(stringArray, count * IntPtr.Size) != IntPtr.Zero)
                ++count;

            return count;
        }

        private static string[] PtrToStringArray(int count, IntPtr stringArray)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "< 0");

            if (stringArray == IntPtr.Zero)
                return new string[count];

            string[] members = new string[count];

            for (int i = 0; i < count; ++i)
            {
                IntPtr s = Marshal.ReadIntPtr(stringArray, i * IntPtr.Size);
                members[i] = PtrToString(s);
            }

            return members;
        }

        #endregion

        // AuthenticateUser function is PAM based, so it will support more than local users
        [DllImport(ImportFileName)]
        private static extern int AuthenticateUser(string userName, string password);

        // ChangeUserPassword function is PAM based, so it will support more than local users
        [DllImport(ImportFileName)]
        private static extern int ChangeUserPassword(string userName, string oldPassword, string newPassword);

        // This function may return ID for non-local users if nsswitch.conf is configured to do so
        [DllImport(ImportFileName)]
        private static extern int GetLocalUserID(string userName, out uint userID);

        // Preallocate outbound userName to 256 characters
        [DllImport(ImportFileName)]
        private static extern int GetLocalUserName(uint uid, StringBuilder userName);

        // Returns a char* that needs to marshaled to a .NET string
        [DllImport(ImportFileName)]
        private static extern IntPtr GetLocalUserGecos(string userName);

        [DllImport(ImportFileName, EntryPoint = "GetLocalUserPasswordInformation")] // 64-bit version
        private static extern int GetLocalUserPasswordInformation64(string userName, ref UserPasswordInformation userPasswordInfo, out AccountStatus status);

        [DllImport(ImportFileName, EntryPoint = "GetLocalUserPasswordInformation")] // 32-bit version
        private static extern int GetLocalUserPasswordInformation32(string userName, ref UserPasswordInformation32 userPasswordInfo, out AccountStatus status);

        [DllImport(ImportFileName, EntryPoint = "GetLocalUserPasswordInformation")] // Mac OS X version (Mono on Mac doesn't want to return struct values :-p)
        private static extern int GetLocalUserPasswordInformationMac(string userName, out int lastChangeDate, out int maxDaysForChange, out int accountExpirationDate);

        [DllImport(ImportFileName)]
        private static extern int SetLocalUserPassword(string userName, string password, string salt);

        // Returns a char* that needs to be marshaled to a .NET string
        [DllImport(ImportFileName)]
        private static extern IntPtr GetPasswordHash(string password, string salt);

        [DllImport(ImportFileName)]
        private static extern int GetLocalUserGroupCount(string userName);

        // Preallocate groupIDs as an unsigned integer array sized from GetLocalUserGroupCount
        [DllImport(ImportFileName)]
        private static extern int GetLocalUserGroupIDs(string userName, int groupCount, ref uint[] groupsIDs);

        [DllImport(ImportFileName)]
        private static extern int GetLocalGroupID(string groupName, out uint groupID);

        // Preallocate outbound userName to 256 characters
        [DllImport(ImportFileName)]
        private static extern int GetLocalGroupName(uint uid, StringBuilder groupName);

        // Parameter groupMembers is a char*** out parameter that needs to be marshaled into
        // a .NET string array and must then be freed using FreeLocalGroupMembers function
        [DllImport(ImportFileName)]
        private static extern int GetLocalGroupMembers(string groupName, out IntPtr groupMembers);

        [DllImport(ImportFileName)]
        private static extern void FreeLocalGroupMembers(IntPtr groupMembers);

        #endregion
    }
}