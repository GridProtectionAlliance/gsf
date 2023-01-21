//******************************************************************************************************
//  Frame.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  04/14/2011 - J. Ritchie Carroll
//       Added received and published timestamps for measurements.
//  05/11/2011 - J. Ritchie Carroll
//       Updated to use a concurrent dictionary.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Implementation of a basic <see cref="IFrame"/>.
    /// </summary>
    /// <remarks>
    /// A frame represents a collection of measurements at a given time.
    /// </remarks>
    public class Frame : IFrame
    {
        #region [ Members ]

        // Fields
        private Ticks m_timestamp;                                                          // Time, represented as 100-nanosecond ticks, of this frame of data
        private ShortTime m_lifespan;                                                       // Elapsed time since creation of this frame of data
        private int m_sortedMeasurements;                                                   // Total measurements sorted into this frame

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="Frame"/> given the specified parameters.
        /// </summary>
        /// <param name="timestamp">Timestamp, in ticks, for this <see cref="Frame"/>.</param>
        /// <param name="expectedMeasurements">Expected number of measurements for the <see cref="Frame"/>.</param>
        public Frame(Ticks timestamp, int expectedMeasurements = -1)
        {
            m_timestamp = timestamp;
            m_lifespan = ShortTime.Now;

            if (expectedMeasurements > 0)
                Measurements = new ConcurrentDictionary<MeasurementKey, IMeasurement>(s_defaultConcurrencyLevel, expectedMeasurements * 2);
            else
                Measurements = new ConcurrentDictionary<MeasurementKey, IMeasurement>();

            m_sortedMeasurements = -1;
        }

        /// <summary>
        /// Constructs a new <see cref="Frame"/> given the specified parameters.
        /// </summary>
        /// <param name="timestamp">Timestamp, in ticks, for this <see cref="Frame"/>.</param>
        /// <param name="measurements">Initial set of measurements to load into the <see cref="Frame"/>, if any.</param>
        public Frame(Ticks timestamp, IDictionary<MeasurementKey, IMeasurement> measurements)
        {
            m_timestamp = timestamp;
            m_lifespan = ShortTime.Now;
            Measurements = new ConcurrentDictionary<MeasurementKey, IMeasurement>(measurements);
            m_sortedMeasurements = -1;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Keyed measurements in this <see cref="Frame"/>.
        /// </summary>
        public ConcurrentDictionary<MeasurementKey, IMeasurement> Measurements { get; }

        /// <summary>
        /// Gets or sets published state of this <see cref="Frame"/> (pre-processing).
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets total number of measurements that have been sorted into this <see cref="Frame"/>.
        /// </summary>
        /// <remarks>
        /// If this property has not been assigned a value, the property will return measurement count.
        /// </remarks>
        public int SortedMeasurements
        {
            get
            {
                if (m_sortedMeasurements == -1)
                    return Measurements.Count;

                return m_sortedMeasurements;
            }
            set => m_sortedMeasurements = value;
        }

        /// <summary>
        /// Gets or sets exact timestamp, in <see cref="Ticks"/>, of the data represented in this <see cref="Frame"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public virtual Ticks Timestamp
        {
            get => m_timestamp;
            set => m_timestamp = value;
        }

        /// <summary>
        /// Gets the life-span of this <see cref="Frame"/> since its creation.
        /// </summary>
        public ShortTime Lifespan => m_lifespan;

        /// <summary>
        /// Gets timestamp, in ticks, of when this <see cref="Frame"/> was created.
        /// </summary>
        public Ticks CreatedTimestamp => m_lifespan.UtcTime.Ticks;

        /// <summary>
        /// Gets or sets reference to last measurement that was sorted into this <see cref="Frame"/>.
        /// </summary>
        public IMeasurement LastSortedMeasurement { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Create a copy of this <see cref="Frame"/> and its measurements.
        /// </summary>
        /// <remarks>
        /// The measurement dictionary of this <see cref="Frame"/> is synclocked during copy.
        /// </remarks>
        /// <returns>A cloned <see cref="Frame"/>.</returns>
        public virtual Frame Clone()
        {
            lock (Measurements)
            {
                return new Frame(m_timestamp, Measurements);
            }
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
            return CompareTo(other) == 0;
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
            if (obj is IFrame other)
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
            if (other is not null)
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
            if (obj is IFrame other)
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
            // ReSharper disable once NonReadonlyMemberInGetHashCode
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
            if ((object)frame1 is not null)
                return frame1.Equals(frame2);

            return (object)frame2 is null;
        }

        /// <summary>
        /// Compares two <see cref="Frame"/> timestamps for inequality.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator !=(Frame frame1, Frame frame2)
        {
            if ((object)frame1 is not null)
                return !frame1.Equals(frame2);

            return (object)frame2 is not null;
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is greater than right <see cref="Frame"/> timestamp.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator >(Frame frame1, Frame frame2)
        {
            if ((object)frame1 is not null)
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
            if ((object)frame1 is not null)
                return frame1.CompareTo(frame2) >= 0;

            return (object)frame2 is null;
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is less than right <see cref="Frame"/> timestamp.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator <(Frame frame1, Frame frame2)
        {
            if ((object)frame1 is not null)
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
            if ((object)frame1 is not null)
                return frame1.CompareTo(frame2) <= 0;

            return (object)frame2 is null;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly int s_defaultConcurrencyLevel = Environment.ProcessorCount * 4;

        #endregion
    }
}