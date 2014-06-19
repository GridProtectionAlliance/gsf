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
//  06/18/2014 - J. Ritchie Carroll
//       Updated code to use pooled PIConnection instances.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using PISDK;
using PISDKCommon;
using ConnectionPoint = System.Tuple<PIAdapters.PIConnection, PISDK.PIPoint>;

namespace PIAdapters
{
    /// <summary>
    /// Exports measurements to PI if the point tag or alternate tag corresponds to a PI point's tag name.
    /// </summary>
    [Description("OSI-PI : Archives measurements to an OSI-PI server using 64-bit PI SDK")]
    public class PIOutputAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="PointsPerConnection"/>.
        /// </summary>
        public const int DefaultPointsPerConnection = 20;

        // Fields

        // Cached mapping between GSFSchema measurements and PI points
        private readonly ConcurrentDictionary<MeasurementKey, ConnectionPoint> m_mappedConnectionPoints;

        private readonly ShortSynchronizedOperation m_restartConnection;    // Restart connection operation
        private readonly List<PIConnection> m_connectionPool;               // PI server connection object pool for data writes
        private readonly HashSet<MeasurementKey> m_pendingMappings;         // List of pending measurement mappings
        private PIConnection m_connection;                                  // PI server connection for meta-data synchronization
        private string m_serverName;                                        // Server for PI connection
        private string m_userName;                                          // Username for PI connection
        private string m_password;                                          // Password for PI connection
        private int m_connectTimeout;                                       // PI connection timeout
        private int m_processedMeasurements;                                // Track the processed measurements
        private bool m_runMetadataSync;                                     // Whether or not to automatically create/update PI points on the server
        private string m_pointSource;                                       // Point source to set on PI points when automatically created by the adapter
        private string m_pointClass;                                        // Point class to use for new PI points when automatically created by the adapter
        private int m_pointsPerConnection;                                  // Number of points each PI connection is set to handle
        private int m_currentConnectionPoints;                              // Total number of measurements mapped to current pool item
        private DateTime m_lastMetadataRefresh;                             // Tracks time of last meta-data refresh
        private int m_processedMappings;                                    // Total number of mappings processed so far
        private bool m_refreshingMetadata;                                  // Flag that determines if meta-data is currently refreshing
        private bool m_disposed;                                            // Flag that determines if class is disposed

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PIOutputAdapter"/>
        /// </summary>
        public PIOutputAdapter()
        {
            m_pointsPerConnection = DefaultPointsPerConnection;
            m_mappedConnectionPoints = new ConcurrentDictionary<MeasurementKey, ConnectionPoint>();
            m_restartConnection = new ShortSynchronizedOperation(Start);
            m_connectionPool = new List<PIConnection>();
            m_pendingMappings = new HashSet<MeasurementKey>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets total number of points a single <see cref="PIConnection"/> will handle for archiving data.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the total number of points a single PI connection will handle for archiving data. This variable is dependent upon the sample rate of incoming data, i.e., if sample rate is low then points per connection can be high."), DefaultValue(DefaultPointsPerConnection)]
        public int PointsPerConnection
        {
            get
            {
                return m_pointsPerConnection;
            }
            set
            {
                m_pointsPerConnection = value;
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
                return m_serverName;
            }
            set
            {
                m_serverName = value;
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
        [ConnectionStringParameter, Description("Defines the point source string used when automatically creating new PI points during the metadata update"), DefaultValue("GSF")]
        public string PIPointSource
        {
            get
            {
                return m_pointSource;
            }
            set
            {
                m_pointSource = value;
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
                return m_pointClass;
            }
            set
            {
                m_pointClass = value;
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
                status.AppendFormat("        OSI-PI server name: {0}", m_serverName);
                status.AppendLine();
                status.AppendFormat("       Connected to server: {0}", (object)m_connection == null ? "No" : m_connection.Connected ? "Yes" : "No");
                status.AppendLine();
                status.AppendFormat("    Meta-data sync enabled: {0}", m_runMetadataSync);
                status.AppendLine();

                if (m_runMetadataSync)
                {
                    status.AppendFormat("       OSI-PI point source: {0}", m_pointSource);
                    status.AppendLine();
                    status.AppendFormat("        OSI-PI point class: {0}", m_pointClass);
                    status.AppendLine();
                }

                status.AppendFormat(" Active tag-map cache size: {0:N0} mappings", m_mappedConnectionPoints.Count);
                status.AppendLine();
                status.AppendFormat("      Pending tag-mappings: {0:N0} mappings, {1:0.00%} complete", m_pendingMappings.Count, 1.0D - m_pendingMappings.Count / (double)m_mappedConnectionPoints.Count);
                status.AppendLine();
                status.AppendFormat("   PI connection pool size: {0:N0}", m_connectionPool.Count);
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
                        if ((object)m_connection != null)
                        {
                            m_connection.Disconnected -= m_connection_Disconnected;
                            m_connection.Dispose();
                            m_connection = null;
                        }

                        if ((object)m_connectionPool != null)
                        {
                            foreach (PIConnection connection in m_connectionPool)
                            {
                                connection.Disconnected -= m_connection_Disconnected;
                                connection.Dispose();
                            }

                            m_connectionPool.Clear();
                        }

                        if ((object)m_mappedConnectionPoints != null)
                            m_mappedConnectionPoints.Clear();
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
        /// Returns a brief status of this <see cref="PIOutputAdapter"/>
        /// </summary>
        /// <param name="maxLength">Maximum number of characters in the status string</param>
        /// <returns>Status</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Archived {0:N0} measurements to PI.", m_processedMeasurements).CenterText(maxLength);
        }

        /// <summary>
        /// Initializes this <see cref="PIOutputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            if (!settings.TryGetValue("ServerName", out m_serverName))
                throw new InvalidOperationException("Server name is a required setting for PI connections. Please add a server in the format servername=myservername to the connection string.");

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
                m_pointSource = setting;
            else
                m_pointSource = "GSF";

            if (settings.TryGetValue("PIPointClass", out setting))
                m_pointClass = setting;
            else
                m_pointClass = "classic";
        }

        /// <summary>
        /// Connects to the configured PI server.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_processedMappings = 0;

            m_connection = new PIConnection
            {
                ServerName = m_serverName,
                UserName = m_userName,
                Password = m_password,
                ConnectTimeout = m_connectTimeout
            };

            m_connection.Disconnected += m_connection_Disconnected;
            m_connection.Open();

            // Kick off meta-data refresh
            RefreshMetadata();
        }

        /// <summary>
        /// Closes this <see cref="PIOutputAdapter"/> connections to the PI server.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_mappedConnectionPoints.Clear();

            lock (m_connectionPool)
            {
                foreach (PIConnection connection in m_connectionPool)
                {
                    connection.Disconnected -= m_connection_Disconnected;
                    connection.Dispose();
                }

                m_connectionPool.Clear();
            }

            if ((object)m_connection != null)
            {
                m_connection.Disconnected -= m_connection_Disconnected;
                m_connection.Dispose();
                m_connection = null;
            }
        }

        /// <summary>
        /// Sorts measurements and sends them to the configured PI server in batches
        /// </summary>
        /// <param name="measurements">Measurements to queue</param>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if ((object)measurements == null || measurements.Length <= 0 || (object)m_connection == null)
                return;

            // We use dictionaries to create a grouping construct whereby each PI connection will have its own set of points and associated values
            // so that archive operations for each PI connection can be processed in parallel
            Dictionary<PIConnection, Dictionary<PIPoint, PIValues>> connectionPointValues = new Dictionary<PIConnection, Dictionary<PIPoint, PIValues>>();
            ConnectionPoint connectionPoint;

            foreach (IMeasurement measurement in measurements)
            {
                try
                {
                    // Lookup connection point mapping for this measurement
                    if (!m_mappedConnectionPoints.TryGetValue(measurement.Key, out connectionPoint))
                        continue;

                    // If connection point is not defined, kick off process to create a new mapping
                    if ((object)connectionPoint == null)
                    {
                        lock (m_pendingMappings)
                        {
                            // Only start mapping process if one is not already pending
                            if (!m_pendingMappings.Contains(measurement.Key))
                            {
                                // No mapping is defined for this point, get a connection from the
                                // server pool and establish a mapping for this point
                                m_pendingMappings.Add(measurement.Key);
                                ThreadPool.QueueUserWorkItem(EstablishConnectionPointMapping, measurement.Key);
                            }
                        }
                    }
                    else
                    {
                        // Get PI values collection for this connection point
                        Dictionary<PIPoint, PIValues> pointValues = connectionPointValues.GetOrAdd(connectionPoint.Item1, connection => new Dictionary<PIPoint, PIValues>());
                        PIValues values = pointValues.GetOrAdd(connectionPoint.Item2, point => new PIValues()
                        {
                            ReadOnly = false
                        });

                        // Add measurement value to PI values collection
                        values.Add(new DateTime(measurement.Timestamp).ToLocalTime(), measurement.AdjustedValue, null);
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException(string.Format("Failed to collate measurement value into OSI-PI point value collection for '{0}': {1}", measurement.Key, ex.Message), ex));
                }
            }

            // Handle inserts for each PI connection in parallel
            Parallel.ForEach(connectionPointValues, connectionPointValue =>
            {
                PIConnection connection = connectionPointValue.Key;
                Dictionary<PIPoint, PIValues> pointValues = connectionPointValue.Value;

                foreach (KeyValuePair<PIPoint, PIValues> pointValue in pointValues)
                {
                    PIPoint point = pointValue.Key;
                    PIValues values = pointValue.Value;

                    try
                    {
                        // Add values for current PI point
                        connection.Execute(server => point.Data.UpdateValues(values));
                        Interlocked.Add(ref m_processedMeasurements, values.Count);
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(new InvalidOperationException(string.Format("Failed to archive OSI-PI point values tag '{0}': {1}", point.Name, ex.Message), ex));
                    }
                }
            });
        }

