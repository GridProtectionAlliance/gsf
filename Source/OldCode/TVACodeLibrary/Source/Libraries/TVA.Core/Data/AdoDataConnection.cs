//*******************************************************************************************************
//  AdoDataConnection.cs - Gbtc
//
//  Tennessee Valley Authority, 2011
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/07/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/19/2011 - Stephen C. Wills
//       Added database awareness and Oracle database compatibility.
//  10/18/2011 - J. Ritchie Carroll
//       Modified ADO database class to allow directly instantied instances, as well as configured.
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

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using TVA.Configuration;

namespace TVA.Data
{
    /// <summary>
    /// Specifies the database type underlying an <see cref="AdoDataConnection"/>.
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// Underlying ADO database type is Microsoft Access.
        /// </summary>
        Access,

        /// <summary>
        /// Underlying ADO database type is SQL Server.
        /// </summary>
        SQLServer,

        /// <summary>
        /// Underlying ADO database type is MySQL.
        /// </summary>
        MySQL,

        /// <summary>
        /// Underlying ADO database type is Oracle.
        /// </summary>
        Oracle,

        /// <summary>
        /// Underlying ADO database type is SQLite.
        /// </summary>
        SQLite,

        /// <summary>
        /// Underlying ADO database type is unknown.
        /// </summary>
        Other
    }

    /// <summary>
    /// Creates a new <see cref="IDbConnection"/> from any specified or configured ADO.NET data source.
    /// </summary>
    /// <remarks>
    /// Example connection and data provider strings:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Database type</term>
    ///         <description>Example connection string / data provider string</description>
    ///     </listheader>
    ///     <item>
    ///         <term>SQL Server</term>
    ///         <description>
    ///         ConnectionString = "Data Source=serverName; Initial Catalog=databaseName; User ID=userName; Password=password"<br/>
    ///         DataProviderString = "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter"
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>Oracle</term>
    ///         <description>
    ///         ConnectionString = "Data Source=tnsName; User ID=schemaUserName; Password=schemaPassword"<br/>
    ///         DataProviderString = "AssemblyName={Oracle.DataAccess, Version=2.112.2.0, Culture=neutral, PublicKeyToken=89b483f429c47342}; ConnectionType=Oracle.DataAccess.Client.OracleConnection; AdapterType=Oracle.DataAccess.Client.OracleDataAdapter"
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>MySQL</term>
    ///         <description>
    ///         ConnectionString = "Server=serverName; Database=databaseName; Uid=root; Pwd=password; allow user variables = true"<br/>
    ///         DataProviderString = "AssemblyName={MySql.Data, Version=6.3.6.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d}; ConnectionType=MySql.Data.MySqlClient.MySqlConnection; AdapterType=MySql.Data.MySqlClient.MySqlDataAdapter"
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>SQLite</term>
    ///         <description>
    ///         ConnectionString = "Data Source=databaseName.db; Version=3"<br/>
    ///         DataProviderString = "AssemblyName={System.Data.SQLite, Version=1.0.74.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139}; ConnectionType=System.Data.SQLite.SQLiteConnection; AdapterType=System.Data.SQLite.SQLiteDataAdapter"
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>Access / OleDB</term>
    ///         <description>
    ///         ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=databaseName.mdb"<br/>
    ///         DataProviderString = "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.OleDb.OleDbConnection; AdapterType=System.Data.OleDb.OleDbDataAdapter"
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>ODBC Connection</term>
    ///         <description>
    ///         ConnectionString = "Driver={SQL Server Native Client 10.0}; Server=serverName; Database=databaseName; Uid=userName; Pwd=password"<br/>
    ///         DataProviderString = "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.Odbc.OdbcConnection; AdapterType=System.Data.Odbc.OdbcDataAdapter"
    ///         </description>
    ///     </item>
    /// </list>
    /// Example configuration file that defines connection settings:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0" encoding="utf-8"?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="TVA.Configuration.CategorizedSettingsSection, TVA.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <systemSettings>
    ///       <add name="ConnectionString" value="Data Source=localhost\SQLEXPRESS; Initial Catalog=MyDatabase; Integrated Security=SSPI" description="ADO database connection string" encrypted="false" />
    ///       <add name="DataProviderString" value="AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter" description="ADO database provider string" encrypted="false" />
    ///     </systemSettings>
    ///   </categorizedSettings>
    ///   <startup>
    ///     <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0" />
    ///   </startup>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </remarks>
    public class AdoDataConnection : IDisposable
    {
        #region [ Members ]

        // Fields
        private IDbConnection m_connection;
        private DatabaseType m_databaseType;
        private readonly string m_connectionString;
        private readonly Type m_connectionType;
        private readonly Type m_adapterType;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates and opens a new <see cref="AdoDataConnection"/> based on connection settings in configuration file.
        /// </summary>
        /// <param name="settingsCategory">Settings category to use for connection settings.</param>
        public AdoDataConnection(string settingsCategory)
        {
            if (string.IsNullOrWhiteSpace(settingsCategory))
                throw new ArgumentNullException("settingsCategory", "Parameter cannot be null or empty");

            // Only need to establish data types and load settings once per defined section since they are being loaded from config file
            AdoDataConnection configuredConnection;

            if (!s_configuredConnections.TryGetValue(settingsCategory, out configuredConnection))
            {
                string connectionString, dataProviderString;

                try
                {
                    // Load connection settings from the system settings category				
                    ConfigurationFile config = ConfigurationFile.Current;
                    CategorizedSettingsElementCollection configSettings = config.Settings[settingsCategory];

                    connectionString = configSettings["ConnectionString"].Value;
                    dataProviderString = configSettings["DataProviderString"].Value;

                    if (string.IsNullOrWhiteSpace(connectionString))
                        throw new NullReferenceException("ConnectionString setting is not defined in the configuration file.");

                    if (string.IsNullOrWhiteSpace(dataProviderString))
                        throw new NullReferenceException("DataProviderString setting is not defined in the configuration file.");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to load ADO database connection settings from configuration file: " + ex.Message, ex);
                }

                // Define connection settings without opening a connection
                configuredConnection = new AdoDataConnection(connectionString, dataProviderString, false);
                s_configuredConnections.TryAdd(settingsCategory, configuredConnection);
            }

            try
            {
                // Copy static instance data to member variables
                m_databaseType = configuredConnection.m_databaseType;
                m_connectionString = configuredConnection.m_connectionString;
                m_connectionType = configuredConnection.m_connectionType;
                m_adapterType = configuredConnection.m_adapterType;

                // Open ADO.NET provider connection
                m_connection = (IDbConnection)Activator.CreateInstance(m_connectionType);
                m_connection.ConnectionString = m_connectionString;
                m_connection.Open();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to open ADO data connection, verify \"ConnectionString\" in configuration file: " + ex.Message, ex);
            }
        }
        /// <summary>
        /// Creates and opens a new <see cref="AdoDataConnection"/> from specified <paramref name="connectionString"/> and <paramref name="dataProviderString"/>.
        /// </summary>
        /// <param name="connectionString">Database specific ADO connection string.</param>
        /// <param name="dataProviderString">Key/value pairs that define which ADO assembly and types to load.</param>
        public AdoDataConnection(string connectionString, string dataProviderString)
            : this(connectionString, dataProviderString, true)
        {
        }

        // Creates a new AdoDataConnection, optionally opening connection.
        private AdoDataConnection(string connectionString, string dataProviderString, bool openConnection)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("connectionString", "Parameter cannot be null or empty");

            if (string.IsNullOrWhiteSpace(dataProviderString))
                throw new ArgumentNullException("dataProviderString", "Parameter cannot be null or empty");

            // Cache connection string as member level variable
            m_connectionString = connectionString;

            try
            {
                // Attempt to load configuration from an ADO.NET database connection
                Dictionary<string, string> settings;
                string assemblyName, connectionTypeName, adapterTypeName;
                Assembly assembly;

                settings = dataProviderString.ParseKeyValuePairs();
                assemblyName = settings["AssemblyName"].ToNonNullString();
                connectionTypeName = settings["ConnectionType"].ToNonNullString();
                adapterTypeName = settings["AdapterType"].ToNonNullString();

                if (string.IsNullOrEmpty(connectionTypeName))
                    throw new NullReferenceException("ADO database connection type was undefined.");

                if (string.IsNullOrEmpty(adapterTypeName))
                    throw new NullReferenceException("ADO database adapter type was undefined.");

                assembly = Assembly.Load(new AssemblyName(assemblyName));
                m_connectionType = assembly.GetType(connectionTypeName);
                m_adapterType = assembly.GetType(adapterTypeName);
                m_databaseType = GetDatabaseType();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load ADO data provider, verify \"DataProviderString\": " + ex.Message, ex);
            }

            if (!openConnection)
                return;

            try
            {
                // Open ADO.NET provider connection
                m_connection = (IDbConnection)Activator.CreateInstance(m_connectionType);
                m_connection.ConnectionString = m_connectionString;
                m_connection.Open();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to open ADO data connection, verify \"ConnectionString\": " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="AdoDataConnection"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~AdoDataConnection()
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
                return m_adapterType;
            }
        }

        /// <summary>
        /// Gets or sets the type of the database underlying the <see cref="AdoDataConnection"/>.
        /// </summary>
        /// <remarks>
        /// This value is automatically assigned based on the adapter type specified in the data provider string, however,
        /// if the database type cannot be determined it will be set to <see cref="TVA.Data.DatabaseType.Other"/>. In this
        /// case, if you know the behavior of your custom ADO database connection matches that of another defined database
        /// type, you can manually assign the database type to allow for database interaction interoperability.
        /// </remarks>
        public DatabaseType DatabaseType
        {
            get
            {
                return m_databaseType;
            }
            set
            {
                m_databaseType = value;
            }
        }

        /// <summary>
        /// Gets a value to indicate whether source database is Microsoft Access.
        /// </summary>
        public bool IsJetEngine
        {
            get
            {
                return m_databaseType == DatabaseType.Access;
            }
        }

        /// <summary>
        /// Gets a value to indicate whether source database is Microsoft SQL Server.
        /// </summary>
        public bool IsSQLServer
        {
            get
            {
                return m_databaseType == DatabaseType.SQLServer;
            }
        }

        /// <summary>
        /// Gets a value to indicate whether source database is MySQL.
        /// </summary>
        public bool IsMySQL
        {
            get
            {
                return m_databaseType == DatabaseType.MySQL;
            }
        }

        /// <summary>
        /// Gets a value to indicate whether source database is Oracle.
        /// </summary>
        public bool IsOracle
        {
            get
            {
                return m_databaseType == DatabaseType.Oracle;
            }
        }

        /// <summary>
        /// Gets a value to indicate whether source database is SQLite.
        /// </summary>
        public bool IsSqlite
        {
            get
            {
                return m_databaseType == DatabaseType.SQLite;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="AdoDataConnection"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AdoDataConnection"/> object and optionally releases the managed resources.
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
                        if ((object)m_connection != null)
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

        /// <summary>
        /// Returns proper <see cref="System.Boolean"/> implementation for connected <see cref="AdoDataConnection"/> database type.
        /// </summary>
        /// <param name="value"><see cref="System.Boolean"/> to format per database type.</param>
        /// <returns>Proper <see cref="System.Boolean"/> implementation for connected <see cref="AdoDataConnection"/> database type.</returns>
        public object Bool(bool value)
        {
            if (IsOracle)
                return value ? 1 : 0;

            return value;
        }

        /// <summary>
        /// Returns proper <see cref="System.Guid"/> implementation for connected <see cref="AdoDataConnection"/> database type.
        /// </summary>
        /// <param name="guid"><see cref="System.Guid"/> to format per database type.</param>
        /// <returns>Proper <see cref="System.Guid"/> implementation for connected <see cref="AdoDataConnection"/> database type.</returns>
        public object Guid(Guid guid)
        {
            if (IsJetEngine)
                return "{" + guid.ToString() + "}";

            if (IsOracle || IsSqlite)
                return guid.ToString().ToLower();

            //return "P" + guid.ToString();

            return guid;
        }

        /// <summary>
        /// Retrieves <see cref="System.Guid"/> from a <see cref="DataRow"/> field based on database type.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> from which value needs to be retrieved.</param>
        /// <param name="fieldName">Name of the field which contains <see cref="System.Guid"/>.</param>
        /// <returns><see cref="System.Guid"/>.</returns>
        public Guid Guid(DataRow row, string fieldName)
        {
            if (IsJetEngine || IsMySQL || IsOracle || IsSqlite)
                return System.Guid.Parse(row.Field<object>(fieldName).ToString());

            return row.Field<Guid>(fieldName);
        }

        /// <summary>
        /// Returns current UTC time in an implementation that is proper for connected <see cref="AdoDataConnection"/> database type.
        /// </summary>
        /// <param name="usePrecisionTime">Set to <c>true</c> to use precision time.</param>
        /// <returns>Current UTC time in implementation that is proper for connected <see cref="AdoDataConnection"/> database type.</returns>
        public object UtcNow(bool usePrecisionTime = false)
        {
            if (usePrecisionTime)
            {
                if (IsJetEngine)
                    return DateTime.UtcNow.ToOADate();

                return DateTime.UtcNow;
            }

            if (IsJetEngine)
                return DateTime.UtcNow.ToOADate();

            return DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a parameterized query string for the underlying database type based on the given format string and parameter names.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="parameterNames">A string array that contains zero or more parameter names to format.</param>
        /// <returns>A parameterized query string based on the given format and parameter names.</returns>
        public string ParameterizedQueryString(string format, params string[] parameterNames)
        {
            char paramChar = IsOracle ? ':' : '@';
            object[] parameters = parameterNames.Select(name => paramChar + name).Cast<object>().ToArray();
            return string.Format(format, parameters);
        }

        private DatabaseType GetDatabaseType()
        {
            DatabaseType type = DatabaseType.Other;

            if ((object)m_adapterType != null)
            {
                switch (m_adapterType.Name)
                {
                    case "SqlDataAdapter":
                        type = DatabaseType.SQLServer;
                        break;
                    case "MySqlDataAdapter":
                        type = DatabaseType.MySQL;
                        break;
                    case "OracleDataAdapter":
                        type = DatabaseType.Oracle;
                        break;
                    case "SQLiteDataAdapter":
                        type = DatabaseType.SQLite;
                        break;
                    case "OleDbDataAdapter":
                        if ((object)m_connectionString != null && m_connectionString.Contains("Microsoft.Jet.OLEDB"))
                            type = DatabaseType.Access;
                        break;
                }
            }

            return type;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<string, AdoDataConnection> s_configuredConnections;

        // Static Constructor
        static AdoDataConnection()
        {
            s_configuredConnections = new ConcurrentDictionary<string, AdoDataConnection>(StringComparer.InvariantCultureIgnoreCase);
        }

        // Static Methods

        /// <summary>
        /// Forces a reload of cached configuration connection settings.
        /// </summary>
        public static void ReloadConfigurationSettings()
        {
            if ((object)s_configuredConnections != null)
                s_configuredConnections.Clear();
        }

        #endregion
    }
}
