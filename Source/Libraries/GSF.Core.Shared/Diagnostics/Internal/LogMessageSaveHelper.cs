//******************************************************************************************************
//  LogMessageSaveHelper.cs - Gbtc
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
using System.Data;
using System.IO;
using GSF.IO;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Assists in the saving of a LogMessage. This class is here to help de-duplicate classes so they don't take so much 
    /// space or memory.
    /// </summary>
    internal class LogMessageSaveHelper
    {
        private static readonly LogMessageSaveHelper Simple = new LogMessageSaveHelper(true);

        public static LogMessageSaveHelper Create(bool isSimple = false)
        {
            if (isSimple)
                return Simple;
            return new LogMessageSaveHelper(false);
        }

        private bool m_isSimple;

        private Dictionary<LogEventPublisherDetails, int> m_ownerSaveLookup;
        private List<LogEventPublisherDetails> m_ownerLoadLookup;

        private Dictionary<LogStackMessages, int> m_stackMessagesSaveLookup;
        private List<LogStackMessages> m_stackMessagesLoadLookup;

        private Dictionary<LogStackTrace, int> m_stackTraceSaveLookup;
        private List<LogStackTrace> m_stackTraceLoadLookup;

        private Dictionary<PublisherTypeDefinition, int> m_publisherTypeDefinitionSaveLookup;
        private List<PublisherTypeDefinition> m_publisherTypeDefinitionLoadLookup;

        private Dictionary<string, int> m_stringSaveLookup;
        private List<string> m_stringLoadLookup;

        private LogMessageSaveHelper(bool isSimple)
        {
            if (isSimple)
            {
                m_isSimple = true;
            }
            else
            {
                m_isSimple = false;
                m_ownerSaveLookup = new Dictionary<LogEventPublisherDetails, int>();
                m_ownerLoadLookup = new List<LogEventPublisherDetails>();
                m_stackMessagesSaveLookup = new Dictionary<LogStackMessages, int>();
                m_stackMessagesLoadLookup = new List<LogStackMessages>();
                m_stackTraceSaveLookup = new Dictionary<LogStackTrace, int>();
                m_stackTraceLoadLookup = new List<LogStackTrace>();
                m_publisherTypeDefinitionSaveLookup = new Dictionary<PublisherTypeDefinition, int>();
                m_publisherTypeDefinitionLoadLookup = new List<PublisherTypeDefinition>();
                m_stringSaveLookup = new Dictionary<string, int>();
                m_stringLoadLookup = new List<string>();
            }
        }

        #region [ LogMessageOwner ]

        public void SaveEventPublisherDetails(Stream stream, LogEventPublisherDetails publisherDetails)
        {
            if (publisherDetails == null)
                throw new ArgumentNullException(nameof(publisherDetails));

            if (m_isSimple)
            {
                stream.Write((byte)0);
                publisherDetails.Save(stream, this);
            }
            else
            {
                int lookupId;
                if (m_ownerSaveLookup.TryGetValue(publisherDetails, out lookupId))
                {
                    stream.Write((byte)1);
                    stream.Write(lookupId);
                }
                else
                {
                    lookupId = m_ownerSaveLookup.Count;
                    stream.Write((byte)3);
                    publisherDetails.Save(stream, this);
                    m_ownerSaveLookup.Add(publisherDetails, lookupId);
                }
            }
        }

        public LogEventPublisherDetails LoadEventPublisherDetails(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 0:
                    return new LogEventPublisherDetails(stream, this);
                case 1:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        int id = stream.ReadInt32();
                        return m_ownerLoadLookup[id];
                    }
                case 2:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        int id = stream.ReadInt32();
                        var details = new LogEventPublisherDetails(stream, this);
                        m_ownerLoadLookup.Add(details);
                        return details;
                    }
                case 3:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        var details = new LogEventPublisherDetails(stream, this);
                        m_ownerLoadLookup.Add(details);
                        return details;
                    }
                default:
                    throw new VersionNotFoundException();
            }
        }

        #endregion

        #region [ PublisherTypeDefinition ]

        public void SavePublisherTypeDefinition(Stream stream, PublisherTypeDefinition publisherDetails)
        {
            if (publisherDetails == null)
                throw new ArgumentNullException(nameof(publisherDetails));

            if (m_isSimple)
            {
                stream.Write((byte)0);
                publisherDetails.Save(stream);
            }
            else
            {
                int lookupId;
                if (m_publisherTypeDefinitionSaveLookup.TryGetValue(publisherDetails, out lookupId))
                {
                    stream.Write((byte)1);
                    stream.Write(lookupId);
                }
                else
                {
                    lookupId = m_publisherTypeDefinitionSaveLookup.Count;
                    stream.Write((byte)3);
                    publisherDetails.Save(stream);
                    m_publisherTypeDefinitionSaveLookup.Add(publisherDetails, lookupId);
                }
            }
        }

        public PublisherTypeDefinition LoadPublisherTypeDefinition(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 0:
                    return new PublisherTypeDefinition(stream);
                case 1:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        int id = stream.ReadInt32();
                        return m_publisherTypeDefinitionLoadLookup[id];
                    }
                case 2:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        int id = stream.ReadInt32();
                        var details = new PublisherTypeDefinition(stream);
                        m_publisherTypeDefinitionLoadLookup.Add(details);
                        return details;
                    }
                case 3:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        var details = new PublisherTypeDefinition(stream);
                        m_publisherTypeDefinitionLoadLookup.Add(details);
                        return details;
                    }
                default:
                    throw new VersionNotFoundException();
            }
        }

        #endregion

        #region [ StackMessages ]

        public void SaveStackMessages(Stream stream, LogStackMessages message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (ReferenceEquals(message, LogStackMessages.Empty))
            {
                stream.Write((byte)0);
            }
            else if (m_isSimple)
            {
                stream.Write((byte)1);
                message.Save(stream);
            }
            else
            {
                int lookupId;
                if (m_stackMessagesSaveLookup.TryGetValue(message, out lookupId))
                {
                    stream.Write((byte)2);
                    stream.Write(lookupId);
                }
                else
                {
                    lookupId = m_stackMessagesSaveLookup.Count;
                    stream.Write((byte)4);
                    message.Save(stream);
                    m_stackMessagesSaveLookup.Add(message, lookupId);
                }
            }
        }

        public LogStackMessages LoadStackMessages(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 0:
                    return LogStackMessages.Empty;
                case 1:
                    return new LogStackMessages(stream);
                case 2:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        int id = stream.ReadInt32();
                        return m_stackMessagesLoadLookup[id];
                    }
                case 3:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        int id = stream.ReadInt32();
                        var messages = new LogStackMessages(stream);
                        m_stackMessagesLoadLookup.Add(messages);
                        return messages;
                    }
                case 4:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        var messages = new LogStackMessages(stream);
                        m_stackMessagesLoadLookup.Add(messages);
                        return messages;
                    }
                default:
                    throw new VersionNotFoundException();
            }
        }

        #endregion

        #region [ StackTrace ]

        public void SaveStackTrace(Stream stream, LogStackTrace trace)
        {
            if (trace == null)
                throw new ArgumentNullException(nameof(trace));

            if (ReferenceEquals(trace, LogStackTrace.Empty))
            {
                stream.Write((byte)0);
            }
            else if (m_isSimple)
            {
                stream.Write((byte)1);
                trace.Save(stream);
            }
            else
            {
                int lookupId;
                if (m_stackTraceSaveLookup.TryGetValue(trace, out lookupId))
                {
                    stream.Write((byte)2);
                    stream.Write(lookupId);
                }
                else
                {
                    lookupId = m_stackTraceSaveLookup.Count;
                    stream.Write((byte)4);
                    trace.Save(stream);
                    m_stackTraceSaveLookup.Add(trace, lookupId);
                }
            }
        }

        public LogStackTrace LoadStackTrace(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 0:
                    return LogStackTrace.Empty;
                case 1:
                    return new LogStackTrace(stream);
                case 2:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        int id = stream.ReadInt32();
                        return m_stackTraceLoadLookup[id];
                    }
                case 3:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        int id = stream.ReadInt32();
                        var trace = new LogStackTrace(stream);
                        m_stackTraceLoadLookup.Add(trace);
                        return trace;
                    }
                case 4:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        var trace = new LogStackTrace(stream);
                        m_stackTraceLoadLookup.Add(trace);
                        return trace;
                    }
                default:
                    throw new VersionNotFoundException();
            }
        }

        #endregion

        #region [ String ]

        public void SaveString(Stream stream, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
            {
                stream.Write((byte)3);
            }
            else if (m_isSimple)
            {
                stream.Write((byte)0);
                stream.Write(value);
            }
            else
            {
                int lookupId;

                if (m_stringSaveLookup.TryGetValue(value, out lookupId))
                {
                    stream.Write((byte)1);
                    stream.Write(lookupId);
                }
                else
                {
                    lookupId = m_stringSaveLookup.Count;
                    stream.Write((byte)4);
                    stream.Write(value);
                    m_stringSaveLookup.Add(value, lookupId);
                }
            }
        }

        public string LoadString(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 0:
                    return stream.ReadString();
                case 1:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        int id = stream.ReadInt32();
                        return m_stringLoadLookup[id];
                    }
                case 2:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        int id = stream.ReadInt32();
                        var value = stream.ReadString();
                        m_stringLoadLookup.Add(value);
                        return value;
                    }
                case 3:
                    {
                        return string.Empty;
                    }
                case 4:
                    {
                        if (m_isSimple)
                            throw new Exception("Cannot load without a LogMessageSaveHelper");
                        var value = stream.ReadString();
                        m_stringLoadLookup.Add(value);
                        return value;
                    }
                default:
                    throw new VersionNotFoundException();
            }
        }

        #endregion

    }
}
