//******************************************************************************************************
//  RadixCodec.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/11/2017 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF
{
    /// <summary>
    /// Represents a radix value codec for conversion of base-10 integer values to and from other base values.
    /// </summary>
    /// <remarks>
    /// The primary use case of this class is to provide compact string encodings of integer values.
    /// The encodings produced by this class do not manage arbitrary sized bytes arrays, nor do they
    /// include padding - as a result, encodings are not intended to comply with RFC 3548.
    /// </remarks>
    public class RadixCodec
    {
        #region [ Members ]

        // Nested Types
        private class RadixIntegerCodec<T> where T : IComparable<T>, IEquatable<T>
        {
            #region [ Members ]

            private readonly RadixCodec m_parent;
            private readonly bool m_caseSensitive;
            private readonly int m_bitSize;
            private readonly T m_minValue;
            private readonly T m_zeroValue;
            private readonly Func<T, T> m_abs;
            private readonly Func<T, int, int> m_mod;
            private readonly Func<T, int, T> m_divide;
            private readonly Func<long, bool, T> m_convert;
            private readonly string m_minRadix;

            #endregion

            #region [ Constructors ]

            public RadixIntegerCodec(RadixCodec parent, bool caseSensitive, int bitSize, T minValue, T zeroValue, Func<T, T> abs, Func<T, int, int> mod, Func<T, int, T> divide, Func<long, bool, T> convert, string minRadix = null)
            {
                m_parent = parent;
                m_caseSensitive = caseSensitive;
                m_bitSize = bitSize;
                m_minValue = minValue;
                m_zeroValue = zeroValue;
                m_abs = abs;
                m_mod = mod;
                m_divide = divide;
                m_convert = convert;
                m_minRadix = minRadix ?? Encode(minValue);
            }

            #endregion

            #region [ Methods ]

            public T Decode(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value));

                bool isNegative = false;
                long result = 0;
                long multiplier = 1;
                int radix = m_parent.Digits.Length;

                value = value.Trim();

                if (!m_caseSensitive)
                    value = value.ToUpperInvariant();

                for (int i = value.Length - 1; i >= 0; i--)
                {
                    if (i == 0 && value[i] == '-')
                    {
                        isNegative = true;
                        break;
                    }

                    int digit = m_parent.Digits.IndexOf(value[i]);

                    if (digit == -1)
                        throw new ArgumentException($"Invalid characters in radix-{m_parent.Digits.Length} value: \"{value}\".", nameof(value));

                    result += digit * multiplier;
                    multiplier *= radix;
                }

                return m_convert(result, isNegative);
            }

            public string Encode(T value)
            {
                // Check for minimum T value, this avoids a Math.Abs exception:
                // "Negating the minimum value of a twos complement number is invalid."
                if (value.Equals(m_minValue) && m_minValue.CompareTo(m_zeroValue) < 0)
                    return m_minRadix;

                bool isNegative = value.CompareTo(m_zeroValue) < 0;
                char[] buffer = new char[m_bitSize];
                int index = m_bitSize - 1;
                int radix = m_parent.Digits.Length;

                value = m_abs(value);

                do
                    buffer[index--] = m_parent.Digits[m_mod(value, radix)];
                while ((value = m_divide(value, radix)).CompareTo(m_zeroValue) != 0);

                string result = new string(buffer, index + 1, m_bitSize - index - 1);

                if (isNegative)
                    result = $"-{result}";

                return result;
            }

            #endregion
        }

        // Fields

        /// <summary>
        /// Defines the available digits for a radix value codec.
        /// </summary>
        public readonly string Digits;

        private readonly RadixIntegerCodec<short> m_int16;
        private readonly RadixIntegerCodec<ushort> m_uint16;
        private readonly RadixIntegerCodec<Int24> m_int24;
        private readonly RadixIntegerCodec<UInt24> m_uint24;
        private readonly RadixIntegerCodec<int> m_int32;
        private readonly RadixIntegerCodec<uint> m_uint32;
        private readonly RadixIntegerCodec<long> m_int64;
        private readonly RadixIntegerCodec<ulong> m_uint64;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="RadixCodec"/>.
        /// </summary>
        /// <param name="digits">Digits to use for radix values.</param>
        /// <param name="caseSensitive">Determines if alphabetic radix characters are case sensitive.</param>
        public RadixCodec(string digits, bool caseSensitive)
        {
            if (string.IsNullOrWhiteSpace(digits))
                throw new ArgumentNullException(nameof(digits));

            if (digits.Length < 2)
                throw new ArgumentOutOfRangeException(nameof(digits), "Minimum of 2 characters required for radix value digits.");

            Digits = digits;

            // ReSharper disable once PossibleLossOfFraction
            RadixIntegerCodec<decimal> int96 = new RadixIntegerCodec<decimal>(this, caseSensitive, 64, decimal.MinValue, 0, Math.Abs, (i, d) => (int)(i % d), (i, d) => (ulong)i / (ulong)d, (v, n) => n ? -v : v);
            m_int64 = new RadixIntegerCodec<long>(this, caseSensitive, 64, long.MinValue, 0L, Math.Abs, (i, d) => (int)(i % d), (i, d) => i / d, (v, n) => n ? -v : v, int96.Encode(long.MinValue));
            m_uint64 = new RadixIntegerCodec<ulong>(this, caseSensitive, 64, ulong.MinValue, 0UL, i => i, (i, d) => (int)(i % (ulong)d), (i, d) => i / (ulong)d, (v, n) => (ulong)(n ? -v : v));
            m_int32 = new RadixIntegerCodec<int>(this, caseSensitive, 32, int.MinValue, 0, Math.Abs, (i, d) => i % d, (i, d) => i / d, (v, n) => (int)(n ? -v : v), m_int64.Encode(int.MinValue));
            m_uint32 = new RadixIntegerCodec<uint>(this, caseSensitive, 32, uint.MinValue, 0U, i => i, (i, d) => (int)(i % (uint)d), (i, d) => i / (uint)d, (v, n) => (uint)(n ? -v : v));
            m_int24 = new RadixIntegerCodec<Int24>(this, caseSensitive, 24, Int24.MinValue, 0, i => (Int24)Math.Abs(i), (i, d) => i % d, (i, d) => (Int24)(i / d), (v, n) => (Int24)(n ? -v : v), m_int32.Encode(Int24.MinValue));
            m_uint24 = new RadixIntegerCodec<UInt24>(this, caseSensitive, 24, UInt24.MinValue, 0, i => i, (i, d) => (int)(i % (uint)d), (i, d) => (UInt24)(i / (uint)d), (v, n) => (UInt24)(uint)(n ? -v : v));
            m_int16 = new RadixIntegerCodec<short>(this, caseSensitive, 16, short.MinValue, 0, Math.Abs, (i, d) => i % d, (i, d) => (short)(i / d), (v, n) => (short)(n ? -v : v), m_int32.Encode(short.MinValue));
            m_uint16 = new RadixIntegerCodec<ushort>(this, caseSensitive, 16, ushort.MinValue, 0, i => i, (i, d) => i % (ushort)d, (i, d) => (ushort)(i / d), (v, n) => (ushort)(n ? -v : v));
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Radix value string.</returns>
        public string Encode(short value) => m_int16.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Radix value string.</returns>
        public string Encode(ushort value) => m_uint16.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Radix value string.</returns>
        public string Encode(Int24 value) => m_int24.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Radix value string.</returns>
        public string Encode(UInt24 value) => m_uint24.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Radix value string.</returns>
        public string Encode(int value) => m_int32.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Radix value string.</returns>
        public string Encode(uint value) => m_uint32.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Radix value string.</returns>
        public string Encode(long value) => m_int64.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Radix value string.</returns>
        public string Encode(ulong value) => m_uint64.Encode(value);

        /// <summary>
        /// Attempts to convert a radix value to an integer value.
        /// </summary>
        /// <param name="radixValue">Radix value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string radixValue, out short value)
        {
            try
            {
                value = m_int16.Decode(radixValue);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        /// Attempts to convert a radix value to an integer value.
        /// </summary>
        /// <param name="radixValue">Radix value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string radixValue, out ushort value)
        {
            try
            {
                value = m_uint16.Decode(radixValue);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        /// Attempts to convert a radix value to an integer value.
        /// </summary>
        /// <param name="radixValue">Radix value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string radixValue, out Int24 value)
        {
            try
            {
                value = m_int24.Decode(radixValue);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        /// Attempts to convert a radix value to an integer value.
        /// </summary>
        /// <param name="radixValue">Radix value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string radixValue, out UInt24 value)
        {
            try
            {
                value = m_uint24.Decode(radixValue);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        /// Attempts to convert a radix value to an integer value.
        /// </summary>
        /// <param name="radixValue">Radix value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string radixValue, out int value)
        {
            try
            {
                value = m_int32.Decode(radixValue);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        /// Attempts to convert a radix value to an integer value.
        /// </summary>
        /// <param name="radixValue">Radix value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string radixValue, out uint value)
        {
            try
            {
                value = m_uint32.Decode(radixValue);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        /// Attempts to convert a radix value to an integer value.
        /// </summary>
        /// <param name="radixValue">Radix value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string radixValue, out long value)
        {
            try
            {
                value = m_int64.Decode(radixValue);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        /// Attempts to convert a radix value to an integer value.
        /// </summary>
        /// <param name="radixValue">Radix value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string radixValue, out ulong value)
        {
            try
            {
                value = m_uint64.Decode(radixValue);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        /// Converts a radix value to an integer value.
        /// </summary>
        /// <typeparam name="T">Integer type to convert</typeparam>
        /// <param name="radixValue">Radix value to convert.</param>
        /// <returns>Decoded integer value.</returns>
        /// <exception cref="ArgumentNullException">Radix value is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Invalid radix value character.</exception>
        /// <exception cref="OverflowException">Decoded radix value overflowed integer type.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Only integer types Int16, UInt16, Int24, UInt24, Int32, UInt32, Int64 and UInt64 are supported.</exception>
        public T Decode<T>(string radixValue) => (T)Decode(typeof(T), radixValue);

        /// <summary>
        /// Converts a radix value to an integer value.
        /// </summary>
        /// <param name="type">Integer type to convert.</param>
        /// <param name="radixValue">Radix value to convert.</param>
        /// <returns>Decoded integer value.</returns>
        /// <exception cref="ArgumentNullException">Radix value is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Invalid radix value character.</exception>
        /// <exception cref="OverflowException">Decoded radix value overflowed integer type.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Only integer types Int16, UInt16, Int24, UInt24, Int32, UInt32, Int64 and UInt64 are supported.</exception>
        public object Decode(Type type, string radixValue)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16:
                    return m_int16.Decode(radixValue);
                case TypeCode.UInt16:
                    return m_uint16.Decode(radixValue);
                case TypeCode.Int32:
                    if (type == typeof(Int24))
                        return m_int24.Decode(radixValue);
                    return m_int32.Decode(radixValue);
                case TypeCode.UInt32:
                    if (type == typeof(UInt24))
                        return m_uint24.Decode(radixValue);
                    return m_uint32.Decode(radixValue);
                case TypeCode.Int64:
                    return m_int64.Decode(radixValue);
                case TypeCode.UInt64:
                    return m_uint64.Decode(radixValue);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Only integer types Int16, UInt16, Int24, UInt24, Int32, UInt32, Int64 and UInt64 are supported.");
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static RadixCodec s_radix16;
        private static RadixCodec s_radix32;
        private static RadixCodec s_radix36;
        private static RadixCodec s_radix64;
        private static RadixCodec s_radix86;

        // Static Properties

        /// <summary>
        /// Gets a radix-16 value (hex) encoding.
        /// </summary>
        /// <remarks>
        /// int.MaxValue encodes to "7FFFFFFF", 8 characters
        /// int.MinValue encodes to "-80000000", 9 characters
        /// uint.MaxValue encodes to "FFFFFFFF", 8 characters
        /// long.MaxValue encodes to "7FFFFFFFFFFFFFFF", 16 characters
        /// long.MinValue encodes to "-8000000000000000", 17 characters
        /// ulong.MaxValue encodes to "FFFFFFFFFFFFFFFF", 16 characters
        /// </remarks>                                                               1234567890123456
        public static RadixCodec Radix16 = s_radix16 ?? (s_radix16 = new RadixCodec("0123456789ABCDEF", false));

        /// <summary>
        /// Gets a radix-32 value encoding.
        /// </summary>
        /// <remarks>
        /// int.MaxValue encodes to "1VVVVVV", 7 characters
        /// int.MinValue encodes to "-2000000", 8 characters
        /// uint.MaxValue encodes to "3VVVVVV", 7 characters
        /// long.MaxValue encodes to "7VVVVVVVVVVVV", 13 characters
        /// long.MinValue encodes to "-8000000000000", 14 characters
        /// ulong.MaxValue encodes to "FVVVVVVVVVVVV", 13 characters
        /// </remarks>                                                               12345678901234567890123456789012
        public static RadixCodec Radix32 = s_radix32 ?? (s_radix32 = new RadixCodec("0123456789ABCDEFGHIJKLMNOPQRSTUV", false));

        /// <summary>
        /// Gets a radix-36 value encoding.
        /// </summary>
        /// <remarks>
        /// int.MaxValue encodes to "ZIK0ZJ", 6 characters -- ideal int size with reduced digit set
        /// int.MinValue encodes to "-ZIK0ZK", 7 characters
        /// uint.MaxValue encodes to "1Z141Z3", 7 characters
        /// long.MaxValue encodes to "1Y2P0IJ32E8E7", 13 characters
        /// long.MinValue encodes to "-1Y2P0IJ32E8E8", 14 characters
        /// ulong.MaxValue encodes to "3W5E11264SGSF", 13 characters
        /// </remarks>                                                               123456789012345678901234567890123456
        public static RadixCodec Radix36 = s_radix36 ?? (s_radix36 = new RadixCodec("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", false));

        /// <summary>
        /// Gets a radix-64 value encoding.
        /// </summary>
        /// <remarks>
        /// int.MaxValue encodes to "1/////", 6 characters
        /// int.MinValue encodes to "-200000", 7 characters
        /// uint.MaxValue encodes to "3/////", 6 characters
        /// long.MaxValue encodes to "7//////////", 11 characters
        /// long.MinValue encodes to "-80000000000", 12 characters
        /// ulong.MaxValue encodes to "F//////////", 11 characters
        /// </remarks>                                                               1234567890123456789012345678901234567890123456789012345678901234
        public static RadixCodec Radix64 = s_radix64 ?? (s_radix64 = new RadixCodec("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+/", true));

        /// <summary>
        /// Gets a radix-86 value encoding.
        /// </summary>
        /// <remarks>
        /// int.MaxValue encodes to "$S2Jx", 5 characters -- base 86 reduces int size to 5
        /// int.MinValue encodes to "-$S2Jy", 6 characters
        /// uint.MaxValue encodes to "1qu4dh", 6 characters
        /// long.MaxValue encodes to "1X2qL^UmlIt", 11 characters
        /// long.MinValue encodes to "-1X2qL^UmlIu", 12 characters
        /// ulong.MaxValue encodes to "2&5Sh]zLIbZ", 11 characters
        /// </remarks>                                                               1234567890123456789012345678901234567890123456789012345678901234567890123456
        public static RadixCodec Radix86 = s_radix86 ?? (s_radix86 = new RadixCodec("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&*+;=?@[]^", true));

        #endregion
    }
}
