//******************************************************************************************************
//  LoadingAdjustedTimestamp.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  11/15/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************

using System;
using System.Threading;
using GSF.Diagnostics;

namespace GSF.Threading
{
    /// <summary>
    /// This will provide the user with a timestamp based upon system loading. This timestamp will only move forward in time and may advance
    /// rapidly when trying to catch up with system time. 
    /// </summary>
    /// <remarks>
    /// The purpose for this class is to allow LagTime values to dynamically adjust based on perceived system performance.
    /// There were issues where the system would have multi second delays introduced based on exception handling or long garbage collection
    /// cycles that would cause all measurements to arrive outside the typical system lag time. This class
    /// attempts to detect when these events occur and provide increased lag time during these windows.
    /// </remarks>
    public static class LoadingAdjustedTimestamp
    {
        /// <summary>
        /// This event will be raised when the idle thread is falling behind, but the real-time thread is keeping up.
        /// This should only be used by certain diagnostics code to dump the current thread pool work items.
        /// </summary>
        internal static event Action OnHighLoad;

        private static readonly LogPublisher Log;

        private static readonly LogEventPublisher LogLoadingClock;
        private static readonly LogEventPublisher LogSmall;
        private static readonly LogEventPublisher LogMedium;
        private static readonly LogEventPublisher LogLarge;

        private static readonly Thread MonitorHighThread;
        private static readonly Thread MonitorNormalThread;
        private static readonly Thread MonitorLowThread;

        private static long s_currentTime;
        private static ShortTime s_currentTimeSetTime;

        private static int s_highProcessCount;
        private static int s_normalProcessCount;
        private static int s_lowProcessCount;

        static LoadingAdjustedTimestamp()
        {
            Reset();

            Log = Logger.CreatePublisher(typeof(LoadingAdjustedTimestamp), MessageClass.Component);
            LogLoadingClock = Log.RegisterEvent(MessageLevel.Info, MessageFlags.SystemHealth, $"Loading Clock is behind", 0, MessageRate.EveryFewSeconds(1), 1);
            LogSmall = Log.RegisterEvent(MessageLevel.Info, MessageFlags.SystemHealth, $"Short High Load Condition Detected", 0, MessageRate.EveryFewSeconds(10), 5);
            LogMedium = Log.RegisterEvent(MessageLevel.Warning, MessageFlags.SystemHealth, $"Medium High Load Condition Detected", 0, MessageRate.EveryFewSeconds(10), 5);
            LogLarge = Log.RegisterEvent(MessageLevel.Error, MessageFlags.SystemHealth, $"Long High Load Condition Detected", 0, MessageRate.EveryFewSeconds(10), 5);

            LogLoadingClock.ShouldRaiseMessageSupressionNotifications = false;
            LogSmall.ShouldRaiseMessageSupressionNotifications = false;
            LogMedium.ShouldRaiseMessageSupressionNotifications = false;
            LogLarge.ShouldRaiseMessageSupressionNotifications = false;

            MonitorHighThread = new Thread(MonitorHighPriority, short.MaxValue);
            MonitorHighThread.Priority = ThreadPriority.Highest;
            MonitorHighThread.IsBackground = true;
            MonitorHighThread.Start();

            MonitorNormalThread = new Thread(MonitorNormalPriority, short.MaxValue);
            MonitorNormalThread.Priority = ThreadPriority.Normal;
            MonitorNormalThread.IsBackground = true;
            MonitorNormalThread.Start();

            MonitorLowThread = new Thread(MonitorLowPriority, short.MaxValue);
            MonitorLowThread.Priority = ThreadPriority.Lowest;
            MonitorLowThread.IsBackground = true;
            MonitorLowThread.Start();
        }

        private static void Reset()
        {
            Interlocked.Exchange(ref s_currentTime, DateTime.UtcNow.Ticks);
            s_currentTimeSetTime = ShortTime.Now;
            Interlocked.Exchange(ref s_highProcessCount, 0);
            Interlocked.Exchange(ref s_normalProcessCount, 0);
            Interlocked.Exchange(ref s_lowProcessCount, 0);
        }

        private static void MonitorHighPriority()
        {
            //This one will primarily detect when the process is paused. 
            //This occurs when garbage collection occurs. 

            ShortTime lastTime = ShortTime.Now;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);
                    ShortTime currentTime = ShortTime.Now;

                    long sleepDelta = (long)lastTime.ElapsedTicks(currentTime);
                    long additionalDelay = sleepDelta - 10 * TimeSpan.TicksPerMillisecond;
                    double additionalDelayMS = additionalDelay / (double)TimeSpan.TicksPerMillisecond;

                    if (additionalDelayMS > 1000)
                    {
                        LogLarge.Publish(additionalDelayMS.ToString() + " ms (High Priority)");
                    }
                    else if (additionalDelayMS > 300)
                    {
                        LogMedium.Publish(additionalDelayMS.ToString() + " ms (High Priority)");
                    }
                    else if (additionalDelayMS > 100)
                    {
                        LogSmall.Publish(additionalDelayMS.ToString() + " ms (High Priority)");
                    }

                    Interlocked.Increment(ref s_highProcessCount);

                    AdjustRealtime();

