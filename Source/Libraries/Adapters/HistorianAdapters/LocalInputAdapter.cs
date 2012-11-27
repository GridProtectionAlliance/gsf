//******************************************************************************************************
//  LocalInputAdapter.cs - Gbtc
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
//  11/09/2011 - Ritchie
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF;
using GSF.Historian;
using GSF.Historian.Files;
using GSF.IO;

namespace HistorianAdapters
{
    /// <summary>
    /// Represents an output adapter that publishes measurements to TVA Historian for archival.
    /// </summary>
    [Description("Local Historian Reader: reads data from local openHistorian for replay.")]
    public class LocalInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Constants
        private const long DefaultPublicationInterval = 333333;

        // Fields
        private System.Timers.Timer m_readTimer;
        private string m_archiveLocation;
        private ArchiveFile m_archiveFile;
        private IEnumerator<IDataPoint> m_dataReader;
        private string m_instanceName;
        private long m_publicationInterval;
        private long m_publicationTime;
        private bool m_simulateTimestamp;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="LocalInputAdapter"/>.
        /// </summary>
        public LocalInputAdapter()
        {
            // Setup a read timer
            m_readTimer = new System.Timers.Timer();
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
        Description("Define the archive location (i.e., the file system path) of the historical data.")]
        public string ArchiveLocation
        {
            get
            {
                return m_archiveLocation;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(m_archiveLocation))
                    throw new ArgumentNullException("value", "The archiveLocation setting must be specified.");

                m_archiveLocation = FilePath.GetDirectoryName(m_archiveLocation);
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
        /// basically a delay, or timer interval, overwhich to process data. A value of -1 means to use the default processing
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
                status.AppendFormat("      Publication interval: {0}\r\n", m_publicationInterval);

                if (m_archiveFile != null)
                    status.Append(m_archiveFile.Status);

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
                        if (m_readTimer != null)
                        {
                            m_readTimer.Elapsed -= m_readTimer_Elapsed;
                            m_readTimer.Dispose();
                        }
                        m_readTimer = null;

                        if (m_archiveFile != null)
                        {
                            m_archiveFile.Close();
                            m_archiveFile.Dispose();
                        }
                        m_archiveFile = null;
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
        /// Initializes this <see cref="RemoteInputAdapter"/>.
        /// </summary>
        /// <exception cref="ArgumentException"><b>HistorianID</b>, <b>Server</b>, <b>Port</b>, <b>Protocol</b>, or <b>InitiateConnection</b> is missing from the <see cref="AdapterBase.Settings"/>.</exception>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting, errorMessage = "{0} is missing from settings - Example: instanceName=PPA; archiveLocation=C:\\Program Files\\openPDC\\Archive\\; publicationInterval=333333";

            // Validate settings.
            if (!settings.TryGetValue("instanceName", out m_instanceName))
                throw new ArgumentException(string.Format(errorMessage, "instanceName"));

            if (!settings.TryGetValue("archiveLocation", out m_archiveLocation))
                throw new ArgumentException(string.Format(errorMessage, "archiveLocation"));

            if (!(settings.TryGetValue("publicationInterval", out setting) && long.TryParse(setting, out m_publicationInterval)))
                m_publicationInterval = DefaultPublicationInterval;

            if (settings.TryGetValue("simulateTimestamp", out setting))
                m_simulateTimestamp = setting.ParseBoolean();

            // Define output measurements this input adapter can support based on the instance name
            OutputSourceIDs = new string[] { m_instanceName };

            // Validate path name by assignment
            ArchiveLocation = m_archiveLocation;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="RemoteInputAdapter"/>.
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
                const string StateFileName = "{0}{1}_startup.dat";
                const string IntercomFileName = "{0}scratch.dat";
                const string MetadataFileName = "{0}{1}_dbase.dat";

                if (Directory.Exists(m_archiveLocation))
                {
                    // Specified directory is a valid one.
                    string[] matches = Directory.GetFiles(m_archiveLocation, "*_archive.d");

                    if (matches.Length > 0)
                    {
                        // Capture the instance name
                        string folder = FilePath.GetDirectoryName(matches[0]);
                        string instance = FilePath.GetFileName(matches[0]).Split('_')[0];

                        // Setup historian reader
                        m_archiveFile = new ArchiveFile();
                        m_archiveFile.StateFile = new StateFile();
                        m_archiveFile.StateFile.FileAccessMode = FileAccess.Read;
                        m_archiveFile.StateFile.FileName = string.Format(StateFileName, folder, instance);

                        m_archiveFile.IntercomFile = new IntercomFile();
                        m_archiveFile.IntercomFile.FileAccessMode = FileAccess.Read;
                        m_archiveFile.IntercomFile.FileName = string.Format(IntercomFileName, folder);

                        m_archiveFile.MetadataFile = new MetadataFile();
                        m_archiveFile.MetadataFile.FileAccessMode = FileAccess.Read;
                        m_archiveFile.MetadataFile.FileName = string.Format(MetadataFileName, folder, instance);

                        // Capture active archive
                        m_archiveFile.FileAccessMode = FileAccess.Read;
                        m_archiveFile.FileName = matches[0];

                        m_archiveFile.HistoricFileListBuildStart += m_archiveFile_HistoricFileListBuildStart;
                        m_archiveFile.HistoricFileListBuildComplete += m_archiveFile_HistoricFileListBuildComplete;
                        m_archiveFile.HistoricFileListBuildException += m_archiveFile_HistoricFileListBuildException;
                        m_archiveFile.DataReadException += m_archiveFile_DataReadException;

                        // Open the active archive
                        m_archiveFile.Open();

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
            if (m_readTimer != null)
            {
                m_readTimer.Enabled = false;

                lock (m_readTimer)
                {
                    m_dataReader = null;
                }
            }

            if (m_archiveFile != null)
            {
                m_archiveFile.Close();
                m_archiveFile.Dispose();
            }
            m_archiveFile = null;
        }

        // Kick start read process for historian
        private void StartDataReader(object state)
        {
            MeasurementKey[] requestedKeys = SupportsTemporalProcessing ? RequestedOutputMeasurementKeys : OutputMeasurements.MeasurementKeys().ToArray();

            if (Enabled && m_archiveFile != null && requestedKeys != null && requestedKeys.Length > 0)
            {
                IEnumerable<int> historianIDs = requestedKeys.Select(key => unchecked((int)key.ID));
                m_publicationTime = 0;

                // Start data read from historian
                lock (m_readTimer)
                {
                    TimeTag startTime = base.StartTimeConstraint < TimeTag.MinValue ? TimeTag.MinValue : base.StartTimeConstraint > TimeTag.MaxValue ? TimeTag.MaxValue : new TimeTag(base.StartTimeConstraint);
                    TimeTag stopTime = base.StopTimeConstraint < TimeTag.MinValue ? TimeTag.MinValue : base.StopTimeConstraint > TimeTag.MaxValue ? TimeTag.MaxValue : new TimeTag(base.StopTimeConstraint);

                    m_dataReader = m_archiveFile.ReadData(historianIDs, startTime, stopTime).GetEnumerator();
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
        private void m_readTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
                    while (timestamp > m_publicationTime)
                        m_publicationTime += m_publicationInterval;

                    do
                    {
                        // Lookup measurement key for this point
                        key = new MeasurementKey(Guid.Empty, unchecked((uint)currentPoint.HistorianID), m_instanceName);

                        // Add current measurement to the collection for publication
                        measurements.Add(new Measurement()
                        {
                            ID = key.SignalID,
                            Key = key,
                            Timestamp = m_simulateTimestamp ? PrecisionTimer.UtcNow.Ticks : timestamp,
                            Value = currentPoint.Value
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
                            // Finished reading all available data
                            m_readTimer.Enabled = false;
                            OnProcessingComplete();
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

        private void m_archiveFile_DataReadException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        private void m_archiveFile_HistoricFileListBuildException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        private void m_archiveFile_HistoricFileListBuildStart(object sender, EventArgs e)
        {
            OnStatusMessage("Building list of historic archive files...");
        }

        private void m_archiveFile_HistoricFileListBuildComplete(object sender, EventArgs e)
        {
            OnStatusMessage("Completed building list of historic archive files.");
        }

        #endregion
    }
}
