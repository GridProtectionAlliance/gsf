//******************************************************************************************************
//  ThreadPoolMonitor.cs - Gbtc
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
//  11/07/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using GSF.Threading;
using Microsoft.Diagnostics.Runtime;

namespace GSF.Diagnostics
{
    /// <summary>
    /// This class will monitor the performance of the ThreadPool and report any indications of system stress.
    /// </summary>
    public static class ThreadPoolMonitor
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
        private class Monitor
        {
            private int m_delay;
            private ScheduledTask m_task;
            private ShortTime? m_time;
            private LogEventPublisher m_logSmall;
            private LogEventPublisher m_logMedium;
            private LogEventPublisher m_logLarge;

            public Monitor(int delay)
            {
                m_logSmall = Log.RegisterEvent(MessageLevel.Info, MessageFlags.SystemHealth, $"ThreadPool Small Delay Variance ({delay} ms)", 0, MessageRate.EveryFewSeconds(10), 5);
                m_logMedium = Log.RegisterEvent(MessageLevel.Warning, MessageFlags.SystemHealth, $"ThreadPool Medium Delay Variance ({delay} ms)", 0, MessageRate.EveryFewSeconds(10), 5);
                m_logLarge = Log.RegisterEvent(MessageLevel.Error, MessageFlags.SystemHealth, $"ThreadPool Large Delay Variance ({delay} ms)", 0, MessageRate.EveryFewSeconds(10), 5);
                m_delay = delay;
                m_time = null;
                m_task = new ScheduledTask();
                m_task.Running += task_Running;
            }

            private void task_Running(object sender, EventArgs<ScheduledTaskRunningReason> e)
            {
                ShortTime currentTime = ShortTime.Now;
                ShortTime queueTime;

                lock (this)
                {
                    if (!m_time.HasValue)
                    {
                        return; //Should never happen, but just in case;
                    }
                    queueTime = m_time.Value;
                    m_time = null;
                }

                double delay = (currentTime - queueTime).TotalMilliseconds;

                if (delay > m_delay + 1000)
                {
                    m_logLarge.Publish(delay.ToString() + " ms");
                }
                else if (delay > m_delay + 250)
                {
                    m_logMedium.Publish(delay.ToString() + " ms");
                }
                else if (delay > m_delay + 50)
                {
                    m_logSmall.Publish(delay.ToString() + " ms");
                }
                else
                {
                    return;
                }

                //If I published a message, a long delay could have occurred and messed
                //up my timer, therefore reset the current time.
                lock (this)
                {
                    if (m_time.HasValue)
                    {
                        m_time = ShortTime.Now;
                    }
                }

            }

            public void Start()
            {
                lock (this)
                {
                    //Check if the last queue has processed, if not, then quit.
                    if (m_time.HasValue)
                        return;

                    m_time = ShortTime.Now;
                    if (m_delay == 0)
                    {
                        m_task.Start();
                    }
                    else
                    {
                        m_task.Start(m_delay);
                    }
                }
            }

        }

        private class MonitorSleep
        {
            private LogEventPublisher m_logSmall;
            private LogEventPublisher m_logMedium;
            private LogEventPublisher m_logLarge;
            private Thread s_monitorSleepsGC;

            public MonitorSleep()
            {
                m_logSmall = Log.RegisterEvent(MessageLevel.Info, MessageFlags.SystemHealth, $"Process Pause Small", 0, MessageRate.EveryFewSeconds(10), 5);
                m_logMedium = Log.RegisterEvent(MessageLevel.Warning, MessageFlags.SystemHealth, $"Process Pause Medium", 0, MessageRate.EveryFewSeconds(10), 5);
                m_logLarge = Log.RegisterEvent(MessageLevel.Error, MessageFlags.SystemHealth, $"Process Pause Large", 0, MessageRate.EveryFewSeconds(10), 5);

                s_monitorSleepsGC = new Thread(MonitorSleeps);
                s_monitorSleepsGC.Priority = ThreadPriority.Highest;
                s_monitorSleepsGC.IsBackground = true;
                s_monitorSleepsGC.Start();
            }

            private void MonitorSleeps()
            {
                ShortTime lastTime = ShortTime.Now;
                while (true)
                {
                    try
                    {
                        Thread.Sleep(10);
                        ShortTime currentTime = ShortTime.Now;

                        double delay = (currentTime - lastTime).TotalMilliseconds;

                        if (delay > 1000)
                        {
                            m_logLarge.Publish(delay.ToString() + " ms");
                        }
                        else if (delay > 250)
                        {
                            m_logMedium.Publish(delay.ToString() + " ms");
                        }
                        else if (delay > 50)
                        {
                            m_logSmall.Publish(delay.ToString() + " ms");
                        }
                        else
                        {

                        }
                        lastTime = currentTime;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(1000);
                    }

                }
            }

        }

        private class MonitorGC
        {
            private Thread s_monitor;

            public MonitorGC()
            {
                s_monitor = new Thread(Monitor);
                s_monitor.Priority = ThreadPriority.AboveNormal;
                s_monitor.IsBackground = true;
                s_monitor.Start();
            }

