//******************************************************************************************************
//  MatchTimestamp.cs - Gbtc
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
using GSF.Diagnostics;
using GSF.IO;

namespace LogFileViewer.Filters
{
    internal class MatchTimestamp : IMessageMatch
    {
        private DateTime m_timestamp;
        private bool m_before;

        public MatchTimestamp(LogMessage typeName)
        {
            m_timestamp = typeName.UtcTime;
        }

        public MatchTimestamp(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 1:
                    m_timestamp = stream.ReadDateTime();
                    m_before = stream.ReadBoolean();
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public void Save(Stream stream)
        {
            stream.Write((byte)1);
            stream.Write(m_timestamp);
            stream.Write(m_before);
        }

        public FilterType TypeCode => FilterType.Timestamp;

        public bool IsIncluded(LogMessage log)
        {
            if (m_before)
            {
                return log.UtcTime >= m_timestamp;
            }
            else
            {
                return log.UtcTime <= m_timestamp;
            }
        }

        public string Description
        {
            get
            {
                if (m_before)
                {
                    return "Exclude if before: " + m_timestamp;
                }
                else
                {
                    return "Exclude if after: " + m_timestamp;
                }
            }
        }

        public IEnumerable<Tuple<string, Func<bool>>> GetMenuButtons()
        {
            return new[]
                   {
                       Tuple.Create<string, Func<bool>>("Exclude Before", () => { m_before = true; return true;}),
                       Tuple.Create<string, Func<bool>>("Exclude After", () => { m_before = false; return true;}),
                       Tuple.Create<string, Func<bool>>("Exclude 5 Minutes Before", () =>{m_before = true; m_timestamp = m_timestamp.AddMinutes(-5);return true;}),
                       Tuple.Create<string, Func<bool>>("Exclude 5 Minutes After", () =>{m_before = false; m_timestamp = m_timestamp.AddMinutes(5);return true;}),
                   };
        }
        public void ToggleResult()
        {
            m_before = !m_before;
        }
    }
}