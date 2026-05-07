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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using GSF;
using GSF.Collections;
using GSF.Diagnostics;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
#if !MONO
using GSF.TimeSeries.UI.Editors;
#endif
using Timer = System.Timers.Timer;

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
        public const string DefaultFilePattern = @"**\*";

        /// <summary>
        /// Default value for <see cref="BlockSize"/>.
        /// </summary>
        public const int DefaultBlockSize = 16384;

        /// <summary>
        /// Default value for <see cref="ScanInterval"/>.
        /// </summary>
        public const double DefaultScanInterval = 0.0D;

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

        /// <summary>
        /// Default value for <see cref="DeleteFilesWhenProcessed"/>.
        /// </summary>
        public const bool DefaultDeleteFilesWhenProcessed = false;

        // Fields
        private string m_filePattern;
        private string m_folderExclusion;
        private int m_blockSize;
        private double m_scanInterval;
        private double m_processInterval;

        private readonly ConcurrentQueue<string> m_unprocessedFiles;
        private int m_processedFiles;
        private FileStream m_activeFileStream;
        private FileProcessor m_fileProcessor;
        private Timer m_scanTimer;
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
            m_scanInterval = DefaultScanInterval;
            m_processInterval = DefaultProcessInterval;
            m_unprocessedFiles = new ConcurrentQueue<string>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the path to the directory to be watched for files.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the path to the directory to be watched for files.")]
#if !MONO
        [CustomConfigurationEditor(typeof(FolderBrowserEditor))]
#endif
        public string WatchDirectory { get; set; }

        /// <summary>
        /// Gets or sets the pattern used to match files that appear in the watch folder.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultFilePattern),
        Description("Determines which files are to be processed when they appear in the watch folder.")]
        public string FilePattern
        {
            get
            {
                return m_filePattern;
            }
            set
            {
                m_filePattern = value;

                if (m_fileProcessor is not null)
                    m_fileProcessor.Filter = value;
            }
        }

        /// <summary>
        /// Gets or sets the pattern used to determine whether a folder should be excluded from enumeration.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(null),
        Description("Determines which folders should be ignored when scanning the watch folder.")]
        public string FolderExclusion
        {
            get
            {
                return m_folderExclusion;
            }
            set
            {
                m_folderExclusion = value;

                if (m_fileProcessor is not null)
                    m_fileProcessor.FolderExclusion = value;
            }
        }

        /// <summary>
        /// Gets or sets the statistic that defines the number of buffer block retransmissions in the system.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(null),
        Description("Determines the statistic that defines the number of buffer block retransmissions in the system.")]
#if !MONO
        [CustomConfigurationEditor(typeof(RetransmissionStatPicker))]
