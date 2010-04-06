//*******************************************************************************************************
//  SecureUser.cs - Gbtc
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Security;
using TVA.Configuration;
using TVA.Data;
using TVA.Identity;

namespace TVA.Security
{
    #region [ Enumerations ]

    /// <summary>
    /// Inidicates the type of <see cref="IPrincipal"/> and <see cref="IIdentity"/> objects to be attached to threads in the 
    /// <see cref="AppDomain.CurrentDomain">current application domain</see> for the purpose of implementing role-based security.
    /// </summary>
    public enum SecurityPrincipal
    {
        /// <summary>
        /// <see cref="SecureUserPrincipal"/> and <see cref="SecureUserIdentity"/> objects are attached to the threads.
        /// </summary>
        CustomPrincipal,
        /// <summary>
        /// <see cref="System.Security.Principal.WindowsPrincipal"/> and <see cref="System.Security.Principal.WindowsIdentity"/> objects are attached to the threads.
        /// </summary>
        WindowsPrincipal,
    }

    #endregion

    /// <summary>
    /// A class that provides a mechanism for securing applications using role-based security.
    /// </summary>
    /// <seealso cref="SecureUserIdentity"/>
    /// <seealso cref="SecureUserPrincipal"/>
    public class SecureUser : ISupportLifecycle, IPersistSettings
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ApplicationName"/> property.
        /// </summary>
        public const string DefaultApplicationName = "SecureApplication";

        /// <summary>
        /// Specifies the default value for the <see cref="ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "Primary={Server=DB1;Database=AppSec;Trusted_Connection=True};Backup={Server=DB2;Database=AppSec;Trusted_Connection=True}";

        /// <summary>
        /// Specifies the default value for the <see cref="SecurityPrincipal"/> property.
        /// </summary>
        public const SecurityPrincipal DefaultSecurityPrincipal = SecurityPrincipal.CustomPrincipal;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = true;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "ApplicationSecurity";

        /// <summary>
        /// Regular expression used for validating the passwords of external users.
        /// </summary>
        private const string StrongPasswordRegex = "^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$";

