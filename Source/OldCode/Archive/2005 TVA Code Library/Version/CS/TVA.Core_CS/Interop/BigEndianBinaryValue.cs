//*******************************************************************************************************
//  BigEndianBinaryValue.cs
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
//  10/03/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Interop
{
    /// <summary>
    /// Represents a big-endian ordered binary data sample stored as a byte array, 
    /// but implicitly castable to most common native types.
    /// </summary>
    public class BigEndianBinaryValue
    {
        #region [ Members ]

        // Fields
        private byte[] m_buffer;

        #endregion

        #region [ Constructors ]

        public BigEndianBinaryValue(byte[] buffer)
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

        public static implicit operator Byte(BigEndianBinaryValue value)
        {
            return value.m_buffer[0];
        }

        public static implicit operator BigEndianBinaryValue(Byte value)
        {
            return new BigEndianBinaryValue(new byte[] { value });
        }

        public static implicit operator Int16(BigEndianBinaryValue value)
        {
            return EndianOrder.BigEndian.ToInt16(value.m_buffer, 0);
        }

        public static implicit operator BigEndianBinaryValue(Int16 value)
        {
            return new BigEndianBinaryValue(EndianOrder.BigEndian.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt16(BigEndianBinaryValue value)
        {
            return EndianOrder.BigEndian.ToUInt16(value.m_buffer, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator BigEndianBinaryValue(UInt16 value)
        {
            return new BigEndianBinaryValue(EndianOrder.BigEndian.GetBytes(value));
        }

        public static implicit operator Int24(BigEndianBinaryValue value)
        {
            return EndianOrder.BigEndian.ToInt24(value.m_buffer, 0);
        }

        public static implicit operator BigEndianBinaryValue(Int24 value)
        {
            return new BigEndianBinaryValue(EndianOrder.BigEndian.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt24(BigEndianBinaryValue value)
        {
            return EndianOrder.BigEndian.ToUInt24(value.m_buffer, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator BigEndianBinaryValue(UInt24 value)
        {
            return new BigEndianBinaryValue(EndianOrder.BigEndian.GetBytes(value));
        }

        public static implicit operator Int32(BigEndianBinaryValue value)
        {
            return EndianOrder.BigEndian.ToInt32(value.m_buffer, 0);
        }

        public static implicit operator BigEndianBinaryValue(Int32 value)
        {
            return new BigEndianBinaryValue(EndianOrder.BigEndian.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt32(BigEndianBinaryValue value)
        {
            return EndianOrder.BigEndian.ToUInt32(value.m_buffer, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator BigEndianBinaryValue(UInt32 value)
        {
            return new BigEndianBinaryValue(EndianOrder.BigEndian.GetBytes(value));
        }

        public static implicit operator Int64(BigEndianBinaryValue value)
        {
            return EndianOrder.BigEndian.ToInt64(value.m_buffer, 0);
        }

        public static implicit operator BigEndianBinaryValue(Int64 value)
        {
            return new BigEndianBinaryValue(EndianOrder.BigEndian.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt64(BigEndianBinaryValue value)
        {
            return EndianOrder.BigEndian.ToUInt64(value.m_buffer, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator BigEndianBinaryValue(UInt64 value)
        {
            return new BigEndianBinaryValue(EndianOrder.BigEndian.GetBytes(value));
        }

        public static implicit operator Single(BigEndianBinaryValue value)
        {
            return EndianOrder.BigEndian.ToSingle(value.m_buffer, 0);
        }

        public static implicit operator BigEndianBinaryValue(Single value)
        {
            return new BigEndianBinaryValue(EndianOrder.BigEndian.GetBytes(value));
        }

        public static implicit operator Double(BigEndianBinaryValue value)
        {
            return EndianOrder.BigEndian.ToDouble(value.m_buffer, 0);
        }

        public static implicit operator BigEndianBinaryValue(Double value)
        {
            return new BigEndianBinaryValue(EndianOrder.BigEndian.GetBytes(value));
        }

        #endregion
    }
}
