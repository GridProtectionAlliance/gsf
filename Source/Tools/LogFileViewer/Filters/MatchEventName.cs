//******************************************************************************************************
//  MatchEventName.cs - Gbtc
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
using System.Collections.Generic;
using System.Data;
using System.IO;
using GSF.IO;
using GSF.Diagnostics;

namespace LogFileViewer.Filters
{
    internal class MatchEventName : IMessageMatch
    {
        private string m_typeName;
        private string m_eventName;
        private bool m_includeIfMatched;

        public MatchEventName(LogMessage typeName)
        {
            m_typeName = typeName.EventPublisherDetails.TypeName;
            m_eventName = typeName.EventPublisherDetails.EventName;
        }

        public MatchEventName(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 1:
                    m_typeName = stream.ReadString();
                    m_eventName = stream.ReadString();
                    m_includeIfMatched = stream.ReadBoolean();
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public FilterType TypeCode => FilterType.Event;


        public void Save(Stream stream)
        {
            stream.Write((byte)1);
            stream.Write(m_typeName);
            stream.Write(m_eventName);
            stream.Write(m_includeIfMatched);
        }

        public bool IsIncluded(LogMessage log)
        {
            if (log.EventPublisherDetails.TypeName == m_typeName && log.EventPublisherDetails.EventName == m_eventName)
                return m_includeIfMatched;
            return !m_includeIfMatched;
        }

        public string Description
        {
            get
            {
                if (m_includeIfMatched)
                    return "Include if Event: " + m_eventName + "(" + m_typeName + ")";
                else
                    return "Exclude if Event: " + m_eventName + "(" + m_typeName + ")";
            }
        }

        public IEnumerable<Tuple<string, Func<bool>>> GetMenuButtons()
        {
            return new[]
                   {
                       Tuple.Create<string, Func<bool>>("Include Event", () => { m_includeIfMatched = true; return true;}),
                       Tuple.Create<string, Func<bool>>("Exclude Event", () => { m_includeIfMatched = false; return true;})
                   };
        }
        public void ToggleResult()
        {
            m_includeIfMatched = !m_includeIfMatched;
        }
    }
}