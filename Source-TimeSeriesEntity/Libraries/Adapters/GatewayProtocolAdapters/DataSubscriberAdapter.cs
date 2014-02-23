//******************************************************************************************************
//  DataSubscriberAdapter.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  01/13/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using GSF;
using GSF.Configuration;
using GSF.Data;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Statistics;
using GSF.TimeSeries.Transport;
using AdoDataConnection = GSF.Data.AdoDataConnection;
using ConnectionStringParser = GSF.Configuration.ConnectionStringParser<GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute, GSF.TimeSeries.Adapters.NestedConnectionStringParameterAttribute>;
using DataSetEqualityComparer = GSF.Data.DataSetEqualityComparer;

namespace GatewayProtocolAdapters
{
    [Description("DataSubscriber: client that subscribes to a publishing server for a streaming data.")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DataSubscriberAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Fields
        private DataSubscriber m_dataSubscriber;
        private DataSubscriberSettings m_settingsObject;

        private SynchronizedOperation m_synchronizeMetadataOperation;
        private volatile DataSet m_receivedMetadata;
        private volatile DataSet m_synchronizedMetadata;
        private bool m_metadataSynchronized;

        private Guid m_nodeID;
        private int m_gatewayProtocolID;
        private long m_syncProgressTotalActions;
        private int m_syncProgressActionsCount;
        private long m_syncProgressUpdateInterval;
        private long m_syncProgressLastMessage;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public DataSubscriberAdapter()
        {
            m_settingsObject = new DataSubscriberSettings();
            m_synchronizeMetadataOperation = new SynchronizedOperation(SynchronizeMetadata);
        }

        #endregion

        #region [ Properties ]

        public bool Authenticated
        {
            get
            {
                return m_dataSubscriber.Authenticated;
            }
        }

        public long TotalBytesReceived
        {
            get
            {
                return m_dataSubscriber.TotalBytesReceived;
            }
        }

        public long LifetimeMeasurements
        {
            get
            {
                return m_dataSubscriber.LifetimeMeasurements;
            }
        }

        public long MinimumMeasurementsPerSecond
        {
            get
            {
                return m_dataSubscriber.MinimumMeasurementsPerSecond;
            }
        }

        public long MaximumMeasurementsPerSecond
        {
            get
            {
                return m_dataSubscriber.MaximumMeasurementsPerSecond;
            }
        }

        public long AverageMeasurementsPerSecond
        {
            get
            {
                return m_dataSubscriber.AverageMeasurementsPerSecond;
            }
        }

        public long LifetimeMinimumLatency
        {
            get
            {
                return m_dataSubscriber.LifetimeMinimumLatency;
            }
        }

        public long LifetimeMaximumLatency
        {
            get
            {
                return m_dataSubscriber.LifetimeMaximumLatency;
            }
        }

        public long LifetimeAverageLatency
        {
            get
            {
                return m_dataSubscriber.LifetimeAverageLatency;
            }
        }

        public override string ConnectionString
        {
            get
            {
                return base.ConnectionString;
            }
            set
            {
                ConnectionStringParser parser;
                base.ConnectionString = value;
                parser = new ConnectionStringParser();
                parser.ParseConnectionString(value, SettingsObject);
            }
        }

        public override object SettingsObject
        {
            get
            {
                return m_settingsObject;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets flag that determines if the data input connects asynchronously.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the status of this <see cref="DataSubscriber"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.Append(m_dataSubscriber.Status);
                status.Append(base.Status);
                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        public override void Initialize()
        {
            string localCertificateSetting;
            string remoteCertificateSetting;

            base.Initialize();

            m_dataSubscriber = new DataSubscriber()
            {
                SecurityMode = m_settingsObject.SecurityMode,
                OperationalModes = m_settingsObject.OperationalModes,
                DataLossInterval = m_settingsObject.DataLossInterval,
                BufferSize = m_settingsObject.BufferSize
            };

            if (!Settings.ContainsKey("SecurityMode"))
                m_dataSubscriber.RequireAuthentication = m_settingsObject.RequireAuthentication;

            if (Settings.ContainsKey("ReceiveInternalMetadata"))
                m_dataSubscriber.ReceiveInternalMetadata = m_settingsObject.ReceiveInternalMetadata;

            if (Settings.ContainsKey("ReceiveExternalMetadata"))
                m_dataSubscriber.ReceiveExternalMetadata = m_settingsObject.ReceiveExternalMetadata;

            if (m_settingsObject.SecurityMode == SecurityMode.TLS)
            {
                localCertificateSetting = m_settingsObject.TlsSecuritySettings.LocalCertificate;
                remoteCertificateSetting = m_settingsObject.TlsSecuritySettings.RemoteCertificate;

                if (string.IsNullOrEmpty(localCertificateSetting) || !File.Exists(localCertificateSetting))
                    localCertificateSetting = GetLocalCertificate();

                if (string.IsNullOrEmpty(remoteCertificateSetting) || !RemoteCertificateExists())
                    throw new ArgumentException("The \"remoteCertificate\" setting must be defined and certificate file must exist when using TLS security mode.");

                m_dataSubscriber.LocalCertificateFilePath = localCertificateSetting;
                m_dataSubscriber.RemoteCertificateFilePath = remoteCertificateSetting;
                m_dataSubscriber.ValidPolicyErrors = m_settingsObject.TlsSecuritySettings.ValidPolicyErrors;
                m_dataSubscriber.ValidChainFlags = m_settingsObject.TlsSecuritySettings.ValidChainFlags;
                m_dataSubscriber.CheckCertificateRevocation = m_settingsObject.TlsSecuritySettings.CheckCertificateRevocation;
            }

            m_dataSubscriber.ConnectionEstablished += DataSubscriber_ConnectionEstablished;
            m_dataSubscriber.ConnectionAuthenticated += DataSubscriber_ConnectionAuthenticated;
            m_dataSubscriber.MetaDataReceived += DataSubscriber_MetaDataReceived;
            m_dataSubscriber.ProcessingComplete += DataSubscriber_ProcessingComplete;
            m_dataSubscriber.StatusMessage += DataSubscriber_StatusMessage;
            m_dataSubscriber.ProcessException += DataSubscriber_ProcessException;

            // Register subscriber with the statistics engine
            StatisticsEngine.Register(this, "Subscriber", "SUB");
        }

        public override string GetShortStatus(int maxLength)
        {
            return m_dataSubscriber.GetShortStatus(maxLength);
        }

        /// <summary>
        /// Returns the measurements signal IDs that were authorized after the last successful subscription request.
        /// </summary>
        [AdapterCommand("Gets authorized signal IDs from last subscription request.", "Administrator", "Editor", "Viewer")]
        public Guid[] GetAuthorizedSignalIDs()
        {
            return m_dataSubscriber.GetAuthorizedSignalIDs();
        }

        /// <summary>
        /// Returns the measurements signal IDs that were unauthorized after the last successful subscription request.
        /// </summary>
        [AdapterCommand("Gets unauthorized signal IDs from last subscription request.", "Administrator", "Editor", "Viewer")]
        public Guid[] GetUnauthorizedSignalIDs()
        {
            return m_dataSubscriber.GetUnauthorizedSignalIDs();
        }

        /// <summary>
        /// Resets the counters for the lifetime statistics without interrupting the adapter's operations.
        /// </summary>
        [AdapterCommand("Resets the counters for the lifetime statistics without interrupting the adapter's operations.", "Administrator", "Editor")]
        public virtual void ResetLifetimeCounters()
        {
            m_dataSubscriber.ResetLifetimeCounters();
        }

        /// <summary>
        /// Initiate a meta-data refresh.
        /// </summary>
        [AdapterCommand("Initiates a meta-data refresh.", "Administrator", "Editor")]
        public virtual void RefreshMetadata()
        {
            m_dataSubscriber.SendServerCommand(ServerCommand.MetaDataRefresh, m_settingsObject.MetadataFilters);
        }

        /// <summary>
        /// Attempts to connect to data input source.
        /// </summary>
        protected override void AttemptConnection()
        {
            if ((object)m_dataSubscriber != null)
                m_dataSubscriber.Start();
        }

        /// <summary>
        /// Attempts to disconnect from data input source.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if ((object)m_dataSubscriber != null)
                m_dataSubscriber.Stop();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataSubscriberAdapter"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.

                    if (disposing)
                    {
                        if ((object)m_dataSubscriber != null)
                        {
                            m_dataSubscriber.Dispose();
                            m_dataSubscriber = null;
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

        // Gets the path to the local certificate from the configuration file
        private string GetLocalCertificate()
        {
            CategorizedSettingsElement localCertificateElement = ConfigurationFile.Current.Settings["systemSettings"]["LocalCertificate"];
            string localCertificate = null;

            if ((object)localCertificateElement != null)
                localCertificate = localCertificateElement.Value;

            if ((object)localCertificate == null || !File.Exists(FilePath.GetAbsolutePath(localCertificate)))
                throw new InvalidOperationException("Unable to find local certificate. Local certificate file must exist when using TLS security mode.");

            return localCertificate;
        }

        // Checks if the specified certificate exists
        private bool RemoteCertificateExists()
        {
            string remoteCertificate = m_settingsObject.TlsSecuritySettings.RemoteCertificate;
            string fullPath = FilePath.GetAbsolutePath(remoteCertificate);
            CategorizedSettingsElement remoteCertificateElement;

            if (!File.Exists(fullPath))
            {
                remoteCertificateElement = ConfigurationFile.Current.Settings["systemSettings"]["RemoteCertificatesPath"];

                if ((object)remoteCertificateElement != null)
                {
                    remoteCertificate = Path.Combine(remoteCertificateElement.Value, remoteCertificate);
                    fullPath = FilePath.GetAbsolutePath(remoteCertificate);
                }
            }

            return File.Exists(fullPath);
        }

        private void StartSubscription()
        {
            UnsynchronizedSubscriptionInfo info = new UnsynchronizedSubscriptionInfo(false)
            {
                UseCompactMeasurementFormat = true,
                FilterExpression = string.Join(";", OutputSignalIDs),
                UdpDataChannel = (m_settingsObject.DataChannelPort >= 0) && (m_settingsObject.DataChannelPort < 65536),
                DataChannelLocalPort = m_settingsObject.DataChannelPort
            };

            if (OutputSignalIDs.Count > 0)
                m_dataSubscriber.Subscribe(info);
            else if (!m_settingsObject.SynchronizeMetadata || m_metadataSynchronized)
                OnStatusMessage("WARNING: No measurements are currently defined for subscription.");

            if (m_settingsObject.SynchronizeMetadata && !m_metadataSynchronized)
                m_dataSubscriber.SendServerCommand(ServerCommand.MetaDataRefresh, m_settingsObject.MetadataFilters);
        }

        // Initialize (or reinitialize) the output signals associated with the data subscriber.
        // Returns true if output signals were updated, otherwise false if they remain the same.
        private bool UpdateOutputSignals()
        {
            IList<Guid> originalOutputMeasurements = OutputSignalIDs.ToList();
            string setting;

            // Reapply filter expressions and/or sourceIDs. This can be important after
            // a meta-data refresh which may have added new measurements that could now
            // be applicable as desired output measurements.
            OutputSignalIDs.Clear();

            if (Settings.TryGetValue("outputMeasurements", out setting))
                OutputSignalIDs.UnionWith(ParseFilterExpression(DataSource, true, setting));

            OutputSourceIDs = OutputSourceIDs;

            // If active measurements are defined, attempt to defined desired subscription points from there
            if ((object)DataSource != null && DataSource.Tables.Contains("ActiveMeasurements"))
            {
                try
                {
                    // Filter to points associated with this subscriber that have been requested for subscription, are enabled and not owned locally
                    DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select("Subscribed <> 0");
                    HashSet<Guid> subscribedMeasurements = new HashSet<Guid>();
                    Guid signalID;

                    foreach (DataRow row in filteredRows)
                    {
                        // Parse primary measurement identifier
                        signalID = row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();
                        subscribedMeasurements.Add(signalID);
                    }

                    // Combine subscribed output signal with any existing output signal and return unique set
                    OutputSignalIDs.UnionWith(subscribedMeasurements);
                }
                catch (Exception ex)
                {
                    // Errors here may not be catastrophic, this simply limits the auto-assignment of input measurement keys desired for subscription
                    OnProcessException(new InvalidOperationException(string.Format("Failed to apply subscribed measurements to subscription filter: {0}", ex.Message), ex));
                }
            }

            // Ensure that we are not attempting to subscribe to
            // signals that we know cannot be published
            TryFilterOutputSignals();

            // Determine if output signals have changed
            return OutputSignalIDs.SetEquals(originalOutputMeasurements);
        }

        // When synchronizing meta-data, the publisher sends meta-data for all possible signals we can subscribe to.
        // Here we check each signal defined in OutputSignalIDs to determine whether that signal was defined in
        // the published meta-data rather than blindly attempting to subscribe to all signals.
        private void TryFilterOutputSignals()
        {
            IEnumerable<Guid> signals;
            Guid signalID = Guid.Empty;

            try
            {
                if ((object)DataSource != null && DataSource.Tables.Contains("ActiveMeasurements"))
                {
                    signals = DataSource.Tables["ActiveMeasurements"]
                        .Select(string.Format("DeviceID = {0}", ID))
                        .Where(row => Guid.TryParse(row["SignalID"].ToNonNullString(), out signalID))
                        .Select(row => signalID);

                    OutputSignalIDs.IntersectWith(signals);
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Error when filtering output measurements by device ID: {0}", ex.Message), ex));
            }
        }

        // Handles meta-data synchronization to local system.
        private void SynchronizeMetadata()
        {
            // TODO: This function is complex and very closely tied to the current time-series data schema - perhaps it should be moved outside this class and referenced
            // TODO: as a delegate that can be assigned and called to allow other schemas as well. DataPublisher is already very flexible in what data it can deliver.
            try
            {
                DataSet metadata = m_receivedMetadata;
                IDbTransaction transaction = null;

                Ticks startTime;
                int metadataSynchronizationTimeout;

                int parentID;
                object historianID;

                // Only perform database synchronization if meta-data has changed since last update
                if (!SynchronizedMetadataChanged(metadata))
                    return;

                if ((object)metadata == null)
                {
                    OnStatusMessage("WARNING: Meta-data synchronization was not performed, deserialized dataset was empty.");
                    return;
                }

                // Track total meta-data synchronization process time
                startTime = DateTime.UtcNow.Ticks;

                // Open the configuration database using settings found in the config file
                using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
                using (IDbCommand command = database.Connection.CreateCommand())
                {
                    if (m_settingsObject.UseTransactionForMetadata)
                        transaction = database.Connection.BeginTransaction(database.DefaultIsloationLevel());

                    try
                    {
                        metadataSynchronizationTimeout = m_settingsObject.MetadataSynchronizationTimeout;

                        if ((object)transaction != null)
                            command.Transaction = transaction;

                        // Query the actual record ID based on the known run-time ID for this subscriber device
                        parentID = Convert.ToInt32(command.ExecuteScalar(string.Format("SELECT SourceID FROM Runtime WHERE ID = {0} AND SourceTable='Device'", ID), metadataSynchronizationTimeout));

                        // Validate that the subscriber device is marked as a concentrator (we are about to associate children devices with it)
                        if (!command.ExecuteScalar(string.Format("SELECT IsConcentrator FROM Device WHERE ID = {0}", parentID), metadataSynchronizationTimeout).ToString().ParseBoolean())
                            command.ExecuteNonQuery(string.Format("UPDATE Device SET IsConcentrator = 1 WHERE ID = {0}", parentID), metadataSynchronizationTimeout);

                        // Get any historian associated with the subscriber device
                        historianID = command.ExecuteScalar(string.Format("SELECT HistorianID FROM Device WHERE ID = {0}", parentID), metadataSynchronizationTimeout);

                        // Determine the active node ID - we cache this since this value won't change for the lifetime of this class
                        if (m_nodeID == Guid.Empty)
                            m_nodeID = Guid.Parse(command.ExecuteScalar(string.Format("SELECT NodeID FROM IaonInputAdapter WHERE ID = {0}", (int)ID), metadataSynchronizationTimeout).ToString());

                        // Determine the protocol record auto-inc ID value for the gateway transport protocol (GEP) - this value is also cached since it shouldn't change for the lifetime of this class
                        if (m_gatewayProtocolID == 0)
                            m_gatewayProtocolID = int.Parse(command.ExecuteScalar("SELECT ID FROM Protocol WHERE Acronym='GatewayTransport'", metadataSynchronizationTimeout).ToString());

                        // Ascertain total number of actions required for all meta-data synchronization so some level feed back can be provided on progress
                        InitSyncProgress(metadata.Tables.Cast<DataTable>().Select(dataTable => (long)dataTable.Rows.Count).Sum() + 3);

                        // Prefix all children devices with the name of the parent since the same device names could appear in different connections (helps keep device names unique)
                        string sourcePrefix = Name + "!";
                        Dictionary<string, int> deviceIDs = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
                        string deviceAcronym, signalTypeAcronym;
                        decimal longitude, latitude;
                        decimal? location;
                        object originalSource;
                        int deviceID;

                        // Check to see if data for the "DeviceDetail" table was included in the meta-data
                        if (metadata.Tables.Contains("DeviceDetail"))
                        {
                            DataTable deviceDetail = metadata.Tables["DeviceDetail"];
                            List<Guid> uniqueIDs = new List<Guid>();
                            DataRow[] deviceRows;

                            // Define SQL statement to query if this device is already defined (this should always be based on the unique guid-based device ID)
                            string deviceExistsSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Device WHERE UniqueID = {0}", "uniqueID");

                            // Define SQL statement to insert new device record
                            string insertDeviceSql = database.ParameterizedQueryString("INSERT INTO Device(NodeID, ParentID, HistorianID, Acronym, Name, ProtocolID, FramesPerSecond, OriginalSource, AccessID, Longitude, Latitude, ContactList, IsConcentrator, Enabled) " +
                                                                                       "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, 0, 1)", "nodeID", "parentID", "historianID", "acronym", "name", "protocolID", "framesPerSecond", "originalSource", "accessID", "longitude", "latitude", "contactList");

                            // Define SQL statement to update device's guid-based unique ID after insert
                            string updateDeviceUniqueIDSql = database.ParameterizedQueryString("UPDATE Device SET UniqueID = {0} WHERE Acronym = {1}", "uniqueID", "acronym");

                            // Define SQL statement to query if a device can be safely updated
                            string deviceIsUpdatableSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Device WHERE UniqueID = {0} AND (ParentID <> {1} OR ParentID IS NULL)", "uniqueID", "parentID");

                            // Define SQL statement to update existing device record
                            string updateDeviceSql = database.ParameterizedQueryString("UPDATE Device SET Acronym = {0}, Name = {1}, OriginalSource = {2}, ProtocolID = {3}, FramesPerSecond = {4}, HistorianID = {5}, AccessID = {6}, Longitude = {7}, Latitude = {8}, ContactList = {9} WHERE UniqueID = {10}",
                                                                                       "acronym", "name", "originalSource", "protocolID", "framesPerSecond", "historianID", "accessID", "longitude", "latitude", "contactList", "uniqueID");

                            // Define SQL statement to retrieve device's auto-inc ID based on its unique guid-based ID
                            string queryDeviceIDSql = database.ParameterizedQueryString("SELECT ID FROM Device WHERE UniqueID = {0}", "uniqueID");

                            // Define SQL statement to retrieve all unique device ID's for the current parent to check for mismatches
                            string queryUniqueDeviceIDsSql = database.ParameterizedQueryString("SELECT UniqueID FROM Device WHERE ParentID = {0}", "parentID");

                            // Define SQL statement to remove device records that no longer exist in the meta-data
                            string deleteDeviceSql = database.ParameterizedQueryString("DELETE FROM Device WHERE UniqueID = {0}", "uniqueID");

                            // Determine which device rows should be synchronized based on operational mode flags
                            if (m_dataSubscriber.ReceiveInternalMetadata && m_dataSubscriber.ReceiveExternalMetadata)
                                deviceRows = deviceDetail.Select();
                            else if (m_dataSubscriber.ReceiveInternalMetadata)
                                deviceRows = deviceDetail.Select("OriginalSource IS NULL");
                            else if (m_dataSubscriber.ReceiveExternalMetadata)
                                deviceRows = deviceDetail.Select("OriginalSource IS NOT NULL");
                            else
                                deviceRows = new DataRow[0];

                            // Check existence of optional meta-data fields
                            DataColumnCollection deviceDetailColumns = deviceDetail.Columns;
                            bool accessIDFieldExists = deviceDetailColumns.Contains("AccessID");
                            bool longitudeFieldExists = deviceDetailColumns.Contains("Longitude");
                            bool latitudeFieldExists = deviceDetailColumns.Contains("Latitude");
                            bool companyAcronymFieldExists = deviceDetailColumns.Contains("CompanyAcronym");
                            bool protocolNameFieldExists = deviceDetailColumns.Contains("ProtocolName");
                            bool vendorAcronymFieldExists = deviceDetailColumns.Contains("VendorAcronym");
                            bool vendorDeviceNameFieldExists = deviceDetailColumns.Contains("VendorDeviceName");
                            bool interconnectionNameFieldExists = deviceDetailColumns.Contains("InterconnectionName");

                            // Older versions of GEP did not include the AccessID field, so this is treated as optional
                            int accessID = 0;

                            foreach (DataRow row in deviceRows)
                            {
                                Guid uniqueID = Guid.Parse(row.Field<object>("UniqueID").ToString());

                                // Track unique device Guids in this meta-data session, we'll need to remove any old associated devices that no longer exist
                                uniqueIDs.Add(uniqueID);

                                // We will synchronize meta-data only if the source owns this device and it's not defined as a concentrator (these should normally be filtered by publisher - but we check just in case).
                                if (!row["IsConcentrator"].ToNonNullString("0").ParseBoolean())
                                {
                                    if (accessIDFieldExists)
                                        accessID = (int)row.Field<long>("AccessID");

                                    // Get longitude and latitude values if they are defined
                                    longitude = 0M;
                                    latitude = 0M;

                                    if (longitudeFieldExists)
                                    {
                                        location = row.ConvertNullableField<decimal>("Longitude");

                                        if (location.HasValue)
                                            longitude = location.Value;
                                    }

                                    if (latitudeFieldExists)
                                    {
                                        location = row.ConvertNullableField<decimal>("Latitude");

                                        if (location.HasValue)
                                            latitude = location.Value;
                                    }

                                    // Save any reported extraneous values from device meta-data in connection string formatted contact list - all fields are considered optional
                                    Dictionary<string, string> contactList = new Dictionary<string, string>();

                                    if (companyAcronymFieldExists)
                                        contactList["companyAcronym"] = row.Field<string>("CompanyAcronym") ?? string.Empty;

                                    if (protocolNameFieldExists)
                                        contactList["protocolName"] = row.Field<string>("ProtocolName") ?? string.Empty;

                                    if (vendorAcronymFieldExists)
                                        contactList["vendorAcronym"] = row.Field<string>("VendorAcronym") ?? string.Empty;

                                    if (vendorDeviceNameFieldExists)
                                        contactList["vendorDeviceName"] = row.Field<string>("VendorDeviceName") ?? string.Empty;

                                    if (interconnectionNameFieldExists)
                                        contactList["interconnectionName"] = row.Field<string>("InterconnectionName") ?? string.Empty;

                                    // Determine if device record already exists
                                    if (Convert.ToInt32(command.ExecuteScalar(deviceExistsSql, metadataSynchronizationTimeout, database.Guid(uniqueID))) == 0)
                                    {
                                        // Insert new device record
                                        command.ExecuteNonQuery(insertDeviceSql, metadataSynchronizationTimeout, database.Guid(m_nodeID), parentID, historianID, sourcePrefix + row.Field<string>("Acronym"), row.Field<string>("Name"), m_gatewayProtocolID, row.ConvertField<int>("FramesPerSecond"),
                                                                m_settingsObject.Internal ? (object)DBNull.Value : string.IsNullOrEmpty(row.Field<string>("ParentAcronym")) ? sourcePrefix + row.Field<string>("Acronym") : sourcePrefix + row.Field<string>("ParentAcronym"), accessID, longitude, latitude, contactList.JoinKeyValuePairs());

                                        // Guids are normally auto-generated during insert - after insertion update the Guid so that it matches the source data. Most of the database
                                        // scripts have triggers that support properly assigning the Guid during an insert, but this code ensures the Guid will always get assigned.
                                        command.ExecuteNonQuery(updateDeviceUniqueIDSql, metadataSynchronizationTimeout, database.Guid(uniqueID), sourcePrefix + row.Field<string>("Acronym"));
                                    }
                                    else
                                    {
                                        // Perform safety check to preserve device records which are not safe to overwrite
                                        if (Convert.ToInt32(command.ExecuteScalar(deviceIsUpdatableSql, metadataSynchronizationTimeout, database.Guid(uniqueID), parentID)) > 0)
                                            continue;

                                        // Gateway is assuming ownership of the device records when the "internal" flag is true - this means the device's measurements can be forwarded to another party. From a device record perspective,
                                        // ownership is inferred by setting 'OriginalSource' to null. When gateway doesn't own device records (i.e., the "internal" flag is false), this means the device's measurements can only be consumed
                                        // locally - from a device record perspective this means the 'OriginalSource' field is set to the acronym of the PDC or PMU that generated the source measurements. This field allows a mirrored source
                                        // restriction to be implemented later to ensure all devices in an output protocol came from the same original source connection, if desired.
                                        originalSource = m_settingsObject.Internal ? (object)DBNull.Value : string.IsNullOrEmpty(row.Field<string>("ParentAcronym")) ? sourcePrefix + row.Field<string>("Acronym") : sourcePrefix + row.Field<string>("ParentAcronym");

                                        // Update existing device record
                                        command.ExecuteNonQuery(updateDeviceSql, metadataSynchronizationTimeout, sourcePrefix + row.Field<string>("Acronym"), row.Field<string>("Name"), originalSource, m_gatewayProtocolID, row.ConvertField<int>("FramesPerSecond"), historianID, accessID, longitude, latitude, contactList.JoinKeyValuePairs(), database.Guid(uniqueID));
                                    }
                                }

                                // Capture local device ID auto-inc value for measurement association
                                deviceIDs[row.Field<string>("Acronym")] = Convert.ToInt32(command.ExecuteScalar(queryDeviceIDSql, metadataSynchronizationTimeout, database.Guid(uniqueID)));

                                // Periodically notify user about synchronization progress
                                UpdateSyncProgress();
                            }

                            // Remove any device records associated with this subscriber that no longer exist in the meta-data
                            if (uniqueIDs.Count > 0)
                            {
                                // Sort unique ID list so that binary search can be used for quick lookups
                                uniqueIDs.Sort();

                                DataTable deviceUniqueIDs = command.RetrieveData(database.AdapterType, queryUniqueDeviceIDsSql, metadataSynchronizationTimeout, parentID);
                                Guid uniqueID;

                                foreach (DataRow deviceRow in deviceUniqueIDs.Rows)
                                {
                                    uniqueID = database.Guid(deviceRow, "UniqueID");

                                    // Remove any devices in the database that are associated with the parent device and do not exist in the meta-data
                                    if (uniqueIDs.BinarySearch(uniqueID) < 0)
                                        command.ExecuteNonQuery(deleteDeviceSql, metadataSynchronizationTimeout, database.Guid(uniqueID));
                                }
                                UpdateSyncProgress();
                            }
                        }

                        // Check to see if data for the "MeasurementDetail" table was included in the meta-data
                        if (metadata.Tables.Contains("MeasurementDetail"))
                        {
                            DataTable measurementDetail = metadata.Tables["MeasurementDetail"];
                            List<Guid> signalIDs = new List<Guid>();
                            DataRow[] measurementRows;

                            // Define SQL statement to query if this measurement is already defined (this should always be based on the unique signal ID Guid)
                            string measurementExistsSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Measurement WHERE SignalID = {0}", "signalID");

                            // Define SQL statement to insert new measurement record
                            string insertMeasurementSql = database.ParameterizedQueryString("INSERT INTO Measurement(DeviceID, HistorianID, PointTag, AlternateTag, SignalTypeID, PhasorSourceIndex, SignalReference, Description, Internal, Subscribed, Enabled) " +
                                                                                            "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, 0, 1)", "deviceID", "historianID", "pointTag", "alternateTag", "signalTypeID", "phasorSourceIndex", "signalReference", "description", "internal");

                            // Define SQL statement to update measurement's signal ID after insert
                            string updateMeasurementSignalIDSql = database.ParameterizedQueryString("UPDATE Measurement SET SignalID = {0}, AlternateTag = NULL WHERE AlternateTag = {1}", "signalID", "alternateTag");

                            // Define SQL statement to update existing measurement record
                            string updateMeasurementSql = database.ParameterizedQueryString("UPDATE Measurement SET HistorianID = {0}, PointTag = {1}, SignalTypeID = {2}, PhasorSourceIndex = {3}, SignalReference = {4}, Description = {5}, Internal = {6} WHERE SignalID = {7}",
                                                                                            "historianID", "pointTag", "signalTypeID", "phasorSourceIndex", "signalReference", "description", "internal", "signalID");

                            // Define SQL statement to retrieve all measurement signal ID's for the current parent to check for mismatches - note that we use the ActiveMeasurements view
                            // since it associates measurements with their top-most parent runtime device ID, this allows us to easily query all measurements for the parent device
                            string queryMeasurementSignalIDsSql = database.ParameterizedQueryString("SELECT SignalID FROM ActiveMeasurement WHERE DeviceID = {0}", "deviceID");

                            // Define SQL statement to retrieve measurement's associated device ID, i.e., actual record ID, based on measurement's signal ID
                            string queryMeasurementDeviceIDSql = database.ParameterizedQueryString("SELECT DeviceID FROM Measurement WHERE SignalID = {0}", "signalID");

                            // Define SQL statement to remove device records that no longer exist in the meta-data
                            string deleteMeasurementSql = database.ParameterizedQueryString("DELETE FROM Measurement WHERE SignalID = {0}", "signalID");

                            // Load signal type ID's from local database associated with their acronym for proper signal type translation
                            Dictionary<string, int> signalTypeIDs = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

                            foreach (DataRow row in command.RetrieveData(database.AdapterType, "SELECT ID, Acronym FROM SignalType").Rows)
                            {
                                signalTypeAcronym = row.Field<string>("Acronym");

                                if (!string.IsNullOrWhiteSpace(signalTypeAcronym))
                                    signalTypeIDs[signalTypeAcronym] = row.ConvertField<int>("ID");
                            }

                            // Determine which measurement rows should be synchronized based on operational mode flags
                            if (m_dataSubscriber.ReceiveInternalMetadata && m_dataSubscriber.ReceiveExternalMetadata)
                                measurementRows = measurementDetail.Select();
                            else if (m_dataSubscriber.ReceiveInternalMetadata)
                                measurementRows = measurementDetail.Select("Internal <> 0");
                            else if (m_dataSubscriber.ReceiveExternalMetadata)
                                measurementRows = measurementDetail.Select("Internal = 0");
                            else
                                measurementRows = new DataRow[0];

                            // Older versions of GEP did not include the PhasorSourceIndex field, so this is treated as optional
                            bool phasorSourceIndexFieldExists = measurementDetail.Columns.Contains("PhasorSourceIndex");
                            object phasorSourceIndex = DBNull.Value;

                            foreach (DataRow row in measurementRows)
                            {
                                // Get device and signal type acronyms
                                deviceAcronym = row.Field<string>("DeviceAcronym") ?? string.Empty;
                                signalTypeAcronym = row.Field<string>("SignalAcronym") ?? string.Empty;

                                // Get phasor source index if field is defined
                                if (phasorSourceIndexFieldExists)
                                {
                                    // Using ConvertNullableField extension since publisher could use SQLite database in which case
                                    // all integers would arrive in data set as longs and need to be converted back to integers
                                    int? index = row.ConvertNullableField<int>("PhasorSourceIndex");
                                    phasorSourceIndex = index.HasValue ? (object)index.Value : (object)DBNull.Value;
                                }

                                // Make sure we have an associated device and signal type already defined for the measurement
                                if (!string.IsNullOrWhiteSpace(deviceAcronym) && deviceIDs.ContainsKey(deviceAcronym) && !string.IsNullOrWhiteSpace(signalTypeAcronym) && signalTypeIDs.ContainsKey(signalTypeAcronym))
                                {
                                    // Prefix the tag name with the "updated" device name
                                    deviceID = deviceIDs[deviceAcronym];
                                    string pointTag = sourcePrefix + row.Field<string>("PointTag");
                                    Guid signalID = Guid.Parse(row.Field<object>("SignalID").ToString());

                                    // Track unique measurement signal Guids in this meta-data session, we'll need to remove any old associated measurements that no longer exist
                                    signalIDs.Add(signalID);

                                    // Determine if measurement record already exists
                                    if (Convert.ToInt32(command.ExecuteScalar(measurementExistsSql, metadataSynchronizationTimeout, database.Guid(signalID))) == 0)
                                    {
                                        string alternateTag = Guid.NewGuid().ToString();

                                        // Insert new measurement record
                                        command.ExecuteNonQuery(insertMeasurementSql, metadataSynchronizationTimeout, deviceID, historianID, pointTag, alternateTag, signalTypeIDs[signalTypeAcronym], phasorSourceIndex, sourcePrefix + row.Field<string>("SignalReference"), row.Field<string>("Description") ?? string.Empty, database.Bool(m_settingsObject.Internal));

                                        // Guids are normally auto-generated during insert - after insertion update the Guid so that it matches the source data. Most of the database
                                        // scripts have triggers that support properly assigning the Guid during an insert, but this code ensures the Guid will always get assigned.
                                        command.ExecuteNonQuery(updateMeasurementSignalIDSql, metadataSynchronizationTimeout, database.Guid(signalID), alternateTag);
                                    }
                                    else
                                    {
                                        // Update existing measurement record. Note that this update assumes that measurements will remain associated with a static source device.
                                        command.ExecuteNonQuery(updateMeasurementSql, metadataSynchronizationTimeout, historianID, pointTag, signalTypeIDs[signalTypeAcronym], phasorSourceIndex, sourcePrefix + row.Field<string>("SignalReference"), row.Field<string>("Description") ?? string.Empty, database.Bool(m_settingsObject.Internal), database.Guid(signalID));
                                    }
                                }

                                // Periodically notify user about synchronization progress
                                UpdateSyncProgress();
                            }

                            // Remove any measurement records associated with existing devices in this session but no longer exist in the meta-data
                            if (signalIDs.Count > 0)
                            {
                                // Sort signal ID list so that binary search can be used for quick lookups
                                signalIDs.Sort();

                                // Query all the guid-based signal ID's for all measurement records associated with the parent device using run-time ID
                                DataTable measurementSignalIDs = command.RetrieveData(database.AdapterType, queryMeasurementSignalIDsSql, metadataSynchronizationTimeout, (int)ID);
                                Guid signalID;

                                // Walk through each database record and see if the measurement exists in the provided meta-data
                                foreach (DataRow measurementRow in measurementSignalIDs.Rows)
                                {
                                    signalID = database.Guid(measurementRow, "SignalID");

                                    // Remove any measurements in the database that are associated with received devices and do not exist in the meta-data
                                    if (signalIDs.BinarySearch(signalID) < 0)
                                    {
                                        // Measurement was not in the meta-data, get the measurement's actual record based ID for its associated device
                                        object measurementDeviceID = command.ExecuteScalar(queryMeasurementDeviceIDSql, metadataSynchronizationTimeout, database.Guid(signalID));

                                        // If the unknown measurement is directly associated with a device that exists in the meta-data it is assumed that this measurement
                                        // was removed from the publishing system and no longer exists therefore we remove it from the local measurement cache. If the user
                                        // needs custom local measurements associated with a remote device, they should be associated with the parent device only.
                                        if (measurementDeviceID != null && !(measurementDeviceID is DBNull) && deviceIDs.ContainsValue(Convert.ToInt32(measurementDeviceID)))
                                            command.ExecuteNonQuery(deleteMeasurementSql, metadataSynchronizationTimeout, database.Guid(signalID));
                                    }
                                }

                                UpdateSyncProgress();
                            }
                        }

                        // Check to see if data for the "PhasorDetail" table was included in the meta-data
                        if (metadata.Tables.Contains("PhasorDetail"))
                        {
                            Dictionary<int, int> maxSourceIndicies = new Dictionary<int, int>();
                            int sourceIndex;

                            // Phasor data is normally only needed so that the user can properly generate a mirrored IEEE C37.118 output stream from the source data.
                            // This is necessary since, in this protocol, the phasors are described (i.e., labeled) as a unit (i.e., as a complex number) instead of
                            // as two distinct angle and magnitude measurements.

                            // Define SQL statement to query if phasor record is already defined (no Guid is defined for these simple label records)
                            string phasorExistsSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Phasor WHERE DeviceID = {0} AND SourceIndex = {1}", "deviceID", "sourceIndex");

                            // Define SQL statement to insert new phasor record
                            string insertPhasorSql = database.ParameterizedQueryString("INSERT INTO Phasor(DeviceID, Label, Type, Phase, SourceIndex) VALUES ({0}, {1}, {2}, {3}, {4})", "deviceID", "label", "type", "phase", "sourceIndex");

                            // Define SQL statement to update existing phasor record
                            string updatePhasorSql = database.ParameterizedQueryString("UPDATE Phasor SET Label = {0}, Type = {1}, Phase = {2} WHERE DeviceID = {3} AND SourceIndex = {4}", "label", "type", "phase", "deviceID", "sourceIndex");

                            // Define SQL statement to delete a phasor record
                            string deletePhasorSql = database.ParameterizedQueryString("DELETE FROM Phasor WHERE DeviceID = {0} AND SourceIndex > {1}", "deviceID", "sourceIndex");

                            foreach (DataRow row in metadata.Tables["PhasorDetail"].Rows)
                            {
                                // Get device acronym
                                deviceAcronym = row.Field<string>("DeviceAcronym") ?? string.Empty;

                                // Make sure we have an associated device already defined for the phasor record
                                if (!string.IsNullOrWhiteSpace(deviceAcronym) && deviceIDs.ContainsKey(deviceAcronym))
                                {
                                    deviceID = deviceIDs[deviceAcronym];

                                    // Determine if phasor record already exists
                                    if (Convert.ToInt32(command.ExecuteScalar(phasorExistsSql, metadataSynchronizationTimeout, deviceID, row.ConvertField<int>("SourceIndex"))) == 0)
                                    {
                                        // Insert new phasor record
                                        command.ExecuteNonQuery(insertPhasorSql, metadataSynchronizationTimeout, deviceID, row.Field<string>("Label") ?? "undefined", (row.Field<string>("Type") ?? "V").TruncateLeft(1), (row.Field<string>("Phase") ?? "+").TruncateLeft(1), row.ConvertField<int>("SourceIndex"));
                                    }
                                    else
                                    {
                                        // Update existing phasor record
                                        command.ExecuteNonQuery(updatePhasorSql, metadataSynchronizationTimeout, row.Field<string>("Label") ?? "undefined", (row.Field<string>("Type") ?? "V").TruncateLeft(1), (row.Field<string>("Phase") ?? "+").TruncateLeft(1), deviceID, row.ConvertField<int>("SourceIndex"));
                                    }

                                    // Track largest source index for each device
                                    maxSourceIndicies.TryGetValue(deviceID, out sourceIndex);

                                    if (row.ConvertField<int>("SourceIndex") > sourceIndex)
                                        maxSourceIndicies[deviceID] = row.ConvertField<int>("SourceIndex");
                                }

                                // Periodically notify user about synchronization progress
                                UpdateSyncProgress();
                            }

                            // Remove any phasor records associated with existing devices in this session but no longer exist in the meta-data
                            if (maxSourceIndicies.Count > 0)
                            {
                                foreach (KeyValuePair<int, int> deviceIndexPair in maxSourceIndicies)
                                {
                                    command.ExecuteNonQuery(deletePhasorSql, metadataSynchronizationTimeout, deviceIndexPair.Key, deviceIndexPair.Value);
                                }
                            }
                        }

                        if ((object)transaction != null)
                            transaction.Commit();

                        // Update local in-memory synchronized meta-data cache
                        m_synchronizedMetadata = metadata;
                        m_metadataSynchronized = true;
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(new InvalidOperationException("Failed to synchronize meta-data to local cache: " + ex.Message, ex));

                        if ((object)transaction != null)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch (Exception rollbackException)
                            {
                                OnProcessException(new InvalidOperationException("Failed to roll back database transaction due to exception: " + rollbackException.Message, rollbackException));
                            }
                        }

                        return;
                    }
                    finally
                    {
                        if ((object)transaction != null)
                            transaction.Dispose();
                    }
                }

                OnStatusMessage("Meta-data synchronization completed successfully in {0}", (DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(3));

                // Send notification that system configuration has changed
                OnConfigurationChanged();

                // For automatic connections, when meta-data refresh is complete, update output measurements to see if any
                // points for subscription have changed after re-application of filter expressions and if so, resubscribe
                if (UpdateOutputSignals())
                {
                    OnStatusMessage("Meta-data received from publisher modified measurement availability, adjusting active subscription...");

                    // Updating subscription will restart data stream monitor upon successful resubscribe
                    StartSubscription();
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Failed to synchronize meta-data to local cache: " + ex.Message, ex));
            }
        }

        private void InitSyncProgress(long totalActions)
        {
            m_syncProgressTotalActions = totalActions;
            m_syncProgressActionsCount = 0;

            // We update user on progress with 5 messages or every 15 seconds
            m_syncProgressUpdateInterval = (long)(totalActions * 0.2D);
            m_syncProgressLastMessage = DateTime.UtcNow.Ticks;
        }

        private void UpdateSyncProgress()
        {
            m_syncProgressActionsCount++;

            if (m_syncProgressActionsCount % m_syncProgressUpdateInterval == 0 || DateTime.UtcNow.Ticks - m_syncProgressLastMessage > 150000000)
            {
                OnStatusMessage("Meta-data synchronization is {0:0.0%} complete...", m_syncProgressActionsCount / (double)m_syncProgressTotalActions);
                m_syncProgressLastMessage = DateTime.UtcNow.Ticks;
            }
        }

        private bool SynchronizedMetadataChanged(DataSet newSynchronizedMetadata)
        {
            try
            {
                return !DataSetEqualityComparer.Default.Equals(m_synchronizedMetadata, newSynchronizedMetadata);
            }
            catch
            {
                return true;
            }
        }

        private void DataSubscriber_ConnectionEstablished(object sender, EventArgs eventArgs)
        {
            string sharedSecret;
            string authenticationID;

            OnConnected();

            if (m_settingsObject.SecurityMode != SecurityMode.Gateway)
            {
                StartSubscription();
            }
            else
            {
                sharedSecret = m_settingsObject.GatewaySecuritySettings.SharedSecret;
                authenticationID = m_settingsObject.GatewaySecuritySettings.AuthenticationID;
                m_dataSubscriber.Authenticate(sharedSecret, authenticationID);
            }
        }

        private void DataSubscriber_ConnectionAuthenticated(object sender, EventArgs eventArgs)
        {
            StartSubscription();
        }

        private void DataSubscriber_MetaDataReceived(object sender, EventArgs<DataSet> eventArgs)
        {
            m_receivedMetadata = eventArgs.Argument;
            m_synchronizeMetadataOperation.RunOnce();
        }

        private void DataSubscriber_ProcessingComplete(object sender, EventArgs<string> eventArgs)
        {
            OnProcessingComplete();
        }

        private void DataSubscriber_StatusMessage(object sender, EventArgs<string> eventArgs)
        {
            OnStatusMessage(eventArgs.Argument);
        }

        private void DataSubscriber_ProcessException(object sender, EventArgs<Exception> eventArgs)
        {
            OnProcessException(eventArgs.Argument);
        }

        #endregion
    }
}
