//******************************************************************************************************
//  EndianOrderTest.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/13/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// Uncomment only for hard coded testing of BigEndian architecture code
//#define ForceBigEndianTesting

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests
{
    [TestClass]
    public class EndianOrderTest
    {
        #region [ Byte Swap/Copy Only Endian Order Testing Classes ]

        // These classes represent a brute-force approach to Endian order handling
        // that are used as a base-line for testing other optimized approaches

        /// <summary>
        /// Represents a big-endian byte order interoperability class.
        /// </summary>
        private class BigEndianOrderTester : EndianOrderTester
        {
            #region [ Constructors ]

            /// <summary>
            /// Constructs a new instance of the <see cref="BigEndianOrderTester"/> class.
            /// </summary>
            public BigEndianOrderTester()
                : base(Endianness.BigEndian)
            {
            }

            #endregion

            #region [ Static ]

            private static BigEndianOrderTester s_endianOrder;

            /// <summary>
            /// Returns the default instance of the <see cref="BigEndianOrderTester"/> class.
            /// </summary>
            public static BigEndianOrderTester Default
            {
                get
                {
                    if ((object)s_endianOrder == null)
                        s_endianOrder = new BigEndianOrderTester();

                    return s_endianOrder;
                }
            }

            #endregion
        }

        /// <summary>
        /// Represents a little-endian byte order interoperability class.
        /// </summary>
        private class LittleEndianOrderTester : EndianOrderTester
        {
            #region [ Constructors ]

            /// <summary>
            /// Constructs a new instance of the <see cref="LittleEndianOrderTester"/> class.
            /// </summary>
            public LittleEndianOrderTester()
                : base(Endianness.LittleEndian)
            {
            }

            #endregion

            #region [ Static ]

            private static LittleEndianOrderTester s_endianOrder;

            /// <summary>
            /// Returns the default instance of the <see cref="LittleEndianOrderTester"/> class.
            /// </summary>
            public static LittleEndianOrderTester Default
            {
                get
                {
                    if ((object)s_endianOrder == null)
                        s_endianOrder = new LittleEndianOrderTester();

                    return s_endianOrder;
                }
            }

            #endregion
        }

        /// <summary>
        /// Represents a native-endian byte order interoperability class.
        /// </summary>
        private class NativeEndianOrderTester : EndianOrderTester
        {
            #region [ Constructors ]

            /// <summary>
            /// Constructs a new instance of the <see cref="NativeEndianOrderTester"/> class.
            /// </summary>
            public NativeEndianOrderTester()
                : base(BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian)
            {
            }

            #endregion

            #region [ Static ]

            private static NativeEndianOrderTester s_endianOrder;

            /// <summary>
            /// Returns the default instance of the <see cref="NativeEndianOrderTester"/> class.
            /// </summary>
            public static NativeEndianOrderTester Default
            {
                get
                {
                    if ((object)s_endianOrder == null)
                        s_endianOrder = new NativeEndianOrderTester();

                    return s_endianOrder;
                }
            }

            #endregion
        }

        /// <summary>
        /// Represents an endian byte order interoperability class.
        /// </summary>
        /// <remarks>
        /// Intel systems use little-endian byte order, other systems, such as Unix, use big-endian byte ordering.
        /// Little-endian ordering means bits are ordered such that the bit whose in-memory representation is right-most is the most-significant-bit in a byte.
        /// Big-endian ordering means bits are ordered such that the bit whose in-memory representation is left-most is the most-significant-bit in a byte.
        /// </remarks>
        private class EndianOrderTester
        {
            #region [ Members ]

            // Delegates
            private delegate void CopyBufferFunction(byte[] sourceBuffer, int sourceIndex, byte[] destinationBuffer, int destinationIndex, int length);
            private delegate byte[] CoerceByteOrderFunction(byte[] buffer);

            // Fields
            private readonly Endianness m_targetEndianness;
            private readonly CopyBufferFunction m_copyBuffer;
            private readonly CoerceByteOrderFunction m_coerceByteOrder;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Constructs a new instance of the <see cref="EndianOrderTester"/> class.
            /// </summary>
            /// <param name="targetEndianness">Endianness parameter.</param>
            protected EndianOrderTester(Endianness targetEndianness)
            {
                m_targetEndianness = targetEndianness;

                // We perform this logic only once for speed in conversions - we can do this because neither
                // the target nor the OS endian order will change during the lifecycle of this class...
                if (targetEndianness == Endianness.BigEndian)
                {
#if ForceBigEndianTesting
                    if (!BitConverter.IsLittleEndian) // <- Hard coded test for alternate architecture
#else
                    if (BitConverter.IsLittleEndian)
#endif
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
#if ForceBigEndianTesting
                    if (!BitConverter.IsLittleEndian) // <- Hard coded test for alternate architecture
#else
                    if (BitConverter.IsLittleEndian)
#endif
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
            /// Returns the target endian-order of this <see cref="EndianOrderTester"/> representation.
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
            /// Returns a <see cref="Boolean"/> value converted from one byte at a specified position in a byte array.
            /// </summary>
            /// <param name="value">An array of bytes.</param>
            /// <param name="startIndex">The starting position within value.</param>
            /// <returns>true if the byte at startIndex in value is nonzero; otherwise, false.</returns>
            /// <exception cref="ArgumentNullException">value is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
            [SuppressMessage("Microsoft.Performance", "CA1822")]
            public bool ToBoolean(byte[] value, int startIndex)
            {
                return BitConverter.ToBoolean(value, startIndex);
            }

            /// <summary>
            /// Returns a Unicode character converted from two bytes, accounting for target endian-order, at a specified position in a byte array.
            /// </summary>
            /// <param name="value">An array of bytes (i.e., buffer containing binary image of value).</param>
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
            /// <param name="value">An array of bytes (i.e., buffer containing binary image of value).</param>
            /// <param name="startIndex">The starting position within value.</param>
            /// <returns>A double-precision floating point number formed by eight bytes beginning at startIndex.</returns>
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
            /// <param name="value">An array of bytes (i.e., buffer containing binary image of value).</param>
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
            /// Returns a 24-bit signed integer converted from three bytes, accounting for target endian-order, at a specified position in a byte array.
            /// </summary>
            /// <param name="value">An array of bytes (i.e., buffer containing binary image of value).</param>
            /// <param name="startIndex">The starting position within value.</param>
            /// <returns>A 24-bit signed integer formed by three bytes beginning at startIndex.</returns>
            /// <exception cref="ArgumentNullException">value is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
            public Int24 ToInt24(byte[] value, int startIndex)
            {
                byte[] buffer = new byte[3];

                m_copyBuffer(value, startIndex, buffer, 0, 3);

                return Int24.GetValue(buffer, 0);
            }

            /// <summary>
            /// Returns a 32-bit signed integer converted from four bytes, accounting for target endian-order, at a specified position in a byte array.
            /// </summary>
            /// <param name="value">An array of bytes (i.e., buffer containing binary image of value).</param>
            /// <param name="startIndex">The starting position within value.</param>
            /// <returns>A 32-bit signed integer formed by four bytes beginning at startIndex.</returns>
            /// <exception cref="ArgumentNullException">value is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
            public int ToInt32(byte[] value, int startIndex)
            {
                byte[] buffer = new byte[4];

                m_copyBuffer(value, startIndex, buffer, 0, 4);

                return BitConverter.ToInt32(buffer, 0);
            }

            /// <summary>
            /// Returns a 64-bit signed integer converted from eight bytes, accounting for target endian-order, at a specified position in a byte array.
            /// </summary>
            /// <param name="value">An array of bytes (i.e., buffer containing binary image of value).</param>
            /// <param name="startIndex">The starting position within value.</param>
            /// <returns>A 64-bit signed integer formed by eight bytes beginning at startIndex.</returns>
            /// <exception cref="ArgumentNullException">value is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
            public long ToInt64(byte[] value, int startIndex)
            {
                byte[] buffer = new byte[8];

                m_copyBuffer(value, startIndex, buffer, 0, 8);

                return BitConverter.ToInt64(buffer, 0);
            }

            /// <summary>
            /// Returns a single-precision floating point number converted from four bytes, accounting for target endian-order, at a specified position in a byte array.
            /// </summary>
            /// <param name="value">An array of bytes (i.e., buffer containing binary image of value).</param>
            /// <param name="startIndex">The starting position within value.</param>
            /// <returns>A single-precision floating point number formed by four bytes beginning at startIndex.</returns>
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
            /// <param name="value">An array of bytes (i.e., buffer containing binary image of value).</param>
            /// <param name="startIndex">The starting position within value.</param>
            /// <returns>A 16-bit unsigned integer formed by two bytes beginning at startIndex.</returns>
            /// <exception cref="ArgumentNullException">value is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
            public ushort ToUInt16(byte[] value, int startIndex)
            {
                byte[] buffer = new byte[2];

                m_copyBuffer(value, startIndex, buffer, 0, 2);

                return BitConverter.ToUInt16(buffer, 0);
            }

            /// <summary>
            /// Returns a 24-bit unsigned integer converted from three bytes, accounting for target endian-order, at a specified position in a byte array.
            /// </summary>
            /// <param name="value">An array of bytes (i.e., buffer containing binary image of value).</param>
            /// <param name="startIndex">The starting position within value.</param>
            /// <returns>A 24-bit unsigned integer formed by three bytes beginning at startIndex.</returns>
            /// <exception cref="ArgumentNullException">value is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
            public UInt24 ToUInt24(byte[] value, int startIndex)
            {
                byte[] buffer = new byte[3];

                m_copyBuffer(value, startIndex, buffer, 0, 3);

                return UInt24.GetValue(buffer, 0);
            }

            /// <summary>
            /// Returns a 32-bit unsigned integer converted from four bytes, accounting for target endian-order, at a specified position in a byte array.
            /// </summary>
            /// <param name="value">An array of bytes (i.e., buffer containing binary image of value).</param>
            /// <param name="startIndex">The starting position within value.</param>
            /// <returns>A 32-bit unsigned integer formed by four bytes beginning at startIndex.</returns>
            /// <exception cref="ArgumentNullException">value is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
            public uint ToUInt32(byte[] value, int startIndex)
            {
                byte[] buffer = new byte[4];

                m_copyBuffer(value, startIndex, buffer, 0, 4);

                return BitConverter.ToUInt32(buffer, 0);
            }

            /// <summary>
            /// Returns a 64-bit unsigned integer converted from eight bytes, accounting for target endian-order, at a specified position in a byte array.
            /// </summary>
            /// <param name="value">An array of bytes (i.e., buffer containing binary image of value).</param>
            /// <param name="startIndex">The starting position within value.</param>
            /// <returns>A 64-bit unsigned integer formed by eight bytes beginning at startIndex.</returns>
            /// <exception cref="ArgumentNullException">value is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
            public ulong ToUInt64(byte[] value, int startIndex)
            {
                byte[] buffer = new byte[8];

                m_copyBuffer(value, startIndex, buffer, 0, 8);

                return BitConverter.ToUInt64(buffer, 0);
            }

            /// <summary>
            /// Returns the specified value as an array of bytes in the target endian-order.
            /// </summary>
            /// <param name="value">The value to convert.</param>
            /// <returns>An array of bytes with length 1.</returns>
            /// <typeparam name="T">Native value type to get bytes for.</typeparam>
            /// <exception cref="ArgumentException"><paramref name="value"/> type is not primitive.</exception>
            /// <exception cref="InvalidOperationException">Cannot get bytes for <paramref name="value"/> type.</exception>
            public byte[] GetBytes<T>(T value) where T : struct, IConvertible
            {
                if (!typeof(T).IsPrimitive)
                    throw new ArgumentException("Value type is not primitive", "value");

                IConvertible nativeValue = (IConvertible)value;

                switch (nativeValue.GetTypeCode())
                {
                    case TypeCode.Char:
                        return GetBytes(nativeValue.ToChar(null));
                    case TypeCode.Boolean:
                        return GetBytes(nativeValue.ToBoolean(null));
                    case TypeCode.Int16:
                        return GetBytes(nativeValue.ToInt16(null));
                    case TypeCode.UInt16:
                        return GetBytes(nativeValue.ToUInt16(null));
                    case TypeCode.Int32:
                        return GetBytes(nativeValue.ToInt32(null));
                    case TypeCode.UInt32:
                        return GetBytes(nativeValue.ToUInt32(null));
                    case TypeCode.Int64:
                        return GetBytes(nativeValue.ToInt64(null));
                    case TypeCode.UInt64:
                        return GetBytes(nativeValue.ToUInt64(null));
                    case TypeCode.Single:
                        return GetBytes(nativeValue.ToSingle(null));
                    case TypeCode.Double:
                        return GetBytes(nativeValue.ToDouble(null));
                    default:
                        throw new InvalidOperationException("Cannot get bytes for value type " + nativeValue.GetTypeCode());
                }
            }

            /// <summary>
            /// Returns the specified <see cref="Boolean"/> value as an array of bytes in the target endian-order.
            /// </summary>
            /// <param name="value">The <see cref="Boolean"/> value to convert.</param>
            /// <returns>An array of bytes with length 1.</returns>
            [SuppressMessage("Microsoft.Performance", "CA1822")]
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
            public byte[] GetBytes(ushort value)
            {
                return m_coerceByteOrder(BitConverter.GetBytes(value));
            }

            /// <summary>
            /// Returns the specified 24-bit unsigned integer value as an array of bytes.
            /// </summary>
            /// <param name="value">The number to convert.</param>
            /// <returns>An array of bytes with length 3.</returns>
            public byte[] GetBytes(UInt24 value)
            {
                return m_coerceByteOrder(UInt24.GetBytes(value));
            }

            /// <summary>
            /// Returns the specified 32-bit unsigned integer value as an array of bytes.
            /// </summary>
            /// <param name="value">The number to convert.</param>
            /// <returns>An array of bytes with length 4.</returns>
            public byte[] GetBytes(uint value)
            {
                return m_coerceByteOrder(BitConverter.GetBytes(value));
            }

            /// <summary>
            /// Returns the specified 64-bit unsigned integer value as an array of bytes.
            /// </summary>
            /// <param name="value">The number to convert.</param>
            /// <returns>An array of bytes with length 8.</returns>
            public byte[] GetBytes(ulong value)
            {
                return m_coerceByteOrder(BitConverter.GetBytes(value));
            }

            /// <summary>
            /// Copies the specified primitive type value as an array of bytes in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The <see cref="Boolean"/> value to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <typeparam name="T">Native value type to get bytes for.</typeparam>
            /// <exception cref="ArgumentException"><paramref name="value"/> type is not primitive.</exception>
            /// <exception cref="InvalidOperationException">Cannot get bytes for <paramref name="value"/> type.</exception>
            /// <returns>Length of bytes copied into array based on size of <typeparamref name="T"/>.</returns>
            public int CopyBytes<T>(T value, byte[] destinationArray, int destinationIndex) where T : struct, IConvertible
            {
                if (!typeof(T).IsPrimitive)
                    throw new ArgumentException("Value type is not primitive", "value");

                IConvertible nativeValue = (IConvertible)value;

                switch (nativeValue.GetTypeCode())
                {
                    case TypeCode.Char:
                        return CopyBytes(nativeValue.ToChar(null), destinationArray, destinationIndex);
                    case TypeCode.Boolean:
                        return CopyBytes(nativeValue.ToBoolean(null), destinationArray, destinationIndex);
                    case TypeCode.Int16:
                        return CopyBytes(nativeValue.ToInt16(null), destinationArray, destinationIndex);
                    case TypeCode.UInt16:
                        return CopyBytes(nativeValue.ToUInt16(null), destinationArray, destinationIndex);
                    case TypeCode.Int32:
                        return CopyBytes(nativeValue.ToInt32(null), destinationArray, destinationIndex);
                    case TypeCode.UInt32:
                        return CopyBytes(nativeValue.ToUInt32(null), destinationArray, destinationIndex);
                    case TypeCode.Int64:
                        return CopyBytes(nativeValue.ToInt64(null), destinationArray, destinationIndex);
                    case TypeCode.UInt64:
                        return CopyBytes(nativeValue.ToUInt64(null), destinationArray, destinationIndex);
                    case TypeCode.Single:
                        return CopyBytes(nativeValue.ToSingle(null), destinationArray, destinationIndex);
                    case TypeCode.Double:
                        return CopyBytes(nativeValue.ToDouble(null), destinationArray, destinationIndex);
                    default:
                        throw new InvalidOperationException("Cannot copy bytes for value type " + nativeValue.GetTypeCode());
                }
            }

            /// <summary>
            /// Copies the specified <see cref="Boolean"/> value as an array of 1 byte in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The <see cref="Boolean"/> value to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <returns>Length of bytes copied into array based on size of <paramref name="value"/>.</returns>
            public int CopyBytes(bool value, byte[] destinationArray, int destinationIndex)
            {
                m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 1);
                return 1;
            }

            /// <summary>
            /// Copies the specified Unicode character value as an array of 2 bytes in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The Unicode character value to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <returns>Length of bytes copied into array based on size of <paramref name="value"/>.</returns>
            public int CopyBytes(char value, byte[] destinationArray, int destinationIndex)
            {
                m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2);
                return 2;
            }

            /// <summary>
            /// Copies the specified double-precision floating point value as an array of 8 bytes in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The number to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <returns>Length of bytes copied into array based on size of <paramref name="value"/>.</returns>
            public int CopyBytes(double value, byte[] destinationArray, int destinationIndex)
            {
                m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8);
                return 8;
            }

            /// <summary>
            /// Copies the specified 16-bit signed integer value as an array of 2 bytes in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The number to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <returns>Length of bytes copied into array based on size of <paramref name="value"/>.</returns>
            public int CopyBytes(short value, byte[] destinationArray, int destinationIndex)
            {
                m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2);
                return 2;
            }

            /// <summary>
            /// Copies the specified 24-bit signed integer value as an array of 3 bytes in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The number to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <returns>Length of bytes copied into array based on size of <paramref name="value"/>.</returns>
            public int CopyBytes(Int24 value, byte[] destinationArray, int destinationIndex)
            {
                m_copyBuffer(Int24.GetBytes(value), 0, destinationArray, destinationIndex, 3);
                return 3;
            }

            /// <summary>
            /// Copies the specified 32-bit signed integer value as an array of 4 bytes in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The number to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <returns>Length of bytes copied into array based on size of <paramref name="value"/>.</returns>
            public int CopyBytes(int value, byte[] destinationArray, int destinationIndex)
            {
                m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4);
                return 4;
            }

            /// <summary>
            /// Copies the specified 64-bit signed integer value as an array of 8 bytes in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The number to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <returns>Length of bytes copied into array based on size of <paramref name="value"/>.</returns>
            public int CopyBytes(long value, byte[] destinationArray, int destinationIndex)
            {
                m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8);
                return 8;
            }

            /// <summary>
            /// Copies the specified single-precision floating point value as an array of 4 bytes in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The number to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <returns>Length of bytes copied into array based on size of <paramref name="value"/>.</returns>
            public int CopyBytes(float value, byte[] destinationArray, int destinationIndex)
            {
                m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4);
                return 4;
            }

            /// <summary>
            /// Copies the specified 16-bit unsigned integer value as an array of 2 bytes in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The number to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <returns>Length of bytes copied into array based on size of <paramref name="value"/>.</returns>
            public int CopyBytes(ushort value, byte[] destinationArray, int destinationIndex)
            {
                m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 2);
                return 2;
            }

            /// <summary>
            /// Copies the specified 24-bit unsigned integer value as an array of 3 bytes in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The number to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <returns>Length of bytes copied into array based on size of <paramref name="value"/>.</returns>
            public int CopyBytes(UInt24 value, byte[] destinationArray, int destinationIndex)
            {
                m_copyBuffer(UInt24.GetBytes(value), 0, destinationArray, destinationIndex, 3);
                return 3;
            }

            /// <summary>
            /// Copies the specified 32-bit unsigned integer value as an array of 4 bytes in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The number to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <returns>Length of bytes copied into array based on size of <paramref name="value"/>.</returns>
            public int CopyBytes(uint value, byte[] destinationArray, int destinationIndex)
            {
                m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 4);
                return 4;
            }

            /// <summary>
            /// Copies the specified 64-bit unsigned integer value as an array of 8 bytes in the target endian-order to the destination array.
            /// </summary>
            /// <param name="value">The number to convert and copy.</param>
            /// <param name="destinationArray">The destination buffer.</param>
            /// <param name="destinationIndex">The byte offset into <paramref name="destinationArray"/>.</param>
            /// <returns>Length of bytes copied into array based on size of <paramref name="value"/>.</returns>
            public int CopyBytes(ulong value, byte[] destinationArray, int destinationIndex)
            {
                m_copyBuffer(BitConverter.GetBytes(value), 0, destinationArray, destinationIndex, 8);
                return 8;
            }

            #endregion
        }

        #endregion

        #region [ Static Endian Testing Classes ]

        public class StaticLittleEndianTester
        {
            public static StaticLittleEndianTester Default = new StaticLittleEndianTester();

            public Endianness TargetEndianness = Endianness.LittleEndian;

            public byte[] GetBytes<T>(T value) where T : struct, IConvertible
            {
                return LittleEndian.GetBytes(value);
            }

            public byte[] GetBytes(Int24 value)
            {
                return LittleEndian.GetBytes(value);
            }

            public byte[] GetBytes(UInt24 value)
            {
                return LittleEndian.GetBytes(value);
            }

            public int CopyBytes<T>(T value, byte[] destinationArray, int destinationIndex) where T : struct, IConvertible
            {
                return LittleEndian.CopyBytes(value, destinationArray, destinationIndex);
            }

            public int CopyBytes(Int24 value, byte[] destinationArray, int destinationIndex)
            {
                return LittleEndian.CopyBytes(value, destinationArray, destinationIndex);
            }

            public int CopyBytes(UInt24 value, byte[] destinationArray, int destinationIndex)
            {
                return LittleEndian.CopyBytes(value, destinationArray, destinationIndex);
            }

            public bool ToBoolean(byte[] value, int startIndex)
            {
                return LittleEndian.ToBoolean(value, startIndex);
            }

            public char ToChar(byte[] value, int startIndex)
            {
                return LittleEndian.ToChar(value, startIndex);
            }

            public double ToDouble(byte[] value, int startIndex)
            {
                return LittleEndian.ToDouble(value, startIndex);
            }

            public short ToInt16(byte[] value, int startIndex)
            {
                return LittleEndian.ToInt16(value, startIndex);
            }

            public Int24 ToInt24(byte[] value, int startIndex)
            {
                return LittleEndian.ToInt24(value, startIndex);
            }

            public int ToInt32(byte[] value, int startIndex)
            {
                return LittleEndian.ToInt32(value, startIndex);
            }

            public long ToInt64(byte[] value, int startIndex)
            {
                return LittleEndian.ToInt64(value, startIndex);
            }

            public float ToSingle(byte[] value, int startIndex)
            {
                return LittleEndian.ToSingle(value, startIndex);
            }

            public ushort ToUInt16(byte[] value, int startIndex)
            {
                return LittleEndian.ToUInt16(value, startIndex);
            }

            public UInt24 ToUInt24(byte[] value, int startIndex)
            {
                return LittleEndian.ToUInt24(value, startIndex);
            }

            public uint ToUInt32(byte[] value, int startIndex)
            {
                return LittleEndian.ToUInt32(value, startIndex);
            }

            public ulong ToUInt64(byte[] value, int startIndex)
            {
                return LittleEndian.ToUInt64(value, startIndex);
            }
        }

        public class StaticBigEndianTester
        {
            public static StaticBigEndianTester Default = new StaticBigEndianTester();

            public Endianness TargetEndianness = Endianness.BigEndian;

            public byte[] GetBytes<T>(T value) where T : struct, IConvertible
            {
                return BigEndian.GetBytes(value);
            }

            public byte[] GetBytes(Int24 value)
            {
                return BigEndian.GetBytes(value);
            }

            public byte[] GetBytes(UInt24 value)
            {
                return BigEndian.GetBytes(value);
            }

            public int CopyBytes<T>(T value, byte[] destinationArray, int destinationIndex) where T : struct, IConvertible
            {
                return BigEndian.CopyBytes(value, destinationArray, destinationIndex);
            }

            public int CopyBytes(Int24 value, byte[] destinationArray, int destinationIndex)
            {
                return BigEndian.CopyBytes(value, destinationArray, destinationIndex);
            }

            public int CopyBytes(UInt24 value, byte[] destinationArray, int destinationIndex)
            {
                return BigEndian.CopyBytes(value, destinationArray, destinationIndex);
            }

            public bool ToBoolean(byte[] value, int startIndex)
            {
                return BigEndian.ToBoolean(value, startIndex);
            }

            public char ToChar(byte[] value, int startIndex)
            {
                return BigEndian.ToChar(value, startIndex);
            }

            public double ToDouble(byte[] value, int startIndex)
            {
                return BigEndian.ToDouble(value, startIndex);
            }

            public short ToInt16(byte[] value, int startIndex)
            {
                return BigEndian.ToInt16(value, startIndex);
            }

            public Int24 ToInt24(byte[] value, int startIndex)
            {
                return BigEndian.ToInt24(value, startIndex);
            }

            public int ToInt32(byte[] value, int startIndex)
            {
                return BigEndian.ToInt32(value, startIndex);
            }

            public long ToInt64(byte[] value, int startIndex)
            {
                return BigEndian.ToInt64(value, startIndex);
            }

            public float ToSingle(byte[] value, int startIndex)
            {
                return BigEndian.ToSingle(value, startIndex);
            }

            public ushort ToUInt16(byte[] value, int startIndex)
            {
                return BigEndian.ToUInt16(value, startIndex);
            }

            public UInt24 ToUInt24(byte[] value, int startIndex)
            {
                return BigEndian.ToUInt24(value, startIndex);
            }

            public uint ToUInt32(byte[] value, int startIndex)
            {
                return BigEndian.ToUInt32(value, startIndex);
            }

            public ulong ToUInt64(byte[] value, int startIndex)
            {
                return BigEndian.ToUInt64(value, startIndex);
            }
        }

        #endregion

        [TestMethod]
        public void TestDataTypeEncodings()
        {
            bool[] boolValues = { true, false };
            char[] charValues = { char.MinValue, (char)1, (char)(char.MaxValue / 2), char.MaxValue };
            short[] shortValues = { short.MinValue, short.MinValue / (short)2, (short)-1, (short)0, (short)1, short.MaxValue / (short)2, short.MaxValue };
            ushort[] ushortValues = { ushort.MinValue, (ushort)1, ushort.MaxValue / (ushort)2, ushort.MaxValue };
            Int24[] int24Values = { Int24.MinValue, Int24.MinValue / (Int24)2, (Int24)(-1), (Int24)0, (Int24)1, Int24.MaxValue / (Int24)2, Int24.MaxValue };
            UInt24[] uint24Values = { UInt24.MinValue, (UInt24)1, UInt24.MaxValue / (UInt24)2, UInt24.MaxValue };
            int[] intValues = { int.MinValue, int.MinValue / 2, -1, 0, 1, int.MaxValue / 2, int.MaxValue };
            uint[] uintValues = { uint.MinValue, 1U, uint.MaxValue / 2U, uint.MaxValue };
            long[] longValues = { long.MinValue, long.MinValue / 2L, -1L, 0L, 1L, long.MaxValue / 2L, long.MaxValue };
            ulong[] ulongValues = { ulong.MinValue, 1UL, ulong.MaxValue / 2UL, ulong.MaxValue };
            float[] floatValues = { float.MinValue, float.MinValue / 2.0F, -1.0F, 0.0F, 1.0F, float.MaxValue / 2.0F, float.MaxValue, float.NaN, float.NegativeInfinity, float.PositiveInfinity };
            double[] doubleValues = { double.MinValue, double.MinValue / 2.0D, -1.0D, 0.0D, 1.0D, double.MaxValue / 2.0D, double.MaxValue, double.NaN, double.NegativeInfinity, double.PositiveInfinity };

#if ForceBigEndianTesting
            // Swap all values to big-endian order - note that since endian order tester class is now operating in reverse, we use little-endian for this
            byte[] valueBytes;
            IConvertible convertibleValue;
            EndianOrderTester swapper = LittleEndianOrderTester.Default;

            for (int i = 0; i < boolValues.Length; i++)
            {
                valueBytes = BitConverter.GetBytes(boolValues[i]);
                boolValues[i] = swapper.ToBoolean(valueBytes, 0);
            }

            for (int i = 0; i < charValues.Length; i++)
            {
                valueBytes = BitConverter.GetBytes(charValues[i]);
                charValues[i] = swapper.ToChar(valueBytes, 0);
            }

            for (int i = 0; i < shortValues.Length; i++)
            {
                valueBytes = BitConverter.GetBytes(shortValues[i]);
                shortValues[i] = swapper.ToInt16(valueBytes, 0);
            }

            for (int i = 0; i < ushortValues.Length; i++)
            {
                valueBytes = BitConverter.GetBytes(ushortValues[i]);
                ushortValues[i] = swapper.ToUInt16(valueBytes, 0);
            }

            for (int i = 0; i < int24Values.Length; i++)
            {
                valueBytes = Int24.GetBytes(int24Values[i]);
                int24Values[i] = swapper.ToInt24(valueBytes, 0);
            }

            for (int i = 0; i < uint24Values.Length; i++)
            {
                valueBytes = UInt24.GetBytes(uint24Values[i]);
                uint24Values[i] = swapper.ToUInt24(valueBytes, 0);
            }

            for (int i = 0; i < intValues.Length; i++)
            {
                valueBytes = BitConverter.GetBytes(intValues[i]);
                intValues[i] = swapper.ToInt32(valueBytes, 0);
            }

            for (int i = 0; i < uintValues.Length; i++)
            {
                valueBytes = BitConverter.GetBytes(uintValues[i]);
                uintValues[i] = swapper.ToUInt32(valueBytes, 0);
            }

            for (int i = 0; i < longValues.Length; i++)
            {
                valueBytes = BitConverter.GetBytes(longValues[i]);
                longValues[i] = swapper.ToInt64(valueBytes, 0);
            }

            for (int i = 0; i < ulongValues.Length; i++)
            {
                valueBytes = BitConverter.GetBytes(ulongValues[i]);
                ulongValues[i] = swapper.ToUInt64(valueBytes, 0);
            }

            for (int i = 0; i < floatValues.Length; i++)
            {
                valueBytes = BitConverter.GetBytes(floatValues[i]);
                floatValues[i] = swapper.ToSingle(valueBytes, 0);
            }

            for (int i = 0; i < doubleValues.Length; i++)
            {
                valueBytes = BitConverter.GetBytes(doubleValues[i]);
                doubleValues[i] = swapper.ToDouble(valueBytes, 0);
            }
#else
            TestValues(boolValues, NativeEndianOrder.Default, NativeEndianOrderTester.Default, sizeof(bool));
            TestValues(charValues, NativeEndianOrder.Default, NativeEndianOrderTester.Default, sizeof(char));
            TestValues(shortValues, NativeEndianOrder.Default, NativeEndianOrderTester.Default, sizeof(short));
            TestValues(ushortValues, NativeEndianOrder.Default, NativeEndianOrderTester.Default, sizeof(ushort));
            TestValues(int24Values, NativeEndianOrder.Default, NativeEndianOrderTester.Default, 3);
            TestValues(uint24Values, NativeEndianOrder.Default, NativeEndianOrderTester.Default, 3);
            TestValues(intValues, NativeEndianOrder.Default, NativeEndianOrderTester.Default, sizeof(int));
            TestValues(uintValues, NativeEndianOrder.Default, NativeEndianOrderTester.Default, sizeof(uint));
            TestValues(longValues, NativeEndianOrder.Default, NativeEndianOrderTester.Default, sizeof(long));
            TestValues(ulongValues, NativeEndianOrder.Default, NativeEndianOrderTester.Default, sizeof(ulong));
            TestValues(floatValues, NativeEndianOrder.Default, NativeEndianOrderTester.Default, sizeof(float));
            TestValues(doubleValues, NativeEndianOrder.Default, NativeEndianOrderTester.Default, sizeof(double));
#endif
            TestValues(boolValues, LittleEndianOrder.Default, LittleEndianOrderTester.Default, sizeof(bool));
            TestValues(charValues, LittleEndianOrder.Default, LittleEndianOrderTester.Default, sizeof(char));
            TestValues(shortValues, LittleEndianOrder.Default, LittleEndianOrderTester.Default, sizeof(short));
            TestValues(ushortValues, LittleEndianOrder.Default, LittleEndianOrderTester.Default, sizeof(ushort));
            TestValues(int24Values, LittleEndianOrder.Default, LittleEndianOrderTester.Default, 3);
            TestValues(uint24Values, LittleEndianOrder.Default, LittleEndianOrderTester.Default, 3);
            TestValues(intValues, LittleEndianOrder.Default, LittleEndianOrderTester.Default, sizeof(int));
            TestValues(uintValues, LittleEndianOrder.Default, LittleEndianOrderTester.Default, sizeof(uint));
            TestValues(longValues, LittleEndianOrder.Default, LittleEndianOrderTester.Default, sizeof(long));
            TestValues(ulongValues, LittleEndianOrder.Default, LittleEndianOrderTester.Default, sizeof(ulong));
            TestValues(floatValues, LittleEndianOrder.Default, LittleEndianOrderTester.Default, sizeof(float));
            TestValues(doubleValues, LittleEndianOrder.Default, LittleEndianOrderTester.Default, sizeof(double));

            TestValues(boolValues, BigEndianOrder.Default, BigEndianOrderTester.Default, sizeof(bool));
            TestValues(charValues, BigEndianOrder.Default, BigEndianOrderTester.Default, sizeof(char));
            TestValues(shortValues, BigEndianOrder.Default, BigEndianOrderTester.Default, sizeof(short));
            TestValues(ushortValues, BigEndianOrder.Default, BigEndianOrderTester.Default, sizeof(ushort));
            TestValues(int24Values, BigEndianOrder.Default, BigEndianOrderTester.Default, 3);
            TestValues(uint24Values, BigEndianOrder.Default, BigEndianOrderTester.Default, 3);
            TestValues(intValues, BigEndianOrder.Default, BigEndianOrderTester.Default, sizeof(int));
            TestValues(uintValues, BigEndianOrder.Default, BigEndianOrderTester.Default, sizeof(uint));
            TestValues(longValues, BigEndianOrder.Default, BigEndianOrderTester.Default, sizeof(long));
            TestValues(ulongValues, BigEndianOrder.Default, BigEndianOrderTester.Default, sizeof(ulong));
            TestValues(floatValues, BigEndianOrder.Default, BigEndianOrderTester.Default, sizeof(float));
            TestValues(doubleValues, BigEndianOrder.Default, BigEndianOrderTester.Default, sizeof(double));

            TestValues(boolValues, StaticLittleEndianTester.Default, LittleEndianOrderTester.Default, sizeof(bool));
            TestValues(charValues, StaticLittleEndianTester.Default, LittleEndianOrderTester.Default, sizeof(char));
            TestValues(shortValues, StaticLittleEndianTester.Default, LittleEndianOrderTester.Default, sizeof(short));
            TestValues(ushortValues, StaticLittleEndianTester.Default, LittleEndianOrderTester.Default, sizeof(ushort));
            TestValues(int24Values, StaticLittleEndianTester.Default, LittleEndianOrderTester.Default, 3);
            TestValues(uint24Values, StaticLittleEndianTester.Default, LittleEndianOrderTester.Default, 3);
            TestValues(intValues, StaticLittleEndianTester.Default, LittleEndianOrderTester.Default, sizeof(int));
            TestValues(uintValues, StaticLittleEndianTester.Default, LittleEndianOrderTester.Default, sizeof(uint));
            TestValues(longValues, StaticLittleEndianTester.Default, LittleEndianOrderTester.Default, sizeof(long));
            TestValues(ulongValues, StaticLittleEndianTester.Default, LittleEndianOrderTester.Default, sizeof(ulong));
            TestValues(floatValues, StaticLittleEndianTester.Default, LittleEndianOrderTester.Default, sizeof(float));
            TestValues(doubleValues, StaticLittleEndianTester.Default, LittleEndianOrderTester.Default, sizeof(double));

            TestValues(boolValues, StaticBigEndianTester.Default, BigEndianOrderTester.Default, sizeof(bool));
            TestValues(charValues, StaticBigEndianTester.Default, BigEndianOrderTester.Default, sizeof(char));
            TestValues(shortValues, StaticBigEndianTester.Default, BigEndianOrderTester.Default, sizeof(short));
            TestValues(ushortValues, StaticBigEndianTester.Default, BigEndianOrderTester.Default, sizeof(ushort));
            TestValues(int24Values, StaticBigEndianTester.Default, BigEndianOrderTester.Default, 3);
            TestValues(uint24Values, StaticBigEndianTester.Default, BigEndianOrderTester.Default, 3);
            TestValues(intValues, StaticBigEndianTester.Default, BigEndianOrderTester.Default, sizeof(int));
            TestValues(uintValues, StaticBigEndianTester.Default, BigEndianOrderTester.Default, sizeof(uint));
            TestValues(longValues, StaticBigEndianTester.Default, BigEndianOrderTester.Default, sizeof(long));
            TestValues(ulongValues, StaticBigEndianTester.Default, BigEndianOrderTester.Default, sizeof(ulong));
            TestValues(floatValues, StaticBigEndianTester.Default, BigEndianOrderTester.Default, sizeof(float));
            TestValues(doubleValues, StaticBigEndianTester.Default, BigEndianOrderTester.Default, sizeof(double));
        }

        private void TestValues<T>(IEnumerable<T> values, dynamic eOrder, EndianOrderTester eOrderTester, int typeSize) where T : struct, IConvertible
        {
            byte[] valueBytes, valueTestBytes, copiedBytes = new byte[typeSize], copiedTestBytes = new byte[typeSize];
            IConvertible convertibleValue;
            int length, testLength;

            Assert.IsTrue(eOrder.TargetEndianness == eOrderTester.TargetEndianness);

            foreach (T value in values)
            {
                convertibleValue = value as IConvertible;
                Assert.IsNotNull(convertibleValue);

                // Test "GetBytes" implementation
                if (typeof(T) == typeof(Int24))
                {
                    Int24 int24Value = (Int24)convertibleValue.ToInt32(null);
                    valueBytes = eOrder.GetBytes(int24Value);
                    valueTestBytes = eOrderTester.GetBytes(int24Value);
                    Assert.IsTrue(valueBytes.CompareTo(valueTestBytes) == 0);
                }
                else if (typeof(T) == typeof(UInt24))
                {
                    UInt24 uint24Value = (UInt24)convertibleValue.ToUInt32(null);
                    valueBytes = eOrder.GetBytes(uint24Value);
                    valueTestBytes = eOrderTester.GetBytes(uint24Value);
                    Assert.IsTrue(valueBytes.CompareTo(valueTestBytes) == 0);
                }
                else
                {
                    valueBytes = eOrder.GetBytes(value);
                    valueTestBytes = eOrderTester.GetBytes(value);
                    Assert.IsTrue(valueBytes.CompareTo(valueTestBytes) == 0);
                }

                // Test "CopyBytes" implementation
                if (typeof(T) == typeof(Int24))
                {
                    Int24 int24Value = (Int24)convertibleValue.ToInt32(null);
                    length = eOrder.CopyBytes(int24Value, copiedBytes, 0);
                    testLength = eOrderTester.CopyBytes(int24Value, copiedTestBytes, 0);
                    Assert.IsTrue(copiedBytes.CompareTo(copiedTestBytes) == 0);
                    Assert.IsTrue(length == testLength && length == typeSize);
                }
                else if (typeof(T) == typeof(UInt24))
                {
                    UInt24 uint24Value = (UInt24)convertibleValue.ToUInt32(null);
                    length = eOrder.CopyBytes(uint24Value, copiedBytes, 0);
                    testLength = eOrderTester.CopyBytes(uint24Value, copiedTestBytes, 0);
                    Assert.IsTrue(copiedBytes.CompareTo(copiedTestBytes) == 0);
                    Assert.IsTrue(length == testLength && length == typeSize);
                }
                else
                {
                    length = eOrder.CopyBytes(value, copiedBytes, 0);
                    testLength = eOrderTester.CopyBytes(value, copiedTestBytes, 0);
                    Assert.IsTrue(copiedBytes.CompareTo(copiedTestBytes) == 0);
                    Assert.IsTrue(length == testLength && length == typeSize);
                }

                // Validate that CopiedBytes for value matches GetBytes
                Assert.IsTrue(copiedBytes.CompareTo(valueBytes) == 0);

                // Test "To<type>" implementation
                switch (typeSize)
                {
                    case 1:
                        Assert.IsTrue(convertibleValue.ToBoolean(null) == eOrder.ToBoolean(copiedBytes, 0));
                        Assert.IsTrue(eOrder.ToBoolean(copiedBytes, 0) == eOrderTester.ToBoolean(copiedBytes, 0));
                        break;
                    case 2:
                        if (typeof(T) == typeof(char))
                        {
                            Assert.IsTrue(convertibleValue.ToChar(null) == eOrder.ToChar(copiedBytes, 0));
                            Assert.IsTrue(eOrder.ToChar(copiedBytes, 0) == eOrderTester.ToChar(copiedBytes, 0));
                        }
                        else if (typeof(T) == typeof(short))
                        {
                            Assert.IsTrue(convertibleValue.ToInt16(null) == eOrder.ToInt16(copiedBytes, 0));
                            Assert.IsTrue(eOrder.ToInt16(copiedBytes, 0) == eOrderTester.ToInt16(copiedBytes, 0));
                        }
                        else
                        {
                            Assert.IsTrue(convertibleValue.ToUInt16(null) == eOrder.ToUInt16(copiedBytes, 0));
                            Assert.IsTrue(eOrder.ToUInt16(copiedBytes, 0) == eOrderTester.ToUInt16(copiedBytes, 0));
                        }
                        break;
                    case 3:
                        if (typeof(T) == typeof(Int24))
                        {
                            Assert.IsTrue(convertibleValue.ToInt32(null) == eOrder.ToInt24(copiedBytes, 0));
                            Assert.IsTrue(eOrder.ToInt24(copiedBytes, 0) == eOrderTester.ToInt24(copiedBytes, 0));
                        }
                        else
                        {
                            Assert.IsTrue(convertibleValue.ToUInt32(null) == eOrder.ToUInt24(copiedBytes, 0));
                            Assert.IsTrue(eOrder.ToUInt24(copiedBytes, 0) == eOrderTester.ToUInt24(copiedBytes, 0));
                        }
                        break;
                    case 4:
                        if (typeof(T) == typeof(int))
                        {
                            Assert.IsTrue(convertibleValue.ToInt32(null) == eOrder.ToInt32(copiedBytes, 0));
                            Assert.IsTrue(eOrder.ToInt32(copiedBytes, 0) == eOrderTester.ToInt32(copiedBytes, 0));
                        }
                        else if (typeof(T) == typeof(uint))
                        {
                            Assert.IsTrue(convertibleValue.ToUInt32(null) == eOrder.ToUInt32(copiedBytes, 0));
                            Assert.IsTrue(eOrder.ToUInt32(copiedBytes, 0) == eOrderTester.ToUInt32(copiedBytes, 0));
                        }
                        else
                        {
                            float singleValue = convertibleValue.ToSingle(null);

                            if (float.IsNaN(singleValue))
                            {
                                Assert.IsTrue(float.IsNaN(eOrder.ToSingle(copiedBytes, 0)));
                                Assert.IsTrue(float.IsNaN(eOrderTester.ToSingle(copiedBytes, 0)));
                            }
                            else if (float.IsNegativeInfinity(singleValue))
                            {
                                Assert.IsTrue(float.IsNegativeInfinity(eOrder.ToSingle(copiedBytes, 0)));
                                Assert.IsTrue(float.IsNegativeInfinity(eOrderTester.ToSingle(copiedBytes, 0)));
                            }
                            else if (float.IsPositiveInfinity(singleValue))
                            {
                                Assert.IsTrue(float.IsPositiveInfinity(eOrder.ToSingle(copiedBytes, 0)));
                                Assert.IsTrue(float.IsPositiveInfinity(eOrderTester.ToSingle(copiedBytes, 0)));
                            }
                            else
                            {
                                Assert.IsTrue(singleValue == eOrder.ToSingle(copiedBytes, 0));
                                Assert.IsTrue(eOrder.ToSingle(copiedBytes, 0) == eOrderTester.ToSingle(copiedBytes, 0));
                            }
                        }
                        break;
                    case 8:
                        if (typeof(T) == typeof(long))
                        {
                            Assert.IsTrue(convertibleValue.ToInt64(null) == eOrder.ToInt64(copiedBytes, 0));
                            Assert.IsTrue(eOrder.ToInt64(copiedBytes, 0) == eOrderTester.ToInt64(copiedBytes, 0));
                        }
                        else if (typeof(T) == typeof(ulong))
                        {
                            Assert.IsTrue(convertibleValue.ToUInt64(null) == eOrder.ToUInt64(copiedBytes, 0));
                            Assert.IsTrue(eOrder.ToUInt64(copiedBytes, 0) == eOrderTester.ToUInt64(copiedBytes, 0));
                        }
                        else
                        {
                            double doubleValue = convertibleValue.ToDouble(null);

                            if (double.IsNaN(doubleValue))
                            {
                                Assert.IsTrue(double.IsNaN(eOrder.ToDouble(copiedBytes, 0)));
                                Assert.IsTrue(double.IsNaN(eOrderTester.ToDouble(copiedBytes, 0)));
                            }
                            else if (double.IsNegativeInfinity(doubleValue))
                            {
                                Assert.IsTrue(double.IsNegativeInfinity(eOrder.ToDouble(copiedBytes, 0)));
                                Assert.IsTrue(double.IsNegativeInfinity(eOrderTester.ToDouble(copiedBytes, 0)));
                            }
                            else if (double.IsPositiveInfinity(doubleValue))
                            {
                                Assert.IsTrue(double.IsPositiveInfinity(eOrder.ToDouble(copiedBytes, 0)));
                                Assert.IsTrue(double.IsPositiveInfinity(eOrderTester.ToDouble(copiedBytes, 0)));
                            }
                            else
                            {
                                Assert.IsTrue(doubleValue == eOrder.ToDouble(copiedBytes, 0));
                                Assert.IsTrue(eOrder.ToDouble(copiedBytes, 0) == eOrderTester.ToDouble(copiedBytes, 0));
                            }
                        }
                        break;
                }
            }
        }
    }
}
