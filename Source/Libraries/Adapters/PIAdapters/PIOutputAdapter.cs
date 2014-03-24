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
using System.Threading;
using GSF;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using PISDK;
using PISDKCommon;

namespace PIAdapters
{
    /// <summary>
    /// Exports measurements to PI if the point tag or alternate tag corresponds to a PI point's tag name.
    /// </summary>
    [Description("PI : Archives measurements to a PI server using PI SDK")]
    public class PIOutputAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Fields
        private string m_servername;                                            // Server for PI connection
        private string m_userName;                                              // Username for PI connection
        private string m_password;                                              // Password for PI connection
        private int m_connectTimeout;                                           // PI connection timeout
        private PISDK.PISDK m_pi;                                               // PI SDK object
        private Server m_server;                                                // PI server object for archiving data
        private ConcurrentDictionary<MeasurementKey, PIPoint> m_tagMap;         // Cache the mapping between GSFSchema measurements and PI points
        private int m_processedMeasurements;                                    // Track the processed measurements
        private readonly LongSynchronizedOperation m_metadataRefreshOperation;  // Operation that handles metadata refresh
        private bool m_runMetadataSync;                                         // Whether or not to automatically create/update PI points on the server
        private string m_piPointSource;                                         // Point source to set on PI points when automatically created by the adapter
        private string m_piPointClass;                                          // Point class to use for new PI points when automatically created by the adapter
        private bool m_bulkUpdate;                                              // Flags whether the adapter will update each point in bulk or one update at a time
        private string m_tagMapCacheFileName;                                   // Tag map cache file name
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PIOutputAdapter"/>
        /// </summary>
        public PIOutputAdapter()
        {
            m_connectTimeout = 30000;
            m_metadataRefreshOperation = new LongSynchronizedOperation(ExecuteMetadataRefresh) { IsBackground = true };
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
                        m_server = null;

                        if (m_tagMap != null)
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
        }

        /// <summary>
        /// Connects to the configured PI server.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_pi = new PISDK.PISDK();
            m_server = m_pi.Servers[m_servername];

