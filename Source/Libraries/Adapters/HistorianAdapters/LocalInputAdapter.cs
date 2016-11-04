//******************************************************************************************************
//  LocalInputAdapter.cs - Gbtc
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
//  11/09/2011 - Ritchie
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using GSF;
using GSF.Historian;
using GSF.Historian.Files;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using Timer = System.Timers.Timer;

namespace HistorianAdapters
{
    /// <summary>
    /// Represents an output adapter that publishes measurements to openHistorian for archival.
    /// </summary>
    [Description("Local Historian Reader: Reads data from local openHistorian for replay")]
    public class LocalInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Constants
        private const long DefaultPublicationInterval = 333333;

        // Fields
        private Timer m_readTimer;
        private string m_archiveLocation;
        private string m_archiveOffloadLocation;
        private ArchiveReader m_archiveReader;
        private IEnumerator<IDataPoint> m_dataReader;
        private string m_instanceName;
        private long m_publicationInterval;
        private long m_publicationTime;
        private bool m_simulateTimestamp;
        private int[] m_historianIDs;
        private TimeTag m_startTime;
        private TimeTag m_stopTime;
        private bool m_autoRepeat;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="LocalInputAdapter"/>.
        /// </summary>
        public LocalInputAdapter()
        {
            // Setup a read timer
            m_readTimer = new Timer();
            m_readTimer.Elapsed += m_readTimer_Elapsed;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets instance name defined for this <see cref="LocalInputAdapter"/>.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the instance name the archive to read. Leave this value blank to default to the adapter name."),
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
        /// Gets or sets archive path for this <see cref="LocalInputAdapter"/>.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the archive location (i.e., the file system path) of the historical data."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.FolderBrowserEditor")]
        public string ArchiveLocation
        {
            get
            {
                return m_archiveLocation;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value), "The archiveLocation setting must be specified.");

                m_archiveLocation = FilePath.GetDirectoryName(value);
            }
        }

        /// <summary>
        /// Gets or sets archive offload path for this <see cref="LocalInputAdapter"/>.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the archive offload location (i.e., the file system path) of offloaded historical data."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.FolderBrowserEditor")]
        public string ArchiveOffloadLocation
        {
            get
            {
                return m_archiveOffloadLocation;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    m_archiveOffloadLocation = null;
                else
                    m_archiveOffloadLocation = FilePath.GetDirectoryName(value);
            }
        }

        /// <summary>
        /// Gets or sets the publication interval for this <see cref="LocalInputAdapter"/>.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the publication time interval in 100-nanosecond tick intervals for reading historical data."),
        DefaultValue(DefaultPublicationInterval)]
        public long PublicationInterval
        {
            get
            {
                return m_publicationInterval;
            }
            set
            {
                m_publicationInterval = value;
            }
        }

        /// <summary>
        /// Gets the start time for reading data.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the start time for reading data into real-time session, or do not define to start reading from the beginning of the available data. Either StartTimeConstraint or StopTimeConstraint must be defined in order to start reading data into real-time session. Value should not be defined when using adapter for subscription based temporal session support."),
        DefaultValue("")]
        public new string StartTimeConstraint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the stop time for reading data.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the stop time for reading data into real-time session, or do not define to keep reading until the end of the available data. Either StartTimeConstraint or StopTimeConstraint must be defined in order to start reading data into real-time session. Value should not be defined when using adapter for subscription based temporal session support."),
        DefaultValue("")]
        public new string StopTimeConstraint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that determines whether timestamps are
        /// simulated for the purposes of real-time concentration.
        /// </summary>
        [ConnectionStringParameter,
        Description("Indicate whether timestamps are simulated for real-time concentration."),
        DefaultValue(false)]
        public bool SimulateTimestamp
        {
            get
            {
                return m_simulateTimestamp;
            }
            set
            {
                m_simulateTimestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets value that determines if the input data should be replayed repeatedly.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define if the input data should be replayed repeatedly."),
        DefaultValue(false)]
        public bool AutoRepeat
        {
            get
            {
                return m_autoRepeat;
            }
            set
            {
                m_autoRepeat = value;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                // If the start/time constraints are defined in the connection string, it is expected that this adapter
                // will be used in real-time. For temporal sessions these properties will be defined via method call to
                // the SetTemporalConstraint function.
                Dictionary<string, string> settings = Settings;
                return !(settings.ContainsKey("startTimeConstraint") || settings.ContainsKey("stopTimeConstraint"));
            }
        }

        /// <summary>
        /// Gets or sets the desired processing interval, in milliseconds, for the adapter.
        /// </summary>
        /// <remarks>
        /// With the exception of the values of -1 and 0, this value specifies the desired processing interval for data, i.e.,
        /// basically a delay, or timer interval, over which to process data. A value of -1 means to use the default processing
        /// interval while a value of 0 means to process data as fast as possible.
        /// </remarks>
        public override int ProcessingInterval
        {
            get
            {
                return base.ProcessingInterval;
            }
            set
            {
                base.ProcessingInterval = value;

                // Set read timer interval to the requested processing interval
                m_readTimer.Interval = value <= 0 ? 1 : value;
            }
        }

        /// <summary>
        /// Gets flag that determines if this <see cref="LocalInputAdapter"/> uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.Append(base.Status);

                status.AppendFormat("             Instance name: {0}\r\n", m_instanceName);
                status.AppendFormat("          Archive location: {0}\r\n", FilePath.TrimFileName(m_archiveLocation, 51));
                status.AppendFormat("  Archive offload location: {0}\r\n", FilePath.TrimFileName(m_archiveOffloadLocation.ToNonNullString(), 51));
                status.AppendFormat("      Publication interval: {0}\r\n", m_publicationInterval);
                status.AppendFormat("               Auto-repeat: {0}\r\n", m_autoRepeat);
                status.AppendFormat("            Start time-tag: {0}\r\n", m_startTime);
                status.AppendFormat("             Stop time-tag: {0}\r\n", m_stopTime);

                if ((object)m_archiveReader != null)
                    status.Append(m_archiveReader.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="LocalInputAdapter"/> object and optionally releases the managed resources.
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
                        if ((object)m_readTimer != null)
                        {
                            m_readTimer.Elapsed -= m_readTimer_Elapsed;
                            m_readTimer.Dispose();
                            m_readTimer = null;
                        }

                        if ((object)m_archiveReader != null)
                        {
                            m_archiveReader.HistoricFileListBuildStart -= m_archiveReader_HistoricFileListBuildStart;
                            m_archiveReader.HistoricFileListBuildComplete -= m_archiveReader_HistoricFileListBuildComplete;
                            m_archiveReader.HistoricFileListBuildException -= m_archiveReader_HistoricFileListBuildException;
                            m_archiveReader.DataReadException -= m_archiveReader_DataReadException;
                            m_archiveReader.Dispose();
                            m_archiveReader.Dispose();
                            m_archiveReader = null;
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
        /// Initializes this <see cref="LocalInputAdapter"/>.
        /// </summary>
        /// <exception cref="ArgumentException"><b>HistorianID</b>, <b>Server</b>, <b>Port</b>, <b>Protocol</b>, or <b>InitiateConnection</b> is missing from the <see cref="AdapterBase.Settings"/>.</exception>
        public override void Initialize()
        {
            const string errorMessage = "{0} is missing from settings - Example: instanceName=PPA; archiveLocation=D:\\ArchiveFiles\\; publicationInterval=333333";

            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Validate settings.
            settings.TryGetValue("instanceName", out m_instanceName);

            if (((object)OutputSourceIDs == null || OutputSourceIDs.Length == 0) && string.IsNullOrEmpty(m_instanceName))
                throw new ArgumentException(string.Format(errorMessage, "instanceName"));

            if (!settings.TryGetValue("archiveLocation", out m_archiveLocation))
                throw new ArgumentException(string.Format(errorMessage, "archiveLocation"));

            settings.TryGetValue("archiveOffloadLocation", out m_archiveOffloadLocation);

            if (!(settings.TryGetValue("publicationInterval", out setting) && long.TryParse(setting, out m_publicationInterval)))
                m_publicationInterval = DefaultPublicationInterval;

            if (settings.TryGetValue("simulateTimestamp", out setting))
                m_simulateTimestamp = setting.ParseBoolean();

            if (settings.TryGetValue("autoRepeat", out setting))
                m_autoRepeat = setting.ParseBoolean();

            // Define output measurements this input adapter can support based on the instance name (if not already defined)
            if (string.IsNullOrEmpty(m_instanceName))
            {
                if ((object)OutputSourceIDs != null && OutputSourceIDs.Length > 0)
                    m_instanceName = OutputSourceIDs[0];
            }
            else
            {
                OutputSourceIDs = new[] { m_instanceName };
            }

            // Validate path names by assignment
            ArchiveLocation = m_archiveLocation;
            ArchiveOffloadLocation = m_archiveOffloadLocation;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="LocalInputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            if (Enabled && m_publicationTime > 0)
                return string.Format("Publishing data for {0}...", (new DateTime(m_publicationTime)).ToString("yyyy-MM-dd HH:mm:ss.fff")).CenterText(maxLength);

            return "Not currently publishing data".CenterText(maxLength);
        }

        /// <summary>
        /// Attempts to connect to this <see cref="LocalInputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            // This adapter is only engaged for history, so we don't process any data unless a temporal constraint is defined
            if (this.TemporalConstraintIsDefined())
            {
                // Turn off read timer if it's active
                m_readTimer.Enabled = false;

                // Attempt to open historian files
                if (Directory.Exists(m_archiveLocation))
                {
                    // Specified directory is a valid one.
                    string[] matches = Directory.GetFiles(m_archiveLocation, "*_archive*.d");

                    if (matches.Length > 0)
                    {
                        // Capture the instance name
                        string fileName = matches[0].Remove(matches[0].IndexOf("_archive", StringComparison.OrdinalIgnoreCase)) + "_archive.d";

                        // Setup historian reader
                        m_archiveReader = new ArchiveReader();
                        m_archiveReader.HistoricFileListBuildStart += m_archiveReader_HistoricFileListBuildStart;
                        m_archiveReader.HistoricFileListBuildComplete += m_archiveReader_HistoricFileListBuildComplete;
                        m_archiveReader.HistoricFileListBuildException += m_archiveReader_HistoricFileListBuildException;
                        m_archiveReader.DataReadException += m_archiveReader_DataReadException;

                        // Open the active archive
                        m_archiveReader.Open(fileName, m_archiveLocation);

                        try
                        {
                            // Start the data reader on its own thread so connection attempt can complete in a timely fashion...
                            ThreadPool.QueueUserWorkItem(StartDataReader);
                        }
                        catch (Exception ex)
                        {
                            // Process exception for logging
                            OnProcessException(new InvalidOperationException("Failed to start data reader due to exception: " + ex.Message, ex));
                        }
                    }
                }
                else
                {
                    OnProcessException(new InvalidOperationException("Cannot open historian files, directory does not exist: " + m_archiveLocation));
                }
            }
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="LocalInputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if ((object)m_readTimer != null)
            {
                m_readTimer.Enabled = false;

                lock (m_readTimer)
                {
                    m_dataReader = null;
                }
            }

            if ((object)m_archiveReader != null)
            {
                m_archiveReader.HistoricFileListBuildStart -= m_archiveReader_HistoricFileListBuildStart;
                m_archiveReader.HistoricFileListBuildComplete -= m_archiveReader_HistoricFileListBuildComplete;
                m_archiveReader.HistoricFileListBuildException -= m_archiveReader_HistoricFileListBuildException;
                m_archiveReader.DataReadException -= m_archiveReader_DataReadException;
                m_archiveReader.Dispose();
            }

            m_archiveReader = null;
        }

        // Kick start read process for historian
        private void StartDataReader(object state)
        {
            MeasurementKey[] requestedKeys = SupportsTemporalProcessing ? RequestedOutputMeasurementKeys : OutputMeasurements.MeasurementKeys().ToArray();

            if (Enabled && (object)m_archiveReader != null && (object)requestedKeys != null && requestedKeys.Length > 0)
            {
                m_historianIDs = requestedKeys.Select(key => unchecked((int)key.ID)).ToArray();
                m_publicationTime = 0;

                // Start data read from historian
                lock (m_readTimer)
                {
                    m_startTime = base.StartTimeConstraint < TimeTag.MinValue ? TimeTag.MinValue : base.StartTimeConstraint > TimeTag.MaxValue ? TimeTag.MaxValue : new TimeTag(base.StartTimeConstraint);
                    m_stopTime = base.StopTimeConstraint < TimeTag.MinValue ? TimeTag.MinValue : base.StopTimeConstraint > TimeTag.MaxValue ? TimeTag.MaxValue : new TimeTag(base.StopTimeConstraint);

                    m_dataReader = m_archiveReader.ReadData(m_historianIDs, m_startTime, m_stopTime).GetEnumerator();
                    m_readTimer.Enabled = m_dataReader.MoveNext();

                    if (m_readTimer.Enabled)
                    {
                        OnStatusMessage("Starting historical data read...");
                    }
                    else
                    {
                        OnStatusMessage("No historical data was available to read for given timeframe.");
                        OnProcessingComplete();
                    }
                }
            }
            else
            {
                m_readTimer.Enabled = false;
                OnStatusMessage("No measurement keys have been requested for reading, historian reader is idle.");
                OnProcessingComplete();
            }
        }

        // Process next data read
        private void m_readTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<IMeasurement> measurements = new List<IMeasurement>();

            if (Monitor.TryEnter(m_readTimer))
            {
                try
                {
                    IDataPoint currentPoint = m_dataReader.Current;
                    long timestamp = currentPoint.Time.ToDateTime().Ticks;
                    MeasurementKey key;

                    if (m_publicationTime == 0)
                        m_publicationTime = timestamp;

                    // Set next reasonable publication time
                    while (m_publicationTime < timestamp)
                        m_publicationTime += m_publicationInterval;

                    do
                    {
                        // Lookup measurement key for this point
                        key = MeasurementKey.LookUpOrCreate(m_instanceName, unchecked((uint)currentPoint.HistorianID));

                        // Add current measurement to the collection for publication
                        measurements.Add(new Measurement
                        {
                            Key = key,
                            Timestamp = m_simulateTimestamp ? DateTime.UtcNow.Ticks : timestamp,
                            Value = currentPoint.Value,
                            StateFlags = currentPoint.Quality.MeasurementQuality()
                        });

                        // Attempt to move to next record
                        if (m_dataReader.MoveNext())
                        {
                            // Read record value
                            currentPoint = m_dataReader.Current;
                            timestamp = currentPoint.Time.ToDateTime().Ticks;
                        }
                        else
                        {
                            if (timestamp < m_stopTime.ToDateTime().Ticks && m_startTime.ToDateTime().Ticks < timestamp)
                            {
                                // Could be attempting read with a future end time - in these cases attempt to re-read current data
                                // from now to end time in case any new data as been archived in the mean-time
                                m_startTime = new TimeTag(timestamp + Ticks.PerMillisecond);
                                m_dataReader = m_archiveReader.ReadData(m_historianIDs, m_startTime, m_stopTime).GetEnumerator();

                                if (!m_dataReader.MoveNext())
                                {
                                    // Finished reading all available data
                                    m_readTimer.Enabled = false;

                                    if (m_autoRepeat)
                                        ThreadPool.QueueUserWorkItem(StartDataReader);
                                    else
                                        OnProcessingComplete();
                                }
                            }
                            else
                            {
                                // Finished reading all available data
                                m_readTimer.Enabled = false;

                                if (m_autoRepeat)
                                    ThreadPool.QueueUserWorkItem(StartDataReader);
                                else
                                    OnProcessingComplete();
                            }

                            break;
                        }
                    }
                    while (timestamp <= m_publicationTime);
                }
                catch (InvalidOperationException)
                {
                    // Pooled timer thread executed after last read, verify timer has stopped
                    m_readTimer.Enabled = false;
                }
                finally
                {
                    Monitor.Exit(m_readTimer);
                }
            }

            // Publish all measurements for this time interval
            if (measurements.Count > 0)
                OnNewMeasurements(measurements);
        }

        private void m_archiveReader_DataReadException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        private void m_archiveReader_HistoricFileListBuildException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        private void m_archiveReader_HistoricFileListBuildStart(object sender, EventArgs e)
        {
            OnStatusMessage("Building list of historic archive files...");
        }

        private void m_archiveReader_HistoricFileListBuildComplete(object sender, EventArgs e)
        {
            OnStatusMessage("Completed building list of historic archive files.");
        }

        #endregion
    }
}
