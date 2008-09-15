//*******************************************************************************************************
//  TVA.PrecisionTimer.cs - High-resolution Timer and Timestamp Class
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/22/2003 - Leslie Sanford (Multimedia.Timer)
//       Original version of source code.
//  04/10/2008 - James Brock (DateTimePrecise)
//       Original version of source code.
//  08/21/2008 - J. Ritchie Carroll
//       Integrated, merged and adapted for TVA Code Library use as PrecisionTimer.
//  09/12/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace TVA
{
    #region " Original Copyright Notices "

    // Copyright (c) 2006 Leslie Sanford
    // *
    // * Permission is hereby granted, free of charge, to any person obtaining a copy
    // * of this software and associated documentation files (the "Software"), to
    // * deal in the Software without restriction, including without limitation the
    // * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
    // * sell copies of the Software, and to permit persons to whom the Software is
    // * furnished to do so, subject to the following conditions:
    // *
    // * The above copyright notice and this permission notice shall be included in
    // * all copies or substantial portions of the Software.
    // *
    // * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    // * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    // * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    // * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    // * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    // * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    // * THE SOFTWARE.
    //
    // * Leslie Sanford
    // * Email: jabberdabber@hotmail.com
    //

    // See also: The Code Project Open License (CPOL)
    // http://www.codeproject.com/info/cpol10.aspx

    #endregion

    /// <summary>
    /// Represents information about the multimedia Timer's capabilities.
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
    /// The exception that is thrown when a timer fails to start.
    /// </summary>
    public class TimerStartException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the TimerStartException class.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        public TimerStartException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Represents the Windows multimedia timer.
    /// </summary>
    public class PrecisionTimer : IDisposable
    {
        #region " Private Members "

        #region " DateTimePrecise Adaptation "

        /// <summary>
        /// This class provides a way to get a DateTime that exhibits the relative precision of
        /// System.Diagnostics.Stopwatch, and the absolute accuracy of DateTime.Now.
        /// </summary>
        /// <remarks>
        /// This class is based on James Brock's DateTimePrecise class which can be found on the Code Project:
        /// http://www.codeproject.com/KB/cs/DateTimePrecise.aspx?msg=2688543#xx2688543xx
        /// </remarks>
        private class PreciseTime
        {
            private sealed class ImmutableTimeState
            {
                public ImmutableTimeState(DateTime observedTime, DateTime baseTime, long elapsedTicks, long systemFrequency)
                {

                    this.ObservedTime = observedTime;
                    this.BaseTime = baseTime;
                    this.ElapsedTicks = elapsedTicks;
                    this.SystemFrequency = systemFrequency;

                }

                public readonly DateTime ObservedTime;
                public readonly DateTime BaseTime;
                public readonly long ElapsedTicks;
                public readonly long SystemFrequency;
            }

            private Stopwatch m_stopwatch;
            private long m_synchronizePeriodStopwatchTicks;
            private long m_synchronizePeriodClockTicks;
            private ImmutableTimeState m_timeState;

            /// <summary>Creates a new instance of DateTimePrecise.</summary>
            /// <remarks>
            /// A large value of synchronizePeriodSeconds may cause arithmetic overthrow
            /// exceptions to be thrown. A small value may cause the time to be unstable.
            /// A good value is 10.
            /// </remarks>
            /// <param name="synchronizePeriodSeconds">The number of seconds after which the class will synchronize itself with the system clock.</param>
            public PreciseTime(long synchronizePeriodSeconds)
            {
                m_stopwatch = Stopwatch.StartNew();
                m_stopwatch.Start();

                DateTime t = DateTime.UtcNow;
                m_timeState = new ImmutableTimeState(t, t, m_stopwatch.ElapsedTicks, Stopwatch.Frequency);

                m_synchronizePeriodStopwatchTicks = synchronizePeriodSeconds * Stopwatch.Frequency;
                m_synchronizePeriodClockTicks = synchronizePeriodSeconds * Common.TicksPerSecond;
            }

            public DateTime UtcNow
            {
                get
                {
                    long elapsedTicks = m_stopwatch.ElapsedTicks;
                    ImmutableTimeState timeState = m_timeState;
                    DateTime precisionTime = timeState.BaseTime.AddTicks(((int)((elapsedTicks - timeState.ElapsedTicks) * Common.TicksPerSecond) / timeState.SystemFrequency));

                    if (elapsedTicks >= timeState.ElapsedTicks + m_synchronizePeriodStopwatchTicks)
                    {
                        // Perform clock resynchronization
                        DateTime systemTime = DateTime.UtcNow;

                        // Last parameter is a calculation that asymptotically approachs the measured system frequency
                        m_timeState = new ImmutableTimeState(systemTime, precisionTime, elapsedTicks, (int)(((elapsedTicks - timeState.ElapsedTicks) * Common.TicksPerSecond * 2) / 
                            (systemTime.Ticks - timeState.ObservedTime.Ticks + systemTime.Ticks + systemTime.Ticks - precisionTime.Ticks - timeState.ObservedTime.Ticks)));
                    }

                    // Return high-resolution timestamp
                    return precisionTime;
                }
            }
        }

        #endregion

        // Defines constants for the multimedia Timer's event types.
        private enum TimerMode
        {
            OneShot, // Timer event occurs once.
            Periodic // Timer event occurs periodically.
        }

        // Represents the method that is called by Windows when a timer event occurs.
        private delegate void TimerProc(int id, int msg, int user, int param1, int param2);

        // Gets timer capabilities.
        [DllImport("winmm.dll")]
        private static extern int timeGetDevCaps(ref TimerCapabilities caps, int sizeOfTimerCaps);

        // Creates and starts the timer.
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimerProc proc, int user, TimerMode mode);

        // Stops and destroys the timer.
        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        // Timer identifier.
        private int m_timerID;

        // Timer mode.
        private TimerMode m_mode;

        // Period between timer events in milliseconds.
        private int m_period;

        // Timer resolution in milliseconds.
        private int m_resolution;

        // Called by Windows when a timer periodic event occurs.
        private TimerProc m_timeProc;

        // Indicates whether or not the timer is running.
        private bool m_running = false;

        // Indicates whether or not the timer has been disposed.
        private bool m_disposed = false;

        // Private user event args to pass into Ticks call
        private EventArgs m_eventArgs;

        // Multimedia timer capabilities.
        private static TimerCapabilities m_capabilities;

        // Precise time implementation.
        private static PreciseTime m_preciseTime;

        #endregion

        #region " Public Events "

        /// <summary>Occurs when the Timer has started.</summary>
        public event EventHandler Started;

        /// <summary>Occurs when the Timer has stopped.</summary>
        public event EventHandler Stopped;

        /// <summary>Occurs when the time period has elapsed.</summary>
        public event EventHandler Tick;

        #endregion

        #region " Construction "

        /// <summary>
        /// Initialize class.
        /// </summary>
        static PrecisionTimer()
        {
            // Get multimedia timer capabilities
            timeGetDevCaps(ref m_capabilities, Marshal.SizeOf(m_capabilities));

            // We just use the recommended synchronization period for general purpose TVA use
            m_preciseTime = new PreciseTime(10);
        }

        /// <summary>
        /// Initializes a new instance of the Timer class.
        /// </summary>
        public PrecisionTimer()
        {
            // Initialize timer with default values.
            m_mode = TimerMode.Periodic;
            m_period = Capabilities.PeriodMinimum;
            m_resolution = 1;
            m_running = false;
            m_timeProc = TimerEventCallback;
        }

        ~PrecisionTimer()
        {
            try
            {
                // Stop and destroy timer.
                if (IsRunning)
                    timeKillEvent(m_timerID);
            }
            finally
            {
            }
        }

        #region " IDisposable Members "

        /// <summary>
        /// Frees timer resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) below.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed && IsRunning) Stop();
            m_disposed = true;
        }

        #endregion

        #endregion

        #region " Shared Properties "

        /// <summary>Gets a high-resolution DateTime value of the current time on this computer, expressed in Coordinated Universal Time (UTC).</summary>
        /// <remarks>
        /// <para>
        /// This shared property provides a way to get a DateTime that exhibits the relative precision of
        /// System.Diagnostics.Stopwatch, and the absolute accuracy of DateTime.UtcNow.
        /// </para>
        /// <para>
        /// This property is useful for obtaining high-resolution accuarate timestamps for events that occur in the
        /// "sub-second" world (e.g., timestamping events happening hundreds or thousands of times per second).
        /// Note that the normal DateTime.UtcNow property has a maximum resolution of ~16 milliseconds.
        /// </para>
        /// </remarks>
        public static DateTime UtcNow
        {
            get
            {
                return m_preciseTime.UtcNow;
            }
        }

        /// <summary>Gets a high-resolution DateTime value of the current time on this computer, expressed in the local time zone.</summary>
        /// <remarks>
        /// <para>
        /// This shared property provides a way to get a DateTime that exhibits the relative precision of
        /// System.Diagnostics.Stopwatch, and the absolute accuracy of DateTime.Now.
        /// </para>
        /// <para>
        /// This property is useful for obtaining high-resolution accuarate timestamps for events that occur in the
        /// "sub-second" world (e.g., timestamping events happening hundreds or thousands of times per second).
        /// Note that the normal DateTime.Now property has a maximum resolution of ~16 milliseconds.
        /// </para>
        /// </remarks>
        public static DateTime Now
        {
            get
            {
                return UtcNow.ToLocalTime();
            }
        }

        #endregion

        #region " Methods "

        /// <summary>
        /// Starts the timer.
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
        /// Starts the timer.
        /// </summary>
        /// <param name="userArgs">User defined event arguments to pass into raised Tick event</param>
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

            if (m_running) return;

            // Cache user event args to pass into Ticks paramter
            m_eventArgs = userArgs;

            // Create and start timer.
            m_timerID = timeSetEvent(m_period, m_resolution, m_timeProc, 0, m_mode);

            // If the timer was created successfully.
            if (m_timerID != 0)
            {
                m_running = true;

                if (Started != null)
                    Started(this, EventArgs.Empty);
            }
            else
            {
                throw new TimerStartException("Unable to start multimedia Timer.");
            }
        }

        /// <summary>
        /// Stops timer.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        public void Stop()
        {
            if (m_disposed)
                throw new ObjectDisposedException("PrecisionTimer");

            if (!m_running) return;

            // Stop and destroy timer.
            timeKillEvent(m_timerID);
            m_timerID = 0;
            m_running = false;

            if (Stopped != null)
                Stopped(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets or sets the time between Tick events, in milliseconds.
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
                    throw new ArgumentOutOfRangeException("Period", value, "Multimedia Timer period out of range.");

                m_period = value;

                if (IsRunning && m_mode == TimerMode.Periodic)
                {
                    Stop();
                    Start(m_eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets the timer resolution, in milliseconds.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        /// <remarks>
        /// The resolution is in milliseconds. The resolution increases  with smaller values;
        /// a resolution of 0 indicates periodic events  should occur with the greatest possible
        /// accuracy. To reduce system  overhead, however, you should use the maximum value
        /// appropriate for your application.
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
                    throw new ArgumentOutOfRangeException("Resolution", value, "Multimedia timer resolution out of range.");

                m_resolution = value;

                if (IsRunning && m_mode == TimerMode.Periodic)
                {
                    Stop();
                    Start(m_eventArgs);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the PrecisionTimer should raise the Tick event each time
        /// the specified period elapses or only after the first time it elapses.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>
        /// true if the PrecisionTimer should raise the Tick event each time the interval elapses;
        /// false if it should raise the Tick event only once, after the first time the interval elapses.
        /// The default is true.
        /// </returns>
        [DefaultValue(true), Description("Gets or sets a value indicating whether the PrecisionTimer should raise the Tick event each time the specified period elapses or only after the first time it elapses."), Category("Behavior")]
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
        /// Gets a value indicating whether the Timer is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return m_running;
            }
        }

        /// <summary>
        /// Gets the system multimedia timer capabilities.
        /// </summary>
        public static TimerCapabilities Capabilities
        {
            get
            {
                return m_capabilities;
            }
        }

        // Callback method called by the Win32 multimedia timer when a timer event occurs.
        private void TimerEventCallback(int id, int msg, int user, int param1, int param2)
        {
            if (Tick != null)
                Tick(this, m_eventArgs);

            if (m_mode == TimerMode.OneShot)
                Stop();
        }

        #endregion
    }
}