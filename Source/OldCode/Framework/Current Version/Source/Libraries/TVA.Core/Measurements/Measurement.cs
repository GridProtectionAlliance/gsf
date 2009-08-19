//*******************************************************************************************************
//  Measurement.cs
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
//  12/8/2005 - James R. Carroll
//       Initial version of source generated
//  09/16/2008 - James R. Carroll
//      Converted to C#.
//  08/06/2009 - Josh Patterson
//      Edited Comments
//
//*******************************************************************************************************

using System;
using System.ComponentModel;

namespace TVA.Measurements
{
    /// <summary>
    /// Implementation of a basic measurement.
    /// </summary>
    [CLSCompliant(false)]
    public class Measurement : IMeasurement
    {
        #region [ Members ]

        // Fields
        private uint m_id;
        private string m_source;
        private MeasurementKey m_key;
        private string m_tagName;
        private Ticks m_timestamp;
        private double m_value;
        private double m_adder;
        private double m_multiplier;
        private bool m_valueQualityIsGood;
        private bool m_timestampQualityIsGood;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> using default settings.
        /// </summary>
        public Measurement()
            : this(uint.MaxValue, "__", double.NaN, 0.0, 1.0, 0)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the new measurement.</param>
        /// <param name="source">Source name of the new measurement.</param>
        public Measurement(uint id, string source)
            : this(id, source, double.NaN, 0.0, 1.0, 0)
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
            : this(id, source, value, 0.0, 1.0, timestamp)
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
            : this(id, source, double.NaN, adder, multiplier, 0)
        {
            m_tagName = tagName;
        }

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the new measurement.</param>
        /// <param name="source">Source name of the new measurement.</param>
        /// <param name="value">Value of the new measurement.</param>
        /// <param name="adder">Defined adder to apply to the new measurement.</param>
        /// <param name="multiplier">Defined multiplier to apply to the new measurement.</param>
        /// <param name="timestamp">Timestamp, in ticks, of the new measurement.</param>
        public Measurement(uint id, string source, double value, double adder, double multiplier, Ticks timestamp)
        {
            m_id = id;
            m_source = source;
            m_key = new MeasurementKey(m_id, m_source);
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
        /// Gets or sets a boolean value determining if the quality of the numeric value of this <see cref="Measurement"/> is good.
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
        /// Gets or sets a boolean value determining if the quality of the timestamp of this <see cref="Measurement"/> is good.
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
        /// <remarks>This implementation of a basic measurement compares itself by value.</remarks>
        public int CompareTo(IMeasurement other)
        {
            return m_value.CompareTo(other.Value);
        }

        /// <summary>
        /// Compares the <see cref="Measurement"/> with the specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Measurement"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IMeasurement"/>.</exception>
        /// <remarks>This implementation of a basic measurement compares itself by value.</remarks>
        public int CompareTo(object obj)
        {
            IMeasurement other = obj as IMeasurement;
            
            if ((object)other != null)
                return CompareTo(other);

            throw new ArgumentException("Measurement can only be compared with other IMeasurements...");
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="Measurement"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Measurement"/>.</returns>
        /// <remarks>Hash code based on value of measurement.</remarks>
        public override int GetHashCode()
        {
            return m_value.GetHashCode();
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
            return new Measurement(measurementToClone.ID, measurementToClone.Source, measurementToClone.Value, measurementToClone.Adder, measurementToClone.Multiplier, measurementToClone.Timestamp);
        }

        /// <summary>
        /// Creates a copy of the specified measurement using a new timestamp.
        /// </summary>
        /// <param name="measurementToClone">Specified measurement to clone.</param>
        /// <param name="timestamp">New timestamp, in ticks, for cloned measurement.</param>
        /// <returns>A copy of the <see cref="Measurement"/> object.</returns>
        public static Measurement Clone(IMeasurement measurementToClone, Ticks timestamp)
        {
            return new Measurement(measurementToClone.ID, measurementToClone.Source, measurementToClone.Value, measurementToClone.Adder, measurementToClone.Multiplier, timestamp);
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
            return new Measurement(measurementToClone.ID, measurementToClone.Source, value, measurementToClone.Adder, measurementToClone.Multiplier, timestamp);
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

                if (string.IsNullOrEmpty(tagName))
                    return keyText;
                else
                    return string.Format("{0} [{1}]", tagName, keyText);
            }
        }

        #endregion
    }
}