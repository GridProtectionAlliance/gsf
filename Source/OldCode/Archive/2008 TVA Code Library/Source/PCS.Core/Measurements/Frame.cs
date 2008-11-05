//*******************************************************************************************************
//  Frame.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  6/22/2006 - J. Ritchie Carroll
//       Initial version of source generated
//  09/16/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;

namespace PCS.Measurements
{
    /// <summary>Implementation of a basic frame.</summary>
    /// <remarks>A frame represents a collection of measurements at a given time.</remarks>
    public class Frame : IFrame
    {
        #region [ Members ]

        // Fields
        private long m_ticks;
        private bool m_published;
        private int m_publishedMeasurements;
        private Dictionary<MeasurementKey, IMeasurement> m_measurements;
        private long m_startSortTime;
        private long m_lastSortTime;
        private IMeasurement m_lastSortedMeasurement;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="Frame"/> given the specified parameters.
        /// </summary>
        /// <param name="ticks">Timestamp, in ticks, for this frame.</param>
        public Frame(long ticks)
        {
            m_ticks = ticks;
            m_measurements = new Dictionary<MeasurementKey, IMeasurement>(100);
            m_publishedMeasurements = -1;
        }

        /// <summary>
        /// Constructs a new <see cref="Frame"/> given the specified parameters.
        /// </summary>
        /// <param name="ticks">Timestamp, in ticks, for this frame.</param>
        /// <param name="measurements">Initial set of measurements to load into the frame, if any.</param>
        /// <param name="startSortTime">Time, in ticks, of when measurements began sorting into frame, if available.</param>
        /// <param name="lastSortTime">Time, in ticks, of when last measurement was sorted into frame, if available.</param>
        public Frame(long ticks, Dictionary<MeasurementKey, IMeasurement> measurements, long startSortTime, long lastSortTime)
        {
            m_ticks = ticks;
            m_measurements = new Dictionary<MeasurementKey, IMeasurement>(measurements);
            m_startSortTime = startSortTime;
            m_lastSortTime = lastSortTime;
            m_publishedMeasurements = -1;
        }

        #endregion

        #region [ Properties ]

        /// <summary>Keyed measurements in this frame.</summary>
        public IDictionary<MeasurementKey, IMeasurement> Measurements
        {
            get
            {
                return m_measurements;
            }
        }

        /// <summary>Gets or sets published state of this frame.</summary>
        public bool Published
        {
            get
            {
                return m_published;
            }
            set
            {
                m_published = value;
            }
        }

        /// <summary>Gets or sets total number of measurements that have been published for this frame.</summary>
        /// <remarks>If this property has not been assigned a value, the property will return measurement count.</remarks>
        public int PublishedMeasurements
        {
            get
            {
                if (m_publishedMeasurements == -1)
                    return m_measurements.Count;

                return m_publishedMeasurements;
            }
            set
            {
                m_publishedMeasurements = value;
            }
        }

        /// <summary>Gets or sets exact timestamp, in ticks, of the data represented in this frame.</summary>
        /// <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.</remarks>
        public long Ticks
        {
            get
            {
                return m_ticks;
            }
            set
            {
                m_ticks = value;
            }
        }

        /// <summary>Gets the DateTime representation of ticks of this frame.</summary>
        public DateTime Timestamp
        {
            get
            {
                return new DateTime(m_ticks);
            }
        }

        /// <summary>Gets or sets reference to last measurement that was sorted into this frame.</summary>
        public IMeasurement LastSortedMeasurement
        {
            get
            {
                return m_lastSortedMeasurement;
            }
            set
            {
                m_lastSortedMeasurement = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>Create a copy of this frame and its measurements.</summary>
        /// <remarks>This frame's measurement dictionary is synclocked during copy.</remarks>
        public Frame Clone()
        {
            lock (m_measurements)
            {
                return new Frame(m_ticks, m_measurements, m_startSortTime, m_lastSortTime);
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
            return (CompareTo(other) == 0);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Frame"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Frame"/>.</param>
        /// <returns>
        /// true if the specified <see cref="Object"/> is equal to the current <see cref="Frame"/>;
        /// otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentException"><see cref="Object"/> is not an <see cref="IFrame"/>.</exception>
        public override bool Equals(object obj)
        {
            IFrame other = obj as IFrame;
            if (other != null) return Equals(other);
            throw new ArgumentException("Object is not an IFrame");
        }

        /// <summary>
        /// Compares the <see cref="Frame"/> with an <see cref="IFrame"/>.
        /// </summary>
        /// <param name="other">The <see cref="IFrame"/> to compare with the current <see cref="Frame"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <remarks>This implementation of a basic frame compares itself by timestamp.</remarks>
        public int CompareTo(IFrame other)
        {
            return m_ticks.CompareTo(other.Ticks);
        }

        /// <summary>
        /// Compares the <see cref="Frame"/> with the specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Frame"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><see cref="Object"/> is not an <see cref="IFrame"/>.</exception>
        /// <remarks>This implementation of a basic frame compares itself by timestamp.</remarks>
        public int CompareTo(object obj)
        {
            IFrame other = obj as IFrame;
            if (other != null) return CompareTo(other);
            throw new ArgumentException("Frame can only be compared with other IFrames...");
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="Frame"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Frame"/>.</returns>
        /// <remarks>Hash code based on timestamp of frame.</remarks>
        public override int GetHashCode()
        {
            return m_ticks.GetHashCode();
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Compares two <see cref="Frame"/> timestamps for equality.
        /// </summary>
        public static bool operator ==(Frame frame1, Frame frame2)
        {
            return frame1.Equals(frame2);
        }

        /// <summary>
        /// Compares two <see cref="Frame"/> timestamps for inequality.
        /// </summary>
        public static bool operator !=(Frame frame1, Frame frame2)
        {
            return !frame1.Equals(frame2);
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is greater than right <see cref="Frame"/> timestamp.
        /// </summary>
        public static bool operator >(Frame frame1, Frame frame2)
        {
            return frame1.CompareTo(frame2) > 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is greater than or equal to right <see cref="Frame"/> timestamp.
        /// </summary>
        public static bool operator >=(Frame frame1, Frame frame2)
        {
            return frame1.CompareTo(frame2) >= 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is less than right <see cref="Frame"/> timestamp.
        /// </summary>
        public static bool operator <(Frame frame1, Frame frame2)
        {
            return frame1.CompareTo(frame2) < 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is less than or equal to right <see cref="Frame"/> timestamp.
        /// </summary>
        public static bool operator <=(Frame frame1, Frame frame2)
        {
            return frame1.CompareTo(frame2) <= 0;
        }

        #endregion
    }
}