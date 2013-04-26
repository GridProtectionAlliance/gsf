//******************************************************************************************************
//  FileInputAdapter.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  04/25/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Timers;
using GSF;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace FileAdapters
{
    /// <summary>
    /// Input adapter that reads blocks of data into buffers and publishes them as measurements.
    /// </summary>
    public class FileInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Constants
        public const string DefaultFilePattern = "*";
        public const int DefaultBlockSize = 16384;
        public const double DefaultWatchInterval = 5.0D;
        public const double DefaultProcessInterval = 1.0D;

        // Fields
        private string m_watchDirectory;
        private string m_filePattern;
        private int m_blockSize;
        private double m_watchInterval;
        private double m_processInterval;

        private List<string> m_processedFiles;
        private Queue<string> m_unprocessedFiles;
        private FileStream m_activeFileStream;
        private Timer m_watchTimer;
        private Timer m_processTimer;
        private byte[] m_buffer;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FileInputAdapter"/> class.
        /// </summary>
        public FileInputAdapter()
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
        Description("Defines the path to the directory to be watched for files.")]
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
                m_buffer = new byte[m_blockSize];
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
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="FileInputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            const string errorMessage = @"{0} is missing from Settings - Example: watchDirectory=C:\Files; outputMeasurements=FILES:20";

            Dictionary<string, string> settings;
            string setting;
            int blockSize;

            base.Initialize();
            settings = Settings;

            // Required parameters

            if (!settings.TryGetValue("watchDirectory", out setting))
                throw new ArgumentException(string.Format(errorMessage, "watchDirectory"));

            m_watchDirectory = FilePath.GetAbsolutePath(setting);

            if (OutputMeasurements.Length <= 0)
                throw new ArgumentException(string.Format(errorMessage, "outputMeasurements"));

            // Optional parameters

            if (!settings.TryGetValue("filePattern", out m_filePattern))
                m_filePattern = DefaultFilePattern;

            if (settings.TryGetValue("blockSize", out setting) && int.TryParse(setting, out blockSize))
                BlockSize = blockSize;
            else
                BlockSize = DefaultBlockSize;

            if (!settings.TryGetValue("watchInterval", out setting) || !double.TryParse(setting, out m_watchInterval))
                m_watchInterval = DefaultWatchInterval;

            if (!settings.TryGetValue("processInterval", out setting) || !double.TryParse(setting, out m_processInterval))
                m_processInterval = DefaultProcessInterval;

            m_watchTimer = new Timer();
            m_watchTimer.AutoReset = false;
            m_watchTimer.Interval = m_watchInterval;
            m_watchTimer.Elapsed += WatchTimer_Elapsed;

            m_processTimer = new Timer();
            m_processTimer.AutoReset = false;
            m_processTimer.Interval = m_processInterval;
            m_processTimer.Elapsed += ProcessTimer_Elapsed;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="FileInputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="FileInputAdapter"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            if ((object)m_activeFileStream != null)
                return string.Format("Currently reading from file {0}", m_unprocessedFiles.Peek()).CenterText(maxLength);

            return string.Format("{0} files processed by {1}", m_processedFiles.Count, Name).CenterText(maxLength);
        }

        /// <summary>
        /// Empties the processed file list.
        /// </summary>
        [AdapterCommand("Empties the processed file list so that newly dropped files with the same name as old processed files will not be deleted.")]
        public void ClearProcessedFileList()
        {
            m_processedFiles.Clear();
        }

        /// <summary>
        /// Attempts to connect to data input source.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_watchTimer.Start();
            m_processTimer.Start();
        }

        /// <summary>
        /// Attempts to disconnect from data input source.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_watchTimer.Stop();
            m_processTimer.Stop();
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
            int bytesRead = 0;
            int fileNameByteLength;
            FileInfo fileInfo;

            if (Enabled)
            {
                if ((object)m_activeFileStream != null)
                {
                    // Set first byte to 0 to indicate that
                    // this is not the beginning of a file
                    m_buffer[0] = 0;

                    // Read a block into memory
                    bytesRead = 1 + m_activeFileStream.Read(m_buffer, 1, m_blockSize);

                    if (bytesRead == 1)
                    {
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
                    // Get info about the next file to process
                    fileInfo = new FileInfo(m_unprocessedFiles.Peek());

                    // Set first byte to 1 to indicate that
                    // this is the beginning of a file
                    m_buffer[0] = 1;

                    // Copy file info into the buffer
                    fileNameByteLength = Encoding.Unicode.GetByteCount(fileInfo.Name);
                    EndianOrder.BigEndian.CopyBytes(fileNameByteLength, m_buffer, 1);
                    Encoding.Unicode.GetBytes(fileInfo.Name, 0, fileInfo.Name.Length, m_buffer, 5);
                    EndianOrder.BigEndian.CopyBytes(fileInfo.Length, m_buffer, 5 + fileNameByteLength);
                    bytesRead = 1 + 4 + fileNameByteLength + 8;
                    
                    // Wait for lock to open the file
                    FilePath.WaitForReadLock(fileInfo.FullName);

                    // Open file and read
                    m_activeFileStream = fileInfo.OpenRead();
                    bytesRead += m_activeFileStream.Read(m_buffer, bytesRead, m_blockSize - bytesRead);
                }

                if ((object)m_activeFileStream != null)
                {
                    // Publish next block of file data
                    OnNewMeasurement(new BufferBlockMeasurement(m_buffer, 0, bytesRead)
                    {
                        ID = OutputMeasurements[0].ID,
                        Key = OutputMeasurements[0].Key,
                        Timestamp = PrecisionTimer.UtcNow.Ticks
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
            OnNewMeasurements(new IMeasurement[] { measurement });
        }

        #endregion
    }
}
