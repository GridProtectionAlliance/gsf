//******************************************************************************************************
//  UserInfo.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//       Fixed bug introduced by marking the type as enabled upon initialization (Initialize()).
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
//       perfomed in the domain to which the host machine (where this code is executing) is joined.
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
//       Fixed a bug in the Active Directory lookup of MaximumPasswordAge property.
//  04/04/2011 - J. Ritchie Carroll
//       Added Exists and DomainAvailable properties to accomodate testing when domain server is
//       not available (such as when laptop is used when not connected to the domain).
//  04/05/2011 - J. Ritchie Carroll
//       Updated class to attempt reintialization after system resume in case user no longer has
//       access to domain.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  11/23/2011 - J. Ritchie Carroll
//       Modified to support new life cycle interface requirements (i.e., Diposed event).
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  03/05/2013 - Joe France
//      Prefer the lastlogontimestamp property over lastLogon property.
//  03/06/2013 - Pinal C. Patel
//       Added new GetUserPropertyValue() and GetUserPropertyValueAsString() methods to replace old
//       GetUserProperty() method and marked GetUserPropertyValue() as obsolete.
//       Updated LastLogon to revert using lastLogon property instead of lastLogonTimestamp since it is
//       updated less frequestly and fixed the parsing logic for it since the return value is a large
//       integer.
//
//******************************************************************************************************

