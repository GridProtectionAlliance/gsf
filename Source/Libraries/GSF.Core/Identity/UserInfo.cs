//******************************************************************************************************
//  UserInfo.cs - Gbtc
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
//  01/10/2004 - J. Ritchie Carroll
//       Original version of source code generated.
//  01/03/2006 - Pinal C. Patel
//       2.0 version of source code migrated from 1.1 source (GSF.Shared.Identity).
//  09/27/2006 - Pinal C. Patel
//       Added Authenticate() function.
//  09/29/2006 - Pinal C. Patel
//       Added support to impersonate privileged user for retrieving user information.
//  11/06/2007 - Pinal C. Patel
//       Modified the logic of Authenticate method to use the user's DirectoryEntry instance.
//       Modified UserEntry property to impersonate privileged user if configured for the instance.
//  11/08/2007 - J. Ritchie Carroll
//       Implemented user customizable implementation of privileged account credentials.
//  09/15/2008 - J. Ritchie Carroll
//       Converted to C#.
//  10/06/2008 - Pinal C. Patel
//       Edited code comments.
//  10/06/2008 - Pinal C. Patel
//       Added static properties RemoteUserID and RemoteUserInfo.
//  06/18/2009 - Pinal C. Patel
//       Modified GetUserProperty() to quit if Enabled is false.
//  07/14/2009 - Pinal C. Patel
//       Modified GetUserProperty() to allow information to be retrieved from the logged-on domain only
//       in order to prevent timeout issues when trying to retrieve information for a non-domain user.
//  08/06/2009 - Pinal C. Patel
//       Enabled upon initialization rather than instantiation.
//  08/12/2009 - Pinal C. Patel
//       Fixed issue introduced by marking the type as enabled upon initialization (Initialize()).
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  03/11/2010 - Pinal C. Patel
//       Modified AuthenticateUser() to return IPrincipal of the authenticated user instead of boolean.
//  04/07/2010 - Pinal C. Patel
//       Updated the timeout prevention logic used in GetUserProperty() to work correctly when 
//       privileged identity is specified for Active Directory operations.
//  05/24/2010 - Pinal C. Patel
//       Added the ability to derive the domain name for added flexibility if one is not specified.
//  05/27/2010 - Pinal C. Patel
//       Modified DirectoryEntry object creation in Initialize() to take in to consideration the domain.
//       Modified Initialize() to move the derivation of domain name after DirectoryEntry initialization.
//  06/17/2010 - Pinal C. Patel
//       Modified the process of getting the user's DirectoryEntry object in Initialize() method to use 
//       the default constructor of DirectorySearcher resulting in the search for the user to be 
//       performed in the domain to which the host machine (where this code is executing) is joined.
//  07/12/2010 - Pinal C. Patel
//       Added publicly accessible ImpersonatePrivilegedAccount() method allow for the impersonation of
//       privileged domain account outside of UserInfo class.
//       Added a constructor to allow for the AD search root to be specified.
//  11/04/2010 - Pinal C. Patel
//       Modified Initialize() to initialize UserEntry only if the machine is joined to a domain.
//  02/14/2011 - J. Ritchie Carroll
//       Added WinNT DirectoryEntry lookups for local accounts when user is not connected to a domain.
//  02/15/2011 - J. Ritchie Carroll
//       Modified "Groups" property to always return domain prefixed group names for the user.
//       Modified to allow default lookup of account name from machines not connected to a domain even 
//       if local machine name is not specified as domain name -and- to allow lookup of local account
//       information even when connected to a domain if machine name is specified as domain name.
//  02/23/2011 - Pinal C. Patel
//       Fixed a issue in the Active Directory lookup of MaximumPasswordAge property.
//  04/04/2011 - J. Ritchie Carroll
//       Added Exists and DomainAvailable properties to accommodate testing when domain server is
//       not available (such as when laptop is used when not connected to the domain).
//  04/05/2011 - J. Ritchie Carroll
//       Updated class to attempt reintialization after system resume in case user no longer has
//       access to domain.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  11/23/2011 - J. Ritchie Carroll
//       Modified to support new life cycle interface requirements (i.e., Disposed event).
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  03/05/2013 - Joe France
//      Prefer the lastLogonTimestamp property over lastLogon property.
//  03/06/2013 - Pinal C. Patel
//       Added new GetUserPropertyValue() and GetUserPropertyValueAsString() methods to replace old
//       GetUserProperty() method and marked GetUserPropertyValue() as obsolete.
//       Updated LastLogon to revert using lastLogon property instead of lastLogonTimestamp since it is
//       updated less frequently and fixed the parsing logic for it since the return value is a large
//       integer.
//  06/03/2013 - J. Ritchie Carroll
//       Added static local user/group manipulation functions.
//  01/02/2014 - Stephen C. Wills
//       Added static account-to-SID conversion functions.
//
//******************************************************************************************************

using System;
using System.Configuration;
using System.Security;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.Configuration;

namespace GSF.Identity
{
    /// <summary>
    /// Represents information about a local user or a domain user (e.g., from Active Directory).
    /// </summary>
    /// <remarks>
    /// See <a href="http://msdn.microsoft.com/en-us/library/ms677980.aspx" target="_blank">http://msdn.microsoft.com/en-us/library/ms677980.aspx</a> for more information on active directory properties.
    /// </remarks>
    /// <example>
    /// This example shows how to retrieve user information from Active Directory:
    /// <code>
    /// using System;
    /// using GSF.Identity;
    ///
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // Retrieve and display user information from Active Directory.
    ///         using (UserInfo user = new UserInfo("XYZCorp\\johndoe"))
    ///         {
    ///             Console.WriteLine(string.Format("First Name: {0}", user.FirstName));
    ///             Console.WriteLine(string.Format("Last Name: {0}", user.LastName));
    ///             Console.WriteLine(string.Format("Middle Initial: {0}", user.MiddleInitial));
    ///             Console.WriteLine(string.Format("Email Address: {0}", user.Email));
    ///             Console.WriteLine(string.Format("Telephone Number: {0}", user.Telephone));
    ///         }
    ///
    ///         Console.ReadLine();
    ///     }
    /// }
    /// </code>
    /// This example shows the config file section that can be used to specify the domain account to be used for Active Directory queries:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="GSF.Configuration.CategorizedSettingsSection, GSF.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
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
    public sealed class UserInfo : ISupportLifecycle, IPersistSettings
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "ActiveDirectory";

