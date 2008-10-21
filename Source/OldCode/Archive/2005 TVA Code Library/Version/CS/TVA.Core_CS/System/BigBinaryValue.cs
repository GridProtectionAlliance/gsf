/**************************************************************************\
   Copyright (c) 2008 - Gbtc, James Ritchie Carroll
   All rights reserved.
  
   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
       
      * Redistributions in binary form must reproduce the above
        copyright notice, this list of conditions and the following
        disclaimer in the documentation and/or other materials provided
        with the distribution.
  
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY
   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
   PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
   OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
   OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  
\**************************************************************************/

namespace System
{
    /// <summary>
    /// Represents a big-endian ordered binary data sample stored as a byte array, 
    /// but implicitly castable to most common native types.
    /// </summary>
    public class BigBinaryValue : BinaryValueBase<BigEndianOrder>
    {
        #region [ Constructors ]

        /// <summary>Creates a new big-endian ordered binary value from the given byte array.</summary>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        /// <param name="startIndex">The offset in the buffer where the data starts.</param>
        /// <param name="length">The number of data bytes that make up the binary value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of the <paramref name="buffer"/> -or-
        /// <paramref name="length"/> is less than 0 -or-
        /// <paramref name="startIndex"/> and <paramref name="length"/> do not specify a valid region in the <paramref name="buffer"/>
        /// </exception>
        public BigBinaryValue(byte[] buffer, int startIndex, int length)
            : base(buffer, startIndex, length)
        {
        }

        /// <summary>Creates a new big-endian ordered binary value from the given byte array.</summary>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        public BigBinaryValue(byte[] buffer)
            : base(buffer, 0, buffer.Length)
        {
        }

        #endregion

        #region [ Operators ]

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
