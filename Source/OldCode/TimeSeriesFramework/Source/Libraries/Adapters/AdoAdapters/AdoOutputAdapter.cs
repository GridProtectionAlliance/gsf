//******************************************************************************************************
//  AdoOutputAdapter.cs - Gbtc
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
//  05/02/2011 - J. Ritchie Carroll
//       Cast ID field back to a signed integer to work with most database types per suggestion by Hugo.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using TimeSeriesFramework;
using TimeSeriesFramework.Adapters;
using TVA;

namespace AdoAdapters
{
    /// <summary>
    /// Represents an output adapter that archives measurements to a database.
    /// </summary>
    [Description("ADO: archives measurements to any ADO data source (e.g., OSI-PI via ODBC).")]
    public class AdoOutputAdapter : OutputAdapterBase
    {

        #region [ Members ]

        // Fields
        private Dictionary<string, string> m_fieldNames;
        private string m_dbTableName;
        private string m_dbConnectionString;
        private string m_dataProviderString;
        private string m_timestampFormat;
        private bool m_isJetEngine;

        private IDbConnection m_connection;
        private long m_measurementCount;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoOutputAdapter"/> class.
        /// </summary>
        public AdoOutputAdapter()
        {
            m_fieldNames = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the table name in the data source used to archive data.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the table name in the data source used to archive data."),
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
                m_isJetEngine = m_dbConnectionString.Contains("Microsoft.Jet.OLEDB");
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
        /// Gets or sets the format used to output measurement timestamps to the data source.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the date and time format used to output the timestamp to the data source."),
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
        /// Returns a flag that determines if measurements sent to this
        /// <see cref="AdoOutputAdapter"/> are destined for archival.
        /// </summary>
        public override bool OutputIsForArchive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a flag that determines if this <see cref="AdoOutputAdapter"/>
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
        /// Initializes this <see cref="AdoOutputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            Type measurementType = typeof(IMeasurement);
            IEnumerable<string> measurementProperties = measurementType.GetProperties().Select(property => property.Name);
            StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;

            // Parse database field names from the connection string.
            foreach (string key in settings.Keys)
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

            // Get table name or default to PICOMP.
            if (!settings.TryGetValue("tableName", out m_dbTableName))
                m_dbTableName = "PICOMP";

            // Get database connection string or default to empty.
            if (!settings.TryGetValue("dbConnectionString", out m_dbConnectionString))
                m_dbConnectionString = string.Empty;

            m_isJetEngine = m_dbConnectionString.Contains("Microsoft.Jet.OLEDB");

            // Get data provider string or default to a generic ODBC connection.
            if (!settings.TryGetValue("dataProviderString", out m_dataProviderString))
                m_dataProviderString = "AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.Odbc.OdbcConnection; AdapterType=System.Data.Odbc.OdbcDataAdapter";

            // Get timestamp format or default to "dd-MMM-yyyy HH:mm:ss.fff".
            if (!settings.TryGetValue("timestampFormat", out m_timestampFormat))
                m_timestampFormat = "dd-MMM-yyyy HH:mm:ss.fff";
            else
            {
                // Null timestamp format means output as ticks.
                if (m_timestampFormat.Equals("null", ignoreCase))
                    m_timestampFormat = null;
            }

            // Create a new database connection object.
            Dictionary<string, string> dataProviderSettings = m_dataProviderString.ParseKeyValuePairs();
            Assembly assm = Assembly.Load(dataProviderSettings["AssemblyName"]);
            Type connectionType = assm.GetType(dataProviderSettings["ConnectionType"]);
            m_connection = (IDbConnection)Activator.CreateInstance(connectionType);
            m_connection.ConnectionString = m_dbConnectionString;
        }

        /// <summary>
        /// Attempts to connect to this <see cref="AdoOutputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_connection.Open();
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="AdoOutputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_connection.Close();
            m_measurementCount = 0;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdoOutputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Archived {0} measurements to database.", m_measurementCount).CenterText(maxLength);
        }

        /// <summary>
        /// Archives <paramref name="measurements"/> locally.
        /// </summary>
        /// <param name="measurements">Measurements to be archived.</param>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            Type measurementType = typeof(IMeasurement);
            string commandString = "INSERT INTO {0}({1}) VALUES ({2})";

            foreach (IMeasurement measurement in measurements)
            {
                IDbCommand command = m_connection.CreateCommand();
                StringBuilder fieldList = new StringBuilder();
                StringBuilder valueList = new StringBuilder();

                // Build the field list and value list.
                foreach (string fieldName in m_fieldNames.Keys)
                {
                    IDbDataParameter parameter = command.CreateParameter();
                    string propertyName = m_fieldNames[fieldName];
                    object value = measurementType.GetProperty(propertyName).GetValue(measurement, null);

                    if (fieldList.Length > 0)
                        fieldList.Append(',');
                    fieldList.Append(fieldName);

                    if (valueList.Length > 0)
                        valueList.Append(',');
                    valueList.Append('@');
                    valueList.Append(fieldName);

                    parameter.ParameterName = "@" + fieldName;
                    parameter.Direction = ParameterDirection.Input;

                    switch (propertyName.ToLower())
                    {
                        case "id":
                            // IMeasurement.ID field is an uint, cast this back to a
                            // signed integer to work with most database field types
                            parameter.Value = Convert.ToInt32(value);
                            break;
                        case "signalid":
                            parameter.Value = m_isJetEngine ? "{" + value + "}" : value;
                            break;
                        case "timestamp":
                        case "publishedtimestamp":
                        case "receivedtimestamp":
                            Ticks timestamp = (Ticks)value;

                            // If the value is a timestamp, use the timestamp format
                            // specified by the user when inserting the timestamp.
                            if (m_timestampFormat == null)
                                parameter.Value = (long)timestamp;
                            else
                                parameter.Value = timestamp.ToString(m_timestampFormat);
                            break;
                        case "timestampqualityisgood":
                        case "valuequalityisgood":
                            parameter.Value = Convert.ToBoolean(value) ? 1 : 0;
                            break;
                        default:
                            parameter.Value = value;
                            break;
                    }

                    command.Parameters.Add(parameter);
                }

                // Set the command text and execute the command.
                command.CommandText = string.Format(commandString, m_dbTableName, fieldList, valueList);
                command.ExecuteNonQuery();
            }

            m_measurementCount += measurements.Length;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AdoOutputAdapter"/> object and optionally releases the managed resources.
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
                            m_connection.Dispose();

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

        #endregion

    }
}
