﻿//******************************************************************************************************
//  LocalOutputAdapter.cs - Gbtc
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
//  09/10/2009 - Pinal C. Patel
//       Generated original version of source code.
//  09/11/2009 - Pinal C. Patel
//       Added support to refresh metadata from one or more external sources.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/17/2009 - Pinal C. Patel
//       Added option to refresh metadata during connection.
//       Modified RefreshMetadata() to perform synchronous refresh.
//       Corrected the implementation of Dispose().
//  09/18/2009 - Pinal C. Patel
//       Added override to Status property and added event handler to archive rollover notification.
//  10/28/2009 - Pinal C. Patel
//       Modified to allow for multiple instances of the adapter to be loaded and configured with 
//       different settings by persisting the settings in the config file under unique categories.
//  11/18/2009 - Pinal C. Patel
//       Added support for the replication of local historian archive.
//  12/01/2009 - Pinal C. Patel
//       Modified Initialize() to load all available metadata providers.
//  12/11/2009 - Pinal C. Patel
//       Fixed the implementation for allowing multiple adapter instances.
//       Expanded the adapter status to include dynamically loaded plug-ins.
//  04/28/2010 - Pinal C. Patel
//       Modified ProcessMeasurements() method to not throw an exception if the archive file is not 
//       open as this will be handled by ArchiveFile.WriteData() method if necessary.
//  06/13/2010 - J. Ritchie Carroll
//       Modified loaded plug-in's to use lower-cased instance name for configuration settings for
//       consistency and better looking configuration categories. Added static data operation to 
//       automatically optimize settings for defined local historians.
//  09/24/2010 - J. Ritchie Carroll
//       Added provider and service section to list of category sections to be removed when unused. 
//       Added automatic URL namespace reservation for built-in web services.
//  11/07/2010 - Pinal C. Patel
//       Modified namespace reservation logic to handle the changed URI format in SelfHostingService.
//  11/21/2011 - J. Ritchie Carroll
//       Modified historian optimization procedure to dynamically add reading adapters.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GSF;
using GSF.Collections;
using GSF.Configuration;
using GSF.Data;
using GSF.Diagnostics;
using GSF.Historian;
using GSF.Historian.DataServices;
using GSF.Historian.Files;
using GSF.Historian.MetadataProviders;
using GSF.Historian.Replication;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace HistorianAdapters;

/// <summary>
/// Represents an output adapter that archives measurements to a local archive.
/// </summary>
[Description("Local v1.0 openHistorian: [Deprecated for v2.0, see openHistorian.com] Archives measurements to a local in-process 1.0 openHistorian.")]
public class LocalOutputAdapter : OutputAdapterBase
{
    #region [ Members ]

