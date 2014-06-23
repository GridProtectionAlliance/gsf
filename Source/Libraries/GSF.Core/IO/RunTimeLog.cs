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
        private const string Format = "yyyy-MM-dd HH:mm:ss.fff";

        // Events

        /// <summary>
        /// Event is raised when there is an exception encountered while processing run-time log.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        /// <summary>
        /// Raised after the source object has been properly disposed.
        /// </summary>
        public event EventHandler Disposed;

        // Fields
        private readonly string m_logName;
        private readonly Timer m_flushTimer;
        private readonly object m_readerWriterLock;
        private Ticks m_start;
        private Ticks m_stop;
        private Ticks m_running;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new run-time log.
        /// </summary>
        /// <param name="logName">File name of run-time log.</param>
        public RunTimeLog(string logName)
        {
            if (string.IsNullOrWhiteSpace(logName))
                throw new ArgumentNullException("logName");

            m_logName = FilePath.GetAbsolutePath(logName);

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
        /// Gets the file name for the run-time log.
        /// </summary>
        public string LogName
        {
            get
            {
                return m_logName;
            }
        }

        /// <summary>
        /// Gets last known start-time.
        /// </summary>
        public Ticks Start
        {
            get
            {
                return m_start;
            }
        }

        /// <summary>
        /// Gets last known stop-time.
        /// </summary>
        public Ticks Stop
        {
            get
            {
                return m_stop;
            }
        }

        /// <summary>
        /// Gets last known running-time (10-second resolution).
        /// </summary>
        public Ticks Running
        {
            get
            {
                return m_running;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the object is enabled.
        /// </summary>
        /// <remarks>
        /// This property controls the automatic write log timer.
        /// </remarks>
        public bool Enabled
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
                        m_stop = DateTime.UtcNow.Ticks;
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
        /// Initialization performs initial run-time log read, establishes new start time and enables automatic write log timer.
        /// </remarks>
        public void Initialize()
        {
            ReadLog();
            m_start = DateTime.UtcNow.Ticks;
            m_flushTimer.Start();
        }

        /// <summary>
        /// Reads the run-time log.
        /// </summary>
        public void ReadLog()
        {
            try
            {
                lock (m_readerWriterLock)
                {
                    if (File.Exists(m_logName))
                    {
                        using (StreamReader reader = File.OpenText(m_logName))
                        {
                            Dictionary<string, string> settings = reader.ReadToEnd().Replace(Environment.NewLine, ";").ParseKeyValuePairs();
                            string setting;
                            DateTime time;

                            if (settings.TryGetValue("start", out setting) && DateTime.TryParseExact(setting, Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out time))
                                m_start = time.Ticks;
                            else
                                m_start = DateTime.UtcNow.Ticks;

                            if (settings.TryGetValue("stop", out setting) && DateTime.TryParseExact(setting, Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out time))
                                m_stop = time.Ticks;
                            else
                                m_stop = DateTime.UtcNow.Ticks;

                            if (settings.TryGetValue("running", out setting) && DateTime.TryParseExact(setting, Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out time))
                                m_running = time.Ticks;
                            else
                                m_running = DateTime.UtcNow.Ticks;
                        }
                    }
                    else
                    {
                        m_start = m_stop = m_running = DateTime.UtcNow.Ticks;
                    }
                }
            }
            catch (Exception ex)
            {
                m_start = m_stop = m_running = DateTime.UtcNow.Ticks;
                OnProcessException(new InvalidOperationException("Failed to read run-time log: " + ex.Message, ex));
            }

            // Validate stop time - the point of keeping a running time is so that if host application fails to
            // flush log during shutdown - the last known running time can become the assumed stop time.
            if (m_running > m_stop)
                m_stop = m_running;
        }

        /// <summary>
        /// Writes the run-time log.
        /// </summary>
        public void WriteLog()
        {
            if (Monitor.TryEnter(m_readerWriterLock))
            {
                try
                {
                    using (StreamWriter writer = File.CreateText(m_logName))
                    {
                        writer.WriteLine("start={0}", m_start.ToString(Format, CultureInfo.InvariantCulture));
                        writer.WriteLine("stop={0}", m_stop.ToString(Format, CultureInfo.InvariantCulture));
                        writer.WriteLine("running={0}", m_running.ToString(Format, CultureInfo.InvariantCulture));
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
            m_running = DateTime.UtcNow.Ticks;
            WriteLog();
        }

        #endregion
    }
}
