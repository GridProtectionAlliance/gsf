//******************************************************************************************************
//  SynchronizedTask.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/19/2026 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GSF.Core.Threading
{
    /// <summary>
    /// Represents a task that cannot run while it is already in progress.
    /// </summary>
    /// <typeparam name="T">The type of object returned by the task when it executes.</typeparam>
    public class SynchronizedTask<T>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SynchronizedTask{T}"/> class.
        /// </summary>
        /// <param name="callback">The callback to be executed when the task runs</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> is null</exception>
        public SynchronizedTask(Func<Task<T>> callback)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            Callback = callback;
        }

        /// <summary>
        /// Gets a value to indicate whether the synchronized
        /// task is currently executing its callback.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (TaskLock)
                    return RunningTask is not null;
            }
        }

        /// <summary>
        /// Gets a value to indiate whether the synchronized task
        /// has an additional callback that is pending execution after
        /// the currently running task has completed.
        /// </summary>
        public bool IsPending
        {
            get
            {
                lock (TaskLock)
                    return PendingTask is not null;
            }
        }

        private Func<Task<T>> Callback { get; }

        private object TaskLock { get; } = new();
        private Task<T> LastCompletedTask { get; set; }
        private Task<T> RunningTask { get; set; }
        private Task<T> PendingTask { get; set; }

        /// <summary>
        /// Executes the callback on another thread or marks the
        /// task as pending if the callback is already running.
        /// </summary>
        /// <returns>A task that completes when the callback completes.</returns>
        public Task<T> RunAsync()
        {
            lock (TaskLock)
            {
                if (RunningTask is null)
                {
                    RunningTask = Task.Run(ExecuteCallback);
                    return RunningTask;
                }

                if (PendingTask is null)
                {
                    Task<T> runningTask = RunningTask;

                    PendingTask = Task.Run(async () =>
                    {
                        await runningTask.ConfigureAwait(false);
                        return await ExecuteCallback().ConfigureAwait(false);
                    });
                }

                return PendingTask;
            }
        }

        /// <summary>
        /// Returns a task that completes when the currently
        /// running and pending tasks are both complete.
        /// </summary>
        /// <returns>A task instance.</returns>
        public async Task FlushAsync()
        {
            Task pendingTask;

            lock (TaskLock)
                pendingTask = PendingTask ?? RunningTask ?? Task.CompletedTask;

            try { await pendingTask; }
            catch { /* Ignore errors when flushing */ }
        }

        /// <summary>
        /// Returns an awaiter used to await the most recently completed callback.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        public TaskAwaiter<T> GetAwaiter()
        {
            lock (TaskLock)
            {
                Task<T> task = LastCompletedTask ?? RunningTask ?? RunAsync();
                return task.GetAwaiter();
            }
        }

        /// <summary>
        /// Configures an awaiter used to await the most recently completed callback.
        /// </summary>
        /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false</param>
        /// <returns>An object used to await the most recently completed callback.</returns>
        public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            lock (TaskLock)
            {
                Task<T> task = LastCompletedTask ?? RunningTask ?? RunAsync();
                return task.ConfigureAwait(continueOnCapturedContext);
            }
        }

        private async Task<T> ExecuteCallback()
        {
            try
            {
                return await Callback().ConfigureAwait(false);
            }
            finally
            {
                lock (TaskLock)
                {
                    LastCompletedTask = RunningTask;
                    RunningTask = PendingTask;
                    PendingTask = null;
                }
            }
        }
    }
}
