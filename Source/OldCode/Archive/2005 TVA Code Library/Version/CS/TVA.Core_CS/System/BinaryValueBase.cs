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
    /// Represents a binary data sample stored as a byte array, 
    /// but implicitly castable to most common native types.
    /// </summary>
    public class BinaryValueBase<TEndianOrder> where TEndianOrder : EndianOrder
    {
        #region [ Members ]

        // Fields
        protected byte[] m_buffer;

        #endregion

        #region [ Constructors ]

        /// <summary>Creates a new binary value from the given byte array.</summary>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        /// <param name="startIndex">The offset in the buffer where the data starts.</param>
        /// <param name="length">The number of data bytes that make up the binary value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of the <paramref name="buffer"/> -or-
        /// <paramref name="length"/> is less than 0 -or-
        /// <paramref name="startIndex"/> and <paramref name="length"/> do not specify a valid region in the <paramref name="buffer"/>
        /// </exception>
        protected BinaryValueBase(byte[] buffer, int startIndex, int length)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "cannot be negative");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "cannot be negative");

            if (startIndex >= buffer.Length)
                throw new ArgumentOutOfRangeException("startIndex", "not a valid index in buffer");

            if (startIndex + length > buffer.Length)
                throw new ArgumentOutOfRangeException("length", "exceeds buffer size");

            m_buffer = new byte[length];

            // Extract specified region of source buffer as desired representation of binary value
            System.Buffer.BlockCopy(buffer, startIndex, m_buffer, 0, length);
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
            ValidateBufferLength(TypeCode.Byte, sizeof(Byte));
            return m_buffer[0];
        }

        public Int16 ToInt16()
        {
            ValidateBufferLength(TypeCode.Int16, sizeof(Int16));
            return m_endianOrder.ToInt16(m_buffer, 0);
        }

        [CLSCompliant(false)]
        public UInt16 ToUInt16()
        {
            ValidateBufferLength(TypeCode.UInt16, sizeof(UInt16));
            return m_endianOrder.ToUInt16(m_buffer, 0);
        }

        public Int24 ToInt24()
        {
            // There is no system type code for Int24
            if (m_buffer.Length < 3)
                throw new InvalidOperationException("Binary value buffer is too small to represent a Int24 - buffer length needs to be at least 3");

            return m_endianOrder.ToInt24(m_buffer, 0);
        }

        [CLSCompliant(false)]
        public UInt24 ToUInt24()
        {
            // There is no system type code for UInt24
            if (m_buffer.Length < 3)
                throw new InvalidOperationException("Binary value buffer is too small to represent a UInt24 - buffer length needs to be at least 3");

            return m_endianOrder.ToUInt24(m_buffer, 0);
        }

        public Int32 ToInt32()
        {
            ValidateBufferLength(TypeCode.Int32, sizeof(Int32));
            return m_endianOrder.ToInt32(m_buffer, 0);
        }

        [CLSCompliant(false)]
        public UInt32 ToUInt32()
        {
            ValidateBufferLength(TypeCode.UInt32, sizeof(UInt32));
            return m_endianOrder.ToUInt32(m_buffer, 0);
        }

        public Int64 ToInt64()
        {
            ValidateBufferLength(TypeCode.Int64, sizeof(Int64));
            return m_endianOrder.ToInt64(m_buffer, 0);
        }

        [CLSCompliant(false)]
        public UInt64 ToUInt64()
        {
            ValidateBufferLength(TypeCode.UInt64, sizeof(UInt64));
            return m_endianOrder.ToUInt64(m_buffer, 0);
        }

        public Single ToSingle()
        {
            ValidateBufferLength(TypeCode.Single, sizeof(Single));
            return m_endianOrder.ToSingle(m_buffer, 0);
        }

        public Double ToDouble()
        {
            ValidateBufferLength(TypeCode.Double, sizeof(Double));
            return m_endianOrder.ToDouble(m_buffer, 0);
        }

        private void ValidateBufferLength(TypeCode typeCode, int size)
        {
            if (m_buffer.Length < size)
                throw new InvalidOperationException(string.Format("Binary value buffer is too small to represent a {0} - buffer length needs to be at least {1}", typeCode, size));
        }

        #endregion

        #region [ Static ]

        // Because each "typed" instance will be it's own class - each will have its own static variable instance
        protected static TEndianOrder m_endianOrder;

        #endregion
    }
}
