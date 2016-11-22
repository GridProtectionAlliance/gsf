//******************************************************************************************************
//  Logger.cs - Gbtc
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
//  10/24/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using GSF.Threading;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Manages the collection and reporting of logging information in a system.
    /// </summary>
    public static class Logger
    {
        private static LoggerInternal s_logger;

        /// <summary>
        /// The default console based log subscriber.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly LogSubscriptionConsole Console;
        /// <summary>
        /// The default file based log writer.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly LogSubscriptionFileWriter FileWriter;

        private static readonly ConcurrentDictionary<int, int> LogSuppressionChecking = new ConcurrentDictionary<int, int>();
        private static readonly Dictionary<int, List<LogStackMessages>> ThreadStackDetails = new Dictionary<int, List<LogStackMessages>>();

        private static readonly LogPublisher Log;
        private static readonly LogEventPublisher EventFirstChanceException;
        private static readonly LogEventPublisher EventAppDomainException;

        static Logger()
        {
            //Initializes the empty object of StackTraceDetails
            LogStackTrace.Initialize();
            LogStackMessages.Initialize();

            s_logger = new LoggerInternal();
            Console = new LogSubscriptionConsole();
            FileWriter = new LogSubscriptionFileWriter(1000);

            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Log = CreatePublisher(typeof(Logger), MessageClass.Component);
            Log.InitialStackTrace = LogStackTrace.Empty;
            EventFirstChanceException = Log.RegisterEvent(MessageLevel.NA, MessageFlags.SystemHealth, "First Chance App Domain Exception", 30, MessageRate.PerSecond(30), 1000);
            EventAppDomainException = Log.RegisterEvent(MessageLevel.Critical, MessageFlags.SystemHealth, "Unhandled App Domain Exception");

            ShutdownHandler.Initialize();
        }

        /// <summary>
        /// Gets if Log Messages should be suppressed.
        /// </summary>
        public static bool ShouldSuppressLogMessages => LogSuppressionChecking.ContainsKey(Thread.CurrentThread.ManagedThreadId);

        /// <summary>
        /// Ensures that the logger has been initialized. 
        /// </summary>
        internal static void Initialize()
        {
            //Handled in the static constructor.
        }

        /// <summary>
        /// Ensures that the logger is properly shutdown.
        /// This is called from ShutdownHandler.
        /// </summary>
        internal static void Shutdown()
        {
            try
            {
                s_logger.Dispose();
                Console.Verbose = VerboseLevel.None;
                FileWriter.Dispose();
            }
            catch (Exception)
            {
            }
        }

        static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (ShouldSuppressLogMessages)
                return;
            using (SuppressLogMessages())
            {
                try
                {
                    var perm = new ReflectionPermission(PermissionState.Unrestricted);
                    perm.Demand();
                }
                catch (SecurityException)
                {
                    //Cannot raise messages if this permission is denied.
                    return;
                }

                try
                {
                    EventFirstChanceException.Publish(null, null, e.Exception);
                }
                catch (Exception)
                {
                    //swallow any exceptions.
                }
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (ShouldSuppressLogMessages)
                return;
            using (SuppressLogMessages())
            {
                try
                {
                    var perm = new ReflectionPermission(PermissionState.Unrestricted);
                    perm.Demand();
                }
                catch (SecurityException)
                {
                    //Cannot raise messages if this permission is denied.
                    return;
                }
                EventAppDomainException.Publish(null, null, e.ExceptionObject as Exception);
            }
        }

        /// <summary>
        /// Looks up the type of the log source
        /// </summary>
        /// <param name="type">the type</param>
        /// <param name="classification">the classification of the type of messages that this publisher will raise.</param>
        /// <returns></returns>
        public static LogPublisher CreatePublisher(Type type, MessageClass classification)
        {
            return new LogPublisher(s_logger.CreateType(type), classification);
        }

        /// <summary>
        /// Creates a <see cref="LogSubscriber"/>
        /// </summary>
        /// <returns></returns>
        public static LogSubscriber CreateSubscriber(VerboseLevel level = VerboseLevel.None)
        {
            var subscriber = new LogSubscriber(s_logger.CreateSubscriber());
            subscriber.SubscribeToAll(level);
            return subscriber;
        }

        /// <summary>
        /// Searches the current stack frame for all related messages that will be published with this message.
        /// </summary>
        /// <returns></returns>
        public static LogStackMessages GetStackMessages()
        {
            int threadid = Thread.CurrentThread.ManagedThreadId;

            List<LogStackMessages> stack;

            lock (ThreadStackDetails)
            {
                if (!ThreadStackDetails.TryGetValue(threadid, out stack))
                {
                    return LogStackMessages.Empty;
                }
            }

            return new LogStackMessages(stack);
        }


        /// <summary>
        /// Temporarily appends data to the thread's stack so the data can be propagated to any messages generated on this thread.
        /// Be sure to call Dispose on the returned object to remove this from the stack.
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static StackDetailsDisposal AppendStackMessages(LogStackMessages messages)
        {
            int threadid = Thread.CurrentThread.ManagedThreadId;

            List<LogStackMessages> stack;

            lock (ThreadStackDetails)
            {
                if (!ThreadStackDetails.TryGetValue(threadid, out stack))
                {
                    stack = new List<LogStackMessages>();
                    ThreadStackDetails.Add(threadid, stack);
                }
            }

            stack.Add(messages);
            return new StackDetailsDisposal(stack.Count);
        }

        /// <summary>
        /// Sets a flag that will prevent log messages from being raised on this thread.
        /// Remember to dispose of the callback to remove this suppression.
        /// </summary>
        /// <returns></returns>
        public static SuppressLogMessagesDisposal SuppressLogMessages()
        {
            return new SuppressLogMessagesDisposal(LogSuppressionChecking.TryAdd(Thread.CurrentThread.ManagedThreadId, 1));
        }

        /// <summary>
        /// Temporarily appends data to the thread's stack so the data can be propagated to any messages generated on this thread.
        /// Be sure to call Dispose on the returned object to remove this from the stack.
        /// </summary>
        /// <returns></returns>
        public static StackDetailsDisposal AppendStackMessages(string key, string value)
        {
            return AppendStackMessages(new LogStackMessages(key, value));
        }

        /// <summary>
        /// When putting messages on the stack. This struct is returned. Be sure to dispose it.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public struct StackDetailsDisposal : IDisposable
        {
            /// <summary>
            /// The depth of the stack messages
            /// </summary>
            public int Depth { get; private set; }

            /// <summary>
            /// Creates a new StackDetailsDisposal
            /// </summary>
            /// <param name="depth">the stack depth</param>
            internal StackDetailsDisposal(int depth)
            {
                Depth = depth;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
                if (Depth == 0)
                    return;
                int threadid = Thread.CurrentThread.ManagedThreadId;

                if (Depth == 1)
                {
                    lock (ThreadStackDetails)
                    {
                        ThreadStackDetails.Remove(threadid);
                    }
                }
                else
                {
                    lock (ThreadStackDetails)
                    {
                        List<LogStackMessages> stack;
                        if (ThreadStackDetails.TryGetValue(threadid, out stack))
                        {
                            while (stack.Count >= Depth)
                            {
                                stack.RemoveAt(stack.Count - 1);
                            }
                        }
                    }
                }
                Depth = 0;
            }
        }

        /// <summary>
        /// When Suppressing Log Messages. This struct is returned. Be sure to dispose it.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public struct SuppressLogMessagesDisposal : IDisposable
        {
            internal bool ShouldRemove { get; private set; }

            internal SuppressLogMessagesDisposal(bool shouldRemove)
            {
                ShouldRemove = shouldRemove;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
                if (ShouldRemove)
                {
                    int threadid = Thread.CurrentThread.ManagedThreadId;
                    int value;
                    LogSuppressionChecking.TryRemove(threadid, out value);
                    ShouldRemove = false;
                }
            }
        }
    }



}
