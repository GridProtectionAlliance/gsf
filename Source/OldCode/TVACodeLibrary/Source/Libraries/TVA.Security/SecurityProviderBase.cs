//*******************************************************************************************************
//  SecurityProviderBase.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
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
//       Fixed a bug in the caching mechanism employed in Current static property.
//  12/03/2010 - Pinal C. Patel
//       Added TranslateRole() to allow providers to perform translation on role name.
//  01/05/2011 - Pinal C. Patel
//       Added CanRefreshData, CanUpdateData, CanResetPassword and CanChangePassword properties along 
//       with accompanying RefreshData(), UpdateData(), ResetPassword() and ChangePassword() methods.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using TVA.Configuration;

namespace TVA.Security
{
    /// <summary>
    /// Base class for a provider of role-based security in applications.
    /// </summary>
    /// <example>
    /// This examples shows how to extend <see cref="SecurityProviderBase"/> to use a flat-file for the security datastore:
    /// <code>
    /// using System.Data;
    /// using System.IO;
    /// using TVA;
    /// using TVA.Data;
    /// using TVA.IO;
    /// using TVA.Security;
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
    ///     <section name="categorizedSettings" type="TVA.Configuration.CategorizedSettingsSection, TVA.Core" />
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
        private bool m_persistSettings;
        private string m_settingsCategory;
        private UserData m_userData;
        private bool m_canRefreshData;
        private bool m_canUpdateData;
        private bool m_canResetPassword;
        private bool m_canChangePassword;
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the security provider.
        /// </summary>
        /// <param name="username">Name that uniquely identifies the user.</param>
        /// <param name="canRefreshData">true if the security provider can refresh <see cref="UserData"/> from the backend datastore, otherwise false.</param>
        /// <param name="canUpdateData">true if the security provider can update <see cref="UserData"/> in the backend datastore, otherwise false.</param>
        /// <param name="canResetPassword">true if the security provider can reset user password, otherwise false.</param>
        /// <param name="canChangePassword">true if the security provider can change user password, otherwise false.</param>
        protected SecurityProviderBase(string username, bool canRefreshData, bool canUpdateData, bool canResetPassword, bool canChangePassword)
        {
            // Verify specified username.
            if (string.IsNullOrEmpty(username))
                username = Thread.CurrentPrincipal.Identity.Name;

            // Remove domain from username.
            if (username.Contains('\\'))
                username = username.Split('\\')[1];

            // Initialize member variables.
            m_userData = new UserData(username);
            m_canRefreshData = canRefreshData;
            m_canUpdateData = canUpdateData;
            m_canResetPassword = canResetPassword;
            m_canChangePassword = canChangePassword;
            m_applicationName = DefaultApplicationName;
            m_connectionString = DefaultConnectionString;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
        }

        /// <summary>
        /// Releases the unmanaged resources before the security provider is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~SecurityProviderBase()
        {
            Dispose(false);
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
                    throw new ArgumentNullException("value");

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the security provider is currently enabled.
        /// </summary>
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
        /// Gets the <see cref="UserData"/> object containing information about the user.
        /// </summary>
        public virtual UserData UserData
        {
            get
            {
                return m_userData;
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
        /// Geta a boolean value that indicates whether <see cref="UpdateData"/> operation is supported.
        /// </summary>
        public virtual bool CanUpdateData 
        {
            get
            {
                return m_canUpdateData;
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

        #endregion

        #region [ Methods ]

        #region [ Abstract ]

        /// <summary>
        /// When overridden in a derived class, authenticates the user.
        /// </summary>
        /// <param name="password">Password to be used for authentication.</param>
        /// <returns>true if the user is authenticated, otherwise false.</returns>
        public abstract bool Authenticate(string password);

        /// <summary>
        /// When overridden in a derived class, refreshes the <see cref="UserData"/> from the backend datastore.
        /// </summary>
        /// <returns>true if <see cref="UserData"/> is refreshed, otherwise false.</returns>
        public abstract bool RefreshData();

        /// <summary>
        /// When overridden in a derived class, updates the <see cref="UserData"/> in the backend datastore.
        /// </summary>
        /// <returns>true if <see cref="UserData"/> is updated, otherwise false.</returns>
        public abstract bool UpdateData();

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
        /// Releases all the resources used by the security provider.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the security provider.
        /// </summary>
        public virtual void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();             // Load settings from config file.
                if (CanRefreshData)
                    RefreshData();          // Load user data from datastore.
                Authenticate(string.Empty); // Authenticate user credentials.
                m_initialized = true;       // Initialize only once.
            }
        }

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
                settings.Add("ApplicationName", m_applicationName, "Name of the application being secured as defined in the backend security datastore.");
                settings.Add("ConnectionString", m_connectionString, "Connection string to be used for connection to the backend security datastore.");
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
        /// Releases the unmanaged resources used by the security provider and optionally releases the managed resources.
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
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        #endregion
    }
}
