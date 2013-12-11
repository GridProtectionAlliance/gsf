//******************************************************************************************************
//  TimeSeriesValueBase.cs - Gbtc
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
//  10/16/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using ProtoBuf;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents the fundamental base class for any time-series entity.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(1, typeof(Measurement<>))]
    public abstract class TimeSeriesEntityBase : ITimeSeriesEntity
    {
        #region [ Members ]

        // Fields
        [ProtoMember(1)]
        private readonly Guid m_id;
        [ProtoMember(2)]
        private readonly Ticks m_timestamp;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="TimeSeriesEntityBase"/> class.
        /// </summary>
        /// <param name="id">The fundamental identifier of the <see cref="TimeSeriesEntityBase"/></param>
        /// <param name="timestamp">The exact timestamp, in ticks, of the data represented by this <see cref="TimeSeriesEntityBase"/></param>
        protected TimeSeriesEntityBase(Guid id, Ticks timestamp)
        {
            m_id = id;
            m_timestamp = timestamp;
        }

        /// <summary>
        /// DO NOT USE.  ProtoBuf-net requires a parameterless constructor and 
        /// can access it via reflection.  This is protected so that 
        /// Measurement has access to it.  
        /// </summary>
        protected TimeSeriesEntityBase()
        {
            
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="Guid"/> based signal ID of this <see cref="TimeSeriesEntityBase"/>.
        /// </summary>
        /// <remarks>
        /// This is the fundamental identifier of the <see cref="TimeSeriesEntityBase"/>.
        /// </remarks>
        public Guid ID
        {
            get
            {
                return m_id;
            }
        }

        /// <summary>
        /// Gets exact timestamp, in ticks, of the data represented by this <see cref="TimeSeriesEntityBase"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public Ticks Timestamp
        {
            get
            {
                return m_timestamp;
            }
        }

        #endregion
    }
}
