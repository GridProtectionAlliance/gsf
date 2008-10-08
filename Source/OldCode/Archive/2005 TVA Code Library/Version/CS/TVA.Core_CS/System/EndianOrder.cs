/**************************************************************************\
   Copyright (c) 2008, James Ritchie Carroll
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
        BigEndian,
        LittleEndian
    }

    #endregion

    /// <summary>Big-endian byte order interoperability class</summary>
    public class BigEndianOrder : EndianOrder
    {
        #region [ Constructors ]

        public BigEndianOrder()
            : base(Endianness.BigEndian)
        {
        }

        #endregion

        #region [ Static ]

        private static BigEndianOrder m_endianOrder;

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

        public LittleEndianOrder()
            : base(Endianness.LittleEndian)
        {
        }

        #endregion

        #region [ Static ]

        private static LittleEndianOrder m_endianOrder;

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

        public NativeEndianOrder()
            : base(BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian)
        {
        }

        #endregion

        #region [ Static ]

        private static NativeEndianOrder m_endianOrder;

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

        // For non-standard length byte manipulations, we expose copy function that will copy OS-ordered source buffer into proper target endian-order
        public void CopyBuffer(byte[] sourceBuffer, int sourceIndex, byte[] destinationBuffer, int destinationIndex, int length)
        {
            m_copyBuffer(sourceBuffer, sourceIndex, destinationBuffer, destinationIndex, length);
        }

        public byte[] CoerceByteOrder(byte[] buffer)
        {
            return m_coerceByteOrder(buffer);
        }

        public bool ToBoolean(byte[] value, int startIndex)
        {
            return BitConverter.ToBoolean(value, startIndex);
        }

        public char ToChar(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[2];

            m_copyBuffer(value, startIndex, buffer, 0, 2);

            return BitConverter.ToChar(buffer, 0);
        }

        public double ToDouble(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[8];

            m_copyBuffer(value, startIndex, buffer, 0, 8);

            return BitConverter.ToDouble(buffer, 0);
        }

        public short ToInt16(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[2];

            m_copyBuffer(value, startIndex, buffer, 0, 2);

            return BitConverter.ToInt16(buffer, 0);
        }

        public Int24 ToInt24(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[3];

            m_copyBuffer(value, startIndex, buffer, 0, 3);

            return Int24.GetValue(buffer, 0);
        }

        public int ToInt32(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[4];

            m_copyBuffer(value, startIndex, buffer, 0, 4);

            return BitConverter.ToInt32(buffer, 0);
        }

        public long ToInt64(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[8];

            m_copyBuffer(value, startIndex, buffer, 0, 8);

            return BitConverter.ToInt64(buffer, 0);
        }

        public float ToSingle(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[4];

            m_copyBuffer(value, startIndex, buffer, 0, 4);

            return BitConverter.ToSingle(buffer, 0);
        }

        [CLSCompliant(false)]
        public ushort ToUInt16(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[2];

            m_copyBuffer(value, startIndex, buffer, 0, 2);

            return BitConverter.ToUInt16(buffer, 0);
        }

        [CLSCompliant(false)]
        public UInt24 ToUInt24(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[3];

            m_copyBuffer(value, startIndex, buffer, 0, 3);

            return UInt24.GetValue(buffer, 0);
        }

        [CLSCompliant(false)]
        public uint ToUInt32(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[4];

            m_copyBuffer(value, startIndex, buffer, 0, 4);

            return BitConverter.ToUInt32(buffer, 0);
        }

        [CLSCompliant(false)]
        public ulong ToUInt64(byte[] value, int startIndex)
        {
            byte[] buffer = new byte[8];

            m_copyBuffer(value, startIndex, buffer, 0, 8);

            return BitConverter.ToUInt64(buffer, 0);
        }

        public byte[] GetBytes(bool value)
        {
            // No need to reverse buffer for one byte:
            return BitConverter.GetBytes(value);
        }

        public byte[] GetBytes(char value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        public byte[] GetBytes(double value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        public byte[] GetBytes(short value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        public byte[] GetBytes(Int24 value)
        {
            return m_coerceByteOrder(Int24.GetBytes(value));
        }

        public byte[] GetBytes(int value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        public byte[] GetBytes(long value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        public byte[] GetBytes(float value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        [CLSCompliant(false)]
        public byte[] GetBytes(ushort value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        [CLSCompliant(false)]
        public byte[] GetBytes(UInt24 value)
        {
            return m_coerceByteOrder(UInt24.GetBytes(value));
        }

        [CLSCompliant(false)]
        public byte[] GetBytes(uint value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        [CLSCompliant(false)]
        public byte[] GetBytes(ulong value)
        {
            return m_coerceByteOrder(BitConverter.GetBytes(value));
        }

        public void CopyBytes(bool value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 1);
        }

        public void CopyBytes(char value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2);
        }

        public void CopyBytes(double value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8);
        }

        public void CopyBytes(short value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2);
        }

        public void CopyBytes(Int24 value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(Int24.GetBytes(value), 0, destinationArray, destinationIndex, 3);
        }

        public void CopyBytes(int value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4);
        }

        public void CopyBytes(long value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8);
        }

        public void CopyBytes(float value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4);
        }

        [CLSCompliant(false)]
        public void CopyBytes(ushort value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2);
        }

        [CLSCompliant(false)]
        public void CopyBytes(UInt24 value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(UInt24.GetBytes(value), 0, destinationArray, destinationIndex, 3);
        }

        [CLSCompliant(false)]
        public void CopyBytes(uint value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4);
        }

        [CLSCompliant(false)]
        public void CopyBytes(ulong value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8);
        }

        #endregion

        #region [ Static ]

        /// <summary>Big-Endian byte order conversion class.</summary>
        public static EndianOrder BigEndian;

        /// <summary>Little-Endian byte order conversion class.</summary>
        public static EndianOrder LittleEndian;

        static EndianOrder()
        {
            BigEndian = new EndianOrder(Endianness.BigEndian);
            LittleEndian = new EndianOrder(Endianness.LittleEndian);
        }

        #endregion
    }
}
