//******************************************************************************************************
//  AdoOutputAdapter.cs - Gbtc
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
//  11/18/2010 - Stephen C. Wills
//       Generated original version of source code.
//  05/02/2011 - J. Ritchie Carroll
//       Cast ID field back to a signed integer to work with most database types per suggestion by Hugo.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace AdoAdapters
{
    /// <summary>
    /// Represents an output adapter that archives measurements to a database.
    /// </summary>
    [Description("ADO: Archives measurements to an ADO data source")]
    public class AdoOutputAdapter : OutputAdapterBase
    {

        #region [ Members ]

        // Fields
        private readonly Dictionary<string, string> m_fieldNames;
        private readonly List<string> m_fieldList;
        private string m_dbTableName;
        private string m_dbConnectionString;
        private string m_dataProviderString;
        private string m_timestampFormat;
        private int m_bulkInsertLimit;
        private bool m_isJetEngine;
        private bool m_isOracle;

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
            m_fieldList = new List<string>();
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
        /// Gets or sets the maximum number of measurements to be collated into one insert statement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the maximum number of measurements to be collated into one insert statement."),
        DefaultValue("1024")]
        public int BulkInsertLimit
        {
            get
            {
                return m_bulkInsertLimit;
            }
            set
            {
                m_bulkInsertLimit = value;
            }
        }

        /// <summary>
        /// Returns a flag that determines if measurements sent to this
        /// <see cref="AdoOutputAdapter"/> are destined for archival.
        /// </summary>
        public override bool OutputIsForArchive => true;

        /// <summary>
        /// Gets a flag that determines if this <see cref="AdoOutputAdapter"/>
        /// uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect => false;

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
            IEnumerable<string> measurementProperties = GetAllProperties(measurementType).Select(property => property.Name);
            StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;
            string setting;

            // Parse database field names from the connection string.
            foreach (string key in settings.Keys)
            {
                if (key.EndsWith("FieldName", ignoreCase))
                {
                    int fieldNameIndex = key.LastIndexOf("FieldName", ignoreCase);
                    string subKey = key.Substring(0, fieldNameIndex);
                    string propertyName = measurementProperties.FirstOrDefault(name => name.Equals(subKey, ignoreCase));
                    string fieldName = settings[key];

                    if (propertyName != null)
                    {
                        m_fieldNames[fieldName] = propertyName;
                        m_fieldList.Add(fieldName);
                    }
                    else
                    {
                        OnProcessException(MessageLevel.Warning, new ArgumentException($"Measurement property not found: {subKey}"));
                    }
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

            // Get bulk insert limit
            if (settings.TryGetValue("bulkInsertLimit", out setting))
                m_bulkInsertLimit = int.Parse(setting);
            else
                m_bulkInsertLimit = 1024;

            // Create a new database connection object.
            Dictionary<string, string> dataProviderSettings = m_dataProviderString.ParseKeyValuePairs();
            Assembly assm = Assembly.Load(dataProviderSettings["AssemblyName"]);
            Type connectionType = assm.GetType(dataProviderSettings["ConnectionType"]);
            m_connection = (IDbConnection)Activator.CreateInstance(connectionType);
            m_connection.ConnectionString = m_dbConnectionString;
            m_isOracle = m_connection.GetType().Name == "OracleConnection";
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
            return $"Archived {m_measurementCount} measurements to database.".CenterText(maxLength);
        }

        /// <summary>
        /// Archives <paramref name="measurements"/> locally.
        /// </summary>
        /// <param name="measurements">Measurements to be archived.</param>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if ((object)measurements != null)
            {
                for (int i = 0; i < measurements.Length; i += m_bulkInsertLimit)
                {
                    BulkInsert(measurements.Skip(i).Take(m_bulkInsertLimit));
                }
            }
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

        private PropertyInfo[] GetAllProperties(Type type)
        {
            List<Type> typeList = new List<Type>();
            typeList.Add(type);

            if (type.IsInterface)
                typeList.AddRange(type.GetInterfaces());

            List<PropertyInfo> propertyList = new List<PropertyInfo>();

            foreach (Type interfaceType in typeList)
            {
                foreach (PropertyInfo property in interfaceType.GetProperties())
                    propertyList.Add(property);
            }

            return propertyList.ToArray();
        }

        private void BulkInsert(IEnumerable<IMeasurement> measurements)
        {
            Type measurementType = typeof(IMeasurement);

            IDbCommand command = null;
            StringBuilder commandBuilder = new StringBuilder();
            string insertFormat = "INSERT INTO {0} ({1}) ";
            string selectFormat = "SELECT {0} ";
            string unionFormat = "UNION ALL ";

            StringBuilder valuesBuilder;
            string fields = m_fieldList.Aggregate((field1, field2) => field1 + "," + field2);
            string values;

            IDbDataParameter parameter = null;
            char paramChar = m_isOracle ? ':' : '@';
            int paramCount = 0;

            try
            {
                command = m_connection.CreateCommand();
                commandBuilder.Append(string.Format(insertFormat, m_dbTableName, fields));

                foreach (IMeasurement measurement in measurements)
                {
                    valuesBuilder = new StringBuilder();

                    // Build the values list.
                    foreach (string fieldName in m_fieldList)
                    {
                        string propertyName = m_fieldNames[fieldName];
                        PropertyInfo firstOrDefault = GetAllProperties(measurementType).FirstOrDefault(prop => prop.Name == propertyName);

                        if ((object)firstOrDefault != null)
                        {
                            object value = firstOrDefault.GetValue(measurement, null);

                            if (valuesBuilder.Length > 0)
                                valuesBuilder.Append(',');

                            if ((object)value == null)
                            {
                                valuesBuilder.Append("NULL");
                                continue;
                            }

                            valuesBuilder.Append(paramChar);
                            valuesBuilder.Append('p');
                            valuesBuilder.Append(paramCount);

                            parameter = command.CreateParameter();
                            parameter.ParameterName = paramChar + "p" + paramCount;
                            parameter.Direction = ParameterDirection.Input;

                            switch (propertyName.ToLower())
                            {
                                case "id":
                                    parameter.Value = m_isJetEngine ? "{" + value + "}" : value;
                                    break;
                                case "key":
                                    parameter.Value = value.ToString();
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
                                case "stateflags":
                                    // IMeasurement.StateFlags field is an uint, cast this back to a
                                    // signed integer to work with most database field types
                                    parameter.Value = Convert.ToInt32(value);
                                    break;
                                default:
                                    parameter.Value = value;
                                    break;
                            }
                        }

                        command.Parameters.Add(parameter);
                        paramCount++;
                    }

                    values = valuesBuilder.ToString();
                    commandBuilder.Append(string.Format(selectFormat, values));
                    commandBuilder.Append(unionFormat);
                }

                // Remove "UNION ALL " from the end of the command text.
                commandBuilder.Remove(commandBuilder.Length - unionFormat.Length, unionFormat.Length);

                // Set the command text and execute the command.
                command.CommandText = commandBuilder.ToString();
                command.ExecuteNonQuery();

                m_measurementCount += measurements.Count();
            }
            finally
            {
                if ((object)command != null)
                    command.Dispose();
            }
        }

        #endregion

    }
}