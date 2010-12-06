//*******************************************************************************************************
//  LdapSecurityProvider.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/08/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/03/2010 - Pinal C. Patel
//       Override the default behavior of TranslateRole() to translate a SID to its role name.
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
using System.DirectoryServices;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using TVA.Identity;

namespace TVA.Security
{
    /// <summary>
    /// Represents an <see cref="ISecurityProvider"/> that uses Active Directory for its backend datastore and credential authentication.
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
    ///     <section name="categorizedSettings" type="TVA.Configuration.CategorizedSettingsSection, TVA.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <securityProvider>
    ///       <add name="ApplicationName" value="" description="Name of the application being secured as defined in the backend security datastore."
    ///         encrypted="false" />
    ///       <add name="ConnectionString" value="LDAP://DC=COMPANY,DC=COM"
    ///         description="Connection string to be used for connection to the backend security datastore."
    ///         encrypted="false" />
    ///       <add name="ProviderType" value="TVA.Security.LdapSecurityProvider, TVA.Security"
    ///         description="The type to be used for enforcing security." encrypted="false" />
    ///       <add name="IncludedResources" value="*=*" description="Semicolon delimited list of resources to be secured along with role names."
    ///         encrypted="false" />
    ///       <add name="ExcludedResources" value="" description="Semicolon delimited list of resources to be excluded from being secured."
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
        private const int ACCOUNTDISABLED = 2;
        private const int LOCKED = 16;
        private const int PASSWD_CANT_CHANGE = 64;
        private const int DONT_EXPIRE_PASSWORD = 65536;

        // Fields
        private WindowsPrincipal m_windowsPrincipal;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapSecurityProvider"/> class.
        /// </summary>
        /// <param name="username"></param>
        public LdapSecurityProvider(string username)
            : base(username)
        {
        }

        #endregion

        #region [ Properties ]

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

