//*******************************************************************************************************
//  MeasurementKey.cs
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
//  12/8/2005 - J. Ritchie Carroll
//      Initial version of source generated.
//  09/16/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA.Measurements
{
    /// <summary>Defines a primary key for a measurement</summary>
    public struct MeasurementKey : IEquatable<MeasurementKey>, IComparable<MeasurementKey>, IComparable
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private string m_source;
        private int m_hashCode;

        #endregion

        #region [ Constructors ]

        public MeasurementKey(int id, string source)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException("source", "MeasurementKey source cannot be null");

            m_id = id;
            m_source = source.ToUpper();
            GenHashCode();
        }

        #endregion

        #region [ Properties ]

        public int ID
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

        public override string ToString()
        {
            return string.Format("{0}:{1}", m_source, m_id);
        }

        public override int GetHashCode()
        {
            return m_hashCode;
        }

        public bool Equals(MeasurementKey other)
        {
            return (m_hashCode == other.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            // Can't use cast "as" on a structure...
            if (obj is MeasurementKey) return Equals((MeasurementKey)obj);
            throw new ArgumentException("Object is not a MeasurementKey");
        }

        public int CompareTo(MeasurementKey other)
        {
            int sourceCompare = string.Compare(m_source, other.Source, true);

            if (sourceCompare == 0)
                return (m_id < other.ID ? -1 : (m_id > other.ID ? 1 : 0));
            else
                return sourceCompare;
        }

        public int CompareTo(object obj)
        {
            // Can't use cast "as" on a structure...
            if (obj is MeasurementKey) return CompareTo((MeasurementKey)obj);
            throw new ArgumentException("Object is not a MeasurementKey");
        }

        private void GenHashCode()
        {
            // We cache hash code during construction or after element value change to speed structure usage
            m_hashCode = (m_source + m_id.ToString()).GetHashCode();
        }

        #endregion

        #region [ Operators ]

        public static bool operator ==(MeasurementKey key1, MeasurementKey key2)
        {
            return key1.Equals(key2);
        }

        public static bool operator !=(MeasurementKey key1, MeasurementKey key2)
        {
            return !key1.Equals(key2);
        }

        public static bool operator >(MeasurementKey key1, MeasurementKey key2)
        {
            return key1.CompareTo(key2) > 0;
        }

        public static bool operator >=(MeasurementKey key1, MeasurementKey key2)
        {
            return key1.CompareTo(key2) >= 0;
        }

        public static bool operator <(MeasurementKey key1, MeasurementKey key2)
        {
            return key1.CompareTo(key2) < 0;
        }

        public static bool operator <=(MeasurementKey key1, MeasurementKey key2)
        {
            return key1.CompareTo(key2) <= 0;
        }

        #endregion
    }
}