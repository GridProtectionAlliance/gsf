//*******************************************************************************************************
//  UserInfo.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/10/2004 - J. Ritchie Carroll
//       Original version of source code generated.
//  01/03/2006 - Pinal C. Patel
//       2.0 version of source code migrated from 1.1 source (TVA.Shared.Identity).
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
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using TVA.Configuration;
using TVA.Interop;

namespace TVA.Identity
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
    /// using TVA.Identity;
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
    /// </example>
    public class UserInfo : ISupportLifecycle, IPersistSettings
    {
        #region [ Members ]

        // Constants
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int SECURITY_IMPERSONATION = 2;

        // Fields
        private string m_domain;
        private string m_username;
        private DirectoryEntry m_userEntry;
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
        /// <param name="domain">Domain where the user's account exists.</param>
        /// <param name="username">Username of user's account whose information is to be retrieved.</param>
        public UserInfo(string domain, string username)
            : this()
        {
            m_domain = domain;
            m_username = username;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfo"/> class.
        /// </summary>
        /// <param name="loginID">Login ID in "domain\username" format of the user's account whose information is to be retrieved.</param>
        public UserInfo(string loginID)
            : this()
        {
            string[] parts = loginID.Split('\\');

            if (parts.Length != 2)
                throw new ArgumentException("Expected login ID in format of 'domain\\username'.");

            if (string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
                throw new ArgumentException("Expected login ID in format of 'domain\\username'.");

            m_domain = parts[0];
            m_username = parts[1];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfo"/> class.
        /// </summary>
        private UserInfo()
        {
            m_settingsCategory = this.GetType().Name;
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
        /// <see cref="Enabled"/> property is not be set by user-code directly.
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
                    throw (new ArgumentNullException("value"));

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
        /// Gets the First Name of the user.
        /// </summary>
        /// <remarks>Returns the value retrieved for the "givenName" active directory property.</remarks>
        public string FirstName
        {
            get
            {
                return GetUserProperty("givenName");
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
                return GetUserProperty("sn");
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
                return GetUserProperty("displayName");
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
                return GetUserProperty("initials");
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
                    return m_domain + "\\" + m_username;
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
                return GetUserProperty("mail");
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
                return GetUserProperty("wWWHomePage");
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
                return GetUserProperty("description");
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
                return GetUserProperty("telephoneNumber");
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
                return GetUserProperty("title");
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
                return GetUserProperty("company");
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
                return GetUserProperty("physicalDeliveryOfficeName");
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
                return GetUserProperty("department");
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
                return GetUserProperty("l");
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
                return GetUserProperty("streetAddress");
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
        /// Initializes the <see cref="UserInfo"/> object.
        /// </summary>
        public void Initialize()
        {
            if (!m_initialized)
            {
                // Load settings from config file.
                LoadSettings();

                // Initialize the directory entry object used to retrieve AD info.
                WindowsImpersonationContext currentContext = null;
                try
                {
                    // 11/06/2007 - PCP: Some change in the AD now causes the searching the AD to fail also if
                    // this code is not being executed under a domain account which was not the case before;
                    // before only AD property lookup had this behavior.

                    // Impersonate to the privileged account if specified.
                    if (!string.IsNullOrEmpty(m_privilegedDomain) &&
                        !string.IsNullOrEmpty(m_privilegedUserName) &&
                        !string.IsNullOrEmpty(m_privilegedPassword))
                    {
                        currentContext = ImpersonateUser(m_privilegedDomain,
                                                         m_privilegedUserName,
                                                         m_privilegedPassword);
                    }

                    // 02/27/2007 - PCP: Using the default directory entry instead of specifying the domain name.
                    // This is done to overcome "The server is not operational" COM exception that was being
                    // encountered when a domain name was being specified.
                    DirectoryEntry entry = new DirectoryEntry("LDAP://" + m_domain);

                    using (DirectorySearcher searcher = new DirectorySearcher(entry))
                    {
                        searcher.Filter = "(SAMAccountName=" + m_username + ")";
                        SearchResult result = searcher.FindOne();
                        if (result != null)
                            m_userEntry = result.GetDirectoryEntry();
                    }
                }
                catch
                {
                    m_userEntry = null;
                    throw;
                }
                finally
                {
                    EndImpersonation(currentContext);   // Undo impersonation if it was performed.
                }

                m_enabled = true;       // Mark as enabled.
                m_initialized = true;   // Initialize only once.
            }
        }

        /// <summary>
        /// Saves settings for the <see cref="UserInfo"/> object to the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElement element = null;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                element = settings["PrivilegedDomain", true];
                element.Update(m_privilegedDomain, element.Description, element.Encrypted);
                element = settings["PrivilegedUserName", true];
                element.Update(m_privilegedUserName, element.Description, element.Encrypted);
                element = settings["PrivilegedPassword", true];
                element.Update(m_privilegedPassword, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="UserInfo"/> object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("PrivilegedDomain", m_privilegedDomain, "Domain of privileged domain user account.");
                settings.Add("PrivilegedUserName", m_privilegedUserName, "Username of privileged domain user account.");
                settings.Add("PrivilegedPassword", m_privilegedPassword, "Password of privileged domain user account.", true);
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
        /// using TVA.Identity;
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
        /// Returns the value for specified active directory property.
        /// </summary>
        /// <param name="propertyName">Name of the active directory property whose value is to be retrieved.</param>
        /// <returns>Value for the specified active directory property.</returns>
        public string GetUserProperty(string propertyName)
        {
            WindowsImpersonationContext currentContext = null;
            try
            {
                // Allow lookup to logged-on domain only to prevent timeouts.
                if (string.Compare(m_domain, Environment.UserDomainName, true) != 0 ||
                    string.Compare(Environment.MachineName, Environment.UserDomainName, true) == 0)
                    return string.Empty;

                // Initialize if uninitialized.
                Initialize();

                // Quit if disabled.
                if (!m_enabled)
                    return string.Empty;

                // Impersonate to the privileged account if specified.
                if (!string.IsNullOrEmpty(m_privilegedDomain) &&
                    !string.IsNullOrEmpty(m_privilegedUserName) &&
                    !string.IsNullOrEmpty(m_privilegedPassword))
                {
                    currentContext = ImpersonateUser(m_privilegedDomain,
                                                     m_privilegedUserName,
                                                     m_privilegedPassword);
                }

                if (m_userEntry == null)
                    return string.Empty;
                else
                    return m_userEntry.Properties[propertyName][0].ToString().Replace("  ", " ").Trim();
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                EndImpersonation(currentContext);   // Undo impersonation if it was performed.
            }
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

                        if (m_userEntry != null)
                            m_userEntry.Dispose();
                    }
                }
                finally
                {
                    m_enabled = false;  // Mark as disabled.
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static UserInfo m_currentUserInfo;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DuplicateToken(IntPtr ExistingTokenHandle, int SecurityImpersonationLevel, ref IntPtr DuplicateTokenHandle);

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
                if (m_currentUserInfo == null)
                    m_currentUserInfo = new UserInfo(CurrentUserID);

                return m_currentUserInfo;
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
                if (userID == null)
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
        /// <example>
        /// This example shows how to validate a user's credentials:
        /// <code>
        /// using System;
        /// using TVA.Identity;
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
        ///         if (UserInfo.AuthenticateUser(domain, username, password))
        ///             Console.WriteLine("Successfully authenticated user \"{0}\\{1}\".", domain, username);
        ///         else
        ///             Console.WriteLine("Failed to authenticate user \"{0}\\{1}\".", domain, username);
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static bool AuthenticateUser(string domain, string username, string password)
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
        /// <example>
        /// This example shows how to validate a user's credentials and retrieve an error message if validation fails: 
        /// <code>
        /// using System;
        /// using TVA.Identity;
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
        ///         if (UserInfo.AuthenticateUser(domain, username, password, out errorMessage))
        ///             Console.WriteLine("Successfully authenticated user \"{0}\\{1}\".", domain, username);
        ///         else
        ///             Console.WriteLine("Failed to authenticate user \"{0}\\{1}\" due to exception: {2}", domain, username, errorMessage);
        ///
        ///         Console.ReadLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static bool AuthenticateUser(string domain, string username, string password, out string errorMessage)
        {
            IntPtr tokenHandle = IntPtr.Zero;
            bool authenticated;

            try
            {
                errorMessage = null;

                // Call LogonUser to attempt authentication
                authenticated = LogonUser(username, domain, password, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT, out tokenHandle);

                if (!authenticated)
                    errorMessage = WindowsApi.GetLastErrorMessage();
            }
            finally
            {
                // Free the token
                if (tokenHandle != IntPtr.Zero)
                    CloseHandle(tokenHandle);
            }

            return authenticated;
        }

        /// <summary>
        /// Impersonates the specified user.
        /// </summary>
        /// <param name="domain">Domain of user to impersonate.</param>
        /// <param name="username">Username of user to impersonate.</param>
        /// <param name="password">Password of user to impersonate.</param>
        /// <returns>A <see cref="WindowsImpersonationContext"/> object of the impersonated user.</returns>
        /// <remarks>After impersonating a user the code executes under the impersonated user's identity.</remarks>
        /// <example>
        /// This example shows how to impersonate a user:
        /// <code>
        /// using System;
        /// using TVA.Identity;
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
            WindowsImpersonationContext impersonatedUser;
            IntPtr tokenHandle = IntPtr.Zero;
            IntPtr dupeTokenHandle = IntPtr.Zero;

            try
            {
                // Calls LogonUser to obtain a handle to an access token.
                if (!LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out tokenHandle))
                    throw new InvalidOperationException(string.Format("Failed to impersonate user \"{0}\\{1}\". {2}", domain, username, WindowsApi.GetLastErrorMessage()));

                if (!DuplicateToken(tokenHandle, SECURITY_IMPERSONATION, ref dupeTokenHandle))
                {
                    CloseHandle(tokenHandle);
                    throw new InvalidOperationException(string.Format("Failed to impersonate user \"{0}\\{1}\". Exception thrown while trying to duplicate token.", domain, username));
                }

                // The token that is passed into WindowsIdentity must be a primary token in order to use it for impersonation.
                impersonatedUser = WindowsIdentity.Impersonate(dupeTokenHandle);
            }
            finally
            {
                // Frees the tokens.
                if (tokenHandle != IntPtr.Zero)
                    CloseHandle(tokenHandle);

                if (dupeTokenHandle != IntPtr.Zero)
                    CloseHandle(dupeTokenHandle);
            }

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
        /// using TVA.Identity;
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
            if (impersonatedUser != null)
            {
                impersonatedUser.Undo();
                impersonatedUser.Dispose();
            }

            impersonatedUser = null;
        }

        #endregion
    }
}