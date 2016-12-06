//******************************************************************************************************
//  PrecisionInputTimer.cs - Gbtc
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
//  07/25/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Threading;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Precision input timer.
    /// </summary>
    /// <remarks>
    /// This class is used to create highly accurate simulated data inputs aligned to the local clock.<br/>
    /// One static instance of this internal class is created per encountered frame rate.
    /// </remarks>
    public sealed class PrecisionInputTimer : IDisposable
    {
        #region [ Members ]

        // Fields
        private PrecisionTimer m_timer;
        private bool m_useWaitHandleA;
        private readonly object m_timerTickLock;
        private ManualResetEventSlim m_frameWaitHandleA;
        private ManualResetEventSlim m_frameWaitHandleB;
        private readonly int m_framesPerSecond;
        private readonly int m_frameWindowSize;
        private readonly int[] m_frameMilliseconds;
        private int m_lastFrameIndex;
        private long m_lastFrameTime;
        private long m_missedPublicationWindows;
        private long m_lastMissedWindowTime;
        private long m_resynchronizations;
        private int m_referenceCount;
        private Action<Exception> m_exceptionHandler;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Create a new <see cref="PrecisionInputTimer"/> class.
        /// </summary>
        /// <param name="framesPerSecond">Desired frame rate for <see cref="PrecisionTimer"/>.</param>
        internal PrecisionInputTimer(int framesPerSecond)
        {
            // Create synchronization objects
            m_timerTickLock = new object();
            m_frameWaitHandleA = new ManualResetEventSlim(false);
            m_frameWaitHandleB = new ManualResetEventSlim(false);
            m_useWaitHandleA = true;
            m_framesPerSecond = framesPerSecond;

            // Create a new precision timer for this timer state
            m_timer = new PrecisionTimer();
            m_timer.Resolution = 1;
            m_timer.Period = 1;
            m_timer.AutoReset = true;

            // Attach handler for timer ticks
            m_timer.Tick += m_timer_Tick;

            m_frameWindowSize = (int)Math.Round(1000.0D / framesPerSecond) * 2;
            m_frameMilliseconds = Ticks.MillisecondDistribution(framesPerSecond);

            // Start high resolution timer on a separate thread so the start
            // time can synchronized to the top of the millisecond
            ThreadPool.QueueUserWorkItem(SynchronizeInputTimer);
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="PrecisionInputTimer"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~PrecisionInputTimer()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets frames per second for this <see cref="PrecisionInputTimer"/>.
        /// </summary>
        public int FramesPerSecond
        {
            get
            {
                return m_framesPerSecond;
            }
        }

        /// <summary>
        /// Gets array of frame millisecond times for this <see cref="PrecisionInputTimer"/>.
        /// </summary>
        public int[] FrameMilliseconds
        {
            get
            {
                return m_frameMilliseconds;
            }
        }

        /// <summary>
        /// Gets reference count for this <see cref="PrecisionInputTimer"/>.
        /// </summary>
        public int ReferenceCount
        {
            get
            {
                return m_referenceCount;
            }
        }

        /// <summary>
        /// Gets number of resynchronizations that have occurred for this <see cref="PrecisionInputTimer"/>.
        /// </summary>
        public long Resynchronizations
        {
            get
            {
                return m_resynchronizations;
            }
        }

        /// <summary>
        /// Gets time of last frame, in ticks.
        /// </summary>
        public long LastFrameTime
        {
            get
            {
                return m_lastFrameTime;
            }
        }

        /// <summary>
        /// Gets a reference to the frame wait handle.
        /// </summary>
        public ManualResetEventSlim FrameWaitHandle
        {
            get
            {
                if (m_useWaitHandleA)
                    return m_frameWaitHandleA;

                return m_frameWaitHandleB;
            }
        }

        /// <summary>
        /// Gets or sets function used to handle exceptions.
        /// </summary>
        public Action<Exception> ExceptionHandler
        {
            get
            {
                return m_exceptionHandler;
            }
            set
            {
                m_exceptionHandler = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="PrecisionInputTimer"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PrecisionInputTimer"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_timer != null)
                        {
                            m_timer.Tick -= m_timer_Tick;
                            m_timer.Dispose();
                        }
                        m_timer = null;

                        if ((object)m_frameWaitHandleA != null)
                        {
                            m_frameWaitHandleA.Set();
                            m_frameWaitHandleA.Dispose();
                        }
                        m_frameWaitHandleA = null;

                        if ((object)m_frameWaitHandleB != null)
                        {
                            m_frameWaitHandleB.Set();
                            m_frameWaitHandleB.Dispose();
                        }
                        m_frameWaitHandleB = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Adds a reference to this <see cref="PrecisionInputTimer"/>.
        /// </summary>
        public void AddReference()
        {
            m_referenceCount++;
        }

        /// <summary>
        /// Removes a reference to this <see cref="PrecisionInputTimer"/>.
        /// </summary>
        public void RemoveReference()
        {
            m_referenceCount--;
        }

        // This timer function is called every millisecond so that frames can be published at the exact desired time 
        private void m_timer_Tick(object sender, EventArgs e)
        {
            // Slower systems or systems under stress may have trouble keeping up with a 1-ms timer, so
            // we only process this code if it's not already processing...
            if (Monitor.TryEnter(m_timerTickLock, 2))
            {
                try
                {
                    DateTime now = DateTime.UtcNow;
                    int frameMilliseconds, milliseconds = now.Millisecond;
                    long ticks = now.Ticks;
                    bool releaseTimer = false, resync = false;

                    // Make sure current time is reasonably close to current frame index
                    if (Math.Abs(milliseconds - m_frameMilliseconds[m_lastFrameIndex]) > m_frameWindowSize)
                        m_lastFrameIndex = 0;

                    // See if it is time to publish
                    for (int frameIndex = m_lastFrameIndex; frameIndex < m_frameMilliseconds.Length; frameIndex++)
                    {
                        frameMilliseconds = m_frameMilliseconds[frameIndex];

                        if (frameMilliseconds == milliseconds)
                        {
                            // See if system skipped a publication window
                            if (m_lastFrameIndex != frameIndex)
                            {
                                // We monitor for missed windows in quick succession (within 1.5 seconds)
                                if (ticks - m_lastMissedWindowTime > 15000000L)
                                {
                                    // Threshold has passed since last missed window, so we reset counters
                                    m_lastMissedWindowTime = ticks;
                                    m_missedPublicationWindows = 0;
                                }

                                m_missedPublicationWindows++;

                                // If the system is starting to skip publications it could need resynchronization,
                                // so in this case we restart the high-resolution timer to get the timer started
                                // closer to the top of the millisecond
                                resync = m_missedPublicationWindows > 4;
                            }

                            // Prepare index for next check, time moving forward
                            m_lastFrameIndex = frameIndex + 1;

                            if (m_lastFrameIndex >= m_frameMilliseconds.Length)
                                m_lastFrameIndex = 0;

                            if (resync)
                            {
                                if ((object)m_timer != null)
                                {
                                    m_timer.Stop();
                                    ThreadPool.QueueUserWorkItem(SynchronizeInputTimer);
                                    m_resynchronizations++;
                                }
                            }

                            releaseTimer = true;
                            break;
                        }

                        // If time has yet to pass, wait till the next tick
                        if (frameMilliseconds > milliseconds)
                            break;
                    }

                    if (releaseTimer)
                    {
                        // Baseline time-stamp to the top of the millisecond for frame publication
                        m_lastFrameTime = ticks - ticks % Ticks.PerMillisecond;

                        // Pulse all waiting threads toggling between ready handles
                        if (m_useWaitHandleA)
                        {
                            m_frameWaitHandleB.Reset();
                            m_useWaitHandleA = false;
                            m_frameWaitHandleA.Set();
                        }
                        else
                        {
                            m_frameWaitHandleA.Reset();
                            m_useWaitHandleA = true;
                            m_frameWaitHandleB.Set();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if ((object)m_exceptionHandler != null)
                        m_exceptionHandler(new InvalidOperationException("Exception thrown by precision input timer: " + ex.Message, ex));
                }
                finally
                {
                    Monitor.Exit(m_timerTickLock);
                }
            }
        }

        private void SynchronizeInputTimer(object state)
        {
            // Start timer at as close to the top of the millisecond as possible 
            bool repeat = true;
            long last = 0, next;

            while (repeat)
            {
                next = DateTime.UtcNow.Ticks % Ticks.PerMillisecond % 1000;
                repeat = next > last;
                last = next;
            }

            m_lastMissedWindowTime = 0;
            m_missedPublicationWindows = 0;

            if ((object)m_timer != null)
                m_timer.Start();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Dictionary<int, PrecisionInputTimer> s_inputTimers = new Dictionary<int, PrecisionInputTimer>();

        // Static Methods

        /// <summary>
        /// Attach to a <see cref="PrecisionInputTimer"/> for the specified <paramref name="framesPerSecond"/>.
        /// </summary>
        /// <param name="framesPerSecond">Desired frames per second for the input timer.</param>
        /// <param name="exceptionHandler">Optional delegate to handle exception reporting from the timer.</param>        
        /// /// <returns>A <see cref="PrecisionInputTimer"/> that can be used for the specified <paramref name="framesPerSecond"/>.</returns>
        public static PrecisionInputTimer Attach(int framesPerSecond, Action<Exception> exceptionHandler = null)
        {
            PrecisionInputTimer timer;

            lock (s_inputTimers)
            {
                // Get static input timer for given frames per second creating it if needed
                if (!s_inputTimers.TryGetValue(framesPerSecond, out timer))
                {
                    try
                    {
                        // Create a new precision input timer
                        timer = new PrecisionInputTimer(framesPerSecond);
                        timer.ExceptionHandler = exceptionHandler;

                        // Add timer state for given rate to static collection
                        s_inputTimers.Add(framesPerSecond, timer);
                    }
                    catch (Exception ex)
                    {
                        // Process exception for logging
                        if ((object)exceptionHandler != null)
                            exceptionHandler(new InvalidOperationException("Failed to create precision input timer due to exception: " + ex.Message, ex));
                        else
                            throw;
                    }
                }

                // Increment reference count for input timer at given frame rate
                if ((object)timer != null)
                    timer.AddReference();
            }

            return timer;
        }

        /// <summary>
        /// Detach from the <see cref="PrecisionInputTimer"/>.
        /// </summary>
        /// <param name="timer">Timer instance to detach from.</param>
        /// <remarks>
        /// Timer reference will be set to <c>null</c> after detach.
        /// </remarks>
        public static void Detach(ref PrecisionInputTimer timer)
        {
            if ((object)timer != null)
            {
                lock (s_inputTimers)
                {
                    // Verify static frame rate timer for given frames per second exists
                    if (s_inputTimers.ContainsKey(timer.FramesPerSecond))
                    {
                        // Decrement reference count
                        timer.RemoveReference();

                        // If timer is no longer being referenced we stop it and remove it from static collection
                        if (timer.ReferenceCount == 0)
                        {
                            timer.Dispose();
                            s_inputTimers.Remove(timer.FramesPerSecond);
                        }
                    }
                }
            }

            timer = null;
        }

        #endregion
    }
}
