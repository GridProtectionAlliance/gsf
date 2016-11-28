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
        private enum SuppressionMode
        {
            None = 0,
            FirstChanceExceptionOnly = 1,
            AllMessages = 2,
        }

        /// <summary>
        /// This information is maintained in a ThreadLocal variable and is about 
        /// messages and log suppression applied at higher levels of the calling stack.
        /// </summary>
        private class ThreadStack
        {
            private List<LogStackMessages> m_threadStackDetails = new List<LogStackMessages>();
            private LogStackMessages m_stackMessageCache;
            private List<SuppressionMode> m_logMessageSuppressionStack = new List<SuppressionMode>();

            public bool ShouldSuppressLogMessages => m_logMessageSuppressionStack.Count > 0 && m_logMessageSuppressionStack[m_logMessageSuppressionStack.Count - 1] >= SuppressionMode.AllMessages;
            public bool ShouldSuppressFirstChanceLogMessages => m_logMessageSuppressionStack.Count > 0 && m_logMessageSuppressionStack[m_logMessageSuppressionStack.Count - 1] >= SuppressionMode.FirstChanceExceptionOnly;

            public LogStackMessages GetStackMessages()
            {
                if (m_stackMessageCache == null)
                {
                    if (m_threadStackDetails.Count == 0)
                    {
                        m_stackMessageCache = LogStackMessages.Empty;
                    }
                    else
                    {
                        m_stackMessageCache = new LogStackMessages(m_threadStackDetails);
                    }
                }
                return m_stackMessageCache;
            }

            public StackDisposal AppendStackMessages(LogStackMessages messages)
            {
                m_stackMessageCache = null;
                m_threadStackDetails.Add(messages);

                int depth = m_threadStackDetails.Count;
                if (depth >= s_stackDisposalStackMessages.Length)
                {
                    GrowStackDisposal(depth);
                }
                return s_stackDisposalStackMessages[depth];
            }

            public StackDisposal SuppressLogMessages(SuppressionMode suppressionMode)
            {
                m_logMessageSuppressionStack.Add(suppressionMode);
                int depth = m_logMessageSuppressionStack.Count;
                if (depth >= s_stackDisposalSuppressionFlags.Length)
                {
                    GrowStackDisposal(depth);
                }
                return s_stackDisposalSuppressionFlags[depth];
            }

            public void RemoveStackMessage(int depth)
            {
                while (m_threadStackDetails.Count >= depth)
                {
                    m_threadStackDetails.RemoveAt(m_threadStackDetails.Count - 1);
                }
                m_stackMessageCache = null;
            }

            public void RemoveSuppression(int depth)
            {
                while (m_logMessageSuppressionStack.Count >= depth)
                {
                    m_logMessageSuppressionStack.RemoveAt(m_logMessageSuppressionStack.Count - 1);
                }
            }
        }

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

        private static readonly ThreadLocal<ThreadStack> ThreadItems = new ThreadLocal<ThreadStack>(() => new ThreadStack());

        private static readonly LogPublisher Log;
        private static readonly LogEventPublisher EventFirstChanceException;
        private static readonly LogEventPublisher EventAppDomainException;
        private static StackDisposal[] s_stackDisposalStackMessages;
        private static StackDisposal[] s_stackDisposalSuppressionFlags;
        private static readonly object SyncRoot = new object();

        static Logger()
        {
            //Initializes the empty object of StackTraceDetails
            LogStackTrace.Initialize();
            LogStackMessages.Initialize();
            GrowStackDisposal(1);

            s_logger = new LoggerInternal(out s_logger);
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
                Log.Publish(MessageLevel.Critical, MessageFlags.SystemHealth, "Logger is shutting down.");
                s_logger.Dispose();
                Console.Verbose = VerboseLevel.None;
                FileWriter.Dispose(); 
                //Cannot raise log messages here since the logger is now shutdown.
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Gets if Log Messages should be suppressed.
        /// </summary>
        public static bool ShouldSuppressLogMessages => ThreadItems.Value.ShouldSuppressLogMessages;

        /// <summary>
        /// Gets if First Chance Exception Log Messages should be suppressed.
        /// </summary>
        public static bool ShouldSuppressFirstChanceLogMessages => ThreadItems.Value.ShouldSuppressFirstChanceLogMessages;

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if ((Thread.CurrentThread.ThreadState & (ThreadState.AbortRequested | ThreadState.Aborted)) != 0)
            {
                return;
            }
            if (ShouldSuppressFirstChanceLogMessages)
                return;
            using (SuppressFirstChanceExceptionLogMessages())
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

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if ((Thread.CurrentThread.ThreadState & (ThreadState.AbortRequested | ThreadState.Aborted)) != 0)
            {
                return;
            }
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
            return new LogPublisher(s_logger, s_logger.CreateType(type), classification);
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
            return ThreadItems.Value.GetStackMessages();
        }

        /// <summary>
        /// Temporarily appends data to the thread's stack so the data can be propagated to any messages generated on this thread.
        /// Be sure to call Dispose on the returned object to remove this from the stack.
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static IDisposable AppendStackMessages(LogStackMessages messages)
        {
            return ThreadItems.Value.AppendStackMessages(messages);
        }

        /// <summary>
        /// Sets a flag that will prevent log messages from being raised on this thread.
        /// Remember to dispose of the callback to remove this suppression.
        /// </summary>
        /// <returns></returns>
        public static IDisposable SuppressLogMessages()
        {
            return ThreadItems.Value.SuppressLogMessages(SuppressionMode.AllMessages);
        }

        /// <summary>
        /// Sets a flag that will prevent First Chance Exception log messages from being raised on this thread.
        /// Remember to dispose of the callback to remove this suppression.
        /// </summary>
        /// <returns></returns>
        public static IDisposable SuppressFirstChanceExceptionLogMessages()
        {
            return ThreadItems.Value.SuppressLogMessages(SuppressionMode.FirstChanceExceptionOnly);
        }

        /// <summary>
        /// Sets a flag that will allow log messages to be raised again.
        /// Remember to dispose of the callback to remove this override.
        /// </summary>
        /// <returns></returns>
        public static IDisposable OverrideSuppressLogMessages()
        {
            return ThreadItems.Value.SuppressLogMessages(SuppressionMode.None);
        }

        /// <summary>
        /// Temporarily appends data to the thread's stack so the data can be propagated to any messages generated on this thread.
        /// Be sure to call Dispose on the returned object to remove this from the stack.
        /// </summary>
        /// <returns></returns>
        public static IDisposable AppendStackMessages(string key, string value)
        {
            return AppendStackMessages(new LogStackMessages(key, value));
        }

        private static void GrowStackDisposal(int desiredSize)
        {
            //Since these depths are relatively small, growing them both together has minor consequence.
            lock (SyncRoot)
            {
                while (s_stackDisposalStackMessages == null || s_stackDisposalStackMessages.Length < desiredSize)
                {
                    //Note: both are grown together and completely reinitialized to improve 
                    //      locality of reference.
                    int lastSize = s_stackDisposalStackMessages?.Length ?? 2;
                    StackDisposal[] stackMessages = new StackDisposal[lastSize * 2];
                    for (int x = 0; x < stackMessages.Length; x++)
                    {
                        stackMessages[x] = new StackDisposal(x, DisposeStackMessage);
                    }
                    StackDisposal[] suppressionFlags = new StackDisposal[lastSize * 2];
                    for (int x = 0; x < suppressionFlags.Length; x++)
                    {
                        suppressionFlags[x] = new StackDisposal(x, DisposeSuppressionFlags);
                    }
                    s_stackDisposalStackMessages = stackMessages;
                    s_stackDisposalSuppressionFlags = suppressionFlags;
                }
            }
        }

        private static void DisposeStackMessage(int depth)
        {
            ThreadItems.Value.RemoveStackMessage(depth);
        }
        private static void DisposeSuppressionFlags(int depth)
        {
            ThreadItems.Value.RemoveSuppression(depth);
        }

        /// <summary>
        /// A class that will undo a temporary change in the stack variables. Note, this class 
        /// will be reused. Therefore setting some kind of disposed flag will cause make this 
        /// class unusable. The side effect of multiple calls to Dispose is tolerable.
        /// </summary>
        private class StackDisposal : IDisposable
        {
            private readonly int m_depth;
            private readonly Action<int> m_callback;

            internal StackDisposal(int depth, Action<int> callback)
            {
                m_depth = depth;
                m_callback = callback;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
                m_callback(m_depth);
            }
        }
    }



}
