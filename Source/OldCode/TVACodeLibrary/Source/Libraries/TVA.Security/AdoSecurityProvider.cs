//******************************************************************************************************
//  AdoSecurityProvider.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/24/2010 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

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

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;
using TVA.Configuration;
using TVA.Data;

namespace TVA.Security
{
    /// <summary>
    /// Represents an <see cref="ISecurityProvider"/> that uses ADO.NET data source (SQL Server, MySQL, Microsoft Access etc) for its
    /// backend datastore and authenticates internal users against Active Directory and external users against the database.
    /// </summary>
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
    ///       <add name="ConnectionString" value="Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\ProgramData\openPDC\openPDC1.mdb" 
    ///         description="Configuration database connection string" encrypted="false"/>
    ///       <add name="DataProviderString" value="AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.OleDb.OleDbConnection;AdapterType=System.Data.OleDb.OleDbDataAdapter" 
    ///         description="Configuration database ADO.NET data provider assembly type creation string" encrypted="false"/>    
    ///       <add name="ApplicationName" value="SEC_APP" description="Name of the application being secured." encrypted="false" />    
    ///       <add name="ProviderType" value="TVA.Security.AdoSecurityProvider, TVA.Security"
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
    public class AdoSecurityProvider : LdapSecurityProvider
    {
        #region [ Members ]

        // Constants
        private const int MinimumPasswordLength = 8;
        private const string PasswordRequirementRegex = "^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).*$";
        private const string PasswordRequirementError = "Invalid Password: Password must be at least 8 characters; must contain at least 1 number, 1 upper case letter, and 1 lower case letter";

        /// <summary>
        /// Defines the provider ID for the <see cref="AdoSecurityProvider"/>.
        /// </summary>
        public new const int ProviderID = 1;

        // Nested Types

        /// <summary>
        /// Creates a new <see cref="IDbConnection"/> to configured ADO.NET data source.
        /// </summary>
        private class DataConnection : IDisposable
        {
            #region [ Members ]

            //Fields
            private IDbConnection m_connection;
            private bool m_disposed;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Creates a new <see cref="DataConnection"/>.
            /// </summary>
            /// <param name="settingsCategory">Settings category to use for connection settings.</param>
            public DataConnection(string settingsCategory)
            {
                // Only need to establish data types and load settings once
                if (s_connectionType == null || string.IsNullOrEmpty(s_connectionString))
                {
                    try
                    {
                        // Load connection settings from the system settings category				
                        ConfigurationFile config = ConfigurationFile.Current; //new ConfigurationFile("~/web.config", ApplicationType.Web);
                        CategorizedSettingsElementCollection configSettings = config.Settings[settingsCategory];

                        string dataProviderString = configSettings["DataProviderString"].Value;
                        s_connectionString = configSettings["ConnectionString"].Value;

                        if (string.IsNullOrEmpty(s_connectionString))
                            throw new NullReferenceException("ConnectionString setting was undefined.");

                        if (string.IsNullOrEmpty(dataProviderString))
                            throw new NullReferenceException("DataProviderString setting was undefined.");

                        // Attempt to load configuration from an ADO.NET database connection
                        Dictionary<string, string> settings;
                        string assemblyName, connectionTypeName, adapterTypeName;
                        Assembly assembly;

                        settings = dataProviderString.ParseKeyValuePairs();
                        assemblyName = settings["AssemblyName"].ToNonNullString();
                        connectionTypeName = settings["ConnectionType"].ToNonNullString();
                        adapterTypeName = settings["AdapterType"].ToNonNullString();

                        if (string.IsNullOrEmpty(connectionTypeName))
                            throw new NullReferenceException("Database connection type was undefined.");

                        if (string.IsNullOrEmpty(adapterTypeName))
                            throw new NullReferenceException("Database adapter type was undefined.");

                        assembly = Assembly.Load(new AssemblyName(assemblyName));
                        s_connectionType = assembly.GetType(connectionTypeName);
                        s_adapterType = assembly.GetType(adapterTypeName);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Failed to load defined data provider - check \"DataProviderString\" in configuration file: " + ex.Message, ex);
                    }
                }

                try
                {
                    // Open ADO.NET provider connection
                    m_connection = (IDbConnection)Activator.CreateInstance(s_connectionType);
                    m_connection.ConnectionString = s_connectionString;
                    m_connection.Open();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to open data connection - check \"ConnectionString\" in configuration file: " + ex.Message, ex);
                }
            }

