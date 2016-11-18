//******************************************************************************************************
//  MySqlInputAdapter.cs - Gbtc
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
//  11/13/2009 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace MySqlAdapters
{
    /// <summary>
    /// Represents an input adapter that reads measurements from a MySQL database table.
    /// </summary>
    [Description("MySQL: Reads measurements from a MySQL database")]
    public class MySqlInputAdapter : InputAdapterBase
    {

        #region [ Members ]

        // Fields
        private string m_mySqlConnectionString;
        private DbConnection m_connection;
        private Timer m_timer;
        private int m_inputInterval;
        private int m_measurementsPerInput;
        private int m_startingMeasurement;
        private bool m_fakeTimestamps;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlInputAdapter"/> class.
        /// </summary>
        public MySqlInputAdapter()
        {
            m_timer = new Timer();
            m_inputInterval = 33;
            m_measurementsPerInput = 5;
            m_fakeTimestamps = false;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the interval of time, in milliseconds, between sending frames into the concentrator.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the interval of time, in milliseconds, between sending frames into the concentrator."),
        DefaultValue(33)]
        public int InputInterval
        {
            get
            {
                return m_inputInterval;
            }
            set
            {
                m_inputInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of measurements that are read from the MySQL database in each frame.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the number of measurements that are read from the MySQL database in each frame."),
        DefaultValue(5)]
        public int MeasurementsPerInput
        {
            get
            {
                return m_measurementsPerInput;
            }
            set
            {
                m_measurementsPerInput = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that determines whether timestamps are simulated for real-time concentration.
        /// </summary>
        [ConnectionStringParameter,
        Description("Indicate whether timestamps are simulated for real-time concentration."),
        DefaultValue(false)]
        public bool FakeTimestamps
        {
            get
            {
                return m_fakeTimestamps;
            }
            set
            {
                m_fakeTimestamps = value;
            }
        }

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
        /// Gets a flag that determines if this <see cref="MySqlInputAdapter"/>
        /// uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes this <see cref="MySqlInputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            if (settings.TryGetValue("inputInterval", out setting))
                m_inputInterval = int.Parse(setting);

            if (settings.TryGetValue("measurementsPerInput", out setting))
                m_measurementsPerInput = int.Parse(setting);

            if (settings.TryGetValue("fakeTimestamps", out setting))
                m_fakeTimestamps = bool.Parse(setting);

            // Create the MySQL connection string using only the portions of the
            // original connection string that are used by MySQL
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

            // Create a new MySql connection object
            m_connection = CreateConnection();
            m_connection.ConnectionString = m_mySqlConnectionString;
            m_connection.StateChange += m_connection_StateChange;

            // Override input interval based on temporal processing interval if it's not set to default
            if (ProcessingInterval > -1)
            {
                if (ProcessingInterval == 0)
                    m_inputInterval = 1;
                else
                    m_inputInterval = ProcessingInterval;
            }

            // Set up the timer to trigger inputs
            m_timer.Interval = m_inputInterval;
            m_timer.AutoReset = true;
            m_timer.Elapsed += m_timer_Elapsed;
        }

        /// <summary>
        /// Attempts to connect to this <see cref="MySqlInputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_connection.Open();
            m_timer.Start();
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="MySqlInputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_timer.Stop();
            m_connection.Close();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="MySqlInputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("{0} measurements read from database.", ProcessedMeasurements).CenterText(maxLength);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MySqlInputAdapter"/> object and optionally releases the managed resources.
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

                        if (m_timer != null)
                        {
                            m_timer.Elapsed -= m_timer_Elapsed;
                            m_timer.Dispose();
                        }
                        m_timer = null;
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

        private void m_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<IMeasurement> measurements = new List<IMeasurement>();
            StringBuilder columnString = new StringBuilder();

            int timestampColumn = GetColumnIndex("Timestamp");
            int idColumn = GetColumnIndex("SignalID");
            int valueColumn = GetColumnIndex("Value");

            string commandString;
            IDbCommand command;
            IDataReader reader;

            foreach (string columnName in s_measurementColumns)
            {
                if (columnString.Length > 0)
                    columnString.Append(',');

                columnString.Append(columnName);
            }

            commandString = string.Format("SELECT {0} FROM Measurement LIMIT {1},{2}", columnString, m_startingMeasurement, m_measurementsPerInput);
            command = m_connection.CreateCommand();
            command.CommandText = commandString;

            using (reader = command.ExecuteReader())
            {

                while (reader.Read())
                {
                    Ticks timeStamp = m_fakeTimestamps ? new Ticks(DateTime.UtcNow) : new Ticks(reader.GetInt64(timestampColumn));
                    MeasurementKey key = MeasurementKey.LookUpBySignalID(reader.GetGuid(idColumn));
                    if (key != MeasurementKey.Undefined)
                    {
                        measurements.Add(new Measurement
                        {
                            MeasurementMetadata = key.MeasurementMetadata,
                            Value = reader.GetDouble(valueColumn),
                            Timestamp = timeStamp
                        });
                    }
                }
            }
            OnNewMeasurements(measurements);
            m_startingMeasurement += m_measurementsPerInput;
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

        // Collection of column names used in database queries.
        private static readonly string[] s_measurementColumns = { "SignalID", "Timestamp", "Value" };

        // Collection of keys that can be used in a MySQL connection string.
        private static readonly string[] s_validKeys =
        {
            "server", "port", "protocol",
            "database", "uid", "user", "pwd", "password",
            "encryption", "encrypt", "charset",
            "default command timeout", "connection timeout",
            "ignore prepare", "shared memory name"
        };

        // Static Methods

        // Gets the index of the column specified by the column name.
        private static int GetColumnIndex(string columnName)
        {
            for (int i = 0; i < s_measurementColumns.Length; i++)
            {
                if (s_measurementColumns[i] == columnName)
                    return i;
            }

            return -1;
        }

        #endregion
    }
}