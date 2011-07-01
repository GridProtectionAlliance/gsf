//******************************************************************************************************
//  AdoInputAdapter.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  11/18/2010 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Timers;
using TimeSeriesFramework;
using TimeSeriesFramework.Adapters;
using TVA;

namespace AdoAdapters
{
    /// <summary>
    /// Represents an input adapter that reads measurements from a database table.
    /// </summary>
    [Description("ADO: reads measurements from any ADO data source (e.g., OSI-PI via ODBC).")]
    public class AdoInputAdapter : InputAdapterBase
    {

        #region [ Members ]

        // Fields
        private Dictionary<string, string> m_fieldNames;
        private string m_dbTableName;
        private string m_dbConnectionString;
        private string m_dataProviderString;
        private string m_timestampFormat;
        private int m_framesPerSecond;
        private bool m_simulateTimestamps;

        private IList<IMeasurement> m_dbMeasurements;
        private Timer m_timer;
        private int m_nextIndex;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoInputAdapter"/> class.
        /// </summary>
        public AdoInputAdapter()
        {
            m_fieldNames = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            m_dbMeasurements = new List<IMeasurement>();
            m_timer = new Timer();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the table name in the data source from which measurements are read.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the table name in the data source from which measurements are read."),
        DefaultValue("PICOMP")]
        public string TableName
        {
            get
            {
                return m_dbTableName;
            }
            set
            {
                m_dbTableName = value;
            }
        }

        /// <summary>
        /// Gets or sets the connection string used to connect to the data source.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the connection string used to connect to the data source."),
        DefaultValue("")]
        public string DbConnectionString
        {
            get
            {
                return m_dbConnectionString;
            }
            set
            {
                m_dbConnectionString = value;
            }
        }

