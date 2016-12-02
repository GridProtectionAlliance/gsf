//******************************************************************************************************
//  EventTimer.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  11/17/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************

using System;
using GSF.Diagnostics;

namespace GSF.Threading
{
    /// <summary>
    /// A reoccurring timer that fires on a given interval. This event timer will always fire at the top of the specified interval.
    /// If the callback takes too long, the next interval will be skipped.
    /// </summary>
    public class EventTimer
        : DisposableLoggingClassBase
    {
        /// <summary>
        /// An event that fires every time the specified period elapses.
        /// </summary>
        public event EventHandler Elapsed;

        ScheduledTask m_timer;
        TimeSpan m_period;
        TimeSpan m_dayOffset;
        bool m_started;
        bool m_disposed;

        /// <summary>
        /// Creates a <see cref="EventTimer"/>
        /// </summary>
        /// <param name="period"></param>
        /// <param name="dayOffset"></param>
        EventTimer(TimeSpan period, TimeSpan dayOffset)
            : base(MessageClass.Component)
        {
            m_period = period;
            m_dayOffset = dayOffset;
            Log.InitialStackMessages = Log.InitialStackMessages.Union("Timer", string.Format("EventTimer: {0} in {1}", m_period.ToString(), m_dayOffset.ToString()));
        }

        /// <summary>
        /// The amount of time before the next interval will occur.
        /// </summary>
        public TimeSpan TimeUntilNextExecution
        {
            get
            {
                long current = DateTime.UtcNow.Ticks;
                long subtractOffset = current - m_dayOffset.Ticks;
                long remainderTicks = m_period.Ticks - subtractOffset % m_period.Ticks;
                int delay = (int)(remainderTicks / TimeSpan.TicksPerMillisecond) + 1;
                if (delay < 10)
                    delay += (int)m_period.TotalMilliseconds;
                return new TimeSpan(delay * TimeSpan.TicksPerMillisecond);
            }
        }

        /// <summary>
        /// Modifies the timer. Note, it takes some time for this timer to go into effect.
        /// </summary>
        /// <param name="period"></param>
        /// <param name="dayOffset"></param>
        public void ChangeTimer(TimeSpan period, TimeSpan dayOffset = default(TimeSpan))
        {
            m_period = period;
            m_dayOffset = dayOffset;
        }

        /// <summary>
        /// This timer will reliably fire the directory polling every interval.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TimerRunning(object sender, EventArgs<ScheduledTaskRunningReason> e)
        {
            //This cannot be combined with m_directoryPolling because 
            //Scheduled task does not support managing multiple conflicting timers.
            if (e.Argument == ScheduledTaskRunningReason.Disposing)
                return;

            try
            {
                Elapsed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Log.Publish(MessageLevel.Error, "Event Timer Exception on raising event.", null, null, ex);
            }

            RestartTimer();
        }

        /// <summary>
        /// Immediately executes the timer, not waiting for the elapsed interval.
        /// </summary>
        public void RunNow()
        {
            m_timer.Start();
        }

        void RestartTimer()
        {
            long current = DateTime.UtcNow.Ticks;
            long subtractOffset = current - m_dayOffset.Ticks;
            long remainderTicks = m_period.Ticks - subtractOffset % m_period.Ticks;
            int delay = (int)(remainderTicks / TimeSpan.TicksPerMillisecond) + 1;
            if (delay < 10)
                delay += (int)m_period.TotalMilliseconds;

            m_timer.Start(delay);
        }

        /// <summary>
        /// Starts the watching
        /// </summary>
        public void Start(ThreadingMode mode = ThreadingMode.ThreadPool)
        {
            if (m_started)
                return;
            m_started = true;

            Log.Publish(MessageLevel.Debug, "EventTimer Started");

            m_timer = new ScheduledTask(mode);
            m_timer.Running += TimerRunning;
            RestartTimer();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DisposableLoggingClassBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "m_timer")]
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                m_disposed = true;
                m_timer?.Dispose();
                m_timer = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates a <see cref="EventTimer"/> on the specified interval.
        /// </summary>
        /// <param name="period">How often the time will reset</param>
        /// <param name="dayOffset">The offset as represented in UTC time when the event should fire</param>
        /// <returns></returns>
        public static EventTimer Create(TimeSpan period, TimeSpan dayOffset = default(TimeSpan))
        {
            return new EventTimer(period, dayOffset);
        }

        /// <summary>
        /// Creates a <see cref="EventTimer"/> on the specified interval.
        /// </summary>
        /// <param name="periodInSecond">How often the time will reset</param>
        /// <param name="dayOffsetInSecond">The offset as represented in UTC time when the event should fire</param>
        /// <returns></returns>
        public static EventTimer CreateSeconds(double periodInSecond, double dayOffsetInSecond = 0)
        {
            return new EventTimer(new TimeSpan((long)(periodInSecond * TimeSpan.TicksPerSecond)), new TimeSpan((long)(dayOffsetInSecond * TimeSpan.TicksPerSecond)));
        }

        /// <summary>
        /// Creates a <see cref="EventTimer"/> on the specified interval.
        /// </summary>
        /// <param name="periodInMinutes">How often the time will reset</param>
        /// <param name="dayOffsetInMinutes">The offset as represented in UTC time when the event should fire</param>
        /// <returns></returns>
        public static EventTimer CreateMinutes(double periodInMinutes, double dayOffsetInMinutes = 0)
        {
            return new EventTimer(new TimeSpan((long)(periodInMinutes * TimeSpan.TicksPerMinute)), new TimeSpan((long)(dayOffsetInMinutes * TimeSpan.TicksPerMinute)));
        }

        /// <summary>
        /// Creates a <see cref="EventTimer"/> on the specified interval.
        /// </summary>
        /// <param name="periodInHours">How often the time will reset</param>
        /// <param name="dayOffsetInHours">The offset as represented in UTC time when the event should fire</param>
        /// <returns></returns>
        public static EventTimer CreateHours(double periodInHours, double dayOffsetInHours = 0)
        {
            return new EventTimer(new TimeSpan((long)(periodInHours * TimeSpan.TicksPerHour)), new TimeSpan((long)(dayOffsetInHours * TimeSpan.TicksPerHour)));
        }


    }
}
