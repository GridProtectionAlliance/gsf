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

        /// <summary>Creates a new binary value, ordered in the endianness of the OS, from the given byte array.</summary>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        /// <param name="startIndex">The offset in the buffer where the data starts.</param>
        /// <param name="length">The number of data bytes that make up the binary value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of the <paramref name="buffer"/> -or-
        /// <paramref name="length"/> is less than 0 -or-
        /// <paramref name="startIndex"/> and <paramref name="length"/> do not specify a valid region in the <paramref name="buffer"/>
        /// </exception>
        public BinaryValue(byte[] buffer, int startIndex, int length)
            : base(buffer, startIndex, length)
        {
        }

        /// <summary>Creates a new binary value, ordered in the endianness of the OS, from the given byte array.</summary>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        public BinaryValue(byte[] buffer)
            : base(buffer, 0, buffer.Length)
        {
        }

        #endregion

        #region [ Operators ]

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
