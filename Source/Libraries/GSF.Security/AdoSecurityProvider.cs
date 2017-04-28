//******************************************************************************************************
//  AdoSecurityProvider.cs - Gbtc
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
//  11/24/2010 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  07/18/2011 - Stephen C. Wills
//       Modified a SELECT statement in RefreshData to explicitly alias columns to
//       fix an error when using SQLite databases.
//  09/01/2011 - Stephen C. Wills
//       Modified references to views in the database whose names have changed.
//  09/28/2011 - Stephen C. Wills
//       Modified database queries to work with Oracle database.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  01/16/2014 - J. Ritchie Carroll
//       Added security context caching so security system can load without database access
//       using last cached security state.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.Collections;
using GSF.Configuration;
using GSF.Data;
using GSF.Data.Model;
using GSF.Identity;
using GSF.Security.Model;

namespace GSF.Security
{
    /// <summary>
    /// Represents an <see cref="ISecurityProvider"/> that uses ADO.NET data source (SQL Server, MySQL, Oracle, etc.) for its
    /// backend data store and authenticates internal users against Active Directory and external users against the database.
    /// </summary>
    /// <example>
    /// Required config file entries (automatically added):
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="GSF.Configuration.CategorizedSettingsSection, GSF.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <securityProvider>
    ///       <add name="ProviderType" value="GSF.Security.AdoSecurityProvider, GSF.Security" description="The type to be used for enforcing security."
    ///         encrypted="false" />
    ///       <add name="UserCacheTimeout" value="5" description="Defines the timeout, in whole minutes, for a user's provider cache. Any value less than 1 will cause cache reset every minute."
    ///         encrypted="false" />
    ///       <add name="ConnectionString" value="Eval(systemSettings.ConnectionString)" description="Configuration database connection string"
    ///         encrypted="false"/>
    ///       <add name="DataProviderString" value="Eval(systemSettings.DataProviderString)" description="Configuration database ADO.NET data provider assembly type creation string"
    ///         encrypted="false"/>    
    ///       <add name="LdapPath" value="" description="Specifies the LDAP path used to initialize the security provider."
    ///         encrypted="false" />
    ///       <add name="ApplicationName" value="SEC_APP" description="Name of the application being secured."
    ///         encrypted="false" />
    ///       <add name="IncludedResources" value="*=*" description="Semicolon delimited list of resources to be secured along with role names."
    ///         encrypted="false" />
    ///       <add name="ExcludedResources" value="" description="Semicolon delimited list of resources to be excluded from being secured."
    ///         encrypted="false" />    
    ///       <add name="NotificationSmtpServer" value="localhost" description="SMTP server to be used for sending out email notification messages."
    ///         encrypted="false" />
    ///       <add name="NotificationSenderEmail" value="sender@company.com" description="Email address of the sender of email notification messages." 
    ///         encrypted="false" />
    ///       <add name="CacheRetryDelayInterval" value="200" description="Wait interval, in milliseconds, before retrying load of user data cache."
    ///         encrypted="false"/>
    ///       <add name="CacheMaximumRetryAttempts" value="10" description="Maximum retry attempts allowed for loading user data cache."
    ///         encrypted="false"/>
    ///       <add name="EnableOfflineCaching" value="True" description="True to enable caching of user information for authentication in offline state, otherwise False."
    ///         encrypted="false"/>
    ///       <add name="PasswordRequirementsRegex" value="^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).*$" description="Regular expression used to validate new passwords for database users."
    ///         encrypted="false" />
    ///       <add name="PasswordRequirementsError" value="Invalid Password: Password must be at least 8 characters; must contain at least 1 number, 1 upper case letter, and 1 lower case letter" description="Error message to be displayed when new database user password fails regular expression test."
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
    /// <remarks>
    /// Minimum expected table schema for ADO Security Provider:
    /// <code>
    /// <![CDATA[
    /// CREATE TABLE UserAccount
    /// (
    ///     ID UNIQUEINDENTIFIER NOT NULL DEFAULT NEWID(),
    ///     Name VARCHAR(200) NOT NULL,
    ///     Password VARCHAR(200) DEFAULT NULL,
    ///     FirstName VARCHAR(200) DEFAULT NULL,
    ///     LastName VARCHAR(200) DEFAULT NULL,
    ///     Phone VARCHAR(200) DEFAULT NULL,
    ///     Email VARCHAR(200) DEFAULT NULL,
    ///     LockedOut TINYINT NOT NULL DEFAULT 0,
    ///     UseADAuthentication TINYINT NOT NULL DEFAULT 1,
    ///     ChangePasswordOn DATETIME DEFAULT NULL,
    ///     CONSTRAINT PK_UserAccount PRIMARY KEY (ID ASC),
    ///     CONSTRAINT IX_UserAccount UNIQUE KEY (Name)
    /// );
    /// 
    /// CREATE TABLE SecurityGroup
    /// (
    ///     ID UNIQUEINDENTIFIER NOT NULL DEFAULT NEWID(),
    ///     Name VARCHAR(200) NOT NULL,
    ///     CONSTRAINT PK_SecurityGroup PRIMARY KEY (ID ASC),
    ///     CONSTRAINT IX_SecurityGroup UNIQUE KEY (Name)
    /// );
    /// 
    /// CREATE TABLE SecurityGroupUserAccount
    /// (
    ///     SecurityGroupID UNIQUEINDENTIFIER NOT NULL,
    ///     UserAccountID UNIQUEINDENTIFIER NOT NULL
    /// );
    /// 
    /// CREATE TABLE ApplicationRole
    /// (
    ///     ID UNIQUEINDENTIFIER NOT NULL DEFAULT NEWID(),
    ///     Name VARCHAR(200) NOT NULL,
    ///     NodeID UNIQUEINDENTIFIER NOT NULL,
    ///     CONSTRAINT PK_ApplicationRole PRIMARY KEY (ID ASC),
    ///     CONSTRAINT IX_ApplicationRole UNIQUE KEY (NodeID, Name)
    /// );
    /// 
    /// CREATE TABLE ApplicationRoleUserAccount
    /// (
    ///     ApplicationRoleID UNIQUEINDENTIFIER NOT NULL,
    ///     UserAccountID UNIQUEINDENTIFIER NOT NULL  
    /// );
    /// 
    /// CREATE TABLE ApplicationRoleSecurityGroup
    /// (
    ///     ApplicationRoleID UNIQUEINDENTIFIER NOT NULL,
    ///     SecurityGroupID UNIQUEINDENTIFIER NOT NULL  
    /// );
    /// ]]>
    /// </code>
    /// </remarks>
    public class AdoSecurityProvider : LdapSecurityProvider
    {
        #region [ Members ]

