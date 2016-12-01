//******************************************************************************************************
//  EventFilter.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  11/01/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data;
using System.IO;
using GSF.Diagnostics;
using GSF.IO;

namespace LogFileViewer.Filters
{
    internal class EventFilter : FilterBase
    {
        public enum Mode
        {
            EventOnly,
            WithType,
            WithAssembly,
            WithRelatedType,
        }

        private string m_eventname;
        private string[] m_matchingNames;
        private Mode m_mode;

        public EventFilter(Mode mode, string eventname, params string[] matchingNames)
        {
            m_eventname = eventname;
            m_matchingNames = matchingNames;
            m_mode = mode;
        }

        public EventFilter(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 1:
                    m_mode = (Mode)stream.ReadNextByte();
                    m_eventname = stream.ReadString();
                    m_matchingNames = new string[stream.ReadInt32()];
                    for (int x = 0; x < m_matchingNames.Length; x++)
                    {
                        m_matchingNames[x] = stream.ReadString();
                    }
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public override bool IsMatch(LogMessage log)
        {
            switch (m_mode)
            {
                case Mode.EventOnly:
                    return log.EventName == m_eventname;
                case Mode.WithType:
                    return log.EventName == m_eventname && m_matchingNames[0] == log.TypeName;
                case Mode.WithRelatedType:
                    if (log.EventName != m_eventname)
                        return false;
                    foreach (var item in m_matchingNames)
                    {
                        if (!(log.TypeName == item || log.RelatedTypes.Contains(item)))
                            return false;
                    }
                    return true;
                case Mode.WithAssembly:
                    return log.EventName == m_eventname && m_matchingNames[0] == log.AssemblyName;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void SaveInternal(Stream stream)
        {
            stream.Write((byte)1);
            stream.Write((byte)m_mode);
            stream.Write(m_eventname);
            stream.Write(m_matchingNames.Length);
            foreach (var name in m_matchingNames)
            {
                stream.Write(name);
            }
        }

        protected override string DescriptionInternal
        {
            get
            {

                switch (m_mode)
                {
                    case Mode.EventOnly:
                        return $"Event: {m_eventname}";
                    case Mode.WithType:
                        return $"Event: {m_eventname} (Type:{m_matchingNames[0]})";
                    case Mode.WithAssembly:
                        return $"Event: {m_eventname} (Assembly:{m_matchingNames[0]})";
                    case Mode.WithRelatedType:
                        return $"Event: {m_eventname} (Types:{string.Join(", ", m_matchingNames)})";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}