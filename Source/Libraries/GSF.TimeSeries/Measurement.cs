//******************************************************************************************************
//  Measurement.cs - Gbtc
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
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GSF.Collections;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a basic measurement implementation.
    /// </summary>
    [Serializable]
    public class Measurement : IMeasurement
    {
        #region [ Members ]

        // Fields
        private double m_value;
        private Ticks m_timestamp;

        [NonSerialized]
        private ShortTime m_lifespan;

        private MeasurementMetadata m_metadata;
        private MeasurementStateFlags m_stateFlags;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> using default settings.
        /// </summary>
        public Measurement()
        {
            m_metadata = MeasurementMetadata.Undefined;
            m_lifespan = ShortTime.Now;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="Guid"/> based signal ID of the <see cref="Measurement"/> implementation.
        /// </summary>
        public Guid ID => m_metadata.Key.SignalID;

        /// <summary>
        /// Gets or sets the raw measurement value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>.
        /// </summary>
        /// <returns>Raw value of this <see cref="Measurement"/> (i.e., value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>).</returns>
        public double Value
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
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="Measurement"/>.
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
            set
            {
                m_timestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets associated metadata values for the <see cref="Measurement"/> implementation.
        /// </summary>
        public MeasurementMetadata Metadata
        {
            get
            {
                return m_metadata;
            }
            set
            {
                m_metadata = value;
            }
        }

        /// <summary>
        /// Gets the primary <see cref="MeasurementKey"/> of this <see cref="Measurement"/> implementation.
        /// </summary>
        public MeasurementKey Key => m_metadata.Key;

        /// <summary>
        /// Gets the text based tag name of this <see cref="Measurement"/> implementation.
        /// </summary>
        public string TagName => m_metadata.TagName;

        /// <summary>
        /// Gets an offset to add to the measurement value. This defaults to 0.0.
        /// </summary>
        [DefaultValue(0.0)]
        public double Adder => m_metadata.Adder;

        /// <summary>
        /// Gets a multiplicative offset to apply to the measurement value. This defaults to 1.0.
        /// </summary>
        [DefaultValue(1.0)]
        public double Multiplier => m_metadata.Multiplier;

        /// <summary>
        /// Gets the adjusted numeric value of this measurement, taking into account the specified <see cref="Measurement.Adder"/> and <see cref="Measurement.Multiplier"/> offsets.
        /// </summary>
        /// <remarks>
        /// Note that returned value will be offset by <see cref="Adder"/> and <see cref="Multiplier"/>.
        /// </remarks>
        /// <returns><see cref="Value"/> offset by <see cref="Adder"/> and <see cref="Multiplier"/> (i.e., <c><see cref="Value"/> * <see cref="Multiplier"/> + <see cref="Adder"/></c>).</returns>
        public double AdjustedValue => Value * Multiplier + Adder;

        /// <summary>
        /// Gets or sets <see cref="MeasurementStateFlags"/> associated with this <see cref="Measurement"/>.
        /// </summary>
        public MeasurementStateFlags StateFlags
        {
            get
            {
                return m_stateFlags;
            }
            set
            {
                m_stateFlags = value;
            }
        }

        /// <summary>
        /// Gets function used to apply a down-sampling filter over a sequence of <see cref="Measurement"/> values.
        /// </summary>
        public MeasurementValueFilterFunction MeasurementValueFilter => m_metadata.MeasurementValueFilter;


        /// <summary>
        /// Gets the life-span of this <see cref="Measurement"/> since its creation.
        /// </summary>
        public ShortTime Lifespan => m_lifespan;

        /// <summary>
        /// Gets timestamp, in ticks, of when this <see cref="Measurement"/> was created.
        /// </summary>
        public Ticks CreatedTimestamp => m_lifespan.UtcTime.Ticks;

        // Big-Endian binary value interpretation
        BigBinaryValue ITimeSeriesValue.Value
        {
            get
            {
                return Value;
            }
            set
            {
                switch (value.TypeCode)
                {
                    case TypeCode.Byte:
                        Value = (byte)value;
                        break;
                    case TypeCode.SByte:
                        Value = (sbyte)value;
                        break;
                    case TypeCode.Int16:
                        Value = (short)value;
                        break;
                    case TypeCode.UInt16:
                        Value = (ushort)value;
                        break;
                    case TypeCode.Int32:
                        Value = (int)value;
                        break;
                    case TypeCode.UInt32:
                        Value = (uint)value;
                        break;
                    case TypeCode.Int64:
                        Value = (long)value;
                        break;
                    case TypeCode.UInt64:
                        Value = (ulong)value;
                        break;
                    case TypeCode.Single:
                        Value = (float)value;
                        break;
                    case TypeCode.Double:
                        Value = (double)value;
                        break;
                    //case TypeCode.Boolean:
                    //    break;
                    //case TypeCode.Char:
                    //    break;
                    //case TypeCode.DateTime:
                    //    break;
                    //case TypeCode.Decimal:
                    //    break;
                    //case TypeCode.String:
                    //    m_value = double.Parse(value);
                    //    break;
                    default:
                        Value = value;
                        break;
                }
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
            return ToString(this);
        }

        /// <summary>
        /// Determines whether the specified <see cref="ITimeSeriesValue"/> is equal to the current <see cref="Measurement"/>.
        /// </summary>
        /// <param name="other">The <see cref="ITimeSeriesValue"/> to compare with the current <see cref="Measurement"/>.</param>
        /// <returns>
        /// true if the specified <see cref="ITimeSeriesValue"/> is equal to the current <see cref="Measurement"/>;
        /// otherwise, false.
        /// </returns>
        public bool Equals(ITimeSeriesValue other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Measurement"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Measurement"/>.</param>
        /// <returns>
        /// true if the specified <see cref="Object"/> is equal to the current <see cref="Measurement"/>;
        /// otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            ITimeSeriesValue other = obj as ITimeSeriesValue;

            if ((object)other != null)
                return Equals(other);

            return false;
        }

        /// <summary>
        /// Compares the <see cref="Measurement"/> with an <see cref="ITimeSeriesValue"/>.
        /// </summary>
        /// <param name="other">The <see cref="ITimeSeriesValue"/> to compare with the current <see cref="Measurement"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <remarks>Measurement implementations should compare by hash code.</remarks>
        public int CompareTo(ITimeSeriesValue other)
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
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="Measurement"/>.</exception>
        /// <remarks>Measurement implementations should compare by hash code.</remarks>
        public int CompareTo(object obj)
        {
            ITimeSeriesValue other = obj as ITimeSeriesValue;

            if ((object)other != null)
                return CompareTo(other);

            throw new ArgumentException("Measurement can only be compared with other measurements or time-series values");
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="Measurement"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Measurement"/>.</returns>
        /// <remarks>Hash code based on value of measurement.</remarks>
        public override int GetHashCode()
        {
            return Key.GetHashCode();
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
            return (object)measurement1 != null && measurement1.Equals(measurement2);
        }

        /// <summary>
        /// Compares two <see cref="Measurement"/> values for inequality.
        /// </summary>
        /// <param name="measurement1">A <see cref="Measurement"/> left hand operand.</param>
        /// <param name="measurement2">A <see cref="Measurement"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator !=(Measurement measurement1, Measurement measurement2)
        {
            return (object)measurement1 != null && !measurement1.Equals(measurement2);
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

        // Static Fields

        /// <summary>
        /// Represents an undefined measurement.
        /// </summary>
        public static readonly Measurement Undefined = new Measurement
        {
            Metadata = MeasurementMetadata.Undefined
        };

        // Static Methods

        /// <summary>
        /// Creates a copy of the specified measurement.
        /// </summary>
        /// <param name="measurementToClone">Specified measurement to clone.</param>
        /// <returns>A copy of the <see cref="Measurement"/> object.</returns>
        public static Measurement Clone(IMeasurement measurementToClone)
        {
            return new Measurement
            {
                Metadata = measurementToClone.Metadata,
                Value = measurementToClone.Value,
                Timestamp = measurementToClone.Timestamp,
                StateFlags = measurementToClone.StateFlags
            };
        }

        /// <summary>
        /// Creates a copy of the specified measurement using a new timestamp.
        /// </summary>
        /// <param name="measurementToClone">Specified measurement to clone.</param>
        /// <param name="timestamp">New timestamp, in ticks, for cloned measurement.</param>
        /// <returns>A copy of the <see cref="Measurement"/> object.</returns>
        public static Measurement Clone(IMeasurement measurementToClone, Ticks timestamp)
        {
            return new Measurement
            {
                Metadata = measurementToClone.Metadata,
                Value = measurementToClone.Value,
                Timestamp = timestamp,
                StateFlags = measurementToClone.StateFlags
            };
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
            return new Measurement
            {
                Metadata = measurementToClone.Metadata,
                Value = value,
                Timestamp = timestamp,
                StateFlags = measurementToClone.StateFlags
            };
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the specified <see cref="IMeasurement"/>.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> to convert to a <see cref="String"/> representation.</param>
        /// <param name="includeTagName">Set to <c>true</c> to include measurement's tag name, if defined; otherwise set to <c>false</c>.</param>
        /// <returns>A <see cref="String"/> that represents the specified <see cref="IMeasurement"/>.</returns>
        public static string ToString(IMeasurement measurement, bool includeTagName = true)
        {
            if ((object)measurement == null)
                return "Undefined";

            string tagName = measurement.TagName;
            string keyText = measurement.Key.ToString();

            if (includeTagName && !string.IsNullOrWhiteSpace(tagName))
                return $"{tagName} [{keyText}]";

            return keyText;
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
