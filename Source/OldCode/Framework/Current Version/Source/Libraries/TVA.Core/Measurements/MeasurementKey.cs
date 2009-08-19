//*******************************************************************************************************
//  MeasurementKey.cs
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
//      Initial version of source generated.
//  09/16/2008 - James R. Carroll
//      Converted to C#.
//  08/06/2009 - Josh Patterson
//      Edited Comments
//
//*******************************************************************************************************

using System;

namespace TVA.Measurements
{
    /// <summary>
    /// Represents a primary key for a measurement.
    /// </summary>
    [CLSCompliant(false)]
    public struct MeasurementKey : IEquatable<MeasurementKey>, IComparable<MeasurementKey>, IComparable
    {
        #region [ Members ]

        // Fields
        private uint m_id;
        private string m_source;
        private int m_hashCode;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="MeasurementKey"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the measurement that this <see cref="MeasurementKey"/> represents.</param>
        /// <param name="source">Source of the measurement that this <see cref="MeasurementKey"/> represents (e.g., name of archive).</param>
        /// <exception cref="ArgumentNullException">source cannot be null.</exception>
        public MeasurementKey(uint id, string source)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException("source", "MeasurementKey source cannot be null");

            m_id = id;
            m_source = source.ToUpper();
            m_hashCode = 0;
            
            GenHashCode();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the numeric ID of this <see cref="MeasurementKey"/>.
        /// </summary>
        public uint ID
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
                    GenHashCode();
                }
            }
        }

        /// <summary>
        /// Gets or sets the source of this <see cref="MeasurementKey"/>.
        /// </summary>
        /// <remarks>
        /// This value is typically used to track the archive name in which the measurement, that this <see cref="MeasurementKey"/> represents, is stored.
        /// </remarks>
        public string Source
        {
            get
            {
                return m_source;
            }
            set
            {
                if (string.Compare(m_source, value, true) != 0)
                {
                    m_source = value.ToUpper();
                    GenHashCode();
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="MeasurementKey"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="MeasurementKey"/>.</returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", m_source, m_id);
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="MeasurementKey"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="MeasurementKey"/>.</returns>
        public override int GetHashCode()
        {
            return m_hashCode;
        }

        /// <summary>
        /// Determines whether the specified <see cref="MeasurementKey"/> is equal to the current <see cref="MeasurementKey"/>.
        /// </summary>
        /// <param name="other">The <see cref="MeasurementKey"/> to compare with the current <see cref="MeasurementKey"/>.</param>
        /// <returns>
        /// true if the specified <see cref="MeasurementKey"/> is equal to the current <see cref="MeasurementKey"/>;
        /// otherwise, false.
        /// </returns>
        public bool Equals(MeasurementKey other)
        {
            return (m_hashCode == other.m_hashCode);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="MeasurementKey"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="MeasurementKey"/>.</param>
        /// <returns>
        /// true if the specified <see cref="Object"/> is equal to the current <see cref="MeasurementKey"/>;
        /// otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not a <see cref="MeasurementKey"/>.</exception>
        public override bool Equals(object obj)
        {
            // Can't use cast "as" on a structure...
            if (obj is MeasurementKey)
                return Equals((MeasurementKey)obj);

            return false;
        }

        /// <summary>
        /// Compares the <see cref="MeasurementKey"/> with another <see cref="MeasurementKey"/>.
        /// </summary>
        /// <param name="other">The <see cref="MeasurementKey"/> to compare with the current <see cref="MeasurementKey"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(MeasurementKey other)
        {
            int sourceCompare = string.Compare(m_source, other.Source, true);

            if (sourceCompare == 0)
                return (m_id < other.ID ? -1 : (m_id > other.ID ? 1 : 0));
            else
                return sourceCompare;
        }

        /// <summary>
        /// Compares the <see cref="MeasurementKey"/> with the specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="MeasurementKey"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not a <see cref="MeasurementKey"/>.</exception>
        public int CompareTo(object obj)
        {
            // Can't use cast "as" on a structure...
            if (obj is MeasurementKey)
                return CompareTo((MeasurementKey)obj);

            throw new ArgumentException("Object is not a MeasurementKey");
        }

        private void GenHashCode()
        {
            // We cache hash code during construction or after element value change to speed structure usage
            m_hashCode = (m_source + m_id.ToString()).GetHashCode();
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Compares two <see cref="MeasurementKey"/> values for equality.
        /// </summary>
        /// <param name="key1">A <see cref="MeasurementKey"/> left hand operand.</param>
        /// <param name="key2">A <see cref="MeasurementKey"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator ==(MeasurementKey key1, MeasurementKey key2)
        {
            return key1.Equals(key2);
        }

        /// <summary>
        /// Compares two <see cref="MeasurementKey"/> values for inequality.
        /// </summary>
        /// <param name="key1">A <see cref="MeasurementKey"/> left hand operand.</param>
        /// <param name="key2">A <see cref="MeasurementKey"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator !=(MeasurementKey key1, MeasurementKey key2)
        {
            return !key1.Equals(key2);
        }

        /// <summary>
        /// Returns true if left <see cref="MeasurementKey"/> value is greater than right <see cref="MeasurementKey"/> value.
        /// </summary>
        /// <param name="key1">A <see cref="MeasurementKey"/> left hand operand.</param>
        /// <param name="key2">A <see cref="MeasurementKey"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator >(MeasurementKey key1, MeasurementKey key2)
        {
            return key1.CompareTo(key2) > 0;
        }

        /// <summary>
        /// Returns true if left <see cref="MeasurementKey"/> value is greater than or equal to right <see cref="MeasurementKey"/> value.
        /// </summary>
        /// <param name="key1">A <see cref="MeasurementKey"/> left hand operand.</param>
        /// <param name="key2">A <see cref="MeasurementKey"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator >=(MeasurementKey key1, MeasurementKey key2)
        {
            return key1.CompareTo(key2) >= 0;
        }

        /// <summary>
        /// Returns true if left <see cref="MeasurementKey"/> value is less than right <see cref="MeasurementKey"/> value.
        /// </summary>
        /// <param name="key1">A <see cref="MeasurementKey"/> left hand operand.</param>
        /// <param name="key2">A <see cref="MeasurementKey"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator <(MeasurementKey key1, MeasurementKey key2)
        {
            return key1.CompareTo(key2) < 0;
        }

        /// <summary>
        /// Returns true if left <see cref="MeasurementKey"/> value is less than or equal to right <see cref="MeasurementKey"/> value.
        /// </summary>
        /// <param name="key1">A <see cref="MeasurementKey"/> left hand operand.</param>
        /// <param name="key2">A <see cref="MeasurementKey"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator <=(MeasurementKey key1, MeasurementKey key2)
        {
            return key1.CompareTo(key2) <= 0;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Converts the string representation of a <see cref="MeasurementKey"/> into its value equivalent.
        /// </summary>
        /// <param name="value">A string representing the <see cref="MeasurementKey"/> to convert.</param>
        /// <returns>A <see cref="MeasurementKey"/> value equivalent the representation contained in <paramref name="value"/>.</returns>
        /// <exception cref="FormatException">The value is not in the correct format for a <see cref="MeasurementKey"/> value.</exception>
        public static MeasurementKey Parse(string value)
        {
            string[] elem = value.Trim().Split(':');

            if (elem.Length == 2)
                return new MeasurementKey(uint.Parse(elem[1].Trim()), elem[0].Trim());

            throw new FormatException("The value is not in the correct format for a MeasurementKey value.");
        }

        #endregion
    }
}