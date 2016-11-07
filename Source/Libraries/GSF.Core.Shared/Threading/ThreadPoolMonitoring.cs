//******************************************************************************************************
//  ThreadPoolMonitoring.cs - Gbtc
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using GSF.Diagnostics;
using GSF.Threading;

namespace GSF.Core.Threading
{
    /// <summary>
    /// This class will monitor the performance of the ThreadPool and report any indications of system stress.
    /// </summary>
    public static class ThreadPoolMonitoring
    {
        private static readonly LogPublisher Log = Logger.CreatePublisher(typeof(ThreadPoolMonitoring), MessageClass.Component);

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
                else if (delay > m_delay + 100)
                {
                    m_logMedium.Publish(delay.ToString() + " ms");
                }
                else if (delay > m_delay + 10)
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

        private static ScheduledTask s_fireTimers;
        private static List<Monitor> s_monitors;

        static ThreadPoolMonitoring()
        {
            s_monitors = new List<Monitor>();
            s_monitors.Add(new Monitor(0));
            s_monitors.Add(new Monitor(10));
            s_monitors.Add(new Monitor(100));
            s_monitors.Add(new Monitor(1000));
            s_monitors.Add(new Monitor(10000));

            s_fireTimers = new ScheduledTask();
            s_fireTimers.Running += FireTimersRunning;
            s_fireTimers.Start(250);
        }

        private static void FireTimersRunning(object sender, EventArgs<ScheduledTaskRunningReason> e)
        {
            foreach (var monitor in s_monitors)
            {
                monitor.Start();
            }
            s_fireTimers.Start(1000);
        }

        public static void Initialize()
        {

        }

        //static void m_timing_Running(object sender, GSF2.EventArgs<ScheduledTaskRunningReason> e)
        //{
        //    if (e.Argument == ScheduledTaskRunningReason.Disposing)
        //        return;

        //    double msDelta;
        //    switch (s_stage)
        //    {
        //        case 0:
        //            Thread.MemoryBarrier();
        //            s_time = ShortTime.Now;
        //            Thread.MemoryBarrier();
        //            ThreadPool.QueueUserWorkItem(RunImmediately);
        //            s_stage = 1;
        //            return;
        //        case 1:
        //            s_time = ShortTime.Now;
        //            s_timing.Start(1);
        //            s_stage = 2;
        //            return;
        //        case 2:
        //            msDelta = (ShortTime.Now - s_time).TotalMilliseconds;
        //            s_sb.AppendFormat("Threadpool 1ms Timer Time: {0} ms; ", msDelta.ToString("0.000"));
        //            s_time = ShortTime.Now;
        //            s_stage = 3;
        //            s_timing.Start(10);
        //            return;
        //        case 3:
        //            msDelta = (ShortTime.Now - s_time).TotalMilliseconds;
        //            s_sb.AppendFormat("Threadpool 10ms Timer Time: {0} ms; ", msDelta.ToString("0.000"));
        //            s_time = ShortTime.Now;
        //            s_stage = 4;
        //            s_timing.Start(100);
        //            return;
        //        case 4:
        //            msDelta = (ShortTime.Now - s_time).TotalMilliseconds;
        //            s_sb.AppendFormat("Threadpool 100ms Timer Time: {0} ms; ", msDelta.ToString("0.000"));
        //            s_time = ShortTime.Now;
        //            s_stage = 5;
        //            s_timing.Start(1000);
        //            return;
        //        case 5:
        //            msDelta = (ShortTime.Now - s_time).TotalMilliseconds;
        //            s_sb.AppendFormat("Threadpool 1000ms Timer Time: {0} ms; ", msDelta.ToString("0.000"));
        //            s_time = ShortTime.Now;
        //            s_stage = 0;
        //            s_timing.Start(30000);
        //            Log.Publish(MessageClass.Application, MessageLevel.Info, MessageFlags.None, "ThreadPool Performance", s_sb.ToString());
        //            s_sb.Clear();
        //            return;
        //    }

        //}

        //static void RunImmediately(object state)
        //{
        //    Thread.MemoryBarrier();
        //    var msDelta = (ShortTime.Now - s_time).TotalMilliseconds;
        //    Thread.MemoryBarrier();

        //    s_sb.AppendFormat("Threadpool Queue Time: {0} ms; ", msDelta.ToString("0.000"));
        //    s_timing.Start(10);
        //}


    }
}