        private void EstablishConnectionPointMapping(object state)
        {
            MeasurementKey key = (MeasurementKey)state;
            ConnectionPoint connectionPoint;

            try
            {
                connectionPoint = CreateMappedConnectionPoint(key);

                if ((object)connectionPoint == null)
                    m_mappedConnectionPoints.TryRemove(key, out connectionPoint);
                else
                    m_mappedConnectionPoints[key] = connectionPoint;
            }
            catch (Exception ex)
            {
                m_mappedConnectionPoints.TryRemove(key, out connectionPoint);
                OnProcessException(new InvalidOperationException(string.Format("Failed to create connection point mapping for '{0}': {1}", key, ex.Message), ex));
            }
            finally
            {
                lock (m_pendingMappings)
                {
                    m_pendingMappings.Remove(key);
                }

                // Provide some level of feed back on progress of mapping process
                m_processedMappings++;

                if (m_processedMappings % 100 == 0)
                    OnStatusMessage("Mapped {0:N0} PI tags to measurements, {1:0.00%} complete...", m_mappedConnectionPoints.Count - m_pendingMappings.Count, 1.0D - m_pendingMappings.Count / (double)m_mappedConnectionPoints.Count);
            }
        }

        private ConnectionPoint CreateMappedConnectionPoint(MeasurementKey key)
        {
            PIConnection connection;
            PIPoint point = null;

            lock (m_connectionPool)
            {
                // We dynamically allocate pooled PI server connections each handling a maximum number of points.
                // PI's threading model can handle many connections each archiving a small volume of points, but
                // falls behind under load when archiving a large volume of points from a single connection.
                if (m_currentConnectionPoints <= m_pointsPerConnection && m_connectionPool.Count > 0)
                {
                    // Last connection has points available
                    connection = m_connectionPool[m_connectionPool.Count - 1];
                }
                else
                {
                    // Create a new connection
                    connection = new PIConnection
                    {
                        ServerName = m_serverName,
                        UserName = m_userName,
                        Password = m_password,
                        ConnectTimeout = m_connectTimeout
                    };

                    // Since PI doesn't detection disconnection until an operation is attempted,
                    // we must monitor for disconnections from the pooled connections as well
                    connection.Disconnected += m_connection_Disconnected;
                    connection.Open();

                    // Add the new connection to the server pool
                    m_connectionPool.Add(connection);

                    // Reset current connection point count
                    m_currentConnectionPoints = 0;
                }
            }

            // Increment current connection point count
            m_currentConnectionPoints++;

            // Map measurement to PI point
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
                    PointList points = null;
                    bool foundPoint = false;

                    // Attempt lookup by EXDESC signal ID
                    if (m_runMetadataSync)
                    {
                        connection.Execute(server => points = server.GetPoints(string.Format("EXDESC='{0}'", key.SignalID)));

                        if ((object)points != null && points.Count > 0)
                        {
                            point = points[1];
                            foundPoint = true;
                        }
                    }

                    if (!foundPoint)
                    {
                        try
                        {
                            connection.Execute(server => point = server.PIPoints[tagname]);
                        }
                        catch
                        {
                            point = null;
                        }

                        if ((object)point == null)
                        {
                            connection.Execute(server => points = server.GetPoints(string.Format("TAG='{0}'", tagname)));

                            if ((object)points != null && points.Count > 0)
                            {
                                point = points[1];
                            }
                            else
                            {
                                point = null;
                                m_currentConnectionPoints--;

                                if (!m_refreshingMetadata)
                                    OnStatusMessage("[WARNING] No PI points found for tag '{0}'. Data will not be archived for '{1}'.", tagname, key);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Failed to map '{0}' to a PI tag: {1}", key, ex.Message), ex));
            }

            // If no point could be mapped - return null connection mapping so key can be removed from tag-map
            if ((object)point == null)
                return null;

            return new ConnectionPoint(connection, point);
        }

        /// <summary>
        /// Sends updated metadata to PI.
        /// </summary>
        protected override void ExecuteMetadataRefresh()
        {
            if (!Initialized || (object)m_connection == null)
                return;

            if (!m_connection.Connected)
                return;

            MeasurementKey[] inputMeasurements = InputMeasurementKeys;

            // Establish initial tag-map (much of the meta-data may already exist)
            LoadTagMap(inputMeasurements);

            if (!m_runMetadataSync)
                return;

            try
            {
                OnStatusMessage("Beginning metadata refresh...");
                m_refreshingMetadata = true;

                if ((object)inputMeasurements != null && inputMeasurements.Length > 0)
                {
                    int processed = 0, total = inputMeasurements.Length;

                    AdoDataConnection database = null;
                    PointList pointList = null;
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
                            string signalID = rows[0]["SignalID"].ToNonNullString("undefined");
                            m_connection.Execute(server => pointList = server.GetPoints(string.Format("EXDESC='{0}'", signalID)));

                            if ((object)pointList == null || pointList.Count == 0)
                            {
                                // Did not find tag using signal ID, see if desired point tag already exists
                                PIPoint point = null;

                                try
                                {
                                    m_connection.Execute(server => point = server.PIPoints[tagname]);
                                }
                                catch
                                {
                                    point = null;
                                }

                                if ((object)point == null)
                                {
                                    // Attempt to add point if it doesn't exist
                                    try
                                    {
                                        NamedValues values = new NamedValues();

                                        values.Add("pointsource", m_pointSource);
                                        values.Add("Descriptor", rows[0]["Description"].ToString());
                                        values.Add("exdesc", rows[0]["SignalID"].ToString());
                                        values.Add("sourcetag", rows[0]["PointTag"].ToString());

                                        // Engineering units is a new field for this view -- handle the case that it's not there
                                        if (measurements.Columns.Contains("EngineeringUnits"))
                                            values.Add("engunits", rows[0]["EngineeringUnits"].ToString());

                                        m_connection.Execute(server => server.PIPoints.Add(tagname, m_pointClass, PointTypeConstants.pttypFloat32, values));
                                    }
                                    catch (Exception ex)
                                    {
                                        OnProcessException(new InvalidOperationException(string.Format("Failed to add PI tag '{0}' for measurement '{1}': {2}", tagname, key, ex.Message), ex));
                                    }

                                    addedNewPoint = true;
                                }
                            }
                            else
                            {
                                try
                                {
                                    // Rename tag-name if needed
                                    if (string.CompareOrdinal(pointList[1].Name, tagname) != 0)
                                        m_connection.Execute(server => server.PIPoints.Rename(pointList[1].Name, tagname));
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
                                    // See if ActiveMeasurements contains updated on column
                                    if (measurements.Columns.Contains("UpdatedOn"))
                                    {
                                        updatedOn = Convert.ToDateTime(rows[0]["UpdatedOn"]);
                                    }
                                    else
                                    {
                                        // Attempt to lookup last update time for record
                                        if ((object)database == null)
                                            database = new AdoDataConnection("systemSettings");

                                        updatedOn = Convert.ToDateTime(database.Connection.ExecuteScalar(string.Format("SELECT UpdatedOn FROM Measurement WHERE SignalID = '{0}'", key.SignalID)));
                                    }
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
                                        PIErrors errors = null;
                                        NamedValues edits = new NamedValues();
                                        NamedValues edit = new NamedValues();

                                        edit.Add("pointsource", m_pointSource);
                                        edit.Add("Descriptor", rows[0]["Description"].ToString());
                                        edit.Add("exdesc", rows[0]["SignalID"].ToString());
                                        edit.Add("sourcetag", rows[0]["PointTag"].ToString());

                                        // Engineering units is a new field for this view -- handle the case that it's not there
                                        if (measurements.Columns.Contains("EngineeringUnits"))
                                            edit.Add("engunits", rows[0]["EngineeringUnits"].ToString());

                                        edits.Add(rows[0]["PointTag"].ToString(), edit);

                                        m_connection.Execute(server =>
                                        {
                                            IPIPoints2 pts2 = (IPIPoints2)server.PIPoints;
                                            pts2.EditTags(edits, out errors);
                                        });

                                        if ((object)errors != null && errors.Count > 0)
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
            finally
            {
                m_refreshingMetadata = false;
            }

            m_lastMetadataRefresh = DateTime.UtcNow;

            OnStatusMessage("Completed metadata refresh successfully.");

            // Re-establish tag-map cache since meta-data and tags may exist now that didn't before
            LoadTagMap(inputMeasurements);
        }

        // Resets the PI tag to MeasurementKey mapping for loading data into PI by finding PI points that match either the GSFSchema point tag or alternate tag
        private void LoadTagMap(MeasurementKey[] inputMeasurements)
        {
            OnStatusMessage("Establishing tag-map dictionary...");

            List<MeasurementKey> newTags = new List<MeasurementKey>();

            if ((object)inputMeasurements != null && inputMeasurements.Length > 0)
            {
                foreach (MeasurementKey key in inputMeasurements)
                {
                    // Add key to dictionary with null value if not defined, actual mapping will happen dynamically as needed
                    if (!m_mappedConnectionPoints.ContainsKey(key))
                        m_mappedConnectionPoints.TryAdd(key, null);

                    newTags.Add(key);
                }
            }

            if (newTags.Count > 0)
            {
                // Determine which tags no longer exist
                ConnectionPoint removedConnectionPoint;
                HashSet<MeasurementKey> tagsToRemove = new HashSet<MeasurementKey>(m_mappedConnectionPoints.Keys);

                // If there are existing tags that are not part of new updates, these need to be removed
                tagsToRemove.ExceptWith(newTags);

                if (tagsToRemove.Count > 0)
                {
                    foreach (MeasurementKey key in tagsToRemove)
                    {
                        m_mappedConnectionPoints.TryRemove(key, out removedConnectionPoint);
                    }

                    OnStatusMessage("Detected {0:N0} tags that have been removed from OSI-PI output - primary tag-map has been updated...", tagsToRemove.Count);
                }

                if (m_mappedConnectionPoints.Count == 0)
                    OnStatusMessage("[WARNING] No PI tags were mapped to measurements - no tag-map exists so no points will be archived.");
            }
            else
            {
                if (m_mappedConnectionPoints.Count > 0)
                    OnStatusMessage("[WARNING] No PI tags were mapped to measurements - existing tag-map with {0:N0} tags remains in use.", m_mappedConnectionPoints.Count);
                else
                    OnStatusMessage("[WARNING] No PI tags were mapped to measurements - no tag-map exists so no points will be archived.");
            }
        }

        private void m_connection_Disconnected(object sender, EventArgs e)
        {
            // Since we may get a plethora of these requests, we use a synchronized operation to restart once
            m_restartConnection.RunOnceAsync();
        }

        #endregion
    }
}