//******************************************************************************************************
//  SecurityProviderBase.cs - Gbtc
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
//  03/22/2010 - Pinal C. Patel
//       Generated original version of source code.
//  05/24/2010 - Pinal C. Patel
//       Modified RefreshData() method to not query the AD at all for external user.
//  05/27/2010 - Pinal C. Patel
//       Added usage example to code comments.
//  06/15/2010 - Pinal C. Patel
//       Added checks to the in-process caching logic.
//  06/24/2010 - Pinal C. Patel
//       Added LogError() and LogLogin() methods.
//  06/25/2010 - Pinal C. Patel
//       Fixed a issue in the caching mechanism employed in Current static property.
//  12/03/2010 - Pinal C. Patel
//       Added TranslateRole() to allow providers to perform translation on role name.
//  01/05/2011 - Pinal C. Patel
//       Added CanRefreshData, CanUpdateData, CanResetPassword and CanChangePassword properties along 
//       with accompanying RefreshData(), UpdateData(), ResetPassword() and ChangePassword() methods.
//  11/23/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;
using GSF.Configuration;

namespace GSF.Security
{
    /// <summary>
    /// Base class for a provider of role-based security in applications.
    /// </summary>
    /// <example>
    /// This examples shows how to extend <see cref="SecurityProviderBase"/> to use a flat-file for the security data store:
    /// <code>
    /// using System.Data;
    /// using System.IO;
    /// using GSF;
    /// using GSF.Data;
    /// using GSF.IO;
    /// using GSF.Security;
    /// 
    /// namespace CustomSecurity
    /// {
    ///     public class FlatFileSecurityProvider : SecurityProviderBase
    ///     {
    ///         private const int LeastPrivilegedLevel = 5;
    /// 
    ///         public FlatFileSecurityProvider(string username)
    ///             : base(username)
    ///         {
    ///         }
    /// 
    ///         public override bool RefreshData()
    ///         {
    ///             // Check for a valid username.
    ///             if (string.IsNullOrEmpty(UserData.Username))
    ///                 return false;
    /// 
    ///             // Check if a file name is specified.
    ///             if (string.IsNullOrEmpty(ConnectionString))
    ///                 return false;
    /// 
    ///             // Check if file exist on file system.
    ///             string file = FilePath.GetAbsolutePath(ConnectionString);
    ///             if (!File.Exists(file))
    ///                 return false;
    /// 
    ///             // Read the data from the specified file.
    ///             DataTable data = File.ReadAllText(file).ToDataTable(",", true);
    ///             DataRow[] user = data.Select(string.Format("Username = '{0}'", UserData.Username));
    ///             if (user.Length > 0)
    ///             {
    ///                 // User exists in the specified file.
    ///                 UserData.IsDefined = true;
    ///                 UserData.Password = user[0]["Password"].ToNonNullString();
    /// 
    ///                 for (int i = LeastPrivilegedLevel; i >= int.Parse(user[0]["Level"].ToNonNullString()); i--)
    ///                 {
    ///                     UserData.Roles.Add(i.ToString());
    ///                 }
    ///             }
    /// 
    ///             return true;
    ///         }
    /// 
    ///         public override bool Authenticate(string password)
    ///         {
    ///             // Compare password hashes to authenticate.
    ///             return (UserData.Password == SecurityProviderUtility.EncryptPassword(password));
    ///         }
    ///     }
    /// }
    /// </code>
    /// Config file entries that go along with the above example:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="GSF.Configuration.CategorizedSettingsSection, GSF.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <securityProvider>
    ///       <add name="ApplicationName" value="SEC_APP" description="Name of the application being secured as defined in the backend security datastore."
    ///         encrypted="false" />
    ///       <add name="ConnectionString" value="Security.csv" description="Connection string to be used for connection to the backend security datastore."
    ///         encrypted="false" />
    ///       <add name="ProviderType" value="CustomSecurity.FlatFileSecurityProvider, CustomSecurity"
    ///         description="The type to be used for enforcing security." encrypted="false" />
    ///       <add name="IncludedResources" value="*=*" description="Semicolon delimited list of resources to be secured along with role names."
    ///         encrypted="false" />
    ///       <add name="ExcludedResources" value="" description="Semicolon delimited list of resources to be excluded from being secured."
    ///         encrypted="false" />
    ///       <add name="NotificationSmtpServer" value="localhost" description="SMTP server to be used for sending out email notification messages."
    ///         encrypted="false" />
    ///       <add name="NotificationSenderEmail" value="sender@company.com" description="Email address of the sender of email notification messages." 
    ///         encrypted="false" />
    ///     </securityProvider>
    ///   </categorizedSettings>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="SecurityIdentity"/>
    /// <seealso cref="SecurityPrincipal"/>
    public abstract class SecurityProviderBase : ISecurityProvider
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ApplicationName"/> property.
        /// </summary>
        public const string DefaultApplicationName = "SEC_APP";

