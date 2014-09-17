//******************************************************************************************************
//  UnixUserInfo.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  08/27/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// Undefine to use internally linked unmanaged functions (e.g., when Mono hosted in gsf service)
#define USE_SHARED_OBJECT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using GSF.Annotations;
using GSF.Configuration;
using GSF.Console;
using GSF.Units;
using Novell.Directory.Ldap;

#pragma warning disable 0649

namespace GSF.Identity
{
    // Unix implementation of key UserInfo class elements
    internal class UnixUserInfo : IUserInfo
    {
#if USE_SHARED_OBJECT
        private const string IMPORT = "GSF.POSIX.so";
#else
        private const string IMPORT = "__Internal";
#endif

        #region [ Members ]

        // Nested Types

        internal class UnixSecurityIdentity : WindowsIdentity
        {
            #region [ Members ]

            // Fields
            private readonly string m_providerType;
            private readonly LdapConnection m_connection;

            #endregion

            #region [ Constructors ]

            public UnixSecurityIdentity(string userName)
                : this(userName, null)
            {
            }

            public UnixSecurityIdentity(string userName, LdapConnection connection)
                : base(GetUserIDAsToken(userName), null, WindowsAccountType.Normal, true)
            {
                m_providerType = (object)connection == null ? "PAM" : "LDAP";
                m_connection = connection;
            }

            #endregion

            #region [ Properties ]

            public string ProviderType
            {
                get
                {
                    return m_providerType;
                }
            }

            public LdapConnection Connection
            {
                get
                {
                    return m_connection;
                }
            }

            #endregion

            #region [ Static ]

            private static IntPtr GetUserIDAsToken(string userName)
            {
                uint userID;
                return GetLocalUserID(userName, out userID) == 0 ? new IntPtr((long)userID) : IntPtr.Zero;
            }

            #endregion
        }

        private struct UserPasswordInformation
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

        // Fields
        private readonly UserInfo m_parent;
        private LdapConnection m_connection;
        private LdapEntry m_userEntry;
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
                        WindowsPrincipal windowsPrincipal = Thread.CurrentPrincipal as WindowsPrincipal;

