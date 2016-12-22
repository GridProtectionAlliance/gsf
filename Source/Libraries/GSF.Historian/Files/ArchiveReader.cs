//******************************************************************************************************
//  ArchiveReader.cs - Gbtc
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
//  09/29/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Timers;
using GSF.IO;
using Timer = System.Timers.Timer;

namespace GSF.Historian.Files
{
    /// <summary>
    /// Opens a local set of historian files for reading with active archive file being monitored for roll-over.
    /// </summary>
    /// <remarks>
    /// This class is expected to be used as an out-of-process instance of the <see cref="ArchiveFile"/> that
    /// will properly open archive files as read-only and yield to roll-over processing.
    /// </remarks>
    public class ArchiveReader : IDisposable
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when <see cref="ArchiveFile.Rollover()"/> to a new <see cref="ArchiveFile"/> is started.
        /// </summary>
        [Category("Rollover"),
        Description("Occurs when Rollover to a new ArchiveFile is started.")]
        public event EventHandler RolloverStart;

        /// <summary>
        /// Occurs when <see cref="ArchiveFile.Rollover()"/> to a new <see cref="ArchiveFile"/> is complete.
        /// </summary>
        [Category("Rollover"),
        Description("Occurs when Rollover to a new ArchiveFile is complete.")]
        public event EventHandler RolloverComplete;

        /// <summary>
        /// Occurs when the process of building historic <see cref="ArchiveFile"/> list is started.
        /// </summary>
        [Category("File"),
        Description("Occurs when the process of building historic ArchiveFile list is started.")]
        public event EventHandler HistoricFileListBuildStart;

        /// <summary>
        /// Occurs when the process of building historic <see cref="ArchiveFile"/> list is complete.
        /// </summary>
        [Category("File"),
        Description("Occurs when the process of building historic ArchiveFile list is complete.")]
        public event EventHandler HistoricFileListBuildComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered in historic <see cref="ArchiveFile"/> list building process.
        /// </summary>
        [Category("File"),
        Description("Occurs when an Exception is encountered in historic ArchiveFile list building process.")]
        public event EventHandler<EventArgs<Exception>> HistoricFileListBuildException;

        /// <summary>
        /// Occurs when the historic <see cref="ArchiveFile"/> list is updated to reflect addition or deletion of historic <see cref="ArchiveFile"/>s.
        /// </summary>
        [Category("File"),
        Description("Occurs when the historic ArchiveFile list is updated to reflect addition or deletion of historic ArchiveFiles.")]
        public event EventHandler HistoricFileListUpdated;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while reading <see cref="IDataPoint"/> from the current or historic <see cref="ArchiveFile"/>.
        /// </summary>
        [Category("Data"),
        Description("Occurs when an Exception is encountered while reading IDataPoint from the current or historic ArchiveFile.")]
        public event EventHandler<EventArgs<Exception>> DataReadException;

