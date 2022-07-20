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
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace GSF
{
    /// <summary>
    /// Represents a radix value codec for conversion of base-10 integer values to and from other base values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The primary use case of this class is to provide compact string-based encodings of integer values, e.g.,
    /// storing an unsigned 32-bit integer value in a string-based database field that only holds 6 characters;
    /// the maximum <see cref="UInt32"/> value of 4294967295 requires 10 characters of storage as a string.
    /// </para>
    /// <para>
    /// </para>
    /// The codec algorithm for <see cref="RadixCodec"/> works much like base-64 encoding but with variable base
    /// sizes and an integer source data focus, not a byte-array. The encoded base value strings are not intended
    /// to provide binary compression, many of the radix value encodings of integers produced by this class will
    /// have a byte-size that is greater than native bytes that make up integer. Since the encodings produced by
    /// this class do not manage arbitrary sized bytes arrays nor do they include padding, the encodings are not
    /// intended to comply with RFC 3548.
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
                int radix = m_parent.Radix;

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
                        throw new ArgumentException($"Invalid characters in radix-{m_parent.Radix} value: \"{value}\".", nameof(value));

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
                int radix = m_parent.Radix;

                value = m_abs(value);

                do
                    buffer[index--] = m_parent.Digits[m_mod(value, radix)];
                while ((value = m_divide(value, radix)).CompareTo(m_zeroValue) != 0);

                string result = new(buffer, index + 1, m_bitSize - index - 1);

                if (isNegative)
                    result = $"-{result}";

                return result;
            }

            #endregion
        }

        // Constants
        private const string InvalidType = "Only integer types Int16, UInt16, Int24, UInt24, Int32, UInt32, Int64 and UInt64 are supported.";

        // Fields

        /// <summary>
        /// Defines the available digits for a radix value codec.
        /// </summary>
        /// <remarks>
        /// Characters must be unique. Length determines radix, i.e., target base value.
        /// </remarks>
        public readonly string Digits;

        /// <summary>
        /// Gets the radix, i.e., target base value, for this <see cref="RadixCodec"/>.
        /// </summary>
        public int Radix => Digits.Length;

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
        /// <param name="caseSensitive">Determines if alphabetic radix <paramref name="digits"/> are case sensitive.</param>
        /// <remarks>
        /// Length of <paramref name="digits"/> will be radix, i.e., target base value.
        /// </remarks>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public RadixCodec(string digits, bool caseSensitive)
        {
            if (string.IsNullOrWhiteSpace(digits))
                throw new ArgumentNullException(nameof(digits));

            switch (digits.Length)
            {
                case < 2:
                    throw new ArgumentOutOfRangeException(nameof(digits), "Minimum of 2 characters required for radix value digits.");
                case > ushort.MaxValue:
                    throw new ArgumentOutOfRangeException(nameof(digits), $"Maximum number of radix value digits is {ushort.MaxValue:N0}.");
            }

            Digits = digits;

            // ReSharper disable once PossibleLossOfFraction
            RadixIntegerCodec<decimal> int96 = new(this, caseSensitive, 64, decimal.MinValue, 0, Math.Abs, (i, d) => (int)(i % d), (i, d) => (ulong)i / (ulong)d, (v, n) => n ? -v : v);
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
        public T Decode<T>(string radixValue) where T : unmanaged => (T)Decode(typeof(T), radixValue);

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
        public object Decode(Type type, string radixValue) => Type.GetTypeCode(type) switch
        {
            TypeCode.Int16 => m_int16.Decode(radixValue),
            TypeCode.UInt16 => m_uint16.Decode(radixValue),
            TypeCode.Int32 => m_int32.Decode(radixValue),
            TypeCode.UInt32 => m_uint32.Decode(radixValue),
            TypeCode.Int64 => m_int64.Decode(radixValue),
            TypeCode.UInt64 => m_uint64.Decode(radixValue),
            TypeCode.Object => 
                type == typeof(Int24) ? m_int24.Decode(radixValue) : 
                type == typeof(UInt24) ? m_uint24.Decode(radixValue) : 
                throw new ArgumentOutOfRangeException(nameof(type), InvalidType),
            _ => throw new ArgumentOutOfRangeException(nameof(type), InvalidType)
        };

        #endregion

        #region [ Static ]

        // Static Fields
        private static RadixCodec s_radix2;
        private static RadixCodec s_radix8;
        private static RadixCodec s_radix16;
        private static RadixCodec s_radix32;
        private static RadixCodec s_radix36;
        private static RadixCodec s_radix64;
        private static RadixCodec s_radix64B;
        private static RadixCodec s_radix86;
        private static RadixCodec s_radix256;
        private static RadixCodec s_radix65535;

        // Static Properties

        /// <summary>
        /// Gets a radix-2 value (binary) encoding.
        /// </summary>
        /// <remarks>
        /// int.MaxValue encodes to "1111111111111111111111111111111", 31 characters
        /// int.MinValue encodes to "-10000000000000000000000000000000", 33 characters
        /// uint.MaxValue encodes to "11111111111111111111111111111111", 32 characters
        /// long.MaxValue encodes to "111111111111111111111111111111111111111111111111111111111111111", 63 characters
        /// long.MinValue encodes to "-1000000000000000000000000000000000000000000000000000000000000000", 65 characters
        /// ulong.MaxValue encodes to "1111111111111111111111111111111111111111111111111111111111111111", 64 characters
        /// </remarks>                                                  12
        public static RadixCodec Radix2 => s_radix2 ??= new RadixCodec("01", false);

        /// <summary>
        /// Gets a radix-8 value (octal) encoding.
        /// </summary>
        /// <remarks>
        /// int.MaxValue encodes to "17777777777", 11 characters
        /// int.MinValue encodes to "-20000000000", 12 characters
        /// uint.MaxValue encodes to "37777777777", 11 characters
        /// long.MaxValue encodes to "777777777777777777777", 21 characters
        /// long.MinValue encodes to "-1000000000000000000000", 23 characters
        /// ulong.MaxValue encodes to "1777777777777777777777", 22 characters
        /// </remarks>                                                  12345678
        public static RadixCodec Radix8 => s_radix8 ??= new RadixCodec("01234567", false);

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
        /// </remarks>                                                    1234567890123456
        public static RadixCodec Radix16 => s_radix16 ??= new RadixCodec("0123456789ABCDEF", false);

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
        /// </remarks>                                                    12345678901234567890123456789012
        public static RadixCodec Radix32 => s_radix32 ??= new RadixCodec("0123456789ABCDEFGHIJKLMNOPQRSTUV", false);

        /// <summary>
        /// Gets a radix-36 value encoding.
        /// </summary>
        /// <remarks>
        /// int.MaxValue encodes to "ZIK0ZJ", 6 characters -- ideal 32-bit integer size for minimal case insensitive digit set
        /// int.MinValue encodes to "-ZIK0ZK", 7 characters
        /// uint.MaxValue encodes to "1Z141Z3", 7 characters
        /// long.MaxValue encodes to "1Y2P0IJ32E8E7", 13 characters
        /// long.MinValue encodes to "-1Y2P0IJ32E8E8", 14 characters
        /// ulong.MaxValue encodes to "3W5E11264SGSF", 13 characters
        /// </remarks>                                                    123456789012345678901234567890123456
        public static RadixCodec Radix36 => s_radix36 ??= new RadixCodec("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", false);

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
        /// </remarks>                                                    1234567890123456789012345678901234567890123456789012345678901234
        public static RadixCodec Radix64 => s_radix64 ??= new RadixCodec("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+/", true);

        /// <summary>
        /// Gets a radix-64 value encoding with the standard Base64 character sequence (results are unpadded).
        /// </summary>
        /// <remarks>
        /// int.MaxValue encodes to "B/////", 6 characters
        /// int.MinValue encodes to "-CAAAAA", 7 characters
        /// uint.MaxValue encodes to "D/////", 6 characters
        /// long.MaxValue encodes to "H//////////", 11 characters
        /// long.MinValue encodes to "-IAAAAAAAAAA", 12 characters
        /// ulong.MaxValue encodes to "P//////////", 11 characters
        /// </remarks>                                                      1234567890123456789012345678901234567890123456789012345678901234
        public static RadixCodec Radix64B => s_radix64B ??= new RadixCodec("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/", true);

        /// <summary>
        /// Gets a radix-86 value encoding.
        /// </summary>
        /// <remarks>
        /// int.MaxValue encodes to "$S2Jx", 5 characters
        /// int.MinValue encodes to "-$S2Jy", 6 characters -- base 86 reduces 32-bit integer sizes to a maximum of 6 characters
        /// uint.MaxValue encodes to "1qu4dh", 6 characters
        /// long.MaxValue encodes to "1X2qL^UmlIt", 11 characters
        /// long.MinValue encodes to "-1X2qL^UmlIu", 12 characters
        /// ulong.MaxValue encodes to "2&amp;5Sh]zLIbZ", 11 characters
        /// </remarks>                                                    1234567890123456789012345678901234567890123456789012345678901234567890123456
        public static RadixCodec Radix86 => s_radix86 ??= new RadixCodec("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&*+;=?@[]^", true);

        /// <summary>
        /// Gets a radix-256 value encoding.
        /// </summary>
        /// <remarks>
        /// int.MaxValue encodes to "Āƀƀƀ", 4 characters
        /// int.MinValue encodes to "-ā000", 5 characters -- base 256 reduces 32-bit integer sizes to a maximum of 5 characters
        /// uint.MaxValue encodes to "ƀƀƀƀ", 4 characters -- base 256 reduces 32-bit unsigned integer sizes to a maximum of 4 characters
        /// long.MaxValue encodes to "Āƀƀƀƀƀƀƀ", 8 characters
        /// long.MinValue encodes to "-ā0000000", 9 characters
        /// ulong.MaxValue encodes to "ƀƀƀƀƀƀƀƀ", 8 characters
        /// </remarks>
        public static RadixCodec Radix256 => s_radix256 ??= new RadixCodec(GenerateRadixDigits(256), true);

        /// <summary>
        /// Gets a radix-65535 value encoding. This is the largest supported radix.
        /// </summary>
        /// <remarks>
        /// int.MaxValue encodes to "魴魳", 2 characters
        /// int.MinValue encodes to "-魴魴", 3 characters -- base 65535 reduces 32-bit integer sizes to a maximum of 3 characters (not 3 bytes)
        /// uint.MaxValue encodes to "120", 3 characters
        /// long.MaxValue encodes to "魵魶魵魳", 4 characters
        /// long.MinValue encodes to "-魵魶魵魴", 5 characters
        /// ulong.MaxValue encodes to "14640", 5 characters
        /// </remarks>
        public static RadixCodec Radix65535 => s_radix65535 ??= new RadixCodec(GenerateRadixDigits(65535), true);

        private static string GenerateRadixDigits(int length)
        {
            StringBuilder digits = new(length);
            char digit = '0';

            for (int i = 0; i < length; i++)
            {
                digits.Append(digit++);

                while (!char.IsLetterOrDigit(digit))
                    digit++;
            }
            
            return digits.ToString();
        }

        #endregion
    }
}
