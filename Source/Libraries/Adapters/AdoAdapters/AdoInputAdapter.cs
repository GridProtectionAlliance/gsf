//******************************************************************************************************
//  AdoInputAdapter.cs - Gbtc
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
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using GSF;
using GSF.IO;
using GSF.Parsing;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Transport;
using SerializationFormat = GSF.SerializationFormat;

namespace AdoAdapters
{
    /// <summary>
    /// Represents an input adapter that reads measurements from a database table.
    /// </summary>
    [Description("ADO: Reads measurements from an ADO data source")]
    public class AdoInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Fields
        private readonly Dictionary<string, string> m_fieldNames;
        private string m_dbTableName;
        private string m_dbConnectionString;
        private string m_dataProviderString;
        private string m_timestampFormat;
        private int m_framesPerSecond;
        private bool m_simulateTimestamps;
        private IList<IMeasurement> m_dbMeasurements;
        private string m_cacheFileName;
        private int m_nextIndex;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoInputAdapter"/> class.
        /// </summary>
        public AdoInputAdapter()
        {
            m_fieldNames = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            m_dbMeasurements = new List<IMeasurement>();
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
        /// Gets or sets a cache file name so that when defined, future data loads will be from cache instead of the database as an optimization.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines a cache file name so that when defined, future data loads will be from cache instead of the database as an optimization."),
        DefaultValue(null),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.FileDialogEditor", "type=save; defaultExt=.ado; filter=ADO Cache Files|*.ado|All Files|*.*")]
        public string CacheFileName
        {
            get
            {
                return m_cacheFileName;
            }
            set
            {
                m_cacheFileName = value;
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

            // Override frames per second based on temporal processing interval if it's not set to default
            if (ProcessingInterval > -1)
            {
                if (ProcessingInterval == 0)
                {
                    m_framesPerSecond = (int)Ticks.PerSecond;
                }
                else
                {
                    // Minimum processing rate for this class is one frame per second
                    if (ProcessingInterval >= 1000)
                        m_framesPerSecond = 1;
                    else
                        m_framesPerSecond = (int)Ticks.PerMillisecond / ProcessingInterval;
                }
            }

            // Determine whether to perform timestamp simulation; default is true.
            if (settings.TryGetValue("simulateTimestamps", out setting))
                m_simulateTimestamps = bool.Parse(setting);
            else
                m_simulateTimestamps = true;

            // Load cache file name, if defined
            settings.TryGetValue("cacheFileName", out m_cacheFileName);

            if (m_cacheFileName != null)
                m_cacheFileName = FilePath.GetAbsolutePath(m_cacheFileName);

            try
            {
                // Get measurements from the database.
                ThreadPool.QueueUserWorkItem(GetDbMeasurements);
            }
            catch (Exception ex)
            {
                // Process exception for logging
                OnProcessException(new InvalidOperationException("Failed to queue database query due to exception: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// Attempts to connect to this <see cref="AdoInputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            //m_timer.Start();
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="AdoInputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            //m_timer.Stop();
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

        // Gets the database field names specified by the user in the connection string.
        private void GetFieldNames(Dictionary<string, string> settings)
        {
            IEnumerable<string> measurementProperties = GetAllProperties(typeof(IMeasurement)).Select(property => property.Name);
            StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;

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
        private void GetDbMeasurements(object state)
        {
            IDbConnection connection = null;

            // Get measurements from the database.
            try
            {
                SignalIndexCache signalIndexCache = new SignalIndexCache();
                CompactMeasurement measurement;
                long startTime = DateTime.UtcNow.Ticks;

                if (m_cacheFileName != null && File.Exists(m_cacheFileName))
                {
                    OnStatusMessage("Loading cached input data...");

                    try
                    {
                        using (FileStream data = File.OpenRead(m_cacheFileName))
                        {
                            byte[] buffer = new byte[4];
                            int signalIndexCacheImageSize;
                            int compactMeasurementSize;
                            int totalMeasurements;

                            // Read the signal index cache image size from the file
                            if (data.Read(buffer, 0, 4) != 4)
                                throw new EndOfStreamException();

                            signalIndexCacheImageSize = LittleEndian.ToInt32(buffer, 0);

                            // Resize buffer to accomodate exact signal index cache
                            buffer = new byte[signalIndexCacheImageSize];

                            // Read the signal index cache image from the file
                            if (data.Read(buffer, 0, signalIndexCacheImageSize) != signalIndexCacheImageSize)
                                throw new EndOfStreamException();

                            // Deserialize the signal index cache
                            signalIndexCache = Serialization.Deserialize<SignalIndexCache>(buffer, SerializationFormat.Binary);

                            // Read the size of each compact measurement from the file
                            if (data.Read(buffer, 0, 4) != 4)
                                throw new EndOfStreamException();

                            compactMeasurementSize = LittleEndian.ToInt32(buffer, 0);

                            // Read the total number of compact measurements from the file
                            if (data.Read(buffer, 0, 4) != 4)
                                throw new EndOfStreamException();

                            totalMeasurements = LittleEndian.ToInt32(buffer, 0);

                            // Resize buffer to accomodate compact measurement if needed (not likely)
                            if (buffer.Length < compactMeasurementSize)
                                buffer = new byte[compactMeasurementSize];

                            // Read each compact measurement image from the file
                            for (int i = 0; i < totalMeasurements; i++)
                            {
                                if (data.Read(buffer, 0, compactMeasurementSize) != compactMeasurementSize)
                                    throw new EndOfStreamException();

                                // Parse compact measurement
                                measurement = new CompactMeasurement(signalIndexCache);
                                measurement.ParseBinaryImage(buffer, 0, compactMeasurementSize);

                                m_dbMeasurements.Add(measurement);

                                if (m_dbMeasurements.Count % 50000 == 0)
                                    OnStatusMessage("Loaded {0} records so far...", m_dbMeasurements.Count);
                            }

                            OnStatusMessage("Completed data load in {0}", ((Ticks)(DateTime.UtcNow.Ticks - startTime)).ToElapsedTimeString(2));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is EndOfStreamException)
                            throw (EndOfStreamException)ex;

                        throw new EndOfStreamException(ex.Message, ex);
                    }
                }
                else
                {
                    OnStatusMessage("Loading database input data...");

                    const string MeasurementTable = "ActiveMeasurements";

                    Dictionary<string, string> dataProviderSettings = m_dataProviderString.ParseKeyValuePairs();
                    Assembly assm = Assembly.Load(dataProviderSettings["AssemblyName"]);
                    Type connectionType = assm.GetType(dataProviderSettings["ConnectionType"]);

                    Dictionary<Guid, MeasurementKey> lookupCache = new Dictionary<Guid, MeasurementKey>();
                    IDbCommand command;
                    IDataReader dbReader;
                    MeasurementKey key;
                    Guid id;
                    ushort index = 0;

                    connection = (IDbConnection)Activator.CreateInstance(connectionType);
                    connection.ConnectionString = m_dbConnectionString;
                    connection.Open();

                    command = connection.CreateCommand();
                    command.CommandText = string.Format("SELECT * FROM {0}", m_dbTableName);

                    using (dbReader = command.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            measurement = new CompactMeasurement(signalIndexCache);

                            foreach (string fieldName in m_fieldNames.Keys)
                            {
                                object value = dbReader[fieldName];
                                string propertyName = m_fieldNames[fieldName];

                                switch (propertyName)
                                {
                                    case "Timestamp":
                                        // If the value is a timestamp, use the timestamp format
                                        // specified by the user when reading the timestamp.
                                        if (m_timestampFormat == null)
                                            measurement.Timestamp = long.Parse(value.ToNonNullString());
                                        else
                                            measurement.Timestamp = DateTime.ParseExact(value.ToNonNullString(), m_timestampFormat, CultureInfo.CurrentCulture);
                                        break;
                                    case "ID":
                                        if (Guid.TryParse(value.ToString(), out id))
                                        {
                                            if (!lookupCache.TryGetValue(id, out key))
                                            {
                                                if (DataSource.Tables.Contains(MeasurementTable))
                                                {
                                                    DataRow[] filteredRows = DataSource.Tables[MeasurementTable].Select(string.Format("SignalID = '{0}'", id));

                                                    if (filteredRows.Length > 0)
                                                        key = MeasurementKey.LookUpOrCreate(id, filteredRows[0]["ID"].ToString());
                                                }

                                                if (key != MeasurementKey.Undefined)
                                                {
                                                    // Cache measurement key associated with ID
                                                    lookupCache[id] = key;

                                                    // Assign a runtime index optimization for distinct measurements
                                                    signalIndexCache.Reference.TryAdd(index++, new Tuple<Guid, string, uint>(id, key.Source, key.ID));
                                                }
                                            }

                                            measurement.CommonMeasurementFields = key.CommonMeasurementFields;
                                        }
                                        break;
                                    case "Key":
                                        if (MeasurementKey.TryParse(value.ToString(), out key))
                                        {
                                            if (!lookupCache.ContainsKey(key.SignalID))
                                            {
                                                // Cache measurement key associated with ID
                                                lookupCache[key.SignalID] = key;

                                                // Assign a runtime index optimization for distinct measurements
                                                signalIndexCache.Reference.TryAdd(index++, new Tuple<Guid, string, uint>(key.SignalID, key.Source, key.ID));
                                            }

                                            measurement.CommonMeasurementFields = key.CommonMeasurementFields;
                                        }
                                        break;
                                    case "Value":
                                        measurement.Value = Convert.ToDouble(value);
                                        break;
                                    default:
                                        PropertyInfo property = GetAllProperties(typeof(IMeasurement)).FirstOrDefault(propertyInfo => propertyInfo.Name == propertyName);

                                        if (property != null)
                                        {
                                            Type propertyType = property.PropertyType;
                                            Type valueType = value.GetType();

                                            if (property.PropertyType.IsAssignableFrom(value.GetType()))
                                            {
                                                property.SetValue(measurement, value, null);
                                            }
                                            else if (property.PropertyType == typeof(string))
                                            {
                                                property.SetValue(measurement, value.ToNonNullString(), null);
                                            }
                                            else if (valueType == typeof(string))
                                            {
                                                MethodInfo parseMethod = propertyType.GetMethod("Parse", new[] { typeof(string) });

                                                if (parseMethod != null && parseMethod.IsStatic)
                                                    property.SetValue(measurement, parseMethod.Invoke(null, new[] { value }), null);
                                            }
                                            else
                                            {
                                                string exceptionMessage = string.Format("The type of field {0} could not be converted to the type of property {1}.", fieldName, propertyName);
                                                OnProcessException(new InvalidCastException(exceptionMessage));
                                            }
                                        }
                                        else
                                        {
                                            string exceptionMessage = string.Format("The type of field {0} could not be converted to the type of property {1} - no property match was found.", fieldName, propertyName);
                                            OnProcessException(new InvalidCastException(exceptionMessage));
                                        }
                                        break;
                                }

                                m_dbMeasurements.Add(measurement);

                                if (m_dbMeasurements.Count % 50000 == 0)
                                    OnStatusMessage("Loaded {0} records so far...", m_dbMeasurements.Count);
                            }
                        }
                    }

                    OnStatusMessage("Sorting data by time...");

                    m_dbMeasurements = m_dbMeasurements.OrderBy(m => (long)m.Timestamp).ToList();

                    OnStatusMessage("Completed data load in {0}", ((Ticks)(DateTime.UtcNow.Ticks - startTime)).ToElapsedTimeString(2));

                    if (m_cacheFileName != null)
                    {
                        OnStatusMessage("Caching data for next initialization...");

                        using (FileStream data = File.OpenWrite(m_cacheFileName))
                        {
                            byte[] signalIndexCacheImage = Serialization.Serialize(signalIndexCache, SerializationFormat.Binary);
                            int compactMeasurementSize = (new CompactMeasurement(signalIndexCache)).BinaryLength;

                            // Write the signal index cache image size to the file
                            data.Write(LittleEndian.GetBytes(signalIndexCacheImage.Length), 0, 4);

                            // Write the signal index cache image to the file
                            data.Write(signalIndexCacheImage, 0, signalIndexCacheImage.Length);

                            // Write the size of each compact measurement to the file
                            data.Write(LittleEndian.GetBytes(compactMeasurementSize), 0, 4);

                            // Write the total number of compact measurements to the file
                            data.Write(LittleEndian.GetBytes(m_dbMeasurements.Count), 0, 4);

                            // Write each compact measurement image to the file
                            for (int i = 0; i < m_dbMeasurements.Count; i++)
                            {
                                ((ISupportBinaryImage)m_dbMeasurements[i]).CopyBinaryImageToStream(data);
                            }
                        }
                    }
                }

                OnStatusMessage("Entering data read cycle...");
                ThreadPool.QueueUserWorkItem(PublishData);
            }
            catch (EndOfStreamException ex)
            {
                OnProcessException(new EndOfStreamException(string.Format("Failed load cached data from {0} due to file corruption{1} cache will be recreated from database", m_cacheFileName, string.IsNullOrWhiteSpace(ex.Message) ? "," : ": " + ex.Message + " - ")));

                // If the cached file is corrupt, delete it and load from the database
                if (File.Exists(m_cacheFileName))
                    File.Delete(m_cacheFileName);

                m_dbMeasurements.Clear();
                GetDbMeasurements(null);
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Failed during data load: " + ex.Message, ex));
            }
            finally
            {

                if (connection != null)
                    connection.Close();
            }
        }

        private PropertyInfo[] GetAllProperties(Type type)
        {
            List<Type> typeList = new List<Type>();
            typeList.Add(type);

            if (type.IsInterface)
            {
                typeList.AddRange(type.GetInterfaces());
            }

            List<PropertyInfo> propertyList = new List<PropertyInfo>();

            foreach (Type interfaceType in typeList)
            {
                foreach (PropertyInfo property in interfaceType.GetProperties())
                {
                    propertyList.Add(property);
                }
            }

            return propertyList.ToArray();
        }

        // Publishes the frame measurements.
        private void PublishData(object state)
        {
            long now, currentDataTime, publicationTime = 0;
            long ticksPerFrame = Ticks.PerSecond / m_framesPerSecond;
            long toleranceWindow = ticksPerFrame / 2;

            while (Enabled)
            {
                List<IMeasurement> measurements = new List<IMeasurement>();
                now = DateTime.UtcNow.Ticks;

                // See if it is time to publish
                if (now - publicationTime >= ticksPerFrame)
                {
                    // Initialize publication time
                    if (publicationTime == 0)
                        publicationTime = ((Ticks)now).BaselinedTimestamp(BaselineTimeInterval.Second);
                    else
                        publicationTime += ticksPerFrame;

                    currentDataTime = m_dbMeasurements[m_nextIndex].Timestamp;

                    // Prepare next frame of data for all measurements with the current time
                    while (m_nextIndex < m_dbMeasurements.Count && Math.Abs((long)m_dbMeasurements[m_nextIndex].Timestamp - currentDataTime) < toleranceWindow)
                    {
                        // Clone the measurement so we can safely update the timestamp
                        IMeasurement measurement = Measurement.Clone(m_dbMeasurements[m_nextIndex]);

                        if (m_simulateTimestamps)
                            measurement.Timestamp = publicationTime;

                        measurements.Add(measurement);
                        m_nextIndex++;
                    }

                    OnNewMeasurements(measurements);

                    // Prepare index for next check, time moving forward
                    if (m_nextIndex == m_dbMeasurements.Count)
                        m_nextIndex = 0;
                }

                Thread.Sleep(1);
            }
        }

        #endregion
    }
}