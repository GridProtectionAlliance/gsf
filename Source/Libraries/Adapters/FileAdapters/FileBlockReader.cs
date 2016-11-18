//******************************************************************************************************
//  FileBlockReader.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  04/25/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using GSF;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.UI.Editors;

namespace FileAdapters
{
    /// <summary>
    /// Action adapter that reads blocks of data into buffers and publishes them as measurements.
    /// </summary>
    [Description("FileBlockReader: Reads blocks of data from files and publishes them as buffer block measurements")]
    public class FileBlockReader : FacileActionAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="FilePattern"/>.
        /// </summary>
        public const string DefaultFilePattern = "*";

        /// <summary>
        /// Default value for <see cref="BlockSize"/>.
        /// </summary>
        public const int DefaultBlockSize = 16384;

        /// <summary>
        /// Default value for <see cref="WatchInterval"/>.
        /// </summary>
        public const double DefaultWatchInterval = 5.0D;

        /// <summary>
        /// Default value for <see cref="ProcessInterval"/>.
        /// </summary>
        public const double DefaultProcessInterval = 1.0D;

        /// <summary>
        /// Default value for <see cref="RetransmissionThreshold"/>.
        /// </summary>
        public const double DefaultRetransmissionThreshold = 25.0D;

        /// <summary>
        /// Default value for <see cref="BlockSizeAdjustment"/>.
        /// </summary>
        public const double DefaultBlockSizeAdjustment = 5.0D;

        /// <summary>
        /// Default value for <see cref="ProcessIntervalAdjustment"/>.
        /// </summary>
        public const double DefaultProcessIntervalAdjustment = 5.0D;

        // Fields
        private string m_watchDirectory;
        private string m_filePattern;
        private int m_blockSize;
        private double m_watchInterval;
        private double m_processInterval;
        private double m_retransmissionThreshold;
        private double m_blockSizeAdjustment;
        private double m_processIntervalAdjustment;

        private readonly List<string> m_processedFiles;
        private readonly Queue<string> m_unprocessedFiles;
        private FileStream m_activeFileStream;
        private Timer m_watchTimer;
        private Timer m_processTimer;
        private byte[] m_buffer;
        private long m_bufferBlocksSent;
        private long m_bufferBlocksSentLastAdjustment;
        private int m_throttleMultiplier;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FileBlockReader"/> class.
        /// </summary>
        public FileBlockReader()
        {
            m_filePattern = DefaultFilePattern;
            BlockSize = DefaultBlockSize;
            m_watchInterval = DefaultWatchInterval;

            m_processedFiles = new List<string>();
            m_unprocessedFiles = new Queue<string>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the path to the directory to be watched for files.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the path to the directory to be watched for files."),
        CustomConfigurationEditor(typeof(FolderBrowserEditor))]
        public string WatchDirectory
        {
            get
            {
                return m_watchDirectory;
            }
            set
            {
                m_watchDirectory = value;
            }
        }

