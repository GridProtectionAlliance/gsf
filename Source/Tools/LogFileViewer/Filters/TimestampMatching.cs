//******************************************************************************************************
//  TimestampMatching.cs - Gbtc
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
//  12/01/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data;
using System.IO;
using GSF.IO;

namespace LogFileViewer.Filters
{
    public enum TimestampMatchingMode
    {
        /// <summary>
        /// All data before this time
        /// </summary>
        Before = 0,
        /// <summary>
        /// All data after this time
        /// </summary>
        After = 1,
        /// <summary>
        /// All data between the two timestamps
        /// </summary>
        Inside = 2,
        /// <summary>
        /// All data outside the two timestamps
        /// </summary>
        Outside = 3
    }

    public class TimestampMatching
    {
        private DateTime m_first;
        private DateTime m_second;
        private TimestampMatchingMode m_mode;

        public TimestampMatching(TimestampMatchingMode mode, DateTime first, DateTime second)
        {
            m_first = first;
            m_second = second;
            m_mode = mode;
        }

        public TimestampMatching(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 1:
                    m_mode = (TimestampMatchingMode)stream.ReadNextByte();
                    m_first = stream.ReadDateTime();
                    m_second = stream.ReadDateTime();
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public void Save(Stream stream)
        {
            stream.Write((byte)1);
            stream.Write((byte)m_mode);
            stream.Write(m_first);
            stream.Write(m_second);
        }

        public bool IsMatch(DateTime time)
        {
            switch (m_mode)
            {
                case TimestampMatchingMode.Before:
                    return time < m_first;
                case TimestampMatchingMode.After:
                    return time > m_first;
                case TimestampMatchingMode.Inside:
                    return m_first <= time && time <= m_second;
                case TimestampMatchingMode.Outside:
                    return time < m_first || time > m_second;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToString()
        {
            switch (m_mode)
            {
                case TimestampMatchingMode.Before:
                    return "Before: " + m_first.ToLocalTime();
                case TimestampMatchingMode.After:
                    return "After: " + m_first.ToLocalTime();
                case TimestampMatchingMode.Inside:
                    return "Inside: " + m_first.ToLocalTime() + " and " + m_second.ToLocalTime();
                case TimestampMatchingMode.Outside:
                    return "Outside: " + m_first.ToLocalTime() + " and " + m_second.ToLocalTime();
                default:
                    return "Unknown";
            }
        }





    }
}