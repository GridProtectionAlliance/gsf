//******************************************************************************************************
//  MetadataImportAdapter.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/24/2018 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF;
using GSF.Collections;
using GSF.Configuration;
using GSF.Data;
using GSF.Diagnostics;
using GSF.Scheduling;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;

namespace MetadataAdapters
{
    /// <summary>
    /// Represents an adapter that reads metadata from a file and updates the database.
    /// </summary>
    [Description("Metadata Import: Reads metadata from a file and updates the database.")]
    public class MetadataImportAdapter : FacileActionAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Defines the default value for the <see cref="ImportSchedule"/> property.
        /// </summary>
        public const string DefaultImportSchedule = "*/5 * * * *";

        /// <summary>
        /// Defines the default value for the <see cref="UseTransactionForMetadata"/> property.
        /// </summary>
        public const bool DefaultUseTransactionForMetadata = true;

        /// <summary>
        /// Defines the default value for the <see cref="MetadataSynchronizationTimeout"/> property.
        /// </summary>
        public const int DefaultMetadataSynchronizationTimeout = 0;

        /// <summary>
        /// Defines the default value for the <see cref="UseSourcePrefixNames"/> property.
        /// </summary>
        public const bool DefaultUseSourcePrefixNames = false;

        private const string ScheduleName = nameof(MetadataImportAdapter);

        // Fields
        private Action<DataSet> m_synchronizeMetadataAction;
        private ScheduleManager m_scheduleManager;
        private DateTime m_lastMetaDataRefreshTime;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MetadataImportAdapter"/> class.
        /// </summary>
        public MetadataImportAdapter()
        {
            m_synchronizeMetadataAction = GetSynchronizeMetadataAction();
            m_scheduleManager = new ScheduleManager();
            m_scheduleManager.ScheduleDue += ScheduleManager_ScheduleDue;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the path to the file in which the metadata to be imported will be defined.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the path to the file in which the metadata to be imported will be defined.")]
        public string ImportFilePath { get; set; }

        /// <summary>
        /// Gets or sets the schedule, using cron syntax, to search for metadata files to import.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultImportSchedule)]
        [Description("Defines the schedule, using cron syntax, to search for metadata files to import.")]
        public string ImportSchedule { get; set; }

        /// <summary>
        /// Gets or sets the acronym of the device used as the parent of the devices defined in the metadata.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue("")]
        [Description("Defines the acronym of the device used as the parent of the devices defined in the metadata.")]
        public string ParentDeviceAcronym { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if metadata synchronization should be performed within a transaction.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultUseTransactionForMetadata)]
        [Description("Defines the flag that determines if metadata synchronization should be performed within a transaction.")]
        public bool UseTransactionForMetadata { get; set; }

