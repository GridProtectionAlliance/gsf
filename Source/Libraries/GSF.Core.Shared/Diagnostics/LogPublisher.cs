//******************************************************************************************************
//  LogPublisher.cs - Gbtc
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

// ReSharper disable InconsistentlySynchronizedField

namespace GSF.Diagnostics
{
    /// <summary>
    /// A publisher of log messages. 
    /// </summary>
    /// <remarks>
    /// <see cref="InitialStackMessages"/> and <see cref="InitialStackTrace"/> can be modified so messages that are generated 
    /// with this instance will have this data appended to the log message.
    /// 
    /// The user can either call one of the Publish overloads to lazily publish a message, or they
    /// can register a message with RegisterEvent so calling this message will incur little overhead.
    /// If registering an event, the user can check <see cref="LogEventPublisher.HasSubscribers"/> to determine if the log message can be skipped altogether. 
    /// Registering events also allows the user to specify the auto-suppression algorithm and the depth of the stack trace that will be recorded on a message being raised.
    /// 
    /// </remarks>
    public class LogPublisher
    {
        /// <summary>
        /// Occurs when a new <see cref="LogMessage"/> is ready to be published.
        /// </summary>
        private LoggerInternal m_logger;

        private readonly LogPublisherInternal m_publisherInstance;
        private readonly MessageClass m_classification;

        /// <summary>
        /// The stack messages that existed when this publisher was created. This can be modified by the user of this publisher.
        /// Any messages that get published by this class will automatically have this data added to the log message.
        /// </summary>
        public LogStackMessages InitialStackMessages;

        /// <summary>
        /// The stack trace that existed when this publisher was created. This can be modified by the user of this publisher.
        /// Any messages that get published by this class will automatically have this data added to the log message.
        /// </summary>
        public LogStackTrace InitialStackTrace;

        /// <summary>
        /// The maximum number of distinct events that this publisher can generate. (Default: 20)
        /// </summary>
        /// <remarks>
        /// Since message suppression and collection occurs at the event name level, it is important
        /// to have only a few distinct message types. This is the limit so misapplication
        /// of this publisher will not cause memory impacts on the system.
        /// 
        /// It is recommended to keep the event name as a fixed string and not report any other meta data
        /// with the event.
        /// </remarks>
        // ReSharper disable once ConvertToConstant.Global
        public int MaxDistinctEventPublisherCount { get; set; }

        /// <summary>
        /// Where the <see cref="LogEventPublisherInternal"/>s of specific events are cached.
        /// </summary>
        private readonly ConcurrentDictionary<Tuple<LogMessageAttributes, string>, LogEventPublisherInternal> m_lookupEventPublishers;

        private LogEventPublisherInternal m_excessivePublisherEventNames;

        internal LogPublisher(LoggerInternal logger, LogPublisherInternal publisherInstance, MessageClass classification)
        {
            m_logger = logger;
            m_publisherInstance = publisherInstance;
            m_classification = classification;
            InitialStackMessages = Logger.GetStackMessages();
            InitialStackTrace = new LogStackTrace(true, 1, 10);
            MaxDistinctEventPublisherCount = 20;
            m_lookupEventPublishers = new ConcurrentDictionary<Tuple<LogMessageAttributes, string>, LogEventPublisherInternal>();

        }

        /// <summary>
        /// Initializes an <see cref="LogEventPublisher"/> with the provided values.
        /// </summary>
        /// <param name="level">the level of the message and associated flags if any</param>
        /// <param name="eventName">the name of the event.</param>
        /// <returns></returns>
        public LogEventPublisher RegisterEvent(MessageLevel level, string eventName)
        {
            LogMessageAttributes flag = new LogMessageAttributes(m_classification, level, MessageSuppression.None, MessageFlags.None);
            LogEventPublisherInternal publisher = InternalRegisterEvent(flag, eventName);
            return new LogEventPublisher(this, publisher);
        }

        /// <summary>
        /// Initializes an <see cref="LogEventPublisher"/> with the provided values.
        /// </summary>
        /// <param name="level">the level of the message</param>
        /// <param name="flags">associated flags</param>
        /// <param name="eventName">the name of the event.</param>
        /// <returns></returns>
        public LogEventPublisher RegisterEvent(MessageLevel level, MessageFlags flags, string eventName)
        {
            LogMessageAttributes flag = new LogMessageAttributes(m_classification, level, MessageSuppression.None, flags);
            LogEventPublisherInternal publisher = InternalRegisterEvent(flag, eventName);
            return new LogEventPublisher(this, publisher);
        }

        /// <summary>
        /// Initializes an <see cref="LogEventPublisher"/> with the provided values.
        /// </summary>
        /// <param name="level">the level of the message</param>
        /// <param name="eventName"></param>
        /// <param name="stackTraceDepth"></param>
        /// <param name="messagesPerSecond"></param>
        /// <param name="burstLimit"></param>
        /// <returns></returns>
        public LogEventPublisher RegisterEvent(MessageLevel level, string eventName, int stackTraceDepth, MessageRate messagesPerSecond, int burstLimit)
        {
            LogMessageAttributes flag = new LogMessageAttributes(m_classification, level, MessageSuppression.None, MessageFlags.None);
            LogEventPublisherInternal publisher = InternalRegisterEvent(flag, eventName, stackTraceDepth, messagesPerSecond, burstLimit);
            return new LogEventPublisher(this, publisher);
        }

