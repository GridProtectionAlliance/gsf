//******************************************************************************************************
//  TimerTrace.cs - Gbtc
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
//  11/19/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using GSF.Reflection;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Executes a trace on the <see cref="System.Threading.Timer"/> for all queued timers.
    /// </summary>
    /// <remarks>
    /// This class heavily relies on reflection to get the Timer queue.
    /// Therefore it is very unlikely to work in MONO and can break 
    /// if Microsoft changes any of the member names or how the Timer 
    /// works.
    /// 
    /// In this case <see cref="WorksInThisRuntime"/> will be set to false
    /// and <see cref="GetTrace"/> will return "Not Supported"
    /// 
    /// </remarks>
    public static class TimerTrace
    {
        private static LogPublisher Log = Logger.CreatePublisher(typeof(TimerTrace), MessageClass.Component);
        /// <summary>
        /// Indicates that this trace works in the runtime version of .NET.
        /// </summary>
        public static bool WorksInThisRuntime { get; private set; }

        private static Type s_timerQueueType;
        private static MethodInfo s_timerQueueInstanceMethod;
        private static FieldInfo s_timerQueueTimersField;

        private static Type s_timerQueueTimerType;
        private static FieldInfo s_timerQueueTimerTimerCallbackField;
        private static FieldInfo s_timerQueueTimerNextField;

        private static Type s_timerType;
        private static FieldInfo s_timerOnIntervalElapsedField;

        static TimerTrace()
        {
            WorksInThisRuntime = false;

            try
            {
                s_timerQueueType = Type.GetType("System.Threading.TimerQueue");
                s_timerQueueInstanceMethod = s_timerQueueType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetMethod;
                s_timerQueueTimersField = s_timerQueueType?.GetField("m_timers", BindingFlags.NonPublic | BindingFlags.Instance);

                s_timerQueueTimerType = Type.GetType("System.Threading.TimerQueueTimer");
                s_timerQueueTimerTimerCallbackField = s_timerQueueTimerType?.GetField("m_timerCallback", BindingFlags.NonPublic | BindingFlags.Instance);
                s_timerQueueTimerNextField = s_timerQueueTimerType?.GetField("m_next", BindingFlags.NonPublic | BindingFlags.Instance);

                s_timerType = typeof(System.Timers.Timer);
                s_timerOnIntervalElapsedField = s_timerType?.GetField("onIntervalElapsed", BindingFlags.NonPublic | BindingFlags.Instance);
                WorksInThisRuntime = true;
            }
            catch (Exception ex)
            {
                Log.Publish(MessageLevel.Error, MessageFlags.BugReport, "Error in constructor", null, null, ex);
            }

        }

        /// <summary>
        /// Gets all of the callbacks for all timers.
        /// </summary>
        /// <param name="sb"></param>
        public static void GetTrace(StringBuilder sb)
        {
            if (!WorksInThisRuntime)
            {
                sb.AppendLine("Not Supported");
                return;
            }

            try
            {

                object timerQueue = s_timerQueueInstanceMethod.Invoke(null, null);
                lock (timerQueue)
                {
                    object timerQueueTimer = s_timerQueueTimersField.GetValue(timerQueue);

                    while (timerQueueTimer != null)
                    {
                        ProcessTimerQueueTimer(sb, timerQueueTimer);
                        timerQueueTimer = s_timerQueueTimerNextField.GetValue(timerQueueTimer);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Publish(MessageLevel.Error, MessageFlags.BugReport, "Error in GetTrace", null, null, ex);

                WorksInThisRuntime = false;
            }
        }


        private static void ProcessTimerQueueTimer(StringBuilder sb, object state)
        {
            TimerCallback timerCallback = (TimerCallback)s_timerQueueTimerTimerCallbackField.GetValue(state);

            if (TryProcessTimersTimerCallback(sb, timerCallback))
            {

            }
            else
            {
                sb.AppendLine(timerCallback.Method.GetFriendlyMethodNameWithClass());
            }
            return;
        }

        private static bool TryProcessTimersTimerCallback(StringBuilder sb, TimerCallback timerCallback)
        {
            if (timerCallback.Target != null && timerCallback.Target.GetType() == s_timerType)
            {
                System.Timers.Timer t = timerCallback.Target as System.Timers.Timer;
                ElapsedEventHandler onIntervalElapsed = (ElapsedEventHandler)s_timerOnIntervalElapsedField.GetValue(t);
                sb.AppendLine(onIntervalElapsed.Method.GetFriendlyMethodNameWithClass());
                return true;
            }
            return false;
        }
    }
}
