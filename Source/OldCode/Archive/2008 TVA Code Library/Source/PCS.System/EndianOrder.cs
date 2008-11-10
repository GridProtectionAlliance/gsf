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
    #region [ Enumerations ]

    /// <summary>Endian Byte Order Enumeration</summary>
    public enum Endianness
    {
        /// <summary>Big-endian byte order.</summary>
        BigEndian,
        /// <summary>Little-endian byte order.</summary>
        LittleEndian
    }

    #endregion

    /// <summary>Big-endian byte order interoperability class</summary>
    public class BigEndianOrder : EndianOrder
    {
        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="BigEndianOrder"/> class.
        /// </summary>
        public BigEndianOrder()
            : base(Endianness.BigEndian)
        {
        }

        #endregion

        #region [ Static ]

        private static BigEndianOrder m_endianOrder;

        /// <summary>
        /// Returns the default instance of the <see cref="BigEndianOrder"/> class.
        /// </summary>
        public static BigEndianOrder Default
        {
            get
            {
                if (m_endianOrder == null) m_endianOrder = new BigEndianOrder();
                return m_endianOrder;
            }
        }

        #endregion
    }

    /// <summary>Little-endian byte order interoperability class</summary>
    public class LittleEndianOrder : EndianOrder
    {
        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="LittleEndianOrder"/> class.
        /// </summary>
        public LittleEndianOrder()
            : base(Endianness.LittleEndian)
        {
        }

        #endregion

        #region [ Static ]

        private static LittleEndianOrder m_endianOrder;

        /// <summary>
        /// Returns the default instance of the <see cref="LittleEndianOrder"/> class.
        /// </summary>
        public static LittleEndianOrder Default
        {
            get
            {
                if (m_endianOrder == null) m_endianOrder = new LittleEndianOrder();
                return m_endianOrder;
            }
        }

        #endregion
    }

    /// <summary>Native-endian byte order interoperability class</summary>
    public class NativeEndianOrder : EndianOrder
    {
        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="NativeEndianOrder"/> class.
        /// </summary>
        public NativeEndianOrder()
            : base(BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian)
        {
        }

        #endregion

        #region [ Static ]

        private static NativeEndianOrder m_endianOrder;

        /// <summary>
        /// Returns the default instance of the <see cref="NativeEndianOrder"/> class.
        /// </summary>
        public static NativeEndianOrder Default
        {
            get
            {
                if (m_endianOrder == null) m_endianOrder = new NativeEndianOrder();
                return m_endianOrder;
            }
        }

        #endregion
    }

    /// <summary>Endian byte order interoperability class</summary>
    /// <remarks>
    /// Intel systems use little-endian byte order, other systems, such as Unix, use big-endian byte ordering.
    /// Little-endian ordering means bits are ordered such that the bit whose in-memory representation is right-most is the most-significant-bit in a byte.
    /// Big-endian ordering means bits are ordered such that the bit whose in-memory representation is left-most is the most-significant-bit in a byte.
    /// </remarks>
    public class EndianOrder
    {
        #region [ Members ]

        // Delegates
        private delegate void CopyBufferFunction(byte[] sourceBuffer, int sourceIndex, byte[] destinationBuffer, int destinationIndex, int length);
        private delegate byte[] CoerceByteOrderFunction(byte[] buffer);

        // Fields
        private Endianness m_targetEndianness;
        private CopyBufferFunction m_copyBuffer;
        private CoerceByteOrderFunction m_coerceByteOrder;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="EndianOrder"/> class.
        /// </summary>
        protected EndianOrder(Endianness targetEndianness)
        {
            m_targetEndianness = targetEndianness;

            // We perform this logic only once for speed in conversions - we can do this because neither
            // the target nor the OS endian order will change during the lifecycle of this class...
            if (targetEndianness == Endianness.BigEndian)
            {
                if (BitConverter.IsLittleEndian)
                {
                    // If OS is little endian and we want big endian, we swap the bytes
                    m_copyBuffer = SwapCopy;
                    m_coerceByteOrder = ReverseBuffer;
                }
                else
                {
                    // If OS is big endian and we want big endian, we just copy the bytes
                    m_copyBuffer = BlockCopy;
                    m_coerceByteOrder = PassThroughBuffer;
                }
            }
            else
            {
                if (BitConverter.IsLittleEndian)
                {
                    // If OS is little endian and we want little endian, we just copy the bytes
                    m_copyBuffer = BlockCopy;
                    m_coerceByteOrder = PassThroughBuffer;
                }
                else
                {
                    // If OS is big endian and we want little endian, we swap the bytes
                    m_copyBuffer = SwapCopy;
                    m_coerceByteOrder = ReverseBuffer;
                }
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns the target endian-order of this <see cref="EndianOrder"/> representation.
        /// </summary>
        public Endianness TargetEndianness
        {
            get
            {
                return m_targetEndianness;
            }
        }

        #endregion

        #region [ Methods ]

        private void BlockCopy(byte[] sourceBuffer, int sourceIndex, byte[] destinationBuffer, int destinationIndex, int length)
        {
            Buffer.BlockCopy(sourceBuffer, sourceIndex, destinationBuffer, destinationIndex, length);
        }

        // This function behaves just like Array.Copy but takes a little-endian source array and copies it in big-endian order,
        // or if the source array is big-endian it will copy it in little-endian order
        private void SwapCopy(byte[] sourceBuffer, int sourceIndex, byte[] destinationBuffer, int destinationIndex, int length)
        {
            int offset = destinationIndex + length - 1;

            for (int x = sourceIndex; x <= sourceIndex + length - 1; x++)
            {
                destinationBuffer[offset - (x - sourceIndex)] = sourceBuffer[x];
            }
        }

        private byte[] PassThroughBuffer(byte[] buffer)
        {
            return buffer;
        }

        private byte[] ReverseBuffer(byte[] buffer)
        {
            Array.Reverse(buffer);
            return buffer;
        }

        /// <summary>
        /// Copies a buffer in the target endian-order of this <see cref="EndianOrder"/> representation.
        /// </summary>
        /// <param name="sourceBuffer">The source buffer.</param>
        /// <param name="sourceIndex">The byte offset into <paramref name="sourceBuffer"/>.</param>
        /// <param name="destinationBuffer">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationBuffer"/>.</param>
        /// <param name="length">The number of bytes to copy.</param>
        public void CopyBuffer(byte[] sourceBuffer, int sourceIndex, byte[] destinationBuffer, int destinationIndex, int length)
        {
            // For non-standard length byte manipulations, we expose copy function that will copy OS-ordered source buffer into proper target endian-order
            m_copyBuffer(sourceBuffer, sourceIndex, destinationBuffer, destinationIndex, length);
        }

        /// <summary>
        /// Changes the order of a buffer (reverse or pass-through) based on the target endian-order of this <see cref="EndianOrder"/> representation.
        /// </summary>
        public byte[] CoerceByteOrder(byte[] buffer)
        {
            return m_coerceByteOrder(buffer);
        }

        /// <summary>
        /// Returns a <see cref="Boolean"/> value converted from one byte at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>true if the byte at startIndex in value is nonzero; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        public bool ToBoolean(byte[] value, int startIndex)
        {
            return BitConverter.ToBoolean(value, startIndex);
        }

        /// <summary>
        /// Returns a Unicode character converted from two bytes, accounting for target endian-order, at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A character formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        public char ToChar(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[2];

            m_copyBuffer(value, startIndex, buffer, 0, 2);

            return BitConverter.ToChar(buffer, 0);
        }

        /// <summary>
        /// Returns a double-precision floating point number converted from eight bytes, accounting for target endian-order, at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A double-precision floating point number formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        public double ToDouble(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[8];

            m_copyBuffer(value, startIndex, buffer, 0, 8);

            return BitConverter.ToDouble(buffer, 0);
        }

        /// <summary>
        /// Returns a 16-bit signed integer converted from two bytes, accounting for target endian-order, at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 16-bit signed integer formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        public short ToInt16(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[2];

            m_copyBuffer(value, startIndex, buffer, 0, 2);

            return BitConverter.ToInt16(buffer, 0);
        }

        /// <summary>
        /// Returns a 24-bit signed integer converted from two bytes, accounting for target endian-order, at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 24-bit signed integer formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        public Int24 ToInt24(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[3];

            m_copyBuffer(value, startIndex, buffer, 0, 3);

            return Int24.GetValue(buffer, 0);
        }

        /// <summary>
        /// Returns a 32-bit signed integer converted from two bytes, accounting for target endian-order, at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 32-bit signed integer formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        public int ToInt32(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[4];

            m_copyBuffer(value, startIndex, buffer, 0, 4);

            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        /// Returns a 64-bit signed integer converted from two bytes, accounting for target endian-order, at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 64-bit signed integer formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        public long ToInt64(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[8];

            m_copyBuffer(value, startIndex, buffer, 0, 8);

            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>
        /// Returns a single-precision floating point number converted from eight bytes, accounting for target endian-order, at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A single-precision floating point number formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        public float ToSingle(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[4];

            m_copyBuffer(value, startIndex, buffer, 0, 4);

            return BitConverter.ToSingle(buffer, 0);
        }

        /// <summary>
        /// Returns a 16-bit unsigned integer converted from two bytes, accounting for target endian-order, at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 16-bit unsigned integer formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        [CLSCompliant(false)]
        public ushort ToUInt16(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[2];

            m_copyBuffer(value, startIndex, buffer, 0, 2);

            return BitConverter.ToUInt16(buffer, 0);
        }

        /// <summary>
        /// Returns a 24-bit unsigned integer converted from two bytes, accounting for target endian-order, at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 24-bit unsigned integer formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        [CLSCompliant(false)]
        public UInt24 ToUInt24(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[3];

            m_copyBuffer(value, startIndex, buffer, 0, 3);

            return UInt24.GetValue(buffer, 0);
        }

        /// <summary>
        /// Returns a 32-bit unsigned integer converted from two bytes, accounting for target endian-order, at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 32-bit unsigned integer formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        [CLSCompliant(false)]
        public uint ToUInt32(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[4];

            m_copyBuffer(value, startIndex, buffer, 0, 4);

            return BitConverter.ToUInt32(buffer, 0);
        }

        /// <summary>
        /// Returns a 64-bit unsigned integer converted from two bytes, accounting for target endian-order, at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 64-bit unsigned integer formed by two bytes beginning at startIndex.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        [CLSCompliant(false)]
        public ulong ToUInt64(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[8];

            m_copyBuffer(value, startIndex, buffer, 0, 8);

            return BitConverter.ToUInt64(buffer, 0);
        }

        /// <summary>
        /// Returns the specified <see cref="Boolean"/> value as an array of bytes in the target endian-order.
        /// </summary>
        /// <param name="value">The <see cref="Boolean"/> value to convert.</param>
        /// <returns>An array of bytes with length 1.</returns>
        public byte[] GetBytes(bool value)
        {
            // No need to reverse buffer for one byte:
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Returns the specified Unicode character value as an array of bytes in the target endian-order.
        /// </summary>
        /// <param name="value">The Unicode character value to convert.</param>
        /// <returns>An array of bytes with length 2.</returns>
        public byte[] GetBytes(char value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Returns the specified double-precision floating point value as an array of bytes in the target endian-order.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        public byte[] GetBytes(double value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Returns the specified 16-bit signed integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 2.</returns>
        public byte[] GetBytes(short value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Returns the specified 24-bit signed integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 3.</returns>
        public byte[] GetBytes(Int24 value)
        {
            return m_coerceByteOrder(Int24.GetBytes(value));
        }

        /// <summary>
        /// Returns the specified 32-bit signed integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        public byte[] GetBytes(int value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Returns the specified 64-bit signed integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        public byte[] GetBytes(long value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Returns the specified single-precision floating point value as an array of bytes in the target endian-order.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        public byte[] GetBytes(float value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Returns the specified 16-bit unsigned integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 2.</returns>
        [CLSCompliant(false)]
        public byte[] GetBytes(ushort value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Returns the specified 24-bit unsigned integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 3.</returns>
        [CLSCompliant(false)]
        public byte[] GetBytes(UInt24 value)
        {
            return m_coerceByteOrder(UInt24.GetBytes(value));
        }

        /// <summary>
        /// Returns the specified 32-bit unsigned integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        [CLSCompliant(false)]
        public byte[] GetBytes(uint value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Returns the specified 64-bit unsigned integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        [CLSCompliant(false)]
        public byte[] GetBytes(ulong value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Copies the specified <see cref="Boolean"/> value as an array of 1 byte in the target endian-order to the destination array.
        /// </summary>
        /// <param name="value">The <see cref="Boolean"/> value to convert and copy.</param>
        /// <param name="destinationArray">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
        public void CopyBytes(bool value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 1);
        }

        /// <summary>
        /// Copies the specified Unicode character value as an array of 2 bytes in the target endian-order to the destination array.
        /// </summary>
        /// <param name="value">The Unicode character value to convert and copy.</param>
        /// <param name="destinationArray">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
        public void CopyBytes(char value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2);
        }

        /// <summary>
        /// Copies the specified double-precision floating point value as an array of 8 bytes in the target endian-order to the destination array.
        /// </summary>
        /// <param name="value">The number to convert and copy.</param>
        /// <param name="destinationArray">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
        public void CopyBytes(double value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8);
        }

        /// <summary>
        /// Copies the specified 16-bit signed integer value as an array of 2 bytes in the target endian-order to the destination array.
        /// </summary>
        /// <param name="value">The number to convert and copy.</param>
        /// <param name="destinationArray">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
        public void CopyBytes(short value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2);
        }

        /// <summary>
        /// Copies the specified 24-bit signed integer value as an array of 3 bytes in the target endian-order to the destination array.
        /// </summary>
        /// <param name="value">The number to convert and copy.</param>
        /// <param name="destinationArray">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
        public void CopyBytes(Int24 value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(Int24.GetBytes(value), 0, destinationArray, destinationIndex, 3);
        }

        /// <summary>
        /// Copies the specified 32-bit signed integer value as an array of 4 bytes in the target endian-order to the destination array.
        /// </summary>
        /// <param name="value">The number to convert and copy.</param>
        /// <param name="destinationArray">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
        public void CopyBytes(int value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4);
        }

        /// <summary>
        /// Copies the specified 64-bit signed integer value as an array of 8 bytes in the target endian-order to the destination array.
        /// </summary>
        /// <param name="value">The number to convert and copy.</param>
        /// <param name="destinationArray">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
        public void CopyBytes(long value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8);
        }

        /// <summary>
        /// Copies the specified single-precision floating point value as an array of 4 bytes in the target endian-order to the destination array.
        /// </summary>
        /// <param name="value">The number to convert and copy.</param>
        /// <param name="destinationArray">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
        public void CopyBytes(float value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4);
        }

        /// <summary>
        /// Copies the specified 16-bit unsigned integer value as an array of 2 bytes in the target endian-order to the destination array.
        /// </summary>
        /// <param name="value">The number to convert and copy.</param>
        /// <param name="destinationArray">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
        [CLSCompliant(false)]
        public void CopyBytes(ushort value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2);
        }

        /// <summary>
        /// Copies the specified 24-bit unsigned integer value as an array of 3 bytes in the target endian-order to the destination array.
        /// </summary>
        /// <param name="value">The number to convert and copy.</param>
        /// <param name="destinationArray">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
        [CLSCompliant(false)]
        public void CopyBytes(UInt24 value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(UInt24.GetBytes(value), 0, destinationArray, destinationIndex, 3);
        }

        /// <summary>
        /// Copies the specified 32-bit unsigned integer value as an array of 4 bytes in the target endian-order to the destination array.
        /// </summary>
        /// <param name="value">The number to convert and copy.</param>
        /// <param name="destinationArray">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
        [CLSCompliant(false)]
        public void CopyBytes(uint value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4);
        }

        /// <summary>
        /// Copies the specified 64-bit unsigned integer value as an array of 8 bytes in the target endian-order to the destination array.
        /// </summary>
        /// <param name="value">The number to convert and copy.</param>
        /// <param name="destinationArray">The destination buffer.</param>
        /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
        [CLSCompliant(false)]
        public void CopyBytes(ulong value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8);
        }

        #endregion

        #region [ Static ]

        /// <summary>Default instance of the Big-Endian byte order conversion class.</summary>
        public static EndianOrder BigEndian;

        /// <summary>Default instance of the Little-Endian byte order conversion class.</summary>
        public static EndianOrder LittleEndian;

        static EndianOrder()
        {
            BigEndian = new EndianOrder(Endianness.BigEndian);
            LittleEndian = new EndianOrder(Endianness.LittleEndian);
        }

        #endregion
    }
}