        /// <summary>
        /// Initializes an <see cref="LogEventPublisher"/> with the provided values.
        /// </summary>
        /// <param name="level">the level of the message</param>
        /// <param name="flags">associated flags</param>
        /// <param name="eventName"></param>
        /// <param name="stackTraceDepth"></param>
        /// <param name="messagesPerSecond"></param>
        /// <param name="burstLimit"></param>
        /// <returns></returns>
        public LogEventPublisher RegisterEvent(MessageLevel level, MessageFlags flags, string eventName, int stackTraceDepth, MessageRate messagesPerSecond, int burstLimit)
        {
            LogMessageAttributes flag = new LogMessageAttributes(m_classification, level, MessageSuppression.None, flags);
            LogEventPublisherInternal publisher = InternalRegisterEvent(flag, eventName, stackTraceDepth, messagesPerSecond, burstLimit);
            return new LogEventPublisher(this, publisher);
        }

        /// <summary>
        /// Gets the full name of the type.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_publisherInstance.TypeFullName + " (" + m_publisherInstance.AssemblyFullName + ")";
        }

        /// <summary>
        /// Raises a log message with the provided data.
        /// </summary>
        /// <param name="level">the level of the message</param>
        /// <param name="eventName">A short name about what this message is detailing. Typically this will be a few words.</param>
        /// <param name="message"> A longer message than <see param="eventName"/> giving more specifics about the actual message. 
        /// Typically, this will be up to 1 line of text.</param>
        /// <param name="details">A long text field with the details of the message.</param>
        /// <param name="exception">An exception object if one is provided.</param>
        public void Publish(MessageLevel level, string eventName, string message = null, string details = null, Exception exception = null)
        {
            LogMessageAttributes flag = new LogMessageAttributes(m_classification, level, MessageSuppression.None, MessageFlags.None);
            InternalRegisterEvent(flag, eventName).Publish(message, details, exception, InitialStackMessages, InitialStackTrace);
        }

        /// <summary>
        /// Raises a log message with the provided data.
        /// </summary>
        /// <param name="level">the level of the message</param>
        /// <param name="flags">associated flags</param>
        /// <param name="eventName">A short name about what this message is detailing. Typically this will be a few words.</param>
        /// <param name="message"> A longer message than <see param="eventName"/> giving more specifics about the actual message. 
        /// Typically, this will be up to 1 line of text.</param>
        /// <param name="details">A long text field with the details of the message.</param>
        /// <param name="exception">An exception object if one is provided.</param>
        public void Publish(MessageLevel level, MessageFlags flags, string eventName, string message = null, string details = null, Exception exception = null)
        {
            LogMessageAttributes flag = new LogMessageAttributes(m_classification, level, MessageSuppression.None, flags);
            InternalRegisterEvent(flag, eventName).Publish(message, details, exception, InitialStackMessages, InitialStackTrace);
        }


        /// <summary>
        /// Initializes an <see cref="LogEventPublisher"/> with a series of settings.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="eventName">the name of the event.</param>
        /// <returns></returns>
        private LogEventPublisherInternal InternalRegisterEvent(LogMessageAttributes attributes, string eventName)
        {
            if (eventName == null)
                eventName = string.Empty;
            LogEventPublisherInternal publisher;
            if (m_lookupEventPublishers.TryGetValue(Tuple.Create(attributes, eventName), out publisher))
            {
                return publisher;
            }

            //If messages events are unclassified allow a higher message throughput rate.
            double messagesPerSecond = 1;
            int burstRate = 20;
            if (eventName == string.Empty)
            {
                messagesPerSecond = 5;
                burstRate = 100;
            }
            return InternalRegisterNewEvent(attributes, eventName, 0, messagesPerSecond, burstRate);
        }

        /// <summary>
        /// Initializes an <see cref="LogEventPublisher"/> with a series of settings.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="eventName"></param>
        /// <param name="stackTraceDepth"></param>
        /// <param name="messagesPerSecond"></param>
        /// <param name="burstLimit"></param>
        /// <returns></returns>
        private LogEventPublisherInternal InternalRegisterEvent(LogMessageAttributes attributes, string eventName, int stackTraceDepth, MessageRate messagesPerSecond, int burstLimit)
        {
            if (eventName == null)
                eventName = string.Empty;
            LogEventPublisherInternal publisher;
            if (m_lookupEventPublishers.TryGetValue(Tuple.Create(attributes, eventName), out publisher))
            {
                return publisher;
            }
            return InternalRegisterNewEvent(attributes, eventName, stackTraceDepth, messagesPerSecond, burstLimit);
        }

        private LogEventPublisherInternal InternalRegisterNewEvent(LogMessageAttributes attributes, string eventName, int stackTraceDepth, double messagesPerSecond, int burstLimit)
        {
            //Note: A race condition can cause more then the maximum number of entries to exist, however, this is not a concern.
            if (m_lookupEventPublishers.Count > MaxDistinctEventPublisherCount)
            {
                if (m_excessivePublisherEventNames == null)
                {
                    var owner1 = new LogEventPublisherDetails(m_publisherInstance.TypeFullName, m_publisherInstance.AssemblyFullName,
                        "Excessive Event Names: Event names for this publisher has been limited to " + MaxDistinctEventPublisherCount.ToString() +
                        "Please adjust MaxDistinctEventPublisherCount if this is not a bug and this publisher can truly create this many publishers.");
                    m_excessivePublisherEventNames = new LogEventPublisherInternal(attributes, owner1, m_publisherInstance, m_logger, stackTraceDepth, messagesPerSecond, burstLimit);
                }
                return m_excessivePublisherEventNames;
            }

            LogEventPublisherInternal publisher;
            var owner = new LogEventPublisherDetails(m_publisherInstance.TypeFullName, m_publisherInstance.AssemblyFullName, eventName);
            publisher = new LogEventPublisherInternal(attributes, owner, m_publisherInstance, m_logger, stackTraceDepth, messagesPerSecond, burstLimit);
            publisher = m_lookupEventPublishers.GetOrAdd(Tuple.Create(attributes, eventName), publisher);
            return publisher;
        }

    }
}