            private void Monitor()
            {
                while (true)
                {
                    Thread.Sleep(100);
                    try
                    {
                        GC.RegisterForFullGCNotification(99, 99);
                        switch (GC.WaitForFullGCApproach())
                        {
                            case GCNotificationStatus.Succeeded:
                                Log.Publish(MessageLevel.Warning, "GCWait Succeeded");
                                break;
                            case GCNotificationStatus.Failed:
                                Log.Publish(MessageLevel.Warning, "GCWait Failed");
                                break;
                            case GCNotificationStatus.Canceled:
                                Log.Publish(MessageLevel.Warning, "GCWait Canceled");
                                break;
                            case GCNotificationStatus.Timeout:
                                Log.Publish(MessageLevel.Warning, "GCWait Timeout");
                                break;
                            case GCNotificationStatus.NotApplicable:
                                Log.Publish(MessageLevel.Warning, "GCWait NA");
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        switch (GC.WaitForFullGCComplete())
                        {
                            case GCNotificationStatus.Succeeded:
                                Log.Publish(MessageLevel.Warning, "GC Succeeded");
                                break;
                            case GCNotificationStatus.Failed:
                                Log.Publish(MessageLevel.Warning, "GC Failed");
                                break;
                            case GCNotificationStatus.Canceled:
                                Log.Publish(MessageLevel.Warning, "GC Canceled");
                                break;
                            case GCNotificationStatus.Timeout:
                                Log.Publish(MessageLevel.Warning, "GC Timeout");
                                break;
                            case GCNotificationStatus.NotApplicable:
                                Log.Publish(MessageLevel.Warning, "GC NA");
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Publish(MessageLevel.Warning, "Error", null, null, ex);
                        Thread.Sleep(5000);
                    }
                }
            }

        }


        private static readonly LogPublisher Log = Logger.CreatePublisher(typeof(ThreadPoolMonitor), MessageClass.Component);
        private static ScheduledTask s_fireTimers;
        private static List<Monitor> s_monitors;
        private static ScheduledTask s_fireTimers2;
        private static Thread s_monitorContentionThread;
        private static volatile Tuple<ShortTime> s_lastResetTime;

        private static MonitorSleep s_monitorSleep;
        private static MonitorGC s_monitorGC;

        static ThreadPoolMonitor()
        {
            if (OptimizationOptions.EnableThreadPoolMonitoring)
            {
                s_monitorSleep = new MonitorSleep();
                s_monitorGC = new MonitorGC();

                s_monitors = new List<Monitor>();
                s_monitors.Add(new Monitor(0));
                s_monitors.Add(new Monitor(10));
                s_monitors.Add(new Monitor(100));
                s_monitors.Add(new Monitor(1000));

                s_fireTimers = new ScheduledTask();
                s_fireTimers.Running += FireTimersRunning;
                s_fireTimers.Start(250);

                s_lastResetTime = new Tuple<ShortTime>(ShortTime.Now);
                s_monitorContentionThread = new Thread(RunMonitor);
                s_monitorContentionThread.Priority = ThreadPriority.Highest;
                s_monitorContentionThread.IsBackground = true;
                s_monitorContentionThread.Start();

                s_fireTimers2 = new ScheduledTask();
                s_fireTimers2.Running += FireTimers2Running;
                s_fireTimers2.Start(100);

                Log.Publish(MessageLevel.Info, "Starting ThreadPool Monitoring.");
            }
        }

        private static void FireTimersRunning(object sender, EventArgs<ScheduledTaskRunningReason> e)
        {
            foreach (var monitor in s_monitors)
            {
                monitor.Start();
            }
            s_fireTimers.Start(1000);
        }

        /// <summary>
        /// Initializes the ThreadPool Monitor
        /// </summary>
        public static void Initialize()
        {

        }

        private static void RunMonitor()
        {
            while (true)
            {
                Thread.Sleep(100);
                try
                {
                    var time = s_lastResetTime;
                    if ((ShortTime.Now - time.Item1).TotalSeconds > 1)
                    {
                        MonitorThreads();
                        Thread.Sleep(5000); //Don't do this more than once every 5 seconds.
                    }
                }
                catch (Exception ex)
                {
                    Log.Publish(MessageLevel.Warning, "Error", null, null, ex);
                    Thread.Sleep(5000);
                }
            }
        }

        private static void FireTimers2Running(object sender, EventArgs<ScheduledTaskRunningReason> e)
        {
            s_lastResetTime = new Tuple<ShortTime>(ShortTime.Now);
            ThreadPool.QueueUserWorkItem(RunImmediately);
        }

        private static void RunImmediately(object state)
        {
            s_lastResetTime = new Tuple<ShortTime>(ShortTime.Now);
            s_fireTimers2.Start(100);
        }

        private static void MonitorThreads()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------");
            sb.AppendLine("Thread Pool Queue");
            sb.AppendLine("--------------------------------------------------------");

            ThreadPoolTrace.GetTrace(sb);

            sb.AppendLine("--------------------------------------------------------");
            sb.AppendLine("Timers");
            sb.AppendLine("--------------------------------------------------------");

            TimerTrace.GetTrace(sb);

            if (OptimizationOptions.EnableThreadStackDumping)
            {
                using (var dataTarget = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, 5000, AttachFlag.Passive))
                {
                    foreach (var clr in dataTarget.ClrVersions)
                    {
                        var runtime = clr.CreateRuntime();

                        sb.AppendLine("--------------------------------------------------------");
                        sb.AppendLine("Thread Stacks");
                        sb.AppendLine("--------------------------------------------------------");
                        foreach (var t in runtime.Threads)
                        {
                            bool hasWrittenData = false;

                            foreach (var item in t.StackTrace)
                            {
                                if (item.Method != null)
                                {
                                    if (!hasWrittenData)
                                        hasWrittenData = true;
                                    sb.AppendLine(item.ToString());
                                }
                            }
                            if (hasWrittenData)
                            {
                                sb.AppendLine("--------------------------------------------------------");
                            }
                        }
                    }
                }
            }
            Log.Publish(MessageLevel.Warning, "ThreadPool Stack Trace", "Dumped threadpool stack trace", sb.ToString());
        }
    }
}