                        exists =
                            (object)windowsPrincipal != null &&
                            !string.IsNullOrEmpty(m_parent.UserName) &&
                            string.Compare(windowsPrincipal.Identity.Name, m_parent.UserName, StringComparison.OrdinalIgnoreCase) == 0 &&
                            windowsPrincipal.Identity.IsAuthenticated;
                    }
                }

                if (!exists)
                {
                    if (m_isLocalAccount)
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
                        if (m_isLocalAccount)
                        {
                            CommandResponse response = Command.Execute("lastlog", "-u " + m_parent.UserName);

                            if (response.ExitCode == 0)
                            {
                                string[] lines = response.StandardOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                                if (lines.Length > 1 && lines[1].Length > 43)
                                    DateTime.TryParseExact(lines[1].Substring(43), "ddd MMM d HH:mm:ss zzff yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowInnerWhite, out lastLogon);
                            }
                        }
                        else
                        {
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
                        if (m_isLocalAccount)
                            creationDate = Directory.GetCreationTime("/home/" + m_parent.UserName);
                        else
                            creationDate = Convert.ToDateTime(GetUserPropertyValue("whenCreated"));
                    }
                    catch
                    {
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
                    if (m_isLocalAccount)
                    {
                        UserPasswordInformation userPasswordInfo = new UserPasswordInformation();
                        AccountStatus status;

                        if (GetLocalUserPasswordInformation(m_parent.UserName, userPasswordInfo, out status) == 0)
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
                                if (maxPasswordAge != long.MaxValue)
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

                if (m_enabled && m_isLocalAccount)
                {
                    UserPasswordInformation userPasswordInfo = new UserPasswordInformation();
                    AccountStatus status;

                    if (GetLocalUserPasswordInformation(m_parent.UserName, userPasswordInfo, out status) == 0)
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
                    if (m_isLocalAccount)
                    {
                        UserPasswordInformation userPasswordInfo = new UserPasswordInformation();
                        AccountStatus status;

                        if (GetLocalUserPasswordInformation(m_parent.UserName, userPasswordInfo, out status) == 0 && userPasswordInfo.maxDaysForChange < 99999)
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
                    return GetUserPropertyValueCollection("memberOf");

                return LocalGroups;
            }
        }

        public string[] LocalGroups
        {
            get
            {
                return GetLocalUserGroups(m_parent.UserName);

                #region [ Possible Alternate Implementation ]

                //CommandResponse response = Command.Execute("groups", m_parent.UserName);

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
                string userName = PtrToString(GetLocalUserGecos(m_parent.UserName));

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
                return m_isLocalAccount;
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

                    // Initialize the directory entry object used to retrieve active directory information
                    try
                    {
                        // Impersonate to the privileged account if specified
                        currentContext = m_parent.ImpersonatePrivilegedAccount();

                        // Pick up current user or impersonated user principal
                        WindowsPrincipal principal = Thread.CurrentPrincipal as WindowsPrincipal;

                        if ((object)principal != null)
                        {
                            UnixSecurityIdentity identity = principal.Identity as UnixSecurityIdentity;

                            // If user has already been authenticated so we can make some initial assumptions
                            if ((object)identity != null)
                                m_connection = identity.Connection;
                        }

                        if ((object)m_connection != null)
                        {
                            LdapSearchResults results = m_connection.Search("", LdapConnection.SCOPE_ONE, string.Format("(SAMAccountName={0})", m_parent.UserName), null, false);

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
                            throw new InvalidOperationException("Failed to get a valid LDAP connection");
                        }
                    }
                    catch (Exception ex)
                    {
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

                if ((object)m_userEntry != null)
                {
                    // Return requested Active Directory property value
                    LdapAttributeSet attributeSet = m_userEntry.getAttributeSet();

                    IEnumerator enumerator = attributeSet.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        LdapAttribute attribute = (LdapAttribute)enumerator.Current;

                        if (attribute.Name.Equals(propertyName))
                            return attribute.StringValueArray;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public string GetUserPropertyValue(string propertyName)
        {
            string[] values = GetUserPropertyValueCollection(propertyName);

            if ((object)values != null && values.Length > 0)
                return values[0].Replace("  ", " ").Trim();

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
                try
                {
                    // TODO: Following command checks for SAMBA style AD connection, may need to check Likewise, others? Is Mac different?
                    return Command.Execute("wbinfo", "-t").ExitCode == 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        // Static Methods
        public static string[] GetBuiltInLocalGroups()
        {
            return s_builtInLocalGroups;
        }

        public static IPrincipal AuthenticateUser(string domain, string username, string password, out string errorMessage)
        {
            WindowsPrincipal principal = null;
            int responseCode;
            errorMessage = null;

            if (UserInfo.IsLocalDomain(domain))
            {
                responseCode = AuthenticateUser(username, password);

                if (responseCode == 0)
                    principal = new WindowsPrincipal(new UnixSecurityIdentity(username));
                else
                    errorMessage = string.Format("Failed to authenticate \"{0}\": {1}", username, GetPAMErrorMessage(responseCode));
            }
            else
            {
                // Attempt PAM based authentication first - if configured, this will be the best option
                string domainUserName = string.Format("{0}\\{1}", domain, username);

                responseCode = AuthenticateUser(domainUserName, password);

                if (responseCode == 0)
                {
                    principal = new WindowsPrincipal(new UnixSecurityIdentity(domainUserName));
                }
                else
                {
                    // Attempt to derive an LDAP path from the configuration for known security providers
                    string ldapPath = GetLdapPath();

                    if ((object)ldapPath == null)
                        return null;

                    try
                    {
                        // Attempt LDAP account authentication
                        LdapConnection connection = new LdapConnection();
                        int port = 389;

                        if (ldapPath.Contains(":"))
                        {
                            string[] parts = ldapPath.Split(':');

                            if (int.TryParse(parts[1].ToNonNullNorWhiteSpace("389").Trim(), out port))
                                ldapPath = parts[0];
                            else
                                port = 389;
                        }

                        connection.Connect(ldapPath, port);
                        connection.Bind(domainUserName, password);

                        principal = new WindowsPrincipal(new UnixSecurityIdentity(domainUserName, connection));
                    }
                    catch (Exception ex)
                    {
                        errorMessage = string.Format("LDAP response: {0}{1}PAM response: {2}", ex.Message, Environment.NewLine, GetPAMErrorMessage(responseCode));
                    }
                }
            }

            return principal;
        }

        private static string GetLdapPath()
        {
            string ldapPath;

            try
            {
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings["SecurityProvider"];

                // Attempt to get LDAP path defined by AdoSecurityProvider
                try
                {
                    // In AdoSecurityProvider the ConnectionString setting is used for database connection
                    // to load role based security -- so it adds a new setting for actual LdapPath
                    ldapPath = settings["LdapPath"].Value;
                }
                catch
                {
                    ldapPath = null;
                }

                // Otherwise, attempt to get LDAP path defined by LdapSecurityProvider
                if ((object)ldapPath == null)
                {
                    string ldapConnectionString = settings["ConnectionString"].Value;

                    if (ldapConnectionString.StartsWith("LDAP://", StringComparison.OrdinalIgnoreCase) ||
                        ldapConnectionString.StartsWith("LDAPS://", StringComparison.OrdinalIgnoreCase))
                        ldapPath = ldapConnectionString;

                    foreach (KeyValuePair<string, string> pair in ldapConnectionString.ParseKeyValuePairs())
                    {
                        if (pair.Value.StartsWith("LDAP://", StringComparison.OrdinalIgnoreCase) ||
                            pair.Value.StartsWith("LDAPS://", StringComparison.OrdinalIgnoreCase))
                            ldapPath = pair.Value;
                    }
                }
            }
            catch
            {
                ldapPath = null;
            }

            return ldapPath;
        }

        public static WindowsImpersonationContext ImpersonateUser(string domain, string userName, string password)
        {
            // I was only able to get Mono WindowsImpersonationContext code to work under two conditions:
            //      (1) Code has to be compiled with gmcs (i.e., Mono compiler)
            //      (2) User requires CAP_SETUID rights (e.g., root) to impersonate
            // Philosophically we would like to have GSF fully compiled under Windows then deployed under Mono to keep
            // the build process simple, so a requirement to build with gmcs is currently precluded. Also, since the
            // desired intention of this function is to emulate and possibly even elevate an existing user's rights as
            // long as you have the right credentials, this function will not work exactly the same on Mono. However,
            // the common use case for this function in GSF is to impersonate a user (such as a domain user) then
            // attempt to initialize a UserInfo object for the user. For example, a local user would not necessarily
            // be able to access the domain so the impersonation is required authenticate and then emulate the user
            // so that the user's domain level information for the user can be requested. For this use case we can
            // just set the thread's current principal to a custom security principal that has a reference the
            // LdapConnection and/or local user data so the UnixUserInfo instance can use this during initialization:
            WindowsImpersonationContext context = null;

            string errorMessage;
            IPrincipal principal = AuthenticateUser(domain, userName, password, out errorMessage);

            if ((object)principal != null)
            {
                // We will attempt to impersonate ourselves as this should be allowed for any user,
                // this way an impersonation context object will at least exist:
                try
                {
                    WindowsIdentity current = WindowsIdentity.GetCurrent();

                    if ((object)current != null)
                        context = WindowsIdentity.Impersonate(current.Token);
                }
                catch
                {
                    context = null;
                }

                // Set current thread principal to authenticated user principal - this creates a viable
                // impersonation for the current thread, but not the entire application domain:
                Thread.CurrentPrincipal = principal;
            }

            return context;
        }

        public static bool LocalUserExists(string userName)
        {
            uint userID;
            return GetLocalUserID(userName, out userID) == 0;
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
                        userName));

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

            int response = SetLocalUserPassword(userName, password, GetRandomSalt());

            if (response != 0)
                throw new InvalidOperationException(string.Format("Cannot set password for local user \"{0}\": {1}", userName, response));
        }

        private static string GetRandomSalt()
        {
            Random random = new Random();
            char[] salt = new char[16];

            for (int i = 0; i < salt.Length; i++)
            {
                switch (random.Next(0, 3))
                {
                    case 0:
                        salt[i] = (char)((int)'A' + random.Next(0, 26));
                        break;
                    case 1:
                        salt[i] = (char)((int)'a' + random.Next(0, 26));
                        break;
                    case 2:
                        salt[i] = (char)((int)'.' + random.Next(0, 12));
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
                    Command.Execute("userdel", string.Format("-f -r -Z {0}", userName));
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
            return GetLocalGroupID(groupName, out groupID) == 0;
        }

        public static bool CreateLocalGroup(string groupName)
        {
            if (!LocalGroupExists(groupName))
            {
                try
                {
                    Command.Execute("groupadd", groupName);
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
                    Command.Execute("groupdel", groupName);
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
                // See if user is in group
                return GetGroupUserSet(groupName).Contains(userName);
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
                // If user already exists in group, exit and return false
                if (GetGroupUserSet(groupName).Contains(userName))
                    return false;

                // Add new user to group
                Command.Execute("gpasswd ", string.Format("-a {0} {1}", userName, groupName));
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
                // If user exists in group, remove user and return true
                if (GetGroupUserSet(groupName).Contains(userName))
                {
                    Command.Execute("gpasswd ", string.Format("-d {0} {1}", userName, groupName));
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Cannot remove user \"{0}\" from local group \"{1}\": {2}", userName, groupName, ex.Message), ex);
            }

            return false;
        }

        private static HashSet<string> GetGroupUserSet(string groupName)
        {
            return new HashSet<string>(PtrToStringArray(GetLocalGroupMembers(groupName)), StringComparer.InvariantCulture);
        }

        public static string UserNameToSID(string userName)
        {
            string[] accountParts;
            bool isLocalDomain = true;

            if ((object)userName == null)
                throw new ArgumentNullException("userName");

            accountParts = userName.Split('\\');

            if (accountParts.Length == 2)
            {
                userName = accountParts[1];
                isLocalDomain = UserInfo.IsLocalDomain(accountParts[0]);
            }

            if (isLocalDomain)
            {
                uint userID;

                if (GetLocalUserID(userName, out userID) == 0)
                    return "user:" + userID;
            }

            return "user:" + userName;
        }

        public static string GroupNameToSID(string groupName)
        {
            string[] accountParts;
            bool isLocalDomain = true;

            if ((object)groupName == null)
                throw new ArgumentNullException("groupName");

            accountParts = groupName.Split('\\');

            if (accountParts.Length == 2)
            {
                groupName = accountParts[1];
                isLocalDomain = UserInfo.IsLocalDomain(accountParts[0]);
            }

            if (isLocalDomain)
            {
                uint groupID;

                if (GetLocalGroupID(groupName, out groupID) == 0)
                    return "group:" + groupID;
            }

            return "group:" + groupName;
        }

        public static string SIDToAccountName(string sid)
        {
            StringBuilder accountName = new StringBuilder(MaxAccountNameLength);

            if (IsUserSID(sid) && GetLocalUserName(ExtractAccountID(sid), accountName) == 0)
                return accountName.ToString();

            if (IsGroupSID(sid) && GetLocalGroupName(ExtractAccountID(sid), accountName) == 0)
                return accountName.ToString();

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

        private static uint ExtractAccountID(string sid)
        {
            return uint.Parse(sid.Substring(sid.IndexOf(':') + 1));
        }

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
                            groups.Add(groupName.ToString());
                    }
                }
            }

            return groups.ToArray();
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
                        return "Permission denied.";
                    case PAMResponseCode.PAM_TRY_AGAIN:
                        return "Not all of the modules were in a position to update the authentication token(s). None of the user's authentication tokens were updated.";
                    default:
                        return responseCode.ToString();
                }
            }

            return responseCode.ToString();
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
                throw new ArgumentOutOfRangeException("count", "< 0");

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
        [DllImport(IMPORT)]
        private static extern int AuthenticateUser(string userName, string password);

        // ChangeUserPassword function is PAM based, so it will support more than local users
        [DllImport(IMPORT)]
        private static extern int ChangeUserPassword(string userName, string oldPassword, string newPassword);

        [DllImport(IMPORT)]
        private static extern int GetLocalUserID(string userName, out uint userID);

        // Preallocate outbound userName to 256 characters
        [DllImport(IMPORT)]
        private static extern int GetLocalUserName(uint uid, StringBuilder userName);

        // Returns a char* that needs to marshaled to a .NET string
        [DllImport(IMPORT)]
        private static extern IntPtr GetLocalUserGecos(string userName);

        // UserPasswordInformation structure contains only blittable types so it will be marshaled by reference 
        [DllImport(IMPORT)]
        private static extern int GetLocalUserPasswordInformation(string userName, UserPasswordInformation userPasswordInfo, out AccountStatus status);

        [DllImport(IMPORT)]
        private static extern int SetLocalUserPassword(string userName, string password, string salt);

        // Returns a char* that needs to be marshaled to a .NET string
        [DllImport(IMPORT)]
        private static extern IntPtr GetPasswordHash(string password, string salt);

        [DllImport(IMPORT)]
        private static extern int GetLocalUserGroupCount(string userName);

        // Preallocate groupIDs as an unsigned integer array sized from GetLocalUserGroupCount
        [DllImport(IMPORT)]
        private static extern int GetLocalUserGroupIDs(string userName, int groupCount, ref uint[] groupsIDs);

        [DllImport(IMPORT)]
        private static extern int GetLocalGroupID(string groupName, out uint groupID);

        // Preallocate outbound userName to 256 characters
        [DllImport(IMPORT)]
        private static extern int GetLocalGroupName(uint uid, StringBuilder groupName);

        // Returns a char** that needs to be marshaled to a .NET string array
        [DllImport(IMPORT)]
        private static extern IntPtr GetLocalGroupMembers(string groupName);

        #endregion
    }
}