        // Fields
        private ArchiveFile m_archiveFile;
        private Timer m_rolloverWatcher;
        private readonly object m_watcherLock;
        private bool m_rolloverInProgress;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ArchiveReader"/>.
        /// </summary>
        public ArchiveReader()
        {
            m_watcherLock = new object();
            m_rolloverWatcher = new Timer();
            m_rolloverWatcher.Interval = 1000;
            m_rolloverWatcher.Elapsed += m_rolloverWatcher_Elapsed;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="ArchiveReader"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~ArchiveReader()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the name of the <see cref="ArchiveFile"/>.
        /// </summary>
        public string FileName
        {
            get
            {
                if ((object)m_archiveFile == null)
                    return null;

                return m_archiveFile.FileName;
            }
        }

        /// <summary>
        /// Gets the path to the offload location used by the <see cref="ArchiveFile"/>.
        /// </summary>
        public string ArchiveOffloadLocation
        {
            get
            {
                if ((object)m_archiveFile == null)
                    return null;

                return m_archiveFile.ArchiveOffloadLocation;
            }
        }

        /// <summary>
        /// Gets the <see cref="StateFile"/> used by the <see cref="ArchiveFile"/>.
        /// </summary>
        public StateFile StateFile
        {
            get
            {
                if ((object)m_archiveFile == null)
                    return null;

                return m_archiveFile.StateFile;
            }
        }

        /// <summary>
        /// Gets the <see cref="MetadataFile"/> used by the <see cref="ArchiveFile"/>.
        /// </summary>
        public MetadataFile MetadataFile
        {
            get
            {
                if ((object)m_archiveFile == null)
                    return null;

                return m_archiveFile.MetadataFile;
            }
        }

        /// <summary>
        /// Gets the <see cref="IntercomFile"/> used by the <see cref="ArchiveFile"/>.
        /// </summary>
        public IntercomFile IntercomFile
        {
            get
            {
                if ((object)m_archiveFile == null)
                    return null;

                return m_archiveFile.IntercomFile;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the internal <see cref="ArchiveFile"/>.
        /// </summary>
        public string Status
        {
            get
            {
                if ((object)m_archiveFile == null)
                    return "";

                return m_archiveFile.Status;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="ArchiveReader"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ArchiveReader"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_rolloverWatcher != null)
                        {
                            m_rolloverWatcher.Stop();
                            m_rolloverWatcher.Dispose();
                        }
                        m_rolloverWatcher = null;

                        CloseArchiveFile();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Opens the <see cref="ArchiveFile"/> for use.
        /// </summary>
        /// <param name="fileName">Archive .D archive file name.</param>
        /// <param name="offloadLocation">Path to archive offload location.</param>
        public void Open(string fileName, string offloadLocation = "")
        {
            CloseArchiveFile();
            m_archiveFile = OpenArchiveFile(fileName, offloadLocation.ToNonNullString());
        }

        /// <summary>
        /// Reads all <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, bool timeSorted = true)
        {
            return ReadData(historianID, TimeTag.MinValue, timeSorted);
        }

        /// <summary>
        /// Reads all <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/> for the specified <paramref name="historianIDs"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, bool timeSorted = true)
        {
            return ReadData(historianIDs, TimeTag.MinValue, timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime"><see cref="String"/> representation of the start time (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, string startTime, bool timeSorted = true)
        {
            return ReadData(historianID, startTime, TimeTag.MinValue.ToString(), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime"><see cref="String"/> representation of the start time (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, string startTime, bool timeSorted = true)
        {
            return ReadData(historianIDs, startTime, TimeTag.MinValue.ToString(), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime"><see cref="String"/> representation of the start time (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="endTime"><see cref="String"/> representation of the end time (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, string startTime, string endTime, bool timeSorted = true)
        {
            return ReadData(historianID, TimeTag.Parse(startTime), TimeTag.Parse(endTime), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime"><see cref="String"/> representation of the start time (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="endTime"><see cref="String"/> representation of the end time (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, string startTime, string endTime, bool timeSorted = true)
        {
            return ReadData(historianIDs, TimeTag.Parse(startTime), TimeTag.Parse(endTime), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="DateTime"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, DateTime startTime, bool timeSorted = true)
        {
            return ReadData(historianID, startTime, TimeTag.MinValue.ToDateTime(), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="DateTime"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, DateTime startTime, bool timeSorted = true)
        {
            return ReadData(historianIDs, startTime, TimeTag.MinValue.ToDateTime(), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="DateTime"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="endTime">End <see cref="DateTime"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, DateTime startTime, DateTime endTime, bool timeSorted = true)
        {
            return ReadData(historianID, new TimeTag(startTime), new TimeTag(endTime), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="DateTime"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="endTime">End <see cref="DateTime"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, DateTime startTime, DateTime endTime, bool timeSorted = true)
        {
            return ReadData(historianIDs, new TimeTag(startTime), new TimeTag(endTime), timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, TimeTag startTime, bool timeSorted = true)
        {
            return ReadData(historianID, startTime, TimeTag.MaxValue, timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, TimeTag startTime, bool timeSorted = true)
        {
            return ReadData(historianIDs, startTime, TimeTag.MaxValue, timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="endTime">End <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(int historianID, TimeTag startTime, TimeTag endTime, bool timeSorted = true)
        {
            return ReadData(new[] { historianID }, startTime, endTime, timeSorted);
        }

        /// <summary>
        /// Reads <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <param name="historianIDs">Historian identifiers for which <see cref="ArchiveDataPoint"/>s are to be retrieved.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="endTime">End <see cref="TimeTag"/> (in UTC) for the <see cref="ArchiveDataPoint"/>s to be retrieved.</param>
        /// <param name="timeSorted">Indicates whether the data retrieved from the archive should be time sorted.</param>
        /// <returns><see cref="IEnumerable{T}"/> object containing zero or more <see cref="ArchiveDataPoint"/>s.</returns>
        public IEnumerable<IDataPoint> ReadData(IEnumerable<int> historianIDs, TimeTag startTime, TimeTag endTime, bool timeSorted = true)
        {
            if ((object)m_archiveFile == null)
                throw new InvalidOperationException("Archive file is not open, cannot read data.");

            return m_archiveFile.ReadData(historianIDs, startTime, endTime, timeSorted);
        }

        // Opens an archive file as read-only and returns the ArchiveFile object.
        private ArchiveFile OpenArchiveFile(string fileName, string offloadLocation)
        {
            const string metadataFileName = "{0}{1}_dbase.dat";
            const string stateFileName = "{0}{1}_startup.dat";
            const string intercomFileName = "{0}scratch.dat";

            string archiveLocation = FilePath.GetDirectoryName(fileName);
            string archiveName = FilePath.GetFileName(fileName);
            string instance = archiveName.Substring(0, archiveName.LastIndexOf("_archive", StringComparison.OrdinalIgnoreCase));

            ArchiveFile file = new ArchiveFile
            {
                FileName = fileName,
                FileAccessMode = FileAccess.Read,
                MonitorNewArchiveFiles = true,
                PersistSettings = false,
                ArchiveOffloadLocation = offloadLocation,
                StateFile = new StateFile
                {
                    FileAccessMode = FileAccess.Read,
                    FileName = string.Format(stateFileName, archiveLocation, instance)
                },
                IntercomFile = new IntercomFile
                {
                    FileAccessMode = FileAccess.Read,
                    FileName = string.Format(intercomFileName, archiveLocation)
                },
                MetadataFile = new MetadataFile
                {
                    FileAccessMode = FileAccess.Read,
                    FileName = string.Format(metadataFileName, archiveLocation, instance),
                    LoadOnOpen = true
                }
            };

            file.DataReadException += file_DataReadException;
            file.HistoricFileListBuildComplete += file_HistoricFileListBuildComplete;
            file.HistoricFileListBuildException += file_HistoricFileListBuildException;
            file.HistoricFileListBuildStart += file_HistoricFileListBuildStart;
            file.HistoricFileListUpdated += file_HistoricFileListUpdated;
            file.RolloverComplete += file_RolloverComplete;
            file.RolloverStart += file_RolloverStart;

            // Initialize the archive file (starts file watchers)
            file.Initialize();

            // Open the active archive file
            file.Open();

            // Start the roll-over watch timer
            m_rolloverWatcher.Start();

            return file;
        }

        private void CloseArchiveFile()
        {
            if ((object)m_archiveFile != null)
            {
                m_archiveFile.DataReadException -= file_DataReadException;
                m_archiveFile.HistoricFileListBuildComplete -= file_HistoricFileListBuildComplete;
                m_archiveFile.HistoricFileListBuildException -= file_HistoricFileListBuildException;
                m_archiveFile.HistoricFileListBuildStart -= file_HistoricFileListBuildStart;
                m_archiveFile.HistoricFileListUpdated -= file_HistoricFileListUpdated;
                m_archiveFile.RolloverComplete -= file_RolloverComplete;
                m_archiveFile.RolloverStart -= file_RolloverStart;
                m_archiveFile.Dispose();
            }
            m_archiveFile = null;
        }

        // Monitors for roll-over notifications
        private void m_rolloverWatcher_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Don't start another rollover activity if one is already in progress...
            if (Monitor.TryEnter(m_watcherLock))
            {
                try
                {
                    // Read the inter-process communications file for changes in roll-over state
                    if ((object)m_archiveFile != null && (object)m_archiveFile.IntercomFile != null && m_archiveFile.IntercomFile.IsOpen)
                    {
                        m_archiveFile.IntercomFile.Load();
                        IntercomRecord record = m_archiveFile.IntercomFile.Read(1);

                        if ((object)record != null)
                        {
                            // Pause processing
                            if (record.RolloverInProgress)
                            {
                                if (!m_rolloverInProgress)
                                {
                                    m_rolloverInProgress = true;

                                    // Notify internal archive file components about the pending rollover
                                    m_archiveFile.RolloverWaitHandle.Reset();

                                    // Raise roll-over start event (sets m_rolloverInProgress flag in ArchiveFile)
                                    m_archiveFile.OnRolloverStart();

                                    // Wait for pending reads to yield
                                    m_archiveFile.WaitForReadersRelease();
                                }

                                // Close the active archive file stream so it can be rolled-over
                                if (m_archiveFile.IsOpen)
                                    m_archiveFile.CloseStream();
                            }

                            // Resume processing
                            if (!record.RolloverInProgress)
                            {
                                // Open new active archive file stream
                                if (!m_archiveFile.IsOpen)
                                    m_archiveFile.OpenStream();

                                if (m_rolloverInProgress)
                                {
                                    m_rolloverInProgress = false;

                                    // Raise roll-over complete event (resets m_rolloverInProgress flag in ArchiveFile)
                                    m_archiveFile.OnRolloverComplete();

                                    // Notify waiting internal archive components that rollover is complete
                                    m_archiveFile.RolloverWaitHandle.Set();
                                }
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    OnDataReadException(new InvalidOperationException("Exception encountered during roll-over processing: " + ex.Message, ex));
                }
                finally
                {
                    Monitor.Exit(m_watcherLock);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="RolloverStart"/> event.
        /// </summary>
        protected internal virtual void OnRolloverStart() => RolloverStart?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="RolloverComplete"/> event.
        /// </summary>
        protected internal virtual void OnRolloverComplete() => RolloverComplete?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="HistoricFileListBuildStart"/> event.
        /// </summary>
        protected virtual void OnHistoricFileListBuildStart() => HistoricFileListBuildStart?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="HistoricFileListBuildComplete"/> event.
        /// </summary>
        protected virtual void OnHistoricFileListBuildComplete() => HistoricFileListBuildComplete?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raise the <see cref="HistoricFileListBuildException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="HistoricFileListBuildException"/> event.</param>
        protected virtual void OnHistoricFileListBuildException(Exception ex) => HistoricFileListBuildException?.Invoke(this, new EventArgs<Exception>(ex));

        /// <summary>
        /// Raises the <see cref="HistoricFileListUpdated"/> event.
        /// </summary>
        protected virtual void OnHistoricFileListUpdated() => HistoricFileListUpdated?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="DataReadException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="DataReadException"/> event.</param>
        protected virtual void OnDataReadException(Exception ex) => DataReadException?.Invoke(this, new EventArgs<Exception>(ex));

        private void file_RolloverStart(object sender, EventArgs e) => OnRolloverStart();

        private void file_RolloverComplete(object sender, EventArgs e) => OnRolloverComplete();

        private void file_HistoricFileListUpdated(object sender, EventArgs e) => OnHistoricFileListUpdated();

        private void file_HistoricFileListBuildStart(object sender, EventArgs e) => OnHistoricFileListBuildStart();

        private void file_HistoricFileListBuildException(object sender, EventArgs<Exception> e) => OnHistoricFileListBuildException(e.Argument);

        private void file_HistoricFileListBuildComplete(object sender, EventArgs e) => OnHistoricFileListBuildComplete();

        private void file_DataReadException(object sender, EventArgs<Exception> e) => OnDataReadException(e.Argument);

        #endregion
    }
}