        /// <summary>
        /// Gets or sets the timeout used when executing database queries during metadata synchronization.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultMetadataSynchronizationTimeout)]
        [Description("Defines the timeout used when exeucting database queries during metadata synchronization.")]
        public int MetadataSynchronizationTimeout { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if child devices associated with a subscription
        /// should be prefixed with the subscription name and an exclamation point to ensure
        /// device name uniqueness - recommended value is <c>true</c>.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultUseSourcePrefixNames)]
        [Description("Defines the flag that determines if child device acronyms should be prefixed with the parent acronym.")]
        public bool UseSourcePrefixNames { get; set; }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

        /// <summary>
        /// Gets or sets primary keys of input measurements the <see cref="MetadataImportAdapter"/> expects, if any.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new MeasurementKey[] InputMeasurementKeys
        {
            get
            {
                return base.InputMeasurementKeys;
            }
            set
            {
                base.InputMeasurementKeys = value;
            }
        }

        /// <summary>
        /// Gets or sets output measurements that the <see cref="MetadataImportAdapter"/> will produce, if any.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new IMeasurement[] OutputMeasurements
        {
            get
            {
                return base.OutputMeasurements;
            }
            set
            {
                base.OutputMeasurements = value;
            }
        }

        /// <summary>
        /// Gets or sets the frames per second to be used by the <see cref="MetadataImportAdapter"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new int FramesPerSecond
        {
            get
            {
                return base.FramesPerSecond;
            }
            set
            {
                base.FramesPerSecond = value;
            }
        }

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double LagTime
        {
            get
            {
                return base.LagTime;
            }
            set
            {
                base.LagTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double LeadTime
        {
            get
            {
                return base.LeadTime;
            }
            set
            {
                base.LeadTime = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="MetadataImportAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            ConnectionStringParser<ConnectionStringParameterAttribute> parser = new ConnectionStringParser<ConnectionStringParameterAttribute>();
            parser.ParseConnectionString(ConnectionString, this);

            if (string.IsNullOrWhiteSpace(ParentDeviceAcronym))
                ParentDeviceAcronym = Name + "!PARENT";

            // Default to not using transactions for metadata on SQL server (helps avoid deadlocks)
            if (!Settings.ContainsKey(nameof(UseTransactionForMetadata)))
            {
                using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
                {
                    if (database.IsSQLServer)
                        UseTransactionForMetadata = false;
                }
            }

            m_scheduleManager.AddSchedule(ScheduleName, ImportSchedule);
        }

        /// <summary>
        /// Starts the <see cref="MetadataImportAdapter"/> or restarts it if it is already running.
        /// </summary>
        public override void Start()
        {
            base.Start();
            m_scheduleManager.Start();
        }

        /// <summary>
        /// Stops the <see cref="MetadataImportAdapter"/>.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            m_scheduleManager.Stop();
        }

        /// <summary>
        /// Searches for metadata to be imported and imports the metadata if found.
        /// </summary>
        [AdapterCommand("Imports metadata if found at the target location.")]
        public void ImportMetadata()
        {
            if (!File.Exists(ImportFilePath))
                return;

            const int MaxFailedAttempts = 5;
            const int DelayAfterFailure = 2000;
            int failedAttempts = 0;

            while (true)
            {
                try
                {
                    DataSet metadata;

                    using (FileStream stream = File.OpenRead(ImportFilePath))
                    {
                        metadata = stream.DeserializeToDataSet();
                    }

                    m_synchronizeMetadataAction(metadata);
                    File.Delete(ImportFilePath);
                    break;
                }
                catch (IOException)
                {
                    // Only throw an IOException
                    // after five failed attempts
                    failedAttempts++;

                    if (failedAttempts >= MaxFailedAttempts)
                        throw;

                    Thread.Sleep(DelayAfterFailure);
                }
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="MetadataExportAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="MetadataExportAdapter"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            string timestampFormat = "HH:mm:ss";

            if (DateTime.Now.Subtract(m_lastMetaDataRefreshTime).TotalDays > 1.0D)
                timestampFormat = "yyyy-MM-dd HH:mm";

            string lastImport = m_lastMetaDataRefreshTime.ToString(timestampFormat);
            string shortStatus = $"Last import: {m_lastMetaDataRefreshTime}";
            return shortStatus.CenterText(maxLength);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MetadataImportAdapter"/> object and optionally releases the managed resources.
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
                        m_scheduleManager.Stop();
                        m_scheduleManager.Dispose();
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
        /// Handles metadata synchronization to local system.
        /// </summary>
        private Action<DataSet> GetSynchronizeMetadataAction()
        {
            DataSet metadataToSynchronize = default(DataSet);
            DataSet synchronizedMetadata = default(DataSet);
            Guid nodeID = default(Guid);
            int virtualProtocolID = default(int);
            long syncProgressTotalActions = default(long);
            long syncProgressActionsCount = default(long);
            long syncProgressUpdateInterval = default(long);
            long syncProgressLastMessage = default(long);

            Action<long> initSyncProgress = totalActions =>
            {
                syncProgressTotalActions = totalActions;
                syncProgressActionsCount = 0;

                // We update user on progress with 5 messages or every 15 seconds
                syncProgressUpdateInterval = (long)(totalActions * 0.2D);
                syncProgressLastMessage = DateTime.UtcNow.Ticks;
            };

            Action updateSyncProgress = () =>
            {
                syncProgressActionsCount++;

                if (syncProgressActionsCount % syncProgressUpdateInterval == 0 || DateTime.UtcNow.Ticks - syncProgressLastMessage > 150000000)
                {
                    OnStatusMessage(MessageLevel.Info, $"metadata synchronization is {syncProgressActionsCount / (double)syncProgressTotalActions:0.0%} complete...");
                    syncProgressLastMessage = DateTime.UtcNow.Ticks;
                }
            };

            Func<DataSet, bool> synchronizedMetadataChanged = newSynchronizedMetadata =>
            {
                try
                {
                    return !DataSetEqualityComparer.Default.Equals(synchronizedMetadata, newSynchronizedMetadata);
                }
                catch
                {
                    return true;
                }
            };

            Action synchronizeMetadata = () =>
            {
                DataSet metadata = metadataToSynchronize;

                // Only perform database synchronization if metadata has changed since last update
                if (!synchronizedMetadataChanged(metadata))
                    return;

                if ((object)metadata == null)
                {
                    OnStatusMessage(MessageLevel.Error, "metadata synchronization was not performed, deserialized dataset was empty.");
                    return;
                }

                // Track total metadata synchronization process time
                Ticks startTime = DateTime.UtcNow.Ticks;
                DateTime updateTime;
                DateTime latestUpdateTime = DateTime.MinValue;

                // Open the configuration database using settings found in the config file
                using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
                using (IDbCommand command = database.Connection.CreateCommand())
                {
                    IDbTransaction transaction = null;

                    if (UseTransactionForMetadata)
                        transaction = database.Connection.BeginTransaction(database.DefaultIsloationLevel);

                    try
                    {
                        if ((object)transaction != null)
                            command.Transaction = transaction;

                        // Determine the active node ID - we cache this since this value won't change for the lifetime of this class
                        if (nodeID == Guid.Empty)
                            nodeID = Guid.Parse(command.ExecuteScalar($"SELECT NodeID FROM IaonActionAdapter WHERE ID = {(int)ID}", MetadataSynchronizationTimeout).ToString());

                        // Determine the protocol record auto-inc ID value for the gateway transport protocol (GEP) - this value is also cached since it shouldn't change for the lifetime of this class
                        if (virtualProtocolID == 0)
                            virtualProtocolID = int.Parse(command.ExecuteScalar("SELECT ID FROM Protocol WHERE Acronym = 'VirtualInput'", MetadataSynchronizationTimeout).ToString());

                        // Define SQL statement to query if parent device is already defined
                        string parentDeviceIDSql = database.ParameterizedQueryString("SELECT ID FROM Device WHERE Acronym = {0}", "acronym");

                        // Define SQL statement to insert new parent device record
                        string insertParentDeviceSql = database.ParameterizedQueryString("INSERT INTO Device(NodeID, HistorianID, Acronym, Name, ProtocolID, IsConcentrator, Enabled) " +
                            "VALUES ({0}, {1}, {2}, {3}, {4}, 1, 1)", "nodeID", "historianID", "acronym", "name", "protocolID");

                        // Query the actual record ID based on the known run-time ID for this subscriber device
                        object sourceID = command.ExecuteScalar(parentDeviceIDSql, MetadataSynchronizationTimeout, ParentDeviceAcronym);

                        if (sourceID == null || sourceID == DBNull.Value)
                        {
                            // Get a historian ID, but exclude the STAT historian
                            object randomHistorianID = command.ExecuteScalar($"SELECT ID FROM Historian WHERE Acronym <> 'STAT'", MetadataSynchronizationTimeout);
                            command.ExecuteNonQuery(insertParentDeviceSql, MetadataSynchronizationTimeout, database.Guid(nodeID), randomHistorianID, ParentDeviceAcronym, ParentDeviceAcronym, virtualProtocolID);
                            sourceID = command.ExecuteScalar(parentDeviceIDSql, MetadataSynchronizationTimeout, ParentDeviceAcronym);
                        }

                        int parentID = Convert.ToInt32(sourceID);

                        // Validate that the subscriber device is marked as a concentrator (we are about to associate children devices with it)
                        if (!command.ExecuteScalar($"SELECT IsConcentrator FROM Device WHERE ID = {parentID}", MetadataSynchronizationTimeout).ToString().ParseBoolean())
                            command.ExecuteNonQuery($"UPDATE Device SET IsConcentrator = 1 WHERE ID = {parentID}", MetadataSynchronizationTimeout);

                        // Get any historian associated with the subscriber device
                        object historianID = command.ExecuteScalar($"SELECT HistorianID FROM Device WHERE ID = {parentID}", MetadataSynchronizationTimeout);

                        // Ascertain total number of actions required for all metadata synchronization so some level feed back can be provided on progress
                        initSyncProgress(metadata.Tables.Cast<DataTable>().Sum(dataTable => (long)dataTable.Rows.Count) + 3);

                        // Prefix all children devices with the name of the parent since the same device names could appear in different connections (helps keep device names unique)
                        string sourcePrefix = UseSourcePrefixNames ? Name + "!" : "";
                        Dictionary<string, int> deviceIDs = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        string deviceAcronym, signalTypeAcronym;
                        int deviceID;

                        // Check to see if data for the "DeviceDetail" table was included in the metadata
                        if (metadata.Tables.Contains("DeviceDetail"))
                        {
                            DataTable deviceDetail = metadata.Tables["DeviceDetail"];
                            List<Guid> uniqueIDs = new List<Guid>();

                            // Define SQL statement to query if this device is already defined (this should always be based on the unique guid-based device ID)
                            string deviceExistsSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Device WHERE UniqueID = {0}", "uniqueID");

                            // Define SQL statement to insert new device record
                            string insertDeviceSql = database.ParameterizedQueryString("INSERT INTO Device(NodeID, ParentID, HistorianID, Acronym, Name, ProtocolID, FramesPerSecond, AccessID, Longitude, Latitude, ContactList, IsConcentrator, Enabled) " +
                                                                                       "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, 0, 1)", "nodeID", "parentID", "historianID", "acronym", "name", "protocolID", "framesPerSecond", "accessID", "longitude", "latitude", "contactList");

                            // Define SQL statement to update device's guid-based unique ID after insert
                            string updateDeviceUniqueIDSql = database.ParameterizedQueryString("UPDATE Device SET UniqueID = {0} WHERE Acronym = {1}", "uniqueID", "acronym");

                            // Define SQL statement to query if a device can be safely updated
                            string deviceIsUpdatableSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Device WHERE UniqueID = {0} AND (ParentID <> {1} OR ParentID IS NULL)", "uniqueID", "parentID");

                            // Define SQL statement to update existing device record
                            string updateDeviceSql = database.ParameterizedQueryString("UPDATE Device SET Acronym = {0}, Name = {1}, ProtocolID = {2}, FramesPerSecond = {3}, HistorianID = {4}, AccessID = {5}, Longitude = {6}, Latitude = {7}, ContactList = {8} WHERE UniqueID = {9}",
                                                                                       "acronym", "name", "protocolID", "framesPerSecond", "historianID", "accessID", "longitude", "latitude", "contactList", "uniqueID");

                            // Define SQL statement to retrieve device's auto-inc ID based on its unique guid-based ID
                            string queryDeviceIDSql = database.ParameterizedQueryString("SELECT ID FROM Device WHERE UniqueID = {0}", "uniqueID");

                            // Define SQL statement to retrieve all unique device ID's for the current parent to check for mismatches
                            string queryUniqueDeviceIDsSql = database.ParameterizedQueryString("SELECT UniqueID FROM Device WHERE ParentID = {0}", "parentID");

                            // Define SQL statement to remove device records that no longer exist in the metadata
                            string deleteDeviceSql = database.ParameterizedQueryString("DELETE FROM Device WHERE UniqueID = {0}", "uniqueID");

                            foreach (DataRow row in deviceDetail.Rows)
                            {
                                Guid uniqueID = Guid.Parse(row.Field<object>("UniqueID").ToString());
                                bool recordNeedsUpdating;

                                // Track unique device Guids in this metadata session, we'll need to remove any old associated devices that no longer exist
                                uniqueIDs.Add(uniqueID);

                                try
                                {
                                    updateTime = Convert.ToDateTime(row["UpdatedOn"]);
                                    recordNeedsUpdating = updateTime > m_lastMetaDataRefreshTime;

                                    if (updateTime > latestUpdateTime)
                                        latestUpdateTime = updateTime;
                                }
                                catch
                                {
                                    recordNeedsUpdating = true;
                                }

                                // We will synchronize metadata only if the source owns this device and it's not defined as a concentrator (these should normally be filtered by publisher - but we check just in case).
                                if (!row["IsConcentrator"].ToNonNullString("0").ParseBoolean())
                                {
                                    int accessID = row.ConvertField<int>("AccessID");

                                    // Get longitude and latitude values if they are defined
                                    decimal longitude = 0M;
                                    decimal latitude = 0M;
                                    decimal? location;

                                    location = row.ConvertNullableField<decimal>("Longitude");

                                    if (location.HasValue)
                                        longitude = location.Value;

                                    location = row.ConvertNullableField<decimal>("Latitude");

                                    if (location.HasValue)
                                        latitude = location.Value;

                                    // Save any reported extraneous values from device metadata in connection string formatted contact list - all fields are considered optional
                                    Dictionary<string, string> contactList = new Dictionary<string, string>();

                                    contactList["companyAcronym"] = row.Field<string>("CompanyAcronym") ?? string.Empty;
                                    contactList["protocolName"] = row.Field<string>("ProtocolName") ?? string.Empty;
                                    contactList["vendorAcronym"] = row.Field<string>("VendorAcronym") ?? string.Empty;
                                    contactList["vendorDeviceName"] = row.Field<string>("VendorDeviceName") ?? string.Empty;
                                    contactList["interconnectionName"] = row.Field<string>("InterconnectionName") ?? string.Empty;

                                    // Determine if device record already exists
                                    if (Convert.ToInt32(command.ExecuteScalar(deviceExistsSql, MetadataSynchronizationTimeout, database.Guid(uniqueID))) == 0)
                                    {
                                        // Insert new device record
                                        command.ExecuteNonQuery(insertDeviceSql, MetadataSynchronizationTimeout, database.Guid(nodeID), parentID, historianID, sourcePrefix + row.Field<string>("Acronym"),
                                            row.Field<string>("Name"), virtualProtocolID, row.ConvertField<int>("FramesPerSecond"), accessID, longitude, latitude, contactList.JoinKeyValuePairs());

                                        // Guids are normally auto-generated during insert - after insertion update the Guid so that it matches the source data. Most of the database
                                        // scripts have triggers that support properly assigning the Guid during an insert, but this code ensures the Guid will always get assigned.
                                        command.ExecuteNonQuery(updateDeviceUniqueIDSql, MetadataSynchronizationTimeout, database.Guid(uniqueID), sourcePrefix + row.Field<string>("Acronym"));
                                    }
                                    else if (recordNeedsUpdating)
                                    {
                                        // Perform safety check to preserve device records which are not safe to overwrite
                                        if (Convert.ToInt32(command.ExecuteScalar(deviceIsUpdatableSql, MetadataSynchronizationTimeout, database.Guid(uniqueID), parentID)) > 0)
                                            continue;

                                        // Update existing device record
                                        command.ExecuteNonQuery(updateDeviceSql, MetadataSynchronizationTimeout, sourcePrefix + row.Field<string>("Acronym"), row.Field<string>("Name"), virtualProtocolID, row.ConvertField<int>("FramesPerSecond"), historianID, accessID, longitude, latitude, contactList.JoinKeyValuePairs(), database.Guid(uniqueID));
                                    }
                                }

                                // Capture local device ID auto-inc value for measurement association
                                deviceIDs[row.Field<string>("Acronym")] = Convert.ToInt32(command.ExecuteScalar(queryDeviceIDSql, MetadataSynchronizationTimeout, database.Guid(uniqueID)));

                                // Periodically notify user about synchronization progress
                                updateSyncProgress();
                            }

                            // Remove any device records associated with this subscriber that no longer exist in the metadata
                            if (uniqueIDs.Count > 0)
                            {
                                // Sort unique ID list so that binary search can be used for quick lookups
                                uniqueIDs.Sort();

                                DataTable deviceUniqueIDs = command.RetrieveData(database.AdapterType, queryUniqueDeviceIDsSql, MetadataSynchronizationTimeout, parentID);
                                Guid uniqueID;

                                foreach (DataRow deviceRow in deviceUniqueIDs.Rows)
                                {
                                    uniqueID = database.Guid(deviceRow, "UniqueID");

                                    // Remove any devices in the database that are associated with the parent device and do not exist in the metadata
                                    if (uniqueIDs.BinarySearch(uniqueID) < 0)
                                        command.ExecuteNonQuery(deleteDeviceSql, MetadataSynchronizationTimeout, database.Guid(uniqueID));
                                }
                                updateSyncProgress();
                            }
                        }

                        // Check to see if data for the "MeasurementDetail" table was included in the metadata
                        if (metadata.Tables.Contains("MeasurementDetail"))
                        {
                            DataTable measurementDetail = metadata.Tables["MeasurementDetail"];
                            List<Guid> signalIDs = new List<Guid>();

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

                            // Load signal type ID's from local database associated with their acronym for proper signal type translation
                            Dictionary<string, int> signalTypeIDs = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                            foreach (DataRow row in command.RetrieveData(database.AdapterType, "SELECT ID, Acronym FROM SignalType", MetadataSynchronizationTimeout).Rows)
                            {
                                signalTypeAcronym = row.Field<string>("Acronym");

                                if (!string.IsNullOrWhiteSpace(signalTypeAcronym))
                                    signalTypeIDs[signalTypeAcronym] = row.ConvertField<int>("ID");
                            }

                            // Define local signal type ID deletion exclusion set
                            List<int> excludedSignalTypeIDs = new List<int>();
                            int signalTypeID;

                            // We are intentionally ignoring CALC and ALRM signals during measurement deletion since if you have subscribed to a device and subsequently created local
                            // calculations and alarms associated with this device, these signals are locally owned and not part of the publisher subscription stream. As a result any
                            // CALC or ALRM measurements that are created at source and then removed could be orphaned in subscriber. The best fix would be to have a simple flag that
                            // clearly designates that a measurement was created locally and is not part of the remote synchronization set.
                            if (signalTypeIDs.TryGetValue("CALC", out signalTypeID))
                                excludedSignalTypeIDs.Add(signalTypeID);

                            if (signalTypeIDs.TryGetValue("ALRM", out signalTypeID))
                                excludedSignalTypeIDs.Add(signalTypeID);

                            string exclusionExpression = "";

                            if (excludedSignalTypeIDs.Count > 0)
                                exclusionExpression = $" AND NOT SignalTypeID IN ({excludedSignalTypeIDs.ToDelimitedString(',')})";

                            // Define SQL statement to remove device records that no longer exist in the metadata
                            string deleteMeasurementSql = database.ParameterizedQueryString($"DELETE FROM Measurement WHERE SignalID = {{0}}{exclusionExpression}", "signalID");

                            // Check existence of optional metadata fields
                            DataColumnCollection measurementDetailColumns = measurementDetail.Columns;
                            bool phasorSourceIndexFieldExists = measurementDetailColumns.Contains("PhasorSourceIndex");
                            bool updatedOnFieldExists = measurementDetailColumns.Contains("UpdatedOn");

                            object phasorSourceIndex = DBNull.Value;

                            foreach (DataRow row in measurementDetail.Select())
                            {
                                bool recordNeedsUpdating;

                                // Determine if record has changed since last synchronization
                                if (updatedOnFieldExists)
                                {
                                    try
                                    {
                                        updateTime = Convert.ToDateTime(row["UpdatedOn"]);
                                        recordNeedsUpdating = updateTime > m_lastMetaDataRefreshTime;

                                        if (updateTime > latestUpdateTime)
                                            latestUpdateTime = updateTime;
                                    }
                                    catch
                                    {
                                        recordNeedsUpdating = true;
                                    }
                                }
                                else
                                {
                                    recordNeedsUpdating = true;
                                }

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
                                    Guid signalID = Guid.Parse(row.Field<object>("SignalID").ToString());

                                    // Track unique measurement signal Guids in this metadata session, we'll need to remove any old associated measurements that no longer exist
                                    signalIDs.Add(signalID);


                                    // Prefix the tag name with the "updated" device name
                                    string pointTag = sourcePrefix + row.Field<string>("PointTag");

                                    // Look up associated device ID (local DB auto-inc)
                                    deviceID = deviceIDs[deviceAcronym];

                                    // Determine if measurement record already exists
                                    if (Convert.ToInt32(command.ExecuteScalar(measurementExistsSql, MetadataSynchronizationTimeout, database.Guid(signalID))) == 0)
                                    {
                                        string alternateTag = Guid.NewGuid().ToString();

                                        // Insert new measurement record
                                        command.ExecuteNonQuery(insertMeasurementSql, MetadataSynchronizationTimeout, deviceID, historianID, pointTag, alternateTag, signalTypeIDs[signalTypeAcronym], phasorSourceIndex, sourcePrefix + row.Field<string>("SignalReference"), row.Field<string>("Description") ?? string.Empty, database.Bool(true));

                                        // Guids are normally auto-generated during insert - after insertion update the Guid so that it matches the source data. Most of the database
                                        // scripts have triggers that support properly assigning the Guid during an insert, but this code ensures the Guid will always get assigned.
                                        command.ExecuteNonQuery(updateMeasurementSignalIDSql, MetadataSynchronizationTimeout, database.Guid(signalID), alternateTag);
                                    }
                                    else if (recordNeedsUpdating)
                                    {
                                        // Update existing measurement record. Note that this update assumes that measurements will remain associated with a static source device.
                                        command.ExecuteNonQuery(updateMeasurementSql, MetadataSynchronizationTimeout, historianID, pointTag, signalTypeIDs[signalTypeAcronym], phasorSourceIndex, sourcePrefix + row.Field<string>("SignalReference"), row.Field<string>("Description") ?? string.Empty, database.Bool(true), database.Guid(signalID));
                                    }
                                }

                                // Periodically notify user about synchronization progress
                                updateSyncProgress();
                            }

                            // Remove any measurement records associated with existing devices in this session but no longer exist in the metadata
                            if (signalIDs.Count > 0)
                            {
                                // Sort signal ID list so that binary search can be used for quick lookups
                                signalIDs.Sort();

                                // Query all the guid-based signal ID's for all measurement records associated with the parent device using run-time ID
                                DataTable measurementSignalIDs = command.RetrieveData(database.AdapterType, queryMeasurementSignalIDsSql, MetadataSynchronizationTimeout, (int)ID);
                                Guid signalID;

                                // Walk through each database record and see if the measurement exists in the provided metadata
                                foreach (DataRow measurementRow in measurementSignalIDs.Rows)
                                {
                                    signalID = database.Guid(measurementRow, "SignalID");

                                    // Remove any measurements in the database that are associated with received devices and do not exist in the metadata
                                    if (signalIDs.BinarySearch(signalID) < 0)
                                    {
                                        // Measurement was not in the metadata, get the measurement's actual record based ID for its associated device
                                        object measurementDeviceID = command.ExecuteScalar(queryMeasurementDeviceIDSql, MetadataSynchronizationTimeout, database.Guid(signalID));

                                        // If the unknown measurement is directly associated with a device that exists in the metadata it is assumed that this measurement
                                        // was removed from the publishing system and no longer exists therefore we remove it from the local measurement cache. If the user
                                        // needs custom local measurements associated with a remote device, they should be associated with the parent device only.
                                        if (measurementDeviceID != null && !(measurementDeviceID is DBNull) && deviceIDs.ContainsValue(Convert.ToInt32(measurementDeviceID)))
                                            command.ExecuteNonQuery(deleteMeasurementSql, MetadataSynchronizationTimeout, database.Guid(signalID));
                                    }
                                }

                                updateSyncProgress();
                            }
                        }

                        // Check to see if data for the "PhasorDetail" table was included in the metadata
                        if (metadata.Tables.Contains("PhasorDetail"))
                        {
                            DataTable phasorDetail = metadata.Tables["PhasorDetail"];
                            Dictionary<int, List<int>> definedSourceIndicies = new Dictionary<int, List<int>>();
                            Dictionary<int, int> metadataToDatabaseIDMap = new Dictionary<int, int>();
                            Dictionary<int, int> sourceToDestinationIDMap = new Dictionary<int, int>();

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
                            string deletePhasorSql = database.ParameterizedQueryString("DELETE FROM Phasor WHERE DeviceID = {0}", "deviceID");

                            // Define SQL statement to query phasor record ID
                            string queryPhasorIDSql = database.ParameterizedQueryString("SELECT ID FROM Phasor WHERE DeviceID = {0} AND SourceIndex = {1}", "deviceID", "sourceIndex");

                            // Define SQL statement to update destinationPhasorID field of existing phasor record
                            string updateDestinationPhasorIDSql = database.ParameterizedQueryString("UPDATE Phasor SET DestinationPhasorID = {0} WHERE ID = {1}", "destinationPhasorID", "id");

                            // Check existence of optional metadata fields
                            DataColumnCollection phasorDetailColumns = phasorDetail.Columns;
                            bool phasorIDFieldExists = phasorDetailColumns.Contains("ID");
                            bool destinationPhasorIDFieldExists = phasorDetailColumns.Contains("DestinationPhasorID");

                            foreach (DataRow row in phasorDetail.Rows)
                            {
                                // Get device acronym
                                deviceAcronym = row.Field<string>("DeviceAcronym") ?? string.Empty;

                                // Make sure we have an associated device already defined for the phasor record
                                if (!string.IsNullOrWhiteSpace(deviceAcronym) && deviceIDs.ContainsKey(deviceAcronym))
                                {
                                    bool recordNeedsUpdating;

                                    // Determine if record has changed since last synchronization
                                    try
                                    {
                                        updateTime = Convert.ToDateTime(row["UpdatedOn"]);
                                        recordNeedsUpdating = updateTime > m_lastMetaDataRefreshTime;

                                        if (updateTime > latestUpdateTime)
                                            latestUpdateTime = updateTime;
                                    }
                                    catch
                                    {
                                        recordNeedsUpdating = true;
                                    }

                                    deviceID = deviceIDs[deviceAcronym];

                                    int sourceIndex = row.ConvertField<int>("SourceIndex");

                                    // Determine if phasor record already exists
                                    if (Convert.ToInt32(command.ExecuteScalar(phasorExistsSql, MetadataSynchronizationTimeout, deviceID, sourceIndex)) == 0)
                                    {
                                        // Insert new phasor record
                                        command.ExecuteNonQuery(insertPhasorSql, MetadataSynchronizationTimeout, deviceID, row.Field<string>("Label") ?? "undefined", (row.Field<string>("Type") ?? "V").TruncateLeft(1), (row.Field<string>("Phase") ?? "+").TruncateLeft(1), sourceIndex);
                                    }
                                    else if (recordNeedsUpdating)
                                    {
                                        // Update existing phasor record
                                        command.ExecuteNonQuery(updatePhasorSql, MetadataSynchronizationTimeout, row.Field<string>("Label") ?? "undefined", (row.Field<string>("Type") ?? "V").TruncateLeft(1), (row.Field<string>("Phase") ?? "+").TruncateLeft(1), deviceID, sourceIndex);
                                    }

                                    if (phasorIDFieldExists && destinationPhasorIDFieldExists)
                                    {
                                        int sourcePhasorID = row.ConvertField<int>("ID");

                                        // Using ConvertNullableField extension since publisher could use SQLite database in which case
                                        // all integers would arrive in data set as longs and need to be converted back to integers
                                        int? destinationPhasorID = row.ConvertNullableField<int>("DestinationPhasorID");

                                        if (destinationPhasorID.HasValue)
                                            sourceToDestinationIDMap[sourcePhasorID] = destinationPhasorID.Value;

                                        // Map all metadata phasor IDs to associated local database phasor IDs
                                        metadataToDatabaseIDMap[sourcePhasorID] = Convert.ToInt32(command.ExecuteScalar(queryPhasorIDSql, MetadataSynchronizationTimeout, deviceID, sourceIndex));
                                    }

                                    // Track defined phasors for each device
                                    definedSourceIndicies.GetOrAdd(deviceID, id => new List<int>()).Add(sourceIndex);
                                }

                                // Periodically notify user about synchronization progress
                                updateSyncProgress();
                            }

                            // Once all phasor records have been processed, handle updating of destination phasor IDs
                            foreach (KeyValuePair<int, int> item in sourceToDestinationIDMap)
                            {
                                int sourcePhasorID, destinationPhasorID;

                                if (metadataToDatabaseIDMap.TryGetValue(item.Key, out sourcePhasorID) && metadataToDatabaseIDMap.TryGetValue(item.Value, out destinationPhasorID))
                                    command.ExecuteNonQuery(updateDestinationPhasorIDSql, MetadataSynchronizationTimeout, destinationPhasorID, sourcePhasorID);
                            }

                            // Remove any phasor records associated with existing devices in this session but no longer exist in the metadata
                            foreach (int id in deviceIDs.Values)
                            {
                                List<int> sourceIndicies;

                                if (definedSourceIndicies.TryGetValue(id, out sourceIndicies))
                                    command.ExecuteNonQuery(deletePhasorSql + $" AND SourceIndex NOT IN ({string.Join(",", sourceIndicies)})", MetadataSynchronizationTimeout, id);
                                else
                                    command.ExecuteNonQuery(deletePhasorSql, MetadataSynchronizationTimeout, id);
                            }
                        }

                        if ((object)transaction != null)
                            transaction.Commit();

                        // Update local in-memory synchronized metadata cache
                        synchronizedMetadata = metadata;
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(MessageLevel.Error, new InvalidOperationException("Failed to synchronize metadata to local cache: " + ex.Message, ex));

                        if ((object)transaction != null)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch (Exception rollbackException)
                            {
                                OnProcessException(MessageLevel.Error, new InvalidOperationException("Failed to roll back database transaction due to exception: " + rollbackException.Message, rollbackException));
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

                m_lastMetaDataRefreshTime = latestUpdateTime > DateTime.MinValue ? latestUpdateTime : DateTime.UtcNow;

                OnStatusMessage(MessageLevel.Info, $"metadata synchronization completed successfully in {(DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(2)}");

                // Send notification that system configuration has changed
                OnConfigurationChanged();
            };

            Action<Exception> handleException = ex =>
            {
                OnProcessException(MessageLevel.Error, new InvalidOperationException("Failed to synchronize metadata to local cache: " + ex.Message, ex));
            };

            LongSynchronizedOperation synchronizeMetadataOperation = new LongSynchronizedOperation(synchronizeMetadata, handleException);

            return metadata =>
            {
                metadataToSynchronize = metadata;
                synchronizeMetadataOperation.RunOnceAsync();
            };
        }

        // Searches for metadata to be imported and imports the metadata if found.
        private void ScheduleManager_ScheduleDue(object sender, EventArgs<Schedule> e)
        {
            ImportMetadata();
        }

        #endregion
    }
}
