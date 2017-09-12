//******************************************************************************************************
//  LdapSecurityProvider.cs - Gbtc
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
//  07/08/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/03/2010 - Pinal C. Patel
//       Override the default behavior of TranslateRole() to translate a SID to its role name.
//  01/05/2011 - Pinal C. Patel
//       Added overrides to RefreshData(), UpdateData(), ResetPassword() and ChangePassword() methods.
//  02/14/2011 - J. Ritchie Carroll
//       Modified provider to be able to use local accounts when user is not connected to a domain.
//  06/09/2011 - Pinal C. Patel
//       Fixed a issue in the caching logic of RefreshData() method.
//  08/16/2011 - Pinal C. Patel
//       Made offline caching of user data for authentication purpose optional and turned on by default.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  03/08/2013 - Pinal C. Patel
//       Modified to enabled persistence on UserInfo only temporarily prior to calling Initialize() to 
//       load privileged user credentials if specified. This is to prevent any accidental updates to the 
//       config file when object gets disposed which would cause a web application to restart. 
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Principal;
using GSF.Configuration;
using GSF.Identity;

namespace GSF.Security
{
    /// <summary>
    /// Represents an <see cref="ISecurityProvider"/> that uses Active Directory for its backend data store and credential authentication.
    /// </summary>
    /// <remarks>
    /// A <a href="http://en.wikipedia.org/wiki/Security_Identifier" target="_blank">Security Identifier</a> can also be specified in 
    /// <b>IncludedResources</b> instead of a role name in the format of 'SID:&lt;Security Identifier&gt;' (Example: SID:S-1-5-21-19610888-1443184010-1631745340-269783).
    /// </remarks>
    /// <example>
    /// Required config file entries:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="GSF.Configuration.CategorizedSettingsSection, GSF.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <securityProvider>
    ///       <add name="ApplicationName" value="" description="Name of the application being secured as defined in the backend security datastore."
    ///         encrypted="false" />
    ///       <add name="ConnectionString" value="LDAP://DC=COMPANY,DC=COM" description="Connection string to be used for connection to the backend security data store."
    ///         encrypted="false" />
    ///       <add name="ProviderType" value="GSF.Security.LdapSecurityProvider, GSF.Security" description="The type to be used for enforcing security."
    ///         encrypted="false" />
    ///       <add name="UserCacheTimeout" value="5" description="Defines the timeout, in whole minutes, for a user's provider cache. Any value less than 1 will cause cache reset every minute."
    ///         encrypted="false" />
    ///       <add name="IncludedResources" value="*=*" description="Semicolon delimited list of resources to be secured along with role names."
    ///         encrypted="false" />
    ///       <add name="ExcludedResources" value="" description="Semicolon delimited list of resources to be excluded from being secured."
    ///         encrypted="false" />
    ///       <add name="NotificationSmtpServer" value="localhost" description="SMTP server to be used for sending out email notification messages."
    ///         encrypted="false" />
    ///       <add name="NotificationSenderEmail" value="sender@company.com" description="Email address of the sender of email notification messages." 
    ///         encrypted="false" />
    ///       <add name="EnableOfflineCaching" value="True" description="True to enable caching of user information for authentication in offline state, otherwise False."
    ///         encrypted="false" />
    ///       <add name="CacheRetryDelayInterval" value="200" description="Wait interval, in milliseconds, before retrying load of user data cache."
    ///         encrypted="false" />
    ///       <add name="CacheMaximumRetryAttempts" value="10" description="Maximum retry attempts allowed for loading user data cache."
    ///         encrypted="false" />
    ///     </securityProvider>
    ///     <activeDirectory>
    ///       <add name="PrivilegedDomain" value="" description="Domain of privileged domain user account."
    ///         encrypted="false" />
    ///       <add name="PrivilegedUserName" value="" description="Username of privileged domain user account."
    ///         encrypted="false" />
    ///       <add name="PrivilegedPassword" value="" description="Password of privileged domain user account."
    ///         encrypted="true" />
    ///     </activeDirectory>
    ///   </categorizedSettings>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    public class LdapSecurityProvider : SecurityProviderBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Defines the provider ID for the <see cref="LdapSecurityProvider"/>.
        /// </summary>
        public const int ProviderID = 0;

