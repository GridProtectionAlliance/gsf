//******************************************************************************************************
//  AnalogSection.cs - Gbtc
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSF.SELEventParser
{
    public class AnalogSection
    {
        private Channel<DateTime> m_timeChannel;
        private List<Channel<double>> m_analogChannels;

        public AnalogSection()
        {
            m_timeChannel = new Channel<DateTime>() { Name = "Time" };
            m_analogChannels = new List<Channel<double>>();
        }

        public Channel<DateTime> TimeChannel
        {
            get
            {
                return m_timeChannel;
            }
            set
            {
                m_timeChannel = value;
            }
        }

        public List<Channel<double>> AnalogChannels
        {
            get
            {
                return m_analogChannels;
            }
            set
            {
                m_analogChannels = value;
            }
        }

        public Channel<double> GetAnalogChannel(string name)
        {
            return m_analogChannels.FirstOrDefault(channel => channel.Name == name);
        }
    }
}
