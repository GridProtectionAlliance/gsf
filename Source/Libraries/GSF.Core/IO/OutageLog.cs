//******************************************************************************************************
//  OutageLog.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/24/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GSF.IO
{
    /// <summary>
    /// Represents a persisted log of outages as a list of start and stop times.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class serializes a list of outages (e.g., a connection outage or data loss) where each outage
    /// consists of a start and end time. The outages are persisted in a log file so that the log can be
    /// operated on even through host application restarts until the outages are processed.
    /// </para>
    /// <para>
    /// No members in the <see cref="OutageLog"/> are guaranteed to be thread safe. Make sure any calls are
    /// synchronized when simultaneously accessed from different threads.
    /// </para>
    /// </remarks>
    public class OutageLog : IProvideStatus, IDisposable
    {
        #region [ Members ]

        // Nested Types
        private static class File
        {
            public static StreamReader OpenText(string path)
            {
                return new StreamReader(new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read));
            }

            public static FileStream OpenWrite(string path)
            {
                return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }

            public static DateTime GetLastWriteTimeUtc(string path)
            {
                return System.IO.File.GetLastWriteTimeUtc(path);
            }
        }

        // Constants

        /// <summary>
        /// Date-time format used by <see cref="OutageLog"/>.
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        // Events

        /// <summary>
        /// Event is raised when the outage log is modified.
        /// </summary>
        public event EventHandler LogModified;

        /// <summary>
        /// Event is raised when there is an exception encountered while processing outage log.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        /// <summary>
        /// Raised after the outage log has been properly disposed.
        /// </summary>
        public event EventHandler Disposed;

        // Fields
        private readonly List<Outage> m_outages;
        private SafeFileWatcher m_logFileWatcher;
        private string m_fileName;
        private int m_suppressFileWatcher;
        private long m_lastReadTime;
        private long m_totalReads;
        private long m_totalWrites;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="OutageLog"/>.
        /// </summary>
        public OutageLog()
        {
            m_outages = new List<Outage>();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="OutageLog"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~OutageLog()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the file name for the outage log; file name can be set with a relative path.
        /// </summary>
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value));

                m_fileName = FilePath.GetAbsolutePath(FilePath.GetValidFilePath(value));

                LogFileWatcher = new SafeFileWatcher(FilePath.GetDirectoryName(m_fileName), FilePath.GetFileName(m_fileName))
                {
                    NotifyFilter = NotifyFilters.LastWrite
                };
            }
        }

        /// <summary>
        /// Gets the full list of outages in the log.
        /// </summary>
        public List<Outage> Outages
        {
            get
            {
                using (GetFileLock(File.OpenText))
                {
                    return new List<Outage>(m_outages);
                }
            }
        }

        /// <summary>
        /// Gets the number of outages in the log.
        /// </summary>
        public int Count
        {
            get
            {
                using (GetFileLock(File.OpenText))
                {
                    return m_outages.Count;
                }
            }
        }

        /// <summary>
        /// Gets the current status details about <see cref="OutageLog"/>.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("      Outage log file name: {0}", FilePath.TrimFileName(m_fileName.ToNonNullString("undefined"), 51));
                status.AppendLine();
                status.AppendFormat("               Total reads: {0:N0}", m_totalReads);
                status.AppendLine();
                status.AppendFormat("              Total writes: {0:N0}", m_totalWrites);
                status.AppendLine();
                status.AppendFormat("           Outage log size: {0:N0} outages", Count);
                status.AppendLine();
                status.AppendFormat("    Monitoring for updates: {0}", (object)LogFileWatcher != null);
                status.AppendLine();

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets a flag that indicates whether the object has been disposed.
        /// </summary>
        public bool IsDisposed => m_disposed;

        private SafeFileWatcher LogFileWatcher
        {
            get
            {
                return m_logFileWatcher;
            }
            set
            {
                if ((object)m_logFileWatcher != null)
                {
                    m_logFileWatcher.EnableRaisingEvents = false;
                    m_logFileWatcher.Changed -= m_logFileWatcher_Changed;

                    if (m_logFileWatcher != value)
                        m_logFileWatcher.Dispose();
                }

                m_logFileWatcher = value;

                if ((object)m_logFileWatcher != null)
                {
                    m_logFileWatcher.Changed += m_logFileWatcher_Changed;
                    m_logFileWatcher.EnableRaisingEvents = true;
                }
            }
        }

        // Gets the name of the outage log
        string IProvideStatus.Name => FilePath.GetFileNameWithoutExtension(m_fileName.ToNonNullString("undefined"));

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="OutageLog"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="OutageLog"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                        LogFileWatcher = null;
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.

                    if ((object)Disposed != null)
                        Disposed(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Initialize the outage log.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initialization performs initial outage log read.
        /// </para>
        /// <para>
        /// It is important to separate initialization from construction such that consumer can attach to events before class
        /// is initialized in case initialization causes events to be raised.
        /// </para>
        /// </remarks>
        public void Initialize()
        {
            if (string.IsNullOrWhiteSpace(m_fileName))
                throw new NullReferenceException("No outage log file name was specified");

            m_totalReads = 0;
            m_totalWrites = 0;

            using (StreamReader reader = GetFileLock(File.OpenText))
            {
                m_outages.AddRange(ReadLog(reader));
            }
        }

        /// <summary>
        /// Adds an outage to the <see cref="OutageLog"/>.
        /// </summary>
        /// <param name="startTime">Start time of outage.</param>
        /// <param name="endTime">End time of outage.</param>
        public void Add(DateTimeOffset startTime, DateTimeOffset endTime)
        {
            Add(new Outage(startTime, endTime));
        }

        /// <summary>
        /// Adds an outage to the <see cref="OutageLog"/>.
        /// </summary>
        /// <param name="outage">Outage to be added.</param>
        public void Add(Outage outage)
        {
            bool modified;

            using (FileStream stream = GetFileLock(File.OpenWrite))
            using (StreamReader reader = new StreamReader(stream))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                List<Outage> outages = ReadLog(reader);
                modified = !m_outages.SequenceEqual(outages);
                outages.AddRange(m_outages);
                outages.Add(Align(outage));
                outages = Outage.MergeOverlapping(outages).ToList();
                modified |= !m_outages.SequenceEqual(outages);

                if (modified)
                {
                    m_outages.Clear();
                    m_outages.AddRange(outages);
                    stream.SetLength(0L);
                    WriteLog(writer);
                }
            }

            if (modified)
                LogModified?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the first outage in the list of outages.
        /// </summary>
        /// <returns>The first outage, or null if there are no outages.</returns>
        public Outage First()
        {
            using (GetFileLock(File.OpenText))
            {
                return m_outages.FirstOrDefault();
            }
        }

        /// <summary>
        /// Removes the given outage from the outage log.
        /// </summary>
        /// <param name="outage">The outage to be removed from the outage log.</param>
        /// <returns>True if the outage was removed; false otherwise.</returns>
        public bool Remove(Outage outage)
        {
            bool removed;
            bool modified;

            using (FileStream stream = GetFileLock(File.OpenWrite))
            using (StreamReader reader = new StreamReader(stream))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                List<Outage> outages = ReadLog(reader);
                modified = !m_outages.SequenceEqual(outages);
                outages.AddRange(m_outages);
                outages = Outage.MergeOverlapping(outages).ToList();
                removed = outages.Remove(outage);
                modified |= !m_outages.SequenceEqual(outages);

                if (modified)
                {
                    m_outages.Clear();
                    m_outages.AddRange(outages);
                    stream.SetLength(0L);
                    WriteLog(writer);
                }
            }

            if (modified)
                LogModified?.Invoke(this, EventArgs.Empty);

            return removed;
        }

        // Reads the outage log.
        private List<Outage> ReadLog(StreamReader reader)
        {
            string line;
            string[] times;
            DateTimeOffset startTime, endTime;
            List<Outage> outages = new List<Outage>();

            while ((object)(line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                times = line.Split(';');

                if (times.Length == 2 &&
                    DateTimeOffset.TryParseExact(times[0].Trim(), DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowInnerWhite, out startTime) &&
                    DateTimeOffset.TryParseExact(times[1].Trim(), DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowInnerWhite, out endTime))
                {
                    try
                    {
                        outages.Add(new Outage(startTime, endTime));
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(new InvalidOperationException($"Failed to create outage from \"{line}\": {ex.Message}", ex));
                    }
                }
                else
                {
                    OnProcessException(new FormatException("Invalid date-time format. Failed to parse start and end times from: " + line));
                }
            }

            m_totalReads++;

            return outages;
        }

        // Writes the outage log - times are in a human readable format.
        private void WriteLog(StreamWriter writer)
        {
            bool isBackground = Thread.CurrentThread.IsBackground;

            try
            {
                Thread.CurrentThread.IsBackground = false;

                foreach (Outage outage in m_outages)
                {
                    writer.WriteLine("{0};{1}",
                        outage.Start.ToUniversalTime().ToString(DateTimeFormat, CultureInfo.InvariantCulture),
                        outage.End.ToUniversalTime().ToString(DateTimeFormat, CultureInfo.InvariantCulture));
                }

                Interlocked.Exchange(ref m_suppressFileWatcher, 1);
                m_totalWrites++;
            }
            finally
            {
                Thread.CurrentThread.IsBackground = isBackground;
            }
        }

        // Gets a lock on the file using the given lock function.
        private T GetFileLock<T>(Func<string, T> lockFunction)
        {
            const int Delay = 200;
            const int MaxRetries = 5000 / Delay;
            int retries = 0;

            while (true)
            {
                try
                {
                    return lockFunction(m_fileName);
                }
                catch (IOException)
                {
                    if (retries >= MaxRetries)
                        throw;
                }

                Thread.Sleep(Delay);
                retries++;
            }
        }

        // Raises ProcessException event.
        private void OnProcessException(Exception ex)
        {
            if ((object)ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        // Watches for changes to the log and adds additional outages.
        private void m_logFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            long lastWriteTime = File.GetLastWriteTimeUtc(m_fileName).Ticks;

            if (Interlocked.Exchange(ref m_lastReadTime, lastWriteTime) == lastWriteTime)
                return;

            if (Interlocked.CompareExchange(ref m_suppressFileWatcher, 0, 1) == 1)
                return;

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    bool modified;

                    using (FileStream stream = GetFileLock(File.OpenWrite))
                    using (StreamReader reader = new StreamReader(stream))
                    using (StreamWriter writer = new StreamWriter(stream))
                    {

                        List<Outage> outages = ReadLog(reader);
                        modified = !m_outages.SequenceEqual(outages);

                        if (modified)
                        {
                            outages.AddRange(m_outages);
                            m_outages.Clear();
                            m_outages.AddRange(Outage.MergeOverlapping(outages));
                            stream.SetLength(0L);
                            WriteLog(writer);
                        }
                    }

                    if (modified)
                        LogModified?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    OnProcessException(ex);
                }
            });
        }

        // Because the outage log saves timestamps down to the millisecond,
        // we must forcefully align incoming outages to the nearest millisecond.
        private Outage Align(Outage outage)
        {
            DateTimeOffset start = outage.Start.AddTicks(-(outage.Start.Ticks % TimeSpan.TicksPerMillisecond));
            DateTimeOffset end = outage.End.AddTicks(-(outage.End.Ticks % TimeSpan.TicksPerMillisecond));

            if (start == outage.Start && end == outage.End)
                return outage;

            return new Outage(start, end);
        }

        #endregion
    }
}