        /// <summary>
        /// Specifies the default value for the <see cref="ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "Primary={Server=DB1;Database=AppSec;Trusted_Connection=True};Backup={Server=DB2;Database=AppSec;Trusted_Connection=True}";

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = true;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "SecurityProvider";

        // Fields
        private string m_applicationName;
        private string m_connectionString;
        private IPrincipal m_passthroughPrincipal;
        private SecureString m_securePassword;
        private string m_authenticationFailureReason;
        private string m_settingsCategory;
        private UserData m_userData;
        private bool m_isUserAuthenticated;
        private bool m_persistSettings;
        private readonly bool m_canRefreshData;
        private readonly bool m_canResetPassword;
        private readonly bool m_canChangePassword;
        private LogEventFunctionSignature m_logEvent;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the security provider.
        /// </summary>
        /// <param name="username">Name that uniquely identifies the user.</param>
        /// <param name="canRefreshData">true if the security provider can refresh <see cref="UserData"/> from the backend data store, otherwise false.</param>
        /// <param name="canResetPassword">true if the security provider can reset user password, otherwise false.</param>
        /// <param name="canChangePassword">true if the security provider can change user password, otherwise false.</param>
        protected SecurityProviderBase(string username, bool canRefreshData, bool canResetPassword, bool canChangePassword)
        {
            // Initialize member variables.
            m_userData = new UserData(username);
            m_canRefreshData = canRefreshData;
            m_canResetPassword = canResetPassword;
            m_canChangePassword = canChangePassword;
            m_applicationName = DefaultApplicationName;
            m_connectionString = DefaultConnectionString;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_logEvent = EventLog.WriteEntry;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the application being secured as defined in the backend security datastore.
        /// </summary>
        public string ApplicationName
        {
            get
            {
                return m_applicationName;
            }
            set
            {
                m_applicationName = value;
            }
        }

        /// <summary>
        /// Gets or sets the connection string to be used for connection to the backend security datastore.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
            }
        }

        /// <summary>
        /// Gets or sets the principal used for passthrough authentication.
        /// </summary>
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
        /// Gets or sets the password as a <see cref="SecureString"/>.
        /// </summary>
        public virtual SecureString SecurePassword
        {
            get
            {
                return m_securePassword;
            }
            set
            {
                if ((object)m_securePassword != null)
                    m_securePassword.Dispose();

                m_securePassword = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="SecurePassword"/> as clear text password.
        /// </summary>
        public string Password
        {
            get
            {
                return SecurePassword.ToUnsecureString();
            }
            set
            {
                SecurePassword = value.ToSecureString();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="LogEventFunctionSignature"/> to use for logging security events for the <see cref="SecurityProviderBase"/> implementation.
        /// </summary>
        /// <remarks>
        /// Set <see cref="LogEvent"/> to <c>null</c> to use default handler, i.e., <see cref="EventLog.WriteEntry(string,string,EventLogEntryType,int)"/>.
        /// </remarks>
        public virtual LogEventFunctionSignature LogEvent
        {
            get
            {
                return m_logEvent;
            }
            set
            {
                if ((object)value == null)
                    m_logEvent = EventLog.WriteEntry;
                else
                    m_logEvent = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether security provider settings are to be saved to the config file.
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
        /// Gets or sets the category under which security provider settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
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
        /// Gets the <see cref="UserData"/> object containing information about the user.
        /// </summary>
        public virtual UserData UserData
        {
            get
            {
                return m_userData;
            }
            protected set
            {
                m_userData = value;
            }
        }

        /// <summary>
        /// Gets the flag that indicates whether the user was
        /// authenticated during the last authentication attempt.
        /// </summary>
        public virtual bool IsUserAuthenticated
        {
            get
            {
                return m_isUserAuthenticated;
            }
            protected set
            {
                m_isUserAuthenticated = value;
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether <see cref="RefreshData"/> operation is supported.
        /// </summary>
        public virtual bool CanRefreshData
        {
            get
            {
                return m_canRefreshData;
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether <see cref="ResetPassword"/> operation is supported.
        /// </summary>
        public virtual bool CanResetPassword
        {
            get
            {
                return m_canResetPassword;
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether <see cref="ChangePassword"/> operation is supported.
        /// </summary>
        public virtual bool CanChangePassword
        {
            get
            {
                return m_canChangePassword;
            }
        }

        /// <summary>
        /// Gets or allows derived classes to set an authentication failure reason.
        /// </summary>
        public virtual string AuthenticationFailureReason
        {
            get
            {
                return m_authenticationFailureReason;
            }
            protected set
            {
                m_authenticationFailureReason = value;
            }
        }

        /// <summary>
        /// Gets the flag that indicates whether the user 
        /// needs to be redirected after the Authentication attempt. 
        /// </summary>
        public virtual bool IsRedirectRequested => false;

        /// <summary>
        /// Gets the URI that user will be redirected to if <see cref="IsRedirectRequested"/> is set.
        /// </summary>
        public virtual string RequestedRedirect => "";

        #endregion

        #region [ Methods ]

        #region [ Abstract ]

        /// <summary>
        /// When overridden in a derived class, authenticates the user.
        /// </summary>
        /// <returns>true if the user is authenticated, otherwise false.</returns>
        public abstract bool Authenticate();

        /// <summary>
        /// When overridden in a derived class, refreshes the <see cref="UserData"/> from the backend datastore.
        /// </summary>
        /// <returns>true if <see cref="UserData"/> is refreshed, otherwise false.</returns>
        public abstract bool RefreshData();

        /// <summary>
        /// When overridden in a derived class, resets user password in the backend datastore.
        /// </summary>
        /// <param name="securityAnswer">Answer to the user's security question.</param>
        /// <returns>true if the password is reset, otherwise false.</returns>
        public abstract bool ResetPassword(string securityAnswer);

        /// <summary>
        /// When overridden in a derived class, changes user password in the backend datastore.
        /// </summary>
        /// <param name="oldPassword">User's current password.</param>
        /// <param name="newPassword">User's new password.</param>
        /// <returns>true if the password is changed, otherwise false.</returns>
        public abstract bool ChangePassword(string oldPassword, string newPassword);

        #endregion

        /// <summary>
        /// Saves security provider settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public virtual void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings["ApplicationName", true].Update(m_applicationName);
                settings["ConnectionString", true].Update(m_connectionString);

                config.Save();
            }
        }

        /// <summary>
        /// Loads saved security provider settings from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public virtual void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("ApplicationName", m_applicationName, "Name of the application being secured as defined in the backend security data store.");
                settings.Add("ConnectionString", m_connectionString, "Connection string to be used for connection to the backend security data store.");
                ApplicationName = settings["ApplicationName"].ValueAs(m_applicationName);
                ConnectionString = settings["ConnectionString"].ValueAs(m_connectionString);
            }
        }

        /// <summary>
        /// Performs a translation of the specified user <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The user role to be translated.</param>
        /// <returns>The user role that the specified user <paramref name="role"/> translates to.</returns>
        public virtual string TranslateRole(string role)
        {
            // Most providers will not perform any kind of translation on role names.
            return role;
        }

        /// <summary>
        /// Performs a translation of the default login page to a different endpoint.
        /// </summary>
        /// <param name="loginUrl"> The URI of the login page specified in the AppSettings </param>
        /// <param name="encodedPath"> The URI requested by the client </param>
        /// <param name="referrer"> The Referrer as specified in the request header </param>
        /// <returns> The URI to be redirected to</returns>
        public virtual string TranslateRedirect(string loginUrl, System.Uri uri, string encodedPath, string referrer)
        {
            return $"{loginUrl}?redir={encodedPath}{referrer}";
        }

        /// <summary>
        /// Gets a list of Roles for this user for a specified ApplicationId.
        /// </summary>
        /// <param name="applicationId">The applicationId for the roles to be returned.</param>
        /// <returns>The roles that the specified user has.</returns>
        public virtual List<string> GetUserRoles(string applicationId)
        {
            return UserData?.Roles ?? new List<string>();
        }

        #endregion
    }
}