using GSF.Configuration;
using GSF.Interop;
using GSF.IO;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.DirectoryServices;
using System.IO;
using System.Management;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace GSF.Identity
{
    /// <summary>
    /// Represents information about a domain user retrieved from Active Directory.
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
    /// <para>
    /// Some methods in this class may not behave as expected when running on under Mono deployments.
    /// </para>
    /// </example>
    public class UserInfo : ISupportLifecycle, IPersistSettings
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

        // Events

        /// <summary>
        /// Occurs when the class has been disposed.
        /// </summary>
        public event EventHandler Disposed;

        private const int ACCOUNTDISABLED = 2;
        private const int LOCKED = 16;
        private const int PASSWD_CANT_CHANGE = 64;
        private const int DONT_EXPIRE_PASSWORD = 65536;
        private const string LogonDomainRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
        private const string LogonDomainRegistryValue = "DefaultDomainName";

        // Fields
        private string m_domain;
        private string m_username;
        private string m_ldapPath;
        private DirectoryEntry m_userEntry;
        private bool m_domainAvailable;
        private int m_userAccountControl;
        private bool m_isWinNT;
        private string m_privilegedDomain;
        private string m_privilegedUserName;
        private string m_privilegedPassword;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;

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
            : this()
        {
            if (string.IsNullOrEmpty(loginID))
                throw new ArgumentNullException("loginID");

            string[] parts = loginID.Split('\\');
            if (parts.Length != 2)
            {
                // Login ID is specified in 'username' format.
                m_username = loginID;
            }
            else
            {
                // Login ID is specified in 'domain\username' format.
                m_domain = parts[0];
                m_username = parts[1];
            }

            m_ldapPath = ldapPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfo"/> class.
        /// </summary>
        private UserInfo()
        {
            // Initialize default settings
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_userAccountControl = -1;

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

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="UserInfo"/> object is currently enabled.
        /// </summary>
        /// <remarks>
        /// <see cref="Enabled"/> property is not to be set by user-code directly.
        /// </remarks>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
                    throw new ArgumentNullException("value");

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets the Login ID of the user.
        /// </summary>
        /// <remarks>Returns the value provided in the <see cref="UserInfo(string)"/> constructor.</remarks>
        public string LoginID
        {
            get
            {
                return m_domain + "\\" + m_username;
            }
        }

        /// <summary>
        /// Gets flag that determines if user domain is available.
        /// </summary>
        /// <returns><c>true</c> if domain responds to <see cref="Initialize()"/>; otherwise <c>false</c>.</returns>
        public bool DomainAvailable
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

                return m_domainAvailable;
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
        /// connectivity to the domain, the user can still login to the latop using cached credentials therefore they are
        /// still considered to exist. Without access to the domain, no user information will be available (such as the
        /// <see cref="Groups"/> listing) - if this data is needed offline it will need to be cached by the application in
        /// anticipation of offline access.
        /// </para>
        /// </remarks>
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
                        exists = (object)windowsPrincipal != null && !string.IsNullOrEmpty(LoginID) && string.Compare(windowsPrincipal.Identity.Name, LoginID, true) == 0 && windowsPrincipal.Identity.IsAuthenticated;
                    }
                }

                if (!exists)
                {
                    if (m_isWinNT)
                    {
                        try
                        {
                            exists = DirectoryEntry.Exists("WinNT://" + m_domain + "/" + m_username);
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

        /// <summary>
        /// Gets the last login time of the user.
        /// </summary>
        public DateTime LastLogon
        {
            get
            {
                if (m_enabled)
                {
                    try
                    {
                        if (m_isWinNT)
                            return DateTime.Parse(GetUserPropertyValueAsString("lastLogin"));

                        return DateTime.FromFileTime(ConvertToLong(GetUserPropertyValue("lastLogon").Value));
                    }
                    catch
                    {
                        return DateTime.MinValue;
                    }
                }
                else
                    return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> when the account was created.
        /// </summary>
        public DateTime AccountCreationDate
        {
            get
            {
                if (m_enabled)
                {
                    try
                    {
                        if (m_isWinNT)
                        {
                            string profilePath = GetUserPropertyValueAsString("profile");

                            if (string.IsNullOrEmpty(profilePath) || !Directory.Exists(profilePath))
                            {
                                // Remove any trailing directory seperator character from the file path.
                                string rootFolder = FilePath.AddPathSuffix(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                                string userFolder = FilePath.GetLastDirectoryName(rootFolder);
                                int folderLocation = rootFolder.LastIndexOf(userFolder);

                                // Remove user profile name for current user (this class may be for user other than ower of current thread)                            
                                rootFolder = FilePath.AddPathSuffix(rootFolder.Substring(0, folderLocation));

                                // Create profile path for user referenced in this UserInfo class
                                profilePath = FilePath.AddPathSuffix(rootFolder + m_username);
                            }

                            return Directory.GetCreationTime(profilePath);
                        }
                        else
                            return Convert.ToDateTime(GetUserPropertyValueAsString("whenCreated"));
                    }
                    catch
                    {
                        return DateTime.MinValue;
                    }
                }
                else
                    return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/>, in UTC, of next password change for the user.
        /// </summary>
        public DateTime NextPasswordChangeDate
        {
            get
            {
                DateTime passwordChangeDate = DateTime.MaxValue;
                int userAccountControl = UserAccountControl;

                if (m_enabled && !PasswordCannotChange && !PasswordDoesNotExpire)
                {
                    try
                    {
                        if (m_isWinNT)
                        {
                            Ticks maxPasswordTicksAge = MaximumPasswordAge;

                            if (maxPasswordTicksAge >= 0)
                            {
                                // WinNT properties are in seconds, not ticks
                                long passwordAge = long.Parse(GetUserPropertyValueAsString("passwordAge"));
                                long maxPasswordAge = (long)maxPasswordTicksAge.ToSeconds();

                                if (passwordAge > maxPasswordAge || GetUserPropertyValueAsString("passwordExpired").ParseBoolean())
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
                            long passwordSetOn = ConvertToLong(GetUserPropertyValue("pwdLastSet").Value);

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
                    }
                    catch
                    {
                        return DateTime.MaxValue;
                    }
                }

                return passwordChangeDate;
            }
        }

        /// <summary>
        /// Gets the account control information of the user.
        /// </summary>
        public int UserAccountControl
        {
            get
            {
                if (m_enabled && m_userAccountControl == -1)
                {
                    if (m_isWinNT)
                        m_userAccountControl = int.Parse(GetUserPropertyValueAsString("userFlags"));
                    else
                        m_userAccountControl = int.Parse(GetUserPropertyValueAsString("userAccountControl"));
                }

                return m_userAccountControl;
            }
        }

        /// <summary>
        /// Gets flag that determines if account is locked-out for this user.
        /// </summary>
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
        public bool PasswordDoesNotExpire
        {
            get
            {
                return Convert.ToBoolean(UserAccountControl & DONT_EXPIRE_PASSWORD);
            }
        }

        /// <summary>
        /// Gets ths maximum password age for the user.
        /// </summary>
        public Ticks MaximumPasswordAge
        {
            get
            {
                if (m_enabled)
                {
                    string maxAgePropertyValue = string.Empty;

                    if (m_isWinNT)
                    {
                        maxAgePropertyValue = GetUserPropertyValueAsString("maxPasswordAge");
                    }
                    else
                    {
                        WindowsImpersonationContext currentContext = null;
                        try
                        {
                            currentContext = ImpersonatePrivilegedAccount();
                            using (DirectorySearcher searcher = CreateDirectorySearcher())
                            {
                                SearchResult searchResult = searcher.FindOne();
                                if ((object)searchResult != null && searchResult.Properties.Contains("maxPwdAge"))
                                    maxAgePropertyValue = searchResult.Properties["maxPwdAge"][0].ToString();
                            }
                        }
                        finally
                        {
                            EndImpersonation(currentContext);
                        }
                    }

                    if (string.IsNullOrEmpty(maxAgePropertyValue))
                        return -1;

                    long maxPasswordAge = long.Parse(maxAgePropertyValue);

                    if (m_isWinNT)
                        return Ticks.FromSeconds(maxPasswordAge);

                    return maxPasswordAge;
                }

                return -1;
            }
        }

        /// <summary>
        /// Gets the groups asscociated with the user.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Groups names are prefixed with their associated domain or computer name.
        /// </para>
        /// <para>
        /// This method always returns an empty string array (i.e., a string array with no elements) under Mono deployments.
        /// </para>
        /// </remarks>
        public string[] Groups
        {
            get
            {
                List<string> groups = new List<string>();

#if !MONO
                if (m_enabled)
                {
                    if (m_isWinNT)
                    {
                        // Get local groups
                        object localGroups = m_userEntry.Invoke("Groups");

                        foreach (object localGroup in (IEnumerable)localGroups)
                        {
                            using (DirectoryEntry groupEntry = new DirectoryEntry(localGroup))
                            {
                                groups.Add(m_domain + "\\" + groupEntry.Name);
                            }
                        }
                    }
                    else
                    {
                        // Get active directory groups
                        m_userEntry.RefreshCache(new string[] { "TokenGroups" });

                        foreach (byte[] sid in m_userEntry.Properties["TokenGroups"])
                        {
                            try
                            {
                                groups.Add(new SecurityIdentifier(sid, 0).Translate(typeof(NTAccount)).ToString());
                            }
                            catch
                            {
                                // Ignoring group SID's that fail to translate to an active AD group, for whatever reason
                            }
                        }
                    }
                }
#endif

                return groups.ToArray();
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
                if (m_isWinNT)
                    return GetNameElements(DisplayName)[0];

                return GetUserPropertyValueAsString("givenName");
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
                if (m_isWinNT)
                    return GetNameElements(DisplayName)[1];

                return GetUserPropertyValueAsString("sn");
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
                if (m_isWinNT)
                {
                    string name = GetUserPropertyValueAsString("fullName");

                    if (string.IsNullOrEmpty(name))
                        name = GetUserPropertyValueAsString("Name");

                    return name;
                }

                return GetUserPropertyValueAsString("displayName");
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
                return GetUserPropertyValueAsString("initials");
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
                if (m_isWinNT)
                    return DisplayName;

                string fName = FirstName;
                string lName = LastName;
                string mInitial = MiddleInitial;
                if (!string.IsNullOrEmpty(fName) && !string.IsNullOrEmpty(lName))
                {
                    if (string.IsNullOrEmpty(mInitial))
                        return fName + " " + lName;

                    else
                        return fName + " " + mInitial + ". " + lName;
                }
                else
                {
                    return LoginID;
                }
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
                return GetUserPropertyValueAsString("mail");
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
                return GetUserPropertyValueAsString("wWWHomePage");
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
                return GetUserPropertyValueAsString("description");
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
                return GetUserPropertyValueAsString("telephoneNumber");
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
                return GetUserPropertyValueAsString("title");
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
                return GetUserPropertyValueAsString("company");
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
                return GetUserPropertyValueAsString("physicalDeliveryOfficeName");
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
                return GetUserPropertyValueAsString("department");
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
                return GetUserPropertyValueAsString("l");
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
                return GetUserPropertyValueAsString("streetAddress");
            }
        }

        /// <summary>
        /// Gets the <see cref="DirectoryEntry"/> object used for retrieving user information.
        /// </summary>
        public DirectoryEntry UserEntry
        {
            get
            {
                return m_userEntry;
            }
        }

        /// <summary>
        /// Gets flag that determines if this <see cref="UserInfo"/> instance is based on a local WinNT account instead of found through LDAP.
        /// </summary>
        public bool IsWinNTEntry
        {
            get
            {
                return m_isWinNT;
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
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        SaveSettings();

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
            if (!m_initialized)
            {
                // Load settings from config file.
                LoadSettings();

                // Handle initialization
                Initialize(null);

                // Initialize user information only once
                m_initialized = true;
            }
        }

        private void Initialize(object state)
        {
            m_enabled = false;

            // Attempt do derive the domain if one is not specified
            if (string.IsNullOrEmpty(m_domain))
            {
                if (!string.IsNullOrEmpty(m_privilegedDomain))
                {
                    // Use domain specified for privileged account
                    m_domain = m_privilegedDomain;
                }
                else
                {
                    // Attempt to use the default logon domain of the host machine. Note that this key will not exist on machines
                    // that do not connect to a domain and the Environment.UserDomainName property will return the machine name.
                    m_domain = Registry.GetValue(LogonDomainRegistryKey, LogonDomainRegistryValue, Environment.UserDomainName).ToString();
                }
            }

            // Set the domain as the local machine if one is not specified
            if (string.IsNullOrEmpty(m_domain))
                m_domain = Environment.MachineName;

            // Use active directory domain account for user information lookup as long as domain is not the current machine
            if (string.Compare(Environment.MachineName, m_domain, true) != 0)
            {
                // Initialize the directory entry object used to retrieve active directory information
                WindowsImpersonationContext currentContext = null;

                try
                {
                    // Impersonate to the privileged account if specified
                    currentContext = ImpersonatePrivilegedAccount();

                    // Initialize the Active Directory searcher object
                    using (DirectorySearcher searcher = CreateDirectorySearcher())
                    {
                        searcher.Filter = "(SAMAccountName=" + m_username + ")";
                        SearchResult result = searcher.FindOne();
                        if ((object)result != null)
                            m_userEntry = result.GetDirectoryEntry();
                    }

                    m_isWinNT = false;
                    m_userAccountControl = -1;
                    m_enabled = true;
                    m_domainAvailable = true;
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
                    m_domainAvailable = false;

                    throw new InitializationException(string.Format("Failed to initialize directory entry for domain user '{0}'", LoginID), ex);
                }
                finally
                {
                    // Undo impersonation if it was performed
                    EndImpersonation(currentContext);
                }
            }
            else
            {
                try
                {
                    // Initialize the directory entry object used to retrieve local account information
                    m_userEntry = new DirectoryEntry("WinNT://" + m_domain + "/" + m_username);
                    m_isWinNT = true;
                    m_userAccountControl = -1;
                    m_enabled = true;
                    m_domainAvailable = DirectoryEntry.Exists("WinNT://" + m_domain + "/" + m_username);
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
                    m_domainAvailable = false;

                    throw new InitializationException(string.Format("Failed to initialize directory entry for user '{0}'", LoginID), ex);
                }
            }
        }

        // If the system is waking back up from a sleep state, this class should reinitialize in case
        // user is no longer connected to a domain
        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                ThreadPool.QueueUserWorkItem(Initialize);
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
                throw new ArgumentNullException("domain");

            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException("username");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            // Set the credentials for privileged domain user account.
            m_privilegedDomain = domain;
            m_privilegedUserName = username;
            m_privilegedPassword = password;
        }

        /// <summary>
        /// Impersonates the defined privileged domain account.
        /// </summary>
        /// <returns>An <see cref="WindowsImpersonationContext"/> if privileged domain account has been defined, otherwise null.</returns>
        /// <remarks>
        /// This method always returns <c>null</c> under Mono deployments.
        /// </remarks>
        public WindowsImpersonationContext ImpersonatePrivilegedAccount()
        {
#if !MONO
            if (!string.IsNullOrEmpty(m_privilegedDomain) &&
                !string.IsNullOrEmpty(m_privilegedUserName) &&
                !string.IsNullOrEmpty(m_privilegedPassword))
            {
                // Privileged domain account is specified
                return UserInfo.ImpersonateUser(m_privilegedDomain, m_privilegedUserName, m_privilegedPassword);
            }
#endif

            // Privileged domain account is not specified
            return null;
        }

        /// <summary>
        /// Returns the value for specified active directory property.
        /// </summary>
        /// <param name="propertyName">Name of the active directory property whose value is to be retrieved.</param>
        /// <returns><see cref="PropertyValueCollection"/> for the specified active directory property.</returns>
        public PropertyValueCollection GetUserPropertyValue(string propertyName)
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
                    currentContext = ImpersonatePrivilegedAccount();

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
                EndImpersonation(currentContext);
            }
        }

        /// <summary>
        /// Returns the value for specified active directory property.
        /// </summary>
        /// <param name="propertyName">Name of the active directory property whose value is to be retrieved.</param>
        /// <returns><see cref="String"/> value for the specified active directory property.</returns>
        public string GetUserPropertyValueAsString(string propertyName)
        {
            PropertyValueCollection value = GetUserPropertyValue(propertyName);
            if (value != null && value.Count > 0)
                return value[0].ToString().Replace("  ", " ").Trim();
            else
                return string.Empty;
        }

        private DirectorySearcher CreateDirectorySearcher()
        {
            DirectorySearcher searcher;
            if (string.IsNullOrEmpty(m_ldapPath))
                searcher = new DirectorySearcher();
            else
                searcher = new DirectorySearcher(new DirectoryEntry(m_ldapPath));
            return searcher;
        }

        private long ConvertToLong(object largeInteger)
        {
            Type type = largeInteger.GetType();

            int highPart = (int)type.InvokeMember("HighPart", BindingFlags.GetProperty, null, largeInteger, null);
            int lowPart = (int)type.InvokeMember("LowPart", BindingFlags.GetProperty, null, largeInteger, null);

            return (long)highPart << 32 | (uint)lowPart;
        }

        // Split a string into first name and last name
        private string[] GetNameElements(string displayName)
        {
            displayName = displayName.Trim();

            if (!string.IsNullOrEmpty(displayName))
            {
                int lastSplit = displayName.LastIndexOf(' ');

                if (lastSplit >= 0)
                    return new string[] { displayName.Substring(0, lastSplit), displayName.Substring(lastSplit + 1) };
                else
                    return new string[] { displayName, "" };
            }
            else
                return new string[] { "", "" };
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static UserInfo s_currentUserInfo;

        // Static Properties

        /// <summary>
        /// Gets the <see cref="LoginID"/> of the current user.
        /// </summary>
        /// <remarks>
        /// The <see cref="LoginID"/> returned is that of the user account under which the code is executing.
        /// </remarks>
        public static string CurrentUserID
        {
            get
            {
                return WindowsIdentity.GetCurrent().Name;
            }
        }

        /// <summary>
        /// Gets the <see cref="UserInfo"/> object for the <see cref="CurrentUserID"/>.
        /// </summary>
        public static UserInfo CurrentUserInfo
        {
            get
            {
                if ((object)s_currentUserInfo == null)
                    s_currentUserInfo = new UserInfo(CurrentUserID);

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
                else
                    return new UserInfo(RemoteUserID);
            }
        }

        // Static Methods

        /// <summary>
        /// Authenticates the specified user credentials.
        /// </summary>
        /// <param name="domain">Domain of user to authenticate.</param>
        /// <param name="username">Username of user to authenticate.</param>
        /// <param name="password">Password of user to authenticate.</param>
        /// <returns>true if the user credentials are authenticated successfully; otherwise false.</returns>
        /// <remarks>
        /// This method always returns <c>null</c> under Mono deployments.
        /// </remarks>
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
        public static IPrincipal AuthenticateUser(string domain, string username, string password)
        {
            string errorMessage;
            return AuthenticateUser(domain, username, password, out errorMessage);
        }

        /// <summary>
        /// Authenticates the specified user credentials.
        /// </summary>
        /// <param name="domain">Domain of user to authenticate.</param>
        /// <param name="username">Username of user to authenticate.</param>
        /// <param name="password">Password of user to authenticate.</param>
        /// <param name="errorMessage">Error message returned, if authentication fails.</param>
        /// <returns>true if the user credentials are authenticated successfully; otherwise false.</returns>
        /// <remarks>
        /// This method always returns <c>null</c> under Mono deployments.
        /// </remarks>
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
        public static IPrincipal AuthenticateUser(string domain, string username, string password, out string errorMessage)
        {
            errorMessage = null;
#if MONO
            return null;
#else
            IntPtr tokenHandle = IntPtr.Zero;

            try
            {
                // Call Win32 LogonUser method.
                if (WindowsApi.LogonUser(username, domain, password, WindowsApi.LOGON32_LOGON_NETWORK, WindowsApi.LOGON32_PROVIDER_DEFAULT, out tokenHandle))
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
#endif

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

        /// <summary>
        /// Impersonates the specified user.
        /// </summary>
        /// <param name="domain">Domain of user to impersonate.</param>
        /// <param name="username">Username of user to impersonate.</param>
        /// <param name="password">Password of user to impersonate.</param>
        /// <returns>A <see cref="WindowsImpersonationContext"/> object of the impersonated user.</returns>
        /// <remarks>
        /// <para>
        /// After impersonating a user the code executes under the impersonated user's identity.
        /// </para>
        /// <para>
        /// This method always returns <c>null</c> under Mono deployments.
        /// </para>
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
        public static WindowsImpersonationContext ImpersonateUser(string domain, string username, string password)
        {
            WindowsImpersonationContext impersonatedUser = null;
#if !MONO
            IntPtr userTokenHandle = IntPtr.Zero;
            IntPtr duplicateTokenHandle = IntPtr.Zero;

            try
            {
                // Calls LogonUser to obtain a handle to an access token.
                if (!WindowsApi.LogonUser(username, domain, password, WindowsApi.LOGON32_LOGON_INTERACTIVE, WindowsApi.LOGON32_PROVIDER_DEFAULT, out userTokenHandle))
                    throw new InvalidOperationException(string.Format("Failed to impersonate user \"{0}\\{1}\": {2}", domain, username, WindowsApi.GetLastErrorMessage()));

                if (!WindowsApi.DuplicateToken(userTokenHandle, WindowsApi.SECURITY_IMPERSONATION, ref duplicateTokenHandle))
                    throw new InvalidOperationException(string.Format("Failed to impersonate user \"{0}\\{1}\" - exception thrown while trying to duplicate token: {2}", domain, username, WindowsApi.GetLastErrorMessage()));

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

#endif
            return impersonatedUser;
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

            impersonatedUser = null;
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the current machine is joined to a domain.
        /// </summary>
        /// <remarks>
        /// This method always returns <c>false</c> under Mono deployments.
        /// </remarks>
        public static bool MachineIsJoinedToDomain
        {
            get
            {
#if MONO
                return false;
#else
                using (ManagementObject wmi = new ManagementObject(string.Format("Win32_ComputerSystem.Name='{0}'", Environment.MachineName)))
                {
                    wmi.Get();
                    return (bool)wmi["PartOfDomain"];
                }
#endif
            }
        }

        #endregion

        #region [ Obsolete ]

        /// <summary>
        /// Returns the value for specified active directory property.
        /// </summary>
        /// <param name="propertyName">Name of the active directory property whose value is to be retrieved.</param>
        /// <returns><see cref="String"/> value for the specified active directory property.</returns>
        [Obsolete("GetUserPropertyValueAsString replaces GetUserProperty", true)]
        public string GetUserProperty(string propertyName)
        {
            return GetUserPropertyValueAsString(propertyName);
        }

        #endregion
    }
}