//******************************************************************************************************
//  LocalOutputAdapter.cs - Gbtc
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
//       Added override to Status property and added event handler to archive rollver notification.
//  10/28/2009 - Pinal C. Patel
//       Modified to allow for multiple instances of the adapter to be loaded and configured with 
//       different settings by persisting the settings in the config file under unique categories.
//  11/18/2009 - Pinal C. Patel
//       Added support for the replication of local historian archive.
//  12/01/2009 - Pinal C. Patel
//       Modified Initialize() to load all available metadata providers.
//  12/11/2009 - Pinal C. Patel
//       Fixed the implementation for allowing multiple adapter instances.
//       Expanded the adapter status to include dynamically loaded plugins.
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
//       Modified historian optimization procedure to dyanmically add reading adapters.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF;
using GSF.Configuration;
using GSF.Data;
using GSF.Historian.DataServices;
using GSF.Historian.Files;
using GSF.Historian.MetadataProviders;
using GSF.Historian.Replication;
using GSF.IO;

namespace HistorianAdapters
{
    /// <summary>
    /// Represents an output adapter that archives measurements to a local archive.
    /// </summary>
    [Description("Local Historian: archives measurements to a local in-process openHistorian.")]
    public class LocalOutputAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Fields
        private ArchiveFile m_archive;
        private DataServices m_dataServices;
        private MetadataProviders m_metadataProviders;
        private ReplicationProviders m_replicationProviders;
        private object m_queuedMetadataRefreshPending;
        private AutoResetEvent m_metadataRefreshComplete;
        private bool m_autoRefreshMetadata;
        private string m_instanceName;
        private string m_archivePath;
        private bool m_useNamespaceReservation;
        private long m_archivedMeasurements;
        private volatile int m_adapterLoadedCount;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalOutputAdapter"/> class.
        /// </summary>
        public LocalOutputAdapter()
            : base()
        {
            m_autoRefreshMetadata = true;
            m_queuedMetadataRefreshPending = new object();
            m_metadataRefreshComplete = new AutoResetEvent(true);
            m_archive = new ArchiveFile();
            m_archive.MetadataFile = new MetadataFile();
            m_archive.StateFile = new StateFile();
            m_archive.IntercomFile = new IntercomFile();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets instance name defined for this <see cref="LocalOutputAdapter"/>.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the instance name for the archive. Leave this value blank to default to the adapter name."),
        DefaultValue("")]
        public string InstanceName
        {
            get
            {
                if (string.IsNullOrEmpty(m_instanceName))
                    return Name.ToLower();

                return m_instanceName;
            }
            set
            {
                m_instanceName = value;
            }
        }

