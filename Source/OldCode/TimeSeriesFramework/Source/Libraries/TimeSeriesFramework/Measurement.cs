//******************************************************************************************************
//  Measurement.cs - Gbtc
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TVA;
using TVA.Collections;

namespace TimeSeriesFramework
{
    /// <summary>
    /// Implementation of a basic measurement.
    /// </summary>
    public class Measurement : IMeasurement
    {
        #region [ Members ]

        // Fields
        private uint m_id;
        private string m_source;
        private MeasurementKey m_key;
        private Guid m_signalID;
        private string m_tagName;
        private Ticks m_timestamp;
        private double m_value;
        private double m_adder;
        private double m_multiplier;
        private bool m_valueQualityIsGood;
        private bool m_timestampQualityIsGood;
        private bool m_isDiscarded;
        private MeasurementValueFilterFunction m_measurementValueFilter;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> using default settings.
        /// </summary>
        public Measurement()
            : this(uint.MaxValue, "__", Guid.Empty, double.NaN, 0.0, 1.0, 0)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the new measurement.</param>
        /// <param name="source">Source name of the new measurement.</param>
        public Measurement(uint id, string source)
            : this(id, source, Guid.Empty, double.NaN, 0.0, 1.0, 0)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="signalID"><see cref="Guid"/> based signal ID of the new measurement.</param>
        public Measurement(Guid signalID)
            : this(uint.MaxValue, "__", signalID, double.NaN, 0.0, 1.0, 0)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the new measurement.</param>
        /// <param name="source">Source name of the new measurement.</param>
        /// <param name="value">Value of the new measurement.</param>
        /// <param name="timestamp">Timestamp, in ticks, of the new measurement.</param>
        public Measurement(uint id, string source, double value, Ticks timestamp)
            : this(id, source, Guid.Empty, value, 0.0, 1.0, timestamp)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="signalID"><see cref="Guid"/> based signal ID of the new measurement.</param>
        /// <param name="value">Value of the new measurement.</param>
        /// <param name="timestamp">Timestamp, in ticks, of the new measurement.</param>
        public Measurement(Guid signalID, double value, Ticks timestamp)
            : this(uint.MaxValue, "__", signalID, value, 0.0, 1.0, timestamp)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the new measurement.</param>
        /// <param name="source">Source name of the new measurement.</param>
        /// <param name="tagName">Text based tag name of the new measurement.</param>
        /// <param name="adder">Defined adder to apply to the new measurement.</param>
        /// <param name="multiplier">Defined multiplier to apply to the new measurement.</param>
        public Measurement(uint id, string source, string tagName, double adder, double multiplier)
            : this(id, source, Guid.Empty, double.NaN, adder, multiplier, 0)
        {
            m_tagName = tagName;
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the new measurement.</param>
        /// <param name="source">Source name of the new measurement.</param>
        /// <param name="signalID"><see cref="Guid"/> based signal ID of the new measurement.</param>
        /// <param name="value">Value of the new measurement.</param>
        /// <param name="adder">Defined adder to apply to the new measurement.</param>
        /// <param name="multiplier">Defined multiplier to apply to the new measurement.</param>
        /// <param name="timestamp">Timestamp, in ticks, of the new measurement.</param>
        public Measurement(uint id, string source, Guid signalID, double value, double adder, double multiplier, Ticks timestamp)
        {
            m_id = id;
            m_source = source;
            m_key = new MeasurementKey(m_id, m_source);
            m_signalID = signalID;
            m_value = value;
            m_adder = adder;
            m_multiplier = multiplier;
            m_timestamp = timestamp;
            m_valueQualityIsGood = true;
            m_timestampQualityIsGood = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the numeric ID of this <see cref="Measurement"/>.
        /// </summary>
        /// <remarks>
        /// <para>In most implementations, this will be a required field.</para>
        /// <para>Note that this field, in addition to <see cref="Source"/>, typically creates the primary key for a <see cref="Measurement"/>.</para>
        /// </remarks>
        public virtual uint ID
        {
            get
            {
                return m_id;
            }
            set
            {
                if (m_id != value)
                {
                    m_id = value;
                    m_key = new MeasurementKey(m_id, m_source);
                }
            }
        }

        /// <summary>
        /// Gets or sets the source of this <see cref="Measurement"/>.
        /// </summary>
        /// <remarks>
        /// <para>In most implementations, this will be a required field.</para>
        /// <para>Note that this field, in addition to <see cref="ID"/>, typically creates the primary key for a <see cref="Measurement"/>.</para>
        /// <para>This value is typically used to track the archive name in which measurement is stored.</para>
        /// </remarks>
        public virtual string Source
        {
            get
            {
                return m_source;
            }
            set
            {
                if (m_source != value)
                {
                    m_source = value;
                    m_key = new MeasurementKey(m_id, m_source);
                }
            }
        }

        /// <summary>
        /// Gets the primary key (a <see cref="MeasurementKey"/>, of this <see cref="Measurement"/>.
        /// </summary>
        public virtual MeasurementKey Key
        {
            get
            {
                return m_key;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Guid"/> based signal ID of this <see cref="Measurement"/>, if available.
        /// </summary>
        public virtual Guid SignalID
        {
            get
            {
                return m_signalID;
            }
            set
            {
                m_signalID = value;
            }
        }

        /// <summary>
        /// Gets or sets the text based tag name of this <see cref="Measurement"/>.
        /// </summary>
        public virtual string TagName
        {
            get
            {
                return m_tagName;
            }
            set
            {
                m_tagName = value;
            }
        }

        /// <summary>
        /// Gets or sets the raw measurement value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>.
        /// </summary>
        /// <returns>Raw value of this <see cref="Measurement"/> (i.e., value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>).</returns>
        public virtual double Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        /// <summary>
        /// Gets the adjusted numeric value of this measurement, taking into account the specified <see cref="Adder"/> and <see cref="Multiplier"/> offsets.
        /// </summary>
        /// <remarks>
        /// Note that returned value will be offset by <see cref="Adder"/> and <see cref="Multiplier"/>.
        /// </remarks>
        /// <returns><see cref="Value"/> offset by <see cref="Adder"/> and <see cref="Multiplier"/> (i.e., <c><see cref="Value"/> * <see cref="Multiplier"/> + <see cref="Adder"/></c>).</returns>
        public virtual double AdjustedValue
        {
            get
            {
                return m_value * m_multiplier + m_adder;
            }
        }

        /// <summary>
        /// Gets or sets an offset to add to the measurement value. This defaults to 0.0.
        /// </summary>
        [DefaultValue(0.0)]
        public virtual double Adder
        {
            get
            {
                return m_adder;
            }
            set
            {
                m_adder = value;
            }
        }

        /// <summary>
        /// Defines a mulplicative offset to apply to the measurement value. This defaults to 1.0.
        /// </summary>
        [DefaultValue(1.0)]
        public virtual double Multiplier
        {
            get
            {
                return m_multiplier;
            }
            set
            {
                m_multiplier = value;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="Measurement"/>.
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
        /// Gets or sets a boolean value that determines if the quality of the numeric value of this <see cref="Measurement"/> is good.
        /// </summary>
        public virtual bool ValueQualityIsGood
        {
            get
            {
                return m_valueQualityIsGood;
            }
            set
            {
                m_valueQualityIsGood = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that determines if the quality of the timestamp of this <see cref="Measurement"/> is good.
        /// </summary>
        public virtual bool TimestampQualityIsGood
        {
            get
            {
                return m_timestampQualityIsGood;
            }
            set
            {
                m_timestampQualityIsGood = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that determines if this <see cref="Measurement"/> has been discarded during sorting.
        /// </summary>
        public virtual bool IsDiscarded
        {
            get
            {
                return m_isDiscarded;
            }
            set
            {
                m_isDiscarded = value;
            }
        }

        /// <summary>
        /// Gets or sets function used to apply a downsampling filter over a sequence of <see cref="IMeasurement"/> values.
        /// </summary>
        public virtual MeasurementValueFilterFunction MeasurementValueFilter
        {
            get
            {
                return m_measurementValueFilter;
            }
            set
            {
                m_measurementValueFilter = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="Measurement"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="Measurement"/>.</returns>
        public override string ToString()
        {
            return Measurement.ToString(this);
        }

        /// <summary>
        /// Determines whether the specified <see cref="IMeasurement"/> is equal to the current <see cref="Measurement"/>.
        /// </summary>
        /// <param name="other">The <see cref="IMeasurement"/> to compare with the current <see cref="Measurement"/>.</param>
        /// <returns>
        /// true if the specified <see cref="IMeasurement"/> is equal to the current <see cref="Measurement"/>;
        /// otherwise, false.
        /// </returns>
        public bool Equals(IMeasurement other)
        {
            return (CompareTo(other) == 0);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Measurement"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Measurement"/>.</param>
        /// <returns>
        /// true if the specified <see cref="Object"/> is equal to the current <see cref="Measurement"/>;
        /// otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IMeasurement"/>.</exception>
        public override bool Equals(object obj)
        {
            IMeasurement other = obj as IMeasurement;
            
            if ((object)other != null)
                return Equals(other);
            
            throw new ArgumentException("Object is not a Measurement");
        }

        /// <summary>
        /// Compares the <see cref="Measurement"/> with an <see cref="IMeasurement"/>.
        /// </summary>
        /// <param name="other">The <see cref="IMeasurement"/> to compare with the current <see cref="Measurement"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <remarks>Measurement implementations should compare by hash code.</remarks>
        public int CompareTo(IMeasurement other)
        {
            if ((object)other != null)
                return GetHashCode().CompareTo(other.GetHashCode());

            return 1;
        }

        /// <summary>
        /// Compares the <see cref="Measurement"/> with the specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Measurement"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IMeasurement"/>.</exception>
        /// <remarks>Measurement implementations should compare by hash code.</remarks>
        public int CompareTo(object obj)
        {
            IMeasurement other = obj as IMeasurement;
            
            if ((object)other != null)
                return CompareTo(other);

            throw new ArgumentException("Measurement can only be compared with other IMeasurements");
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="Measurement"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Measurement"/>.</returns>
        /// <remarks>Hash code based on value of measurement.</remarks>
        public override int GetHashCode()
        {
            return m_key.GetHashCode();
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Compares two <see cref="Measurement"/> values for equality.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator ==(Measurement measurement1, Measurement measurement2)
        {
            return measurement1.Equals(measurement2);
        }

        /// <summary>
        /// Compares two <see cref="Measurement"/> values for inequality.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator !=(Measurement measurement1, Measurement measurement2)
        {
            return !measurement1.Equals(measurement2);
        }

        /// <summary>
        /// Returns true if left <see cref="Measurement"/> value is greater than right <see cref="Measurement"/> value.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator >(Measurement measurement1, Measurement measurement2)
        {
            return measurement1.CompareTo(measurement2) > 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Measurement"/> value is greater than or equal to right <see cref="Measurement"/> value.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator >=(Measurement measurement1, Measurement measurement2)
        {
            return measurement1.CompareTo(measurement2) >= 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Measurement"/> value is less than right <see cref="Measurement"/> value.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator <(Measurement measurement1, Measurement measurement2)
        {
            return measurement1.CompareTo(measurement2) < 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Measurement"/> value is less than or equal to right <see cref="Measurement"/> value.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator <=(Measurement measurement1, Measurement measurement2)
        {
            return measurement1.CompareTo(measurement2) <= 0;
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Creates a copy of the specified measurement.
        /// </summary>
        /// <param name="measurementToClone">Specified measurement to clone.</param>
        /// <returns>A copy of the <see cref="Measurement"/> object.</returns>
        public static Measurement Clone(IMeasurement measurementToClone)
        {
            return new Measurement(measurementToClone.ID, measurementToClone.Source, measurementToClone.SignalID, measurementToClone.Value, measurementToClone.Adder, measurementToClone.Multiplier, measurementToClone.Timestamp);
        }

        /// <summary>
        /// Creates a copy of the specified measurement using a new timestamp.
        /// </summary>
        /// <param name="measurementToClone">Specified measurement to clone.</param>
        /// <param name="timestamp">New timestamp, in ticks, for cloned measurement.</param>
        /// <returns>A copy of the <see cref="Measurement"/> object.</returns>
        public static Measurement Clone(IMeasurement measurementToClone, Ticks timestamp)
        {
            return new Measurement(measurementToClone.ID, measurementToClone.Source, measurementToClone.SignalID, measurementToClone.Value, measurementToClone.Adder, measurementToClone.Multiplier, timestamp);
        }

        /// <summary>
        /// Creates a copy of the specified measurement using a new value and timestamp.
        /// </summary>
        /// <param name="measurementToClone">Specified measurement to clone.</param>
        /// <param name="value">New value for cloned measurement.</param>
        /// <param name="timestamp">New timestamp, in ticks, for cloned measurement.</param>
        /// <returns>A copy of the <see cref="Measurement"/> object.</returns>
        public static Measurement Clone(IMeasurement measurementToClone, double value, Ticks timestamp)
        {
            return new Measurement(measurementToClone.ID, measurementToClone.Source, measurementToClone.SignalID, value, measurementToClone.Adder, measurementToClone.Multiplier, timestamp);
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the specified <see cref="IMeasurement"/>.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> to convert to a <see cref="String"/> representation.</param>
        /// <returns>A <see cref="String"/> that represents the specified <see cref="IMeasurement"/>.</returns>
        public static string ToString(IMeasurement measurement)
        {
            if (measurement == null)
            {
                return "Undefined";
            }
            else
            {
                string tagName = measurement.TagName;
                string keyText = measurement.Key.ToString();

                if (string.IsNullOrWhiteSpace(tagName))
                    return keyText;
                else
                    return string.Format("{0} [{1}]", tagName, keyText);
            }
        }

        /// <summary>
        /// Calculates an average of the specified sequence of <see cref="IMeasurement"/> values.
        /// </summary>
        /// <param name="source">Sequence of <see cref="IMeasurement"/> values over which to run calculation.</param>
        /// <returns>Average of the specified sequence of <see cref="IMeasurement"/> values.</returns>
        public static double AverageValueFilter(IEnumerable<IMeasurement> source)
        {
            return source.Select(m => m.Value).Average();
        }

        /// <summary>
        /// Returns the majority value of the specified sequence of <see cref="IMeasurement"/> values.
        /// </summary>
        /// <param name="source">Sequence of <see cref="IMeasurement"/> values over which to run calculation.</param>
        /// <returns>Majority value of the specified sequence of <see cref="IMeasurement"/> values.</returns>
        public static double MajorityValueFilter(IEnumerable<IMeasurement> source)
        {
            return source.Select(m => m.Value).Majority();
        }

        #endregion
    }
}