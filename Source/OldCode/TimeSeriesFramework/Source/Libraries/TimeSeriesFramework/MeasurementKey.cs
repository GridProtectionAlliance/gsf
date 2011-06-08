//******************************************************************************************************
//  MeasurementKey.cs - Gbtc
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
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;

namespace TimeSeriesFramework
{
    /// <summary>
    /// Represents a primary key for a measurement.
    /// </summary>
    [Serializable()]
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
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentNullException("source", "MeasurementKey source cannot be null or empty");

            ConcurrentDictionary<uint, MeasurementKey> keys = s_cache.GetOrAdd(source, key => new ConcurrentDictionary<uint, MeasurementKey>());

            if (!keys.TryGetValue(id, out this))
            {
                m_id = id;
                m_source = source.ToUpper();
                GenHashCode();
                keys[id] = this;
            }
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

        // Static Fields
        private static ConcurrentDictionary<string, ConcurrentDictionary<uint, MeasurementKey>> s_cache = new ConcurrentDictionary<string, ConcurrentDictionary<uint, MeasurementKey>>(StringComparer.InvariantCultureIgnoreCase);

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

            throw new FormatException("The value is not in the correct format for a MeasurementKey value");
        }

        #endregion
    }

    /// <summary>
    /// Represents an instance of the <see cref="IEqualityComparer{T}"/> for a <see cref="MeasurementKey"/>.
    /// </summary>
    public class MeasurementKeyComparer : IEqualityComparer<MeasurementKey>
    {
        #region [ Methods ]

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first <see cref="MeasurementKey"/> to compare.</param>
        /// <param name="y">The second <see cref="MeasurementKey"/> to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(MeasurementKey x, MeasurementKey y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="MeasurementKey"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(MeasurementKey obj)
        {
            return obj.GetHashCode();
        }

        #endregion

        #region [ Static ]

        private static MeasurementKeyComparer s_comparer;

        /// <summary>
        /// Returns the default instance of the <see cref="MeasurementKeyComparer"/> class.
        /// </summary>
        public static MeasurementKeyComparer Default
        {
            get
            {
                if (s_comparer == null)
                    s_comparer = new MeasurementKeyComparer();
                return s_comparer;
            }
        }

        #endregion
    }
}