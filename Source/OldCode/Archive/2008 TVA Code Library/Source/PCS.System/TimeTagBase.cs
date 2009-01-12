/**************************************************************************\
   Copyright (c) 2008 - Gbtc, James Ritchie Carroll
   All rights reserved.
  
   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
       
      * Redistributions in binary form must reproduce the above
        copyright notice, this list of conditions and the following
        disclaimer in the documentation and/or other materials provided
        with the distribution.
  
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY
   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
   PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
   OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
   OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  
\**************************************************************************/

using System.Runtime.Serialization;

namespace System
{
    /// <summary>Base class for alternate time tag implementations.</summary>
    public abstract class TimeTagBase : ISerializable, IComparable, IComparable<TimeTagBase>, IComparable<DateTime>, IEquatable<TimeTagBase>
    {
        #region [ Members ]

        // Fields
        private long m_baseDateOffsetTicks;
        private double m_seconds;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="TimeTagBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected TimeTagBase(SerializationInfo info, StreamingContext context)
        {
            // Deserializes time tag
            m_baseDateOffsetTicks = info.GetInt64("baseDateOffsetTicks");
            m_seconds = info.GetDouble("seconds");
        }

        /// <summary>Creates a new <see cref="TimeTagBase"/>, given number base time (in ticks) and seconds since base time.</summary>
        /// <param name="baseDateOffsetTicks">Ticks of time tag base.</param>
        /// <param name="seconds">Number of seconds since base time.</param>
        protected TimeTagBase(long baseDateOffsetTicks, double seconds)
        {
            m_baseDateOffsetTicks = baseDateOffsetTicks;
            Value = seconds;
        }

        /// <summary>Creates a new <see cref="TimeTagBase"/>, given standard .NET DateTime.</summary>
        /// <param name="baseDateOffsetTicks">Ticks of time tag base.</param>
        /// <param name="timestamp">.NET DateTime used to create time tag from.</param>
        protected TimeTagBase(long baseDateOffsetTicks, DateTime timestamp)
        {
            // Zero base 100-nanosecond ticks from 1/1/1970 and convert to seconds.
            m_baseDateOffsetTicks = baseDateOffsetTicks;
            Value = Ticks.ToSeconds(timestamp.Ticks - m_baseDateOffsetTicks);
        }

        #endregion

        #region [ Properties ]

        /// <summary>Gets or sets number of seconds since base time.</summary>
        public virtual double Value
        {
            get
            {
                return m_seconds;
            }
            set
            {
                m_seconds = value;
                if (m_seconds < 0) m_seconds = 0;
            }
        }

        /// <summary>Gets ticks representing the absolute minimum time of this time tag implementation.</summary>
        public virtual long BaseDateOffsetTicks
        {
            get
            {
                return m_baseDateOffsetTicks;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>Returns standard .NET <see cref="DateTime"/> representation for time tag.</summary>
        public virtual DateTime ToDateTime()
        {
            // Converts m_seconds to 100-nanosecond ticks and add the base time offset.
            return new DateTime(Seconds.ToTicks(m_seconds) + m_baseDateOffsetTicks);
        }

        /// <summary>Returns basic textual representation for time tag.</summary>
        /// <remarks>Format is "yyyy-MM-dd HH:mm:ss.fff" so that textual representation can be sorted in the
        /// correct chronological order.</remarks>
        public override string ToString()
        {
            return ToDateTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// Compares the <see cref="TimeTagBase"/> with another <see cref="TimeTagBase"/>.
        /// </summary>
        /// <param name="other">The <see cref="TimeTagBase"/> to compare with the current <see cref="TimeTagBase"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(TimeTagBase other)
        {
            // Since compared time tags may not have the same base time, we compare using .NET date time.
            return CompareTo(other.ToDateTime());
        }

        /// <summary>
        /// Compares the <see cref="TimeTagBase"/> with a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="other">The <see cref="DateTime"/> to compare with the current <see cref="TimeTagBase"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(DateTime other)
        {
            return ToDateTime().CompareTo(other);
        }

        /// <summary>
        /// Compares the <see cref="TimeTagBase"/> with the specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="TimeTagBase"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><see cref="Object"/> is not an <see cref="TimeTagBase"/> or a <see cref="DateTime"/>.</exception>
        public virtual int CompareTo(object obj)
        {
            TimeTagBase timetag = obj as TimeTagBase;
            if (timetag != null) return CompareTo(timetag);
            if (obj is DateTime) return CompareTo((DateTime)obj);
            throw new ArgumentException("TimeTag can only be compared with other TimeTags or DateTimes...");
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="TimeTagBase"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="TimeTagBase"/>.</param>
        /// <returns>
        /// true if the specified <see cref="Object"/> is equal to the current <see cref="TimeTagBase"/>;
        /// otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentException"><see cref="Object"/> is not an <see cref="TimeTagBase"/>.</exception>
        public override bool Equals(object obj)
        {
            return (CompareTo(obj) == 0);
        }

        /// <summary>
        /// Determines whether the specified <see cref="TimeTagBase"/> is equal to the current <see cref="TimeTagBase"/>.
        /// </summary>
        /// <param name="other">The <see cref="TimeTagBase"/> to compare with the current <see cref="TimeTagBase"/>.</param>
        /// <returns>
        /// true if the specified <see cref="TimeTagBase"/> is equal to the current <see cref="TimeTagBase"/>;
        /// otherwise, false.
        /// </returns>
        public bool Equals(TimeTagBase other)
        {
            return (CompareTo(other) == 0);
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="TimeTagBase"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="TimeTagBase"/>.</returns>
        /// <remarks>Hash code based on number of seconds timetag represents.</remarks>
        public override int GetHashCode()
        {
            return (int)(m_seconds * 1000);
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serializes time tag.
            info.AddValue("baseDateOffsetTicks", m_baseDateOffsetTicks);
            info.AddValue("seconds", m_seconds);
        }

        #endregion
    }
}