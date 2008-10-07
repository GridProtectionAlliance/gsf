//*******************************************************************************************************
//  LittleBinaryValue.cs
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
    /// Represents a little-endian ordered binary data sample stored as a byte array, 
    /// but implicitly castable to most common native types.
    /// </summary>
    public class LittleBinaryValue : BinaryValueBase<LittleEndianOrder>
    {
        #region [ Constructors ]

        public LittleBinaryValue(byte[] buffer)
            : base(buffer)
        {
        }

        #endregion

        #region [ Operators ]

        // Operators cannot be inherited from the base class

        public static implicit operator Byte(LittleBinaryValue value)
        {
            return value.m_buffer[0];
        }

        public static implicit operator LittleBinaryValue(Byte value)
        {
            return new LittleBinaryValue(new byte[] { value });
        }

        public static implicit operator Int16(LittleBinaryValue value)
        {
            return value.ToInt16();
        }

        public static implicit operator LittleBinaryValue(Int16 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt16(LittleBinaryValue value)
        {
            return value.ToUInt16();
        }

        [CLSCompliant(false)]
        public static implicit operator LittleBinaryValue(UInt16 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int24(LittleBinaryValue value)
        {
            return value.ToInt24();
        }

        public static implicit operator LittleBinaryValue(Int24 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt24(LittleBinaryValue value)
        {
            return value.ToUInt24();
        }

        [CLSCompliant(false)]
        public static implicit operator LittleBinaryValue(UInt24 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int32(LittleBinaryValue value)
        {
            return value.ToInt32();
        }

        public static implicit operator LittleBinaryValue(Int32 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt32(LittleBinaryValue value)
        {
            return value.ToUInt32();
        }

        [CLSCompliant(false)]
        public static implicit operator LittleBinaryValue(UInt32 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int64(LittleBinaryValue value)
        {
            return value.ToInt64();
        }

        public static implicit operator LittleBinaryValue(Int64 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt64(LittleBinaryValue value)
        {
            return value.ToUInt64();
        }

        [CLSCompliant(false)]
        public static implicit operator LittleBinaryValue(UInt64 value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Single(LittleBinaryValue value)
        {
            return value.ToSingle();
        }

        public static implicit operator LittleBinaryValue(Single value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Double(LittleBinaryValue value)
        {
            return value.ToDouble();
        }

        public static implicit operator LittleBinaryValue(Double value)
        {
            return new LittleBinaryValue(m_endianOrder.GetBytes(value));
        }

        #endregion

        #region [ Static ]

        static LittleBinaryValue()
        {
            m_endianOrder = LittleEndianOrder.Default;
        }

        #endregion
    }
}