        internal const string SecurityExceptionFormat = "{0} user account control information cannot be obtained. {1} may not have needed rights to {2}.";
        internal const string UnknownErrorFormat = "Unknown error. Invalid value returned when querying user account control for {0}. Value: '{1}'";
        internal const int ACCOUNTDISABLED = 2;
        internal const int LOCKED = 16;
        internal const int PASSWD_CANT_CHANGE = 64;
        internal const int DONT_EXPIRE_PASSWORD = 65536;

        // Events

        /// <summary>
        /// Occurs when the class has been disposed.
        /// </summary>
        public event EventHandler Disposed;

        // Fields
        private IUserInfo m_userInfo;
        private string m_domain;
        private readonly string m_userName;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private IPrincipal m_passthroughPrincipal;
        private int m_userAccountControl;
        private bool m_disposed;

        internal string m_ldapPath;
        internal string m_privilegedDomain;
        internal string m_privilegedUserName;
        internal string m_privilegedPassword;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfo"/> class.
        /// </summary>
        /// <param name="loginID">
        /// Login ID in 'domain\username' format of the user's account whose information is to be retrieved. Login ID 
        /// can also be specified in 'username' format without the domain name, in which case the domain name will be
        /// approximated based on the privileged user domain if specified, default logon domain of the host machine 
        /// if available, or the domain of the identity that owns the host process.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="loginID"/> is a null or empty string.</exception>
        public UserInfo(string loginID)
            : this(loginID, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfo"/> class.
        /// </summary>
        /// <param name="loginID">
        /// Login ID in 'domain\username' format of the user's account whose information is to be retrieved. Login ID 
        /// can also be specified in 'username' format without the domain name, in which case the domain name will be
        /// approximated based on the privileged user domain if specified, default logon domain of the host machine 
        /// if available, or the domain of the identity that owns the host process.
        /// </param>
        /// <param name="ldapPath">
        /// String in 'LDAP://' format that specifies the Active Directory node where search for the user starts.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="loginID"/> is a null or empty string.</exception>
        public UserInfo(string loginID, string ldapPath)
        {
            if (string.IsNullOrEmpty(loginID))
                throw new ArgumentNullException(nameof(loginID));

            string[] accountParts = loginID.Split('\\');

            if (accountParts.Length != 2)
            {
                accountParts = loginID.Split('@');

                if (accountParts.Length != 2)
                {
                    // Login ID is specified in 'username' format.
                    m_userName = loginID;
                }
                else
                {
                    // Login ID is specified in 'username@domain' format.
                    m_userName = accountParts[0];
                    m_domain = accountParts[1];
                }
            }
            else
            {
                // Login ID is specified in 'domain\username' format.
                m_domain = accountParts[0];
                m_userName = accountParts[1];
            }

            m_ldapPath = ldapPath;

            // Initialize default settings
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_userAccountControl = -1;

            if (Common.IsPosixEnvironment)
                m_userInfo = new UnixUserInfo(this);
            else
                m_userInfo = new WindowsUserInfo(this);
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="UserInfo"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~UserInfo()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        bool ISupportLifecycle.Enabled
        {
            get
            {
                return m_userInfo.Enabled;
            }
            set
            {
                m_userInfo.Enabled = value;
            }
        }

        /// <summary>
        /// Gets a flag that indicates whether the object has been disposed.
        /// </summary>
        bool ISupportLifecycle.IsDisposed
        {
            get
            {
                return m_disposed;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="UserInfo"/> object are 
        /// to be saved to the config file.
        /// </summary>
        public bool PersistSettings
        {
            get
            {
                return m_persistSettings;
            }
            set
            {
                m_persistSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the category under which the settings of <see cref="UserInfo"/> object are to be saved
        /// to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty string.</exception>
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets the principal used for passthrough authentication.
        /// </summary>
        /// <remarks>
        /// This is necessary to determine whether a domain user exists when the
        /// computer is disconnected from the domain but still able to authenticate
        /// using the last seen domain user account.
        /// </remarks>
        public IPrincipal PassthroughPrincipal
        {
            get
            {
                return m_passthroughPrincipal;
            }
            set
            {
                m_passthroughPrincipal = value;
            }
        }

        /// <summary>
        /// Gets the domain for the user.
        /// </summary>
        public string Domain
        {
            get
            {
                return m_domain;
            }
            internal set
            {
                m_domain = value;
            }
        }

        /// <summary>
        /// Gets the user name of the user.
        /// </summary>
        public string UserName
        {
            get
            {
                return m_userName;
            }
        }

        /// <summary>
        /// Gets LDAP path defined for this user, if any.
        /// </summary>
        public string LdapPath
        {
            get
            {
                return m_ldapPath;
            }
        }

        /// <summary>
        /// Gets the Login ID of the user.
        /// </summary>
        public string LoginID
        {
            get
            {
                return string.Format("{0}\\{1}", m_domain, m_userName);
            }
        }

        /// <summary>
        /// Gets the ID of the user in LDAP format.
        /// </summary>
        public string LdapID
        {
            get
            {
                return string.Format("{0}@{1}", m_userName, m_domain);
            }
        }

        /// <summary>
        /// Gets flag that determines if domain is responding to user existence.
        /// </summary>
        /// <returns><c>true</c> if domain responds and user exists; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Note that when the domain is unavailable, this function will return <c>false</c>.
        /// </remarks>
        public bool DomainRespondsForUser
        {
            get
            {
                return m_userInfo.DomainRespondsForUser;
            }
        }

        /// <summary>
        /// Gets flag that determines if user exists.
        /// </summary>
        /// <returns><c>true</c> if user is found to exist; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// Unlike other properties in this class, <see cref="Exists"/> will still work even if the domain server is
        /// unavailable. Calling this property without access to a domain server will return <c>true</c> if the user
        /// is authenticated. This only succeeds if the current user is the same as the <see cref="LoginID"/>, there
        /// is no way to determine if a user other than the currently authenticated user exists without access to
        /// the domain server. Local accounts are not subject to these constraints since the domain server will
        /// effectively be your local computer which is always accessible.
        /// </para>
        /// <para>
        /// Since this property can check the <see cref="WindowsPrincipal"/> to determine if the current thread identity
        /// is authenticated, it is important that consumers add the following code at application startup:
        /// <code>AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);</code>
        /// </para>
        /// <para>
        /// If a laptop user that's normally connected to the domain takes their computer on the road without VPN or other
        /// connectivity to the domain, the user can still login to the laptop using cached credentials therefore they are
        /// still considered to exist. Without access to the domain, no user information will be available (such as the
        /// <see cref="Groups"/> listing) - if this data is needed offline it will need to be cached by the application in
        /// anticipation of offline access.
        /// </para>
        /// </remarks>
        public bool Exists
        {
            get
            {
                return m_userInfo.Exists;
            }
        }

        /// <summary>
        /// Gets the last login time of the user.
        /// </summary>
        public DateTime LastLogon
        {
            get
            {
                return m_userInfo.LastLogon;
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> when the account was created.
        /// </summary>
        public DateTime AccountCreationDate
        {
            get
            {
                return m_userInfo.AccountCreationDate;
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/>, in UTC, of next password change for the user.
        /// </summary>
        public DateTime NextPasswordChangeDate
        {
            get
            {
                return m_userInfo.NextPasswordChangeDate;
            }
        }

        /// <summary>
        /// Gets the account control information of the user.
        /// </summary>
        /// <exception cref="SecurityException">User account control information cannot be obtained, may not have needed rights.</exception>
        /// <exception cref="InvalidOperationException">Unknown error. Invalid value returned when querying user account control.</exception>
        public int UserAccountControl
        {
            get
            {
                if (m_userInfo.Enabled && m_userAccountControl == -1)
                {
                    if (m_userInfo.IsLocalAccount)
                    {
                        m_userAccountControl = m_userInfo.LocalUserAccountControl;
                    }
                    else
                    {
                        string userPropertyValue = GetUserPropertyValue("userAccountControl");

                        if (string.IsNullOrEmpty(userPropertyValue))
                            throw new SecurityException(string.Format(SecurityExceptionFormat, "Active directory", CurrentUserID, m_domain));

                        if (!int.TryParse(userPropertyValue, out m_userAccountControl))
                            throw new InvalidOperationException(string.Format(UnknownErrorFormat, LoginID, userPropertyValue));
                    }
                }

                return m_userAccountControl;
            }
            internal set
            {
                m_userAccountControl = value;
            }
        }

        /// <summary>
        /// Gets flag that determines if account is locked-out for this user.
        /// </summary>
        /// <exception cref="SecurityException">User account control information cannot be obtained, may not have needed rights.</exception>
        /// <exception cref="InvalidOperationException">Unknown error. Invalid value returned when querying user account control.</exception>
        public bool AccountIsLockedOut
        {
            get
            {
                return Convert.ToBoolean(UserAccountControl & LOCKED);
            }
        }

        /// <summary>
        /// Gets flag that determines if account is disabled for this user.
        /// </summary>
        /// <exception cref="SecurityException">User account control information cannot be obtained, may not have needed rights.</exception>
        /// <exception cref="InvalidOperationException">Unknown error. Invalid value returned when querying user account control.</exception>
        public bool AccountIsDisabled
        {
            get
            {
                return Convert.ToBoolean(UserAccountControl & ACCOUNTDISABLED);
            }
        }

        /// <summary>
        /// Gets flag that determines if account password cannot change for this user.
        /// </summary>
        /// <exception cref="SecurityException">User account control information cannot be obtained, may not have needed rights.</exception>
        /// <exception cref="InvalidOperationException">Unknown error. Invalid value returned when querying user account control.</exception>
        public bool PasswordCannotChange
        {
            get
            {
                return Convert.ToBoolean(UserAccountControl & PASSWD_CANT_CHANGE);
            }
        }

        /// <summary>
        /// Gets flag that determines if account password does not expire for this user.
        /// </summary>
        /// <exception cref="SecurityException">User account control information cannot be obtained, may not have needed rights.</exception>
        /// <exception cref="InvalidOperationException">Unknown error. Invalid value returned when querying user account control.</exception>
        public bool PasswordDoesNotExpire
        {
            get
            {
                return Convert.ToBoolean(UserAccountControl & DONT_EXPIRE_PASSWORD);
            }
        }

        /// <summary>
        /// Gets this maximum password age for the user.
        /// </summary>
        public Ticks MaximumPasswordAge
        {
            get
            {
                return m_userInfo.MaximumPasswordAge;
            }
        }

        /// <summary>
        /// Gets all the groups associated with the user - this includes local groups and Active Directory groups if applicable.
        /// </summary>
        /// <remarks>
        /// Groups names are prefixed with their associated domain, computer name or BUILTIN.
        /// </remarks>
        public string[] Groups
        {
            get
            {
                return m_userInfo.Groups;
            }
        }

        /// <summary>
        /// Gets the local groups the user is a member of.
        /// </summary>
        /// <remarks>
        /// Groups names are prefixed with BUILTIN or computer name.
        /// </remarks>
        public string[] LocalGroups
        {
            get
            {
                return m_userInfo.LocalGroups;
            }
        }

        /// <summary>
        /// Gets the First Name of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "givenName" active directory property.</remarks>
        public string FirstName
        {
            get
            {
                if (m_userInfo.IsLocalAccount)
                    return GetNameElements(DisplayName)[0];

                return GetUserPropertyValue("givenName");
            }
        }

        /// <summary>
        /// Gets the Last Name of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "sn" active directory property.</remarks>
        public string LastName
        {
            get
            {
                if (m_userInfo.IsLocalAccount)
                    return GetNameElements(DisplayName)[1];

                return GetUserPropertyValue("sn");
            }
        }

        /// <summary>
        /// Gets the Display Name the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "displayName" active directory property.</remarks>
        public string DisplayName
        {
            get
            {
                if (m_userInfo.IsLocalAccount)
                    return m_userInfo.FullLocalUserName;

                return GetUserPropertyValue("displayName");
            }
        }

        /// <summary>
        /// Gets the Middle Initial of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "initials" active directory property.</remarks>
        public string MiddleInitial
        {
            get
            {
                return GetUserPropertyValue("initials");
            }
        }

        /// <summary>
        /// Gets the Full Name of the user.
        /// </summary>
        /// <remarks>Returns the concatenation of <see cref="FirstName"/>, <see cref="MiddleInitial"/> and <see cref="LastName"/> properties.</remarks>
        public string FullName
        {
            get
            {
                if (m_userInfo.IsLocalAccount)
                    return m_userInfo.FullLocalUserName;

                string fName = FirstName;
                string lName = LastName;
                string mInitial = MiddleInitial;

                if (!string.IsNullOrEmpty(fName) && !string.IsNullOrEmpty(lName))
                {
                    if (string.IsNullOrEmpty(mInitial))
                        return fName + " " + lName;

                    return fName + " " + mInitial + ". " + lName;
                }

                return LoginID;
            }
        }

        /// <summary>
        /// Gets the E-Mail address of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "mail" active directory property.</remarks>
        public string Email
        {
            get
            {
                return GetUserPropertyValue("mail");
            }
        }

        /// <summary>
        /// Gets the web page address of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "wWWHomePage" active directory property.</remarks>
        public string Webpage
        {
            get
            {
                return GetUserPropertyValue("wWWHomePage");
            }
        }

        /// <summary>
        /// Gets the description specified for the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "description" active directory property.</remarks>
        public string Description
        {
            get
            {
                return GetUserPropertyValue("description");
            }
        }

        /// <summary>
        /// Gets the Telephone Number of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "telephoneNumber" active directory property.</remarks>
        public string Telephone
        {
            get
            {
                return GetUserPropertyValue("telephoneNumber");
            }
        }

        /// <summary>
        /// Gets the Title of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "title" active directory property.</remarks>
        public string Title
        {
            get
            {
                return GetUserPropertyValue("title");
            }
        }

        /// <summary>
        /// Gets the Company of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "company" active directory property.</remarks>
        public string Company
        {
            get
            {
                return GetUserPropertyValue("company");
            }
        }

        /// <summary>
        /// Gets the Office location of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "physicalDeliveryOfficeName" active directory property.</remarks>
        public string Office
        {
            get
            {
                return GetUserPropertyValue("physicalDeliveryOfficeName");
            }
        }

        /// <summary>
        /// Gets the Department where the user works.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "department" active directory property.</remarks>
        public string Department
        {
            get
            {
                return GetUserPropertyValue("department");
            }
        }

        /// <summary>
        /// Gets the City where the user works.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "l" active directory property.</remarks>
        public string City
        {
            get
            {
                return GetUserPropertyValue("l");
            }
        }

        /// <summary>
        /// Gets the Mailbox address of where the user works.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "streetAddress" active directory property.</remarks>
        public string Mailbox
        {
            get
            {
                return GetUserPropertyValue("streetAddress");
            }
        }

        /// <summary>
        /// Gets flag that determines if this <see cref="UserInfo"/> instance is based on a local account instead of found through LDAP.
        /// </summary>
        public bool IsLocalAccount
        {
            get
            {
                return m_userInfo.IsLocalAccount;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="UserInfo"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="UserInfo"/> object and optionally releases the managed resources.
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
                        if ((object)m_userInfo != null)
                            m_userInfo.Dispose();

                        m_userInfo = null;

                        // This will be done only when the object is disposed by calling Dispose().
                        SaveSettings();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.

                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Initializes the <see cref="UserInfo"/> object.
        /// </summary>
        /// <exception cref="InitializationException">Failed to initialize directory entry for <see cref="LoginID"/>.</exception>
        public void Initialize()
        {
            m_userInfo.Initialize();

            if (string.IsNullOrEmpty(m_domain))
                m_domain = Environment.MachineName;
        }

        /// <summary>
        /// Saves settings for the <see cref="UserInfo"/> object to the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings["PrivilegedDomain", true].Update(m_privilegedDomain);
                settings["PrivilegedUserName", true].Update(m_privilegedUserName);
                settings["PrivilegedPassword", true].Update(m_privilegedPassword);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="UserInfo"/> object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("PrivilegedDomain", m_privilegedDomain, "Domain of privileged domain user account used for Active Directory information lookup, if needed.");
                settings.Add("PrivilegedUserName", m_privilegedUserName, "Username of privileged domain user account used for Active Directory information lookup, if needed.");
                settings.Add("PrivilegedPassword", m_privilegedPassword, "Encrypted password of privileged domain user account used for Active Directory information lookup, if needed.", true);
                m_privilegedDomain = settings["PrivilegedDomain"].ValueAs(m_privilegedDomain);
                m_privilegedUserName = settings["PrivilegedUserName"].ValueAs(m_privilegedUserName);
                m_privilegedPassword = settings["PrivilegedPassword"].ValueAs(m_privilegedPassword);
            }
        }

        /// <summary>
        /// Defines the credentials of a privileged domain account that can be used for impersonation prior to the 
        /// retrieval of user information from the Active Directory.
        /// </summary>
        /// <param name="domain">Domain of privileged domain user account.</param>
        /// <param name="username">Username of privileged domain user account.</param>
        /// <param name="password">Password of privileged domain user account.</param>
        /// <example>
        /// This example shows how to define the identity of the user to be used for retrieving information from Active Directory:
        /// <code>
        /// using System;
        /// using GSF.Identity;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         using (UserInfo user = new UserInfo("XYZCorp\\johndoe"))
        ///         {
        ///             // Define the identity to use for retrieving Active Directory information.
        ///             // Persist identity credentials encrypted to the config for easy access.
        ///             user.PersistSettings = true;
        ///             user.DefinePrivilegedAccount("XYZCorp", "admin", "Passw0rd");
        ///
        ///             // Retrieve and display user information from Active Directory.
        ///             Console.WriteLine(string.Format("First Name: {0}", user.FirstName));
        ///             Console.WriteLine(string.Format("Last Name: {0}", user.LastName));
        ///             Console.WriteLine(string.Format("Middle Initial: {0}", user.MiddleInitial));
        ///             Console.WriteLine(string.Format("Email Address: {0}", user.Email));
        ///             Console.WriteLine(string.Format("Telephone Number: {0}", user.Telephone));
        ///         }
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        ///</example>
        public void DefinePrivilegedAccount(string domain, string username, string password)
        {
            // Check input parameters.
            if (string.IsNullOrEmpty(domain))
                throw new ArgumentNullException(nameof(domain));

            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            // Set the credentials for privileged domain user account.
            m_privilegedDomain = domain;
            m_privilegedUserName = username;
            m_privilegedPassword = password;
        }

        /// <summary>
        /// Impersonates the defined privileged domain account.
        /// </summary>
        /// <returns>An <see cref="WindowsImpersonationContext"/> if privileged domain account has been defined, otherwise null.</returns>
        public WindowsImpersonationContext ImpersonatePrivilegedAccount()
        {
            if (!string.IsNullOrEmpty(m_privilegedDomain) &&
                !string.IsNullOrEmpty(m_privilegedUserName) &&
                !string.IsNullOrEmpty(m_privilegedPassword))
            {
                // Privileged domain account is specified
                return ImpersonateUser(m_privilegedDomain, m_privilegedUserName, m_privilegedPassword);
            }

            // Privileged domain account is not specified
            return null;
        }

        /// <summary>
        /// Attempts to change the user's password.
        /// </summary>
        /// <param name="oldPassword">Old password.</param>
        /// <param name="newPassword">New password.</param>
        public void ChangePassword(string oldPassword, string newPassword)
        {
            m_userInfo.ChangePassword(oldPassword, newPassword);
        }

        /// <summary>
        /// Returns the value for specified active directory property.
        /// </summary>
        /// <param name="propertyName">Name of the active directory property whose value is to be retrieved.</param>
        /// <returns><see cref="String"/> value for the specified active directory property.</returns>
        public string GetUserPropertyValue(string propertyName)
        {
            return m_userInfo.GetUserPropertyValue(propertyName);
        }

        // Split a string into first name and last name
        private static string[] GetNameElements(string displayName)
        {
            displayName = displayName.Trim();

            if (!string.IsNullOrEmpty(displayName))
            {
                int lastSplit = displayName.LastIndexOf(' ');

                if (lastSplit >= 0)
                    return new[] { displayName.Substring(0, lastSplit), displayName.Substring(lastSplit + 1) };

                return new[] { displayName, "" };
            }

            return new[] { "", "" };
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static string m_lastUserID;
        private static UserInfo s_currentUserInfo;

        // Static Properties

        /// <summary>
        /// Gets the ID name of the current user.
        /// </summary>
        /// <remarks>
        /// The ID name returned is that of the user account under which the code is executing.
        /// </remarks>
        public static string CurrentUserID
        {
            get
            {
                try
                {
                    WindowsIdentity identity = WindowsIdentity.GetCurrent();

                    if ((object)identity != null)
                        return identity.Name;

                    return null;
                }
                catch (SecurityException)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="UserInfo"/> object for the <see cref="CurrentUserID"/>.
        /// </summary>
        public static UserInfo CurrentUserInfo
        {
            get
            {
                string currentUserID = CurrentUserID;

                if (!string.IsNullOrEmpty(currentUserID))
                {
                    if ((object)s_currentUserInfo == null ||
                        string.IsNullOrEmpty(m_lastUserID) ||
                        !currentUserID.Equals(m_lastUserID, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            s_currentUserInfo = new UserInfo(currentUserID);
                            s_currentUserInfo.Initialize();
                        }
                        catch (InitializationException)
                        {
                        }
                    }
                }

                m_lastUserID = currentUserID;

                return s_currentUserInfo;
            }
        }

        /// <summary>
        /// Gets the <see cref="LoginID"/> of the remote web user.
        /// </summary>
        /// <remarks>
        /// The <see cref="LoginID"/> returned is that of the remote user accessing the web application. This is 
        /// available only if the virtual directory hosting the web application is configured to use "Integrated 
        /// Windows Authentication".
        /// </remarks>
        public static string RemoteUserID
        {
            get
            {
                return Thread.CurrentPrincipal.Identity.Name;
            }
        }

        /// <summary>
        /// Gets the <see cref="UserInfo"/> object for the <see cref="RemoteUserID"/>.
        /// </summary>
        public static UserInfo RemoteUserInfo
        {
            get
            {
                string userID = RemoteUserID;

                if ((object)userID == null)
                    return null;

                return new UserInfo(RemoteUserID);
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the current machine is joined to a domain (non-local such as AD or LDAP).
        /// </summary>
        public static bool MachineIsJoinedToDomain
        {
            get
            {
                if (Common.IsPosixEnvironment)
                    return UnixUserInfo.MachineIsJoinedToDomain;

                return WindowsUserInfo.MachineIsJoinedToDomain;
            }
        }

        // Static Methods

        /// <summary>
        /// Returns a sorted list of the common built-in local groups. On Windows these groups have a domain name of BUILTIN.
        /// </summary>
        /// <returns>Sorted list of the common built-in local groups.</returns>
        /// <remarks>
        /// Names in this list will not have a "BUILTIN\" prefix.
        /// </remarks>
        public static string[] GetBuiltInLocalGroups()
        {
            if (Common.IsPosixEnvironment)
                return UnixUserInfo.GetBuiltInLocalGroups();

            return WindowsUserInfo.GetBuiltInLocalGroups();
        }

        /// <summary>
        /// Authenticates the specified user credentials.
        /// </summary>
        /// <param name="domain">Domain of user to authenticate.</param>
        /// <param name="userName">Username of user to authenticate.</param>
        /// <param name="password">Password of user to authenticate.</param>
        /// <returns>true if the user credentials are authenticated successfully; otherwise false.</returns>
        /// <example>
        /// This example shows how to validate a user's credentials:
        /// <code>
        /// using System;
        /// using GSF.Identity;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         string domain = "XYZCorp";
        ///         string username = "johndoe";
        ///         string password = "password";
        ///        
        ///         // Authenticate user credentials.
        ///         if ((object)UserInfo.AuthenticateUser(domain, username, password) != null)
        ///             Console.WriteLine("Successfully authenticated user \"{0}\\{1}\".", domain, username);
        ///         else
        ///             Console.WriteLine("Failed to authenticate user \"{0}\\{1}\".", domain, username);
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static IPrincipal AuthenticateUser(string domain, string userName, string password)
        {
            string errorMessage;
            return AuthenticateUser(domain, userName, password, out errorMessage);
        }

        /// <summary>
        /// Authenticates the specified user credentials.
        /// </summary>
        /// <param name="domain">Domain of user to authenticate.</param>
        /// <param name="userName">Username of user to authenticate.</param>
        /// <param name="password">Password of user to authenticate.</param>
        /// <param name="errorMessage">Error message returned, if authentication fails.</param>
        /// <returns>true if the user credentials are authenticated successfully; otherwise false.</returns>
        /// <example>
        /// This example shows how to validate a user's credentials and retrieve an error message if validation fails: 
        /// <code>
        /// using System;
        /// using GSF.Identity;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         string domain = "XYZCorp";
        ///         string username = "johndoe";
        ///         string password = "password";
        ///         string errorMessage;
        ///
        ///         // Authenticate user credentials.
        ///         if ((object)UserInfo.AuthenticateUser(domain, username, password, out errorMessage) != null)
        ///             Console.WriteLine("Successfully authenticated user \"{0}\\{1}\".", domain, username);
        ///         else
        ///             Console.WriteLine("Failed to authenticate user \"{0}\\{1}\" due to exception: {2}", domain, username, errorMessage);
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static IPrincipal AuthenticateUser(string domain, string userName, string password, out string errorMessage)
        {
            if (Common.IsPosixEnvironment)
                return UnixUserInfo.AuthenticateUser(domain, userName, password, out errorMessage);

            return WindowsUserInfo.AuthenticateUser(domain, userName, password, out errorMessage);
        }

        /// <summary>
        /// Impersonates the specified user.
        /// </summary>
        /// <param name="domain">Domain of user to impersonate.</param>
        /// <param name="userName">Username of user to impersonate.</param>
        /// <param name="password">Password of user to impersonate.</param>
        /// <returns>A <see cref="WindowsImpersonationContext"/> object of the impersonated user.</returns>
        /// <remarks>
        /// After impersonating a user the code executes under the impersonated user's identity.
        /// </remarks>
        /// <example>
        /// This example shows how to impersonate a user:
        /// <code>
        /// using System;
        /// using GSF.Identity;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         Console.WriteLine(string.Format("User before impersonation: {0}", UserInfo.CurrentUserID));
        ///         UserInfo.ImpersonateUser("XYZCorp", "johndoe", "password"); // Impersonate user.
        ///         Console.WriteLine(string.Format("User after impersonation: {0}", UserInfo.CurrentUserID));
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static WindowsImpersonationContext ImpersonateUser(string domain, string userName, string password)
        {
            if (Common.IsPosixEnvironment)
                return UnixUserInfo.ImpersonateUser(domain, userName, password);

            return WindowsUserInfo.ImpersonateUser(domain, userName, password);
        }

        /// <summary>
        /// Ends the impersonation of the specified user.
        /// </summary>
        /// <param name="impersonatedUser"><see cref="WindowsImpersonationContext"/> of the impersonated user.</param>
        /// <example>
        /// This example shows how to terminate an active user impersonation:
        /// <code>
        /// using System;
        /// using System.IO;
        /// using System.Security.Principal;
        /// using GSF.Identity;
        ///
        /// class Program
        /// {
        ///     static void Main(string[] args)
        ///     {
        ///         // Impersonate user.
        ///         WindowsImpersonationContext context = UserInfo.ImpersonateUser("XYZCorp", "johndoe", "password");
        ///         // Perform operation requiring elevated privileges.
        ///         Console.WriteLine(File.ReadAllText(@"\\server\share\file.xml"));
        ///         // End the impersonation.
        ///         UserInfo.EndImpersonation(context);
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static void EndImpersonation(WindowsImpersonationContext impersonatedUser)
        {
            if ((object)impersonatedUser != null)
            {
                impersonatedUser.Undo();
                impersonatedUser.Dispose();
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="domain"/> is the local domain (i.e., local machine).
        /// </summary>
        /// <param name="domain">Domain name to check.</param>
        /// <returns><c>true</c>if specified <paramref name="domain"/> is the local domain (i.e., local machine); otherwise, <c>false</c>.</returns>
        public static bool IsLocalDomain(string domain)
        {
            if (Common.IsPosixEnvironment)
                return
                    string.IsNullOrEmpty(domain) ||
                    domain.Equals(".", StringComparison.Ordinal) ||
                    domain.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase);

            // TODO: NT AUTHORITY and such groups can be localized to the OS language, these groups won't be recognized as local domains on non EN-US machines in this code
            return
                string.IsNullOrEmpty(domain) ||
                domain.Equals(".", StringComparison.Ordinal) ||
                domain.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase) ||
                domain.Equals("NT SERVICE", StringComparison.OrdinalIgnoreCase) ||
                domain.Equals("NT AUTHORITY", StringComparison.OrdinalIgnoreCase) ||
                domain.Equals("IIS APPPOOL", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if local user exists.
        /// </summary>
        /// <param name="userName">User name to test for existence.</param>
        /// <returns><c>true</c> if <paramref name="userName"/> exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="userName"/> was <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="userName"/> was specified.</exception>
        public static bool LocalUserExists(string userName)
        {
            if ((object)userName == null)
                throw new ArgumentNullException(nameof(userName));

            // Remove any irrelevant white space
            userName = userName.Trim();

            if (userName.Length == 0)
                throw new ArgumentException("No user name was specified.", nameof(userName));

            if (Common.IsPosixEnvironment)
                return UnixUserInfo.LocalUserExists(userName);

            return WindowsUserInfo.LocalUserExists(userName);
        }

        /// <summary>
        /// Creates a new local user if it does not exist already.
        /// </summary>
        /// <param name="userName">User name to create if it doesn't exist.</param>
        /// <param name="password">Password to user for new user.</param>
        /// <param name="userDescription">Optional user description.</param>
        /// <returns><c>true</c> if user was created; otherwise, <c>false</c> if user already existed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="userName"/> or <paramref name="password"/> was <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="userName"/> was specified.</exception>
        /// <exception cref="InvalidOperationException">Could not create local user.</exception>
        public static bool CreateLocalUser(string userName, string password, string userDescription = null)
        {
            if ((object)userName == null)
                throw new ArgumentNullException(nameof(userName));

            if ((object)password == null)
                throw new ArgumentNullException(nameof(password));

            // Remove any irrelevant white space
            userName = userName.Trim();

            if (userName.Length == 0)
                throw new ArgumentException("Cannot create local user: no user name was specified.", nameof(userName));

            if (Common.IsPosixEnvironment)
                return UnixUserInfo.CreateLocalUser(userName, password, userDescription);

            return WindowsUserInfo.CreateLocalUser(userName, password, userDescription);
        }

        /// <summary>
        /// Sets local user's password.
        /// </summary>
        /// <param name="userName">User name to change password for.</param>
        /// <param name="password">New password fro user.</param>
        /// <exception cref="ArgumentNullException"><paramref name="userName"/> or <paramref name="password"/> was <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="userName"/> was specified or user does not exist.</exception>
        /// <exception cref="InvalidOperationException">Could not set password for local user.</exception>
        public static void SetLocalUserPassword(string userName, string password)
        {
            if ((object)userName == null)
                throw new ArgumentNullException(nameof(userName));

            if ((object)password == null)
                throw new ArgumentNullException(nameof(password));

            // Remove any irrelevant white space
            userName = userName.Trim();

            if (userName.Length == 0)
                throw new ArgumentException("Cannot set password for local user: no user name was specified.", nameof(userName));

            if (Common.IsPosixEnvironment)
                UnixUserInfo.SetLocalUserPassword(userName, password);

            WindowsUserInfo.SetLocalUserPassword(userName, password);
        }

        /// <summary>
        /// Removes local user if it exists.
        /// </summary>
        /// <param name="userName">User name to remove if it exists.</param>
        /// <returns><c>true</c> if user was removed; otherwise, <c>false</c> if user did not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="userName"/> was <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="userName"/> was specified.</exception>
        /// <exception cref="InvalidOperationException">Could not remove local user.</exception>
        public static bool RemoveLocalUser(string userName)
        {
            if ((object)userName == null)
                throw new ArgumentNullException(nameof(userName));

            // Remove any irrelevant white space
            userName = userName.Trim();

            if (userName.Length == 0)
                throw new ArgumentException("Cannot remove local user: no user name was specified.", nameof(userName));

            if (Common.IsPosixEnvironment)
                return UnixUserInfo.RemoveLocalUser(userName);

            return WindowsUserInfo.RemoveLocalUser(userName);
        }

        /// <summary>
        /// Determines if local group exists.
        /// </summary>
        /// <param name="groupName">Group name to test for existence.</param>
        /// <returns><c>true</c> if <paramref name="groupName"/> exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="groupName"/> was <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="groupName"/> was specified.</exception>
        public static bool LocalGroupExists(string groupName)
        {
            if ((object)groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            // Remove any irrelevant white space
            groupName = groupName.Trim();

            if (groupName.Length == 0)
                throw new ArgumentException("No group name was specified.", nameof(groupName));

            groupName = ValidateGroupName(groupName);

            if (Common.IsPosixEnvironment)
                return UnixUserInfo.LocalGroupExists(groupName);

            return WindowsUserInfo.LocalGroupExists(groupName);
        }

        /// <summary>
        /// Creates a new local group if it does not exist already.
        /// </summary>
        /// <param name="groupName">Group name to create if it doesn't exist.</param>
        /// <param name="groupDescription">Optional group description.</param>
        /// <returns><c>true</c> if group was created; otherwise, <c>false</c> if group already existed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="groupName"/> was <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="groupName"/> was specified.</exception>
        /// <exception cref="InvalidOperationException">Could not create local group.</exception>
        public static bool CreateLocalGroup(string groupName, string groupDescription = null)
        {
            if ((object)groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            // Remove any irrelevant white space
            groupName = groupName.Trim();

            if (groupName.Length == 0)
                throw new ArgumentException("Cannot create local group: no group name was specified.", nameof(groupName));

            groupName = ValidateGroupName(groupName);

            if (Common.IsPosixEnvironment)
                return UnixUserInfo.CreateLocalGroup(groupName);

            return WindowsUserInfo.CreateLocalGroup(groupName, groupDescription);
        }

        /// <summary>
        /// Removes local group if it exists.
        /// </summary>
        /// <param name="groupName">Group name to remove if it exists.</param>
        /// <returns><c>true</c> if group was removed; otherwise, <c>false</c> if group did not exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="groupName"/> was <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="groupName"/> was specified.</exception>
        /// <exception cref="InvalidOperationException">Could not remove local group.</exception>
        public static bool RemoveLocalGroup(string groupName)
        {
            if ((object)groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            // Remove any irrelevant white space
            groupName = groupName.Trim();

            if (groupName.Length == 0)
                throw new ArgumentException("Cannot remove local group: no group name was specified.", nameof(groupName));

            groupName = ValidateGroupName(groupName);

            if (Common.IsPosixEnvironment)
                return UnixUserInfo.RemoveLocalGroup(groupName);

            return WindowsUserInfo.RemoveLocalGroup(groupName);
        }

        /// <summary>
        /// Determines if user is in the specified local <paramref name="groupName"/>.
        /// </summary>
        /// <param name="groupName">Group name to test.</param>
        /// <param name="userName">User name to test.</param>
        /// <returns><c>true</c> if user is in group; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="groupName"/> or <paramref name="userName"/> was <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="groupName"/> or <paramref name="userName"/> was specified.</exception>
        /// <exception cref="InvalidOperationException">Could not determine if user was in local group.</exception>
        /// <remarks>
        /// This function will handle Windows service virtual accounts by specifying the complete virtual account name,
        /// such as <c>@"NT SERVICE\MyService"</c>, as the <paramref name="userName"/>. This function can also detect
        /// Active Directory user accounts and groups that may exist in the local group when the <paramref name="userName"/>
        /// is prefixed with a domain name and a backslash "\".
        /// </remarks>
        public static bool UserIsInLocalGroup(string groupName, string userName)
        {
            if ((object)groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            if ((object)userName == null)
                throw new ArgumentNullException(nameof(userName));

            // Remove any irrelevant white space
            groupName = groupName.Trim();
            userName = userName.Trim();

            if (groupName.Length == 0)
                throw new ArgumentException("Cannot determine if user is in local group: no group name was specified.", nameof(groupName));

            if (userName.Length == 0)
                throw new ArgumentException("Cannot determine if user is in local group: no user name was specified.", nameof(userName));

            groupName = ValidateGroupName(groupName);

            if (Common.IsPosixEnvironment)
                return UnixUserInfo.UserIsInLocalGroup(groupName, userName);

            return WindowsUserInfo.UserIsInLocalGroup(groupName, userName);
        }

        /// <summary>
        /// Adds an existing user to the specified local <paramref name="groupName"/>.
        /// </summary>
        /// <param name="groupName">Group name to add local user to.</param>
        /// <param name="userName">Existing local user name.</param>
        /// <returns><c>true</c> if user was added to local group; otherwise, <c>false</c> meaning user was already in group.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="groupName"/> or <paramref name="userName"/> was <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="groupName"/> or <paramref name="userName"/> was specified.</exception>
        /// <exception cref="InvalidOperationException">Could not add user to local group.</exception>
        /// <remarks>
        /// This function will handle Windows service virtual accounts by specifying the complete virtual account name,
        /// such as <c>@"NT SERVICE\MyService"</c>, as the <paramref name="userName"/>. This function can also add
        /// Active Directory user accounts and groups to the local group the when the <paramref name="userName"/> is
        /// prefixed with a domain name and a backslash "\".
        /// </remarks>
        public static bool AddUserToLocalGroup(string groupName, string userName)
        {
            if ((object)groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            if ((object)userName == null)
                throw new ArgumentNullException(nameof(userName));

            // Remove any irrelevant white space
            groupName = groupName.Trim();
            userName = userName.Trim();

            if (groupName.Length == 0)
                throw new ArgumentException("Cannot add user to local group: no group name was specified.", nameof(groupName));

            if (userName.Length == 0)
                throw new ArgumentException("Cannot add user to local group: no user name was specified.", nameof(userName));

            groupName = ValidateGroupName(groupName);

            if (Common.IsPosixEnvironment)
                return UnixUserInfo.AddUserToLocalGroup(groupName, userName);

            return WindowsUserInfo.AddUserToLocalGroup(groupName, userName);
        }

        /// <summary>
        /// Removes an existing user from the specified local <paramref name="groupName"/>.
        /// </summary>
        /// <param name="groupName">Group name to remove local user from.</param>
        /// <param name="userName">Existing local user name.</param>
        /// <returns><c>true</c> if user was removed from local group; otherwise, <c>false</c> meaning user did not exist in group.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="groupName"/> or <paramref name="userName"/> was <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="groupName"/> or <paramref name="userName"/> was specified.</exception>
        /// <exception cref="InvalidOperationException">Could not remove user from local group.</exception>
        /// <remarks>
        /// This function will handle Windows service virtual accounts by specifying the complete virtual account name,
        /// such as <c>@"NT SERVICE\MyService"</c>, as the <paramref name="userName"/>. This function can also remove
        /// Active Directory user accounts and groups from the local group the when the <paramref name="userName"/> is
        /// prefixed with a domain name and a backslash "\".
        /// </remarks>
        public static bool RemoveUserFromLocalGroup(string groupName, string userName)
        {
            if ((object)groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            if ((object)userName == null)
                throw new ArgumentNullException(nameof(userName));

            // Remove any irrelevant white space
            groupName = groupName.Trim();
            userName = userName.Trim();

            if (groupName.Length == 0)
                throw new ArgumentException("Cannot remove user from local group: no group name was specified.", nameof(groupName));

            if (userName.Length == 0)
                throw new ArgumentException("Cannot remove user from local group: no user name was specified.", nameof(userName));

            groupName = ValidateGroupName(groupName);

            if (Common.IsPosixEnvironment)
                return UnixUserInfo.RemoveUserFromLocalGroup(groupName, userName);

            return WindowsUserInfo.RemoveUserFromLocalGroup(groupName, userName);
        }

        /// <summary>
        /// Gets a list of users that exist in the specified local <paramref name="groupName"/>.
        /// </summary>
        /// <param name="groupName">Group name to remove local user from.</param>
        /// <returns>List of users that exist in the in group.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="groupName"/> was <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="groupName"/> was specified.</exception>
        /// <exception cref="InvalidOperationException">Could not get members for local group.</exception>
        public static string[] GetLocalGroupUserList(string groupName)
        {
            if ((object)groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            // Remove any irrelevant white space
            groupName = groupName.Trim();

            if (groupName.Length == 0)
                throw new ArgumentException("Cannot get members for local group: no group name was specified.", nameof(groupName));

            groupName = ValidateGroupName(groupName);

            if (Common.IsPosixEnvironment)
                return UnixUserInfo.GetLocalGroupUserList(groupName);

            return WindowsUserInfo.GetLocalGroupUserList(groupName);
        }

        /// <summary>
        /// Converts the given user name to the SID corresponding to that name.
        /// </summary>
        /// <param name="userName">The user name for which to look up the SID.</param>
        /// <returns>The SID for the given user name, or the user name if no SID can be found.</returns>
        /// <remarks>
        /// If the <paramref name="userName"/> cannot be converted to a SID, <paramref name="userName"/>
        /// will be the return value.
        /// </remarks>
        public static string UserNameToSID(string userName)
        {
            if (Common.IsPosixEnvironment)
                return UnixUserInfo.UserNameToSID(userName);

            return WindowsUserInfo.AccountNameToSID(userName);
        }

        /// <summary>
        /// Converts the given group name to the SID corresponding to that name.
        /// </summary>
        /// <param name="groupName">The group name for which to look up the SID.</param>
        /// <returns>The SID for the given group name, or the group name if no SID can be found.</returns>
        /// <remarks>
        /// If the <paramref name="groupName"/> cannot be converted to a SID, <paramref name="groupName"/>
        /// will be the return value.
        /// </remarks>
        public static string GroupNameToSID(string groupName)
        {
            if (Common.IsPosixEnvironment)
                return UnixUserInfo.GroupNameToSID(groupName);

            return WindowsUserInfo.AccountNameToSID(groupName);
        }

        /// <summary>
        /// Converts the given SID to the corresponding account name.
        /// </summary>
        /// <param name="sid">The SID for which to look up the account name.</param>
        /// <returns>The account name for the given SID, or the SID if no account name can be found.</returns>
        /// <remarks>
        /// If the <paramref name="sid"/> cannot be converted to an account name, <paramref name="sid"/>
        /// will be the return value.
        /// </remarks>
        public static string SIDToAccountName(string sid)
        {
            if (Common.IsPosixEnvironment)
                return UnixUserInfo.SIDToAccountName(sid);

            return WindowsUserInfo.SIDToAccountName(sid);
        }

        /// <summary>
        /// Determines whether the given security identifier identifies a user account.
        /// </summary>
        /// <param name="sid">The security identifier.</param>
        /// <returns>True if the security identifier identifies a user account; false otherwise.</returns>
        public static bool IsUserSID(string sid)
        {
            if (Common.IsPosixEnvironment)
                return UnixUserInfo.IsUserSID(sid);

            return WindowsUserInfo.IsUserSID(sid);
        }

        /// <summary>
        /// Determines whether the given security identifier identifies a group.
        /// </summary>
        /// <param name="sid">The security identifier.</param>
        /// <returns>True if the security identifier identifies a group; false otherwise.</returns>
        public static bool IsGroupSID(string sid)
        {
            if (Common.IsPosixEnvironment)
                return UnixUserInfo.IsGroupSID(sid);

            return WindowsUserInfo.IsGroupSID(sid);
        }

        // DirectoryEntry will only resolve "BUILTIN\" groups with a dot ".\"
        internal static string ValidateGroupName(string groupName)
        {
            if (!Common.IsPosixEnvironment && groupName.StartsWith(@$"{WindowsUserInfo.BuiltInGroupName}\", StringComparison.OrdinalIgnoreCase))
                return Regex.Replace(groupName, @$"^{WindowsUserInfo.BuiltInGroupName}\\", @".\", RegexOptions.IgnoreCase);

            return groupName;
        }

        #endregion
    }
}