//*******************************************************************************************************
//  Frame.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  6/22/2006 - James R. Carroll
//       Initial version of source generated
//  09/16/2008 - James R. Carroll
//      Converted to C#.
//  08/05/2009 - Josh Patterson
//      Edited Comments
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;

namespace TVA.Measurements
{
    /// <summary>
    /// Implementation of a basic <see cref="IFrame"/>.
    /// </summary>
    /// <remarks>
    /// A frame represents a collection of measurements at a given time.
    /// </remarks>
    [CLSCompliant(false)]
    public class Frame : IFrame
    {
        #region [ Members ]

        // Fields
        private Ticks m_timestamp;                                          // Time, represented as 100-nanosecond ticks, of this frame of data
        private bool m_published;                                           // Determines if this frame of data has been published
        private int m_publishedMeasurements;                                // Total measurements published by this frame
        private Dictionary<MeasurementKey, IMeasurement> m_measurements;    // Collection of measurements published by this frame
        private IMeasurement m_lastSortedMeasurement;                       // Last measurement sorted into this frame

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="Frame"/> given the specified parameters.
        /// </summary>
        /// <param name="timestamp">Timestamp, in ticks, for this <see cref="Frame"/>.</param>
        public Frame(Ticks timestamp)
        {
            m_timestamp = timestamp;
            m_measurements = new Dictionary<MeasurementKey, IMeasurement>(100);
            m_publishedMeasurements = -1;
        }

        /// <summary>
        /// Constructs a new <see cref="Frame"/> given the specified parameters.
        /// </summary>
        /// <param name="timestamp">Timestamp, in ticks, for this <see cref="Frame"/>.</param>
        /// <param name="measurements">Initial set of measurements to load into the <see cref="Frame"/>, if any.</param>
        public Frame(Ticks timestamp, IDictionary<MeasurementKey, IMeasurement> measurements)
        {
            m_timestamp = timestamp;
            m_measurements = new Dictionary<MeasurementKey, IMeasurement>(measurements);
            m_publishedMeasurements = -1;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Keyed measurements in this <see cref="Frame"/>.
        /// </summary>
        public virtual IDictionary<MeasurementKey, IMeasurement> Measurements
        {
            get
            {
                return m_measurements;
            }
        }

        /// <summary>
        /// Gets or sets published state of this <see cref="Frame"/>.
        /// </summary>
        public virtual bool Published
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

        /// <summary>
        /// Gets or sets total number of measurements that have been published for this <see cref="Frame"/>.
        /// </summary>
        /// <remarks>
        /// If this property has not been assigned a value, the property will return measurement count.
        /// </remarks>
        public virtual int PublishedMeasurements
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
            set
            {
                m_timestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets reference to last measurement that was sorted into this <see cref="Frame"/>.
        /// </summary>
        public virtual IMeasurement LastSortedMeasurement
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

        /// <summary>
        /// Create a copy of this <see cref="Frame"/> and its measurements.
        /// </summary>
        /// <remarks>
        /// The measurement dictionary of this <see cref="Frame"/> is synclocked during copy.
        /// </remarks>
        /// <returns>A cloned <see cref="Frame"/>.</returns>
        public virtual Frame Clone()
        {
            IDictionary<MeasurementKey, IMeasurement> measurements = Measurements;

            lock (measurements)
            {
                return new Frame(m_timestamp, measurements);
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
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IFrame"/>.</exception>
        public override bool Equals(object obj)
        {
            IFrame other = obj as IFrame;
            
            if ((object)other != null)
                return Equals(other);

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
            return m_timestamp.CompareTo(other.Timestamp);
        }

        /// <summary>
        /// Compares the <see cref="Frame"/> with the specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Frame"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IFrame"/>.</exception>
        /// <remarks>This implementation of a basic frame compares itself by timestamp.</remarks>
        public int CompareTo(object obj)
        {
            IFrame other = obj as IFrame;

            if ((object)other != null)
                return CompareTo(other);

            throw new ArgumentException("Frame can only be compared with other IFrames...");
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
            return frame1.Equals(frame2);
        }

        /// <summary>
        /// Compares two <see cref="Frame"/> timestamps for inequality.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator !=(Frame frame1, Frame frame2)
        {
            return !frame1.Equals(frame2);
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is greater than right <see cref="Frame"/> timestamp.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator >(Frame frame1, Frame frame2)
        {
            return frame1.CompareTo(frame2) > 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is greater than or equal to right <see cref="Frame"/> timestamp.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator >=(Frame frame1, Frame frame2)
        {
            return frame1.CompareTo(frame2) >= 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is less than right <see cref="Frame"/> timestamp.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator <(Frame frame1, Frame frame2)
        {
            return frame1.CompareTo(frame2) < 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Frame"/> timestamp is less than or equal to right <see cref="Frame"/> timestamp.
        /// </summary>
        /// <param name="frame1">The <see cref="Frame"/> left hand operand.</param>
        /// <param name="frame2">The <see cref="Frame"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> representing the result of the operation.</returns>
        public static bool operator <=(Frame frame1, Frame frame2)
        {
            return frame1.CompareTo(frame2) <= 0;
        }

        #endregion
    }
}