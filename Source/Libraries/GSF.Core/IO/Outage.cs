//******************************************************************************************************
//  Outage.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/24/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.IO
{
    /// <summary>
    /// Represents an outage as a start time and an end time.
    /// </summary>
    public class Outage
    {
        #region [ Members ]

        // Fields
        private DateTime m_startTime;
        private DateTime m_endTime;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="Outage"/>.
        /// </summary>
        public Outage()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Outage"/> with the specified start and end time.
        /// </summary>
        /// <param name="startTime">Start time for outage.</param>
        /// <param name="endTime">End time for outage.</param>
        public Outage(DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets start time for <see cref="Outage"/>.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return m_startTime;
            }
            set
            {
                if (m_endTime > DateTime.MinValue && value > m_endTime)
                    throw new ArgumentOutOfRangeException("value", "Outage start time is past end time");

                m_startTime = value;
            }
        }

        /// <summary>
        /// Gets or sets end time for <see cref="Outage"/>.
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                return m_endTime;
            }
            set
            {
                if (m_startTime > value)
                    throw new ArgumentOutOfRangeException("value", "Outage start time is past end time");

                m_endTime = value;
            }
        }

        #endregion
    }
}