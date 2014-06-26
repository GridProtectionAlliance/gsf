//******************************************************************************************************
//  OutageLog.cs - Gbtc
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
using GSF.Threading;

namespace GSF.IO
{
    /// <summary>
    /// Represents an outage as a start time and an end time.
    /// </summary>
    public class Outage
    {
        #region [ Members ]

        // Fields
        private Ticks m_startTime;
        private Ticks m_endTime;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="Outage"/>.
        /// </summary>
        public Outage()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Outage"/> with the specified start and end time.
        /// </summary>
        /// <param name="startTime">Start time for outage.</param>
        /// <param name="endTime">End time for outage.</param>
        public Outage(Ticks startTime, Ticks endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets start time for <see cref="Outage"/>.
        /// </summary>
        public Ticks StartTime
        {
            get
            {
                return m_startTime;
            }
            set
            {
                if (m_endTime > 0 && value > m_endTime)
                    throw new ArgumentOutOfRangeException("value", "Outage start time is past end time");

                m_startTime = value;
            }
        }

        /// <summary>
        /// Gets or sets end time for <see cref="Outage"/>.
        /// </summary>
        public Ticks EndTime
        {
            get
            {
                return m_endTime;
            }
            set
            {
                if (m_startTime > value)
                    throw new ArgumentOutOfRangeException("value", "Outage start time is past end time");

                m_endTime = value;
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a persisted log of outages as a list of start and stop times.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class serializes a list of outages (e.g., a connection outage or data loss) where each outage
    /// consists of start and end time. The outages are persisted in a log file so that the log can be
    /// operated on even through host application restarts until the outages are processed.
    /// </para>
    /// <para>
    /// No members in the <see cref="OutageLog"/> are guaranteed to be thread safe. Make sure any calls are
    /// synchronized when simultaneously accessed from different threads.
    /// </para>
    /// </remarks>
    public class OutageLog : Collection<Outage>, ISupportLifecycle
    {
        #region [ Members ]

        // Constants
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

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
        private readonly ShortSynchronizedOperation m_flushLog;
        private string m_fileName;
        private bool m_loadInProgress;
        private bool m_enabled;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="OutageLog"/>.
        /// </summary>
        public OutageLog()
        {
            m_flushLog = new ShortSynchronizedOperation(WriteLog);
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
                    throw new ArgumentNullException("value");

                m_fileName = FilePath.GetAbsolutePath(value);
            }
        }

        /// <summary>        
        /// Gets or sets a boolean value that indicates whether the run-time log is enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
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
                    //if (disposing)
                    //{
                    //}

                    WriteLog();
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
            ReadLog();
        }

        /// <summary>
        /// Adds an outage to the <see cref="OutageLog"/>.
        /// </summary>
        /// <param name="startTime">Start time of outage.</param>
        /// <param name="endTime">End time of outage.</param>
        public void Add(Ticks startTime, Ticks endTime)
        {
            Add(new Outage(startTime, endTime));
        }

        /// <summary>
        /// Reads the outage log.
        /// </summary>
        public void ReadLog()
        {
            if (string.IsNullOrWhiteSpace(m_fileName))
                throw new NullReferenceException("No outage log file name was specified");

            try
            {
                List<Outage> outages = new List<Outage>();

                if (File.Exists(m_fileName))
                {
                    using (StreamReader reader = File.OpenText(m_fileName))
                    {
                        string line;
                        string[] times;
                        DateTime startTime, endTime;

                        while ((object)(line = reader.ReadLine()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line))
                                continue;

                            times = line.Split(';');

                            if (times.Length == 2 &&
                                DateTime.TryParseExact(times[0].Trim(), DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out startTime) &&
                                DateTime.TryParseExact(times[1].Trim(), DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out endTime))
                            {
                                try
                                {
                                    outages.Add(new Outage(startTime, endTime));
                                }
                                catch (Exception ex)
                                {
                                    OnProcessException(new InvalidOperationException(string.Format("Failed to create outage from \"{0}\": {1}", line, ex.Message), ex));
                                }
                            }
                            else
                            {
                                OnProcessException(new FormatException("Invalid date-time format. Failed to parse start and end times from: " + line));
                            }
                        }
                    }
                }

                lock (this)
                {
                    try
                    {
                        // Don't kick off flush operations during load
                        m_loadInProgress = true;

                        Clear();

                        foreach (Outage outage in outages)
                        {
                            Add(outage);
                        }
                    }
                    finally
                    {
                        m_loadInProgress = false;
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Failed to read outage log: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// Writes the outage log - times are in a human readable format.
        /// </summary>
        public void WriteLog()
        {
            if (string.IsNullOrWhiteSpace(m_fileName))
                throw new NullReferenceException("No outage log file name was specified");

            try
            {
                Outage[] outages;

                lock (this)
                {
                    outages = this.ToArray();
                }

                using (StreamWriter writer = File.CreateText(m_fileName))
                {
                    foreach (Outage outage in outages)
                    {
                        writer.WriteLine("{0};{1}",
                            outage.StartTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
                            outage.EndTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Failed to write outage log: " + ex.Message, ex));
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
            FlushLog();
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
            FlushLog();
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
            FlushLog();
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
            FlushLog();
        }

        // Kick off a flush operation after any kind of change to the outage log
        private void FlushLog()
        {
            if (m_enabled && !m_loadInProgress)
                m_flushLog.RunOnceAsync();
        }

        #endregion
    }
}