    // Fields
    private readonly ArchiveFile m_archive;
    private DataServices m_dataServices;
    private MetadataProviders m_metadataProviders;
    private ReplicationProviders m_replicationProviders;
    private bool m_attemptingConnection;
    private string m_instanceName;
    private string m_archivePath;
    private long m_archivedMeasurements;
    private long m_adapterLoadedCount;
    private ulong m_badDataMessageInterval;
    private readonly Dictionary<int, ulong> m_orphanCounts;
    private readonly ProcessQueue<IDataPoint> m_orphanQueue;
    private readonly Dictionary<int, ulong> m_badTimestampCounts;
    private readonly ProcessQueue<IDataPoint> m_badTimestampQueue;
    private readonly Dictionary<int, ulong> m_outOfSequenceCounts;
    private readonly ProcessQueue<IDataPoint> m_outOfSequenceQueue;
    private readonly ShortSynchronizedOperation m_updateArchiveFileSettings;
    private bool m_disposed;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalOutputAdapter"/> class.
    /// </summary>
    public LocalOutputAdapter()
    {
        AutoRefreshMetadata = true;

        m_archive = new ArchiveFile
        { 
            MetadataFile = new MetadataFile(),
            StateFile = new StateFile(),
            IntercomFile = new IntercomFile()
        };

        MetadataRefreshOperation.IsBackground = false;
        
        m_badDataMessageInterval = 900;
        m_orphanCounts = new Dictionary<int, ulong>();
        m_orphanQueue = ProcessQueue<IDataPoint>.CreateRealTimeQueue(HandleOrphanData);
        m_badTimestampCounts = new Dictionary<int, ulong>();
        m_badTimestampQueue = ProcessQueue<IDataPoint>.CreateRealTimeQueue(HandleBadTimestampData);
        m_outOfSequenceCounts = new Dictionary<int, ulong>();
        m_outOfSequenceQueue = ProcessQueue<IDataPoint>.CreateRealTimeQueue(HandleOutOfSequenceData);
        
        m_updateArchiveFileSettings = new ShortSynchronizedOperation(UpdateArchiveFileSettings, 
            ex => OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed while updating archive file configuration parameters after configuration reload: {ex.Message}", ex)));
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets instance name defined for this <see cref="LocalOutputAdapter"/>.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the instance name for the archive. Leave this value blank to default to the adapter name (blank is typical setting).")]
    [DefaultValue("")]
    public string InstanceName
    {
        get => m_instanceName;
        set => m_instanceName = value;
    }

    /// <summary>
    /// Gets or sets a boolean indicating whether metadata is refreshed when the adapter attempts to connect to the archive.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define a boolean indicating whether to refresh metadata from database on connect.")]
    [DefaultValue(true)]
    public bool AutoRefreshMetadata { get; set; }

    /// <summary>
    /// Gets or sets primary keys of input measurements the <see cref="AdapterBase"/> expects, if any.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override MeasurementKey[] InputMeasurementKeys
    {
        get => base.InputMeasurementKeys;
        set => base.InputMeasurementKeys = value;
    }

    /// <summary>
    /// Gets or sets the path to the archive.
    /// </summary>
    public string ArchivePath
    {
        get => m_archivePath;
        set => m_archivePath = value;
    }

    /// <summary>
    /// Gets connection info for adapter, if any.
    /// </summary>
    public override string ConnectionInfo => $"{InstanceName} [{ArchivePath}]";

    /// <summary>
    /// Returns a flag that determines if measurements sent to this <see cref="LocalOutputAdapter"/> are destined for archival.
    /// </summary>
    public override bool OutputIsForArchive => true;

    /// <summary>
    /// Gets flag that determines if this <see cref="LocalOutputAdapter"/> uses an asynchronous connection.
    /// </summary>
    protected override bool UseAsyncConnect => true;

    /// <summary>
    /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="LocalOutputAdapter"/>.
    /// </summary>
    public override DataSet DataSource
    {
        get => base.DataSource;
        set
        {
            base.DataSource = value;

            // When configuration is reloaded, queue update to adjust archive file size parameters
            m_updateArchiveFileSettings.RunOnceAsync();
        }
    }

    /// <summary>
    /// Returns the detailed status of the data output source.
    /// </summary>
    public override string Status
    {
        get
        {
            StringBuilder status = new();

            status.AppendLine(base.Status);
            status.AppendLine(m_archive.Status);
            status.AppendLine(m_archive.MetadataFile.Status);
            status.AppendLine(m_archive.StateFile.Status);
            status.AppendLine(m_archive.IntercomFile.Status);
            status.AppendLine(m_dataServices.Status);
            status.AppendLine(m_metadataProviders.Status);
            status.Append(m_replicationProviders.Status);

            return status.ToString();
        }
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Refreshes metadata using all available and enabled providers.
    /// </summary>
    protected override void ExecuteMetadataRefresh()
    {
        try
        {
            if (m_archive is not { IsOpen: true, StateFile.IsOpen: true, MetadataFile.IsOpen: true })
                return;

            if (!m_attemptingConnection)
            {
                OnStatusMessage(MessageLevel.Info, "Pausing measurement processing...");
                InternalProcessQueue.Stop();
            }

            // Synchronously refresh the meta-base.
            lock (m_metadataProviders.Adapters)
            {
                foreach (IMetadataProvider provider in m_metadataProviders.Adapters)
                {
                    if (provider.Enabled)
                        provider.Refresh();
                }
            }

            // Request a state file synchronization in case file watchers are disabled
            m_archive.SynchronizeStateFile();

            // Wait for the meta-base to synchronize, up to five seconds
            int waitCounts = 0;

            while (m_archive.StateFile.RecordsOnDisk != m_archive.MetadataFile.RecordsOnDisk && waitCounts < 50)
            {
                Thread.Sleep(100);
                waitCounts++;
            }
        }
        finally
        {
            if (Enabled && !InternalProcessQueue.Enabled)
            {
                if (m_attemptingConnection)
                {
                    // OnConnected starts the internal process queue
                    // and sets m_attemptingConnection to false
                    OnConnected();
                }
                else
                {
                    OnStatusMessage(MessageLevel.Info, "Resuming measurement processing...");
                    InternalProcessQueue.Start();
                }
            }
        }
    }

    /// <summary>
    /// Initializes this <see cref="LocalOutputAdapter"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><b>InstanceName</b> is missing from the <see cref="AdapterBase.Settings"/>.</exception>
    public override void Initialize()
    {
        base.Initialize();

        Dictionary<string, string> settings = Settings;

        // Validate settings.
        if (!settings.TryGetValue("instanceName", out m_instanceName) || string.IsNullOrWhiteSpace(m_instanceName))
            m_instanceName = Name.ToLower();

        // Track instance in static dictionary
        Instances[InstanceName] = this;

        if (!settings.TryGetValue("archivePath", out m_archivePath))
            m_archivePath = FilePath.GetAbsolutePath(FilePath.AddPathSuffix("Archive"));

        if (settings.TryGetValue("refreshMetadata", out string setting) || settings.TryGetValue("autoRefreshMetadata", out setting))
            AutoRefreshMetadata = setting.ParseBoolean();

        if (settings.TryGetValue("badDataMessageInterval", out setting) && ulong.TryParse(setting, out ulong badDataMessageInterval))
            m_badDataMessageInterval = badDataMessageInterval;

        //if (settings.TryGetValue("useNamespaceReservation", out setting))
        //    m_useNamespaceReservation = setting.ParseBoolean();
        //else
        //    m_useNamespaceReservation = false;

        // Initialize metadata file.
        m_instanceName = m_instanceName.ToLower();
        m_archive.MetadataFile.FileName = Path.Combine(m_archivePath, m_instanceName + "_dbase.dat");
        m_archive.MetadataFile.PersistSettings = true;
        m_archive.MetadataFile.SettingsCategory = m_instanceName + m_archive.MetadataFile.SettingsCategory;
        m_archive.MetadataFile.FileAccessMode = FileAccess.ReadWrite;
        m_archive.MetadataFile.Initialize();

        // Initialize state file.
        m_archive.StateFile.FileName = Path.Combine(m_archivePath, m_instanceName + "_startup.dat");
        m_archive.StateFile.PersistSettings = true;
        m_archive.StateFile.SettingsCategory = m_instanceName + m_archive.StateFile.SettingsCategory;
        m_archive.StateFile.FileAccessMode = FileAccess.ReadWrite;
        m_archive.StateFile.Initialize();

        // Initialize intercom file.
        m_archive.IntercomFile.FileName = Path.Combine(m_archivePath, "scratch.dat");
        m_archive.IntercomFile.PersistSettings = true;
        m_archive.IntercomFile.SettingsCategory = m_instanceName + m_archive.IntercomFile.SettingsCategory;
        m_archive.IntercomFile.FileAccessMode = FileAccess.ReadWrite;
        m_archive.IntercomFile.Initialize();

        // Initialize data archive file.           
        m_archive.FileName = Path.Combine(m_archivePath, m_instanceName + "_archive.d");
        m_archive.FileSize = 100;
        m_archive.CompressData = false;
        m_archive.PersistSettings = true;
        m_archive.SettingsCategory = m_instanceName + m_archive.SettingsCategory;

        m_archive.RolloverStart += m_archive_RolloverStart;
        m_archive.RolloverComplete += m_archive_RolloverComplete;
        m_archive.RolloverException += m_archive_RolloverException;

        m_archive.DataReadException += m_archive_DataReadException;
        m_archive.DataWriteException += m_archive_DataWriteException;

        m_archive.OffloadStart += m_archive_OffloadStart;
        m_archive.OffloadComplete += m_archive_OffloadComplete;
        m_archive.OffloadException += m_archive_OffloadException;

        m_archive.OrphanDataReceived += m_archive_OrphanDataReceived;
        m_archive.FutureDataReceived += m_archive_FutureDataReceived;
        m_archive.OutOfSequenceDataReceived += m_archive_OutOfSequenceDataReceived;

        m_archive.Initialize();

        // The archive path may have changed based on the configuration file
        m_archivePath = FilePath.GetDirectoryName(m_archive.FileName);

        // Provide web service support.
        m_dataServices = new DataServices();
        m_dataServices.AdapterCreated += DataServices_AdapterCreated;
        m_dataServices.AdapterLoaded += DataServices_AdapterLoaded;
        m_dataServices.AdapterUnloaded += DataServices_AdapterUnloaded;
        m_dataServices.AdapterLoadException += AdapterLoader_AdapterLoadException;

        // Provide metadata sync support.
        m_metadataProviders = new MetadataProviders();
        m_metadataProviders.AdapterCreated += MetadataProviders_AdapterCreated;
        m_metadataProviders.AdapterLoaded += MetadataProviders_AdapterLoaded;
        m_metadataProviders.AdapterUnloaded += MetadataProviders_AdapterUnloaded;
        m_metadataProviders.AdapterLoadException += AdapterLoader_AdapterLoadException;

        // Provide archive replication support.
        m_replicationProviders = new ReplicationProviders();
        m_replicationProviders.AdapterCreated += ReplicationProviders_AdapterCreated;
        m_replicationProviders.AdapterLoaded += ReplicationProviders_AdapterLoaded;
        m_replicationProviders.AdapterUnloaded += ReplicationProviders_AdapterUnloaded;
        m_replicationProviders.AdapterLoadException += AdapterLoader_AdapterLoadException;
    }

    /// <summary>
    /// Gets a short one-line status of this <see cref="LocalOutputAdapter"/>.
    /// </summary>
    /// <param name="maxLength">Maximum length of the status message.</param>
    /// <returns>Text of the status message.</returns>
    public override string GetShortStatus(int maxLength)
    {
        return $"Archived {m_archivedMeasurements:N0} measurements locally.".CenterText(maxLength);
    }

    /// <summary>
    /// Releases the unmanaged resources used by this <see cref="LocalOutputAdapter"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (m_disposed)
            return;

        try
        {
            // This will be done regardless of whether the object is finalized or disposed.
            if (!disposing)
                return;

            // This will be done only when the object is disposed by calling Dispose().
            if (m_dataServices is not null)
            {
                m_dataServices.AdapterCreated -= DataServices_AdapterCreated;
                m_dataServices.AdapterLoaded -= DataServices_AdapterLoaded;
                m_dataServices.AdapterUnloaded -= DataServices_AdapterUnloaded;
                m_dataServices.AdapterLoadException -= AdapterLoader_AdapterLoadException;
                m_dataServices.Dispose();
            }

            if (m_metadataProviders is not null)
            {
                m_metadataProviders.AdapterCreated -= MetadataProviders_AdapterCreated;
                m_metadataProviders.AdapterLoaded -= MetadataProviders_AdapterLoaded;
                m_metadataProviders.AdapterUnloaded -= MetadataProviders_AdapterUnloaded;
                m_metadataProviders.AdapterLoadException -= AdapterLoader_AdapterLoadException;
                m_metadataProviders.Dispose();
            }

            if (m_replicationProviders is not null)
            {
                m_replicationProviders.AdapterCreated -= ReplicationProviders_AdapterCreated;
                m_replicationProviders.AdapterLoaded -= ReplicationProviders_AdapterLoaded;
                m_replicationProviders.AdapterUnloaded -= ReplicationProviders_AdapterUnloaded;
                m_replicationProviders.AdapterLoadException -= AdapterLoader_AdapterLoadException;
                m_replicationProviders.Dispose();
            }

            if (m_archive is not null)
            {
                m_archive.RolloverStart -= m_archive_RolloverStart;
                m_archive.RolloverComplete -= m_archive_RolloverComplete;
                m_archive.RolloverException -= m_archive_RolloverException;

                m_archive.DataReadException -= m_archive_DataReadException;
                m_archive.DataWriteException -= m_archive_DataWriteException;

                m_archive.OffloadStart -= m_archive_OffloadStart;
                m_archive.OffloadComplete -= m_archive_OffloadComplete;
                m_archive.OffloadException -= m_archive_OffloadException;

                m_archive.FutureDataReceived -= m_archive_FutureDataReceived;
                m_archive.OutOfSequenceDataReceived -= m_archive_OutOfSequenceDataReceived;

                m_archive.Dispose();

                if (m_archive.MetadataFile is not null)
                {
                    m_archive.MetadataFile.Dispose();
                    m_archive.MetadataFile = null;
                }

                if (m_archive.StateFile is not null)
                {
                    m_archive.StateFile.Dispose();
                    m_archive.StateFile = null;
                }

                if (m_archive.IntercomFile is not null)
                {
                    m_archive.IntercomFile.Dispose();
                    m_archive.IntercomFile = null;
                }
            }
        }
        finally
        {
            m_disposed = true;          // Prevent duplicate dispose.
            base.Dispose(disposing);    // Call base class Dispose().
        }
    }

    /// <summary>
    /// Called when data output source connection is established.
    /// </summary>
    protected override void OnConnected()
    {
        m_attemptingConnection = false;
        base.OnConnected();
    }

    /// <summary>
    /// Attempts to connect to this <see cref="LocalOutputAdapter"/>.
    /// </summary>
    protected override void AttemptConnection()
    {
        m_attemptingConnection = true;

        // Open archive metadata file monitoring for checksum mismatch
        try
        {
            m_archive.MetadataFile.Open();
        }
        catch (InvalidDataException)
        {
            if (AutoRefreshMetadata)
            {
                OnStatusMessage(MessageLevel.Warning, "Detected corrupted metadata file, attempting regeneration...");

                // Force a manual refresh of metadata providers - this will regenerate metadata file
                lock (m_metadataProviders.Adapters)
                {
                    foreach (IMetadataProvider provider in m_metadataProviders.Adapters)
                    {
                        if (provider.Enabled)
                            provider.Refresh();
                    }
                }
            }
            else
            {
                OnStatusMessage(MessageLevel.Error, $"Detected corrupted metadata file and {nameof(AutoRefreshMetadata)} is not enabled - cancelling connection.");
                Stop();
            }

            // Report exception, this will reattempt connection if adapter remains enabled
            throw;
        }

        // Open remaining archive files
        m_archive.StateFile.Open();
        m_archive.IntercomFile.Open();
        m_archive.Open();

        Interlocked.Exchange(ref m_adapterLoadedCount, 0);

        // Initialization of services needs to occur after files are open
        m_dataServices.Initialize();
        m_metadataProviders.Initialize();
        m_replicationProviders.Initialize();

        int waitCount = 0;

        // Wait for adapter initialization to complete, up to 2 seconds
        while (waitCount < 20 && Interlocked.Read(ref m_adapterLoadedCount) != m_dataServices.Adapters.Count + m_metadataProviders.Adapters.Count + m_replicationProviders.Adapters.Count)
        {
            Thread.Sleep(100);
            waitCount++;

            if (m_disposed)
                return;
        }

        // Kick off a meta-data refresh...
        if (AutoRefreshMetadata)
            RefreshMetadata();
        else
            OnConnected();

        m_orphanQueue.Start();
        m_badTimestampQueue.Start();
        m_outOfSequenceQueue.Start();
    }

    /// <summary>
    /// Attempts to disconnect from this <see cref="LocalOutputAdapter"/>.
    /// </summary>
    protected override void AttemptDisconnection()
    {
        m_attemptingConnection = false;

        if (m_archive is null)
            return;

        if (m_archive.IsOpen)
        {
            m_archive.Save();
            m_archive.Close();
        }

        if (m_archive.MetadataFile is { IsOpen: true })
        {
            m_archive.MetadataFile.Save();
            m_archive.MetadataFile.Close();
        }

        if (m_archive.StateFile is { IsOpen: true })
        {
            m_archive.StateFile.Save();
            m_archive.StateFile.Close();
        }

        if (m_archive.IntercomFile is { IsOpen: true })
        {
            m_archive.IntercomFile.Save();
            m_archive.IntercomFile.Close();
        }

        OnDisconnected();
        m_archivedMeasurements = 0;
    }

    /// <summary>
    /// Archives <paramref name="measurements"/> locally.
    /// </summary>
    /// <param name="measurements">Measurements to be archived.</param>
    /// <exception cref="InvalidOperationException">Local archive is closed.</exception>
    protected override void ProcessMeasurements(IMeasurement[] measurements)
    {
        foreach (IMeasurement measurement in measurements)
        {
            if (WriteData(measurement))
                m_archivedMeasurements++;
        }
    }

    private bool WriteData(IMeasurement measurement)
    {
        try
        {
            m_archive.WriteData(new ArchiveDataPoint(measurement));
            return true;
        }
        catch (Exception ex)
        {
            OnProcessException(MessageLevel.Warning, ex);
        }

        return false;
    }

    // Do not invoke directly, call m_updateArchiveFileSettings.RunOnceAsync() to queue an operation
    private void UpdateArchiveFileSettings()
    {
        // Error handling managed by short synchronized operation wrapper
        ConfigurationFile configFile = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = configFile.Settings[m_archive.SettingsCategory];

        settings.Add("FileSize", m_archive.FileSize, "Size (in MB) of the file. Typical size = 100.");
        settings.Add("DataBlockSize", m_archive.DataBlockSize, "Size (in KB) of the data blocks in the file.");
        settings.Add("RolloverPreparationThreshold", m_archive.RolloverPreparationThreshold, "Percentage file full when the rollover preparation should begin.");

        m_archive.FileSize = settings["FileSize"].ValueAs(m_archive.FileSize);
        m_archive.DataBlockSize = settings["DataBlockSize"].ValueAs(m_archive.DataBlockSize);
        m_archive.RolloverPreparationThreshold = settings["RolloverPreparationThreshold"].ValueAs(m_archive.RolloverPreparationThreshold);
    }

    private void m_archive_RolloverStart(object sender, EventArgs e)
    {
        OnStatusMessage(MessageLevel.Info, "Archive is being rolled over...");
    }

    private void m_archive_RolloverComplete(object sender, EventArgs e)
    {
        OnStatusMessage(MessageLevel.Info, "Archive rollover is complete.");
    }

    private void m_archive_RolloverException(object sender, EventArgs<Exception> e)
    {
        OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Archive rollover failed: {e.Argument.Message}", e.Argument));
    }

    private void m_archive_DataReadException(object sender, EventArgs<Exception> e)
    {
        OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Archive data read exception: {e.Argument.Message}", e.Argument));
    }

    private void m_archive_DataWriteException(object sender, EventArgs<Exception> e)
    {
        OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Archive write read exception: {e.Argument.Message}", e.Argument));
    }

    private void m_archive_OffloadStart(object sender, EventArgs e)
    {
        OnStatusMessage(MessageLevel.Info, "Archive offload started...");
    }

    private void m_archive_OffloadComplete(object sender, EventArgs e)
    {
        OnStatusMessage(MessageLevel.Info, "Archive offload complete.");
    }

    private void m_archive_OffloadException(object sender, EventArgs<Exception> e)
    {
        OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Archive offload exception: {e.Argument.Message}", e.Argument));
    }

    private void m_archive_OrphanDataReceived(object sender, EventArgs<IDataPoint> e)
    {
        // In case we are receiving many points with bad timestamps, we throttle user messages
        m_orphanQueue?.Add(e.Argument);
    }

    private void HandleOrphanData(IDataPoint point)
    {
        if (point is null)
            return;

        int id = point.HistorianID;
        ulong total = m_orphanCounts.GetOrAdd(id, 0UL);

        if (total % m_badDataMessageInterval == 0UL)
        {
            OnStatusMessage(MessageLevel.Warning, total > 0UL ? 
                $"Received {total:N0} points of orphaned data for {m_instanceName}:{id} @{point.Time:yyyy-MM-dd HH:mm:ss.fffffff}. Measurements which are no longer defined in the metadata will not be archived." : 
                $"Received orphaned data for point {m_instanceName}:{id} @{point.Time:yyyy-MM-dd HH:mm:ss.fffffff}. Measurements which are no longer defined in the metadata will not be archived.");
        }

        m_orphanCounts[id] = total + 1UL;
    }

    private void m_archive_FutureDataReceived(object sender, EventArgs<IDataPoint> e)
    {
        // In case we are receiving many points with bad timestamps, we throttle user messages
        m_badTimestampQueue?.Add(e.Argument);
    }

    private void HandleBadTimestampData(IDataPoint point)
    {
        if (point is null)
            return;

        int id = point.HistorianID;
        ulong total = m_badTimestampCounts.GetOrAdd(id, 0UL);

        if (total % m_badDataMessageInterval == 0UL)
        {
            OnStatusMessage(MessageLevel.Warning, total > 0UL ? 
                $"Received {total:N0} points of data for {m_instanceName}:{id} @{point.Time:yyyy-MM-dd HH:mm:ss.fffffff} with an unreasonable future timestamp. Data with a timestamp beyond {m_archive.LeadTimeTolerance:0.00} minutes of the local clock will not be archived. Check local system clock and data source clock for accuracy." : 
                $"Received data for point {m_instanceName}:{id} @{point.Time:yyyy-MM-dd HH:mm:ss.fffffff} with an unreasonable future timestamp. Data with a timestamp beyond {m_archive.LeadTimeTolerance:0.00} minutes of the local clock will not be archived. Check local system clock and data source clock for accuracy.");
        }

        m_badTimestampCounts[id] = total + 1UL;
    }

    private void m_archive_OutOfSequenceDataReceived(object sender, EventArgs<IDataPoint> e)
    {
        // In case we are receiving many out-of-sequence points, we throttle user messages
        m_outOfSequenceQueue?.Add(e.Argument);
    }

    private void HandleOutOfSequenceData(IDataPoint point)
    {
        if (point is null)
            return;

        int id = point.HistorianID;
        ulong total = m_outOfSequenceCounts.GetOrAdd(id, 0UL);

        if (total % m_badDataMessageInterval == 0UL)
        {
            OnStatusMessage(MessageLevel.Warning, total > 0UL ? 
                $"Received {total:N0} points of out-of-sequence data for {m_instanceName}:{id} @{point.Time:yyyy-MM-dd HH:mm:ss.fffffff}. Data received out of order will not be archived, per configuration." : 
                $"Received out-of-sequence data for {m_instanceName}:{id} @{point.Time:yyyy-MM-dd HH:mm:ss.fffffff}. Data received out of order will not be archived, per configuration.");
        }

        m_outOfSequenceCounts[id] = total + 1UL;
    }

    private void DataServices_AdapterCreated(object sender, EventArgs<IDataService> e)
    {
        e.Argument.SettingsCategory = InstanceName + e.Argument.SettingsCategory;
    }

    private void DataServices_AdapterLoaded(object sender, EventArgs<IDataService> e)
    {
        e.Argument.Archive = m_archive;
        e.Argument.ServiceProcessException += DataServices_ServiceProcessException;
        OnStatusMessage(MessageLevel.Info, $"{e.Argument.GetType().Name} has been loaded.");

        Interlocked.Increment(ref m_adapterLoadedCount);
    }

    private void DataServices_AdapterUnloaded(object sender, EventArgs<IDataService> e)
    {
        e.Argument.Archive = null;
        e.Argument.ServiceProcessException -= DataServices_ServiceProcessException;
        OnStatusMessage(MessageLevel.Info, $"{e.Argument.GetType().Name} has been unloaded.");
    }

    private void MetadataProviders_AdapterCreated(object sender, EventArgs<IMetadataProvider> e)
    {
        e.Argument.SettingsCategory = InstanceName + e.Argument.SettingsCategory;

        if (e.Argument.GetType() != typeof(AdoMetadataProvider))
            return;

        // Populate the default configuration for AdoMetadataProvider.
        if (e.Argument is not AdoMetadataProvider provider)
            return;

        provider.Enabled = true;
        provider.SelectString = $"SELECT * FROM HistorianMetadata WHERE PlantCode='{Name}'";
    }

    private void MetadataProviders_AdapterLoaded(object sender, EventArgs<IMetadataProvider> e)
    {
        e.Argument.Metadata = m_archive.MetadataFile;
        e.Argument.MetadataRefreshStart += MetadataProviders_MetadataRefreshStart;
        e.Argument.MetadataRefreshComplete += MetadataProviders_MetadataRefreshComplete;
        e.Argument.MetadataRefreshTimeout += MetadataProviders_MetadataRefreshTimeout;
        e.Argument.MetadataRefreshException += MetadataProviders_MetadataRefreshException;

        OnStatusMessage(MessageLevel.Info, $"{e.Argument.GetType().Name} has been loaded.");
        Interlocked.Increment(ref m_adapterLoadedCount);
    }

    private void MetadataProviders_AdapterUnloaded(object sender, EventArgs<IMetadataProvider> e)
    {
        e.Argument.Metadata = null;
        e.Argument.MetadataRefreshStart -= MetadataProviders_MetadataRefreshStart;
        e.Argument.MetadataRefreshComplete -= MetadataProviders_MetadataRefreshComplete;
        e.Argument.MetadataRefreshTimeout -= MetadataProviders_MetadataRefreshTimeout;
        e.Argument.MetadataRefreshException -= MetadataProviders_MetadataRefreshException;

        OnStatusMessage(MessageLevel.Info, $"{e.Argument.GetType().Name} has been unloaded.");
    }

    private void ReplicationProviders_AdapterCreated(object sender, EventArgs<IReplicationProvider> e)
    {
        e.Argument.SettingsCategory = InstanceName + e.Argument.SettingsCategory;
    }

    private void ReplicationProviders_AdapterLoaded(object sender, EventArgs<IReplicationProvider> e)
    {
        e.Argument.ReplicationStart += ReplicationProvider_ReplicationStart;
        e.Argument.ReplicationComplete += ReplicationProvider_ReplicationComplete;
        e.Argument.ReplicationProgress += ReplicationProvider_ReplicationProgress;
        e.Argument.ReplicationException += ReplicationProvider_ReplicationException;

        OnStatusMessage(MessageLevel.Info, $"{e.Argument.GetType().Name} has been loaded.");
        Interlocked.Increment(ref m_adapterLoadedCount);
    }

    private void ReplicationProviders_AdapterUnloaded(object sender, EventArgs<IReplicationProvider> e)
    {
        e.Argument.ReplicationStart -= ReplicationProvider_ReplicationStart;
        e.Argument.ReplicationComplete -= ReplicationProvider_ReplicationComplete;
        e.Argument.ReplicationProgress -= ReplicationProvider_ReplicationProgress;
        e.Argument.ReplicationException -= ReplicationProvider_ReplicationException;

        OnStatusMessage(MessageLevel.Info, $"{e.Argument.GetType().Name} has been unloaded.");
    }

    private void AdapterLoader_AdapterLoadException(object sender, EventArgs<Exception> e)
    {
        OnProcessException(MessageLevel.Warning, e.Argument);
    }

    private void DataServices_ServiceProcessException(object sender, EventArgs<Exception> e)
    {
        OnProcessException(MessageLevel.Warning, e.Argument);
    }

    private void MetadataProviders_MetadataRefreshStart(object sender, EventArgs e)
    {
        OnStatusMessage(MessageLevel.Info, $"{sender.GetType().Name} has started metadata refresh...");
    }

    private void MetadataProviders_MetadataRefreshComplete(object sender, EventArgs e)
    {
        OnStatusMessage(MessageLevel.Info, $"{sender.GetType().Name} has finished metadata refresh.");
    }

    private void MetadataProviders_MetadataRefreshTimeout(object sender, EventArgs e)
    {
        OnStatusMessage(MessageLevel.Warning, $"{sender.GetType().Name} has timed-out on metadata refresh.");
    }

    private void MetadataProviders_MetadataRefreshException(object sender, EventArgs<Exception> e)
    {
        OnProcessException(MessageLevel.Warning, e.Argument);
    }

    private void ReplicationProvider_ReplicationStart(object sender, EventArgs e)
    {
        OnStatusMessage(MessageLevel.Info, $"{sender.GetType().Name} has started archive replication...");
    }

    private void ReplicationProvider_ReplicationComplete(object sender, EventArgs e)
    {
        OnStatusMessage(MessageLevel.Info, $"{sender.GetType().Name} has finished archive replication.");
    }

    private void ReplicationProvider_ReplicationProgress(object sender, EventArgs<ProcessProgress<int>> e)
    {
        OnStatusMessage(MessageLevel.Info, $"{sender.GetType().Name} has replicated archive file {e.Argument.ProgressMessage}.");
    }

    private void ReplicationProvider_ReplicationException(object sender, EventArgs<Exception> e)
    {
        OnProcessException(MessageLevel.Warning, e.Argument);
    }

    #endregion

    #region [ Static ]

    /// <summary>
    /// Accesses local output adapter instances.
    /// </summary>
    public static readonly ConcurrentDictionary<string, LocalOutputAdapter> Instances = new(StringComparer.OrdinalIgnoreCase);

    // Static Methods

    // Apply historian configuration optimizations at start-up
    // ReSharper disable UnusedMember.Local
    // ReSharper disable UnusedParameter.Local
    private static void OptimizeLocalHistorianSettings(AdoDataConnection database, string nodeIDQueryString, ulong trackingVersion, string arguments, Action<string> statusMessage, Action<Exception> processException)
    {
        // Make sure setting exists to allow user to by-pass local historian optimizations at startup
        ConfigurationFile configFile = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = configFile.Settings["systemSettings"];
        settings.Add("OptimizeLocalHistorianSettings", true, "Determines if the defined local historians will have their settings optimized at startup");

        // See if this node should optimize local historian settings
        if (!settings["OptimizeLocalHistorianSettings"].ValueAsBoolean())
            return;

        statusMessage("Optimizing settings for local historians...");

        // Load the defined local system historians
        IEnumerable<DataRow> historians = database.Connection.RetrieveData(database.AdapterType, $"SELECT AdapterName FROM RuntimeHistorian WHERE NodeID = {nodeIDQueryString} AND TypeName = 'HistorianAdapters.LocalOutputAdapter'").AsEnumerable();
        IEnumerable<DataRow> readers = database.Connection.RetrieveData(database.AdapterType, $"SELECT * FROM CustomInputAdapter WHERE NodeID = {nodeIDQueryString} AND TypeName = 'HistorianAdapters.LocalInputAdapter'").AsEnumerable();

        // Also check for local historian adapters loaded into CustomOutputAdapters
        historians = historians.Concat(database.Connection.RetrieveData(database.AdapterType, $"SELECT AdapterName, ConnectionString FROM RuntimeCustomOutputAdapter WHERE NodeID = {nodeIDQueryString} AND TypeName = 'HistorianAdapters.LocalOutputAdapter'").AsEnumerable());

        string name, acronym, instanceName;

        // Get current execution path
        string currentPath = FilePath.AddPathSuffix(FilePath.GetAbsolutePath(""));

        // Make sure archive path exists to hold historian files
        string archivePath = FilePath.GetAbsolutePath(FilePath.AddPathSuffix("Archive"));

        if (!Directory.Exists(archivePath))
            Directory.CreateDirectory(archivePath);

        // Apply settings optimizations to local historians
        foreach (DataRow row in historians)
        {
            acronym = row.Field<string>("AdapterName").ToLower();
            name = $"local \'{acronym}\' historian";

            // We handle the statistics historian as a special case
            if (acronym == "stat")
            {
                // File size of the statistics historian is tuned based on the number of statistics being archived (two data blocks per statistic per archive file).
                // This number is rounded up to the nearest MB to prevent rounding errors when serializing the settings to the configuration file.
                // The data block size is set to its minimum of 1 KB which works better for archives with a low sample rate and a large number of signals.
                const int DataBlocksPerSignal = 2;
                int statisticsCount = Convert.ToInt32(database.Connection.ExecuteScalar("SELECT COUNT(*) FROM ActiveMeasurement WHERE SignalType = 'STAT'"));
                int fileSize = (int)Math.Max(1.0D, Math.Ceiling(statisticsCount * DataBlocksPerSignal / 1024.0D));

                // Set rollover preparation such that the historian will not be creating a standby file and rolling over simultaneously.
                // Common.Mid is used to ensure that the setting is between 1 and 95, which are the upper and lower bounds of the rollover preparation threshold.
                // The actual number of data blocks per signal is calculated because it may be different since the file size was rounded up to the nearest MB.
                double actualDataBlocksPerSignal = fileSize * 1024.0D / statisticsCount;
                int rolloverPreparationThreshold = Common.Mid(95, 100 - (int)(100 / actualDataBlocksPerSignal) - 1, 1);

                settings = configFile.Settings["statArchiveFile"];
                settings.Add("FileSize", fileSize, "Size (in MB) of the file.");
                settings.Add("DataBlockSize", 1, "Size (in KB) of the data blocks in the file.");
                settings.Add("RolloverPreparationThreshold", rolloverPreparationThreshold, "Percentage file full when the rollover preparation should begin.");
                settings["FileSize"].Update(fileSize);
                settings["DataBlockSize"].Update(1);
                settings["RolloverPreparationThreshold"].Update(rolloverPreparationThreshold);
            }
            else
            {
                // Make sure needed historian configuration settings are properly defined
                settings = configFile.Settings[$"{acronym}MetadataFile"];
                settings.Add("LoadOnOpen", true, $"True if file records are to be loaded in memory when opened; otherwise False - this defaults to True for the {name} meta-data file.");
                settings.Add("ReloadOnModify", false, $"True if file records loaded in memory are to be re-loaded when file is modified on disk; otherwise False - this defaults to True for the {name} meta-data file.");
                settings["LoadOnOpen"].Update(true);
                settings["ReloadOnModify"].Update(false);

                // Older versions placed the archive data files in the same folder as the executables, for both better organization
                // and performance related to file monitoring, these files are now located in their own folder
                string defaultFileName = $"{archivePath}{Path.DirectorySeparatorChar}{acronym}_dbase.dat";
                settings.Add("FileName", defaultFileName, $"Name of the {acronym} meta-data file including its path.");
                string fileName = settings["FileName"].Value;

                if (string.Compare(FilePath.GetDirectoryName(fileName), currentPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!File.Exists(defaultFileName))
                        File.Move(fileName, defaultFileName);

                    settings["FileName"].Update(defaultFileName);
                }

                settings = configFile.Settings[$"{acronym}StateFile"];
                settings.Add("AutoSaveInterval", 10000, $"Interval in milliseconds at which the file records loaded in memory are to be saved automatically to disk. Use -1 to disable automatic saving - this defaults to 10,000 for the {name} state file.");
                settings.Add("LoadOnOpen", true, $"True if file records are to be loaded in memory when opened; otherwise False - this defaults to True for the {name} state file.");
                settings.Add("SaveOnClose", true, $"True if file records loaded in memory are to be saved to disk when file is closed; otherwise False - this defaults to True for the {name} state file.");
                settings.Add("ReloadOnModify", false, $"True if file records loaded in memory are to be re-loaded when file is modified on disk; otherwise False - this defaults to False for the {name} state file.");
                settings["AutoSaveInterval"].Update(10000);
                settings["LoadOnOpen"].Update(true);
                settings["SaveOnClose"].Update(true);
                settings["ReloadOnModify"].Update(false);

                defaultFileName = $"{archivePath}{Path.DirectorySeparatorChar}{acronym}_startup.dat";
                settings.Add("FileName", defaultFileName, $"Name of the {acronym} state file including its path.");
                fileName = settings["FileName"].Value;

                if (string.Compare(FilePath.GetDirectoryName(fileName), currentPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!File.Exists(defaultFileName))
                        File.Move(fileName, defaultFileName);

                    settings["FileName"].Update(defaultFileName);
                }

                settings = configFile.Settings[$"{acronym}IntercomFile"];
                settings.Add("AutoSaveInterval", 1000, $"Interval in milliseconds at which the file records loaded in memory are to be saved automatically to disk. Use -1 to disable automatic saving - this defaults to 1,000 for the {name} intercom file.");
                settings.Add("LoadOnOpen", true, $"True if file records are to be loaded in memory when opened; otherwise False - this defaults to True for the {name} intercom file.");
                settings.Add("SaveOnClose", true, $"True if file records loaded in memory are to be saved to disk when file is closed; otherwise False - this defaults to True for the {name} intercom file.");
                settings.Add("ReloadOnModify", false, $"True if file records loaded in memory are to be re-loaded when file is modified on disk; otherwise False - this defaults to False for the {name} intercom file.");
                settings["AutoSaveInterval"].Update(1000);
                settings["LoadOnOpen"].Update(true);
                settings["SaveOnClose"].Update(true);
                settings["ReloadOnModify"].Update(false);

                defaultFileName = $"{archivePath}{Path.DirectorySeparatorChar}scratch.dat";
                settings.Add("FileName", defaultFileName, $"Name of the {acronym} intercom file including its path.");
                fileName = settings["FileName"].Value;

                if (string.Compare(FilePath.GetDirectoryName(fileName), currentPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!File.Exists(defaultFileName))
                        File.Move(fileName, defaultFileName);

                    settings["FileName"].Update(defaultFileName);
                }

                settings = configFile.Settings[$"{acronym}ArchiveFile"];
                settings.Add("CacheWrites", true, $"True if writes are to be cached for performance; otherwise False - this defaults to True for the {name} working archive file.");
                settings.Add("ConserveMemory", false, $"True if attempts are to be made to conserve memory; otherwise False - this defaults to False for the {name} working archive file.");
                settings["CacheWrites"].Update(true);
                settings["ConserveMemory"].Update(false);

                defaultFileName = $"{archivePath}{Path.DirectorySeparatorChar}{acronym}_archive.d";
                settings.Add("FileName", defaultFileName, $"Name of the {acronym} working archive file including its path.");
                fileName = settings["FileName"].Value;

                if (string.Compare(FilePath.GetDirectoryName(fileName), currentPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!File.Exists(defaultFileName))
                        File.Move(fileName, defaultFileName);

                    settings["FileName"].Update(defaultFileName);
                }

                // Move any historical files in executable folder to the archive folder...
                string[] archiveFileNames = FilePath.GetFileList($"{FilePath.GetAbsolutePath("")}{Path.DirectorySeparatorChar}{acronym}_archive*.d");

                if (archiveFileNames is
                {
                    Length: > 0
                })
                {
                    statusMessage("Relocating existing historical data to \"Archive\" folder...");

                    foreach (string archiveFileName in archiveFileNames)
                    {
                        defaultFileName = $"{archivePath}{Path.DirectorySeparatorChar}{FilePath.GetFileName(archiveFileName)}";

                        if (!File.Exists(defaultFileName))
                            File.Move(archiveFileName, defaultFileName);
                    }
                }

                // Lookup matching reader record (i.e., LocalInputAdapter with instance name of the current historian)
                DataRow match;

                try
                {
                    match = readers.FirstOrDefault(inputRow =>
                    {
                        if (inputRow["ConnectionString"].ToNonNullString().ParseKeyValuePairs().TryGetValue("instanceName", out instanceName))
                            return string.Compare(instanceName, acronym, StringComparison.OrdinalIgnoreCase) == 0;

                        return false;
                    });
                }
                catch
                {
                    match = null;
                }

                if (match is not null)
                    continue;

                // If no match was found, add record for associated historical data reader
                try
                {
                    instanceName = acronym.ToUpper().Trim();
                    settings = configFile.Settings[$"{acronym}ArchiveFile"];
                    string archiveLocation = FilePath.GetDirectoryName(settings["FileName"].Value);
                    string adapterName = $"{instanceName}READER";
                    string connectionString = string.Format("archiveLocation={0}; instanceName={1}; sourceIDs={1}; publicationInterval=333333; connectOnDemand=true", archiveLocation, instanceName);
                    string query = "INSERT INTO CustomInputAdapter(NodeID, AdapterName, AssemblyName, TypeName, ConnectionString, LoadOrder, Enabled) " + $"VALUES({nodeIDQueryString}, @adapterName, 'HistorianAdapters.dll', 'HistorianAdapters.LocalInputAdapter', @connectionString, 0, 1)";

                    if (database.IsOracle)
                        query = query.Replace('@', ':');

                    database.Connection.ExecuteNonQuery(query, adapterName, connectionString);
                }
                catch (Exception ex)
                {
                    processException(new InvalidOperationException("Failed to add associated historian reader input adapter for local historian: " + ex.Message, ex));
                }
            }
        }

        // Generate lookup table for output adapter names in the system
        List<string> outputAdapterNames = database.RetrieveData("SELECT Acronym FROM Historian").Select()
            .Concat(database.RetrieveData("SELECT AdapterName FROM CustomOutputAdapter").Select())
            .Select(row => row.Field<string>(0))
            .OrderBy(adapterName => adapterName)
            .ToList();

        HashSet<string> outputAdapterNameLookup = [..outputAdapterNames];

        // Create a list to track categories to remove
        HashSet<string> categoriesToRemove = [];

        // Search for unused settings categories
        foreach (PropertyInformation info in configFile.Settings.ElementInformation.Properties)
        {
            name = info.Name;

            if (name.EndsWith("MetadataFile") && outputAdapterNameLookup.Contains(name.Substring(0, name.IndexOf("MetadataFile", StringComparison.OrdinalIgnoreCase))))
                categoriesToRemove.Add(name);

            if (name.EndsWith("StateFile") && outputAdapterNameLookup.Contains(name.Substring(0, name.IndexOf("StateFile", StringComparison.OrdinalIgnoreCase))))
                categoriesToRemove.Add(name);

            if (name.EndsWith("IntercomFile") && outputAdapterNameLookup.Contains(name.Substring(0, name.IndexOf("IntercomFile", StringComparison.OrdinalIgnoreCase))))
                categoriesToRemove.Add(name);

            if (name.EndsWith("ArchiveFile") && outputAdapterNameLookup.Contains(name.Substring(0, name.IndexOf("ArchiveFile", StringComparison.OrdinalIgnoreCase))))
                categoriesToRemove.Add(name);

            if (name.EndsWith("AdoMetadataProvider") && outputAdapterNameLookup.Contains(name.Substring(0, name.IndexOf("AdoMetadataProvider", StringComparison.OrdinalIgnoreCase))))
                categoriesToRemove.Add(name);

            if (name.EndsWith("OleDbMetadataProvider") && outputAdapterNameLookup.Contains(name.Substring(0, name.IndexOf("OleDbMetadataProvider", StringComparison.OrdinalIgnoreCase))))
                categoriesToRemove.Add(name);

            if (name.EndsWith("RestWebServiceMetadataProvider") && outputAdapterNameLookup.Contains(name.Substring(0, name.IndexOf("RestWebServiceMetadataProvider", StringComparison.OrdinalIgnoreCase))))
                categoriesToRemove.Add(name);

            if (name.EndsWith("MetadataService") && outputAdapterNameLookup.Contains(name.Substring(0, name.IndexOf("MetadataService", StringComparison.OrdinalIgnoreCase))))
                categoriesToRemove.Add(name);

            if (name.EndsWith("TimeSeriesDataService") && outputAdapterNameLookup.Contains(name.Substring(0, name.IndexOf("TimeSeriesDataService", StringComparison.OrdinalIgnoreCase))))
                categoriesToRemove.Add(name);

            if (name.EndsWith("HadoopReplicationProvider") && outputAdapterNameLookup.Contains(name.Substring(0, name.IndexOf("HadoopReplicationProvider", StringComparison.OrdinalIgnoreCase))))
                categoriesToRemove.Add(name);
        }

        if (categoriesToRemove.Count > 0)
        {
            statusMessage("Removing unused local historian configuration settings...");

            // Remove any unused settings categories
            foreach (string category in categoriesToRemove)
            {
                configFile.Settings.Remove(category);
            }
        }

        // Save any applied changes
        configFile.Save();

        // Associate temporal data readers with any added OSI-PI historian archive adapters
        AddPIHistorianReaders(database, nodeIDQueryString, processException);
    }

    private static void AddPIHistorianReaders(AdoDataConnection database, string nodeIDQueryString, Action<Exception> processException)
    {
        // Load the defined local PI historians
        IEnumerable<DataRow> historians = database.Connection.RetrieveData(database.AdapterType, $"SELECT AdapterName, ConnectionString FROM RuntimeHistorian WHERE NodeID = {nodeIDQueryString} AND TypeName = 'PIAdapters.PIOutputAdapter'").AsEnumerable();
        IEnumerable<DataRow> readers = database.Connection.RetrieveData(database.AdapterType, $"SELECT * FROM CustomInputAdapter WHERE NodeID = {nodeIDQueryString} AND TypeName = 'PIAdapters.PIPBInputAdapter'").AsEnumerable();

        // Also check for PI adapters loaded into CustomOutputAdapters
        historians = historians.Concat(database.Connection.RetrieveData(database.AdapterType, $"SELECT AdapterName, ConnectionString FROM RuntimeCustomOutputAdapter WHERE NodeID = {nodeIDQueryString} AND TypeName = 'PIAdapters.PIOutputAdapter'").AsEnumerable());

        // Make sure a temporal reader is defined for each OSI-PI historian
        foreach (DataRow row in historians)
        {
            string acronym = row.Field<string>("AdapterName").ToLower();

            // Lookup matching reader record (i.e., PIPBInputAdapter with instance name of the current PI historian)
            DataRow match;

            try
            {
                match = readers.FirstOrDefault(inputRow =>
                {
                    if (inputRow["ConnectionString"].ToNonNullString().ParseKeyValuePairs().TryGetValue("sourceIDs", out string sourceIDs))
                        return string.Compare(sourceIDs, acronym, StringComparison.OrdinalIgnoreCase) == 0;

                    return false;
                });
            }
            catch
            {
                match = null;
            }

            // If no match was found, add record for associated historical data reader
            if (match is not null)
                continue;

            try
            {
                Dictionary<string, string> settings = row.Field<string>("ConnectionString").ToNonNullString().ParseKeyValuePairs();
                string instanceName = acronym.ToUpper().Trim();
                string adapterName = $"{instanceName}READER";

                if (!settings.TryGetValue("ServerName", out string serverName))
                    throw new InvalidOperationException("Server name is a required setting for PI connections. Please add a server in the format servername=myservername to the connection string.");

                string userName = settings.TryGetValue("UserName", out string setting) ? setting : null;
                string password = settings.TryGetValue("Password", out setting) ? setting : null;

                if (!settings.TryGetValue("ConnectTimeout", out setting) || !int.TryParse(setting, out int connectTimeout))
                    connectTimeout = 30000;

                string connectionString = string.IsNullOrEmpty(userName) ? $"ServerName={serverName}; ConnectTimeout={connectTimeout}; sourceIDs={instanceName}; connectOnDemand=true" : $"ServerName={serverName}; UserName={userName}; Password={password.ToNonNullString()}; ConnectTimeout={connectTimeout}; sourceIDs={instanceName}; connectOnDemand=true";

                string query = "INSERT INTO CustomInputAdapter(NodeID, AdapterName, AssemblyName, TypeName, ConnectionString, LoadOrder, Enabled) " + $"VALUES({nodeIDQueryString}, @adapterName, 'PIAdapters.dll', 'PIAdapters.PIPBInputAdapter', @connectionString, 0, 1)";

                if (database.IsOracle)
                    query = query.Replace('@', ':');

                database.Connection.ExecuteNonQuery(query, adapterName, connectionString);
            }
            catch (Exception ex)
            {
                processException(new InvalidOperationException("Failed to add associated OSI-PI historian temporal data reader input adapter for local OSI-PI historian: " + ex.Message, ex));
            }
        }
    }

    //// Create an http namespace reservation
    //private static void AddNamespaceReservation(Uri serviceUri)
    //{
    //    OperatingSystem OS = Environment.OSVersion;
    //    ProcessStartInfo psi = null;
    //    string parameters = null;

    //    if (OS.Platform == PlatformID.Win32NT)
    //    {
    //        if (OS.Version.Major > 5)
    //        {
    //            // Vista, Windows 2008, Window 7, etc use "netsh" for reservations
    //            string everyoneUser = new SecurityIdentifier("S-1-1-0").Translate(typeof(NTAccount)).ToString();
    //            parameters = string.Format(@"http add urlacl url={0}://+:{1}{2} user=\{3}", serviceUri.Scheme, serviceUri.Port, serviceUri.AbsolutePath, everyoneUser);
    //            psi = new ProcessStartInfo("netsh", parameters);
    //        }
    //        else
    //        {
    //            // Attempt to use "httpcfg" for older Windows versions...
    //            parameters = string.Format(@"set urlacl /u {0}://*:{1}{2}/ /a D:(A;;GX;;;S-1-1-0)", serviceUri.Scheme, serviceUri.Port, serviceUri.AbsolutePath);
    //            psi = new ProcessStartInfo("httpcfg", parameters);
    //        }
    //    }

    //    if (psi is not null)
    //    {
    //        psi.Verb = "runas";
    //        psi.CreateNoWindow = true;
    //        psi.WindowStyle = ProcessWindowStyle.Hidden;
    //        psi.UseShellExecute = false;
    //        psi.Arguments = parameters;

    //        using (Process shell = new Process())
    //        {
    //            shell.StartInfo = psi;
    //            shell.Start();
    //            if (!shell.WaitForExit(5000))
    //                shell.Kill();
    //        }
    //    }
    //}

    //private static void RemoveNamespaceReservation(Uri serviceUri)
    //{
    //    OperatingSystem OS = Environment.OSVersion;
    //    ProcessStartInfo psi = null;
    //    string parameters = null;

    //    if (OS.Platform == PlatformID.Win32NT)
    //    {
    //        if (OS.Version.Major > 5)
    //        {
    //            // Vista, Windows 2008, Window 7, etc use "netsh" for reservations
    //            parameters = string.Format(@"http delete urlacl url={0}://+:{1}{2}", serviceUri.Scheme, serviceUri.Port, serviceUri.AbsolutePath);
    //            psi = new ProcessStartInfo("netsh", parameters);
    //        }
    //        else
    //        {
    //            // Attempt to use "httpcfg" for older Windows versions...
    //            parameters = string.Format(@"delete urlacl /u {0}://*:{1}{2}/", serviceUri.Scheme, serviceUri.Port, serviceUri.AbsolutePath);
    //            psi = new ProcessStartInfo("httpcfg", parameters);
    //        }
    //    }

    //    if (psi is not null)
    //    {
    //        psi.Verb = "runas";
    //        psi.CreateNoWindow = true;
    //        psi.WindowStyle = ProcessWindowStyle.Hidden;
    //        psi.UseShellExecute = false;
    //        psi.Arguments = parameters;

    //        using (Process shell = new Process())
    //        {
    //            shell.StartInfo = psi;
    //            shell.Start();
    //            if (!shell.WaitForExit(5000))
    //                shell.Kill();
    //        }
    //    }
    //}

    #endregion
}