            /// <summary>
            /// Releases the unmanaged resources before the <see cref="DataConnection"/> object is reclaimed by <see cref="GC"/>.
            /// </summary>
            ~DataConnection()
            {
                Dispose(false);
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets an open <see cref="IDbConnection"/> to configured ADO.NET data source.
            /// </summary>
            public IDbConnection Connection
            {
                get
                {
                    return m_connection;
                }
            }

            /// <summary>
            /// Gets the type of data adapter for configured ADO.NET data source.
            /// </summary>
            public Type AdapterType
            {
                get
                {
                    return s_adapterType;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Releases all the resources used by the <see cref="DataConnection"/> object.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases the unmanaged resources used by the <see cref="DataConnection"/> object and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (!m_disposed)
                {
                    try
                    {
                        if (disposing)
                        {
                            if (m_connection != null)
                                m_connection.Dispose();
                            m_connection = null;
                        }
                    }
                    finally
                    {
                        m_disposed = true;  // Prevent duplicate dispose.
                    }
                }
            }

            #endregion

            #region [ Static ]

            //Static Fields
            static Type s_connectionType;
            static Type s_adapterType;
            static string s_connectionString;

            #endregion
        }

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
        /// <param name="canRefreshData">true if the security provider can refresh <see cref="UserData"/> from the backend datastore, otherwise false.</param>
        /// <param name="canUpdateData">true if the security provider can update <see cref="UserData"/> in the backend datastore, otherwise false.</param>
        /// <param name="canResetPassword">true if the security provider can reset user password, otherwise false.</param>
        /// <param name="canChangePassword">true if the security provider can change user password, otherwise false.</param>
        protected AdoSecurityProvider(string username, bool canRefreshData, bool canUpdateData, bool canResetPassword, bool canChangePassword)
            : base(username, canRefreshData, canUpdateData, canResetPassword, canChangePassword)
        {
            base.ConnectionString = "Eval(systemSettings.ConnectionString)";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Geta a boolean value that indicates whether <see cref="SecurityProviderBase.UpdateData"/> operation is supported.
        /// </summary>
        public override bool CanUpdateData
        {
            get
            {
                if (UserData.IsDefined && UserData.IsExternal)
                    // Data update supported on external user accounts.
                    return true;
                else
                    // Data update not supported on internal user accounts.
                    return false;
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
            settings.Add("DataProviderString", "Eval(systemSettings.DataProviderString)", "Configuration database ADO.NET data provider assembly type creation string to be used for connection to the backend security datastore.");
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
                DataTable userDataTable = new DataTable();
                DataTable userGroupDataTable = new DataTable();
                DataTable userRoleDataTable = new DataTable();
                DataRow userDataRow = null;
                string groupName, roleName;

                using (IDbConnection dbConnection = (new DataConnection(SettingsCategory)).Connection)
                {
                    if (dbConnection == null)
                        return false;

                    userDataTable.Load(dbConnection.CreateParameterizedCommand("Select ID, Name, Password, FirstName, LastName, Phone, Email, LockedOut, UseADAuthentication, ChangePasswordOn, CreatedOn From UserAccount Where Name = @name", UserData.Username).ExecuteReader());

                    if (userDataTable.Rows.Count <= 0)
                    {
                        // User doesn't exist in the database, however, user may exist in an NT authentication group which may have an explict role assignment. To test for this case
                        // we make the assumption that this is a Windows authenticated user and test for rights within groups
                        UserData.IsDefined = true;
                        UserData.IsExternal = false;
                    }
                    else
                    {
                        userDataRow = userDataTable.Rows[0];
                        UserData.IsDefined = true;
                        UserData.IsExternal = !Convert.ToBoolean(userDataRow["UseADAuthentication"]);
                    }

                    if (UserData.IsExternal && userDataRow != null)
                    {
                        if (string.IsNullOrEmpty(UserData.LoginID))
                            UserData.LoginID = UserData.Username;

                        if (!Convert.IsDBNull(userDataRow["Password"]))
                            UserData.Password = Convert.ToString(userDataRow["Password"]);

                        if (!Convert.IsDBNull(userDataRow["FirstName"]))
                            UserData.FirstName = Convert.ToString(userDataRow["FirstName"]);

                        if (!Convert.IsDBNull(userDataRow["LastName"]))
                            UserData.LastName = Convert.ToString(userDataRow["LastName"]);

                        if (!Convert.IsDBNull(userDataRow["Phone"]))
                            UserData.PhoneNumber = Convert.ToString(userDataRow["Phone"]);

                        if (!Convert.IsDBNull(userDataRow["Email"]))
                            UserData.EmailAddress = Convert.ToString(userDataRow["Email"]);

                        if (!Convert.IsDBNull(userDataRow["ChangePasswordOn"]))
                            UserData.PasswordChangeDateTime = Convert.ToDateTime(userDataRow["ChangePasswordOn"]);

                        if (!Convert.IsDBNull(userDataRow["CreatedOn"]))
                            UserData.AccountCreatedDateTime = Convert.ToDateTime(userDataRow["CreatedOn"]);

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
                        // Load implicitly assigned groups - this happens via NT user groups that get loaded into user data
                        // group collection. When group definitions are defined with the same name as their NT equivalents,
                        // this will allow automatic external group management from within NT group management (AD or local).
                        base.RefreshData(UserData.Groups, AdoSecurityProvider.ProviderID);
                    }

                    // Administrator can lock out NT user as well as database-only user via database
                    if (!UserData.IsLockedOut && userDataRow != null && !Convert.IsDBNull(userDataRow["LockedOut"]))
                        UserData.IsLockedOut = Convert.ToBoolean(userDataRow["LockedOut"]);

                    // Load explicitly assigned groups
                    userGroupDataTable.Load(dbConnection.CreateParameterizedCommand("Select SecurityGroupID, SecurityGroupName, SecurityGroupDescription From SecurityGroupUserAccountDetail Where UserName = @name", UserData.Username).ExecuteReader());

                    foreach (DataRow group in userGroupDataTable.Rows)
                    {
                        if (!Convert.IsDBNull(group["SecurityGroupName"]))
                        {
                            groupName = Convert.ToString(group["SecurityGroupName"]);

                            if (!UserData.Groups.Contains(groupName, StringComparer.InvariantCultureIgnoreCase))
                                UserData.Groups.Add(groupName);
                        }
                    }

                    UserData.Roles.Clear();

                    // Load implicitly assigned roles
                    foreach (string group in UserData.Groups)
                    {
                        userRoleDataTable.Load(dbConnection.CreateParameterizedCommand("Select ApplicationRoleID, ApplicationRoleName, ApplicationRoleDescription From ApplicationRoleSecurityGroupDetail Where SecurityGroupName = @groupName", group).ExecuteReader());
                    }

                    // Load explicitly assigned roles
                    userRoleDataTable.Load(dbConnection.CreateParameterizedCommand("Select ApplicationRoleID, ApplicationRoleName, ApplicationRoleDescription From ApplicationRoleUserAccountDetail Where UserName = @name", UserData.Username).ExecuteReader());
                    userRoleDataTable.Load(dbConnection.CreateParameterizedCommand("Select ApplicationRoleSecurityGroupDetail.ApplicationRoleID, ApplicationRoleSecurityGroupDetail.ApplicationRoleName, ApplicationRoleSecurityGroupDetail.ApplicationRoleDescription From ApplicationRoleSecurityGroupDetail, SecurityGroupUserAccountDetail Where ApplicationRoleSecurityGroupDetail.SecurityGroupID = SecurityGroupUserAccountDetail.SecurityGroupID AND SecurityGroupUserAccountDetail.UserName = @name", UserData.Username).ExecuteReader());

                    foreach (DataRow role in userRoleDataTable.Rows)
                    {
                        if (!Convert.IsDBNull(role["ApplicationRoleName"]))
                        {
                            roleName = Convert.ToString(role["ApplicationRoleName"]);

                            if (!UserData.Roles.Contains(roleName, StringComparer.InvariantCultureIgnoreCase))
                                UserData.Roles.Add(roleName);
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
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
            // Note that blank password should be allowed so that LDAP can authenticate current credentials for
            // pass through authentication, if desired
            if (!UserData.IsDefined || UserData.IsDisabled || UserData.IsLockedOut ||
                (UserData.PasswordChangeDateTime != DateTime.MinValue && UserData.PasswordChangeDateTime <= DateTime.UtcNow))
                return false;

            try
            {
                // Authenticate user credentials.
                UserData.IsAuthenticated = false;
                if (!UserData.IsExternal)
                    // Authenticate against active directory.
                    base.Authenticate(password);
                else
                    // Authenticate against backend datastore.
                    UserData.IsAuthenticated = UserData.Password == SecurityProviderUtility.EncryptPassword(password);

                // Log user authentication result.
                LogLogin(UserData.IsAuthenticated);

                return UserData.IsAuthenticated;
            }
            catch (Exception ex)
            {
                LogError(ex.Source, ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Changes user password in the backend datastore.
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
                //Check if old and new passwords are different.
                if (oldPassword == newPassword)
                    throw new Exception("New password cannot be same as old password.");

                // Perform password change for internal users.
                if (!UserData.IsExternal)
                    return base.ChangePassword(oldPassword, newPassword);

                // Verify old password.
                UserData.PasswordChangeDateTime = DateTime.MinValue;
                if (!Authenticate(oldPassword))
                    return false;

                // Verify new password.
                if (!Regex.IsMatch(newPassword, PasswordRequirementRegex))
                    throw new SecurityException(PasswordRequirementError);

                using (IDbConnection dbConnection = (new DataConnection(SettingsCategory)).Connection)
                {
                    if (dbConnection == null)
                        return false;

                    IDbCommand command = dbConnection.CreateCommand();
                    command.CommandType = CommandType.Text;

                    if (command.Connection.ConnectionString.Contains("Microsoft.Jet.OLEDB"))
                        command.CommandText = "Update UserAccount Set [Password] = @newPassword Where Name = @name AND [Password] = @oldPassword";
                    else
                        command.CommandText = "Update UserAccount Set Password = @newPassword Where Name = @name AND Password = @oldPassword";

                    IDbDataParameter param = command.CreateParameter();
                    param.ParameterName = "@newPassword";
                    param.Value = SecurityProviderUtility.EncryptPassword(newPassword);
                    command.Parameters.Add(param);

                    param = command.CreateParameter();
                    param.ParameterName = "@name";
                    param.Value = UserData.Username;
                    command.Parameters.Add(param);

                    param = command.CreateParameter();
                    param.ParameterName = "@oldPassword";
                    param.Value = UserData.Password;
                    command.Parameters.Add(param);

                    command.ExecuteNonQuery();
                }

                return true;
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (Exception ex)
            {
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
        protected virtual bool LogLogin(bool loginSuccess)
        {
            if (!string.IsNullOrEmpty(ApplicationName))
            {
                if (!UserData.IsDefined)
                    return false;

                using (IDbConnection dbConnection = (new DataConnection(SettingsCategory)).Connection)
                {
                    IDbCommand command = dbConnection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    IDbDataParameter param;
                    command.CommandText = "Insert Into AccessLog (UserName, AccessGranted, Comment) Values (@userName, @accessGranted, @comment)";
                    param = command.CreateParameter();
                    param.ParameterName = "@userName";
                    param.Value = UserData.Username;
                    command.Parameters.Add(param);
                    param = command.CreateParameter();
                    param.ParameterName = "@accessGranted";
                    param.Value = loginSuccess;
                    command.Parameters.Add(param);
                    param = command.CreateParameter();
                    param.ParameterName = "@comment";
                    param.Value = "";
                    command.Parameters.Add(param);
                    command.ExecuteNonQuery();
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Logs information about an encountered exception to the backend datastore.
        /// </summary>
        /// <param name="source">Source of the exception.</param>
        /// <param name="message">Detailed description of the exception.</param>
        /// <returns>true if logging was successful, otherwise false.</returns>
        protected virtual bool LogError(string source, string message)
        {
            if (!string.IsNullOrEmpty(ApplicationName))
            {
                try
                {
                    using (IDbConnection dbConnection = (new DataConnection(SettingsCategory)).Connection)
                    {
                        IDbCommand command = dbConnection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        IDbDataParameter param;
                        command.CommandText = "Insert Into ErrorLog (Source, Message) Values (@source, @message)";
                        param = command.CreateParameter();
                        param.ParameterName = "@source";
                        param.Value = source;
                        command.Parameters.Add(param);
                        param = command.CreateParameter();
                        param.ParameterName = "@message";
                        param.Value = message;
                        command.Parameters.Add(param);
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch
                {
                }
            }

            return false;
        }

        #endregion
    }
}
