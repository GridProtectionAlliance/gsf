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

namespace System
{
    /// <summary>
    /// Represents a binary data sample stored as a byte array ordered in the
    /// endianness of the OS, but implicitly castable to most common native types.
    /// </summary>
    public class BinaryValue : BinaryValueBase<NativeEndianOrder>
    {
        #region [ Constructors ]

        public BinaryValue(byte[] buffer)
            : base(buffer)
        {
        }

        #endregion

        #region [ Operators ]

        // Operators cannot be inherited from the base class, they are static methods tied to their class instance

        public static implicit operator Byte(BinaryValue value)
        {
            return value.m_buffer[0];
        }

        public static implicit operator BinaryValue(Byte value)
        {
            return new BinaryValue(new byte[] { value });
        }

        public static implicit operator Int16(BinaryValue value)
        {
            return value.ToInt16();
        }

        public static implicit operator BinaryValue(Int16 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt16(BinaryValue value)
        {
            return value.ToUInt16();
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt16 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int24(BinaryValue value)
        {
            return value.ToInt24();
        }

        public static implicit operator BinaryValue(Int24 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt24(BinaryValue value)
        {
            return value.ToUInt24();
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt24 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int32(BinaryValue value)
        {
            return value.ToInt32();
        }

        public static implicit operator BinaryValue(Int32 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt32(BinaryValue value)
        {
            return value.ToUInt32();
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt32 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Int64(BinaryValue value)
        {
            return value.ToInt64();
        }

        public static implicit operator BinaryValue(Int64 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        [CLSCompliant(false)]
        public static implicit operator UInt64(BinaryValue value)
        {
            return value.ToUInt64();
        }

        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt64 value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Single(BinaryValue value)
        {
            return value.ToSingle();
        }

        public static implicit operator BinaryValue(Single value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        public static implicit operator Double(BinaryValue value)
        {
            return value.ToDouble();
        }

        public static implicit operator BinaryValue(Double value)
        {
            return new BinaryValue(m_endianOrder.GetBytes(value));
        }

        #endregion

        #region [ Static ]

        static BinaryValue()
        {
            m_endianOrder = NativeEndianOrder.Default;
        }

        #endregion
    }
}
