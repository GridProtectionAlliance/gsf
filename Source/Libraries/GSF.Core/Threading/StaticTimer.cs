//******************************************************************************************************
//  StaticTimer.cs - Gbtc
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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GSF.Threading
{
    /// <summary>
    /// Represents a timer manager which registers callbacks
    /// with existing timers to reduce thread pool queue burden.
    /// </summary>
    public class StaticTimer
    {
        #region [ Members ]

        // Nested Types
        private class TimerAction
        {
            private WeakAction m_action;
            private CancellationToken m_cancellationToken;

            public TimerAction(Action action, CancellationToken cancellationToken)
            {
                m_action = new WeakAction(action);
                m_cancellationToken = cancellationToken;
            }

            public bool IsCancelled
            {
                get
                {
                    return m_cancellationToken.IsCancelled;
                }
            }

            public bool TryInvoke()
            {
                return m_action.TryInvoke();
            }
        }

        // Fields
        private readonly ConcurrentDictionary<int, ConcurrentQueue<TimerAction>> m_dataStreamMonitorActions;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="StaticTimer"/> class.
        /// </summary>
        public StaticTimer()
        {
            m_dataStreamMonitorActions = new ConcurrentDictionary<int, ConcurrentQueue<TimerAction>>();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Registers the given callback with the timer running at the given interval.
        /// </summary>
        /// <param name="interval">The interval at which to run the timer.</param>
        /// <param name="monitorAction">The action to be performed when the timer is triggered.</param>
        public ICancellationToken RegisterDataStreamMonitor(int interval, Action monitorAction)
        {
            CancellationToken cancellationToken = new CancellationToken();
            RegisterDataStreamMonitor(interval, new TimerAction(monitorAction, cancellationToken));
            return cancellationToken;
        }

        private void RegisterDataStreamMonitor(int interval, TimerAction monitorAction)
        {
            ConcurrentQueue<TimerAction> newQueue = null;
            ConcurrentQueue<TimerAction> queue;

            lock (m_dataStreamMonitorActions)
            {
                queue = m_dataStreamMonitorActions.GetOrAdd(interval, i => newQueue = new ConcurrentQueue<TimerAction>());
                queue.Enqueue(monitorAction);
            }

            if (queue == newQueue)
                StartTimer(interval, queue);
        }

        private void StartTimer(int interval, ConcurrentQueue<TimerAction> queue)
        {
            MonitorDataStream(interval, queue, new List<TimerAction>());
        }

        private void MonitorDataStream(int interval, ConcurrentQueue<TimerAction> queue, List<TimerAction> list)
        {
            TimerAction action;

            for (int i = 0; i < list.Count; i++)
            {
                action = list[i];

                // False here indicates that the
                // action has been garbage collected
                if (action.IsCancelled || !action.TryInvoke())
                {
                    int end = list.Count - 1;
                    list[i] = list[end];
                    list.RemoveAt(end);
                    i--;
                }
            }

            // The queue is only used to populate the
            // list from outside the async loop
            while (queue.TryDequeue(out action))
                list.Add(action);

            if (list.Count > 0)
            {
                // We still have callbacks in the list so continue the async loop
                new Action(() => MonitorDataStream(interval, queue, list)).DelayAndExecute(interval);
            }
            else
            {
                lock (m_dataStreamMonitorActions)
                {
                    // We are retiring this async loop so we need
                    // to remove its queue from the lookup table
                    m_dataStreamMonitorActions.TryRemove(interval, out queue);

                    // However, actions may have been queued up in the meantime
                    // so we need to re-register them with a new async loop
                    while (queue.TryDequeue(out action))
                        RegisterDataStreamMonitor(interval, action);
                }
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Default instance of the <see cref="StaticTimer"/> class.
        /// </summary>
        public static readonly StaticTimer Default = new StaticTimer();

        #endregion
    }
}