#endif
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
        DefaultValue(DefaultScanInterval),
        Description("Determines the amount of time, in seconds, between each scan of the watch folder.")]
        public double ScanInterval
        {
            get
            {
                return m_scanInterval;
            }
            set
            {
                m_scanInterval = value;

                if (value > 0.0D && m_scanTimer is not null)
                    m_scanTimer.Interval = value * 1000.0D;
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

                if (m_processTimer is not null)
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
        public double RetransmissionThreshold { get; set; } = DefaultRetransmissionThreshold;

        /// <summary>
        /// Gets or sets the percentage of adjustment to be applied
        /// to the buffer size when throttling buffer blocks.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultBlockSizeAdjustment),
        Description("Defines the percentage by which to adjust the buffer size when throttling buffer blocks.")]
        public double BlockSizeAdjustment { get; set; } = DefaultBlockSizeAdjustment;

        /// <summary>
        /// Gets or sets the percentage of adjustment to be applied
        /// to the process interval when throttling buffer blocks.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultBlockSizeAdjustment),
        Description("Defines the percentage by which to adjust the process interval when throttling buffer blocks.")]
        public double ProcessIntervalAdjustment { get; set; } = DefaultProcessIntervalAdjustment;

        /// <summary>
        /// Gets or sets a flag that determines whether files should be deleted from the watch directory after they have been processed.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultDeleteFilesWhenProcessed),
        Description("Defines a flag that determines whether files should be deleted from the watch directory after they have been processed.")]
        public bool DeleteFilesWhenProcessed { get; set; } = DefaultDeleteFilesWhenProcessed;

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

        /// <summary>
        /// Gets the number of files processed by this adapter.
        /// </summary>
        private int ProcessedFiles => Interlocked.CompareExchange(ref m_processedFiles, 0, 0);

        /// <summary>
        /// Gets the block size after making adjustments for throttling.
        /// </summary>
        private int AdjustedBlockSize
        {
            get
            {
                double blockSizeAdjustment = m_blockSize * (BlockSizeAdjustment * 0.01);
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
                double processIntervalAdjustment = m_processInterval * (ProcessIntervalAdjustment * 0.01);
                return m_processInterval + (m_throttleMultiplier * processIntervalAdjustment);
            }
        }

        /// <inheritdoc/>
        public override string Status
        {
            get
            {
                StringBuilder status = new(base.Status);

                status.AppendLine($"           Processed files: {ProcessedFiles:N0}");
                status.AppendLine($"       Throttle Multiplier: {m_throttleMultiplier:N0}");
                status.AppendLine($"      Throttled Block Size: {AdjustedBlockSize:N0}");
                status.AppendLine($"Throttled Process Interval: {AdjustedProcessInterval:N0}");

                if (m_fileProcessor is not null)
                {
                    bool isEnumerating = m_fileProcessor.IsEnumerating;

                    status.AppendLine($"             Scanned Files: {m_fileProcessor.ProcessedFileCount:N0}");
                    status.AppendLine($"             Skipped Files: {m_fileProcessor.SkippedFileCount:N0}");
                    status.AppendLine($"               Is Scanning: {isEnumerating}");

                    if (isEnumerating)
                    {
                        IEnumerable<string> paths = m_fileProcessor.ActivelyEnumeratedPaths
                            .Select((path, i) => $"    [{i}] {path}");

                        string progress = string.Join(Environment.NewLine, paths);

                        if (progress.Length > 0)
                        {
                            status.AppendLine($"        Currently scanning:");
                            status.AppendLine(progress);
                        }
                    }
                }

                return status.ToString();
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

            WatchDirectory = FilePath.GetAbsolutePath(setting);

            if (OutputMeasurements.Length <= 0)
                throw new ArgumentException(string.Format(ErrorMessage, "outputMeasurements"));

            // Optional parameters

            if (settings.TryGetValue("filePattern", out setting))
                m_filePattern = setting;
            else
                m_filePattern = DefaultFilePattern;

            if (settings.TryGetValue("retransmissionStat", out setting))
                RetransmissionStat = setting;
            else
                RetransmissionStat = string.Empty;

            if (settings.TryGetValue("blockSize", out setting) && int.TryParse(setting, out blockSize))
                BlockSize = blockSize;
            else
                BlockSize = DefaultBlockSize;

            if (!settings.TryGetValue("scanInterval", out setting) || !double.TryParse(setting, out m_scanInterval))
            {
                if (!settings.TryGetValue("watchInterval", out setting) || !double.TryParse(setting, out m_scanInterval))
                    m_scanInterval = DefaultScanInterval;
            }

            if (!settings.TryGetValue("processInterval", out setting) || !double.TryParse(setting, out m_processInterval))
                m_processInterval = DefaultProcessInterval;

            if (settings.TryGetValue("retransmissionThreshold", out setting) && double.TryParse(setting, out double retransmissionThreshold))
                RetransmissionThreshold = retransmissionThreshold;
            else
                RetransmissionThreshold = DefaultRetransmissionThreshold;

            if (settings.TryGetValue("blockSizeAdjustment", out setting) && double.TryParse(setting, out double blockSizeAdjustment))
                BlockSizeAdjustment = blockSizeAdjustment;
            else
                BlockSizeAdjustment = DefaultBlockSizeAdjustment;

            if (settings.TryGetValue("processIntervalAdjustment", out setting) && double.TryParse(setting, out double processIntervalAdjustment))
                ProcessIntervalAdjustment = processIntervalAdjustment;
            else
                ProcessIntervalAdjustment = DefaultProcessIntervalAdjustment;

            if (settings.TryGetValue("deleteFilesWhenProcessed", out setting))
                DeleteFilesWhenProcessed = setting.ParseBoolean();
            else
                DeleteFilesWhenProcessed = DefaultDeleteFilesWhenProcessed;

            if (!Directory.Exists(WatchDirectory))
                Directory.CreateDirectory(WatchDirectory);

            m_fileProcessor = new FileProcessor();
            m_fileProcessor.Filter = m_filePattern;
            m_fileProcessor.FolderExclusion = m_folderExclusion;
            m_fileProcessor.EnumerationStrategy = FileEnumerationStrategy.Sequential;
            m_fileProcessor.OrderedEnumeration = true;
            m_fileProcessor.MaxThreadCount = 1;
            m_fileProcessor.TrackChanges = true;
            m_fileProcessor.Processing += FileProcessor_Processing;
            m_fileProcessor.Error += FileProcessor_Error;

            if (m_scanInterval > 0.0D)
            {
                m_scanTimer = new Timer();
                m_scanTimer.Interval = m_scanInterval * 1000.0D;
                m_scanTimer.Elapsed += ScanTimer_Elapsed;
            }

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
            if (m_activeFileStream is not null && m_unprocessedFiles.TryPeek(out string file))
                return $"Currently reading from file {Path.GetFileName(file)}".CenterText(maxLength);

            return $"{ProcessedFiles} files processed by {Name}".CenterText(maxLength);
        }

        /// <summary>
        /// Check if files have been removed from the directory and remove them from the index.
        /// </summary>
        [AdapterCommand("Remove missing files from the index.", "Administrator", "Editor")]
        public void TrimFileIndex()
        {
            bool Exists(string relativePath)
            {
                string fullPath = Path.Combine(WatchDirectory, relativePath);
                return File.Exists(fullPath);
            }

            using FileBackedDictionary<string, DateTime> fileIndex = GetFileIndex();

            List<string> missingFiles = fileIndex.Keys
                .Where(relativePath => !Exists(relativePath))
                .ToList();

            foreach (string filePath in missingFiles)
                fileIndex.Remove(filePath);

            if (missingFiles.Count > 0)
                fileIndex.Compact();
        }

        /// <summary>
        /// Scans the watch directory to check for missed files.
        /// </summary>
        [AdapterCommand("Scans the watch directory to check for missed files.", "Administrator", "Editor")]
        public void ScanWatchDirectory()
        {
            m_fileProcessor.EnumerateWatchDirectories();
        }

        /// <summary>
        /// Rescans the folder and sends all files from scratch.
        /// </summary>
        [AdapterCommand("Recans the folder and sends all files from scratch.", "Administrator", "Editor")]
        public void ResendAllFiles()
        {
            using (FileBackedDictionary<string, DateTime> fileIndex = GetFileIndex())
                fileIndex.Clear();

            m_fileProcessor.ResetIndexAndStatistics();
            m_fileProcessor.EnumerateWatchDirectories();
        }

        /// <summary>
        /// Stops the currently active scan.
        /// </summary>
        [AdapterCommand("Stops the currently active scan.", "Administrator", "Editor")]
        public void StopWatchDirectoryScan()
        {
            m_fileProcessor.StopEnumeration();
        }

        /// <summary>
        /// Starts the <see cref="FileBlockReader"/> or restarts it if it is already running.
        /// </summary>
        public override void Start()
        {
            base.Start();
            m_fileProcessor.AddTrackedDirectory(WatchDirectory);
            m_fileProcessor.EnumerateWatchDirectories();

            if (m_scanInterval > 0.0D)
                m_scanTimer.Start();

            m_processTimer.Start();
        }

        /// <summary>
        /// Stops the <see cref="FileBlockReader"/>.
        /// </summary>		
        public override void Stop()
        {
            m_fileProcessor?.StopEnumeration();
            m_fileProcessor?.ClearTrackedDirectories();
            m_scanTimer?.Stop();
            m_processTimer?.Stop();

            if (m_activeFileStream is not null)
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
            if (retransmissions >= bufferBlocksSentSinceLastAdjustment * (RetransmissionThreshold * 0.01))
                m_throttleMultiplier++;
            else if (retransmissions < bufferBlocksSentSinceLastAdjustment * (RetransmissionThreshold * 0.005))
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

        // When a file is created or changed, adds it to the unprocessed files queue
        private void FileProcessor_Processing(object sender, FileProcessorEventArgs e)
        {
            if (!HasProcessedFile(e.FullPath))
                m_unprocessedFiles.Enqueue(e.FullPath);
        }

        // Scans the watch folder for new files to transfer
        private void ScanTimer_Elapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (Enabled)
                m_fileProcessor.EnumerateWatchDirectories();
        }

        // Reads the next block from the active file
        private void ProcessTimer_Elapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            byte[] buffer = m_buffer;
            int bytesRead = 0;
            int filePathByteLength;
            FileInfo fileInfo;

            if (Enabled)
            {
                if (m_activeFileStream is not null)
                {
                    // Set first byte to 0 to indicate that
                    // this is not the beginning of a file
                    buffer[0] = 0;

                    // Read a block into memory
                    bytesRead = 1 + m_activeFileStream.Read(buffer, 1, buffer.Length - 1);

                    if (bytesRead == 1)
                    {
                        string activeFilePath = m_activeFileStream.Name;
                        string relativePath = GetRelativePath(activeFilePath);

                        // Notify that processing is done for the current file
                        OnStatusMessage(MessageLevel.Info, "Done processing file {0}.", relativePath);

                        // Move it to indexed files
                        m_unprocessedFiles.TryDequeue(out _);
                        Interlocked.Increment(ref m_processedFiles);
                        AddToIndex(activeFilePath);

                        // Close and delete the now-processed file
                        m_activeFileStream.Dispose();
                        m_activeFileStream = null;

                        if (DeleteFilesWhenProcessed)
                            File.Delete(activeFilePath);
                    }
                }

                if (m_activeFileStream is null && m_unprocessedFiles.TryPeek(out string filePath))
                {
                    string relativePath = GetRelativePath(filePath);

                    // Notify that processing has started for a new file
                    OnStatusMessage(MessageLevel.Info, "Now processing file {0}...", relativePath);

                    // Get info about the next file to process
                    fileInfo = new FileInfo(filePath);

                    // Set first byte to 1 to indicate that
                    // this is the beginning of a file
                    buffer[0] = 1;

                    // Copy file info into the buffer
                    filePathByteLength = Encoding.Unicode.GetByteCount(relativePath);
                    BigEndian.CopyBytes(filePathByteLength, buffer, 1);
                    Encoding.Unicode.GetBytes(relativePath, 0, relativePath.Length, buffer, 5);
                    BigEndian.CopyBytes(fileInfo.Length, buffer, 5 + filePathByteLength);
                    bytesRead = 1 + 4 + filePathByteLength + 8;

                    // Wait for lock to open the file
                    FilePath.WaitForReadLock(fileInfo.FullName);

                    // Open file and read
                    m_activeFileStream = fileInfo.OpenRead();
                    bytesRead += m_activeFileStream.Read(buffer, bytesRead, buffer.Length - bytesRead);
                }

                if (m_activeFileStream is not null)
                {
                    // Publish next block of file data
                    OnNewMeasurement(new BufferBlockMeasurement(buffer, 0, bytesRead)
                    {
                        Metadata = OutputMeasurements[0].Metadata,
                        Timestamp = DateTime.UtcNow.Ticks
                    });
                }

                // Done reading, so start the timer for another read
                if (m_processTimer is not null)
                    m_processTimer.Start();
            }
        }

        /// <summary>
        /// Determines whether the given file has already been processed.
        /// </summary>
        private bool HasProcessedFile(string filePath)
        {
            string relativePath = GetRelativePath(filePath);
            DateTime lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            using FileBackedDictionary<string, DateTime> fileIndex = GetFileIndex();

            return
                fileIndex.TryGetValue(relativePath, out DateTime oldWriteTime) &&
                lastWriteTime <= oldWriteTime;
        }

        /// <summary>
        /// Adds the given file to the index.
        /// </summary>
        private void AddToIndex(string filePath)
        {
            string relativePath = GetRelativePath(filePath);
            DateTime lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            using FileBackedDictionary<string, DateTime> fileIndex = GetFileIndex();
            fileIndex[relativePath] = lastWriteTime;
        }

        /// <summary>
        /// Gets path relative to <see cref="WatchDirectory"/>.
        /// </summary>
        private string GetRelativePath(string filePath)
        {
            string watchDirectory = WatchDirectory.EnsureEnd(Path.DirectorySeparatorChar);

            if (!filePath.StartsWith(watchDirectory, StringComparison.OrdinalIgnoreCase))
            {
                watchDirectory = Path.GetFullPath(watchDirectory);
                Debug.Assert(filePath.StartsWith(watchDirectory, StringComparison.OrdinalIgnoreCase));
            }

            return filePath.Substring(watchDirectory.Length);
        }

        /// <summary>
        /// Gets the index for files that were processed.
        /// </summary>
        private FileBackedDictionary<string, DateTime> GetFileIndex()
        {
            string appData = FilePath.GetCommonApplicationDataFolder();
            string pathToFileIndex = Path.Combine(appData, $"FileIndex_{ID:X8}.bin");
            return new FileBackedDictionary<string, DateTime>(pathToFileIndex, StringComparer.OrdinalIgnoreCase);
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
        /// Raises the <see cref="AdapterBase.ProcessException"/> event.
        /// </summary>
        private void FileProcessor_Error(object sender, ErrorEventArgs e)
        {
            OnProcessException(MessageLevel.Error, e.GetException());
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
                        if (m_activeFileStream is not null)
                        {
                            m_activeFileStream.Dispose();
                            m_activeFileStream = null;
                        }

                        if (m_fileProcessor is not null)
                        {
                            m_fileProcessor.StopEnumeration();
                            m_fileProcessor.ClearTrackedDirectories();
                            m_fileProcessor.Dispose();
                            m_fileProcessor = null;
                        }

                        if (m_scanTimer is not null)
                        {
                            m_scanTimer.Stop();
                            m_scanTimer.Dispose();
                            m_scanTimer = null;
                        }

                        if (m_processTimer is not null)
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
