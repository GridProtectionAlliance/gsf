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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GSF.Threading;

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
    public class OutageLog : ObservableCollection<Outage>, ISupportLifecycle, IProvideStatus
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Date-time format used by <see cref="OutageLog"/>.
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        // Events

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
        private readonly ManualResetEventSlim m_writeLogWaitHandle;
        private readonly ShortSynchronizedOperation m_readLogOperation;
        private readonly ShortSynchronizedOperation m_writeLogOperation;
        private readonly ShortSynchronizedOperation m_condenseLogOperation;
        private readonly object m_readWriteLock;
        private volatile bool m_postponeWriteOperations;
        private long m_totalReads;
        private long m_totalWrites;
        private long m_totalCondenses;
        private string m_fileName;
        private bool m_enabled;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="OutageLog"/>.
        /// </summary>
        public OutageLog()
        {
            m_writeLogWaitHandle = new ManualResetEventSlim(false);
            m_readLogOperation = new ShortSynchronizedOperation(ReadLog);
            m_writeLogOperation = new ShortSynchronizedOperation(WriteLog);
            m_condenseLogOperation = new ShortSynchronizedOperation(CondenseLog);
            m_readWriteLock = new object();
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
            }
        }

        /// <summary>        
        /// Gets or sets a boolean value that indicates whether the run-time log is enabled.
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
                QueueWriteLog();
            }
        }

        /// <summary>
        /// Gets a flag that indicates whether the object has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return m_disposed;
            }
        }

        // Gets the name of the outage log
        string IProvideStatus.Name
        {
            get
            {
                return FilePath.GetFileNameWithoutExtension(m_fileName.ToNonNullString("undefined"));
            }
        }

        /// <summary>
        /// Gets locking object for <see cref="OutageLog"/>.
        /// </summary>
        protected internal object ReadWriteLock
        {
            get
            {
                return m_readWriteLock;
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
                status.AppendFormat("      Log flushing enabled: {0}", Enabled);
                status.AppendLine();
                status.AppendFormat("      Actively reading log: {0} - {1:N0} total reads", m_readLogOperation.IsRunning, m_totalReads);
                status.AppendLine();
                status.AppendFormat("      Actively writing log: {0} - {1:N0} total writes", m_writeLogOperation.IsRunning, m_totalWrites);
                status.AppendLine();
                status.AppendFormat("   Actively condensing log: {0} - {1:N0} total condenses (with action)", m_condenseLogOperation.IsRunning, m_totalCondenses);
                status.AppendLine();
                status.AppendFormat("           Outage log size: {0:N0} outages", Count);
                status.AppendLine();

                return status.ToString();
            }
        }

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
                    {
                        if ((object)m_writeLogWaitHandle != null)
                        {
                            // Signal any waiting threads
                            m_writeLogWaitHandle.Set();
                            m_writeLogWaitHandle.Dispose();
                        }
                    }
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
            m_totalCondenses = 0;

            m_readLogOperation.RunOnce();
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
        /// Requests that outage log be flushed to disk.
        /// </summary>
        /// <remarks>
        /// Any change in <see cref="OutageLog"/> contents will automatically queue a log file flush.
        /// This function only exists to force a flush and allow consumer to use returned wait handle
        /// to block calling thread until flush has completed.
        /// </remarks>
        /// <returns>
        /// Wait handle that can block a calling thread until flush completes.
        /// </returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public ManualResetEventSlim Flush()
        {
            m_writeLogWaitHandle.Reset();
            m_writeLogOperation.RunOnceAsync();

            return m_writeLogWaitHandle;
        }

        /// <summary>
        /// Initiates a log read.
        /// </summary>
        protected void InitiateRead()
        {
            m_readLogOperation.RunOnceAsync();
        }

        /// <summary>
        /// Initiates a log write.
        /// </summary>
        protected void InitiateWrite()
        {
            m_writeLogOperation.RunOnceAsync();
        }

        // Reads the outage log.
        private void ReadLog()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(m_fileName))
                    throw new NullReferenceException("No outage log file name was specified");

                List<Outage> outages = new List<Outage>();

                if (File.Exists(m_fileName))
                {
                    using (StreamReader reader = File.OpenText(m_fileName))
                    {
                        string line;
                        string[] times;
                        DateTimeOffset startTime, endTime;

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
                    }
                }

                lock (m_readWriteLock)
                {
                    try
                    {
                        // Don't kick off write log operations during load
                        m_postponeWriteOperations = true;

                        Clear();

                        foreach (Outage outage in outages)
                        {
                            Add(outage);
                        }
                    }
                    finally
                    {
                        m_postponeWriteOperations = false;
                    }
                }

                m_totalReads++;
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Failed to read outage log: " + ex.Message, ex));
            }
        }

        // Writes the outage log - times are in a human readable format.
        private void WriteLog()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(m_fileName))
                    throw new NullReferenceException("No outage log file name was specified");

                Outage[] outages;

                lock (m_readWriteLock)
                {
                    outages = this.ToArray();
                }

                using (StreamWriter writer = File.CreateText(m_fileName))
                {
                    foreach (Outage outage in outages)
                    {
                        writer.WriteLine("{0};{1}",
                            outage.Start.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
                            outage.End.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
                    }
                }

                if (!m_disposed)
                    m_writeLogWaitHandle.Set();

                m_totalWrites++;
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Failed to write outage log: " + ex.Message, ex));
            }
        }

        // Condenses the outage log (i.e., reduces the overlapping outages into single outages)
        private void CondenseLog()
        {
            List<Outage> mergedOutages;
            bool originalPostponeState;
            int count;

            lock (m_readWriteLock)
            {
                originalPostponeState = m_postponeWriteOperations;

                try
                {
                    // Don't kick off write log operations during load
                    m_postponeWriteOperations = true;

                    // Merge all overlapping outages
                    mergedOutages = Outage.MergeOverlapping(this.OrderBy(outage => outage.Start)).ToList();

                    // Get the current total number of outages
                    count = Count;

                    // Any changes to the collection will trigger another condense operation
                    // so make sure to check whether this condense operation is necessary
                    // before attempting to modify the collection
                    if (count == mergedOutages.Count)
                        return;

                    Clear();

                    foreach (Outage outage in mergedOutages)
                        Add(outage);

                    m_totalCondenses++;
                }
                finally
                {
                    m_postponeWriteOperations = originalPostponeState;
                    QueueWriteLog();
                }
            }
        }

        /// <summary>
        /// Raises <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected virtual void OnProcessException(Exception ex)
        {
            if ((object)ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Removes all elements from the <see cref="OutageLog"/>.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            QueueWriteLog();
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="OutageLog"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero.-or-
        /// <paramref name="index"/> is equal to or greater than <see cref="Collection{T}.Count"/>.
        /// </exception>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            QueueWriteLog();
        }

        /// <summary>
        /// Moves the item at the specified index to a new location in the <see cref="OutageLog"/>.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the <see cref="Outage"/> to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the <see cref="Outage"/>.</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            QueueWriteLog();
        }

        /// <summary>
        /// Inserts an element into the <see cref="OutageLog"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="outage"/> should be inserted.</param>
        /// <param name="outage">The <see cref="Outage"/> to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero.-or-
        /// <paramref name="index"/> is greater than <see cref="Collection{T}.Count"/>.
        /// </exception>
        protected override void InsertItem(int index, Outage outage)
        {
            base.InsertItem(index, outage);
            m_condenseLogOperation.RunOnceAsync();
            QueueWriteLog();
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="outage">The new <see cref="Outage"/> for the element at the specified index.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero.-or-
        /// <paramref name="index"/> is greater than <see cref="Collection{T}.Count"/>.
        /// </exception>
        protected override void SetItem(int index, Outage outage)
        {
            base.SetItem(index, outage);
            m_condenseLogOperation.RunOnceAsync();
            QueueWriteLog();
        }

        // Queues a write log operation after any kind of change to the outage log.
        private void QueueWriteLog()
        {
            if (m_enabled && !m_postponeWriteOperations)
                m_writeLogOperation.RunOnceAsync();
        }

        #endregion
    }
}
