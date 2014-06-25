//******************************************************************************************************
//  RunTimeLog.cs - Gbtc
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
//  06/23/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace GSF.IO
{
    /// <summary>
    /// Represents a run-time log that tracks last start, stop and running times.
    /// </summary>
    public class RunTimeLog : ISupportLifecycle
    {
        #region [ Members ]

        // Constants
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        // Events

        /// <summary>
        /// Event is raised when there is an exception encountered while processing run-time log.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        /// <summary>
        /// Raised after the run-time log has been properly disposed.
        /// </summary>
        public event EventHandler Disposed;

        // Fields
        private readonly Timer m_flushTimer;
        private readonly object m_readerWriterLock;
        private string m_fileName;
        private Ticks m_startTime;
        private Ticks m_stopTime;
        private Ticks m_runningTime;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new run-time log.
        /// </summary>
        public RunTimeLog()
        {
            m_flushTimer = new Timer
            {
                AutoReset = true,
                Interval = 10000.0D
            };

            m_flushTimer.Elapsed += m_flushTimer_Elapsed;
            m_readerWriterLock = new object();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="RunTimeLog"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~RunTimeLog()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the file name for the run-time log; file name can be set with a relative path.
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
        /// Gets last known start-time.
        /// </summary>
        public Ticks StartTime
        {
            get
            {
                return m_startTime;
            }
            protected set
            {
                m_startTime = value;
            }
        }

        /// <summary>
        /// Gets last known stop-time.
        /// </summary>
        public Ticks StopTime
        {
            get
            {
                return m_stopTime;
            }
            protected set
            {
                m_stopTime = value;
            }
        }

        /// <summary>
        /// Gets last known running-time (10-second resolution).
        /// </summary>
        public Ticks RunningTime
        {
            get
            {
                return m_runningTime;
            }
            protected set
            {
                m_runningTime = value;
            }
        }

        /// <summary>        
        /// Gets or sets a boolean value that indicates whether the run-time log is enabled.
        /// </summary>
        /// <remarks>
        /// This property controls the automatic write log timer.
        /// </remarks>
        public virtual bool Enabled
        {
            get
            {
                return m_flushTimer.Enabled;
            }
            set
            {
                m_flushTimer.Enabled = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="RunTimeLog"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="RunTimeLog"/> object and optionally releases the managed resources.
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
                        if ((object)m_flushTimer != null)
                        {
                            m_flushTimer.Stop();
                            m_flushTimer.Elapsed -= m_flushTimer_Elapsed;
                            m_flushTimer.Dispose();
                        }
                    }

                    lock (m_readerWriterLock)
                    {
                        m_stopTime = DateTime.UtcNow.Ticks;
                        WriteLog();
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
        /// Initialized the run-time log.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initialization performs initial run-time log read, establishes new start time and enables automatic write log timer.
        /// </para>
        /// <para>
        /// Last logged stop time will be validated against last logged running time. If the last logged running time is later
        /// than the last logged stop time, the stop time will be set to the running time with the assumption that the log file
        /// was not properly shut down (e.g., due to abnormal host termination).
        /// </para>
        /// </remarks>
        public virtual void Initialize()
        {
            ReadLog();
            m_startTime = DateTime.UtcNow.Ticks;
            m_flushTimer.Start();

            // Validate stop time - the point of keeping a running time is so that if host application fails to
            // flush log during shutdown - the last known running time can become the assumed stop time.
            if (m_runningTime > m_stopTime)
                m_stopTime = m_runningTime;
        }

        /// <summary>
        /// Reads the run-time log.
        /// </summary>
        public void ReadLog()
        {
            if (string.IsNullOrWhiteSpace(m_fileName))
                throw new NullReferenceException("No run-time log file name was specified");

            try
            {
                lock (m_readerWriterLock)
                {
                    if (File.Exists(m_fileName))
                    {
                        using (StreamReader reader = File.OpenText(m_fileName))
                        {
                            Dictionary<string, string> settings = reader.ReadToEnd().Replace(Environment.NewLine, ";").ParseKeyValuePairs();
                            string setting;
                            DateTime time;

                            if (settings.TryGetValue("startTime", out setting) && DateTime.TryParseExact(setting, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out time))
                                m_startTime = time.Ticks;
                            else
                                m_startTime = DateTime.UtcNow.Ticks;

                            if (settings.TryGetValue("stopTime", out setting) && DateTime.TryParseExact(setting, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out time))
                                m_stopTime = time.Ticks;
                            else
                                m_stopTime = DateTime.UtcNow.Ticks;

                            if (settings.TryGetValue("runningTime", out setting) && DateTime.TryParseExact(setting, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out time))
                                m_runningTime = time.Ticks;
                            else
                                m_runningTime = DateTime.UtcNow.Ticks;
                        }
                    }
                    else
                    {
                        m_startTime = m_stopTime = m_runningTime = DateTime.UtcNow.Ticks;
                    }
                }
            }
            catch (Exception ex)
            {
                m_startTime = m_stopTime = m_runningTime = DateTime.UtcNow.Ticks;
                OnProcessException(new InvalidOperationException("Failed to read run-time log: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// Writes the run-time log.
        /// </summary>
        public void WriteLog()
        {
            if (string.IsNullOrWhiteSpace(m_fileName))
                throw new NullReferenceException("No run-time log file name was specified");

            if (Monitor.TryEnter(m_readerWriterLock))
            {
                try
                {
                    using (StreamWriter writer = File.CreateText(m_fileName))
                    {
                        writer.WriteLine("startTime={0}", m_startTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
                        writer.WriteLine("stopTime={0}", m_stopTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
                        writer.WriteLine("runningTime={0}", m_runningTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException("Failed to write run-time log: " + ex.Message, ex));
                }
                finally
                {
                    Monitor.Exit(m_readerWriterLock);
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

        private void m_flushTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_runningTime = DateTime.UtcNow.Ticks;
            WriteLog();
        }

        #endregion
    }
}