            // PI server only allows independent connections to the same PI server from STA threads.
            // We're spinning up a thread here to connect STA, since our current thread is MTA.
            ManualResetEvent connectionEvent = new ManualResetEvent(false);
            Thread connectThread = new Thread(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(m_userName) && !string.IsNullOrEmpty(m_password))
                        m_server.Open(string.Format("UID={0};PWD={1}", m_userName, m_password));
                    else
                        m_server.Open();
                    connectionEvent.Set();
                }
                catch (Exception e)
                {
                    OnProcessException(e);
                }
            });
            connectThread.SetApartmentState(ApartmentState.STA);
            connectThread.Start();

            if (!connectionEvent.WaitOne(m_connectTimeout))
            {
                connectThread.Abort();
                throw new InvalidOperationException("Timeout occurred while connecting to configured PI server.");
            }

            // Kick off meta-data refresh - if this is deployed as a custom adapter this doesn't happen automatically
            RefreshMetadata();
        }

        /// <summary>
        /// Closes this <see cref="PIOutputAdapter"/> connections to the PI server.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if (m_server != null && m_server.Connected)
                m_server.Close();
        }

        /// <summary>
        /// Sorts measurements and sends them to the configured PI server in batches
        /// </summary>
        /// <param name="measurements">Measurements to queue</param>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if (measurements != null)
            {
                if (m_bulkUpdate)
                {
                    Dictionary<MeasurementKey, PIValues> values = new Dictionary<MeasurementKey, PIValues>();

                    foreach (IMeasurement measurement in measurements)
                    {
                        if (m_tagMap.ContainsKey(measurement.Key))
                        {
                            if (!values.ContainsKey(measurement.Key))
                            {
                                values.Add(measurement.Key, new PIValues());
                                values[measurement.Key].ReadOnly = false;
                            }

                            values[measurement.Key].Add(new DateTime(measurement.Timestamp).ToLocalTime(), measurement.AdjustedValue, null);
                        }

                        m_processedMeasurements++;
                    }

                    foreach (MeasurementKey key in values.Keys)
                    {
                        try
                        {
                            // If the key isn't in the dictionary, something has gone wrong finding this point in PI
                            if (m_tagMap.ContainsKey(key))
                                m_tagMap[key].Data.UpdateValues(values[key]);
                        }
                        catch (Exception e)
                        {
                            OnProcessException(e);
                        }
                    }
                }
                else
                {
                    foreach (IMeasurement measurement in measurements)
                    {
                        if (m_tagMap.ContainsKey(measurement.Key))
                            m_tagMap[measurement.Key].Data.UpdateValue(measurement.AdjustedValue, new DateTime(measurement.Timestamp).ToLocalTime());

                        m_processedMeasurements++;
                    }
                }
            }
        }

        /// <summary>
        /// Resets the PI tag to MeasurementKey mapping for loading data into PI by finding PI points that match either the GSFSchema pointtag or alternatetag
        /// </summary>
        private void MapKeysToPoints()
        {
            OnStatusMessage("Mapping measurements to PI tags...");

            List<MeasurementKey> newTags = new List<MeasurementKey>();
            int mapCount = 0;

            if ((object)InputMeasurementKeys != null && InputMeasurementKeys.Any())
            {
                int processed = 0, total = InputMeasurementKeys.Length;

                foreach (MeasurementKey key in InputMeasurementKeys)
                {
                    try
                    {
                        DataRow[] rows = DataSource.Tables["ActiveMeasurements"].Select(string.Format("SignalID='{0}'", key.SignalID));

                        if (rows.Length > 0)
                        {
                            string tagname = rows[0]["PointTag"].ToString();

                            // Use alternate tag if one is defined
                            if (!string.IsNullOrWhiteSpace(rows[0]["AlternateTag"].ToString()) && !rows[0]["AlternateTag"].ToString().Equals("DIGI", StringComparison.OrdinalIgnoreCase))
                                tagname = rows[0]["AlternateTag"].ToString();

                            PointList points;

                            // Two ways to find points here
                            // 1. if we are running metadata sync from the adapter, look for the signal ID in the exdesc field
                            // 2. if the pi points are being manually maintained, look for either the point tag or alternate tag in the actual pi point tag
                            string filter = !m_runMetadataSync ? string.Format("TAG='{0}'", tagname) : string.Format("EXDESC='{0}'", key.SignalID);

                            points = m_server.GetPoints(filter);

                            if ((object)points == null || points.Count == 0)
                            {
                                OnStatusMessage("No PI points found with {0}. Data will not be archived for signal {1}.", new object[] { filter, rows[0]["SIGNALID"] });
                            }
                            else
                            {
                                if (points.Count > 1)
                                    OnStatusMessage("Multiple PI points were found with tag-name matching '{0}' or '{1}' for signal {2}. The first match will be used.", new[] { rows[0]["POINTTAG"], rows[0]["ALTERNATETAG"], rows[0]["SIGNALID"] });

                                m_tagMap.AddOrUpdate(key, points[1], (k, v) => points[1]); // NOTE - The PointList is NOT 0-based (index 1 is the first item in points)
                                newTags.Add(key);
                                mapCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(new InvalidOperationException(string.Format("Failed to map '{0}' to a PI tag: {1}", key, ex.Message), ex));
                    }

                    processed++;

                    if (processed % 100 == 0)
                        OnStatusMessage("Mapped {0} PI tags to measurements, {1:0.00%} complete...", processed, processed / (double)total);
                }
            }

            // Determine which tags no longer exist
            List<MeasurementKey> tagsToRemove = new List<MeasurementKey>();
            PIPoint removedPoint;

            newTags.Sort();

            foreach (MeasurementKey key in m_tagMap.Keys)
            {
                // If existing tags exist that are not part of new updates, these need to be removed
                if (newTags.BinarySearch(key) < 0)
                    tagsToRemove.Add(key);
            }

            foreach (MeasurementKey key in tagsToRemove)
            {
                m_tagMap.TryRemove(key, out removedPoint);
            }

            // Cache tag-map for faster future PI adapter startup
            try
            {
                OnStatusMessage("Caching tag-map for faster future loads...");

                using (FileStream tagMapCache = File.Create(m_tagMapCacheFileName))
                {
                    Stream fileStream = tagMapCache;
                    Dictionary<MeasurementKey, string> tagNames = new Dictionary<MeasurementKey, string>();

                    foreach (KeyValuePair<MeasurementKey, PIPoint> kvp in m_tagMap)
                    {
                        tagNames.Add(kvp.Key, kvp.Value.Name);
                    }

                    Serialization.Serialize(tagNames, GSF.SerializationFormat.Binary, ref fileStream);
                }

                OnStatusMessage("Tag-map cached for faster future loads.");
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Failed to cache tag-map to file '{0}': {1}", m_tagMapCacheFileName, ex.Message), ex));
            }

            OnStatusMessage("Mapped {0} keys to points successfully.", mapCount);
        }

        /// <summary>
        /// Refreshes metadata using all available and enabled providers.
        /// </summary>
        [AdapterCommand("Sends updated metadata to PI", "Administrator", "Editor")]
        public override void RefreshMetadata()
        {
            if (m_runMetadataSync)
                m_metadataRefreshOperation.RunOnceAsync();
        }

        private void ExecuteMetadataRefresh()
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
                                point = m_server.PIPoints[kvp.Value];
                                m_tagMap.AddOrUpdate(kvp.Key, point, (k, v) => point);
                            }
                            catch (Exception ex)
                            {
                                OnProcessException(new InvalidOperationException(string.Format("Failed to map '{0}' to a PI tag loaded from tag-map cache: {1}", kvp.Key, ex.Message), ex));
                            }

                            processed++;

                            if (processed % 100 == 0)
                                OnStatusMessage("Mapped {0} PI tags to measurements using tag-map cache, {1:0.00%} complete...", processed, processed / (double)total);
                        }

                        OnStatusMessage("Mapped {0} keys to points successfully using  tag-map cache .", tagNames.Count);
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException(string.Format("Failed to map measurements from cached tag-map to file '{0}': {1}", m_tagMapCacheFileName, ex.Message), ex));
                    MapKeysToPoints();
                }
            }
            else
            {
                MapKeysToPoints();
            }

            OnStatusMessage("Beginning metadata refresh...");

            try
            {
                base.RefreshMetadata();

                if (InputMeasurementKeys != null && InputMeasurementKeys.Any())
                {
                    int processed = 0, total = InputMeasurementKeys.Length;

                    IPIPoints2 pts2 = (IPIPoints2)m_server.PIPoints;
                    PointList piPoints;
                    DataTable measurements = DataSource.Tables["ActiveMeasurements"];
                    DataRow[] rows;

                    foreach (MeasurementKey key in InputMeasurementKeys)
                    {
                        try
                        {
                            rows = measurements.Select(string.Format("SignalID='{0}'", key.SignalID));

                            if (rows.Length > 0)
                            {
                                string tagname = rows[0]["PointTag"].ToString();

                                // Use alternate tag if one is defined
                                if (!string.IsNullOrWhiteSpace(rows[0]["AlternateTag"].ToString()) && !rows[0]["AlternateTag"].ToString().Equals("DIGI", StringComparison.OrdinalIgnoreCase))
                                    tagname = rows[0]["AlternateTag"].ToString();

                                piPoints = m_server.GetPoints(string.Format("EXDESC='{0}'", rows[0]["SignalID"]));

                                if (piPoints.Count == 0)
                                {
                                    m_server.PIPoints.Add(tagname, m_piPointClass, PointTypeConstants.pttypFloat32);
                                }
                                else if (piPoints[1].Name != tagname)
                                {
                                    pts2.Rename(piPoints[1].Name, tagname);
                                }
                                else
                                {
                                    foreach (PIPoint pt in piPoints)
                                    {
                                        if (pt.Name != rows[0]["PointTag"].ToString())
                                            pts2.Rename(pt.Name, tagname);
                                    }
                                }

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
                                pts2.EditTags(edits, out errors);

                                if (errors.Count > 0)
                                    OnStatusMessage(errors[0].Description);
                            }
                        }
                        catch (Exception ex)
                        {
                            OnProcessException(new InvalidOperationException(string.Format("Failed to update PI tag metadata from measurement '{0}': {1}", key, ex.Message), ex));
                        }

                        processed++;

                        if (processed % 100 == 0)
                            OnStatusMessage("Updated {0} PI tags and associated metadata, {1:0.00%} complete...", processed, processed / (double)total);
                    }
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

            OnStatusMessage("Completed metadata refresh successfully.");

            // Re-map measurements to point tags in case any new tags were added
            MapKeysToPoints();
        }

        /// <summary>
        /// Returns a brief status of this <see cref="PIOutputAdapter"/>
        /// </summary>
        /// <param name="maxLength">Maximum number of characters in the status string</param>
        /// <returns>Status</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Archived {0} measurements to PI.", m_processedMeasurements).CenterText(maxLength);
        }

        #endregion
    }
}
