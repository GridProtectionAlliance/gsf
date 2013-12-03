//******************************************************************************************************
//  Event.cs - Gbtc
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
//  11/19/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.SELEventParser
{
    public class EventHistoryRecord
    {
        #region [ Members ]

        // Fields
        private int m_eventNumber;
        private DateTime m_time;
        private string m_eventType;
        private double m_faultLocation;
        private double m_current;
        private double m_frequency;
        private int m_group;
        private int m_shot;
        private string m_targets;

        #endregion

        #region [ Properties ]

        public int EventNumber
        {
            get
            {
                return m_eventNumber;
            }
            set
            {
                m_eventNumber = value;
            }
        }

        public DateTime Time
        {
            get
            {
                return m_time;
            }
            set
            {
                m_time = value;
            }
        }

        public string EventType
        {
            get
            {
                return m_eventType;
            }
            set
            {
                m_eventType = value;
            }
        }

        public double FaultLocation
        {
            get
            {
                return m_faultLocation;
            }
            set
            {
                m_faultLocation = value;
            }
        }

        public double Current
        {
            get
            {
                return m_current;
            }
            set
            {
                m_current = value;
            }
        }

        public double Frequency
        {
            get
            {
                return m_frequency;
            }
            set
            {
                m_frequency = value;
            }
        }

        public int Group
        {
            get
            {
                return m_group;
            }
            set
            {
                m_group = value;
            }
        }

        public int Shot
        {
            get
            {
                return m_shot;
            }
            set
            {
                m_shot = value;
            }
        }

        public string Targets
        {
            get
            {
                return m_targets;
            }
            set
            {
                m_targets = value;
            }
        }

        #endregion
    }
}
