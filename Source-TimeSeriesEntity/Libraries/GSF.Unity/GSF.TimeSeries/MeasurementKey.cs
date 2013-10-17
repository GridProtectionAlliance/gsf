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

//using GSF.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
//using System.Data;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a primary key for a measurement.
    /// </summary>
    [Serializable()]
    public struct MeasurementKey : IEquatable<MeasurementKey>, IComparable<MeasurementKey>, IComparable
    {
        #region [ Members ]

        // Fields
        private Guid m_signalID;
        private uint m_id;
        private string m_source;
        private int m_hashCode;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="MeasurementKey"/> given the specified parameters.
        /// </summary>
        /// <param name="signalID"><see cref="Guid"/> ID of associated signal, if defined.</param>
        /// <param name="id">Numeric ID of the measurement that this <see cref="MeasurementKey"/> represents.</param>
        /// <param name="source">Source of the measurement that this <see cref="MeasurementKey"/> represents (e.g., name of archive).</param>
        /// <exception cref="ArgumentNullException">source cannot be null.</exception>
        public MeasurementKey(Guid signalID, uint id, string source)
        {
            if (source.IsNullOrWhiteSpace())
                throw new ArgumentNullException("source", "MeasurementKey source cannot be null or empty");

            if (signalID == Guid.Empty)
            {
                ConcurrentDictionary<uint, MeasurementKey> keys = s_keyCache.GetOrAdd(source, key => new ConcurrentDictionary<uint, MeasurementKey>());

                if (!keys.TryGetValue(id, out this))
                {
                    m_id = id;
                    m_source = source.ToUpper();

                    // Generate a static runtime signal ID associated with this measurement key
                    m_signalID = Guid.NewGuid();
                    GenHashCode();

                    // Cache measurement
                    s_idCache[m_signalID] = this;
                    keys[id] = this;
                }
            }
            else
            {
                if (!s_idCache.TryGetValue(signalID, out this))
                {
                    m_id = id;
                    m_source = source.ToUpper();
                    m_signalID = signalID;
                    GenHashCode();

                    // Cache measurement based on signal ID
                    s_idCache[m_signalID] = this;
                    s_keyCache.GetOrAdd(source, kcf => new ConcurrentDictionary<uint, MeasurementKey>())[id] = this;
                }
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="Guid"/> ID of signal associated with this <see cref="MeasurementKey"/>.
        /// </summary>
        public Guid SignalID
        {
            get
            {
                return m_signalID;
            }
            set
            {
                if (m_signalID != value)
                {
                    MeasurementKey old;
                    s_idCache.TryRemove(m_signalID, out old);

                    m_signalID = value;
                    GenHashCode();

                    // Update measurement caches
                    s_idCache[m_signalID] = this;
                    s_keyCache.GetOrAdd(m_source, kcf => new ConcurrentDictionary<uint, MeasurementKey>())[m_id] = this;
                }
            }
        }

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
                m_id = value;
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
                m_source = value.ToUpper();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Update the signal ID of this <see cref="MeasurementKey"/>.
        /// </summary>
        /// <param name="signalID">New <see cref="Guid"/> based signal ID to update this <see cref="MeasurementKey"/> with.</param>
        public void UpdateSignalID(Guid signalID)
        {
            SignalID = signalID;
        }

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
            return (m_signalID == other.m_signalID);
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
            return m_signalID.CompareTo(other.m_signalID);
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
            m_hashCode = m_signalID.GetHashCode();
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
        private static readonly ConcurrentDictionary<Guid, MeasurementKey> s_idCache = new ConcurrentDictionary<Guid, MeasurementKey>();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<uint, MeasurementKey>> s_keyCache = new ConcurrentDictionary<string, ConcurrentDictionary<uint, MeasurementKey>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Represents an undefined measurement key.
        /// </summary>
        public static readonly MeasurementKey Undefined = CreateUndefinedMeasurementKey();

        // Static Methods
  
        public static MeasurementKey Parse(string value)
        {
            return Parse (value, default(Guid));
        }
        
        /// <summary>
        /// Converts the string representation of a <see cref="MeasurementKey"/> into its value equivalent.
        /// </summary>
        /// <param name="value">A string representing the <see cref="MeasurementKey"/> to convert.</param>
        /// <param name="signalID"><see cref="Guid"/> based signal ID associated with this <see cref="MeasurementKey"/>, if defined.</param>
        /// <returns>A <see cref="MeasurementKey"/> value equivalent the representation contained in <paramref name="value"/>.</returns>
        /// <exception cref="FormatException">The value is not in the correct format for a <see cref="MeasurementKey"/> value.</exception>
        public static MeasurementKey Parse(string value, Guid signalID)
        {
            MeasurementKey key;

            if (TryParse(value, signalID, out key))
                return key;

            throw new FormatException("The value is not in the correct format for a MeasurementKey value");
        }

        /// <summary>
        /// Looks up the measurement key associated with the given signal ID.
        /// </summary>
        /// <param name="signalID">The signal ID of the measurement key.</param>
        /// <returns>The measurement key associated with the given signal ID.</returns>
        /// <remarks>
        /// If no measurement key is found with the given signal ID, a measurement key is
        /// returned with generic default values for <see cref="Source"/> and <see cref="ID"/>,
        /// but this key is not cached. In order to cache a measurement key, a Source and ID
        /// must be provided, either via the <see cref="MeasurementKey(Guid, uint, string)"/>
        /// constructor or the <see cref="Parse"/> method.
        /// </remarks>
        public static MeasurementKey LookupBySignalID(Guid signalID)
        {
            MeasurementKey key;

            if (signalID == Guid.Empty)
            {
                key = Undefined;
            }
            else if (!s_idCache.TryGetValue(signalID, out key))
            {
                key = new MeasurementKey();
                key.m_signalID = signalID;
                key.m_source = "__";
                key.m_id = 0;
            }

            return key;
        }

        /// <summary>
        /// Attempts to convert the string representation of a <see cref="MeasurementKey"/> into its value equivalent.
        /// </summary>
        /// <param name="value">A string representing the <see cref="MeasurementKey"/> to convert.</param>
        /// <param name="signalID"><see cref="Guid"/> based signal ID associated with this <see cref="MeasurementKey"/>, if defined.</param>
        /// <param name="key">Output <see cref="MeasurementKey"/> in which to stored parsed value.</param>
        /// <returns>A <c>true</c> if <see cref="MeasurementKey"/>representation contained in <paramref name="value"/> could be parsed; otherwise <c>false</c>.</returns>
        public static bool TryParse(string value, Guid signalID, out MeasurementKey key)
        {
            // Check cache for an existing measurment key definition if signal ID is defined
            if (signalID != Guid.Empty && s_idCache.TryGetValue(signalID, out key))
                return true;

            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    string[] elem = value.Split(':');

                    if (elem.Length == 2)
                    {
                        uint id;

                        if (uint.TryParse(elem[1].Trim(), out id))
                        {
                            key = new MeasurementKey(signalID, id, elem[0].Trim());
                            return true;
                        }
                    }
                }
                catch
                {
                    // Any exceptions result in failure to parse
                }
            }

            key = default(MeasurementKey);
            return false;
        }
  
//        public static void EstablishDefaultCache(IDbConnection connection, Type adapterType)
//        {
//            EstablishDefaultCache(connection, adapterType, "ActiveMeasurement");
//        }
//        
//        /// <summary>
//        /// Establish default <see cref="MeasurementKey"/> cache.
//        /// </summary>
//        /// <param name="connection">The database connection.</param>
//        /// <param name="adapterType">The database adapter type.</param>
//        /// <param name="measurementTable">Measurement table name used to load measurement key cache.</param>
//        /// <remarks>
//        /// Source tables are expected to have at least the following fields:
//        /// <code>
//        ///      ID          NVARCHAR    Measurement key formatted as: ArchiveSource:PointID
//        ///      SignalID    GUID        Unique identification for measurement
//        /// </code>
//        /// </remarks>
//        public static void EstablishDefaultCache(IDbConnection connection, Type adapterType, string measurementTable)
//        {
//            MeasurementKey key;
//
//            // Establish default measurement key cache
//            foreach (DataRow measurement in connection.RetrieveData(adapterType, string.Format("SELECT ID, SignalID FROM {0}", measurementTable)).Rows)
//            {
//                TryParse(measurement["ID"].ToString(), measurement["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>(), out key);
//            }
//        }

        /// <summary>
        /// Creates the undefined measurement key. Used to initialize <see cref="Undefined"/>.
        /// </summary>
        /// <returns>The undefined measurement key.</returns>
        private static MeasurementKey CreateUndefinedMeasurementKey()
        {
            MeasurementKey key = new MeasurementKey();

            key.m_signalID = Guid.Empty;
            key.m_source = "__";
            key.m_id = uint.MaxValue;
            key.m_hashCode = int.MaxValue;

            s_keyCache.GetOrAdd("__", kcf => new ConcurrentDictionary<uint, MeasurementKey>())[uint.MaxValue] = key;

            return key;
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