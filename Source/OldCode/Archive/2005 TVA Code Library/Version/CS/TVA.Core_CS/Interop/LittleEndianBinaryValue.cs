//*******************************************************************************************************
//  LittleEndianBinaryValue.cs
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
    /// Represents a little-endian ordered binary data sample stored as a byte array,
    /// but implicitly castable to most common native types.
    /// </summary>
    public class LittleEndianBinaryValue
    {
        #region [ Members ]

        // Fields
        private byte[] m_buffer;

        #endregion

        #region [ Constructors ]

        public LittleEndianBinaryValue(byte[] buffer)
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

        public static implicit operator Byte(LittleEndianBinaryValue value)
        {
            return value.m_buffer[0];
        }

        public static implicit operator LittleEndianBinaryValue(Byte value)
        {
            return new LittleEndianBinaryValue(new byte[] { value });
        }

        public static implicit operator Int16(LittleEndianBinaryValue value)
        {
            return EndianOrder.LittleEndian.ToInt16(value.m_buffer, 0);
        }

        public static implicit operator LittleEndianBinaryValue(Int16 value)
        {
            return new LittleEndianBinaryValue(EndianOrder.LittleEndian.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt16(LittleEndianBinaryValue value)
        {
            return EndianOrder.LittleEndian.ToUInt16(value.m_buffer, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator LittleEndianBinaryValue(UInt16 value)
        {
            return new LittleEndianBinaryValue(EndianOrder.LittleEndian.GetBytes(value));
        }

        public static implicit operator Int24(LittleEndianBinaryValue value)
        {
            return EndianOrder.LittleEndian.ToInt24(value.m_buffer, 0);
        }

        public static implicit operator LittleEndianBinaryValue(Int24 value)
        {
            return new LittleEndianBinaryValue(EndianOrder.LittleEndian.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt24(LittleEndianBinaryValue value)
        {
            return EndianOrder.LittleEndian.ToUInt24(value.m_buffer, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator LittleEndianBinaryValue(UInt24 value)
        {
            return new LittleEndianBinaryValue(EndianOrder.LittleEndian.GetBytes(value));
        }

        public static implicit operator Int32(LittleEndianBinaryValue value)
        {
            return EndianOrder.LittleEndian.ToInt32(value.m_buffer, 0);
        }

        public static implicit operator LittleEndianBinaryValue(Int32 value)
        {
            return new LittleEndianBinaryValue(EndianOrder.LittleEndian.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt32(LittleEndianBinaryValue value)
        {
            return EndianOrder.LittleEndian.ToUInt32(value.m_buffer, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator LittleEndianBinaryValue(UInt32 value)
        {
            return new LittleEndianBinaryValue(EndianOrder.LittleEndian.GetBytes(value));
        }

        public static implicit operator Int64(LittleEndianBinaryValue value)
        {
            return EndianOrder.LittleEndian.ToInt64(value.m_buffer, 0);
        }

        public static implicit operator LittleEndianBinaryValue(Int64 value)
        {
            return new LittleEndianBinaryValue(EndianOrder.LittleEndian.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt64(LittleEndianBinaryValue value)
        {
            return EndianOrder.LittleEndian.ToUInt64(value.m_buffer, 0);
        }

        [CLSCompliant(false)]
        public static implicit operator LittleEndianBinaryValue(UInt64 value)
        {
            return new LittleEndianBinaryValue(EndianOrder.LittleEndian.GetBytes(value));
        }

        public static implicit operator Single(LittleEndianBinaryValue value)
        {
            return EndianOrder.LittleEndian.ToSingle(value.m_buffer, 0);
        }

        public static implicit operator LittleEndianBinaryValue(Single value)
        {
            return new LittleEndianBinaryValue(EndianOrder.LittleEndian.GetBytes(value));
        }

        public static implicit operator Double(LittleEndianBinaryValue value)
        {
            return EndianOrder.LittleEndian.ToDouble(value.m_buffer, 0);
        }

        public static implicit operator LittleEndianBinaryValue(Double value)
        {
            return new LittleEndianBinaryValue(EndianOrder.LittleEndian.GetBytes(value));
        }

        #endregion
    }
}
