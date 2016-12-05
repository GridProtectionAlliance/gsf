using System.Runtime.InteropServices;

namespace OGE.MeasurementStream
{
    [StructLayout(LayoutKind.Explicit)]
    internal class UnionValues
    {
        [FieldOffset(0)]
        private MeasurementTypeCode m_code;
        [FieldOffset(4)]
        private float m_valueSingle;
        [FieldOffset(4)]
        private uint m_valueUInt32;
        [FieldOffset(4)]
        private int m_valueInt32;
        [FieldOffset(8)]
        private double m_valueDouble;
        [FieldOffset(8)]
        private ulong m_valueUInt64;
        [FieldOffset(8)]
        private long m_valueInt64;

        public float ValueSingle
        {
            get
            {
                return m_valueSingle;
            }
            set
            {
                m_code = MeasurementTypeCode.Single;
                m_valueSingle = value;
            }
        }

        public uint ValueUInt32
        {
            get
            {
                return m_valueUInt32;
            }
            set
            {
                m_code = MeasurementTypeCode.UInt32;
                m_valueUInt32 = value;
            }
        }

        public int ValueInt32
        {
            get
            {
                return m_valueInt32;
            }
            set
            {
                m_code = MeasurementTypeCode.Int32;
                m_valueInt32 = value;
            }
        }

        public double ValueDouble
        {
            get
            {
                return m_valueDouble;
            }
            set
            {
                m_code = MeasurementTypeCode.Double;
                m_valueDouble = value;
            }
        }

        public ulong ValueUInt64
        {
            get
            {
                return m_valueUInt64;
            }
            set
            {
                m_code = MeasurementTypeCode.UInt64;
                m_valueUInt64 = value;
            }
        }


        public long ValueInt64
        {
            get
            {
                return m_valueInt64;
            }
            set
            {
                m_code = MeasurementTypeCode.Int64;
                m_valueInt64 = value;
            }
        }

        public MeasurementTypeCode Code
        {
            get
            {
                return m_code;
            }
            set
            {
                m_code = value;
            }
        }

        public UnionValues()
        {
            
        }

        public UnionValues(float value)
        {
            ValueSingle = value;
        }

        public UnionValues(uint value)
        {
            ValueUInt32 = value;
        }

        public UnionValues(int value)
        {
            ValueInt32 = value;
        }

        public UnionValues(double value)
        {
            ValueDouble = value;
        }

        public UnionValues(ulong value)
        {
            ValueUInt64 = value;
        }

        public UnionValues(long value)
        {
            ValueInt64 = value;
        }

        public static implicit operator UnionValues(float value)
        {
            return new UnionValues(value);
        }

        public static implicit operator UnionValues(uint value)
        {
            return new UnionValues(value);
        }

        public static implicit operator UnionValues(int value)
        {
            return new UnionValues(value);
        }

        public static implicit operator UnionValues(double value)
        {
            return new UnionValues(value);
        }

        public static implicit operator UnionValues(ulong value)
        {
            return new UnionValues(value);
        }

        public static implicit operator UnionValues(long value)
        {
            return new UnionValues(value);
        }

    }
}