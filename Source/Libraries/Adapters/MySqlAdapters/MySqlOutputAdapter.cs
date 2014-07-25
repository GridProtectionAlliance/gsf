//******************************************************************************************************
//  MySqlOutputAdapter.cs - Gbtc
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
//  10/27/2009 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace MySqlAdapters
{
    /// <summary>
    /// Represents an output adapter that archives measurements to a MySQL database table.
    /// </summary>
    [Description("MySQL: Archives measurements to a MySQL database.")]
    public class MySqlOutputAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Fields
        private string m_mySqlConnectionString;
        private DbConnection m_connection;
        private long m_measurementCount;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlOutputAdapter"/> class.
        /// </summary>
        public MySqlOutputAdapter()
        {
            m_mySqlConnectionString = null;
            m_measurementCount = 0;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns a connection string containing only the key-value pairs
        /// that are used to connect to MySQL.
        /// </summary>
        public string MySqlConnectionString
        {
            get
            {
                return m_mySqlConnectionString;
            }
        }

        /// <summary>
        /// Returns a flag that determines if measurements sent to this
        /// <see cref="MySqlOutputAdapter"/> are destined for archival.
        /// </summary>
        public override bool OutputIsForArchive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a flag that determines if this <see cref="MySqlOutputAdapter"/>
        /// uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes this <see cref="MySqlOutputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Create the MySQL connection string using only the portions of the
            // original connection string that are used by MySQL.
            StringBuilder builder = new StringBuilder();
            string[] pairs = ConnectionString.Split(';');

            foreach (string pair in pairs)
            {
                string key = pair.ToLower().Split('=')[0].Trim();

                if (s_validKeys.Contains(key))
                {
                    builder.Append(pair);
                    builder.Append(';');
                }
            }

            m_mySqlConnectionString = builder.ToString();

            // Create a new MySQL connection object
            m_connection = CreateConnection();
            m_connection.ConnectionString = m_mySqlConnectionString;
            m_connection.StateChange += m_connection_StateChange;
        }

        /// <summary>
        /// Attempts to connect to this <see cref="MySqlOutputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_connection.Open();
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="MySqlOutputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_connection.Close();
            m_measurementCount = 0;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="MySqlOutputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Archived {0} measurements to MySQL database.", m_measurementCount).CenterText(maxLength);
        }

        /// <summary>
        /// Archives <paramref name="measurements"/> locally.
        /// </summary>
        /// <param name="measurements">Measurements to be archived.</param>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if ((object)measurements != null)
            {
                foreach (IMeasurement measurement in measurements)
                {
                    // Create the command string to insert the measurement as a record in the table.
                    StringBuilder commandString = new StringBuilder("INSERT INTO Measurement VALUES ('");
                    IDbCommand command = m_connection.CreateCommand();

                    commandString.Append(measurement.ID);
                    commandString.Append("','");
                    commandString.Append((long)measurement.Timestamp);
                    commandString.Append("',");
                    commandString.Append(measurement.AdjustedValue);
                    commandString.Append(')');

                    command.CommandText = commandString.ToString();
                    command.ExecuteNonQuery();

                }
                m_measurementCount += measurements.Length;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MySqlOutputAdapter"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_connection != null)
                        {
                            m_connection.StateChange -= m_connection_StateChange;
                            m_connection.Dispose();
                        }
                        m_connection = null;
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        private DbConnection CreateConnection()
        {
            string[] mySQLConnectorNetVersions = { "6.3.6.0", "6.3.4.0", "6.2.4.0", "6.1.5.0", "6.0.7.0", "5.2.7.0", "5.1.7.0", "5.0.9.0" };
            string assemblyNameFormat = "MySql.Data, Version={0}, Culture=neutral, PublicKeyToken=c5687fc88969c44d";
            string assemblyName;

            // Attempt to load latest version of the MySQL connector net to creator the proper data provider string
            foreach (string connectorNetVersion in mySQLConnectorNetVersions)
            {
                try
                {
                    Assembly mySqlAssembly;
                    Type connectionType;

                    // Create an assembly name based on this version of the MySQL Connector/NET
                    assemblyName = string.Format(assemblyNameFormat, connectorNetVersion);

                    // See if this version of the MySQL Connector/NET can be loaded
                    mySqlAssembly = Assembly.Load(new AssemblyName(assemblyName));

                    // If assembly load succeeded, create a valid data provider string
                    connectionType = mySqlAssembly.GetType("MySql.Data.MySqlClient.MySqlConnection");
                    return (DbConnection)Activator.CreateInstance(connectionType);
                }
                catch
                {
                    // Nothing to do but try next version
                }
            }

            return null;
        }

        private void m_connection_StateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Closed && Enabled)
            {
                // Connection lost,
                // attempt to reconnect
                Start();
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields

        // Collection of keys that can be used in a MySQL connection string.
        private static readonly string[] s_validKeys = 
        {
            "server", "port", "protocol",
            "database", "uid", "user", "pwd", "password",
            "encryption", "encrypt", "charset",
            "default command timeout", "connection timeout",
            "ignore prepare", "shared memory name"
        };

        #endregion
    }
}
