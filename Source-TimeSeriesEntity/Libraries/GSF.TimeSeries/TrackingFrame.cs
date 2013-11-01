//******************************************************************************************************
//  TrackingFrame.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  10/31/2013 - Stephen C. Wills
//       Modified the tracking frame to track each individual time-series entity that entered the frame,
//       as well as the time that the tracking frame was created. Also implemented a lock-free method
//       for safely writing to and reading from a tracking frame based on its usage pattern.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using GSF.Collections;

namespace GSF.TimeSeries
{
    /// <summary>
    /// <see cref="IFrame"/> container used to track <see cref="ITimeSeriesEntity"/> objects for down-sampling.
    /// </summary>
    internal class TrackingFrame
    {
        #region [ Members ]

        // Fields
        private readonly IFrame m_sourceFrame;
        private readonly IDictionary<Guid, IList<ITimeSeriesEntity>> m_entities;
        private Ticks m_createdTimestamp;
        private int m_producerToken;
        private int m_consumerToken;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="TrackingFrame"/> given the specified parameters.
        /// </summary>
        /// <param name="sourceFrame">Source <see cref="IFrame"/> to track.</param>
        public TrackingFrame(IFrame sourceFrame)
        {
            m_sourceFrame = sourceFrame;
            m_entities = new Dictionary<Guid, IList<ITimeSeriesEntity>>();
            m_createdTimestamp = DateTime.UtcNow.Ticks;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets instance of <see cref="IFrame"/> being tracked.
        /// </summary>
        public IFrame SourceFrame
        {
            get
            {
                return m_sourceFrame;
            }
        }

        /// <summary>
        /// Gets timestamp of <see cref="IFrame"/> being tracked.
        /// </summary>
        public long Timestamp
        {
            get
            {
                return m_sourceFrame.Timestamp;
            }
        }

        /// <summary>
        /// Gets the number of signals that have been added to this tracking frame.
        /// </summary>
        public int SignalCount
        {
            get
            {
                return m_entities.Count;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of when this <see cref="TrackingFrame"/> was created.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public Ticks CreatedTimestamp
        {
            get
            {
                return m_createdTimestamp;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds a time-series entity to this tracking frame so that it
        /// can be applied to the source frame at the publication time.
        /// </summary>
        /// <param name="entity">The time-series entity to be added to the tracking frame.</param>
        /// <returns>A boolean value indicating whether the value was successfully added to the tracking frame.</returns>
        /// <remarks>
        /// In multi-threaded scenarios, this thread will fail to add entities to the frame if multiple producer
        /// threads attempt to add entities to the tracking frame at the same time. Additionally, after the first
        /// call to <see cref="GetEntities"/>, the tracking frame will be locked so that this method will always
        /// fail to add the entity to the frame. In these cases, this method will do nothing and return false.
        /// </remarks>
        public bool Add(ITimeSeriesEntity entity)
        {
            IList<ITimeSeriesEntity> entities;

            if (Interlocked.CompareExchange(ref m_consumerToken, 0, 0) == 0)
            {
                if (Interlocked.CompareExchange(ref m_producerToken, 1, 0) == 0)
                {
                    entities = m_entities.GetOrAdd(entity.ID, signalID => new List<ITimeSeriesEntity>());
                    entities.Add(entity);
                    Interlocked.Exchange(ref m_producerToken, 0);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the collection of entities, grouped by signal, that were added to the frame.
        /// </summary>
        /// <returns>The collection of entities that were added to the frame.</returns>
        /// <remarks>
        /// This method causes the tracking frame to be locked so that additional attempts
        /// to add entities to the frame will fail. This allows thread-safety between
        /// <see cref="Add"/> and <c>GetEntities</c> so that the value returned from this
        /// method can be used safely without the possibility of separate thread adding
        /// entities to the collections.
        /// </remarks>
        public IDictionary<Guid, IList<ITimeSeriesEntity>> GetEntities()
        {
            SpinWait spinner = new SpinWait();

            Interlocked.Exchange(ref m_consumerToken, 1);

            while (Interlocked.CompareExchange(ref m_producerToken, 0, 0) != 0)
                spinner.SpinOnce();

            return m_entities;
        }

        #endregion
    }
}