        // Fields
        private string m_applicationName;
        private string m_connectionString;
        private SecurityPrincipal m_securityPrincipal;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private string m_username;
        private string m_password;
        private string m_firstName;
        private string m_lastName;
        private string m_companyName;
        private string m_phoneNumber;
        private string m_emailAddress;
        private string m_securityQuestion;
        private string m_securityAnswer;
        private DateTime m_passwordChangeDateTime;
        private DateTime m_accountCreatedDateTime;
        private bool m_isDefined;
        private bool m_isExternal;
        private bool m_isLockedOut;
        private bool m_isAuthenticated;
        private IList<string> m_groups;
        private IList<string> m_roles;
        private WindowsPrincipal m_windowsPrincipal;
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureUser"/> class.
        /// </summary>
        public SecureUser()
            : this(Thread.CurrentPrincipal.Identity.Name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureUser"/> class.
        /// </summary>
        /// <param name="username">Name that uniquely identifies the user.</param>
        public SecureUser(string username)
        {
            // Properly format input.
            if (!string.IsNullOrEmpty(username))
            {
                if (username.Contains('\\'))
                    // Domain name is provided.
                    m_username = username;
                else
                    // Use the logged on domain.
                    m_username = string.Format("{0}\\{1}", Environment.UserDomainName, username);
            }

            // Initialize member variables.
            m_groups = new List<string>();
            m_roles = new List<string>();
            m_applicationName = DefaultApplicationName;
            m_connectionString = DefaultConnectionString;
            m_securityPrincipal = DefaultSecurityPrincipal;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="SecureUser"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~SecureUser()
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
        /// Gets or sets the <see cref="SecurityPrincipal"/> to be used for enforcing role-based security.
        /// </summary>
        public SecurityPrincipal SecurityPrincipal 
        {
            get 
            { 
                return m_securityPrincipal; 
            }
            set 
            { 
                m_securityPrincipal = value; 
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="SecureUser"/> settings are to be saved to the config file.
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
        /// Gets or sets the category under which <see cref="SecureUser"/> settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
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
        /// Gets or sets a boolean value that indicates whether the <see cref="SecureUser"/> object is currently enabled.
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
        /// Gets the user's login name.
        /// </summary>
        public string Username 
        {
            get { return m_username; }
            protected set { m_username = value;}
        }

        /// <summary>
        /// Gets the user's password.
        /// </summary>
        public string Password 
        {
            get { return m_password; }
            protected set { m_password = value; }
        }

        /// <summary>
        /// Gets the user's first name.
        /// </summary>
        public string FirstName 
        {
            get { return m_firstName; }
            protected set { m_firstName = value; }
        }

        /// <summary>
        /// Gets the user's last name.
        /// </summary>
        public string LastName 
        {
            get { return m_lastName; }
            protected set { m_lastName = value; }
        }

        /// <summary>
        /// Gets the user's company name.
        /// </summary>
        public string CompanyName 
        {
            get { return m_companyName; }
            protected set { m_companyName = value; }
        }

        /// <summary>
        /// Gets the user's phone number.
        /// </summary>
        public string PhoneNumber 
        {
            get { return m_phoneNumber; }
            protected set { m_phoneNumber = value; }
        }

        /// <summary>
        /// Gets the user's email address.
        /// </summary>
        public string EmailAddress 
        {
            get { return m_emailAddress; }
            protected set { m_emailAddress = value; }
        }

        /// <summary>
        /// Gets the user's security question.
        /// </summary>
        public string SecurityQuestion 
        { 
            get { return m_securityQuestion; }
            protected set { m_securityQuestion = value; }
        }

        /// <summary>
        /// Gets the user's security answer.
        /// </summary>
        public string SecurityAnswer 
        {
            get { return m_securityAnswer; }
            protected set { m_securityAnswer = value; }
        }

        /// <summary>
        /// Gets the date and time when user must change the password.
        /// </summary>
        public DateTime PasswordChangeDataTime 
        {
            get { return m_passwordChangeDateTime; }
            protected set { m_passwordChangeDateTime = value; }
        }

        /// <summary>
        /// Gets the date and time when user account was created.
        /// </summary>
        public DateTime AccountCreatedDateTime 
        {
            get { return m_accountCreatedDateTime; }
            protected set { m_accountCreatedDateTime = value; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not the user is defined in the backend security datastore.
        /// </summary>
        public bool IsDefined 
        {
            get { return m_isDefined; }
            protected set { m_isDefined = value; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not the user is defined as an external user in the backend security datastore.
        /// </summary>
        public bool IsExternal 
        {
            get { return m_isExternal; }
            protected set { m_isExternal = value; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not the user account has been locked due to numerous unsuccessful login attempts.
        /// </summary>
        public bool IsLockedOut 
        {
            get { return m_isLockedOut; }
            protected set { m_isLockedOut = value; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not the user has been authenticated.
        /// </summary>
        public bool IsAuthenticated 
        {
            get { return m_isAuthenticated; }
            protected set { m_isAuthenticated = value; }
        }

        /// <summary>
        /// Gets a read-only list of all the groups the user belongs to.
        /// </summary>
        public IList<string> Groups 
        {
            get { return new ReadOnlyCollection<string>(m_groups); }
            protected set { m_groups = value; }
        }

        /// <summary>
        /// Gets a read-only list of all the roles assigned to the user.
        /// </summary>
        public IList<string> Roles 
        {
            get { return new ReadOnlyCollection<string>(m_roles); }
            protected set { m_roles = value; }
        }

        /// <summary>
        /// Gets the original <see cref="WindowsPrincipal"/> of the user if the user exists in Active Directory.
        /// </summary>
        public WindowsPrincipal WindowsPrincipal 
        {
            get { return m_windowsPrincipal; }
            protected set { m_windowsPrincipal = value; }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="SecureUser"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the <see cref="SecureUser"/> object.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="SecureUser"/> 
        /// object is not consumed through the designer surface of the IDE.
        /// </remarks>
        public virtual void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();
                RefreshData();
                Authenticate(string.Empty);
                m_initialized = true; // Initialize only once.
            }
        }

        /// <summary>
        /// Saves <see cref="SecureUser"/> settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public virtual void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElement element = null;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                element = settings["ApplicationName", true];
                element.Update(m_applicationName, element.Description, element.Encrypted);
                element = settings["ConnectionString", true];
                element.Update(m_connectionString, element.Description, element.Encrypted);
                element = settings["SecurityPrincipal", true];
                element.Update(m_securityPrincipal, element.Description, element.Encrypted);

                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="SecureUser"/> settings from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public virtual void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("ApplicationName", m_applicationName, "Name of the application being secured as defined in the backend security datastore.");
                settings.Add("ConnectionString", m_connectionString, "Connection string to be used for connection to the backend security datastore.");
                settings.Add("SecurityPrincipal", m_securityPrincipal, "Principal (CustomPrincipal; WindowsPrincipal) to be used for enforcing role-based security.");
                ApplicationName = settings["ApplicationName"].ValueAs(m_applicationName);
                ConnectionString = settings["ConnectionString"].ValueAs(m_connectionString);
                SecurityPrincipal = settings["SecurityPrincipal"].ValueAs(m_securityPrincipal);
            }
        }

        /// <summary>
        /// Refreshes user data.
        /// </summary>
        /// <returns>true if user data is refreshed, otherwise false.</returns>
        public virtual bool RefreshData()
        {
            // TODO: Move this to external docs
            // Source of user data will depend on the specified SecurityPolicy.
            // - CustomPrincipal: 
            //   1) User must exist in the security database. 
            //   2) Internal user's data will be retrieved from AD and external user's data will be retrieved from database.
            // - WindowsPrincipal: 
            //   1) Check for user's existance in security database is not performed.
            //   2) Internal user's data will be retrieved from AD and external user will be treated as anonymous user.

            // Initialize data.
            m_password = string.Empty;
            m_firstName = string.Empty;
            m_lastName = string.Empty;
            m_companyName = string.Empty;
            m_phoneNumber = string.Empty;
            m_emailAddress = string.Empty;
            m_securityQuestion = string.Empty;
            m_securityAnswer = string.Empty;
            m_passwordChangeDateTime = DateTime.MinValue;
            m_accountCreatedDateTime = DateTime.MinValue;
            m_isDefined = false;
            m_isExternal = false;
            m_isLockedOut = false;
            m_groups.Clear();
            m_roles.Clear();

            bool refreshFromAD = PopulateDataFromActiveDirectory();
            if (m_securityPrincipal == SecurityPrincipal.WindowsPrincipal)
                return refreshFromAD;
            else
                return PopulateDataFromBackendDatabase();
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="password">Password to be used for authentication.</param>
        /// <returns>true if the user is authenticated, otherwise false.</returns>
        public virtual bool Authenticate(string password)
        {
            m_isAuthenticated = false;
            if (m_securityPrincipal == SecurityPrincipal.WindowsPrincipal ||
                (m_securityPrincipal == SecurityPrincipal.CustomPrincipal && m_isDefined && !m_isLockedOut && !m_isExternal))
            {
                // Authenticate against active directory.
                if (!string.IsNullOrEmpty(password))
                {
                    // Validate by performing network logon.
                    string[] userParts = m_username.Split('\\');
                    m_windowsPrincipal = UserInfo.AuthenticateUser(userParts[0], userParts[1], password) as WindowsPrincipal;
                    m_isAuthenticated = m_windowsPrincipal != null && m_windowsPrincipal.Identity.IsAuthenticated;
                }
                else
                {
                    // Validate with current thread principal.
                    m_windowsPrincipal = Thread.CurrentPrincipal as WindowsPrincipal;
                    m_isAuthenticated = m_windowsPrincipal != null && !string.IsNullOrEmpty(m_username) && 
                                        string.Compare(m_windowsPrincipal.Identity.Name, m_username) == 0 && m_windowsPrincipal.Identity.IsAuthenticated;
                }
            }
            else if(m_securityPrincipal == SecurityPrincipal.CustomPrincipal && m_isDefined && !m_isLockedOut && m_isExternal)
            { 
                // Authenticate against backend database.
                m_isAuthenticated = m_password == EncryptPassword(password);
            }

            return m_isAuthenticated;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SecureUser"/> object and optionally releases the managed resources.
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

        /// <summary>
        /// Retrieves user data from Active Directory.
        /// </summary>
        /// <returns>true if user data is retrieved, otherwise false.</returns>
        protected virtual bool PopulateDataFromActiveDirectory()
        {
            if (string.IsNullOrEmpty(m_username))
                return false;

            using (UserInfo adUserInfo = new UserInfo(m_username))
            {
                adUserInfo.Initialize();
                if (adUserInfo.UserEntry != null)
                {
                    // User exists in Active Directory.
                    m_firstName = adUserInfo.FirstName;
                    m_lastName = adUserInfo.LastName;
                    m_companyName = adUserInfo.Company;
                    m_phoneNumber = adUserInfo.Telephone;
                    m_emailAddress = adUserInfo.Email;

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Retrieves user data from backend security datastore.
        /// </summary>
        /// <returns>true if user data is retrieved, otherwise false.</returns>
        protected virtual bool PopulateDataFromBackendDatabase()
        {
            if (string.IsNullOrEmpty(m_username))
                return false;

            DataSet userData;
            DataRow userDataRow;
            using (SqlConnection dbConnection = GetDatabaseConnection())
            {
                if (dbConnection == null)
                    return false;

                // We'll retrieve all the data we need in a single trip to the database by calling the stored
                // procedure 'RetrieveApiData' that will return 3 tables to us:
                // Table1 (Index 0): Information about the user.
                // Table2 (Index 1): Groups the user is a member of.
                // Table3 (Index 2): Roles that are assigned to the user either directly or through a group.
                // TODO: Remove split on m_username after database change
                userData = dbConnection.RetrieveDataSet("dbo.RetrieveApiData", m_username.Split('\\')[1], m_applicationName);

                if (userData.Tables[0].Rows.Count == 0)
                    return false;

                userDataRow = userData.Tables[0].Rows[0];
                m_isDefined = true;
                if (!Convert.IsDBNull(userDataRow["UserPasswordChangeDateTime"]))
                    m_passwordChangeDateTime = Convert.ToDateTime(userDataRow["UserPasswordChangeDateTime"]);
                if (!Convert.IsDBNull(userDataRow["UserAccountCreatedDateTime"]))
                    m_accountCreatedDateTime = Convert.ToDateTime(userDataRow["UserAccountCreatedDateTime"]);
                if (!Convert.IsDBNull(userDataRow["UserIsExternal"]))
                    m_isExternal = Convert.ToBoolean(userDataRow["UserIsExternal"]);
                if (!Convert.IsDBNull(userDataRow["UserIsLockedOut"]))
                    m_isLockedOut = Convert.ToBoolean(userDataRow["UserIsLockedOut"]);

                foreach (DataRow group in userData.Tables[1].Rows)
                {
                    if (!Convert.IsDBNull(group["GroupName"]))
                        m_groups.Add(Convert.ToString(group["GroupName"]));
                }

                foreach (DataRow role in userData.Tables[2].Rows)
                {
                    if (!Convert.IsDBNull(role["RoleName"]))
                        m_roles.Add(Convert.ToString(role["RoleName"]));
                }

                if (m_isExternal)
                {
                    if (!Convert.IsDBNull(userDataRow["UserPassword"]))
                        m_password = Convert.ToString(userDataRow["UserPassword"]);
                    if (!Convert.IsDBNull(userDataRow["UserFirstName"]))
                        m_firstName = Convert.ToString(userDataRow["UserFirstName"]);
                    if (!Convert.IsDBNull(userDataRow["UserLastName"]))
                        m_lastName = Convert.ToString(userDataRow["UserLastName"]);
                    if (!Convert.IsDBNull(userDataRow["UserCompanyName"]))
                        m_companyName = Convert.ToString(userDataRow["UserCompanyName"]);
                    if (!Convert.IsDBNull(userDataRow["UserPhoneNumber"]))
                        m_phoneNumber = Convert.ToString(userDataRow["UserPhoneNumber"]);
                    if (!Convert.IsDBNull(userDataRow["UserEmailAddress"]))
                        m_emailAddress = Convert.ToString(userDataRow["UserEmailAddress"]);
                    if (!Convert.IsDBNull(userDataRow["UserSecurityQuestion"]))
                        m_securityQuestion = Convert.ToString(userDataRow["UserSecurityQuestion"]);
                    if (!Convert.IsDBNull(userDataRow["UserSecurityAnswer"]))
                        m_securityAnswer = Convert.ToString(userDataRow["UserSecurityAnswer"]);
                }

                return true;
            }
        }

        private SqlConnection GetDatabaseConnection()
        {
            SqlConnection connection = null;
            foreach (KeyValuePair<string, string> pair in m_connectionString.ParseKeyValuePairs())
            {
                try
                {
                    // Initialize database connection.
                    connection = new SqlConnection(pair.Value);
                    connection.Open();
                }
                catch (SqlException)
                {
                    // Try other connection strings.
                }
                catch
                {
                    // Bubble-up encountered exception.
                    throw;
                }
            }

            return connection;
        }

        #endregion

        #region [ Static ]

        // Static Properties
        
        /// <summary>
        /// Gets or sets the <see cref="SecureUser"/> object of the current user.
        /// </summary>
        public static SecureUser Current
        {
            get
            {
                SecureUserPrincipal principal = Thread.CurrentPrincipal as SecureUserPrincipal;
                if (principal != null)
                {
                    // Retrieve user from current thread principal.
                    return ((SecureUserIdentity)principal.Identity).Data;
                }
                else
                {
                    // Retrieve user from session data if available.
                    if (Common.GetApplicationType() == ApplicationType.Web && 
                        HttpContext.Current != null && HttpContext.Current.Session != null)
                    {
                        SecureUser user = HttpContext.Current.Session[typeof(SecureUser).Name] as SecureUser;
                        if (user != null && !(Thread.CurrentPrincipal is SecureUserPrincipal))
                            Current = user;

                        return user;
                    }
                    else
                    {
                        return null;
                    }
                }
                //if (Common.GetApplicationType() == ApplicationType.Web)
                //{
                //    // Retrieve user cached in the session data.
                //    SecureUser user = HttpContext.Current.Session[typeof(SecureUser).Name] as SecureUser;
                //    if (user != null && !(Thread.CurrentPrincipal is SecureUserPrincipal))
                //        Current = user;

                //    return user;
                //}
                //else
                //{
                //    // Retrieve user from current thread principal.
                //    SecureUserPrincipal principal = Thread.CurrentPrincipal as SecureUserPrincipal;
                //    if (principal == null)
                //        return null;
                //    else
                //        return ((SecureUserIdentity)principal.Identity).Data;
                //}
            }
            set
            {
                if (value != null)
                {
                    // Login - Setup security principal.
                    value.Initialize();
                    SecureUserIdentity identity = new SecureUserIdentity(value);
                    SecureUserPrincipal principal = new SecureUserPrincipal(identity);
                    Thread.CurrentPrincipal = principal;
                    if (Common.GetApplicationType() == ApplicationType.Web && HttpContext.Current != null)
                    {
                        HttpContext.Current.User = principal;
                        if (HttpContext.Current.Session != null)
                            HttpContext.Current.Session[typeof(SecureUser).Name] = value;
                    }
                }
                else
                {
                    // Logout - Restore original principal.
                    SecureUserPrincipal principal = Thread.CurrentPrincipal as SecureUserPrincipal;
                    if (principal == null)
                        return;

                    SecureUserIdentity identity = principal.Identity as SecureUserIdentity;
                    if (identity.Data.WindowsPrincipal != null)
                        Thread.CurrentPrincipal = identity.Data.WindowsPrincipal;
                    else
                        Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetAnonymous());

                    if (Common.GetApplicationType() == ApplicationType.Web && HttpContext.Current != null)
                    {
                        HttpContext.Current.User = Thread.CurrentPrincipal;
                        if (HttpContext.Current.Session != null)
                            HttpContext.Current.Session[typeof(SecureUser).Name] = null;
                    }
                }
            }
        }

        // Static Methods

        /// <summary>
        /// Encrypts the password to a one-way hash using the SHA1 hash algorithm.
        /// </summary>
        /// <param name="password">Password to be encrypted.</param>
        /// <returns>Encrypted password.</returns>
        public static string EncryptPassword(string password)
        {

            if (Regex.IsMatch(password, StrongPasswordRegex))
            {
                // We prepend salt text to the password and then has it to make it even more secure.
                return FormsAuthentication.HashPasswordForStoringInConfigFile("O3990\\P78f9E66b:a35_VÂ©6M13Â©6~2&[" + password, "SHA1");
            }
            else
            {
                // Password does not meet the strong password rule defined below, so we don't encrypt the password.
                StringBuilder message = new StringBuilder();
                message.Append("Password does not meet the following criteria:");
                message.AppendLine();
                message.Append("- Password must be at least 8 characters");
                message.AppendLine();
                message.Append("- Password must contain at least 1 digit");
                message.AppendLine();
                message.Append("- Password must contain at least 1 upper case letter");
                message.AppendLine();
                message.Append("- Password must contain at least 1 lower case letter");

                throw new SecurityException(message.ToString());
            }
        }

        #endregion
    }
}
