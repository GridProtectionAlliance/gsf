//******************************************************************************************************
//  EventHistory.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/19/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;

namespace GSF.SELEventParser
{
    public class EventHistory
    {
        #region [ Members ]

        // Fields
        private string m_command;
        private Header m_header;
        private List<EventHistoryRecord> m_histories;

        #endregion

        #region [ Properties ]

        public string Command
        {
            get
            {
                return m_command;
            }
            set
            {
                m_command = value;
            }
        }

        public Header Header
        {
            get
            {
                return m_header;
            }
            set
            {
                m_header = value;
            }
        }

        public List<EventHistoryRecord> Histories
        {
            get
            {
                return m_histories;
            }
            set
            {
                m_histories = value;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        public static EventHistory Parse(string[] lines, ref int index)
        {
            EventHistory eventHistory = new EventHistory();

            // Parse the report header
            eventHistory.Header = Header.Parse(lines, ref index);

            // Skip to the next nonblank line
            EventFile.SkipBlanks(lines, ref index);

            // Parse event histories
            eventHistory.Histories = EventHistoryRecord.ParseRecords(lines, ref index);

            return eventHistory;
        }

        #endregion
    }
}
