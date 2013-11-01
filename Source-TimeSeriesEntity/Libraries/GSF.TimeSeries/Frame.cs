//******************************************************************************************************
//  Frame.cs - Gbtc
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
//  04/14/2011 - J. Ritchie Carroll
//       Added received and published timestamps for measurements.
//  05/11/2011 - J. Ritchie Carroll
//       Updated to use a concurrent dictionary.
//  11/01/2013 - Stephen C. Wills
//       Updated to maintain a dictionary of time-series entities.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Implementation of a basic <see cref="IFrame"/>.
    /// </summary>
    /// <remarks>
    /// A frame represents a collection of time-series entities at a given time.
    /// </remarks>
    public class Frame : IFrame
    {
        #region [ Members ]

        // Fields
        private readonly Ticks m_timestamp;                                  // Time, represented as 100-nanosecond ticks, of this frame of data
        private readonly IDictionary<Guid, ITimeSeriesEntity> m_entities;    // Dictionary of time-series entities published by this frame

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="Frame"/> given the specified parameters.
        /// </summary>
        /// <param name="timestamp">Timestamp, in ticks, for this <see cref="Frame"/>.</param>
        /// <param name="expectedEntities">Expected number of time-series entities for the <see cref="Frame"/>.</param>
        public Frame(Ticks timestamp, int expectedEntities = -1)
        {
            m_timestamp = timestamp;

            if (expectedEntities > 0)
                m_entities = new Dictionary<Guid, ITimeSeriesEntity>(expectedEntities * 2);
            else
                m_entities = new Dictionary<Guid, ITimeSeriesEntity>();
        }

        /// <summary>
        /// Constructs a new <see cref="Frame"/> given the specified parameters.
        /// </summary>
        /// <param name="timestamp">Timestamp, in ticks, for this <see cref="Frame"/>.</param>
        /// <param name="entities">Initial set of time-series entities to load into the <see cref="Frame"/>, if any.</param>
        public Frame(Ticks timestamp, IDictionary<Guid, ITimeSeriesEntity> entities)
        {
            m_timestamp = timestamp;
            m_entities = new Dictionary<Guid, ITimeSeriesEntity>(entities);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets exact timestamp, in <see cref="Ticks"/>, of the data represented in this <see cref="Frame"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public virtual Ticks Timestamp
        {
            get
            {
                return m_timestamp;
            }
        }

        /// <summary>
        /// Keyed time-series entities in this <see cref="Frame"/>.
        /// </summary>
        /// <remarks>
        /// Represents a dictionary of time-series entities, keyed by signal ID.
        /// </remarks>
        public IDictionary<Guid, ITimeSeriesEntity> Entities
        {
            get
            {
                return m_entities;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Create a copy of this <see cref="Frame"/> and its time-series entities.
        /// </summary>
        /// <returns>A cloned <see cref="Frame"/>.</returns>
        public virtual Frame Clone()
        {
            return new Frame(m_timestamp, m_entities);
        }

        /// <summary>
        /// Determines whether the specified <see cref="IFrame"/> is equal to the current <see cref="Frame"/>.
        /// </summary>
        /// <param name="other">The <see cref="IFrame"/> to compare with the current <see cref="Frame"/>.</param>
        /// <returns>
        /// true if the specified <see cref="IFrame"/> is equal to the current <see cref="Frame"/>;
        /// otherwise, false.
        /// </returns>
        public bool Equals(IFrame other)
        {
            return (CompareTo(other) == 0);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="Frame"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="Frame"/>.</param>
        /// <returns>
        /// true if the specified <see cref="object"/> is equal to the current <see cref="Frame"/>;
        /// otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            IFrame other = obj as IFrame;

            if ((object)other != null)
                return Equals(other);

            return false;
        }

        /// <summary>
        /// Compares the <see cref="Frame"/> with an <see cref="IFrame"/>.
        /// </summary>
        /// <param name="other">The <see cref="IFrame"/> to compare with the current <see cref="Frame"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <remarks>This implementation of a basic frame compares itself by timestamp.</remarks>
        public int CompareTo(IFrame other)
        {
            if ((object)other != null)
                return m_timestamp.CompareTo(other.Timestamp);

            return 1;
        }

        /// <summary>
        /// Compares the <see cref="Frame"/> with the specified <see cref="object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="Frame"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IFrame"/>.</exception>
        /// <remarks>This implementation of a basic frame compares itself by timestamp.</remarks>
        public int CompareTo(object obj)
        {
            IFrame other = obj as IFrame;

            if ((object)other != null)
                return CompareTo(other);

            throw new ArgumentException("Frame can only be compared with other IFrames");
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="Frame"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Frame"/>.</returns>
        /// <remarks>Hash code based on timestamp of frame.</remarks>
        public override int GetHashCode()
        {
            return m_timestamp.GetHashCode();
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Compares two <see cref="Frame"/> timestamps for equality.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator ==(Frame frame1, Frame frame2)
        {
            if ((object)frame1 != null)
                return frame1.Equals(frame2);

            return ((object)frame2 == null);
        }

        /// <summary>
        /// Compares two <see cref="Frame"/> timestamps for inequality.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator !=(Frame frame1, Frame frame2)
        {
            if ((object)frame1 != null)
                return !frame1.Equals(frame2);

            return ((object)frame2 != null);
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is greater than right <see cref="Frame"/> timestamp.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator >(Frame frame1, Frame frame2)
        {
            if ((object)frame1 != null)
                return frame1.CompareTo(frame2) > 0;

            return false;
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is greater than or equal to right <see cref="Frame"/> timestamp.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator >=(Frame frame1, Frame frame2)
        {
            if ((object)frame1 != null)
                return frame1.CompareTo(frame2) >= 0;

            return ((object)frame2 == null);
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is less than right <see cref="Frame"/> timestamp.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator <(Frame frame1, Frame frame2)
        {
            if ((object)frame1 != null)
                return frame1.CompareTo(frame2) < 0;

            return false;
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is less than or equal to right <see cref="Frame"/> timestamp.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator <=(Frame frame1, Frame frame2)
        {
            if ((object)frame1 != null)
                return frame1.CompareTo(frame2) <= 0;

            return ((object)frame2 == null);
        }

        #endregion
    }
}