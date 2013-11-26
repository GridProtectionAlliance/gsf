//******************************************************************************************************
//  Channel.cs - Gbtc
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
//  11/06/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;

namespace GSF.SELEventParser
{
    public class Channel<T>
    {
        private string m_name;
        private List<Cycle<T>> m_cycles;
        private List<T> m_samples; 

        public Channel()
        {
            m_cycles = new List<Cycle<T>>();
        }

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        public List<Cycle<T>> Cycles
        {
            get
            {
                return m_cycles;
            }
            set
            {
                m_cycles = value;
            }
        }

        public List<T> Samples
        {
            get
            {
                if ((object)m_cycles == null)
                    return null;

                return m_samples ??
                    (m_samples = m_cycles
                        .SelectMany(cycle => cycle.Samples)
                        .ToList());
            }
        }
    }
}
