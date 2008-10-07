//*******************************************************************************************************
//  BigBinaryValue.cs
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

namespace System
{
    /// <summary>
    /// Represents a big-endian ordered binary data sample stored as a byte array, 
    /// but implicitly castable to most common native types.
    /// </summary>
    public class BigBinaryValue : BinaryValueBase<BigEndianOrder>
    {
        #region [ Constructors ]

        public BigBinaryValue(byte[] buffer)
            : base(buffer)
        {
        }

        #endregion

        #region [ Operators ]

        // Operators cannot be inherited from the base class

        public static implicit operator Byte(BigBinaryValue value)
        {
            return value.m_buffer[0];
        }

        public static implicit operator BigBinaryValue(Byte value)
        {
            return new BigBinaryValue(new byte[] { value });
        }

        public static implicit operator Int16(BigBinaryValue value)
        {
            return value.ToInt16();
        }

        public static implicit operator BigBinaryValue(Int16 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt16(BigBinaryValue value)
        {
            return value.ToUInt16();
        }

        [CLSCompliant(false)]
        public static implicit operator BigBinaryValue(UInt16 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int24(BigBinaryValue value)
        {
            return value.ToInt24();
        }

        public static implicit operator BigBinaryValue(Int24 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt24(BigBinaryValue value)
        {
            return value.ToUInt24();
        }

        [CLSCompliant(false)]
        public static implicit operator BigBinaryValue(UInt24 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int32(BigBinaryValue value)
        {
            return value.ToInt32();
        }

        public static implicit operator BigBinaryValue(Int32 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt32(BigBinaryValue value)
        {
            return value.ToUInt32();
        }

        [CLSCompliant(false)]
        public static implicit operator BigBinaryValue(UInt32 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int64(BigBinaryValue value)
        {
            return value.ToInt64();
        }

        public static implicit operator BigBinaryValue(Int64 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt64(BigBinaryValue value)
        {
            return value.ToUInt64();
        }

        [CLSCompliant(false)]
        public static implicit operator BigBinaryValue(UInt64 value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Single(BigBinaryValue value)
        {
            return value.ToSingle();
        }

        public static implicit operator BigBinaryValue(Single value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Double(BigBinaryValue value)
        {
            return value.ToDouble();
        }

        public static implicit operator BigBinaryValue(Double value)
        {
            return new BigBinaryValue(m_endianOrder.GetBytes(value));
        }

        #endregion

        #region [ Static ]

        static BigBinaryValue()
        {
            m_endianOrder = BigEndianOrder.Default;
        }

        #endregion
    }
}
