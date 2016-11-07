//******************************************************************************************************
//  PrecisionTimer.cs - Gbtc
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
//  11/22/2003 - Leslie Sanford
//       Original version of source code for Multimedia.Timer class.
//  04/10/2008 - James Brock
//       Original version of source code for DateTimePrecise class.
//  08/21/2008 - J. Ritchie Carroll
//       Integrated, merged and adapted for general use as PrecisionTimer.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/02/2009 - J. Ritchie Carroll
//       Added SetMinimumTimerResolution and ClearMinimumTimerResolution to control
//       system level minimum timer resolutions.
//  09/22/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

/**********************************************************************************\
   
   PrecisionTimer class
   Copyright © 2009 - J. Ritchie Carroll for combined work
   All rights reserved.
  
   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
       
      * Redistributions in binary form must reproduce the above
        copyright notice, this list of conditions and the following
        disclaimer in the documentation and/or other materials provided
        with the distribution.
  
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY
   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
   PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
   OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
   OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

   ------------------------------------------------------------------------------

   Multimedia.Timer class:
   Copyright (c) 2006 Leslie Sanford

   * Permission is hereby granted, free of charge, to any person obtaining a copy
   * of this software and associated documentation files (the "Software"), to
   * deal in the Software without restriction, including without limitation the
   * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
   * sell copies of the Software, and to permit persons to whom the Software is
   * furnished to do so, subject to the following conditions:
   *
   * The above copyright notice and this permission notice shall be included in
   * all copies or substantial portions of the Software.
   *
   * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
   * THE SOFTWARE.
    
   * Leslie Sanford
   * Email: jabberdabber@hotmail.com  

   ------------------------------------------------------------------------------

   DateTimePrecise class:
   Copyright James Brock
  
   The Code Project Open License (CPOL):
        http://www.codeproject.com/info/cpol10.aspx

\**********************************************************************************/

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Timers;

namespace GSF
{
    /// <summary>
    /// Represents information about the system's multimedia timer capabilities.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TimerCapabilities
    {
        /// <summary>Minimum supported period in milliseconds.</summary>
        public int PeriodMinimum;

        /// <summary>Maximum supported period in milliseconds.</summary>
        public int PeriodMaximum;
    }

    /// <summary>
    /// Represents an exception that is thrown when a <see cref="PrecisionTimer"/> fails to start.
    /// </summary>
    [Serializable]
    public class TimerStartException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimerStartException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public TimerStartException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Represents a high-resolution timer and timestamp class.
    /// </summary>
    /// <remarks>
    /// For Windows deployments, implementation is based on the Windows multimedia timer. For other
    /// deployments, e.g., POSIX, implementation uses a basic timer for compatibility - this should
    /// have ~10ms of resolution when used on standard Linux systems.
    /// </remarks>
    // Future deployments may want to consider an implementation that uses OS specific timers:
    //     For Windows use of multimedia style implementation below should be sufficient.
    //     For Linux see http://www.ittc.ku.edu/utime/ as an example.
    public class PrecisionTimer : IDisposable
    {
        #region [ Members ]

        // Defines constants for the multimedia Timer's event types.
        private enum TimerMode
        {
            OneShot, // Timer event occurs once.
            Periodic // Timer event occurs periodically.
        }

        // Represents the method that is called by Windows when a timer event occurs.
        private delegate void TimerProc(int id, int msg, int user, int param1, int param2);

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> has started.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> has stopped.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Occurs when the <see cref="PrecisionTimer"/> period has elapsed.
        /// </summary>
        public event EventHandler Tick;