        /// <summary>
        /// Gets or sets the data provider string used to connect to the data source.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the data provider string used to connect to the data source."),
        DefaultValue("AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.Odbc.OdbcConnection; AdapterType=System.Data.Odbc.OdbcDataAdapter")]
        public string DataProviderString
        {
            get
            {
                return m_dataProviderString;
            }
            set
            {
                m_dataProviderString = value;
            }
        }

        /// <summary>
        /// Gets or sets the format used to read measurements from the data source.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the date and time format used to read timestamps from the data source."),
        DefaultValue("dd-MMM-yyyy HH:mm:ss.fff")]
        public string TimestampFormat
        {
            get
            {
                return m_timestampFormat;
            }
            set
            {
                m_timestampFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the framerate simulated by this adapter.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the rate at which frames will be sent to the concentrator."),
        DefaultValue(30)]
        public int FramesPerSecond
        {
            get
            {
                return m_framesPerSecond;
            }
            set
            {
                m_framesPerSecond = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether the timestamps should
        /// be simulated for the purposes of real-time concentration.
        /// </summary>
        [ConnectionStringParameter,
        Description("Indicate whether timestamps should be simulated for real-time concentration."),
        DefaultValue(true)]
        public bool SimulateTimestamps
        {
            get
            {
                return m_simulateTimestamps;
            }
            set
            {
                m_simulateTimestamps = value;
            }
        }

        /// <summary>
        /// Gets a flag that determines if this <see cref="AdoInputAdapter"/>
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
        /// Initializes this <see cref="AdoInputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Get the database field names.
            GetFieldNames(settings);

            // Get table name or default to PICOMP.
            if (!settings.TryGetValue("tableName", out m_dbTableName))
                m_dbTableName = "PICOMP";

            // Get database connection string or default to empty.
            if (!settings.TryGetValue("dbConnectionString", out m_dbConnectionString))
                m_dbConnectionString = string.Empty;

            // Get data provider string or default to a generic ODBC connection.
            if (!settings.TryGetValue("dataProviderString", out m_dataProviderString))
                m_dataProviderString = "AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.Odbc.OdbcConnection; AdapterType=System.Data.Odbc.OdbcDataAdapter";

            // Get timestamp format or default to "dd-MMM-yyyy HH:mm:ss.fff".
            if (!settings.TryGetValue("timestampFormat", out m_timestampFormat))
                m_timestampFormat = "dd-MMM-yyyy HH:mm:ss.fff";
            else
            {
                // Null timestamp format means the timestamp is stored as ticks.
                if (m_timestampFormat.Equals("null", StringComparison.CurrentCultureIgnoreCase))
                    m_timestampFormat = null;
            }

            // Get framerate or default to 30.
            if (settings.TryGetValue("framesPerSecond", out setting))
                m_framesPerSecond = int.Parse(setting);
            else
                m_framesPerSecond = 30;

            // Determine whether to perform timestamp simulation; default is true.
            if (settings.TryGetValue("simulateTimestamps", out setting))
                m_simulateTimestamps = bool.Parse(setting);
            else
                m_simulateTimestamps = true;

            // Set up the timer to trigger inputs.
            m_timer.Interval = 1000.0 / m_framesPerSecond;
            m_timer.AutoReset = true;
            m_timer.Elapsed += Timer_Elapsed;

            // Get measurements from the database.
            GetDbMeasurements();
        }

        /// <summary>
        /// Attempts to connect to this <see cref="AdoInputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_timer.Start();
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="AdoInputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_timer.Stop();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdoInputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("{0} measurements read from database.", ProcessedMeasurements).CenterText(maxLength);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AdoInputAdapter"/> object and optionally releases the managed resources.
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
                        if (m_timer != null)
                        {
                            m_timer.Elapsed -= Timer_Elapsed;
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

        // Gets the database field names specified by the user in the connection string.
        private void GetFieldNames(Dictionary<string, string> settings)
        {
            IEnumerable<string> measurementProperties = typeof(IMeasurement).GetProperties().Select(property => property.Name);
            StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;

            // Parse database field names from the connection string.
            foreach (string key in Settings.Keys)
            {
                if (key.EndsWith("FieldName", ignoreCase))
                {
                    int fieldNameIndex = key.LastIndexOf("FieldName", ignoreCase);
                    string subKey = key.Substring(0, fieldNameIndex);
                    string propertyName = measurementProperties.SingleOrDefault(name => name.Equals(subKey, ignoreCase));
                    string fieldName = settings[key];

                    if (propertyName != null)
                        m_fieldNames[fieldName] = propertyName;
                    else
                        OnProcessException(new ArgumentException(string.Format("Measurement property not found: {0}", subKey)));
                }
            }

            // If the user hasn't entered any field names, enter the default field names.
            if (m_fieldNames.Count == 0)
            {
                m_fieldNames.Add("TAG", "TagName");
                m_fieldNames.Add("TIME", "Timestamp");
                m_fieldNames.Add("VALUE", "Value");
            }
        }

        // Retrieves the measurements from the database.
        private void GetDbMeasurements()
        {
            Dictionary<string, string> dataProviderSettings = m_dataProviderString.ParseKeyValuePairs();
            Assembly assm = Assembly.Load(dataProviderSettings["AssemblyName"]);
            Type connectionType = assm.GetType(dataProviderSettings["ConnectionType"]);
            IDbConnection connection = null;

            // Get measurements from the database.
            try
            {
                IDbCommand command;
                IDataReader dbReader;

                connection = (IDbConnection)Activator.CreateInstance(connectionType);
                connection.ConnectionString = m_dbConnectionString;
                connection.Open();

                command = connection.CreateCommand();
                command.CommandText = string.Format("SELECT * FROM {0}", m_dbTableName);

                dbReader = command.ExecuteReader();

                while (dbReader.Read())
                {
                    IMeasurement measurement = new Measurement();

                    foreach (string fieldName in m_fieldNames.Keys)
                    {
                        object value = dbReader[fieldName];
                        string propertyName = m_fieldNames[fieldName];

                        if (propertyName == "Timestamp")
                        {
                            // If the value is a timestamp, use the timestamp format
                            // specified by the user when reading the timestamp.
                            if (m_timestampFormat == null)
                                measurement.Timestamp = long.Parse(value.ToNonNullString());
                            else
                                measurement.Timestamp = DateTime.ParseExact(value.ToNonNullString(), m_timestampFormat, CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            PropertyInfo property = typeof(IMeasurement).GetProperty(propertyName);
                            Type propertyType = property.PropertyType;
                            Type valueType = value.GetType();

                            if (property.PropertyType.IsAssignableFrom(value.GetType()))
                                property.SetValue(measurement, value, null);
                            else if (property.PropertyType == typeof(string))
                                property.SetValue(measurement, value.ToNonNullString(), null);
                            else if (valueType == typeof(string))
                            {
                                MethodInfo parseMethod = valueType.GetMethod("Parse", new Type[] { typeof(string) });

                                if (parseMethod != null && parseMethod.IsStatic)
                                    property.SetValue(measurement, parseMethod.Invoke(null, new object[] { value }), null);
                            }
                            else
                            {
                                string exceptionMessage = string.Format("The type of field {0} could not be converted to the type of property {1}.", fieldName, propertyName);
                                OnProcessException(new InvalidCastException(exceptionMessage));
                            }
                        }
                    }

                    m_dbMeasurements.Add(measurement);
                }

                m_dbMeasurements = m_dbMeasurements.OrderBy(m => (long)m.Timestamp).ToList();
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        // Publishes the next frame of measurements.
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            long nowTicks = DateTime.UtcNow.Ticks;
            long timeTicks = m_dbMeasurements[m_nextIndex].Timestamp;
            List<IMeasurement> measurements = new List<IMeasurement>();

            while (m_nextIndex < m_dbMeasurements.Count && m_dbMeasurements[m_nextIndex].Timestamp == timeTicks)
            {
                IMeasurement clone = Measurement.Clone(m_dbMeasurements[m_nextIndex]);

                if (m_simulateTimestamps)
                    clone.Timestamp = nowTicks;

                measurements.Add(clone);
                m_nextIndex++;
            }

            if (m_nextIndex == m_dbMeasurements.Count)
                m_nextIndex = 0;

            OnNewMeasurements(measurements);
        }

        #endregion
    }
}