        /// <summary>
        /// Gets or sets the path to the archive.
        /// </summary>
        public string ArchivePath
        {
            get
            {
                return m_archivePath;
            }
            set
            {
                m_archivePath = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether or not metadata is
        /// refreshed when the adapter attempts to connect to the archive.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define a boolean indicating whether to refresh metadata on connect."),
        DefaultValue(true)]
        public bool AutoRefreshMetadata
        {
            get
            {
                return m_autoRefreshMetadata;
            }
            set
            {
                m_autoRefreshMetadata = value;
            }
        }

        /// <summary>
        /// Returns a flag that determines if measurements sent to this <see cref="LocalOutputAdapter"/> are destined for archival.
        /// </summary>
        public override bool OutputIsForArchive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets flag that determines if this <see cref="LocalOutputAdapter"/> uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return true;
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
                status.Append(m_archive.Status);
                status.AppendLine();
                status.Append(m_archive.MetadataFile.Status);
                status.AppendLine();
                status.Append(m_archive.StateFile.Status);
                status.AppendLine();
                status.Append(m_archive.IntercomFile.Status);
                status.AppendLine();
                status.Append(m_dataServices.Status);
                status.AppendLine();
                status.Append(m_metadataProviders.Status);
                status.AppendLine();
                status.Append(m_replicationProviders.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Refreshes metadata using all available and enabled providers.
        /// </summary>
        [AdapterCommand("Refreshes metadata using all available and enabled providers.")]
        public override void RefreshMetadata()
        {
            ThreadPool.QueueUserWorkItem(QueueMetadataRefresh);
        }

        private void QueueMetadataRefresh(object state)
        {
            try
            {
                // Queue up a metadata refresh unless another thread has already requested one
                if (Monitor.TryEnter(m_queuedMetadataRefreshPending))
                {
                    try
                    {
                        // Queue new metadata refresh after waiting for any prior refresh to complete
                        if (m_metadataRefreshComplete.WaitOne())
                            ThreadPool.QueueUserWorkItem(ExecuteMetadataRefresh);
                    }
                    finally
                    {
                        Monitor.Exit(m_queuedMetadataRefreshPending);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        private void ExecuteMetadataRefresh(object state)
        {
            try
            {
                bool queueEnabled = false;

                try
                {
                    base.RefreshMetadata();

                    if (m_archive != null && m_archive.IsOpen && m_archive.StateFile != null && m_archive.StateFile.IsOpen && m_archive.MetadataFile != null && m_archive.MetadataFile.IsOpen)
                    {
                        queueEnabled = InternalProcessQueue.Enabled;
                        InternalProcessQueue.Stop();

                        // Synchronously refresh the metabase.
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

                        // Wait for the metabase to synchronize, up to five seconds
                        int waitCounts = 0;

                        while (m_archive.StateFile.RecordsOnDisk != m_archive.MetadataFile.RecordsOnDisk || waitCounts < 50)
                        {
                            Thread.Sleep(100);
                            waitCounts++;
                        }
                    }
                }
                finally
                {
                    m_metadataRefreshComplete.Set();

                    if (queueEnabled)
                        InternalProcessQueue.Start();
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        /// <summary>
        /// Initializes this <see cref="LocalOutputAdapter"/>.
        /// </summary>
        /// <exception cref="ArgumentException"><b>InstanceName</b> is missing from the <see cref="AdapterBase.Settings"/>.</exception>
        public override void Initialize()
        {
            base.Initialize();

            string refreshMetadata;
            string errorMessage = "{0} is missing from Settings - Example: instanceName=XX;archivePath=c:\\;autoRefreshMetadata=True";
            Dictionary<string, string> settings = Settings;
            string setting;

            // Validate settings.
            if (!settings.TryGetValue("instancename", out m_instanceName))
                throw new ArgumentException(string.Format(errorMessage, "instanceName"));

            if (!settings.TryGetValue("archivepath", out m_archivePath))
                m_archivePath = FilePath.GetAbsolutePath(FilePath.AddPathSuffix("Archive"));

            if (settings.TryGetValue("refreshmetadata", out refreshMetadata) || settings.TryGetValue("autorefreshmetadata", out refreshMetadata))
                m_autoRefreshMetadata = refreshMetadata.ParseBoolean();

            if (settings.TryGetValue("useNamespaceReservation", out setting))
                m_useNamespaceReservation = setting.ParseBoolean();
            else
                m_useNamespaceReservation = false;

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
            m_archive.RolloverStart += Archive_RolloverStart;
            m_archive.RolloverComplete += Archive_RolloverComplete;
            m_archive.RolloverException += Archive_RolloverException;
            m_archive.Initialize();

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
            return string.Format("Archived {0} measurements locally.", m_archivedMeasurements).CenterText(maxLength);
        }

        /// <summary>
        /// Releases the unmanaged resources used by this <see cref="LocalOutputAdapter"/> and optionally releases the managed resources.
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
                        // This will be done only when the object is disposed by calling Dispose().
                        if (m_dataServices != null)
                        {
                            m_dataServices.AdapterCreated -= DataServices_AdapterCreated;
                            m_dataServices.AdapterLoaded -= DataServices_AdapterLoaded;
                            m_dataServices.AdapterUnloaded -= DataServices_AdapterUnloaded;
                            m_dataServices.AdapterLoadException -= AdapterLoader_AdapterLoadException;
                            m_dataServices.Dispose();
                        }

                        if (m_metadataProviders != null)
                        {
                            m_metadataProviders.AdapterCreated -= MetadataProviders_AdapterCreated;
                            m_metadataProviders.AdapterLoaded -= MetadataProviders_AdapterLoaded;
                            m_metadataProviders.AdapterUnloaded -= MetadataProviders_AdapterUnloaded;
                            m_metadataProviders.AdapterLoadException -= AdapterLoader_AdapterLoadException;
                            m_metadataProviders.Dispose();
                        }

                        if (m_replicationProviders != null)
                        {
                            m_replicationProviders.AdapterCreated -= ReplicationProviders_AdapterCreated;
                            m_replicationProviders.AdapterLoaded -= ReplicationProviders_AdapterLoaded;
                            m_replicationProviders.AdapterUnloaded -= ReplicationProviders_AdapterUnloaded;
                            m_replicationProviders.AdapterLoadException -= AdapterLoader_AdapterLoadException;
                            m_replicationProviders.Dispose();
                        }

                        if (m_archive != null)
                        {
                            m_archive.RolloverStart -= Archive_RolloverStart;
                            m_archive.RolloverComplete -= Archive_RolloverComplete;
                            m_archive.RolloverException -= Archive_RolloverException;
                            m_archive.Dispose();

                            if (m_archive.MetadataFile != null)
                            {
                                m_archive.MetadataFile.Dispose();
                                m_archive.MetadataFile = null;
                            }

                            if (m_archive.StateFile != null)
                            {
                                m_archive.StateFile.Dispose();
                                m_archive.StateFile = null;
                            }

                            if (m_archive.IntercomFile != null)
                            {
                                m_archive.IntercomFile.Dispose();
                                m_archive.IntercomFile = null;
                            }

                            if (m_metadataRefreshComplete != null)
                                m_metadataRefreshComplete.Dispose();

                            m_metadataRefreshComplete = null;
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
        /// Attempts to connect to this <see cref="LocalOutputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            // Open archive files
            m_archive.MetadataFile.Open();
            m_archive.StateFile.Open();
            m_archive.IntercomFile.Open();
            m_archive.Open();

            m_adapterLoadedCount = 0;

            // Initialization of services needs to occur after files are open
            m_dataServices.Initialize();
            m_metadataProviders.Initialize();
            m_replicationProviders.Initialize();

            int waitCount = 0;

            // Wait for adapter initialization to complete, up to 2 seconds
            while (waitCount < 20 && m_adapterLoadedCount != m_dataServices.Adapters.Count + m_metadataProviders.Adapters.Count + m_replicationProviders.Adapters.Count)
            {
                Thread.Sleep(100);
                waitCount++;
            }

            // Kick off a meta-data refresh...
            if (m_autoRefreshMetadata)
            {
                RefreshMetadata();
                m_autoRefreshMetadata = false;
            }

            OnConnected();
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="LocalOutputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if (m_archive != null)
            {
                if (m_archive.IsOpen)
                {
                    m_archive.Save();
                    m_archive.Close();
                }

                if (m_archive.MetadataFile != null && m_archive.MetadataFile.IsOpen)
                {
                    m_archive.MetadataFile.Save();
                    m_archive.MetadataFile.Close();
                }

                if (m_archive.StateFile != null && m_archive.StateFile.IsOpen)
                {
                    m_archive.StateFile.Save();
                    m_archive.StateFile.Close();
                }

                if (m_archive.IntercomFile != null && m_archive.IntercomFile.IsOpen)
                {
                    m_archive.IntercomFile.Save();
                    m_archive.IntercomFile.Close();
                }

                OnDisconnected();
                m_archivedMeasurements = 0;
            }
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
                m_archive.WriteData(new ArchiveDataPoint(measurement));
            }
            m_archivedMeasurements += measurements.Length;
        }

        private void Archive_RolloverStart(object sender, EventArgs e)
        {
            OnStatusMessage("Archive is being rolled over...");
        }

        private void Archive_RolloverComplete(object sender, EventArgs e)
        {
            OnStatusMessage("Archive rollover is complete.");
        }

        private void Archive_RolloverException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
            OnStatusMessage("Archive rollover failed - {0}", e.Argument.Message);
        }

        private void DataServices_AdapterCreated(object sender, EventArgs<IDataService> e)
        {
            e.Argument.SettingsCategory = InstanceName + e.Argument.SettingsCategory;

            // Attempt to reserve the http namespace reservation for this data service URI
            string uri = string.Empty;
            IDataService provider = e.Argument;

            if (provider != null && !string.IsNullOrWhiteSpace(provider.Endpoints))
            {
                foreach (string endpoint in provider.Endpoints.Split(';'))
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(endpoint))
                        {
                            // Convert the endpoint address to standard URI format.
                            uri = Regex.Replace(endpoint.Trim(), "http\\..*://", "http://", RegexOptions.IgnoreCase);

                            if (m_useNamespaceReservation)
                                AddNamespaceReservation(new Uri(uri));
                            else
                                RemoveNamespaceReservation(new Uri(uri));
                        }
                    }
                    catch (Exception ex)
                    {
                        OnStatusMessage("Unable to set namespace reservation for \"{0}\", this may not be required on this OS version. Message was: {1}", uri, ex.Message);
                    }
                }
            }
        }

        private void DataServices_AdapterLoaded(object sender, EventArgs<IDataService> e)
        {
            e.Argument.Archive = m_archive;
            e.Argument.ServiceProcessException += DataServices_ServiceProcessException;
            OnStatusMessage("{0} has been loaded.", e.Argument.GetType().Name);

            m_adapterLoadedCount++;
        }

        private void DataServices_AdapterUnloaded(object sender, EventArgs<IDataService> e)
        {
            e.Argument.Archive = null;
            e.Argument.ServiceProcessException -= DataServices_ServiceProcessException;
            OnStatusMessage("{0} has been unloaded.", e.Argument.GetType().Name);
        }

        private void MetadataProviders_AdapterCreated(object sender, EventArgs<IMetadataProvider> e)
        {
            e.Argument.SettingsCategory = InstanceName + e.Argument.SettingsCategory;

            if (e.Argument.GetType() == typeof(AdoMetadataProvider))
            {
                // Populate the default configuration for AdoMetadataProvider.
                AdoMetadataProvider provider = e.Argument as AdoMetadataProvider;

                provider.Enabled = true;
                provider.SelectString = string.Format("SELECT * FROM HistorianMetadata WHERE PlantCode='{0}'", Name);

                // The following connection information is now provided via configuration Eval mappings
                //    provider.DataProviderString = config.Settings["SystemSettings"]["DataProviderString"].Value;

                //    ConfigurationFile config = ConfigurationFile.Current;
                //    string connectionString = config.Settings["SystemSettings"]["ConnectionString"].Value;
                //    Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();
                //    string setting;

                //    if (settings.TryGetValue("Provider", out setting))
                //    {
                //        // Check if provider is for Access
                //        if (setting.StartsWith("Microsoft.Jet.OLEDB", StringComparison.OrdinalIgnoreCase))
                //        {
                //            // Make sure path to Access database is fully qualified
                //            if (settings.TryGetValue("Data Source", out setting))
                //            {
                //                settings["Data Source"] = FilePath.GetAbsolutePath(setting);
                //                connectionString = settings.JoinKeyValuePairs();
                //            }
                //        }
                //    }

                //    provider.ConnectionString = connectionString;
            }
        }

        private void MetadataProviders_AdapterLoaded(object sender, EventArgs<IMetadataProvider> e)
        {
            e.Argument.Metadata = m_archive.MetadataFile;
            e.Argument.MetadataRefreshStart += MetadataProviders_MetadataRefreshStart;
            e.Argument.MetadataRefreshComplete += MetadataProviders_MetadataRefreshComplete;
            e.Argument.MetadataRefreshTimeout += MetadataProviders_MetadataRefreshTimeout;
            e.Argument.MetadataRefreshException += MetadataProviders_MetadataRefreshException;
            OnStatusMessage("{0} has been loaded.", e.Argument.GetType().Name);

            m_adapterLoadedCount++;
        }

        private void MetadataProviders_AdapterUnloaded(object sender, EventArgs<IMetadataProvider> e)
        {
            e.Argument.Metadata = null;
            e.Argument.MetadataRefreshStart -= MetadataProviders_MetadataRefreshStart;
            e.Argument.MetadataRefreshComplete -= MetadataProviders_MetadataRefreshComplete;
            e.Argument.MetadataRefreshTimeout -= MetadataProviders_MetadataRefreshTimeout;
            e.Argument.MetadataRefreshException -= MetadataProviders_MetadataRefreshException;
            OnStatusMessage("{0} has been unloaded.", e.Argument.GetType().Name);
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
            OnStatusMessage("{0} has been loaded.", e.Argument.GetType().Name);

            m_adapterLoadedCount++;
        }

        private void ReplicationProviders_AdapterUnloaded(object sender, EventArgs<IReplicationProvider> e)
        {
            e.Argument.ReplicationStart -= ReplicationProvider_ReplicationStart;
            e.Argument.ReplicationComplete -= ReplicationProvider_ReplicationComplete;
            e.Argument.ReplicationProgress -= ReplicationProvider_ReplicationProgress;
            e.Argument.ReplicationException -= ReplicationProvider_ReplicationException;
            OnStatusMessage("{0} has been unloaded.", e.Argument.GetType().Name);
        }

        private void AdapterLoader_AdapterLoadException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        private void DataServices_ServiceProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        private void MetadataProviders_MetadataRefreshStart(object sender, EventArgs e)
        {
            OnStatusMessage("{0} has started metadata refresh...", sender.GetType().Name);
        }

        private void MetadataProviders_MetadataRefreshComplete(object sender, EventArgs e)
        {
            OnStatusMessage("{0} has finished metadata refresh.", sender.GetType().Name);
        }

        private void MetadataProviders_MetadataRefreshTimeout(object sender, EventArgs e)
        {
            OnStatusMessage("{0} has timed-out on metadata refresh.", sender.GetType().Name);
        }

        private void MetadataProviders_MetadataRefreshException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        private void ReplicationProvider_ReplicationStart(object sender, EventArgs e)
        {
            OnStatusMessage("{0} has started archive replication...", sender.GetType().Name);
        }

        private void ReplicationProvider_ReplicationComplete(object sender, EventArgs e)
        {
            OnStatusMessage("{0} has finished archive replication.", sender.GetType().Name);
        }

        private void ReplicationProvider_ReplicationProgress(object sender, EventArgs<ProcessProgress<int>> e)
        {
            OnStatusMessage("{0} has replicated archive file {1}.", sender.GetType().Name, e.Argument.ProgressMessage);
        }

        private void ReplicationProvider_ReplicationException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Apply historian configuration optimizations at start-up
        private static void OptimizeLocalHistorianSettings(IDbConnection connection, Type adapterType, string nodeIDQueryString, string arguments, Action<object, EventArgs<string>> statusMessage, Action<object, EventArgs<Exception>> processException)
        {
            // Make sure setting exists to allow user to by-pass local historian optimizations at startup
            ConfigurationFile configFile = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = configFile.Settings["systemSettings"];
            settings.Add("OptimizeLocalHistorianSettings", true, "Determines if the defined local historians will have their settings optimized at startup");

            // See if this node should optimize local historian settings
            if (settings["OptimizeLocalHistorianSettings"].ValueAsBoolean())
            {
                statusMessage("LocalOutputAdapter", new EventArgs<string>("Optimizing settings for local historians..."));

                // Load the defined local system historians
                IEnumerable<DataRow> historians = connection.RetrieveData(adapterType, string.Format("SELECT AdapterName FROM RuntimeHistorian WHERE NodeID = {0} AND TypeName = 'HistorianAdapters.LocalOutputAdapter'", nodeIDQueryString)).AsEnumerable();
                IEnumerable<DataRow> readers = connection.RetrieveData(adapterType, string.Format("SELECT * FROM CustomInputAdapter WHERE NodeID = {0} AND TypeName = 'HistorianAdapters.LocalInputAdapter'", nodeIDQueryString)).AsEnumerable();

                List<string> validHistorians = new List<string>();
                string name, acronym, currentPath, archivePath, fileName, defaultFileName, instanceName;

                // Get current execution path
                currentPath = FilePath.AddPathSuffix(FilePath.GetAbsolutePath(""));

                // Make sure archive path exists to hold historian files
                archivePath = FilePath.GetAbsolutePath(FilePath.AddPathSuffix("Archive"));

                if (!Directory.Exists(archivePath))
                    Directory.CreateDirectory(archivePath);

                // Apply settings optimizations to local historians
                foreach (DataRow row in historians)
                {
                    acronym = row.Field<string>("AdapterName").ToLower();
                    name = string.Format("local \'{0}\' historian", acronym);
                    validHistorians.Add(acronym);

                    // We handle the statistics historian as a special case
                    if (acronym != "stat")
                    {
                        // Make sure needed historian configuration settings are properly defined
                        settings = configFile.Settings[string.Format("{0}MetadataFile", acronym)];
                        settings.Add("LoadOnOpen", true, string.Format("True if file records are to be loaded in memory when opened; otherwise False - this defaults to True for the {0} meta-data file.", name));
                        settings.Add("ReloadOnModify", false, string.Format("True if file records loaded in memory are to be re-loaded when file is modified on disk; otherwise False - this defaults to True for the {0} meta-data file.", name));
                        settings["LoadOnOpen"].Update(true);
                        settings["ReloadOnModify"].Update(false);

                        // Older versions of the openPDC placed the archive data files in the same folder as the executables, for both better
                        // organization and performance related to file monitoring, these files are now located in their own folder
                        defaultFileName = string.Format("{0}\\{1}_dbase.dat", archivePath, acronym);
                        settings.Add("FileName", defaultFileName, string.Format("Name of the {0} meta-data file including its path.", acronym));
                        fileName = settings["FileName"].Value;

                        if (string.Compare(FilePath.GetDirectoryName(fileName), currentPath, true) == 0)
                        {
                            if (!File.Exists(defaultFileName))
                                File.Move(fileName, defaultFileName);
                            settings["FileName"].Update(defaultFileName);
                        }

                        settings = configFile.Settings[string.Format("{0}StateFile", acronym)];
                        settings.Add("AutoSaveInterval", 10000, string.Format("Interval in milliseconds at which the file records loaded in memory are to be saved automatically to disk. Use -1 to disable automatic saving - this defaults to 10,000 for the {0} state file.", name));
                        settings.Add("LoadOnOpen", true, string.Format("True if file records are to be loaded in memory when opened; otherwise False - this defaults to True for the {0} state file.", name));
                        settings.Add("SaveOnClose", true, string.Format("True if file records loaded in memory are to be saved to disk when file is closed; otherwise False - this defaults to True for the {0} state file.", name));
                        settings.Add("ReloadOnModify", false, string.Format("True if file records loaded in memory are to be re-loaded when file is modified on disk; otherwise False - this defaults to False for the {0} state file.", name));
                        settings["AutoSaveInterval"].Update(10000);
                        settings["LoadOnOpen"].Update(true);
                        settings["SaveOnClose"].Update(true);
                        settings["ReloadOnModify"].Update(false);

                        defaultFileName = string.Format("{0}\\{1}_startup.dat", archivePath, acronym);
                        settings.Add("FileName", defaultFileName, string.Format("Name of the {0} state file including its path.", acronym));
                        fileName = settings["FileName"].Value;

                        if (string.Compare(FilePath.GetDirectoryName(fileName), currentPath, true) == 0)
                        {
                            if (!File.Exists(defaultFileName))
                                File.Move(fileName, defaultFileName);
                            settings["FileName"].Update(defaultFileName);
                        }

                        settings = configFile.Settings[string.Format("{0}IntercomFile", acronym)];
                        settings.Add("AutoSaveInterval", 1000, string.Format("Interval in milliseconds at which the file records loaded in memory are to be saved automatically to disk. Use -1 to disable automatic saving - this defaults to 1,000 for the {0} intercom file.", name));
                        settings.Add("LoadOnOpen", true, string.Format("True if file records are to be loaded in memory when opened; otherwise False - this defaults to True for the {0} intercom file.", name));
                        settings.Add("SaveOnClose", true, string.Format("True if file records loaded in memory are to be saved to disk when file is closed; otherwise False - this defaults to True for the {0} intercom file.", name));
                        settings.Add("ReloadOnModify", false, string.Format("True if file records loaded in memory are to be re-loaded when file is modified on disk; otherwise False - this defaults to False for the {0} intercom file.", name));
                        settings["AutoSaveInterval"].Update(1000);
                        settings["LoadOnOpen"].Update(true);
                        settings["SaveOnClose"].Update(true);
                        settings["ReloadOnModify"].Update(false);

                        defaultFileName = string.Format("{0}\\scratch.dat", archivePath);
                        settings.Add("FileName", defaultFileName, string.Format("Name of the {0} intercom file including its path.", acronym));
                        fileName = settings["FileName"].Value;

                        if (string.Compare(FilePath.GetDirectoryName(fileName), currentPath, true) == 0)
                        {
                            if (!File.Exists(defaultFileName))
                                File.Move(fileName, defaultFileName);
                            settings["FileName"].Update(defaultFileName);
                        }

                        settings = configFile.Settings[string.Format("{0}ArchiveFile", acronym)];
                        settings.Add("CacheWrites", true, string.Format("True if writes are to be cached for performance; otherwise False - this defaults to True for the {0} working archive file.", name));
                        settings.Add("ConserveMemory", false, string.Format("True if attempts are to be made to conserve memory; otherwise False - this defaults to False for the {0} working archive file.", name));
                        settings["CacheWrites"].Update(true);
                        settings["ConserveMemory"].Update(false);

                        defaultFileName = string.Format("{0}\\{1}_archive.d", archivePath, acronym);
                        settings.Add("FileName", defaultFileName, string.Format("Name of the {0} working archive file including its path.", acronym));
                        fileName = settings["FileName"].Value;

                        if (string.Compare(FilePath.GetDirectoryName(fileName), currentPath, true) == 0)
                        {
                            if (!File.Exists(defaultFileName))
                                File.Move(fileName, defaultFileName);
                            settings["FileName"].Update(defaultFileName);
                        }

                        // Move any historical files in executable folder to the archive folder...
                        string[] archiveFileNames = FilePath.GetFileList(string.Format("{0}\\{1}_archive*.d", FilePath.GetAbsolutePath(""), acronym));

                        if (archiveFileNames != null && archiveFileNames.Length > 0)
                        {
                            statusMessage("LocalOutputAdapter", new EventArgs<string>("Relocating existing historical data to \"Archive\" folder..."));

                            foreach (string archiveFileName in archiveFileNames)
                            {
                                defaultFileName = string.Format("{0}\\{1}", archivePath, FilePath.GetFileName(archiveFileName));

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
                                    return string.Compare(instanceName, acronym, true) == 0;

                                return false;
                            });
                        }
                        catch
                        {
                            match = null;
                        }

                        // If no match was found, add record for associated historical data reader
                        if ((object)match == null)
                        {
                            try
                            {
                                instanceName = acronym.ToUpper().Trim();
                                settings = configFile.Settings[string.Format("{0}ArchiveFile", acronym)];
                                string archiveLocation = FilePath.GetDirectoryName(settings["FileName"].Value);
                                string adapterName = string.Format("{0}READER", instanceName);
                                string connectionString = string.Format("archiveLocation={0}; instanceName={1}; sourceIDs={1}; publicationInterval=333333; connectOnDemand=true", archiveLocation, instanceName);
                                string query = string.Format("INSERT INTO CustomInputAdapter(NodeID, AdapterName, AssemblyName, TypeName, ConnectionString, LoadOrder, Enabled) " +
                                    "VALUES({0}, @adapterName, 'HistorianAdapters.dll', 'HistorianAdapters.LocalInputAdapter', @connectionString, 0, 1)", nodeIDQueryString);

                                if (adapterType.Name == "OracleDataAdapter")
                                    query = query.Replace('@', ':');

                                connection.ExecuteNonQuery(query, adapterName, connectionString);
                            }
                            catch (Exception ex)
                            {
                                processException("LocalOutputAdapter", new EventArgs<Exception>(new InvalidOperationException("Failed to add associated historian reader input adapter for local historian: " + ex.Message, ex)));
                            }
                        }
                    }
                }

                // Sort valid historians for binary search
                validHistorians.Sort();

                // Create a list to track categories to remove
                HashSet<string> categoriesToRemove = new HashSet<string>();

                // Search for unused settings categories
                foreach (PropertyInformation info in configFile.Settings.ElementInformation.Properties)
                {
                    name = info.Name;

                    if (name.EndsWith("MetadataFile") && validHistorians.BinarySearch(name.Substring(0, name.IndexOf("MetadataFile"))) < 0)
                        categoriesToRemove.Add(name);

                    if (name.EndsWith("StateFile") && validHistorians.BinarySearch(name.Substring(0, name.IndexOf("StateFile"))) < 0)
                        categoriesToRemove.Add(name);

                    if (name.EndsWith("IntercomFile") && validHistorians.BinarySearch(name.Substring(0, name.IndexOf("IntercomFile"))) < 0)
                        categoriesToRemove.Add(name);

                    if (name.EndsWith("ArchiveFile") && validHistorians.BinarySearch(name.Substring(0, name.IndexOf("ArchiveFile"))) < 0)
                        categoriesToRemove.Add(name);

                    if (name.EndsWith("AdoMetadataProvider") && validHistorians.BinarySearch(name.Substring(0, name.IndexOf("AdoMetadataProvider"))) < 0)
                        categoriesToRemove.Add(name);

                    if (name.EndsWith("OleDbMetadataProvider") && validHistorians.BinarySearch(name.Substring(0, name.IndexOf("OleDbMetadataProvider"))) < 0)
                        categoriesToRemove.Add(name);

                    if (name.EndsWith("RestWebServiceMetadataProvider") && validHistorians.BinarySearch(name.Substring(0, name.IndexOf("RestWebServiceMetadataProvider"))) < 0)
                        categoriesToRemove.Add(name);

                    if (name.EndsWith("MetadataService") && validHistorians.BinarySearch(name.Substring(0, name.IndexOf("MetadataService"))) < 0)
                        categoriesToRemove.Add(name);

                    if (name.EndsWith("TimeSeriesDataService") && validHistorians.BinarySearch(name.Substring(0, name.IndexOf("TimeSeriesDataService"))) < 0)
                        categoriesToRemove.Add(name);

                    if (name.EndsWith("HadoopReplicationProvider") && validHistorians.BinarySearch(name.Substring(0, name.IndexOf("HadoopReplicationProvider"))) < 0)
                        categoriesToRemove.Add(name);
                }

                if (categoriesToRemove.Count > 0)
                {
                    statusMessage("LocalOutputAdapter", new EventArgs<string>("Removing unused local historian configuration settings..."));

                    // Remove any unused settings categories
                    foreach (string category in categoriesToRemove)
                    {
                        configFile.Settings.Remove(category);
                    }
                }

                // Save any applied changes
                configFile.Save();
            }
        }

        /// <summary>
        /// Creates a parameterized query string for the underlying database type 
        /// based on the given format string and the parameter names.
        /// </summary>
        /// <param name="adapterType">The adapter type used to determine the underlying database type.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="parameterNames">A string array that contains zero or more parameter names to format.</param>
        /// <returns>A parameterized query string based on the given format and parameter names.</returns>
        private static string ParameterizedQueryString(Type adapterType, string format, params string[] parameterNames)
        {
            bool oracle = adapterType.Name == "OracleDataAdapter";
            char paramChar = oracle ? ':' : '@';
            object[] parameters = parameterNames.Select(name => paramChar + name).ToArray();
            return string.Format(format, parameters);
        }

        // Create an http namespace reservation
        private static void AddNamespaceReservation(Uri serviceUri)
        {
            OperatingSystem OS = Environment.OSVersion;
            ProcessStartInfo psi = null;
            string parameters = null;

            if (OS.Platform == PlatformID.Win32NT)
            {
                if (OS.Version.Major > 5)
                {
                    // Vista, Windows 2008, Window 7, etc use "netsh" for reservations
                    string everyoneUser = new SecurityIdentifier("S-1-1-0").Translate(typeof(NTAccount)).ToString();
                    parameters = string.Format(@"http add urlacl url={0}://+:{1}{2} user=\{3}", serviceUri.Scheme, serviceUri.Port, serviceUri.AbsolutePath, everyoneUser);
                    psi = new ProcessStartInfo("netsh", parameters);
                }
                else
                {
                    // Attempt to use "httpcfg" for older Windows versions...
                    parameters = string.Format(@"set urlacl /u {0}://*:{1}{2}/ /a D:(A;;GX;;;S-1-1-0)", serviceUri.Scheme, serviceUri.Port, serviceUri.AbsolutePath);
                    psi = new ProcessStartInfo("httpcfg", parameters);
                }
            }

            if (psi != null)
            {
                psi.Verb = "runas";
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.UseShellExecute = false;
                psi.Arguments = parameters;

                using (Process shell = new Process())
                {
                    shell.StartInfo = psi;
                    shell.Start();
                    if (!shell.WaitForExit(5000))
                        shell.Kill();
                }
            }
        }

        private static void RemoveNamespaceReservation(Uri serviceUri)
        {
            OperatingSystem OS = Environment.OSVersion;
            ProcessStartInfo psi = null;
            string parameters = null;

            if (OS.Platform == PlatformID.Win32NT)
            {
                if (OS.Version.Major > 5)
                {
                    // Vista, Windows 2008, Window 7, etc use "netsh" for reservations
                    parameters = string.Format(@"http delete urlacl url={0}://+:{1}{2}", serviceUri.Scheme, serviceUri.Port, serviceUri.AbsolutePath);
                    psi = new ProcessStartInfo("netsh", parameters);
                }
                else
                {
                    // Attempt to use "httpcfg" for older Windows versions...
                    parameters = string.Format(@"delete urlacl /u {0}://*:{1}{2}/", serviceUri.Scheme, serviceUri.Port, serviceUri.AbsolutePath);
                    psi = new ProcessStartInfo("httpcfg", parameters);
                }
            }

            if (psi != null)
            {
                psi.Verb = "runas";
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.UseShellExecute = false;
                psi.Arguments = parameters;

                using (Process shell = new Process())
                {
                    shell.StartInfo = psi;
                    shell.Start();
                    if (!shell.WaitForExit(5000))
                        shell.Kill();
                }
            }
        }

        #endregion
    }
}
