//******************************************************************************************************
//  Header.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  11/05/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.SELEventParser
{
    public class Header
    {
        private string m_relayID;
        private DateTime m_eventTime;
        private string m_stationID;
        private int m_serialNumber;

        public string RelayID
        {
            get
            {
                return m_relayID;
            }
            set
            {
                m_relayID = value;
            }
        }

        public string StationID
        {
            get
            {
                return m_stationID;
            }
            set
            {
                m_stationID = value;
            }
        }

        public DateTime EventTime
        {
            get
            {
                return m_eventTime;
            }
            set
            {
                m_eventTime = value;
            }
        }

        public int SerialNumber
        {
            get
            {
                return m_serialNumber;
            }
            set
            {
                m_serialNumber = value;
            }
        }
    }
}