        /// <summary>
        /// Refreshes the <see cref="UserData"/>.
        /// </summary>
        /// <returns>true if <see cref="SecurityProviderBase.UserData"/> is refreshed, otherwise false.</returns>
        public override bool RefreshData()
        {
            if (string.IsNullOrEmpty(UserData.Username))
                return false;

            // Initialize user data.
            UserData.Initialize();

            // Populate user data.
            UserInfo user = null;
            DirectorySearcher searcher = null;
            WindowsImpersonationContext context = null;
            try
            {
                if (!ConnectionString.StartsWith("LDAP://", StringComparison.CurrentCultureIgnoreCase) ||
                    !ConnectionString.StartsWith("LDAPS://", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Use default LDAP path.
                    user = new UserInfo(UserData.Username);
                    searcher = new DirectorySearcher();
                }
                else
                {
                    // Use specified LDAP path.
                    user = new UserInfo(UserData.Username, ConnectionString);
                    searcher = new DirectorySearcher(new DirectoryEntry(ConnectionString));
                }

                user.PersistSettings = true;
                user.Initialize();
                if (user.UserEntry != null)
                {
                    // User exists in Active Directory.
                    UserData.IsDefined = true;
                    UserData.LoginID = user.LoginID;
                    UserData.FirstName = user.FirstName;
                    UserData.LastName = user.LastName;
                    UserData.CompanyName = user.Company;
                    UserData.PhoneNumber = user.Telephone;
                    UserData.EmailAddress = user.Email;

                    // Retrieve non-standard user information.
                    context = user.ImpersonatePrivilegedAccount();
                    UserData.AccountCreatedDateTime = Convert.ToDateTime(user.UserEntry.Properties["whenCreated"].Value);
                    int userAccountControl = Convert.ToInt32(user.UserEntry.Properties["userAccountControl"].Value);
                    if (userAccountControl > 0)
                    {
                        // Check if account is locked.
                        if (Convert.ToBoolean(userAccountControl & LOCKED))
                            UserData.IsLockedOut = true;

                        // Check if account is disabled.
                        if (Convert.ToBoolean(userAccountControl & ACCOUNTDISABLED))
                            UserData.IsDisabled = true;

                        // Determine date when user must change the password.
                        UserData.PasswordChangeDateTime = DateTime.MaxValue;
                        if (!Convert.ToBoolean(userAccountControl & PASSWD_CANT_CHANGE) &&
                            !Convert.ToBoolean(userAccountControl & DONT_EXPIRE_PASSWORD))
                        {
                            long passwordSetOn = ConvertToLong(user.UserEntry.Properties["pwdLastSet"].Value);
                            if (passwordSetOn == 0)
                            {
                                // User must change password on next logon.
                                UserData.PasswordChangeDateTime = DateTime.MinValue;
                            }
                            else
                            {
                                // User must change password periodically.
                                SearchResult searchResult = searcher.FindOne();
                                if (searchResult != null && searchResult.Properties.Contains("maxPwdAge"))
                                    UserData.PasswordChangeDateTime = DateTime.FromFileTime(passwordSetOn).AddDays(TimeSpan.FromTicks((long)searchResult.Properties["maxPwdAge"][0]).Duration().Days);
                            }
                        }
                    }

                    // Retrieve all groups the user is a member of.
                    string groupName;
                    user.UserEntry.RefreshCache(new string[] { "TokenGroups" });
                    foreach (byte[] sid in user.UserEntry.Properties["TokenGroups"])
                    {
                        groupName = new SecurityIdentifier(sid, 0).Translate(typeof(NTAccount)).ToString().Split('\\')[1];
                        if (!UserData.Roles.Contains(groupName))
                            UserData.Roles.Add(groupName);
                    }

                    //// Retrieve all groups the user is a member of.
                    //int startIndex, substringLength;
                    //foreach (string role in user.UserEntry.Properties["memberOf"])
                    //{
                    //    startIndex = role.IndexOf("CN=", StringComparison.CurrentCultureIgnoreCase);
                    //    if (startIndex >= 0)
                    //    {
                    //        startIndex += 3;
                    //        substringLength = role.IndexOf(',');
                    //        if (substringLength >= 0)
                    //            substringLength -= startIndex;
                    //        else
                    //            substringLength = role.Length;

                    //        UserData.Groups.Add(role.Substring(startIndex, substringLength));
                    //    }
                    //}

                    return true;
                }
                else
                {
                    // No such user in Active Directory.
                    return false;
                }
            }
            finally
            {
                if (user != null)
                    user.Dispose();

                if (searcher != null)
                    searcher.Dispose();

                if (context != null)
                    context.Undo();
            }
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="password">Password to be used for authentication.</param>
        /// <returns>true if the user is authenticated, otherwise false.</returns>
        public override bool Authenticate(string password)
        {
            if (!UserData.IsDefined)
                return false;

            if (string.IsNullOrEmpty(password))
            {
                // Validate with current thread principal.
                m_windowsPrincipal = Thread.CurrentPrincipal as WindowsPrincipal;
                UserData.IsAuthenticated = m_windowsPrincipal != null && !string.IsNullOrEmpty(UserData.LoginID) &&
                                                string.Compare(m_windowsPrincipal.Identity.Name, UserData.LoginID, true) == 0 && m_windowsPrincipal.Identity.IsAuthenticated;
            }
            else
            {
                // Validate by performing network logon.
                string[] userParts = UserData.LoginID.Split('\\');
                m_windowsPrincipal = UserInfo.AuthenticateUser(userParts[0], userParts[1], password) as WindowsPrincipal;
                UserData.IsAuthenticated = m_windowsPrincipal != null && m_windowsPrincipal.Identity.IsAuthenticated;
            }

            return UserData.IsAuthenticated;
        }

        /// <summary>
        /// Performs a translation of the specified user <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The user role to be translated.</param>
        /// <returns>The user role that the specified user <paramref name="role"/> translates to.</returns>
        public override string TranslateRole(string role)
        {
            // Perform a translation from SID to Role only if the input starts with 'SID:'.
            if (role.StartsWith("SID:", StringComparison.CurrentCultureIgnoreCase))
                return new SecurityIdentifier(role.Remove(0, 4)).Translate(typeof(NTAccount)).ToString().Split('\\')[1];

            return role;
        }

        private long ConvertToLong(object largeInteger)
        {
            Type type = largeInteger.GetType();
            int highPart = (int)type.InvokeMember("HighPart", BindingFlags.GetProperty, null, largeInteger, null);
            int lowPart = (int)type.InvokeMember("LowPart", BindingFlags.GetProperty, null, largeInteger, null);

            return (long)highPart << 32 | (uint)lowPart;
        }

        #endregion
    }
}
