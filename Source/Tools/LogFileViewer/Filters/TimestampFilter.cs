//******************************************************************************************************
//  TimestampFilter.cs - Gbtc
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
    internal class TimestampFilter : FilterBase
    {
        public enum Mode
        {
            Before,
            After,
            ExcludeInside,
            Outside,
        }

        private DateTime m_first;
        private DateTime m_second;
        private Mode m_mode;

        public TimestampFilter(DateTime first, DateTime second, Mode mode)
        {
            m_first = first;
            m_second = second;
            m_mode = mode;
        }

        public TimestampFilter(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 1:
                    m_first = stream.ReadDateTime();
                    m_second = stream.ReadDateTime();
                    m_mode = (Mode)stream.ReadNextByte();
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public override bool IsMatch(LogMessage log)
        {
            switch (m_mode)
            {
                case Mode.Before:
                    return log.UtcTime < m_first;
                case Mode.After:
                    return log.UtcTime > m_first;
                case Mode.ExcludeInside:
                    return m_first <= log.UtcTime && log.UtcTime <= m_second;
                case Mode.Outside:
                    return log.UtcTime < m_first || log.UtcTime > m_second;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void SaveInternal(Stream stream)
        {
            stream.Write((byte)1);
            stream.Write(m_first);
            stream.Write(m_second);
            stream.Write((byte)m_mode);
        }

        protected override string DescriptionInternal
        {
            get
            {
                switch (m_mode)
                {
                    case Mode.Before:
                        return "Before: " + m_first.ToLocalTime();
                    case Mode.After:
                        return "After: " + m_first.ToLocalTime();
                    case Mode.ExcludeInside:
                        return "Between: " + m_first.ToLocalTime() + " and " + m_second.ToLocalTime();
                    case Mode.Outside:
                        return "Outside: " + m_first.ToLocalTime() + " and " + m_second.ToLocalTime();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}