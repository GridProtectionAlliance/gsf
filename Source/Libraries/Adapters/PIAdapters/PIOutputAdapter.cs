//******************************************************************************************************
//  PIOutputAdapter.cs - Gbtc
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
//  08/13/2012 - Ryan McCoy
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using PISDK;
using PISDKCommon;

namespace PIAdapters
{
    /// <summary>
    /// Exports measurements to PI if the point tag or alternate tag corresponds to a PI point's tag name.
    /// </summary>
    [Description("OSI-PI : Archives measurements to an OSI-PI server using 64-bit PI SDK")]
    public class PIOutputAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Fields
        private string m_servername;                                            // Server for PI connection
        private string m_userName;                                              // Username for PI connection
        private string m_password;                                              // Password for PI connection
        private int m_connectTimeout;                                           // PI connection timeout
        private PISDK.PISDK m_pi;                                               // PI SDK object
        private Server m_piServer;                                              // PI server object for archiving data
        private ConcurrentDictionary<MeasurementKey, PIPoint> m_tagMap;         // Cache the mapping between GSFSchema measurements and PI points
        private int m_processedMeasurements;                                    // Track the processed measurements
        private bool m_runMetadataSync;                                         // Whether or not to automatically create/update PI points on the server
        private string m_piPointSource;                                         // Point source to set on PI points when automatically created by the adapter
        private string m_piPointClass;                                          // Point class to use for new PI points when automatically created by the adapter
        private bool m_bulkUpdate;                                              // Flags whether the adapter will update each point in bulk or one update at a time
        private string m_tagMapCacheFileName;                                   // Tag map cache file name
        private DateTime m_lastMetadataRefresh;                                 // Tracks time of last meta-data refresh
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PIOutputAdapter"/>
        /// </summary>
        public PIOutputAdapter()
        {
            m_connectTimeout = 30000;
            m_tagMap = new ConcurrentDictionary<MeasurementKey, PIPoint>();
            m_processedMeasurements = 0;
            m_runMetadataSync = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Set this property true to force the adapter to send bulk updates to PI with the UpdateValues method. Set this property to false to update points one value at a time using UpdateValue method.
        /// </summary>
        [ConnectionStringParameter, Description("Set this property true to force the adapter to send bulk updates to PI with the UpdateValues method. Set this property to false to update points one value at a time using UpdateValue method.")]
        public bool BulkUpdate
        {
            get
            {
                return m_bulkUpdate;
            }
            set
            {
                m_bulkUpdate = value;
            }
        }

        /// <summary>
        /// Returns true to indicate that this <see cref="PIOutputAdapter"/> is sending measurements to a historian, OSISoft PI.
        /// </summary>
        public override bool OutputIsForArchive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns false to indicate that this <see cref="PIOutputAdapter"/> will connect synchronously
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the name of the PI server for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the name of the PI server for the adapter's PI connection.")]
        public string ServerName
        {
            get
            {
                return m_servername;
            }
            set
            {
                m_servername = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the PI user ID for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the name of the PI user ID for the adapter's PI connection."), DefaultValue("")]
        public string UserName
        {
            get
            {
                return m_userName;
            }
            set
            {
                m_userName = value;
            }
        }

        /// <summary>
        /// Gets or sets the password used for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the password used for the adapter's PI connection."), DefaultValue("")]
        public string Password
        {
            get
            {
                return m_password;
            }
            set
            {
                m_password = value;
            }
        }

        /// <summary>
        /// Gets or sets the timeout interval (in milliseconds) for the adapter's connection
        /// </summary>
        [ConnectionStringParameter, Description("Defines the timeout interval (in milliseconds) for the adapter's connection"), DefaultValue(30000)]
        public int ConnectTimeout
        {
            get
            {
                return m_connectTimeout;
            }
            set
            {
                m_connectTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this adapter should automatically manage metadata for PI points.
        /// </summary>
        [ConnectionStringParameter, Description("Determines if this adapter should automatically manage metadata for PI points (recommended)."), DefaultValue(true)]
        public bool RunMetadataSync
        {
            get
            {
                return m_runMetadataSync;
            }
            set
            {
                m_runMetadataSync = value;
            }
        }

        /// <summary>
        /// Gets or sets the point source string used when automatically creating new PI points during the metadata update
        /// </summary>
        [ConnectionStringParameter, Description("Defines the point source string used when automatically creating new PI points during the metadata update"), DefaultValue("TSF")]
        public string PIPointSource
        {
            get
            {
                return m_piPointSource;
            }
            set
            {
                m_piPointSource = value;
            }
        }

        /// <summary>
        /// Gets or sets the point class string used when automatically creating new PI points during the metadata update. On the PI server, this class should inherit from classic.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the point class string used when automatically creating new PI points during the metadata update. On the PI server, this class should inherit from classic."), DefaultValue("classic")]
        public string PIPointClass
        {
            get
            {
                return m_piPointClass;
            }
            set
            {
                m_piPointClass = value;
            }
        }

        /// <summary>
        /// Gets or sets the filename to be used for tag map cache.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the filename to be used for tag map cache file name. Leave blank for cache name to be same as adapter name with a \".cache\" extension."), DefaultValue("")]
        public string TagMapCacheFileName
        {
            get
            {
                return m_tagMapCacheFileName;
            }
            set
            {
                m_tagMapCacheFileName = value;
            }
        }
        /// <summary>
        /// Returns the detailed status of the data output source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendLine();
                status.AppendFormat("        OSI-PI server name: {0}", m_servername);
                status.AppendLine();
                status.AppendFormat("       Connected to server: {0}", (object)m_piServer == null ? "No" : m_piServer.Connected ? "Yes" : "No");
                status.AppendLine();
                status.AppendFormat("    Meta-data sync enabled: {0}", m_runMetadataSync);
                status.AppendLine();

                if (m_runMetadataSync)
                {
                    status.AppendFormat("       OSI-PI point source: {0}", m_piPointSource);
                    status.AppendLine();
                    status.AppendFormat("        OSI-PI point class: {0}", m_piPointClass);
                    status.AppendLine();
                }

                status.AppendFormat("    Using bulk tag updates: {0}", m_bulkUpdate);
                status.AppendLine();
                status.AppendFormat("   Tag-map cache file name: {0}", FilePath.TrimFileName(m_tagMapCacheFileName.ToNonNullString("[undefined]"), 51));
                status.AppendLine();
                status.AppendFormat(" Active tag-map cache size: {0:N0} mappings", m_tagMap.Count);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PIOutputAdapter"/> object and optionally releases the managed resources.
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
                        m_pi = null;
                        m_piServer = null;

                        if ((object)m_tagMap != null)
                        {
                            m_tagMap.Clear();
                            m_tagMap = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Initializes this <see cref="PIOutputAdapter"/>.
        /// </summary>
        [HandleProcessCorruptedStateExceptions]
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            if (!settings.TryGetValue("ServerName", out m_servername))
                throw new InvalidOperationException("Server name is a required setting for PI connections. Please add a server in the format server=myservername to the connection string.");

            if (settings.TryGetValue("UserName", out setting))
                m_userName = setting;
            else
                m_userName = null;

            if (settings.TryGetValue("Password", out setting))
                m_password = setting;
            else
                m_password = null;

            if (settings.TryGetValue("ConnectTimeout", out setting))
                m_connectTimeout = Convert.ToInt32(setting);
            else
                m_connectTimeout = 30000;

            if (settings.TryGetValue("RunMetadataSync", out setting))
                m_runMetadataSync = Convert.ToBoolean(setting);
            else
                m_runMetadataSync = true; // By default, assume that PI tags will be automatically maintained

            if (settings.TryGetValue("PIPointSource", out setting))
                m_piPointSource = setting;
            else
                m_piPointSource = "GSF";

            if (settings.TryGetValue("PIPointClass", out setting))
                m_piPointClass = setting;
            else
                m_piPointClass = "classic";

            if (settings.TryGetValue("BulkUpdate", out setting))
                m_bulkUpdate = Convert.ToBoolean(setting);
            else
                m_bulkUpdate = true;

            if (!settings.TryGetValue("TagCacheName", out setting) || string.IsNullOrWhiteSpace(setting))
                setting = Name + "_TagMap.cache";

            m_tagMapCacheFileName = FilePath.GetAbsolutePath(setting);

            try
            {
                // Initialize PI SDK
                m_pi = new PISDK.PISDK();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Failed to initialize OSI-PI SDK: {0}", ex.Message), ex);
            }

            try
            {
                // Locate configured PI server
                m_piServer = m_pi.Servers[m_servername];
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Failed to locate configured OSI-PI server '{0}': {1}", m_servername, ex.Message), ex);
            }
        }

        /// <summary>
        /// Connects to the configured PI server.
        /// </summary>
        [HandleProcessCorruptedStateExceptions]
        protected override void AttemptConnection()
        {
            m_tagMap.Clear();

            // PI server only allows independent connections to the same PI server from STA threads.
            // We're spinning up a thread here to connect STA, since our current thread is MTA.
            ManualResetEventSlim connectionEvent = new ManualResetEventSlim(false);

            Exception connectionException = null;

            Thread connectionThread = new Thread(() =>
            {
                try
                {
                    lock (m_piServer)
                    {
                        if (!string.IsNullOrEmpty(m_userName) && !string.IsNullOrEmpty(m_password))
                            m_piServer.Open(string.Format("UID={0};PWD={1}", m_userName, m_password));
                        else
                            m_piServer.Open();
                    }
                }
                catch (Exception ex)
                {
                    connectionException = ex;
                }
                finally
                {
                    connectionEvent.Set();
                }
            });

            connectionThread.SetApartmentState(ApartmentState.STA);
            connectionThread.Start();

            if (!connectionEvent.Wait(m_connectTimeout))
            {
                connectionThread.Abort();
                throw new TimeoutException(string.Format("Timeout occurred while attempting connection to configured OSI-PI server '{0}'.", m_servername));
            }

            if ((object)connectionException != null)
                throw new InvalidOperationException(string.Format("Failed to connect to configured OSI-PI server '{0}': {1}", m_servername, connectionException.Message), connectionException);

            // Kick off meta-data refresh
            RefreshMetadata();
        }

        /// <summary>
        /// Closes this <see cref="PIOutputAdapter"/> connections to the PI server.
        /// </summary>
        [HandleProcessCorruptedStateExceptions]
        protected override void AttemptDisconnection()
        {
            if ((object)m_piServer != null)
            {
                lock (m_piServer)
                {
                    if (m_piServer.Connected)
                        m_piServer.Close();
                }
            }
        }

        /// <summary>
        /// Sorts measurements and sends them to the configured PI server in batches
        /// </summary>
        /// <param name="measurements">Measurements to queue</param>
        [HandleProcessCorruptedStateExceptions]
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if ((object)measurements == null || measurements.Length <= 0 || (object)m_piServer == null)
                return;

            lock (m_piServer)
            {
                if (!m_piServer.Connected)
                    return;
            }

            PIValues values;
            PIPoint point;

            if (m_bulkUpdate)
            {
                // Group measurement values by ID
                Dictionary<MeasurementKey, PIValues> measurementValues = new Dictionary<MeasurementKey, PIValues>();

                foreach (IMeasurement measurement in measurements)
                {
                    if (m_tagMap.ContainsKey(measurement.Key))
                    {
                        values = measurementValues.GetOrAdd(measurement.Key, key => new PIValues()
                        {
                            ReadOnly = false
                        });

                        values.Add(new DateTime(measurement.Timestamp).ToLocalTime(), measurement.AdjustedValue, null);
                    }
                }

                foreach (MeasurementKey key in measurementValues.Keys)
                {
                    try
                    {
                        if (m_tagMap.TryGetValue(key, out point))
                        {
                            values = measurementValues[key];

                            lock (m_piServer)
                            {
                                // Add values for given PI point
                                point.Data.UpdateValues(values);
                            }

                            m_processedMeasurements += values.Count;
                        }
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(new InvalidOperationException(string.Format("Failed to archive OSI-PI point values for '{0}': {1}", key, ex.Message), ex));
                    }
                }
            }
            else
            {
                foreach (IMeasurement measurement in measurements)
                {
                    try
                    {
                        if (m_tagMap.TryGetValue(measurement.Key, out point))
                        {
                            lock (m_piServer)
                            {
                                point.Data.UpdateValue(measurement.AdjustedValue, new DateTime(measurement.Timestamp).ToLocalTime());
                            }

                            m_processedMeasurements++;
                        }
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(new InvalidOperationException(string.Format("Failed to archive OSI-PI point value for '{0}': {1}", measurement.Key, ex.Message), ex));
                    }
                }
            }
        }

        /// <summary>
        /// Sends updated metadata to PI.
        /// </summary>
        [HandleProcessCorruptedStateExceptions]
        protected override void ExecuteMetadataRefresh()
        {
            if (!Initialized || (object)m_piServer == null)
                return;

            lock (m_piServer)
            {
                if (!m_piServer.Connected)
                    return;
            }

            MeasurementKey[] inputMeasurements = InputMeasurementKeys;
            int previousMeasurementReportingInterval = MeasurementReportingInterval;

            MeasurementReportingInterval = 0;

            // Attempt to load tag-map from existing cache, if any
            LoadCachedTagMap();

            if (m_tagMap.Count > 0)
                MeasurementReportingInterval = previousMeasurementReportingInterval;

            OnStatusMessage("Beginning metadata refresh...");

            try
            {
                if ((object)inputMeasurements != null && inputMeasurements.Length > 0)
                {
                    int processed = 0, total = inputMeasurements.Length;

                    AdoDataConnection database = null;
                    PointList pointList;
                    DataTable measurements = DataSource.Tables["ActiveMeasurements"];
                    DataRow[] rows;

                    foreach (MeasurementKey key in inputMeasurements)
                    {
                        rows = measurements.Select(string.Format("SignalID='{0}'", key.SignalID));

                        if (rows.Length <= 0)
                            continue;

                        string tagname = rows[0]["PointTag"].ToNonNullString().Trim();
                        bool addedNewPoint = false;

                        if (!string.IsNullOrWhiteSpace(tagname))
                        {
                            // Use alternate tag if one is defined - note that digitals are an exception since they use this field for special labeling
                            if (!string.IsNullOrWhiteSpace(rows[0]["AlternateTag"].ToString()) && !rows[0]["SignalType"].ToString().Equals("DIGI", StringComparison.OrdinalIgnoreCase))
                                tagname = rows[0]["AlternateTag"].ToString().Trim();

                            // Attempt to lookup tag using signal ID stored in extended description field
                            lock (m_piServer)
                            {
                                pointList = m_piServer.GetPoints(string.Format("EXDESC='{0}'", rows[0]["SignalID"]));
                            }

                            if (pointList.Count == 0)
                            {
                                // Did not find tag using signal ID, see if desired point tag already exists
                                PIPoint point;

                                try
                                {
                                    lock (m_piServer)
                                    {
                                        point = m_piServer.PIPoints[tagname];
                                    }
                                }
                                catch
                                {
                                    point = null;
                                }

                                if ((object)point == null)
                                {
                                    // Attempt to add point if it doesn't exist
                                    OnStatusMessage("PI point lookup for '{0} [{1}]' by tag-name and \"EXDESC='{2}'\" returned no data, attempting to add new PI point...", tagname, key, rows[0]["SignalID"]);

                                    try
                                    {
                                        NamedValues values = new NamedValues();

                                        values.Add("pointsource", m_piPointSource);
                                        values.Add("Descriptor", rows[0]["Description"].ToString());
                                        values.Add("exdesc", rows[0]["SignalID"].ToString());
                                        values.Add("sourcetag", rows[0]["PointTag"].ToString());

                                        // TODO: Add this field to the ActiveMeasurement view
                                        if (measurements.Columns.Contains("EngineeringUnits")) // engineering units is a new field for this view -- handle the case that it's not there
                                            values.Add("engunits", rows[0]["EngineeringUnits"].ToString());

                                        lock (m_piServer)
                                        {
                                            m_piServer.PIPoints.Add(tagname, m_piPointClass, PointTypeConstants.pttypFloat32, values);
                                        }

                                        addedNewPoint = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        OnProcessException(new InvalidOperationException(string.Format("Failed to add PI tag '{0}' for measurement '{1}': {2}", tagname, key, ex.Message), ex));
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    // Rename tag-name if needed
                                    if (string.CompareOrdinal(pointList[1].Name, tagname) != 0)
                                    {
                                        lock (m_piServer)
                                        {
                                            m_piServer.PIPoints.Rename(pointList[1].Name, tagname);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    OnProcessException(new InvalidOperationException(string.Format("Failed to rename PI tag '{0}' to '{1}' for measurement '{2}': {3}", pointList[1].Name, tagname, key, ex.Message), ex));
                                }
                            }

                            if (!addedNewPoint)
                            {
                                DateTime updatedOn;

                                try
                                {
                                    // Attempt to lookup last update time for record
                                    if ((object)database == null)
                                        database = new AdoDataConnection("systemSettings");

                                    updatedOn = Convert.ToDateTime(database.Connection.ExecuteScalar(string.Format("SELECT UpdatedOn FROM Measurement WHERE SignalID = '{0}'", key.SignalID)));
                                }
                                catch (Exception)
                                {
                                    updatedOn = DateTime.UtcNow;
                                }

                                // Only refresh meta-data if record has been updated since last refresh
                                if (updatedOn > m_lastMetadataRefresh)
                                {
                                    try
                                    {
                                        // Update tag meta-data
                                        PIErrors errors;
                                        NamedValues edits = new NamedValues();
                                        NamedValues edit = new NamedValues();

                                        edit.Add("pointsource", m_piPointSource);
                                        edit.Add("Descriptor", rows[0]["Description"].ToString());
                                        edit.Add("exdesc", rows[0]["SignalID"].ToString());
                                        edit.Add("sourcetag", rows[0]["PointTag"].ToString());

                                        // TODO: Add this field to the ActiveMeasurement view
                                        if (measurements.Columns.Contains("EngineeringUnits")) // engineering units is a new field for this view -- handle the case that it's not there
                                            edit.Add("engunits", rows[0]["EngineeringUnits"].ToString());

                                        edits.Add(rows[0]["PointTag"].ToString(), edit);

                                        lock (m_piServer)
                                        {
                                            IPIPoints2 pts2 = (IPIPoints2)m_piServer.PIPoints;
                                            pts2.EditTags(edits, out errors);
                                        }

                                        if (errors.Count > 0)
                                        {
                                            StringBuilder description = new StringBuilder();

                                            foreach (dynamic error in errors)
                                            {
                                                if (description.Length > 0)
                                                    description.AppendLine();

                                                description.Append(error.Description);
                                            }

                                            OnStatusMessage("[WARNING] Error(s) reported during update of PI tag '{0}' metadata from measurement '{1}': {2}", tagname, key, description.ToString());
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        OnProcessException(new InvalidOperationException(string.Format("Failed to update PI tag '{0}' metadata from measurement '{1}': {2}", tagname, key, ex.Message), ex));
                                    }
                                }
                            }
                        }

                        processed++;

                        if (processed % 100 == 0)
                            OnStatusMessage("Updated {0:N0} PI tags and associated metadata, {1:0.00%} complete...", processed, processed / (double)total);
                    }

                    if ((object)database != null)
                        database.Dispose();
                }
                else
                {
                    OnStatusMessage("OSI-PI historian output adapter is not configured to receive any input measurements - metadata refresh cancelled.");
                }
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }

            m_lastMetadataRefresh = DateTime.UtcNow;

            OnStatusMessage("Completed metadata refresh successfully.");

            // Map measurements to point tags and re-establish tag-map cache
            LoadTagMap(inputMeasurements);

            // Restore original measurement reporting interval
            MeasurementReportingInterval = previousMeasurementReportingInterval;
        }

        private void LoadCachedTagMap()
        {
            // Map measurements to any existing point tags to start archiving what is possible right away...
            if (File.Exists(m_tagMapCacheFileName))
            {
                // Attempt to load tag-map from existing cache file
                try
                {
                    using (FileStream tagMapCache = File.Open(m_tagMapCacheFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        Dictionary<MeasurementKey, string> tagNames = Serialization.Deserialize<Dictionary<MeasurementKey, string>>(tagMapCache, GSF.SerializationFormat.Binary);
                        PIPoint point;

                        OnStatusMessage("Mapping measurements to PI tags using cached tag-map...");

                        int processed = 0, total = tagNames.Count;

                        foreach (KeyValuePair<MeasurementKey, string> kvp in tagNames)
                        {
                            try
                            {
                                lock (m_piServer)
                                {
                                    point = m_piServer.PIPoints[kvp.Value];
                                }

                                m_tagMap.AddOrUpdate(kvp.Key, point, (k, v) => point);
                            }
                            catch (Exception ex)
                            {
                                OnProcessException(new InvalidOperationException(string.Format("Failed to map '{0}' to a PI tag '{1}' loaded from tag-map cache: {2}", kvp.Key, kvp.Value, ex.Message), ex));
                            }

                            processed++;

                            if (processed % 100 == 0)
                                OnStatusMessage("Mapped {0:N0} PI tags to measurements using tag-map cache, {1:0.00%} complete...", processed, processed / (double)total);
                        }

                        OnStatusMessage("Mapped {0:N0} keys to points successfully using tag-map cache .", tagNames.Count);

                        // Use last write time of file as the last meta-data refresh time
                        m_lastMetadataRefresh = File.GetLastWriteTimeUtc(m_tagMapCacheFileName);
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException(string.Format("Failed to map measurements from cached tag-map to file '{0}': {1}", m_tagMapCacheFileName, ex.Message), ex));
                }
            }

        }
        // Resets the PI tag to MeasurementKey mapping for loading data into PI by finding PI points that match either the GSFSchema point tag or alternate tag
        [HandleProcessCorruptedStateExceptions]
        private void LoadTagMap(MeasurementKey[] inputMeasurements)
        {
            OnStatusMessage("Mapping measurements to PI tags...");

            List<MeasurementKey> newTags = new List<MeasurementKey>();

            if ((object)inputMeasurements != null && inputMeasurements.Length > 0)
            {
                int processed = 0, total = inputMeasurements.Length;

                foreach (MeasurementKey key in inputMeasurements)
                {
                    try
                    {
                        DataRow[] rows = DataSource.Tables["ActiveMeasurements"].Select(string.Format("SignalID='{0}'", key.SignalID));

                        if (rows.Length > 0)
                        {
                            string tagname = rows[0]["PointTag"].ToNonNullString().Trim();

                            // Use alternate tag if one is defined
                            if (!string.IsNullOrWhiteSpace(rows[0]["AlternateTag"].ToString()) && !rows[0]["SignalType"].ToString().Equals("DIGI", StringComparison.OrdinalIgnoreCase))
                                tagname = rows[0]["AlternateTag"].ToString().Trim();

                            // Two ways to find points here
                            // 1. if we are running metadata sync from the adapter, look for the signal ID in the exdesc field
                            // 2. if the pi points are being manually maintained, look for either the point tag or alternate tag in the actual pi point tag
                            PointList points;
                            bool foundPoint = false;

                            // Attempt lookup by EXDESC signal ID
                            if (m_runMetadataSync)
                            {
                                lock (m_piServer)
                                {
                                    points = m_piServer.GetPoints(string.Format("EXDESC='{0}'", key.SignalID));
                                }

                                if ((object)points != null && points.Count > 0)
                                {
                                    m_tagMap.AddOrUpdate(key, points[1], (k, v) => points[1]);
                                    newTags.Add(key);
                                    foundPoint = true;
                                }
                            }

                            if (!foundPoint)
                            {
                                PIPoint point;

                                try
                                {
                                    lock (m_piServer)
                                    {
                                        point = m_piServer.PIPoints[tagname];
                                    }

                                    m_tagMap.AddOrUpdate(key, point, (k, v) => point);
                                    newTags.Add(key);
                                }
                                catch
                                {
                                    point = null;
                                }

                                if ((object)point == null)
                                {
                                    lock (m_piServer)
                                    {
                                        points = m_piServer.GetPoints(string.Format("TAG='{0}'", tagname));
                                    }

                                    if ((object)points != null && points.Count > 0)
                                    {
                                        // NOTE - The PointList is NOT 0-based (index 1 is the first item in points)
                                        m_tagMap.AddOrUpdate(key, points[1], (k, v) => points[1]);
                                        newTags.Add(key);
                                    }
                                    else
                                    {
                                        OnStatusMessage("No PI points found for tag '{0}'. Data will not be archived for '{1}'.", tagname, key);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(new InvalidOperationException(string.Format("Failed to map '{0}' to a PI tag: {1}", key, ex.Message), ex));
                    }

                    processed++;

                    if (processed % 100 == 0)
                        OnStatusMessage("Mapped {0:N0} PI tags to measurements, {1:0.00%} complete...", newTags.Count, processed / (double)total);
                }
            }

            if (newTags.Count > 0)
            {
                OnStatusMessage("Mapped {0:N0} PI tags to measurements, checking for removed tags...", newTags.Count);

                // Determine which tags no longer exist
                PIPoint removedPoint;
                HashSet<MeasurementKey> tagsToRemove = new HashSet<MeasurementKey>(m_tagMap.Keys);

                // If there are existing tags that are not part of new updates, these need to be removed
                tagsToRemove.ExceptWith(newTags);

                if (tagsToRemove.Count > 0)
                {
                    foreach (MeasurementKey key in tagsToRemove)
                        m_tagMap.TryRemove(key, out removedPoint);

                    OnStatusMessage("Detected {0:N0} tags that have been removed from OSI-PI output - primary tag-map has been updated...", tagsToRemove.Count);
                }

                if (m_tagMap.Count > 0)
                {
                    // Cache tag-map for faster future PI adapter startup
                    try
                    {
                        OnStatusMessage("Caching tag-map for faster future loads...");

                        using (FileStream tagMapCache = File.Create(m_tagMapCacheFileName))
                        {
                            Stream fileStream = tagMapCache;
                            Dictionary<MeasurementKey, string> tagNames = m_tagMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Name);
                            Serialization.Serialize(tagNames, GSF.SerializationFormat.Binary, ref fileStream);
                        }

                        OnStatusMessage("Tag-map cached for faster future loads.");
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(new InvalidOperationException(string.Format("Failed to cache tag-map to file '{0}': {1}", m_tagMapCacheFileName, ex.Message), ex));
                    }

                    OnStatusMessage("Mapped {0:N0} PI tags to measurements successfully.", m_tagMap.Count);
                }
                else
                {
                    OnStatusMessage("[WARNING] No PI tags were mapped to measurements - no tag-map exists so no points can be archived.");
                }
            }
            else
            {
                if (m_tagMap.Count > 0)
                    OnStatusMessage("[WARNING] No PI tags were mapped to measurements - existing tag-map with {0:N0} tags remains in use.", m_tagMap.Count);
                else
                    OnStatusMessage("[WARNING] No PI tags were mapped to measurements - no tag-map exists so no points can be archived.");
            }
        }

        /// <summary>
        /// Returns a brief status of this <see cref="PIOutputAdapter"/>
        /// </summary>
        /// <param name="maxLength">Maximum number of characters in the status string</param>
        /// <returns>Status</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Archived {0:N0} measurements to PI.", m_processedMeasurements).CenterText(maxLength);
        }

        #endregion
    }
}
