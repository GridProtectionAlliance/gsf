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

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
                m_logSmall.ShouldRaiseMessageSupressionNotifications = false;
                m_logMedium.ShouldRaiseMessageSupressionNotifications = false;
                m_logLarge.ShouldRaiseMessageSupressionNotifications = false;
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
                else if (delay > m_delay + 500)
                {
                    m_logMedium.Publish(delay.ToString() + " ms");
                }
                else if (delay > m_delay + 100)
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

        private static readonly LogPublisher Log = Logger.CreatePublisher(typeof(ThreadPoolMonitor), MessageClass.Component);
        private static List<Monitor> s_monitors;
        private static ShortTime s_lastResetTime;

        static ThreadPoolMonitor()
        {
            if (OptimizationOptions.EnableThreadPoolMonitoring)
            {
                s_monitors = new List<Monitor>();
                s_monitors.Add(new Monitor(0));
                s_monitors.Add(new Monitor(10));
                s_monitors.Add(new Monitor(100));
                s_monitors.Add(new Monitor(1000));

                s_lastResetTime = ShortTime.Now;

                Log.Publish(MessageLevel.Info, "Starting ThreadPool Monitoring.");

                LoadingAdjustedTimestamp.OnHighLoad += LoadingAdjustedTimestamp_OnHighLoad;
            }
        }

        /// <summary>
        /// Initializes the static constructor
        /// </summary>
        public static void Initialize()
        {
            //Initializes the static constructor.
        }

        private static void LoadingAdjustedTimestamp_OnHighLoad()
        {
            if (s_lastResetTime.ElapsedSeconds() > 10)
            {
                s_lastResetTime = ShortTime.Now;
                MonitorThreads();
            }
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