        /// <summary>
        /// Specifies the default value for the <see cref="EnableOfflineCaching"/> property.
        /// </summary>
        public const bool DefaultEnableOfflineCaching = true;

        /// <summary>
        /// Specifies the default value for the <see cref="CacheRetryDelayInterval"/> property.
        /// </summary>
        public const double DefaultCacheRetryDelayInterval = 1000.0D;

        /// <summary>
        /// Specifies the default value for the <see cref="CacheMaximumRetryAttempts"/> property.
        /// </summary>
        public const int DefaultCacheMaximumRetryAttempts = 5;

        // Fields
        private bool m_enableOfflineCaching;
        private double m_cacheRetryDelayInterval;
        private int m_cacheMaximumRetryAttempts;
        private WindowsPrincipal m_windowsPrincipal;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapSecurityProvider"/> class.
        /// </summary>
        /// <param name="username">Name that uniquely identifies the user.</param>
        public LdapSecurityProvider(string username)
            : this(username, true, false, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapSecurityProvider"/> class.
        /// </summary>
        /// <param name="username">Name that uniquely identifies the user.</param>
        /// <param name="canRefreshData">true if the security provider can refresh <see cref="UserData"/> from the backend data store, otherwise false.</param>
        /// <param name="canResetPassword">true if the security provider can reset user password, otherwise false.</param>
        /// <param name="canChangePassword">true if the security provider can change user password, otherwise false.</param>
        protected LdapSecurityProvider(string username, bool canRefreshData, bool canResetPassword, bool canChangePassword)
            : base(username, canRefreshData, canResetPassword, canChangePassword)
        {
            m_enableOfflineCaching = DefaultEnableOfflineCaching;
            m_cacheRetryDelayInterval = DefaultCacheRetryDelayInterval;
            m_cacheMaximumRetryAttempts = DefaultCacheMaximumRetryAttempts;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether user information is to be cached for offline authentication.
        /// </summary>
        public bool EnableOfflineCaching
        {
            get
            {
                return m_enableOfflineCaching;
            }
            set
            {
                m_enableOfflineCaching = value;
            }
        }

        /// <summary>
        /// Gets or sets the wait interval (in milliseconds) before retrying load of offline user data cache.
        /// </summary>
        public double CacheRetryDelayInterval
        {
            get
            {
                return m_cacheRetryDelayInterval;
            }
            set
            {
                m_cacheRetryDelayInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum retry attempts allowed for loading offline user data cache.
        /// </summary>
        public int CacheMaximumRetryAttempts
        {
            get
            {
                return m_cacheMaximumRetryAttempts;
            }
            set
            {
                m_cacheMaximumRetryAttempts = value;
            }
        }

        /// <summary>
        /// Gets the original <see cref="WindowsPrincipal"/> of the user if the user exists in Active Directory.
        /// </summary>
        public WindowsPrincipal WindowsPrincipal
        {
            get
            {
                return m_windowsPrincipal;
            }
            protected set
            {
                m_windowsPrincipal = value;
            }
        }

        #endregion

        #region [ Methods ]

        #region [ Not Supported ]

        /// <summary>
        /// Resets user password in the backend data store.
        /// </summary>
        /// <param name="securityAnswer">Answer to the user's security question.</param>
        /// <returns>true if the password is reset, otherwise false.</returns>
        /// <exception cref="NotSupportedException">Always</exception>
        public override bool ResetPassword(string securityAnswer)
        {
            throw new NotSupportedException();
        }

        #endregion

        /// <summary>
        /// Saves <see cref="LdapSecurityProvider"/> settings to the config file if the <see cref="SecurityProviderBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();

            if (PersistSettings)
            {
                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings["EnableOfflineCaching", true].Update(m_enableOfflineCaching);
                settings["CacheRetryDelayInterval", true].Update(m_cacheRetryDelayInterval);
                settings["CacheMaximumRetryAttempts", true].Update(m_cacheMaximumRetryAttempts);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="LdapSecurityProvider"/> settings from the config file if the <see cref="SecurityProviderBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();

            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("EnableOfflineCaching", m_enableOfflineCaching, "True to enable caching of user group information for authentication in offline state, otherwise False.");
                settings.Add("CacheRetryDelayInterval", m_cacheRetryDelayInterval, "Wait interval, in milliseconds, before retrying load of user data cache.");
                settings.Add("CacheMaximumRetryAttempts", m_cacheMaximumRetryAttempts, "Maximum retry attempts allowed for loading user data cache.");
                EnableOfflineCaching = settings["EnableOfflineCaching"].ValueAs(m_enableOfflineCaching);
                CacheRetryDelayInterval = settings["CacheRetryDelayInterval"].ValueAs(m_cacheRetryDelayInterval);
                CacheMaximumRetryAttempts = settings["CacheMaximumRetryAttempts"].ValueAs(m_cacheMaximumRetryAttempts);
            }
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <returns>true if the user is authenticated, otherwise false.</returns>
        public override bool Authenticate()
        {
            // Check prerequisites
            bool isValid =
                (UserData.IsDefined && !UserData.IsDisabled && !UserData.IsLockedOut) &&
                (UserData.PasswordChangeDateTime == DateTime.MinValue || UserData.PasswordChangeDateTime > DateTime.UtcNow);

            if (!isValid)
                return false;

            if (string.IsNullOrEmpty(Password))
            {
                // Validate with passthrough credentials
                m_windowsPrincipal = PassthroughPrincipal as WindowsPrincipal;

                IsUserAuthenticated =
                    (object)m_windowsPrincipal != null &&
                    ((!string.IsNullOrEmpty(UserData.LoginID) && m_windowsPrincipal.Identity.Name.Equals(UserData.LoginID, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(UserData.Username) && m_windowsPrincipal.Identity.Name.Equals(UserData.Username, StringComparison.OrdinalIgnoreCase))) &&
                    m_windowsPrincipal.Identity.IsAuthenticated;
            }
            else
            {
                // Validate by performing network logon
                string[] userParts = UserData.LoginID.Split('\\');
                m_windowsPrincipal = UserInfo.AuthenticateUser(userParts[0], userParts[1], Password) as WindowsPrincipal;
                IsUserAuthenticated = (object)m_windowsPrincipal != null && m_windowsPrincipal.Identity.IsAuthenticated;
            }

            return IsUserAuthenticated;
        }

        /// <summary>
        /// Refreshes the <see cref="UserData"/> from the backend data store.
        /// </summary>
        /// <returns>true if <see cref="SecurityProviderBase.UserData"/> is refreshed, otherwise false.</returns>
        public override bool RefreshData()
        {
            // For consistency with WindowIdentity principal, user groups are loaded into Roles collection
            UserData userData = new UserData(UserData.Username);
            bool result = RefreshData(userData, userData.Roles, ProviderID);

            if (result)
            {
                string[] parts;

                // Remove domain name prefixes from user group names (again to match WindowIdentity principal implementation)
                for (int i = 0; i < userData.Roles.Count; i++)
                {
                    parts = userData.Roles[i].Split('\\');

                    if (parts.Length == 2)
                        userData.Roles[i] = parts[1];
                }
            }
            
            UserData = userData;

            return result;
        }

        /// <summary>
        /// Refreshes the <see cref="UserData"/> from the backend data store loading user groups into desired collection.
        /// </summary>
        /// <param name="userData">The structure for the data being refreshed.</param>
        /// <param name="groupCollection">Target collection for user groups.</param>
        /// <param name="providerID">Unique provider ID used to distinguish cached user data that may be different based on provider.</param>
        /// <returns>true if <see cref="SecurityProviderBase.UserData"/> is refreshed, otherwise false.</returns>
        protected virtual bool RefreshData(UserData userData, List<string> groupCollection, int providerID)
        {
            if ((object)groupCollection == null)
                throw new ArgumentNullException(nameof(groupCollection));

            if (string.IsNullOrEmpty(userData.Username))
                return false;

            // Initialize user data
            userData.Initialize();

            // Populate user data
            UserInfo user = null;
            UserDataCache userDataCache = null;

            try
            {
                // Get current local user data cache
                if (m_enableOfflineCaching)
                {
                    userDataCache = UserDataCache.GetCurrentCache(providerID);
                    userDataCache.RetryDelayInterval = m_cacheRetryDelayInterval;
                    userDataCache.MaximumRetryAttempts = m_cacheMaximumRetryAttempts;
                    userDataCache.ReloadOnChange = false;
                    userDataCache.AutoSave = true;
                    userDataCache.Load();
                }

                // Create user info object using specified LDAP path if provided
                string ldapPath = GetLdapPath();

                if (string.IsNullOrEmpty(ldapPath))
                    user = new UserInfo(userData.Username);
                else
                    user = new UserInfo(userData.Username, ldapPath);

                // Make sure to load privileged user credentials from config file if present.
                user.PersistSettings = true;

                // Attempt to determine if user exists (this will initialize user object if not initialized already)
                userData.IsDefined = user.Exists;
                userData.LoginID = user.LoginID;

                if (userData.IsDefined)
                {
                    // Fill in user information from domain data if it is available
                    if (user.DomainRespondsForUser)
                    {
                        // Copy relevant user information
                        userData.FirstName = user.FirstName;
                        userData.LastName = user.LastName;
                        userData.CompanyName = user.Company;
                        userData.PhoneNumber = user.Telephone;
                        userData.EmailAddress = user.Email;

                        try
                        {
                            userData.IsLockedOut = user.AccountIsLockedOut;
                            userData.IsDisabled = user.AccountIsDisabled;
                        }
                        catch (SecurityException)
                        {
                            // AD may restrict information on account availability, if so, have to make a safe assumption:
                            userData.IsLockedOut = true;
                            userData.IsDisabled = true;
                        }

                        userData.PasswordChangeDateTime = user.NextPasswordChangeDate;
                        userData.AccountCreatedDateTime = user.AccountCreationDate;

                        // Assign all groups the user is a member of
                        foreach (string groupName in user.Groups)
                        {
                            if (!groupCollection.Contains(groupName, StringComparer.OrdinalIgnoreCase))
                                groupCollection.Add(groupName);
                        }

                        if ((object)userDataCache != null)
                        {
                            // Cache user data so that information can be loaded later if domain is unavailable
                            userDataCache[userData.LoginID] = userData;

                            // Wait for pending serialization since cache is scoped locally to this method and will be disposed before exit
                            userDataCache.WaitForSave();
                        }
                    }
                    else
                    {
                        // Attempt to load previously cached user information when domain is offline
                        UserData cachedUserData;

                        if ((object)userDataCache != null && userDataCache.TryGetUserData(userData.LoginID, out cachedUserData))
                        {
                            // Copy relevant cached user information
                            userData.FirstName = cachedUserData.FirstName;
                            userData.LastName = cachedUserData.LastName;
                            userData.CompanyName = cachedUserData.CompanyName;
                            userData.PhoneNumber = cachedUserData.PhoneNumber;
                            userData.EmailAddress = cachedUserData.EmailAddress;
                            userData.IsLockedOut = cachedUserData.IsLockedOut;
                            userData.IsDisabled = cachedUserData.IsDisabled;
                            userData.Roles.AddRange(cachedUserData.Roles);
                            userData.Groups.AddRange(cachedUserData.Groups);

                            // If domain is offline, a password change cannot be initiated
                            userData.PasswordChangeDateTime = DateTime.MaxValue;
                            userData.AccountCreatedDateTime = cachedUserData.AccountCreatedDateTime;
                        }
                        else
                        {
                            // No previous user data was cached but Windows allowed authentication, so all we know is that user exists
                            userData.IsLockedOut = false;
                            userData.IsDisabled = false;
                            userData.PasswordChangeDateTime = DateTime.MaxValue;
                            userData.AccountCreatedDateTime = DateTime.MinValue;
                        }
                    }
                }

                return userData.IsDefined;
            }
            finally
            {
                if ((object)user != null)
                {
                    user.PersistSettings = false;
                    user.Dispose();
                }

                if ((object)userDataCache != null)
                    userDataCache.Dispose();
            }
        }

        /// <summary>
        /// Changes user password in the backend data store.
        /// </summary>
        /// <param name="oldPassword">User's current password.</param>
        /// <param name="newPassword">User's new password.</param>
        /// <returns>true if the password is changed, otherwise false.</returns>
        /// <remarks>
        /// This method always returns <c>false</c> under Mono deployments.
        /// </remarks>
        public override bool ChangePassword(string oldPassword, string newPassword)
        {
            // Check prerequisites
            if (!UserData.IsDefined || UserData.IsDisabled || UserData.IsLockedOut)
                return false;

            UserInfo user = null;
            WindowsImpersonationContext context = null;

            try
            {
                string ldapPath = GetLdapPath();

                // Create user info object using specified LDAP path if provided
                if (string.IsNullOrEmpty(ldapPath))
                    user = new UserInfo(UserData.Username);
                else
                    user = new UserInfo(UserData.Username, ldapPath);

                // Initialize user entry
                user.PersistSettings = true;
                user.Initialize();

                // Impersonate privileged user
                context = user.ImpersonatePrivilegedAccount();

                // Change user password
                user.ChangePassword(oldPassword, newPassword);

                return true;
            }
            catch (TargetInvocationException ex)
            {
                // Propagate password change error
                if ((object)ex.InnerException == null)
                    throw new SecurityException(ex.Message, ex);
                else
                    throw new SecurityException(ex.InnerException.Message, ex);
            }
            finally
            {
                if ((object)user != null)
                    user.Dispose();

                if ((object)context != null)
                    UserInfo.EndImpersonation(context);

                RefreshData();
            }
        }

        /// <summary>
        /// Performs a translation of the specified user <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The user role to be translated.</param>
        /// <returns>The user role that the specified user <paramref name="role"/> translates to.</returns>
        public override string TranslateRole(string role)
        {
            // Perform a translation from SID to Role only if the input starts with 'SID:'
            if (role.StartsWith("SID:", StringComparison.CurrentCultureIgnoreCase))
                return new SecurityIdentifier(role.Remove(0, 4)).Translate(typeof(NTAccount)).ToString().Split('\\')[1];

            return role;
        }

        /// <summary>
        /// Gets the LDAP path.
        /// </summary>
        /// <returns>The LDAP path.</returns>
        protected virtual string GetLdapPath()
        {
            if (ConnectionString.StartsWith("LDAP://", StringComparison.OrdinalIgnoreCase) ||
                ConnectionString.StartsWith("LDAPS://", StringComparison.OrdinalIgnoreCase))
                return ConnectionString;

            foreach (KeyValuePair<string, string> pair in ConnectionString.ParseKeyValuePairs())
            {
                if (pair.Value.StartsWith("LDAP://", StringComparison.OrdinalIgnoreCase) ||
                    pair.Value.StartsWith("LDAPS://", StringComparison.OrdinalIgnoreCase))
                    return pair.Value;
            }

            return null;
        }

        #endregion
    }
}