        // Constants
        private const string UserAccountTable = "UserAccount";                                      // Table name for user accounts
        private const string SecurityGroupTable = "SecurityGroup";                                  // Table name for security groups
        private const string SecurityGroupUserAccountTable = "SecurityGroupUserAccount";            // Table name for security group user accounts
        private const string ApplicationRoleTable = "ApplicationRole";                              // Table name for application roles
        private const string ApplicationRoleUserAccountTable = "ApplicationRoleUserAccount";        // Table name for application role assignments for user accounts
        private const string ApplicationRoleSecurityGroupTable = "ApplicationRoleSecurityGroup";    // Table name for application role assignments for security groups

        /// <summary>
        /// Default regular expression used to validate new database user passwords.
        /// </summary>
        public const string DefaultPasswordRequirementsRegex = "^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$";

        /// <summary>
        /// Default error message displayed when databases users fail regular expression test.
        /// </summary>
        public const string DefaultPasswordRequirementsError = "Invalid Password: Password must be at least 8 characters; must contain at least 1 number, 1 upper case letter, and 1 lower case letter";

        /// <summary>
        /// Defines the provider ID for the <see cref="AdoSecurityProvider"/>.
        /// </summary>
        public new const int ProviderID = 1;

        private Exception m_lastException;
        private bool m_successfulPassThroughAuthentication;
        private string m_passwordRequirementsRegex;
        private string m_passwordRequirementsError;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoSecurityProvider"/> class.
        /// </summary>
        /// <param name="username">Name that uniquely identifies the user.</param>
        public AdoSecurityProvider(string username)
            : this(username, true, false, false, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoSecurityProvider"/> class.
        /// </summary>
        /// <param name="username">Name that uniquely identifies the user.</param>
        /// <param name="canRefreshData">true if the security provider can refresh <see cref="UserData"/> from the backend data store, otherwise false.</param>
        /// <param name="canUpdateData">true if the security provider can update <see cref="UserData"/> in the backend data store, otherwise false.</param>
        /// <param name="canResetPassword">true if the security provider can reset user password, otherwise false.</param>
        /// <param name="canChangePassword">true if the security provider can change user password, otherwise false.</param>
        protected AdoSecurityProvider(string username, bool canRefreshData, bool canUpdateData, bool canResetPassword, bool canChangePassword)
            : base(username, canRefreshData, canUpdateData, canResetPassword, canChangePassword)
        {
            base.ConnectionString = "Eval(systemSettings.ConnectionString)";

            m_passwordRequirementsRegex = DefaultPasswordRequirementsRegex;
            m_passwordRequirementsError = DefaultPasswordRequirementsError;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a boolean value that indicates whether <see cref="SecurityProviderBase.UpdateData"/> operation is supported.
        /// </summary>
        public override bool CanUpdateData
        {
            get
            {
                // Data update supported on external user accounts.
                if (UserData.IsDefined && UserData.IsExternal)
                    return true;

                // Data update not supported on internal user accounts.
                return false;
            }
        }

        /// <summary>
        /// Gets last exception reported by the <see cref="AdoSecurityProvider"/>.
        /// </summary>
        public Exception LastException
        {
            get
            {
                return m_lastException;
            }
            set
            {
                m_lastException = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Loads saved security provider settings from the config file if the <see cref="SecurityProviderBase.PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SecurityProviderBase.SettingsCategory"/> has a value of null or empty string.</exception>
        public override void LoadSettings()
        {
            base.LoadSettings();

            // Make sure default settings exist
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];

            settings.Add("DataProviderString", "Eval(systemSettings.DataProviderString)", "Configuration database ADO.NET data provider assembly type creation string to be used for connection to the backend security data store.");
            settings.Add("LdapPath", "", "Specifies the LDAP path used to initialize the security provider.");
            settings.Add("PasswordRequirementsRegex", DefaultPasswordRequirementsRegex, "Regular expression used to validate new passwords for database users.");
            settings.Add("PasswordRequirementsError", DefaultPasswordRequirementsError, "Error message to be displayed when new database user password fails regular expression test.");

            m_passwordRequirementsRegex = settings["PasswordRequirementsRegex"].ValueAs(m_passwordRequirementsRegex);
            m_passwordRequirementsError = settings["PasswordRequirementsError"].ValueAs(m_passwordRequirementsError);
        }

        /// <summary>
        /// Refreshes the <see cref="UserData"/>.
        /// </summary>
        /// <returns>true if <see cref="SecurityProviderBase.UserData"/> is refreshed, otherwise false.</returns>
        public override bool RefreshData()
        {
            if (string.IsNullOrEmpty(UserData.Username))
                return false;

            try
            {
                // Initialize user data.
                UserData.Initialize();

                // We'll retrieve all the data we need about a user.
                //   Table1: Information about the user.
                //   Table2: Groups the user is a member of.
                //   Table3: Roles that are assigned to the user either implicitly (NT groups) or explicitly (database) or through a group.

                DataSet securityContext;

                try
                {
                    // Attempt to extract current security context from the database
                    using (AdoDataConnection database = new AdoDataConnection(SettingsCategory))
                    {
                        securityContext = ExtractSecurityContext(database.Connection, ex => LogError(ex.Source, ex.ToString()));
                    }
                }
                catch (InvalidOperationException)
                {
                    // Failed to open ADO connection, attempt to fall back on using security context from last known cached information
                    using (AdoSecurityCache cache = AdoSecurityCache.GetCurrentCache())
                        securityContext = cache.DataSet;
                }

                try
                {
                    // Make sure primary keys are defined on tables that we use "Find" function on later...
                    UpdatePrimaryKey(securityContext.Tables[SecurityGroupTable], "SecurityGroupID");
                    UpdatePrimaryKey(securityContext.Tables[ApplicationRoleTable], "ApplicationRoleID");
                }
                catch
                {
                    // Just going to fall back on slower "Select" function if this fails...
                }

                // Validate that needed security tables exist in the data set
                if ((object)securityContext == null)
                    throw new SecurityException("Failed to load a valid security context. Cannot proceed with user authentication.");

                foreach (string securityTable in s_securityTables)
                {
                    if (!securityContext.Tables.Contains(securityTable))
                        throw new SecurityException($"Failed to load a valid security context - missing table '{securityTable}'. Cannot proceed with user authentication.");
                }

                if (securityContext.Tables[ApplicationRoleTable].Rows.Count == 0)
                    throw new SecurityException($"Failed to load a valid security context - no application roles were found for node ID '{DefaultNodeID}', verify the node ID in the config file '{ConfigurationFile.Current.Configuration.FilePath}'. Cannot proceed with user authentication.");

                DataRow userAccount = null;
                Guid userAccountID = Guid.Empty;
                string userSID = UserInfo.UserNameToSID(UserData.Username);

                // Filter user account data for the current user.
                DataRow[] userAccounts = securityContext.Tables[UserAccountTable].Select($"Name = '{EncodeEscapeSequences(userSID)}'");

                // If SID based lookup failed, try lookup by user name.  Note that is critical that SID based lookup
                // take precedence over name based lookup for proper cross-platform authentication.
                if (userAccounts.Length == 0)
                    userAccounts = securityContext.Tables[UserAccountTable].Select($"Name = '{EncodeEscapeSequences(UserData.Username)}'");

                if (userAccounts.Length == 0)
                {
                    // User doesn't exist in the database, however, user may exist in an NT authentication group which
                    // may have an explicit role assignment. To test for this case we make the assumption that this is
                    // a Windows authenticated user and test for rights within groups
                    UserData.IsDefined = true;
                    UserData.IsExternal = false;
                }
                else
                {
                    userAccount = userAccounts[0];
                    UserData.IsDefined = true;
                    UserData.IsExternal = !Convert.ToBoolean(userAccount["UseADAuthentication"]);
                    userAccountID = Guid.Parse(Convert.ToString(userAccount["ID"]));
                }

                if (UserData.IsExternal && (object)userAccount != null)
                {
                    // Load database user details
                    if (string.IsNullOrEmpty(UserData.LoginID))
                        UserData.LoginID = UserData.Username;

                    if (!Convert.IsDBNull(userAccount["Password"]))
                        UserData.Password = Convert.ToString(userAccount["Password"]);

                    if (!Convert.IsDBNull(userAccount["FirstName"]))
                        UserData.FirstName = Convert.ToString(userAccount["FirstName"]);

                    if (!Convert.IsDBNull(userAccount["LastName"]))
                        UserData.LastName = Convert.ToString(userAccount["LastName"]);

                    if (!Convert.IsDBNull(userAccount["Phone"]))
                        UserData.PhoneNumber = Convert.ToString(userAccount["Phone"]);

                    if (!Convert.IsDBNull(userAccount["Email"]))
                        UserData.EmailAddress = Convert.ToString(userAccount["Email"]);

                    if (!Convert.IsDBNull(userAccount["ChangePasswordOn"]))
                        UserData.PasswordChangeDateTime = Convert.ToDateTime(userAccount["ChangePasswordOn"]);

                    if (!Convert.IsDBNull(userAccount["CreatedOn"]))
                        UserData.AccountCreatedDateTime = Convert.ToDateTime(userAccount["CreatedOn"]);

                    // For possible future use:
                    //if (!Convert.IsDBNull(userDataRow["UserCompanyName"]))
                    //    UserData.CompanyName = Convert.ToString(userDataRow["UserCompanyName"]);
                    //if (!Convert.IsDBNull(userDataRow["UserSecurityQuestion"]))
                    //    UserData.SecurityQuestion = Convert.ToString(userDataRow["UserSecurityQuestion"]);
                    //if (!Convert.IsDBNull(userDataRow["UserSecurityAnswer"]))
                    //    UserData.SecurityAnswer = Convert.ToString(userDataRow["UserSecurityAnswer"]);
                }
                else
                {
                    // Load implicitly assigned groups - this happens via user's NT/AD groups that get loaded into the
                    // user data group collection. When database group definitions are defined with the same name as
                    // their NT/AD equivalents, this will allow automatic external group management.
                    base.RefreshData(UserData.Groups, ProviderID);
                }

                // Administrator can lock out NT/AD user as well as database-only user via database
                if (!UserData.IsLockedOut && (object)userAccount != null && !Convert.IsDBNull(userAccount["LockedOut"]))
                    UserData.IsLockedOut = Convert.ToBoolean(userAccount["LockedOut"]);

                // At this point an NT/AD based user will have a list of groups that is known to be available to the user. A database
                // user will have no groups defined yet. The next step will be to load any explicitly assigned groups the user is a
                // member of. Users can be explicitly assigned to any group, database or NT/AD group. This allows flexibility for
                // local database users to be assigned to an NT/AD group (in the database) as well as NT/AD users to be a member of
                // local database groups - allowing customizable role assignments to be fully managed via groups.

                // Note: some groups, such as "Everyone" (S-1-1-0), will not be automatically returned as valid groups by the base
                // LdapSecurityProvider (see UserInfo class for details). If the security provider needs to support such groups,
                // these groups will need to be manually added to the groups list here before testing to see if the user is a
                // member of these groups. Since these types of groups could be considered a security risk by some applications,
                // it is recommended that if this type of functionality is enabled, that it can be disabled in the configuration.

                if (userAccountID != Guid.Empty)
                {
                    // Filter explicitly assigned security groups for current user
                    DataRow[] userGroups = securityContext.Tables[SecurityGroupUserAccountTable].Select($"UserAccountID = '{EncodeEscapeSequences(userAccountID.ToString())}'");

                    foreach (DataRow row in userGroups)
                    {
                        // Locate associated security group record
                        DataRow securityGroup = null;

                        if (securityContext.Tables[SecurityGroupTable].PrimaryKey.Length > 0)
                        {
                            securityGroup = securityContext.Tables[SecurityGroupTable].Rows.Find(row["SecurityGroupID"]);
                        }
                        else
                        {
                            DataRow[] securityGroups = securityContext.Tables[SecurityGroupTable].Select($"ID = '{EncodeEscapeSequences(row["SecurityGroupID"].ToString())}'");

                            if (securityGroups.Length > 0)
                                securityGroup = securityGroups[0];
                        }

                        if ((object)securityGroup == null || Convert.IsDBNull(securityGroup["Name"]))
                            continue;

                        // Just in case a database user was manually assigned to an NT/AD group, make sure to convert
                        // the group name back to it's human readable name (from a SID). The UserInfo.SIDToAccountName
                        // function will return the original parameter value if name cannot be converted - this allows
                        // the function to work for database group names as well.
                        string groupName = UserInfo.SIDToAccountName(Convert.ToString(securityGroup["Name"]));

                        if (!UserData.Groups.Contains(groupName, StringComparer.OrdinalIgnoreCase))
                            UserData.Groups.Add(groupName);
                    }
                }

                // Explicitly assigned user roles will take precedence over any roles that may be implicitly available to the
                // user based on what group they may be a member of. In practice this means even though a user may be a member
                // of a group with role assignments, if there are any roles directly assigned to the user these will be their
                // effective roles. For example, if a user is in a group that is in the "Administrator" role and the user has
                // an explicit role assignment for the "Viewer" role - the user will be in the "Viewer" role only. This allows
                // individual users to have overridden role assignments even though they may also be part of a group.
                UserData.Roles.Clear();

                // Filter explicitly assigned application roles for current user - this will return an empty set if no
                // explicitly defined roles exist for the user -or- user doesn't exist in the database.
                DataRow[] userApplicationRoles = securityContext.Tables[ApplicationRoleUserAccountTable].Select($"UserAccountID = '{EncodeEscapeSequences(userAccountID.ToString())}'");

                // If no explicitly assigned application roles are found for the current user, we check for implicitly assigned
                // application roles based on the role assignments of the groups the user is a member of.
                if (userApplicationRoles.Length == 0)
                {
                    List<DataRow> implicitRoles = new List<DataRow>();

                    // Filter implicitly assigned application roles for each of the user's database and NT/AD groups. Note that
                    // even if user is not defined in the database, an NT/AD group they are a member of may be associated with
                    // a role - this allows the user to get a role assignment based on this group.
                    foreach (string groupName in UserData.Groups)
                    {
                        // Convert NT/AD group names back to SIDs for lookup in the database
                        string groupSID = UserInfo.GroupNameToSID(groupName);

                        // Locate associated security group record
                        DataRow[] securityGroups = securityContext.Tables[SecurityGroupTable].Select($"Name = '{EncodeEscapeSequences(groupSID)}'");

                        // If SID based lookup failed, try lookup by group name.  Note that is critical that SID based lookup
                        // take precedence over name based lookup for proper cross-platform authentication.
                        if (securityGroups.Length == 0)
                            securityGroups = securityContext.Tables[SecurityGroupTable].Select($"Name = '{EncodeEscapeSequences(groupName)}'");

                        if (securityGroups.Length > 0)
                        {
                            // Found security group by name, access group ID to lookup application roles defined for the group
                            DataRow securityGroup = securityGroups[0];

                            if (!Convert.IsDBNull(securityGroup["ID"]))
                                implicitRoles.AddRange(securityContext.Tables[ApplicationRoleSecurityGroupTable].Select($"SecurityGroupID = '{EncodeEscapeSequences(securityGroup["ID"].ToString())}'"));
                        }
                    }

                    userApplicationRoles = implicitRoles.ToArray();
                }

                // Populate user roles collection - both ApplicationRoleUserAccount and ApplicationRoleSecurityGroup tables contain ApplicationRoleID column
                foreach (DataRow role in userApplicationRoles)
                {
                    if (Convert.IsDBNull(role["ApplicationRoleID"]))
                        continue;

                    // Locate associated application role record
                    DataRow applicationRole = null;

                    if (securityContext.Tables[ApplicationRoleTable].PrimaryKey.Length > 0)
                    {
                        applicationRole = securityContext.Tables[ApplicationRoleTable].Rows.Find(role["ApplicationRoleID"]);
                    }
                    else
                    {
                        DataRow[] applicationRoles = securityContext.Tables[ApplicationRoleTable].Select($"ID = '{EncodeEscapeSequences(role["ApplicationRoleID"].ToString())}'");

                        if (applicationRoles.Length > 0)
                            applicationRole = applicationRoles[0];
                    }

                    if ((object)applicationRole == null || Convert.IsDBNull(applicationRole["Name"]))
                        continue;

                    // Found application role by ID, add role name to user roles if not already defined
                    string roleName = Convert.ToString(applicationRole["Name"]);

                    if (!string.IsNullOrEmpty(roleName) && !UserData.Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase))
                        UserData.Roles.Add(roleName);
                }

                // Cache last user roles
                ThreadPool.QueueUserWorkItem(CacheLastUserRoles);

                return true;
            }
            catch (Exception ex)
            {
                m_lastException = ex;
                LogError(ex.Source, ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="password">Password to be used for authentication.</param>
        /// <returns>true if the user is authenticated, otherwise false.</returns>
        public override bool Authenticate(string password)
        {
            Exception authenticationException = null;

            // Reset authenticated state and failure reason
            UserData.IsAuthenticated = false;
            AuthenticationFailureReason = null;
            m_successfulPassThroughAuthentication = false;

            // Test for pre-authentication failure modes. Note that blank password should be allowed so that LDAP
            // can authenticate current credentials for pass through authentication, if desired.
            if (!UserData.IsDefined)
            {
                AuthenticationFailureReason = $"User \"{UserData.LoginID}\" is not defined.";
            }
            else if (UserData.IsDisabled)
            {
                AuthenticationFailureReason = $"User \"{UserData.LoginID}\" is disabled.";
            }
            else if (UserData.IsLockedOut)
            {
                AuthenticationFailureReason = $"User \"{UserData.LoginID}\" is locked out.";
            }
            else if (UserData.PasswordChangeDateTime != DateTime.MinValue && UserData.PasswordChangeDateTime <= DateTime.UtcNow)
            {
                AuthenticationFailureReason = $"User \"{UserData.LoginID}\" has an expired password or password has not been set.";
            }
            else if (UserData.Roles.Count == 0)
            {
                AuthenticationFailureReason = $"User \"{UserData.LoginID}\" has not been assigned any roles and therefore has no rights. Contact your administrator.";
            }
            else
            {
                try
                {
                    // Determine if user is LDAP or database authenticated
                    if (!UserData.IsExternal)
                    {
                        // Authenticate against active directory (via LDAP base class) - in context of ADO security
                        // provisions, you are only authenticated if you are in a role!
                        UserData.IsAuthenticated = base.Authenticate(password);
                    }
                    else
                    {
                        Password = password;

                        // Authenticate against backend data store
                        UserData.IsAuthenticated =
                            UserData.Password == password ||
                            UserData.Password == SecurityProviderUtility.EncryptPassword(password);
                    }
                }
                catch (Exception ex)
                {
                    authenticationException = ex;
                }
            }

            // Determine if user succeeded with pass-through authentication
            m_successfulPassThroughAuthentication = (string.IsNullOrWhiteSpace(password) && UserData.IsAuthenticated);

            // Log user authentication result if provider has completed initialization sequence or
            // was successfully authenticated via pass-through authentication
            if (Initialized || m_successfulPassThroughAuthentication)
            {
                try
                {
                    // Writing data will fail for read-only databases
                    LogAuthenticationAttempt(UserData.IsAuthenticated);
                }
                catch (Exception ex)
                {
                    // All we can do is track last exception in this case
                    m_lastException = ex;
                }
            }

            // If an exception occurred during authentication, rethrow it after logging authentication attempt
            if ((object)authenticationException != null)
            {
                m_lastException = authenticationException;
                LogError(authenticationException.Source, authenticationException.ToString());
                throw authenticationException;
            }

            return UserData.IsAuthenticated;
        }

        /// <summary>
        /// Changes user password in the backend data store.
        /// </summary>
        /// <param name="oldPassword">User's current password.</param>
        /// <param name="newPassword">User's new password.</param>
        /// <returns>true if the password is changed, otherwise false.</returns>
        /// <exception cref="SecurityException"><paramref name="newPassword"/> does not meet password requirements.</exception>
        public override bool ChangePassword(string oldPassword, string newPassword)
        {
            // Check prerequisites.
            if (!UserData.IsDefined || UserData.IsDisabled || UserData.IsLockedOut)
                return false;

            try
            {
                // Check if old and new passwords are different.
                if (oldPassword == newPassword)
                    throw new Exception("New password cannot be same as old password.");

                // If needed, perform password change for internal NT/AD users.
                if (!UserData.IsExternal)
                    return base.ChangePassword(oldPassword, newPassword);

                // Verify old password.
                UserData.PasswordChangeDateTime = DateTime.MinValue;

                if (!Authenticate(oldPassword))
                    return false;

                // Verify new password.
                if (!Regex.IsMatch(newPassword, m_passwordRequirementsRegex))
                    throw new SecurityException(m_passwordRequirementsError);

                using (IDbConnection dbConnection = (new AdoDataConnection(SettingsCategory)).Connection)
                {
                    if ((object)dbConnection == null)
                        return false;

                    bool oracle = dbConnection.GetType().Name == "OracleConnection";
                    IDbCommand command = dbConnection.CreateCommand();
                    command.CommandType = CommandType.Text;

                    if (command.Connection.ConnectionString.Contains("Microsoft.Jet.OLEDB"))
                        command.CommandText = "UPDATE UserAccount SET [Password] = @newPassword, ChangePasswordOn = @changePasswordOn WHERE Name = @name AND [Password] = @oldPassword";
                    else if (oracle)
                        command.CommandText = "UPDATE UserAccount SET Password = :newPassword, ChangePasswordOn = :changePasswordOn WHERE Name = :name AND Password = :oldPassword";
                    else
                        command.CommandText = "UPDATE UserAccount SET Password = @newPassword, ChangePasswordOn = @changePasswordOn WHERE Name = @name AND Password = @oldPassword";

                    IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = oracle ? ":newPassword" : "@newPassword";
                    param.Value = SecurityProviderUtility.EncryptPassword(newPassword);
                    command.Parameters.Add(param);

                    param = command.CreateParameter();
                    param.ParameterName = oracle ? ":changePasswordOn" : "@changePasswordOn";
                    param.Value = DateTime.UtcNow.AddDays(90.0D);
                    command.Parameters.Add(param);

                    param = command.CreateParameter();
                    param.ParameterName = oracle ? ":name" : "@name";
                    param.Value = UserData.Username;
                    command.Parameters.Add(param);

                    param = command.CreateParameter();
                    param.ParameterName = oracle ? ":oldPassword" : "@oldPassword";
                    param.Value = UserData.Password;
                    command.Parameters.Add(param);

                    command.ExecuteNonQuery();
                }

                return true;
            }
            catch (SecurityException ex)
            {
                m_lastException = ex;
                throw;
            }
            catch (Exception ex)
            {
                m_lastException = ex;
                LogError(ex.Source, ex.ToString());
                throw;
            }
            finally
            {
                RefreshData();
            }
        }

        /// <summary>
        /// Logs user authentication attempt.
        /// </summary>
        /// <param name="loginSuccess">true if user authentication was successful, otherwise false.</param>
        /// <returns>true if logging was successful, otherwise false.</returns>
        protected virtual bool LogAuthenticationAttempt(bool loginSuccess)
        {
            if ((object)UserData != null && !string.IsNullOrWhiteSpace(UserData.Username))
            {
                string message = $"User \"{UserData.Username}\" login attempt {(loginSuccess ? "succeeded using " + (m_successfulPassThroughAuthentication ? "pass-through authentication" : "user acquired password") : "failed")}.";
                EventLogEntryType entryType = loginSuccess ? EventLogEntryType.SuccessAudit : EventLogEntryType.FailureAudit;

                // Suffix authentication failure reason on failed logins if available
                if (!loginSuccess && !string.IsNullOrWhiteSpace(AuthenticationFailureReason))
                    message = string.Concat(message, " ", AuthenticationFailureReason);

                // Attempt to write success or failure to the event log
                try
                {
                    LogEvent(ApplicationName, message, entryType, 1);
                }
                catch (Exception ex)
                {
                    LogError(ex.Source, ex.ToString());
                }

                // Attempt to write success or failure to the database - we allow caller to catch any possible exceptions here so that
                // database exceptions can be tracked separately (via LastException property) from other login exceptions, e.g., when
                // a read-only database is being used or current user only has read-only access to database.
                if (!string.IsNullOrWhiteSpace(SettingsCategory))
                {
                    AdoDataConnection database = new AdoDataConnection(SettingsCategory);

                    using (IDbConnection connection = database.Connection)
                    {
                        connection.ExecuteNonQuery(database.ParameterizedQueryString("INSERT INTO AccessLog (UserName, AccessGranted) VALUES ({0}, {1})", "userName", "accessGranted"), UserData.Username, loginSuccess ? 1 : 0);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Logs information about an encountered exception to the backend data store.
        /// </summary>
        /// <param name="source">Source of the exception.</param>
        /// <param name="message">Detailed description of the exception.</param>
        /// <returns>true if logging was successful, otherwise false.</returns>
        protected virtual bool LogError(string source, string message)
        {
            if (!string.IsNullOrWhiteSpace(SettingsCategory) && !string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    AdoDataConnection database = new AdoDataConnection(SettingsCategory);

                    using (IDbConnection connection = database.Connection)
                    {
                        connection.ExecuteNonQuery(database.ParameterizedQueryString("INSERT INTO ErrorLog (Source, Message) VALUES ({0}, {1})", "source", "message"), source, message);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    // All we can do is track last exception in this case
                    m_lastException = ex;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the LDAP path.
        /// </summary>
        /// <returns>The LDAP path.</returns>
        protected override string GetLdapPath()
        {
            // Load connection settings from the system settings category				
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection configSettings = config.Settings[SettingsCategory];
            return configSettings["LdapPath"].Value;
        }

        // Cache last user roles and monitor for changes
        private void CacheLastUserRoles(object state)
        {
            try
            {
                // Using an inter-process cache for user roles
                using (UserRoleCache userRoleCache = UserRoleCache.GetCurrentCache())
                {
                    HashSet<string> currentRoles;
                    string[] cachedRoles;

                    // Retrieve current user roles
                    currentRoles = new HashSet<string>(UserData.Roles, StringComparer.OrdinalIgnoreCase);

                    // Attempt to retrieve cached user roles
                    cachedRoles = userRoleCache[UserData.Username];

                    bool rolesChanged = false;
                    string message;
                    EventLogEntryType entryType;

                    if ((object)cachedRoles == null)
                    {
                        if (currentRoles.Count == 0)
                        {
                            // New user access granted
                            message = $"Initial Encounter: user \"{UserData.Username}\" attempted login with no assigned roles.";
                            entryType = EventLogEntryType.FailureAudit;
                            rolesChanged = true;
                        }
                        else
                        {
                            // New user access granted
                            message = $"Initial Encounter: user \"{UserData.Username}\" granted access with role{(currentRoles.Count == 1 ? "" : "s")} \"{currentRoles.ToDelimitedString(", ")}\".";
                            entryType = EventLogEntryType.Information;
                            rolesChanged = true;
                        }
                    }
                    else if (!currentRoles.SetEquals(cachedRoles))
                    {
                        if (currentRoles.Count == 0)
                        {
                            // New user access granted
                            message = $"Subsequent Encounter: user \"{UserData.Username}\" attempted login with no assigned roles - role assignment that existed at last login was \"{cachedRoles.ToDelimitedString(", ")}\".";
                            entryType = EventLogEntryType.FailureAudit;
                            rolesChanged = true;
                        }
                        else
                        {
                            // User role access changed
                            message = $"Subsequent Encounter: user \"{UserData.Username}\" granted access with new role{(currentRoles.Count == 1 ? "" : "s")} \"{currentRoles.ToDelimitedString(", ")}\" - role assignment is different from last login, was \"{cachedRoles.ToDelimitedString(", ")}\".";
                            entryType = EventLogEntryType.Warning;
                            rolesChanged = true;
                        }
                    }
                    else
                    {
                        if (currentRoles.Count == 0)
                        {
                            // New user access granted
                            message = $"Subsequent Encounter: user \"{UserData.Username}\" attempted login with no assigned roles - same as last login attempt.";
                            entryType = EventLogEntryType.FailureAudit;
                            rolesChanged = true;
                        }
                        else
                        {
                            message = $"Subsequent Encounter: user \"{UserData.Username}\" granted access with role{(currentRoles.Count == 1 ? "" : "s")} \"{currentRoles.ToDelimitedString(", ")}\" - role assignment is the same as last login.";
                            entryType = EventLogEntryType.SuccessAudit;
                        }
                    }

                    // Log granted role access to event log
                    try
                    {
                        LogEvent(ApplicationName, message, entryType, 0);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Source, ex.ToString());
                    }

                    // If role has changed, update cache
                    if (rolesChanged)
                    {
                        userRoleCache[UserData.Username] = currentRoles.ToArray();
                        userRoleCache.WaitForSave();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Source, ex.ToString());
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly string[] s_securityTables =
        {
            UserAccountTable,                   // User accounts
            SecurityGroupTable,                 // Security groups
            SecurityGroupUserAccountTable,      // Security group user accounts (i.e., users in a group)
            ApplicationRoleTable,               // Application roles (node specific)
            ApplicationRoleUserAccountTable,    // Application role assignments for user accounts
            ApplicationRoleSecurityGroupTable   // Application role assignments for security groups
        };

        /// <summary>
        /// Gets current default Node ID for security.
        /// </summary>
        public static readonly Guid DefaultNodeID;

        // Static Constructor
        static AdoSecurityProvider()
        {
            // Access configuration file system settings
            CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings["systemSettings"];

            // Make sure NodeID setting exists
            systemSettings.Add("NodeID", Guid.NewGuid().ToString(), "Unique Node ID");

            // Get NodeID as currently defined in configuration file
            DefaultNodeID = Guid.Parse(systemSettings["NodeID"].Value.ToNonNullString(Guid.NewGuid().ToString()));

            // Determine whether the node exists in the database and create it if it doesn't
            if (DefaultNodeID != Guid.Empty)
            {
                using (AdoDataConnection connection = new AdoDataConnection(DefaultSettingsCategory))
                {
                    const string NodeCountFormat = "SELECT COUNT(*) FROM Node";
                    const string NodeInsertFormat = "INSERT INTO Node(Name, Description, Enabled) VALUES('Default', 'Default node', 1)";
                    const string NodeUpdateFormat = "UPDATE Node SET ID = {0}";

                    int nodeCount = connection.ExecuteScalar<int?>(NodeCountFormat) ?? 0;

                    if (nodeCount == 0)
                    {
                        connection.ExecuteNonQuery(NodeInsertFormat);
                        connection.ExecuteNonQuery(NodeUpdateFormat, connection.Guid(DefaultNodeID));
                    }
                }
            }

            TableOperations<UserAccount>.TypeRegistry.RegisterType<AdoSecurityProvider>();
        }

        // Static Methods

        /// <summary>
        /// Extracts the current security context from the database.
        /// </summary>
        /// <param name="connection">Existing database connection used to extract security context.</param>
        /// <param name="exceptionHandler">Exception handler to use for any exceptions encountered while updating security cache.</param>
        /// <returns>A new <see cref="DataSet"/> containing the latest security context.</returns>
        public static DataSet ExtractSecurityContext(IDbConnection connection, Action<Exception> exceptionHandler)
        {
            DataSet securityContext = new DataSet("AdoSecurityContext");

            // Read the security context tables from the database connection
            foreach (string securityTable in s_securityTables)
            {
                AddSecurityContextTable(connection, securityContext, securityTable, securityTable == ApplicationRoleTable ? DefaultNodeID : default(Guid));
            }

            // Always cache security context after successful extraction
            Thread cacheSecurityContext = new Thread(() =>
            {
                try
                {
                    using (AdoSecurityCache cache = AdoSecurityCache.GetCurrentCache())
                    {
                        cache.DataSet = securityContext;
                        cache.WaitForSave();
                    }
                }
                catch (Exception ex)
                {
                    exceptionHandler(ex);
                }
            });

            cacheSecurityContext.IsBackground = true;
            cacheSecurityContext.Start();

            return securityContext;
        }

        private static void UpdatePrimaryKey(DataTable table, string columnName)
        {
            if (table.PrimaryKey.Length == 0)
                table.PrimaryKey = new[] { table.Columns[columnName] };
        }

        private static void AddSecurityContextTable(IDbConnection connection, DataSet securityContext, string tableName, Guid nodeID)
        {
            string tableQuery;

            if (nodeID == default(Guid))
                tableQuery = $"SELECT * FROM {tableName}";
            else
                tableQuery = $"SELECT * FROM {tableName} WHERE NodeID = '{nodeID}'";

            using (IDataReader reader = connection.ExecuteReader(tableQuery))
            {
                securityContext.Tables.Add(tableName).Load(reader);
            }
        }

        private static string EncodeEscapeSequences(string value)
        {
            return value.ToNonNullString().Replace("\\", "\\\\");
        }

        #endregion
    }
}