        // Fields
        private readonly Timer m_timer;         // Basic timer implementation for Mono deployments
        private int m_timerID;                  // Timer identifier.
        private readonly TimerProc m_timeProc;  // Called by Windows when a timer periodic event occurs.
        private TimerMode m_mode;               // Timer mode.
        private int m_period;                   // Period between timer events in milliseconds.
        private int m_resolution;               // Timer resolution in milliseconds.
        private bool m_running;                 // Indicates whether or not the timer is running.
        private bool m_disposed;                // Indicates whether or not the timer has been disposed.
        private EventArgs m_eventArgs;          // Private user event args to pass into Ticks call

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="PrecisionTimer"/> class.
        /// </summary>
        public PrecisionTimer()
        {
            // Initialize timer with default values.
            m_mode = TimerMode.Periodic;
            m_running = false;

            if (Common.IsPosixEnvironment)
            {
                m_timer = new Timer();
                m_timer.Elapsed += m_timer_Elapsed;
                m_period = 10;
                m_resolution = 10;
            }
            else
            {
                m_timeProc = TimerEventCallback;
                m_period = Capabilities.PeriodMinimum;
                m_resolution = 1;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="PrecisionTimer"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~PrecisionTimer()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the time between <see cref="Tick"/> events, in milliseconds.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        public int Period
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                return m_period;
            }
            set
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                if (value < Capabilities.PeriodMinimum || value > Capabilities.PeriodMaximum)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Multimedia Timer period out of range");

                m_period = value;

                if (IsRunning && m_mode == TimerMode.Periodic)
                {
                    Stop();
                    Start(m_eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PrecisionTimer"/> resolution, in milliseconds.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The resolution is in milliseconds. The resolution increases  with smaller values;
        /// a resolution of 0 indicates periodic events should occur with the greatest possible
        /// accuracy. To reduce system  overhead, however, you should use the maximum value
        /// appropriate for your application.
        /// </para>
        /// <para>
        /// This property is currently ignored under Mono deployments.
        /// </para>
        /// </remarks>
        public int Resolution
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                return m_resolution;
            }
            set
            {
                if (m_disposed)
                    throw new ObjectDisposedException("PrecisionTimer");

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Multimedia timer resolution out of range");

                m_resolution = value;

                if (IsRunning && m_mode == TimerMode.Periodic)
                {
                    Stop();
                    Start(m_eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="PrecisionTimer"/> should raise the
        /// <see cref="Tick"/> event each time the specified period elapses or only after the first
        /// time it elapses.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>
        /// <c>true</c>true if the <see cref="PrecisionTimer"/> should raise the <see cref="Ticks"/>
        /// event each time the interval elapses; <c>false</c> if it should raise the event only once
        /// after the first time the interval elapses. The default is <c>true</c>.
        /// </returns>
        public bool AutoReset
        {
            get
            {
                if (m_disposed)
                    throw (new ObjectDisposedException("PrecisionTimer"));

                return (m_mode == TimerMode.Periodic);
            }
            set
            {
                if (m_disposed)
                    throw (new ObjectDisposedException("PrecisionTimer"));

                m_mode = (value ? TimerMode.Periodic : TimerMode.OneShot);

                if (IsRunning && m_mode == TimerMode.Periodic)
                {
                    Stop();
                    Start(m_eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="PrecisionTimer"/> is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return m_running;
            }
        }

        /// <summary>
        /// Gets <see cref="System.EventArgs"/> specified in <see cref="Start(System.EventArgs)"/> used to pass into <see cref="Tick"/> event.
        /// </summary>
        public EventArgs EventArgs
        {
            get
            {
                return m_eventArgs;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="PrecisionTimer"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PrecisionTimer"/> object and optionally releases the managed resources.
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
                        if (IsRunning)
                            Stop();

                        if ((object)m_timer != null)
                            m_timer.Dispose();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Starts the <see cref="PrecisionTimer"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// The timer has already been disposed.
        /// </exception>
        /// <exception cref="TimerStartException">
        /// The timer failed to start.
        /// </exception>
        public void Start()
        {
            Start(EventArgs.Empty);
        }

        /// <summary>
        /// Starts the <see cref="PrecisionTimer"/> with the specified <see cref="EventArgs"/>.
        /// </summary>
        /// <param name="userArgs">User defined event arguments to pass into raised <see cref="Ticks"/> event.</param>
        /// <exception cref="ObjectDisposedException">
        /// The timer has already been disposed.
        /// </exception>
        /// <exception cref="TimerStartException">
        /// The timer failed to start.
        /// </exception>
        public void Start(EventArgs userArgs)
        {
            if (m_disposed)
                throw new ObjectDisposedException("PrecisionTimer");

            if (m_running)
                return;

            // Cache user event args to pass into Ticks parameter
            m_eventArgs = userArgs;

            // Create and start timer.
            if (Common.IsPosixEnvironment)
            {
                try
                {
                    m_timer.Interval = m_period;
                    m_timer.AutoReset = (m_mode == TimerMode.Periodic);
                    m_timer.Start();

                    m_running = true;

                    if ((object)Started != null)
                        Started(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    throw new TimerStartException("Unable to start precision timer: " + ex.Message);
                }
            }
            else
            {
                m_timerID = timeSetEvent(m_period, m_resolution, m_timeProc, IntPtr.Zero, m_mode);

                // If the timer was created successfully.
                if (m_timerID != 0)
                {
                    m_running = true;

                    if ((object)Started != null)
                        Started(this, EventArgs.Empty);
                }
                else
                {
                    throw new TimerStartException("Unable to start precision timer");
                }
            }
        }

        /// <summary>
        /// Stops <see cref="PrecisionTimer"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        public void Stop()
        {
            if (m_disposed)
                throw new ObjectDisposedException("PrecisionTimer");

            if (!m_running)
                return;

            // Stop and destroy timer.
            if (Common.IsPosixEnvironment)
            {
                m_timer.Stop();
            }
            else
            {
                timeKillEvent(m_timerID);
                m_timerID = 0;
            }

            m_running = false;

            if ((object)Stopped != null)
                Stopped(this, EventArgs.Empty);
        }

        private void m_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if ((object)Tick != null)
                Tick(this, m_eventArgs);
        }

        // Callback method called by the Win32 multimedia timer when a timer event occurs.
        private void TimerEventCallback(int id, int msg, int user, int param1, int param2)
        {
            if ((object)Tick != null)
                Tick(this, m_eventArgs);

            if (m_mode == TimerMode.OneShot)
                Stop();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly TimerCapabilities s_capabilities;    // Multimedia timer capabilities.

        // Static Constructor
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        static PrecisionTimer()
        {
            if (Common.IsPosixEnvironment)
            {
                s_capabilities.PeriodMinimum = 1;
                s_capabilities.PeriodMaximum = int.MaxValue;
            }
            else
            {
                // Get multimedia timer capabilities
                timeGetDevCaps(ref s_capabilities, Marshal.SizeOf(s_capabilities));
            }
        }

        // Static Properties

        /// <summary>
        /// Gets the system multimedia timer capabilities.
        /// </summary>
        public static TimerCapabilities Capabilities
        {
            get
            {
                return s_capabilities;
            }
        }

        // Static Methods

        /// <summary>
        /// Requests a minimum resolution for periodic timers such as the <see cref="PrecisionTimer"/>.
        /// </summary>
        /// <param name="period">
        /// Minimum timer resolution, in milliseconds, for the application. A lower value specifies a higher (more accurate) resolution.
        /// </param>
        /// <remarks>
        /// <para>
        /// Call this function immediately before using the <see cref="PrecisionTimer"/> and call <see cref="ClearMinimumTimerResolution"/>
        /// immediately after you are finished using the PrecisionTimer. You must match each call to <see cref="SetMinimumTimerResolution"/>
        /// with a call to ClearMinimumTimerResolution specifying the same minimum resolution <paramref name="period"/> in both calls.
        /// An application can make multiple ClearMinimumTimerResolution calls as long as each call is matched with a call to ClearMinimumTimerResolution.
        /// This function affects a global Windows setting. Windows uses the lowest value (that is, highest resolution) requested by any process.
        /// Setting a higher resolution can improve the accuracy of time-out intervals in wait functions. However, it can also reduce overall system
        /// performance, because the thread scheduler switches tasks more often. High resolutions can also prevent the CPU power management system from
        /// entering power-saving modes. See timeBeginPeriod Windows API for more information.
        /// </para>
        /// <para>
        /// This method is currently ignored under Mono deployments.
        /// </para>
        /// </remarks>
        public static void SetMinimumTimerResolution(int period)
        {
            if (!Common.IsPosixEnvironment)
            {
                if (timeBeginPeriod(period) != 0)
                    throw new InvalidOperationException("Specified period resolution is out of range and is not supported.");
            }
        }

        /// <summary>
        /// Clears a previously set minimum timer resolution established using <see cref="SetMinimumTimerResolution"/>.
        /// </summary>
        /// <param name="period">
        /// Minimum timer resolution specified in the previous call to the <see cref="SetMinimumTimerResolution"/> function.
        /// </param>
        /// <remarks>
        /// <para>
        /// Call this function immediately after you are finished using the <see cref="PrecisionTimer"/>. You must match each call to
        /// <see cref="SetMinimumTimerResolution"/> with a call to <see cref="ClearMinimumTimerResolution"/>, specifying the same minimum
        /// resolution <paramref name="period"/> in both calls. An application can make multiple SetMinimumTimerResolution calls as long
        /// as each call is matched with a call to ClearMinimumTimerResolution.
        /// </para>
        /// <para>
        /// This method is currently ignored under Mono deployments.
        /// </para>
        /// </remarks>
        public static void ClearMinimumTimerResolution(int period)
        {
            if (!Common.IsPosixEnvironment)
            {
                if (timeEndPeriod(period) != 0)
                    throw new InvalidOperationException("Specified period resolution is out of range and is not supported.");
            }
        }

        // Gets timer capabilities.
        [DllImport("winmm.dll")]
        private static extern int timeGetDevCaps(ref TimerCapabilities caps, int sizeOfTimerCaps);

        // Creates and starts the timer.
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimerProc proc, IntPtr user, TimerMode mode);

        // Stops and destroys the timer.
        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        // Sets the minimum resolution for periodic timers.
        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int period);

        // Clears a previously set minimum timer resolution.
        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int period);

        #endregion

        #region [ DateTimePrecise Adaptation ]

        // This code is suspected to be a large contributing factor for thrashing of
        // the garbage collector. It has been removed, pending a thorough code review.

        ///// <summary>
        ///// This class provides a way to get a DateTime that exhibits the relative precision of
        ///// System.Diagnostics.Stopwatch, and the absolute accuracy of DateTime.Now.
        ///// </summary>
        ///// <remarks>
        ///// This class is based on James Brock's DateTimePrecise class which can be found on the Code Project:
        ///// http://www.codeproject.com/KB/cs/DateTimePrecise.aspx?msg=2688543#xx2688543xx
        ///// </remarks>
        //private class PreciseTime
        //{
        //    private sealed class ImmutableTimeState
        //    {
        //        public ImmutableTimeState(DateTime observedTime, DateTime baseTime, long elapsedTicks, long systemFrequency)
        //        {
        //            this.ObservedTime = observedTime;
        //            this.BaseTime = baseTime;
        //            this.ElapsedTicks = elapsedTicks;
        //            this.SystemFrequency = systemFrequency;
        //        }

        //        public readonly DateTime ObservedTime;
        //        public readonly DateTime BaseTime;
        //        public readonly long ElapsedTicks;
        //        public readonly long SystemFrequency;
        //    }

        //    private readonly Stopwatch m_stopwatch;
        //    private ImmutableTimeState m_timeState;
        //    private readonly long m_synchronizePeriodStopwatchTicks;
        //    private long m_synchronizePeriodClockTicks;

        //    /// <summary>Creates a new instance of DateTimePrecise.</summary>
        //    /// <remarks>
        //    /// A large value of synchronizePeriodSeconds may cause arithmetic overthrow
        //    /// exceptions to be thrown. A small value may cause the time to be unstable.
        //    /// A good value is 10.
        //    /// </remarks>
        //    /// <param name="synchronizePeriodSeconds">The number of seconds after which the class will synchronize itself with the system clock.</param>
        //    public PreciseTime(long synchronizePeriodSeconds)
        //    {
        //        m_stopwatch = Stopwatch.StartNew();

        //        DateTime t = DateTime.UtcNow;
        //        m_timeState = new ImmutableTimeState(t, t, m_stopwatch.ElapsedTicks, Stopwatch.Frequency);

        //        m_synchronizePeriodStopwatchTicks = synchronizePeriodSeconds * Stopwatch.Frequency;
        //        m_synchronizePeriodClockTicks = synchronizePeriodSeconds * Ticks.PerSecond;
        //    }

        //    public DateTime UtcNow
        //    {
        //        get
        //        {
        //            long elapsedTicks = m_stopwatch.ElapsedTicks;
        //            ImmutableTimeState timeState = m_timeState;
        //            DateTime precisionTime = timeState.BaseTime.AddTicks(((elapsedTicks - timeState.ElapsedTicks) * Ticks.PerSecond) / timeState.SystemFrequency);

        //            if (elapsedTicks >= timeState.ElapsedTicks + m_synchronizePeriodStopwatchTicks)
        //            {
        //                // Perform clock resynchronization
        //                DateTime systemTime = DateTime.UtcNow;
        //                long systemTicks = systemTime.Ticks;
        //                long observedTicks = timeState.ObservedTime.Ticks;
        //                long systemFrequencyEstimationDenominator = systemTicks - observedTicks + systemTicks + systemTicks - precisionTime.Ticks - observedTicks;

        //                if (systemFrequencyEstimationDenominator != 0L)
        //                {
        //                    // Last parameter is a calculation that asymptotically approaches the measured system frequency
        //                    m_timeState = new ImmutableTimeState(systemTime, precisionTime, elapsedTicks, ((elapsedTicks - timeState.ElapsedTicks) * Ticks.PerSecond * 2) / systemFrequencyEstimationDenominator);
        //                }
        //            }

        //            // Return high-resolution timestamp
        //            return precisionTime;
        //        }
        //    }
        //}

        //private static PreciseTime s_preciseTime;           // Precise time implementation.
        //private static Timer s_synchronizer;                // Lightweight timer used for precise time synchronization.

        ///// <summary>
        ///// Gets a high-resolution <see cref="DateTime"/> value of the current time on this computer,
        ///// expressed in Coordinated Universal Time (UTC).
        ///// </summary>
        ///// <remarks>
        ///// <para>
        ///// This shared property provides a way to get a <see cref="DateTime"/> value that exhibits the relative
        ///// precision of <see cref="Stopwatch"/>, and the absolute accuracy of <see cref="DateTime.UtcNow"/>.
        ///// </para>
        ///// <para>
        ///// This property is useful for obtaining high-resolution accurate timestamps for events that occur in the
        ///// "sub-second" world (e.g., time-stamping events happening hundreds or thousands of times per second).
        ///// Note that the normal <see cref="DateTime.UtcNow"/> property has a maximum resolution of ~16 milliseconds.
        ///// </para>
        ///// </remarks>
        //public static DateTime UtcNow
        //{
        //    get
        //    {
        //        // Setup a new precise time class at first call
        //        if ((object)s_preciseTime == null)
        //            InitializePreciseTime();

        //        return s_preciseTime.UtcNow;
        //    }
        //}

        ///// <summary>
        ///// Gets a high-resolution <see cref="DateTime"/> value of the current time on this computer,
        ///// expressed in the local time zone.
        ///// </summary>
        ///// <remarks>
        ///// <para>
        ///// This shared property provides a way to get a <see cref="DateTime"/> value that exhibits the relative
        ///// precision of <see cref="Stopwatch"/>, and the absolute accuracy of <see cref="DateTime.Now"/>.
        ///// </para>
        ///// <para>
        ///// This property is useful for obtaining high-resolution accurate timestamps for events that occur in the
        ///// "sub-second" world (e.g., time-stamping events happening hundreds or thousands of times per second).
        ///// Note that the normal <see cref="DateTime.Now"/> property has a maximum resolution of ~16 milliseconds.
        ///// </para>
        ///// </remarks>
        //public static DateTime Now
        //{
        //    get
        //    {
        //        return UtcNow.ToLocalTime();
        //    }
        //}

        //// Initializes the precise timing mechanism
        //private static void InitializePreciseTime()
        //{
        //    // We just use the recommended synchronization period for general purpose use
        //    const int synchronizationPeriod = 10;

        //    // Create a new precise time class
        //    s_preciseTime = new PreciseTime(synchronizationPeriod);

        //    // We setup a lightweight timer that will make sure precise time mechanism gets
        //    // called regularly, in case user doesn't, so it can maintain synchronization
        //    s_synchronizer = new Timer(synchronizationPeriod * 1000.0D);
        //    s_synchronizer.Elapsed += s_synchronizer_Elapsed;
        //    s_synchronizer.Start();
        //}

        //// We make sure and call PreciseTime.UtcNow regularly so it can maintain synchronization
        //private static void s_synchronizer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    DateTime now = s_preciseTime.UtcNow;
        //}

        #endregion
    }
}