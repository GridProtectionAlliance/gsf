//*******************************************************************************************************
//  DataExtensions.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/05/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  05/25/2004 - J. Ritchie Carroll
//       Added "with parameters" overloads to all basic query functions.
//  12/10/2004 - Tim M Shults
//       Added several new WithParameters overloads that allow a programmer to send just the
//       parameter values instead of creating a series of parameter objects and then sending
//       them through. Easy way to cut down on the amount of code.
//       This code is just for calls to Stored Procedures and will not work for in-line SQL.
//  03/28/2006 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Database.Common).
//  08/21/2007 - Darrell Zuercher
//       Edited code comments.
//  09/15/2008 - J. Ritchie Carroll
//       Converted to C# extensions.
//  09/29/2008 - Pinal C. Patel
//       Reviewed code comments.
//  09/09/2009 - J. Ritchie Carroll
//       Added extensions for ODBC providers.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/02/2009 - Stephen C. Wills
//       Added disposal of database command objects.
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
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace TVA.Data
{
    /// <summary>
    /// Defines extension functions related to database and SQL interaction.
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// The default timeout duration used for executing SQL statements when timeout duration is not specified.
        /// </summary>
        public const int DefaultTimeoutDuration = 30;

        #region [ SQL Encoding String Extension ]

        /// <summary>
        /// Performs SQL encoding on given T-SQL string.
        /// </summary>
        /// <param name="sql">The string on which SQL encoding is to be performed.</param>
        /// <returns>The SQL encoded string.</returns>
        public static string SqlEncode(this string sql)
        {
            return sql.Replace("\'", "\'\'").Replace("/*", "").Replace("--", "");
        }

        #endregion

        #region [ ExecuteNonQuery Overloaded Extension ]

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <returns>The number of rows affected.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static int ExecuteNonQuery<TConnection>(this TConnection connection, string sql) where TConnection : IDbConnection
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The number of rows affected.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static int ExecuteNonQuery<TConnection>(this TConnection connection, string sql, int timeout) where TConnection : IDbConnection
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandTimeout = timeout;
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OleDbConnection connection, string sql, int timeout, params object[] parameters)
        {
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OdbcConnection connection, string sql, int timeout, params object[] parameters)
        {
            using (OdbcCommand command = new OdbcCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this SqlConnection connection, string sql, int timeout, params object[] parameters)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteNonQuery();
            }
        }


        #endregion

        #region [ ExecuteReader Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static IDataReader ExecuteReader<TConnection>(this TConnection connection, string sql) where TConnection : IDbConnection
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static IDataReader ExecuteReader<TConnection>(this TConnection connection, string sql, int timeout) where TConnection : IDbConnection
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, timeout);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static IDataReader ExecuteReader<TConnection>(this TConnection connection, string sql, CommandBehavior behavior, int timeout) where TConnection : IDbConnection
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandTimeout = timeout;
                return command.ExecuteReader(behavior);
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and builds a <see cref="OleDbDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="OleDbDataReader"/> object.</returns>
        public static OleDbDataReader ExecuteReader(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and builds a <see cref="OleDbDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="OleDbDataReader"/> object.</returns>
        public static OleDbDataReader ExecuteReader(this OleDbConnection connection, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteReader(behavior);
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and builds a <see cref="OdbcDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="OdbcDataReader"/> object.</returns>
        public static OdbcDataReader ExecuteReader(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and builds a <see cref="OdbcDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="OdbcDataReader"/> object.</returns>
        public static OdbcDataReader ExecuteReader(this OdbcConnection connection, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            using (OdbcCommand command = new OdbcCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteReader(behavior);
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and builds a <see cref="SqlDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="SqlDataReader"/> object.</returns>
        public static SqlDataReader ExecuteReader(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and builds a <see cref="SqlDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="SqlDataReader"/> object.</returns>
        public static SqlDataReader ExecuteReader(this SqlConnection connection, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteReader(behavior);
            }
        }

        #endregion

        #region [ ExecuteScalar Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static object ExecuteScalar<TConnection>(this TConnection connection, string sql) where TConnection : IDbConnection
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static object ExecuteScalar<TConnection>(this TConnection connection, string sql, int timeout) where TConnection : IDbConnection
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandTimeout = timeout;
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OleDbConnection connection, string sql, int timeout, params object[] parameters)
        {
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OdbcConnection connection, string sql, int timeout, params object[] parameters)
        {
            using (OdbcCommand command = new OdbcCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this SqlConnection connection, string sql, int timeout, params object[] parameters)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteScalar();
            }
        }

        #endregion

        #region [ RetrieveRow Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql, int timeout)
        {
            return connection.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OdbcConnection connection, string sql)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OdbcConnection connection, string sql, int timeout)
        {
            return connection.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OdbcConnection connection, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql, int timeout)
        {
            return connection.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this IDbConnection connection, Type dataAdapterType, string sql)
        {
            return connection.RetrieveRow(dataAdapterType, sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this IDbConnection connection, Type dataAdapterType, string sql, int timeout)
        {
            DataTable dataTable = connection.RetrieveData(dataAdapterType, sql, timeout);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        #endregion

        #region [ RetrieveData Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, 30);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveData(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcConnection connection, string sql)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, 30);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveData(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveData(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this IDbConnection connection, Type dataAdapterType, string sql)
        {
            return connection.RetrieveData(dataAdapterType, sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this IDbConnection connection, Type dataAdapterType, string sql, int timeout)
        {
            return connection.RetrieveDataSet(dataAdapterType, sql, timeout).Tables[0];
        }

        #endregion

        #region [ RetrieveDataSet Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            {
                command.PopulateParameters(parameters);
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command);
                DataSet data = new DataSet("Temp");
                dataAdapter.Fill(data, startRow, maxRows, "Table1");

                return data;
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OdbcConnection connection, string sql)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OdbcConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OdbcConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            using (OdbcCommand command = new OdbcCommand(sql, connection))
            {
                command.PopulateParameters(parameters);
                OdbcDataAdapter dataAdapter = new OdbcDataAdapter(command);
                DataSet data = new DataSet("Temp");
                dataAdapter.Fill(data, startRow, maxRows, "Table1");

                return data;
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple table depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataSet data = new DataSet("Temp");
                dataAdapter.Fill(data, startRow, maxRows, "Table1");

                return data;
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this IDbConnection connection, Type dataAdapterType, string sql)
        {
            return connection.RetrieveDataSet(dataAdapterType, sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this IDbConnection connection, Type dataAdapterType, string sql, int timeout)
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandTimeout = timeout;
                IDataAdapter dataAdapter = (IDataAdapter)Activator.CreateInstance(dataAdapterType, command);
                DataSet data = new DataSet("Temp");
                dataAdapter.Fill(data);

                return data;
            }
        }

        #endregion

        #region [ UpdateData Overloaded Functions ]

        /// <summary>
        /// Updates the underlying data of the <see cref="DataTable"/> using <see cref="OleDbConnection"/>, and
        /// returns the number of rows successfully updated.
        /// </summary>
        /// <param name="sourceData">The <see cref="DataTable"/> used to update the underlying data source.</param>
        /// <param name="sourceSql">The SQL statement used initially to populate the <see cref="DataTable"/>.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for updating the underlying data source.</param>
        /// <returns>The number of rows successfully updated from the <see cref="DataTable"/>.</returns>
        public static int UpdateData(this OleDbConnection connection, DataTable sourceData, string sourceSql)
        {
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter(sourceSql, connection);
            OleDbCommandBuilder commandBuilder = new OleDbCommandBuilder(dataAdapter);
            return dataAdapter.Update(sourceData);
        }

        /// <summary>
        /// Updates the underlying data of the <see cref="DataTable"/> using <see cref="OdbcConnection"/>, and
        /// returns the number of rows successfully updated.
        /// </summary>
        /// <param name="sourceData">The <see cref="DataTable"/> used to update the underlying data source.</param>
        /// <param name="sourceSql">The SQL statement used initially to populate the <see cref="DataTable"/>.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for updating the underlying data source.</param>
        /// <returns>The number of rows successfully updated from the <see cref="DataTable"/>.</returns>
        public static int UpdateData(this OdbcConnection connection, DataTable sourceData, string sourceSql)
        {
            OdbcDataAdapter dataAdapter = new OdbcDataAdapter(sourceSql, connection);
            OdbcCommandBuilder commandBuilder = new OdbcCommandBuilder(dataAdapter);
            return dataAdapter.Update(sourceData);
        }

        /// <summary>
        /// Updates the underlying data of the <see cref="DataTable"/> using <see cref="SqlConnection"/>, and
        /// returns the number of rows successfully updated.
        /// </summary>
        /// <param name="sourceData">The <see cref="DataTable"/> used to update the underlying data source.</param>
        /// <param name="sourceSql">The SQL statement used initially to populate the <see cref="DataTable"/>.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for updating the underlying data source.</param>
        /// <returns>The number of rows successfully updated from the <see cref="DataTable"/>.</returns>
        public static int UpdateData(this SqlConnection connection, DataTable sourceData, string sourceSql)
        {
            SqlDataAdapter dataAdapter = new SqlDataAdapter(sourceSql, connection);
            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);
            return dataAdapter.Update(sourceData);
        }

        #endregion

        #region [ Command Parameter Population Functions ]

        /// <summary>
        /// Takes the <see cref="OleDbCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="OleDbCommand"/> whose parameters are to be populated.</param>
        /// <param name="parameters">The parameters to populate the <see cref="OleDbCommand"/> parameters with.</param>
        public static void PopulateParameters(this OleDbCommand command, object[] parameters)
        {
            command.PopulateParameters(OleDbCommandBuilder.DeriveParameters, parameters);
        }

        /// <summary>
        /// Takes the <see cref="OdbcCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="OdbcCommand"/> whose parameters are to be populated.</param>
        /// <param name="parameters">The parameters to populate the <see cref="OdbcCommand"/> parameters with.</param>
        public static void PopulateParameters(this OdbcCommand command, object[] parameters)
        {
            command.PopulateParameters(OdbcCommandBuilder.DeriveParameters, parameters);
        }

        /// <summary>
        ///  Takes the <see cref="SqlCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="SqlCommand"/> whose parameters are to be populated.</param>
        /// <param name="parameters">The parameters to populate the <see cref="SqlCommand"/> parameters with.</param>
        public static void PopulateParameters(this SqlCommand command, object[] parameters)
        {
            command.PopulateParameters(SqlCommandBuilder.DeriveParameters, parameters);
        }

        /// <summary>
        /// Takes the <see cref="IDbCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="IDbCommand"/> whose parameters are to be populated.</param>
        /// <param name="deriveParameters">The DeriveParameters implementation of the <paramref name="command"/> to use to populate parameters.</param>
        /// <param name="parameters">The parameters to populate the <see cref="IDbCommand"/> parameters with.</param>
        /// <typeparam name="TDbCommand">Then <see cref="IDbCommand"/> type to be used.</typeparam>
        public static void PopulateParameters<TDbCommand>(this TDbCommand command, Action<TDbCommand> deriveParameters, object[] parameters) where TDbCommand : IDbCommand
        {
            // tmshults 12/10/2004
            if (parameters != null)
            {
                string commandText = command.CommandText;

                if (string.IsNullOrEmpty(commandText))
                    throw new ArgumentNullException("command", "command.CommandText is null");

                if (commandText.StartsWith("SELECT ", StringComparison.CurrentCultureIgnoreCase) ||
                    commandText.StartsWith("INSERT ", StringComparison.CurrentCultureIgnoreCase) ||
                    commandText.StartsWith("UPDATE ", StringComparison.CurrentCultureIgnoreCase) ||
                    commandText.StartsWith("DELETE ", StringComparison.CurrentCultureIgnoreCase))
                {
                    // We assume the command to be of type Text if it begins with one of the common SQL keywords.
                    command.CommandType = CommandType.Text;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        command.Parameters.Add(parameters[i]);
                    }
                }
                else
                {
                    // If not we make the command a StoredProcedure type - most common use of parameterized execution.
                    command.CommandType = CommandType.StoredProcedure;

                    // Makes quick query to db to find the parameters for the StoredProc, and then creates them for
                    // the command. The DeriveParameters() only for commands with CommandType of StoredProcedure.
                    deriveParameters(command);

                    // Removes the ReturnValue Parameter.
                    command.Parameters.RemoveAt(0);

                    // Checks to see if the Parameters found match the Values provided.
                    if (command.Parameters.Count != parameters.Length)
                    {
                        // If there are more values than parameters, throws an error.
                        if (parameters.Length > command.Parameters.Count)
                            throw new ArgumentException("You have supplied more Values than Parameters listed for the Stored Procedure");

                        // Otherwise, assume that the missing values are for Parameters that have default values,
                        // and the code uses the default. To do this fill the extended ParamValue as Nothing/Null.
                        Array.Resize(ref parameters, command.Parameters.Count); // Makes the Values array match the Parameters of the Stored Proc.
                    }

                    // Assigns the values to the the Parameters.
                    for (int i = 0; i < command.Parameters.Count; i++)
                    {
                        ((DbParameter)command.Parameters[i]).Value = parameters[i];
                    }
                }
            }
        }

        #endregion

        #region [ CSV / DataTable Conversion Functions ]

        /// <summary>
        /// Converts a delimited string into a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="delimitedData">The delimited text to be converted to <see cref="DataTable"/>.</param>
        /// <param name="delimiter">The character(s) used for delimiting the text.</param>
        /// <param name="header">true, if the delimited text contains header information; otherwise, false.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable ToDataTable(this string delimitedData, string delimiter, bool header)
        {
            DataTable table = new DataTable();
            string pattern;

            // Regex pattern that will be used to split the delimited data.
            pattern = Regex.Escape(delimiter) + "(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

            // Remove any leading and trailing whitespaces, carriage returns or line feeds.
            delimitedData = delimitedData.Trim().Trim(new char[] { '\r', '\n' }).Replace("\n", "");

            string[] lines = delimitedData.Split('\r'); //Splits delimited data into lines.

            int cursor = 0;

            // Assumes that the first line has header information.
            string[] headers = Regex.Split(lines[cursor], pattern);

            // Creates columns.
            if (header)
            {
                // Uses the first row as header row.
                for (int i = 0; i < headers.Length; i++)
                {
                    table.Columns.Add(new DataColumn(headers[i].Trim(new char[] { '\"' }))); //Remove any leading and trailing quotes from the column name.
                }
                cursor++;
            }
            else
            {
                for (int i = 0; i < headers.Length; i++)
                {
                    table.Columns.Add(new DataColumn());
                }
            }

            // Populates the data table with csv data.
            for (; cursor < lines.Length; cursor++)
            {
                // Creates new row.
                DataRow row = table.NewRow();

                // Populates the new row.
                string[] fields = Regex.Split(lines[cursor], pattern);
                for (int i = 0; i < fields.Length; i++)
                {
                    // Removes any leading and trailing quotes from the data.
                    row[i] = fields[i].Trim(new char[] { '\"' });
                }

                // Adds the new row.
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Converts the <see cref="DataTable"/> to a multi-line delimited string (e.g., CSV export).
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> whose data is to be converted to delimited text.</param>
        /// <param name="delimiter">The character(s) to be used for delimiting the text.</param>
        /// <param name="quoted">true, if text is to be surrounded by quotes; otherwise, false.</param>
        /// <param name="header">true, if the delimited text should have header information.</param>
        /// <returns>A string of delimited text.</returns>
        public static string ToDelimitedString(this DataTable table, string delimiter, bool quoted, bool header)
        {
            StringBuilder data = new StringBuilder();

            //Uses the column names as the headers if headers are requested.
            if (header)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    data.Append((quoted ? "\"" : "") + table.Columns[i].ColumnName + (quoted ? "\"" : ""));

                    if (i < table.Columns.Count - 1)
                    {
                        data.Append(delimiter);
                    }
                }
                data.Append("\r\n");
            }

            for (int i = 0; i < table.Rows.Count; i++)
            {
                //Converts data table's data to delimited data.
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    data.Append((quoted ? "\"" : "") + table.Rows[i][j].ToString() + (quoted ? "\"" : ""));

                    if (j < table.Columns.Count - 1)
                    {
                        data.Append(delimiter);
                    }
                }
                data.Append("\r\n");
            }

            //Returns the delimited data.
            return data.ToString();
        }

        #endregion

        #region [ Oracle Extensions ]

        // Because of reference dependency, these should be added to a TVA.Data assembly along with MySql versions if useful
        
        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the number of rows affected.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>The number of rows affected.</returns>
        //public static int ExecuteNonQuery(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    OracleCommand command = new OracleCommand(sql, connection);
        //    command.PopulateParameters(parameters);
        //    return command.ExecuteNonQuery();
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and builds a <see cref="OracleDataReader"/>.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>A <see cref="OracleDataReader"/> object.</returns>
        //public static OracleDataReader ExecuteReader(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    return connection.ExecuteReader(sql, CommandBehavior.Default, parameters);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and builds a <see cref="OracleDataReader"/>.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>A <see cref="OracleDataReader"/> object.</returns>
        //public static OracleDataReader ExecuteReader(this OracleConnection connection, string sql, CommandBehavior behavior, params object[] parameters)
        //{
        //    OracleCommand command = new OracleCommand(sql, connection);
        //    command.PopulateParameters(parameters);
        //    return command.ExecuteReader(behavior);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the value in the first column 
        ///// of the first row in the resultset.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>Value in the first column of the first row in the resultset.</returns>
        //public static object ExecuteScalar(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    OracleCommand command = new OracleCommand(sql, connection);
        //    command.PopulateParameters(parameters);
        //    return command.ExecuteScalar();
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        //public static DataRow RetrieveRow(this OracleConnection connection, string sql)
        //{
        //    return connection.RetrieveRow(sql, null);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        //public static DataRow RetrieveRow(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    DataTable dataTable = connection.RetrieveData(sql, 0, 1, parameters);

        //    if (dataTable.Rows.Count == 0)
        //        dataTable.Rows.Add(dataTable.NewRow());

        //    return dataTable.Rows[0];
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataTable"/> 
        ///// of resultset, if the resultset contains multiple tables.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <returns>A <see cref="DataTable"/> object.</returns>
        //public static DataTable RetrieveData(this OracleConnection connection, string sql)
        //{
        //    return connection.RetrieveData(sql, 0, int.MaxValue);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataTable"/> 
        ///// of resultset, if the resultset contains multiple tables.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="startRow">The zero-based record number to start with.</param>
        ///// <param name="maxRows">The maximum number of records to retrieve.</param>
        ///// <returns>A <see cref="DataTable"/> object.</returns>
        //public static DataTable RetrieveData(this OracleConnection connection, string sql, int startRow, int maxRows)
        //{
        //    return connection.RetrieveData(sql, startRow, maxRows, null);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataTable"/> 
        ///// of resultset, if the resultset contains multiple tables.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>A <see cref="DataTable"/> object.</returns>
        //public static DataTable RetrieveData(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    return connection.RetrieveData(sql, 0, int.MaxValue, parameters);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataTable"/> 
        ///// of resultset, if the resultset contains multiple tables.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="startRow">The zero-based record number to start with.</param>
        ///// <param name="maxRows">The maximum number of records to retrieve.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>A <see cref="DataTable"/> object.</returns>
        //public static DataTable RetrieveData(this OracleConnection connection, string sql, int startRow, int maxRows, params object[] parameters)
        //{
        //    return connection.RetrieveDataSet(sql, startRow, maxRows, parameters).Tables[0];
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the <see cref="DataSet"/> that 
        ///// may contain multiple tables, depending on the SQL statement.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <returns>A <see cref="DataSet"/> object.</returns>
        //public static DataSet RetrieveDataSet(this OracleConnection connection, string sql)
        //{
        //    return connection.RetrieveDataSet(sql, 0, int.MaxValue);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the <see cref="DataSet"/> that 
        ///// may contain multiple tables, depending on the SQL statement.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="startRow">The zero-based record number to start with.</param>
        ///// <param name="maxRows">The maximum number of records to retrieve.</param>
        ///// <returns>A <see cref="DataSet"/> object.</returns>
        //public static DataSet RetrieveDataSet(this OracleConnection connection, string sql, int startRow, int maxRows)
        //{
        //    return connection.RetrieveDataSet(sql, startRow, maxRows, null);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the <see cref="DataSet"/> that 
        ///// may contain multiple tables, depending on the SQL statement.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>A <see cref="DataSet"/> object.</returns>
        //public static DataSet RetrieveDataSet(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    return connection.RetrieveDataSet(sql, 0, int.MaxValue, parameters);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the <see cref="DataSet"/> that 
        ///// may contain multiple tables, depending on the SQL statement.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="startRow">The zero-based record number to start with.</param>
        ///// <param name="maxRows">The maximum number of records to retrieve.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>A <see cref="DataSet"/> object.</returns>
        //public static DataSet RetrieveDataSet(this OracleConnection connection, string sql, int startRow, int maxRows, params object[] parameters)
        //{
        //    OracleCommand command = new OracleCommand(sql, connection);
        //    command.PopulateParameters(parameters);
        //    OracleDataAdapter dataAdapter = new OracleDataAdapter(command);
        //    DataSet data = new DataSet("Temp");
        //    dataAdapter.Fill(data, startRow, maxRows, "Table1");

        //    return data;
        //}

        ///// <summary>
        ///// Updates the underlying data of the <see cref="DataTable"/> using <see cref="OracleConnection"/>, and
        ///// returns the number of rows successfully updated.
        ///// </summary>
        ///// <param name="sourceData">The <see cref="DataTable"/> used to update the underlying data source.</param>
        ///// <param name="sourceSql">The SQL statement used initially to populate the <see cref="DataTable"/>.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for updating the underlying data source.</param>
        ///// <returns>The number of rows successfully updated from the <see cref="DataTable"/>.</returns>
        //public static int UpdateData(this OracleConnection connection, DataTable sourceData, string sourceSql)
        //{
        //    OracleDataAdapter dataAdapter = new OracleDataAdapter(sourceSql, connection);
        //    OracleCommandBuilder commandBuilder = new OracleCommandBuilder(dataAdapter);
        //    return dataAdapter.Update(sourceData);
        //}

        ///// <summary>
        /////  Takes the <see cref="OracleCommand"/> object and populates it with the given parameters.
        ///// </summary>
        ///// <param name="command">The <see cref="OracleCommand"/> whose parameters are to be populated.</param>
        ///// <param name="parameters">The parameters to populate the <see cref="OracleCommand"/> parameters with.</param>
        //public static void PopulateParameters(this OracleCommand command, object[] parameters)
        //{
        //    command.PopulateParameters(OracleCommandBuilder.DeriveParameters, parameters);
        //}

        #endregion
    }
}