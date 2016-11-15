//******************************************************************************************************
//  SharedTimer.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/10/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using GSF.Diagnostics;

namespace GSF.Threading
{
    /// <summary>
    /// Functions similar to <see cref="System.Timers.Timer"/> except timer callbacks with the same <see cref="Interval"/>
    /// are pooled into a single timer and called with the same thread when they contain the same <see cref="SharedTimerScheduler"/>. 
    /// 
    /// Any long running callbacks that have a risk of long delays should not use <see cref="SharedTimer"/> as they 
    /// will effect the reliability of all of the other <see cref="SharedTimer"/>s in the <see cref="SharedTimerScheduler"/>.
    /// </summary>
    public sealed class SharedTimer : IDisposable
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when the interval elapses.
        /// </summary>
        public event EventHandler<EventArgs<DateTime>> Elapsed;

        /// <summary>
        /// Occurs when <see cref="Elapsed"/> has an exception
        /// </summary>
        public event EventHandler<EventArgs<Exception>> UnhandledExceptions;

        // Fields
        private int m_interval;
        private bool m_enabled;
        private bool m_autoReset;
        private bool m_disposed;
        private Action<DateTime> m_callback;
        private SharedTimerScheduler m_scheduler;
        private WeakAction<DateTime> m_registeredCallback;

        /// <summary>
        /// Since there won't be many shared timers, it will be better to not make this publisher a static instance. 
        /// This will provide the initialization stack so it will be easier to distinguish this instance of SharedTimer 
        /// from other instances.
        /// </summary>
        private readonly LogPublisher m_log;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedTimer"/>.
        /// </summary>
        /// <param name="scheduler">The scheduler to use</param>
        /// <param name="interval">The interval of the timer, default is 100</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal SharedTimer(SharedTimerScheduler scheduler, int interval = 100)
        {
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));

            if (scheduler.IsDisposed)
                throw new ArgumentException("Scheduler has been disposed", nameof(scheduler));

            if (interval <= 0)
                throw new ArgumentOutOfRangeException(nameof(interval));

            m_log = Logger.CreatePublisher(typeof(SharedTimerScheduler), MessageClass.Component);
            m_scheduler = scheduler;
            m_interval = interval;
            m_enabled = false;
            m_autoReset = true;
            m_callback = TimerCallback;
            m_scheduler = scheduler;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="SharedTimer" /> should raise the <see cref="Elapsed" /> 
        /// event each time the specified interval elapses or only after the first time it elapses.</summary>
        /// <returns>true if the <see cref="SharedTimer" /> should raise the <see cref="Elapsed" /> event each time the interval elapses; 
        /// false if it should raise the <see cref="Elapsed" /> event only once, after the first time the interval elapses. The default is true.</returns>
        public bool AutoReset
        {
            get
            {
                return m_autoReset;
            }
            set
            {
                if (m_autoReset != value)
                {
                    m_autoReset = value;

                    if (value && m_enabled)
                    {
                        m_registeredCallback?.Clear();
                        m_registeredCallback = m_scheduler.RegisterCallback(m_interval, m_callback);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="SharedTimer" /> should raise the <see cref="Elapsed" /> event.
        /// </summary>
        /// <returns>
        /// true if the <see cref="SharedTimer" /> should raise the <see cref="Elapsed" /> event; otherwise, false. The default is false.
        /// </returns>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (m_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                if (m_enabled != value)
                {
                    m_enabled = value;

                    if (!m_enabled)
                        m_registeredCallback?.Clear();
                    else
                        m_registeredCallback = m_scheduler.RegisterCallback(m_interval, m_callback);
                }
            }
        }

        /// <summary>
        /// Gets or sets the interval at which to raise the <see cref="Elapsed" /> event.
        /// </summary>
        /// <returns>
        /// The time, in milliseconds, between <see cref="Elapsed" /> events. 
        /// The value must be greater than zero, and less than or equal to <see cref="Int32.MaxValue" />. 
        /// The default is 100 milliseconds.
        /// </returns>
        public int Interval
        {
            get
            {
                return m_interval;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (value != m_interval)
                {
                    m_interval = value;

                    if (m_enabled)
                    {
                        m_registeredCallback?.Clear();
                        m_registeredCallback = m_scheduler.RegisterCallback(m_interval, m_callback);
                    }
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Close()
        {
            Enabled = false;
        }

        /// <summary>
        /// Stops the timer and prevents reuse of this class.
        /// </summary>
        public void Dispose()
        {
            Close();
            m_disposed = true;
        }

        /// <summary>
        /// Starts raising the <see cref="Elapsed" /> event by setting 
        /// <see cref="Enabled" /> to true.</summary>
        public void Start()
        {
            Enabled = true;
        }

        /// <summary>
        /// Stops raising the <see cref="Elapsed"/> 
        /// event by setting <see cref="Enabled" /> 
        /// to false.</summary>
        public void Stop()
        {
            Enabled = false;
        }

        /// <summary>
        /// Callback from <see cref="SharedTimerScheduler"/>.
        /// </summary>
        /// <param name="state">the time that the callback was signaled.</param>
        private void TimerCallback(DateTime state)
        {
            if (!m_enabled)
                return;

            if (!m_autoReset)
            {
                m_enabled = false;
                m_registeredCallback?.Clear();
            }

            try
            {
                Elapsed?.Invoke(this, new EventArgs<DateTime>(state));
            }
            catch (Exception ex)
            {
                try
                {
                    EventHandler<EventArgs<Exception>> unhandledExceptions = UnhandledExceptions;

                    if (unhandledExceptions == null)
                        m_log.Publish(MessageLevel.Info, "Swallowed exception", null, null, ex);
                    else
                        unhandledExceptions(this, new EventArgs<Exception>(ex));
                }
                catch (Exception ex2)
                {
                    m_log.Publish(MessageLevel.Warning, MessageFlags.BugReport, "Unhandled Exception callback threw an error", ex2.ToString(), null, ex2);
                }
            }
        }

        #endregion
    }
}