                    lastTime = currentTime;
                }
                catch (Exception ex)
                {
                    Log.Publish(MessageLevel.Warning, MessageFlags.BugReport, "Unexpected Exception Caught", null, null, ex);
                    Thread.Sleep(1000);
                }
            }
        }

        private static void MonitorNormalPriority()
        {
            ShortTime lastTime = ShortTime.Now;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);
                    ShortTime currentTime = ShortTime.Now;

                    long sleepDelta = (long)lastTime.ElapsedTicks(currentTime);
                    long additionalDelay = sleepDelta - 100 * TimeSpan.TicksPerMillisecond;
                    double additionalDelayMS = additionalDelay / (double)TimeSpan.TicksPerMillisecond;

                    if (additionalDelayMS > 1000)
                    {
                        LogLarge.Publish(additionalDelayMS.ToString() + " ms (Normal Priority)");
                    }
                    else if (additionalDelayMS > 500)
                    {
                        LogMedium.Publish(additionalDelayMS.ToString() + " ms (Normal Priority)");
                    }

                    Interlocked.Increment(ref s_normalProcessCount);

                    lastTime = currentTime;
                }
                catch (Exception ex)
                {
                    Log.Publish(MessageLevel.Warning, MessageFlags.BugReport, "Unexpected Exception Caught", null, null, ex);
                    Thread.Sleep(1000);
                }
            }
        }

        private static void MonitorLowPriority()
        {
            ShortTime lastTime = ShortTime.Now;
            while (true)
            {
                try
                {
                    Thread.Sleep(10);
                    ShortTime currentTime = ShortTime.Now;

                    long sleepDelta = (long)lastTime.ElapsedTicks(currentTime);
                    long additionalDelay = sleepDelta - 100 * TimeSpan.TicksPerMillisecond;
                    double additionalDelayMS = additionalDelay / (double)TimeSpan.TicksPerMillisecond;

                    if (additionalDelayMS > 1000)
                    {
                        LogLarge.Publish(additionalDelayMS.ToString() + " ms (Normal Priority)");
                    }
                    else if (additionalDelayMS > 500)
                    {
                        LogMedium.Publish(additionalDelayMS.ToString() + " ms (Normal Priority)");
                    }

                    Interlocked.Increment(ref s_lowProcessCount);

                    lastTime = currentTime;
                }
                catch (Exception ex)
                {
                    Log.Publish(MessageLevel.Warning, MessageFlags.BugReport, "Unexpected Exception Caught", null, null, ex);
                    Thread.Sleep(1000);
                }
            }
        }

        private static void AdjustRealtime()
        {
            //This method is supposed to be called every 10 milliseconds on a high priority thread.
            //However don't update the time unless 100ms has elapsed.

            //Since DateTime can be programmatically changed, I'm using CPU Registers.
            long physicalTimeSinceLastSet = (long)s_currentTimeSetTime.ElapsedTicks();
            if (physicalTimeSinceLastSet < 100 * TimeSpan.TicksPerMillisecond)
                return;

            long clockSinceLastSet = DateTime.UtcNow.Ticks - s_currentTime;

            if (Math.Abs(physicalTimeSinceLastSet - clockSinceLastSet) > TimeSpan.TicksPerSecond)
            {
                Log.Publish(MessageLevel.Warning, MessageFlags.SystemHealth,
                    "System time has been adjusted by more than 1 second. Resetting Loading Figures.",
                    $"Expected Clock Change: {physicalTimeSinceLastSet / TimeSpan.TicksPerMillisecond} ms, Clock change: {clockSinceLastSet / TimeSpan.TicksPerMillisecond}");

                Reset();
                return;
            }
            if (clockSinceLastSet < 0)
            {
                Log.Publish(MessageLevel.Info, MessageFlags.SystemHealth, "System Clock did not advance.",
                    $"Expected Clock Change: {physicalTimeSinceLastSet / TimeSpan.TicksPerMillisecond} ms, Clock change: {clockSinceLastSet / TimeSpan.TicksPerMillisecond}");
            }

            int highDelay = Interlocked.Exchange(ref s_highProcessCount, 0);
            int normalDelay = Interlocked.Exchange(ref s_normalProcessCount, 0);
            int lowDelay = Interlocked.Exchange(ref s_lowProcessCount, 0);

            if (highDelay >= 5 && lowDelay <= 1)
            {
                try
                {
                    OnHighLoad?.Invoke();
                }
                catch (Exception)
                {

                }
            }

            long maxAdjustment = 0;
            if (lowDelay >= 7)
            {
                maxAdjustment = 500 * TimeSpan.TicksPerMillisecond;
            }
            else if (lowDelay >= 5)
            {
                maxAdjustment = 400 * TimeSpan.TicksPerMillisecond;
            }
            else if (normalDelay >= 7)
            {
                maxAdjustment = 300 * TimeSpan.TicksPerMillisecond;
            }
            else if (normalDelay >= 5)
            {
                maxAdjustment = 200 * TimeSpan.TicksPerMillisecond;
            }
            else if (highDelay >= 4)
            {
                maxAdjustment = 100 * TimeSpan.TicksPerMillisecond;
            }
            else
            {
                maxAdjustment = 50 * TimeSpan.TicksPerMillisecond;
            }

            if (maxAdjustment < clockSinceLastSet)
            {
                LogLoadingClock.Publish($"Behind by {clockSinceLastSet / TimeSpan.TicksPerMillisecond}ms adjusting by {maxAdjustment / TimeSpan.TicksPerMillisecond}ms");
            }

            long adjustment = Math.Min(clockSinceLastSet, maxAdjustment);

            if (adjustment >= 0)
            {
                Interlocked.Exchange(ref s_currentTime, s_currentTime + adjustment);
            }
            s_currentTimeSetTime = ShortTime.Now;
        }

        /// <summary>
        /// Gets the current loading Adjusted Time
        /// </summary>
        public static DateTime CurrentTime
        {
            get
            {
                if (Environment.Is64BitProcess)
                {
                    return new DateTime(s_currentTime);
                }
                return new DateTime(Interlocked.Read(ref s_currentTime));
            }
        }

    }
}
