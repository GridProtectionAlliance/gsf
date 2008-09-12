//*******************************************************************************************************
//  TVA.Interop.EndianOrder.vb - Endian byte order interoperability class
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//  01/14/2005 - J. Ritchie Carroll
//       Added GetByte overloads, and To<Type> functions - changes reviewed by John Shugart
//  01/05/2006 - J. Ritchie Carroll
//       2.0 version of source code migrated from 1.1 source (TVA.Interop.EndianOrder)
//  10/18/2006 - J. Ritchie Carroll
//       Added a few minor optimizations to buffer copy functions
//  09/09/2008 - J. Ritchie Carroll
//      Converted to C#
//
//*******************************************************************************************************

using System;

namespace TVA.Interop
{
    /// <summary>Endian Byte Order Enumeration</summary>
    public enum Endianness
    {
        BigEndian,
        LittleEndian
    }

    /// <summary>Endian byte order interoperability class</summary>
    /// <remarks>
    /// Intel systems use little-endian byte order, other systems, such as Unix, use big-endian byte ordering.
    /// Little-endian ordering means bits are ordered such that the bit whose in-memory representation is right-most is the most-significant-bit in a byte.
    /// Big-endian ordering means bits are ordered such that the bit whose in-memory representation is left-most is the most-significant-bit in a byte.
    /// </remarks>
    public sealed class EndianOrder
    {
        // Create shared big-endian class
        public static EndianOrder BigEndian = new EndianOrder(Endianness.BigEndian);

        // Create shared little-endian class
        public static EndianOrder LittleEndian = new EndianOrder(Endianness.LittleEndian);

        private delegate void CopyBufferFunction(byte[] sourceBuffer, int sourceIndex, byte[] destinationBuffer, int destinationIndex, int length);
        private delegate byte[] CoerceByteOrderFunction(byte[] buffer);

        private Endianness m_targetEndianness;
        private CopyBufferFunction m_copyBuffer;
        private CoerceByteOrderFunction m_coerceByteOrder;

        private EndianOrder(Endianness targetEndianness)
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

        public Endianness TargetEndianness
        {
            get
            {
                return m_targetEndianness;
            }
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
        public void CopyBytes(uint value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4);
        }

        [CLSCompliant(false)]
        public void CopyBytes(ulong value, byte[] destinationArray, int destinationIndex)
        {
            m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8);
        }
    }
}