        /// <summary>
        /// Gets or sets the pattern used to match file that appear in the watch folder.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultFilePattern),
        Description("Determines which files are to processed when they appear in the watch folder.")]
        public string FilePattern
        {
            get
            {
                return m_filePattern;
            }
            set
            {
                m_filePattern = value;
            }
        }

        /// <summary>
        /// Gets or sets the statistic that defines the number of buffer block retransmissions in the system.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(null),
        Description("Determines the statistic that defines the number of buffer block retransmissions in the system."),
        CustomConfigurationEditor(typeof(RetransmissionStatPicker))]
        public string RetransmissionStat
        {
            get
            {
                return base.InputMeasurementKeys.First().ToString();
            }
            set
            {
                base.InputMeasurementKeys = ParseInputMeasurementKeys(DataSource, true, value).Take(1).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the size of each block of data that is read from the file.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultBlockSize),
        Description("Determines the size of each block of data that is read from the file.")]
        public int BlockSize
        {
            get
            {
                return m_blockSize;
            }
            set
            {
                m_blockSize = value;
                m_buffer = new byte[AdjustedBlockSize];
            }
        }

        /// <summary>
        /// Gets or sets the amount of time, in seconds, between each scan of the watch folder.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultWatchInterval),
        Description("Determines the amount of time, in seconds, between each scan of the watch folder.")]
        public double WatchInterval
        {
            get
            {
                return m_watchInterval;
            }
            set
            {
                m_watchInterval = value;

                if ((object)m_watchTimer != null)
                    m_watchTimer.Interval = value * 1000.0D;
            }
        }

        /// <summary>
        /// Gets or sets the amount of time, in seconds, between each publication of a block of data from the active file.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultProcessInterval),
        Description("Determines the amount of time, in seconds, between each publication of a block of data from the active file.")]
        public double ProcessInterval
        {
            get
            {
                return m_processInterval;
            }
            set
            {
                m_processInterval = value;

                if ((object)m_processTimer != null)
                    m_processTimer.Interval = AdjustedProcessInterval * 1000.0D;
            }
        }

        /// <summary>
        /// Gets or sets the percentage of buffer blocks that can be retransmitted
        /// during a reporting interval before the <see cref="FileBlockReader"/>
        /// begins throttling its buffer blocks.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultRetransmissionThreshold),
        Description("Defines the percentage of buffer blocks that can be retransmitted before throttling begins.")]
        public double RetransmissionThreshold
        {
            get
            {
                return m_retransmissionThreshold;
            }
            set
            {
                m_retransmissionThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the percentage of adjustment to be applied
        /// to the buffer size when throttling buffer blocks.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultBlockSizeAdjustment),
        Description("Defines the percentage by which to adjust the buffer size when throttling buffer blocks.")]
        public double BlockSizeAdjustment
        {
            get
            {
                return m_blockSizeAdjustment;
            }
            set
            {
                m_blockSizeAdjustment = value;
            }
        }

        /// <summary>
        /// Gets or sets the percentage of adjustment to be applied
        /// to the process interval when throttling buffer blocks.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultBlockSizeAdjustment),
        Description("Defines the percentage by which to adjust the process interval when throttling buffer blocks.")]
        public double ProcessIntervalAdjustment
        {
            get
            {
                return m_processIntervalAdjustment;
            }
            set
            {
                m_processIntervalAdjustment = value;
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
        /// Gets the block size after making adjustments for throttling.
        /// </summary>
        private int AdjustedBlockSize
        {
            get
            {
                double blockSizeAdjustment = m_blockSize * (m_blockSizeAdjustment * 0.01);
                return m_blockSize - (int)(m_throttleMultiplier * blockSizeAdjustment);
            }
        }

        /// <summary>
        /// Gets the process interval after making adjustments for throttling.
        /// </summary>
        private double AdjustedProcessInterval
        {
            get
            {
                double processIntervalAdjustment = m_processInterval * (m_processIntervalAdjustment * 0.01);
                return m_processInterval + (m_throttleMultiplier * processIntervalAdjustment);
            }
        }

        /// <summary>
        /// Gets or sets primary keys of input measurements the <see cref="FileBlockReader"/> expects, if any.
        /// </summary>
        public override MeasurementKey[] InputMeasurementKeys
        {
            get
            {
                return base.InputMeasurementKeys;
            }
            set
            {
                // InputMeasurementKeys was redefined via the RetransmissionStat parameter
            }
        }

        /// <summary>
        /// Gets or sets the frames per second to be used by the <see cref="FacileActionAdapterBase"/>.
        /// </summary>
        /// <remarks>
        /// Overridden to hide base class attributes.
        /// </remarks>
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
        /// <remarks>
        /// <para>Defines the time sensitivity to past measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too old.</para>
        /// <para>This becomes the amount of delay introduced by the concentrator to allow time for data to flow into the system.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one.</exception>
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
        /// <remarks>
        /// <para>Defines the time sensitivity to future measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        /// <para>This becomes the tolerated +/- accuracy of the local clock to real-time.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one.</exception>
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
        /// Initializes <see cref="FileBlockReader"/>.
        /// </summary>
        public override void Initialize()
        {
            const string ErrorMessage = @"{0} is missing from Settings - Example: watchDirectory=C:\Files; outputMeasurements=FILES:20";

            Dictionary<string, string> settings;
            string setting;
            int blockSize;

            base.Initialize();
            settings = Settings;

            // Required parameters

            if (!settings.TryGetValue("watchDirectory", out setting))
                throw new ArgumentException(string.Format(ErrorMessage, "watchDirectory"));

            m_watchDirectory = FilePath.GetAbsolutePath(setting);

            if (OutputMeasurements.Length <= 0)
                throw new ArgumentException(string.Format(ErrorMessage, "outputMeasurements"));

            // Optional parameters

            if (!settings.TryGetValue("filePattern", out m_filePattern))
                m_filePattern = DefaultFilePattern;

            if (settings.TryGetValue("retransmissionStat", out setting))
                RetransmissionStat = setting;
            else
                RetransmissionStat = string.Empty;

            if (settings.TryGetValue("blockSize", out setting) && int.TryParse(setting, out blockSize))
                BlockSize = blockSize;
            else
                BlockSize = DefaultBlockSize;

            if (!settings.TryGetValue("watchInterval", out setting) || !double.TryParse(setting, out m_watchInterval))
                m_watchInterval = DefaultWatchInterval;

            if (!settings.TryGetValue("processInterval", out setting) || !double.TryParse(setting, out m_processInterval))
                m_processInterval = DefaultProcessInterval;

            if (!settings.TryGetValue("retransmissionThreshold", out setting) || !double.TryParse(setting, out m_retransmissionThreshold))
                m_retransmissionThreshold = DefaultRetransmissionThreshold;

            if (!settings.TryGetValue("blockSizeAdjustment", out setting) || !double.TryParse(setting, out m_blockSizeAdjustment))
                m_blockSizeAdjustment = DefaultBlockSizeAdjustment;

            if (!settings.TryGetValue("processIntervalAdjustment", out setting) || !double.TryParse(setting, out m_processIntervalAdjustment))
                m_processIntervalAdjustment = DefaultProcessIntervalAdjustment;

            if (!Directory.Exists(m_watchDirectory))
                Directory.CreateDirectory(m_watchDirectory);

            m_watchTimer = new Timer();
            m_watchTimer.AutoReset = false;
            m_watchTimer.Interval = m_watchInterval * 1000.0D;
            m_watchTimer.Elapsed += WatchTimer_Elapsed;

            m_processTimer = new Timer();
            m_processTimer.AutoReset = false;
            m_processTimer.Interval = m_processInterval * 1000.0D;
            m_processTimer.Elapsed += ProcessTimer_Elapsed;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="FileBlockReader"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="FileBlockReader"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            if ((object)m_activeFileStream != null)
                return string.Format("Currently reading from file {0}", Path.GetFileName(m_unprocessedFiles.Peek())).CenterText(maxLength);

            return string.Format("{0} files processed by {1}", m_processedFiles.Count, Name).CenterText(maxLength);
        }

        /// <summary>
        /// Empties the processed file list.
        /// </summary>
        [AdapterCommand("Empties the processed file list so that newly dropped files with the same name as old processed files will not be deleted.", "Administrator", "Editor")]
        public void ClearProcessedFileList()
        {
            m_processedFiles.Clear();
        }

        /// <summary>
        /// Starts the <see cref="FileBlockReader"/> or restarts it if it is already running.
        /// </summary>
        public override void Start()
        {
            base.Start();
            m_watchTimer.Start();
            m_processTimer.Start();
        }

        /// <summary>
        /// Stops the <see cref="FileBlockReader"/>.
        /// </summary>		
        public override void Stop()
        {
            m_watchTimer.Stop();
            m_processTimer.Stop();

            if ((object)m_activeFileStream != null)
            {
                m_activeFileStream.Dispose();
                m_activeFileStream = null;
            }

            base.Stop();
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            base.QueueMeasurementsForProcessing(measurements);
            ManageThrottleAdjustments((int)measurements.Last().Value);
        }

        // Determines how to adjust the process interval and block size based on the
        // number of retransmissions during the last statistic reporting interval
        private void ManageThrottleAdjustments(int retransmissions)
        {
            long bufferBlocksSent = m_bufferBlocksSent;
            int throttleMultiplier = m_throttleMultiplier;
            int bufferBlocksSentSinceLastAdjustment = (int)(bufferBlocksSent - m_bufferBlocksSentLastAdjustment);

            // Adjust the throttle multiplier based on the number of retransmissions
            if (retransmissions >= bufferBlocksSentSinceLastAdjustment * (m_retransmissionThreshold * 0.01))
                m_throttleMultiplier++;
            else if (retransmissions < bufferBlocksSentSinceLastAdjustment * (m_retransmissionThreshold * 0.005))
                m_throttleMultiplier--;

            // Throttle multiplier cannot be less than 0
            if (m_throttleMultiplier < 0)
                m_throttleMultiplier = 0;

            // If the throttle multiplier changed, update the block size and process interval
            if (throttleMultiplier != m_throttleMultiplier)
            {
                m_buffer = new byte[AdjustedBlockSize];
                m_processTimer.Interval = AdjustedProcessInterval * 1000.0D;
            }

            // Keep track of the buffer blocks sent for the next time we make adjustments
            m_bufferBlocksSentLastAdjustment = bufferBlocksSent;
        }

        // Scans the watch folder for new files to transfer
        private void WatchTimer_Elapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (Enabled)
            {
                // Scan each file and add them to the unprocessed files lists
                foreach (string file in FilePath.GetFileList(Path.Combine(m_watchDirectory, m_filePattern)))
                {
                    if (!m_unprocessedFiles.Contains(file))
                        m_unprocessedFiles.Enqueue(file);
                }

                // Done scanning, so start the timer for another scan
                if ((object)m_watchTimer != null)
                    m_watchTimer.Start();
            }
        }

        // Reads the next block from the active file
        private void ProcessTimer_Elapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            byte[] buffer = m_buffer;
            int bytesRead = 0;
            int fileNameByteLength;
            FileInfo fileInfo;

            if (Enabled)
            {
                if ((object)m_activeFileStream != null)
                {
                    // Set first byte to 0 to indicate that
                    // this is not the beginning of a file
                    buffer[0] = 0;

                    // Read a block into memory
                    bytesRead = 1 + m_activeFileStream.Read(buffer, 1, buffer.Length - 1);

                    if (bytesRead == 1)
                    {
                        // Notify that processing is done for the current file
                        OnStatusMessage("Done processing file {0}.", Path.GetFileName(m_unprocessedFiles.Peek()));

                        // Delete the now-processed file
                        m_activeFileStream.Dispose();
                        m_activeFileStream = null;
                        File.Delete(m_unprocessedFiles.Peek());

                        // Move unprocessed file into processed file list
                        m_processedFiles.Add(m_unprocessedFiles.Peek());
                        m_unprocessedFiles.Dequeue();
                    }
                }

                if ((object)m_activeFileStream == null && m_unprocessedFiles.Count > 0)
                {
                    // Notify that processing has started for a new file
                    OnStatusMessage("Now processing file {0}...", Path.GetFileName(m_unprocessedFiles.Peek()));

                    // Get info about the next file to process
                    fileInfo = new FileInfo(m_unprocessedFiles.Peek());

                    // Set first byte to 1 to indicate that
                    // this is the beginning of a file
                    buffer[0] = 1;

                    // Copy file info into the buffer
                    fileNameByteLength = Encoding.Unicode.GetByteCount(fileInfo.Name);
                    BigEndian.CopyBytes(fileNameByteLength, buffer, 1);
                    Encoding.Unicode.GetBytes(fileInfo.Name, 0, fileInfo.Name.Length, buffer, 5);
                    BigEndian.CopyBytes(fileInfo.Length, buffer, 5 + fileNameByteLength);
                    bytesRead = 1 + 4 + fileNameByteLength + 8;

                    // Wait for lock to open the file
                    FilePath.WaitForReadLock(fileInfo.FullName);

                    // Open file and read
                    m_activeFileStream = fileInfo.OpenRead();
                    bytesRead += m_activeFileStream.Read(buffer, bytesRead, buffer.Length - bytesRead);
                }

                if ((object)m_activeFileStream != null)
                {
                    // Publish next block of file data
                    OnNewMeasurement(new BufferBlockMeasurement(buffer, 0, bytesRead)
                    {
                        CommonMeasurementFields = OutputMeasurements[0].CommonMeasurementFields,
                        Timestamp = DateTime.UtcNow.Ticks
                    });
                }

                // Done reading, so start the timer for another read
                if ((object)m_processTimer != null)
                    m_processTimer.Start();
            }
        }

        /// <summary>
        /// Raises the <see cref="InputAdapterBase.NewMeasurements"/> event.
        /// </summary>
        private void OnNewMeasurement(IMeasurement measurement)
        {
            OnNewMeasurements(new[] { measurement });
            m_bufferBlocksSent++;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FileBlockReader"/> object and optionally releases the managed resources.
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
                        if ((object)m_activeFileStream != null)
                        {
                            m_activeFileStream.Dispose();
                            m_activeFileStream = null;
                        }

                        if ((object)m_watchTimer != null)
                        {
                            m_watchTimer.Stop();
                            m_watchTimer.Dispose();
                            m_watchTimer = null;
                        }

                        if ((object)m_processTimer != null)
                        {
                            m_processTimer.Stop();
                            m_processTimer.Dispose();
                            m_processTimer = null;
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

        #endregion
    }
}
