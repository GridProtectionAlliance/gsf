//*******************************************************************************************************
//  BinaryValue.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  10/06/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Interop
{
    /// <summary>
    /// Represents a binary data sample stored as a byte array ordered in the
    /// endianness of the OS, but implicitly castable to most common native types.
    /// </summary>
    public class BinaryValue : BinaryValue<NativeEndianOrder>
    {
        static BinaryValue()
        {
            m_endianOrder = NativeEndianOrder.Default;
        }

        public BinaryValue(byte[] buffer)
            : base(buffer)
        {
        }
    }

    /// <summary>
    /// Represents a little-endian ordered binary data sample stored as a byte array, 
    /// but implicitly castable to most common native types.
    /// </summary>
    public class LittleBinaryValue : BinaryValue<LittleEndianOrder>
    {
        static LittleBinaryValue()
        {
            m_endianOrder = LittleEndianOrder.Default;
        }

        public LittleBinaryValue(byte[] buffer)
            : base(buffer)
        {
        }
    }

    /// <summary>
    /// Represents a big-endian ordered binary data sample stored as a byte array, 
    /// but implicitly castable to most common native types.
    /// </summary>
    public class BigBinaryValue : BinaryValue<BigEndianOrder>
    {
        static BigBinaryValue()
        {
            m_endianOrder = BigEndianOrder.Default;
        }

        public BigBinaryValue(byte[] buffer)
            : base(buffer)
        {
        }
    }

    /// <summary>
    /// Represents a binary data sample stored as a byte array, 
    /// but implicitly castable to most common native types.
    /// </summary>
    public class BinaryValue<TEndianOrder> where TEndianOrder : EndianOrder
    {
        #region [ Members ]

        // Fields
        private byte[] m_buffer;

        #endregion

        #region [ Constructors ]

        protected BinaryValue(byte[] buffer)
        {
            m_buffer = buffer;
        }

        #endregion

        #region [ Properties ]

        public byte[] Buffer
        {
            get
            {
                return m_buffer;
            }
            set
            {
                m_buffer = value;
            }
        }

        #endregion

        #region [ Methods ]

        public Byte ToByte()
        {
            return (Byte)this;
        }

        public Int16 ToInt16()
        {
            return (Int16)this;
        }

        [CLSCompliant(false)]
        public UInt16 ToUInt16()
        {
            return (UInt16)this;
        }

        public Int24 ToInt24()
        {
            return (Int24)this;
        }

        [CLSCompliant(false)]
        public UInt24 ToUInt24()
        {
            return (UInt24)this;
        }

        public Int32 ToInt32()
        {
            return (Int32)this;
        }

        [CLSCompliant(false)]
        public UInt32 ToUInt32()
        {
            return (UInt32)this;
        }

        public Int64 ToInt64()
        {
            return (Int64)this;
        }

        [CLSCompliant(false)]
        public UInt64 ToUInt64()
        {
            return (UInt64)this;
        }

        public Single ToSingle()
        {
            return (Single)this;
        }

        public Double ToDouble()
        {
            return (Double)this;
        }

        #endregion

        #region [ Operators ]

        public static implicit operator Byte(BinaryValue<TEndianOrder> value)
        {
            return value.m_buffer[0];
        }

        public static implicit operator BinaryValue<TEndianOrder>(Byte value)
        {
            return new BinaryValue<TEndianOrder>(new byte[] { value });
        }

        public static implicit operator Int16(BinaryValue<TEndianOrder> value)
        {
            return m_endianOrder.ToInt16(value.m_buffer, 0);
        }

        public static implicit operator BinaryValue<TEndianOrder>(Int16 value)
        {
            return new BinaryValue<TEndianOrder>(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt16(BinaryValue<TEndianOrder> value)
        {
            return m_endianOrder.ToUInt16(value.m_buffer, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryValue<TEndianOrder>(UInt16 value)
        {
            return new BinaryValue<TEndianOrder>(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int24(BinaryValue<TEndianOrder> value)
        {
            return m_endianOrder.ToInt24(value.m_buffer, 0);
        }

        public static implicit operator BinaryValue<TEndianOrder>(Int24 value)
        {
            return new BinaryValue<TEndianOrder>(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt24(BinaryValue<TEndianOrder> value)
        {
            return m_endianOrder.ToUInt24(value.m_buffer, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryValue<TEndianOrder>(UInt24 value)
        {
            return new BinaryValue<TEndianOrder>(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int32(BinaryValue<TEndianOrder> value)
        {
            return m_endianOrder.ToInt32(value.m_buffer, 0);
        }

        public static implicit operator BinaryValue<TEndianOrder>(Int32 value)
        {
            return new BinaryValue<TEndianOrder>(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt32(BinaryValue<TEndianOrder> value)
        {
            return m_endianOrder.ToUInt32(value.m_buffer, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryValue<TEndianOrder>(UInt32 value)
        {
            return new BinaryValue<TEndianOrder>(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int64(BinaryValue<TEndianOrder> value)
        {
            return m_endianOrder.ToInt64(value.m_buffer, 0);
        }

        public static implicit operator BinaryValue<TEndianOrder>(Int64 value)
        {
            return new BinaryValue<TEndianOrder>(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt64(BinaryValue<TEndianOrder> value)
        {
            return m_endianOrder.ToUInt64(value.m_buffer, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryValue<TEndianOrder>(UInt64 value)
        {
            return new BinaryValue<TEndianOrder>(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Single(BinaryValue<TEndianOrder> value)
        {
            return m_endianOrder.ToSingle(value.m_buffer, 0);
        }

        public static implicit operator BinaryValue<TEndianOrder>(Single value)
        {
            return new BinaryValue<TEndianOrder>(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Double(BinaryValue<TEndianOrder> value)
        {
            return m_endianOrder.ToDouble(value.m_buffer, 0);
        }

        public static implicit operator BinaryValue<TEndianOrder>(Double value)
        {
            return new BinaryValue<TEndianOrder>(m_endianOrder.GetBytes(value));
        }

        #endregion

        #region [ Static ]

        // Because each "typed" instance will be it's own class - each will have its own static variable instance
        protected static TEndianOrder m_endianOrder;

        #endregion
    }
}
