//******************************************************************************************************
//  Base36.cs - Gbtc
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
    /// Represents a radix value codec for conversion of base-10 integer values to and from other radix values.
    /// </summary>
    public class RadixCodec
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Defines the available digits for a radix value codec.
        /// </summary>
        public readonly string Digits;

        private readonly RadixCodec<short> m_int16;
        private readonly RadixCodec<ushort> m_uint16;
        private readonly RadixCodec<Int24> m_int24;
        private readonly RadixCodec<UInt24> m_uint24;
        private readonly RadixCodec<int> m_int32;
        private readonly RadixCodec<uint> m_uint32;
        private readonly RadixCodec<long> m_int64;
        private readonly RadixCodec<ulong> m_uint64;

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
            RadixCodec<decimal> int96 = new RadixCodec<decimal>(this, caseSensitive, 64, null, decimal.MinValue, 0, Math.Abs, (i, d) => (int)(i % d), (i, d) => (ulong)i / (ulong)d, (v, n) => n ? -v : v);
            m_int64 = new RadixCodec<long>(this, caseSensitive, 64, int96.Encode(long.MinValue), long.MinValue, 0L, Math.Abs, (i, d) => (int)(i % d), (i, d) => i / d, (v, n) => n ? -v : v);
            m_uint64 = new RadixCodec<ulong>(this, caseSensitive, 64, "0", ulong.MinValue, 0UL, i => i, (i, d) => (int)(i % (ulong)d), (i, d) => i / (ulong)d, (v, n) => (ulong)(n ? -v : v));
            m_int32 = new RadixCodec<int>(this, caseSensitive, 32, m_int64.Encode(int.MinValue), int.MinValue, 0, Math.Abs, (i, d) => i % d, (i, d) => i / d, (v, n) => (int)(n ? -v : v));
            m_uint32 = new RadixCodec<uint>(this, caseSensitive, 32, "0", uint.MinValue, 0U, i => i, (i, d) => (int)(i % (uint)d), (i, d) => i / (uint)d, (v, n) => (uint)(n ? -v : v));
            m_int24 = new RadixCodec<Int24>(this, caseSensitive, 24, m_int32.Encode(Int24.MinValue), Int24.MinValue, 0, i => (Int24)Math.Abs(i), (i, d) => i % d, (i, d) => (Int24)(i / d), (v, n) => (Int24)(n ? -v : v));
            m_uint24 = new RadixCodec<UInt24>(this, caseSensitive, 24, "0", UInt24.MinValue, 0, i => i, (i, d) => (int)(i % (uint)d), (i, d) => (UInt24)(i / (uint)d), (v, n) => (UInt24)(uint)(n ? -v : v));
            m_int16 = new RadixCodec<short>(this, caseSensitive, 16, m_int32.Encode(short.MinValue), short.MinValue, 0, Math.Abs, (i, d) => i % d, (i, d) => (short)(i / d), (v, n) => (short)(n ? -v : v));
            m_uint16 = new RadixCodec<ushort>(this, caseSensitive, 16, "0", ushort.MinValue, 0, i => i, (i, d) => i % (ushort)d, (i, d) => (ushort)(i / d), (v, n) => (ushort)(n ? -v : v));
        }

        #endregion

        #region [ Properties ]

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Base value string.</returns>
        public string Encode(short value) => m_int16.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Base value string.</returns>
        public string Encode(ushort value) => m_uint16.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Base value string.</returns>
        public string Encode(Int24 value) => m_int24.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Base value string.</returns>
        public string Encode(UInt24 value) => m_uint24.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Base value string.</returns>
        public string Encode(int value) => m_int32.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Base value string.</returns>
        public string Encode(uint value) => m_uint32.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Base value string.</returns>
        public string Encode(long value) => m_int64.Encode(value);

        /// <summary>
        /// Converts integer value to a radix value.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>Base value string.</returns>
        public string Encode(ulong value) => m_uint64.Encode(value);

        /// <summary>
        /// Attempts to convert a radix value to an integer value.
        /// </summary>
        /// <param name="baseValue">Base value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string baseValue, out short value)
        {
            try
            {
                value = m_int16.Decode(baseValue);
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
        /// <param name="baseValue">Base value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string baseValue, out ushort value)
        {
            try
            {
                value = m_uint16.Decode(baseValue);
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
        /// <param name="baseValue">Base value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string baseValue, out Int24 value)
        {
            try
            {
                value = m_int24.Decode(baseValue);
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
        /// <param name="baseValue">Base value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string baseValue, out UInt24 value)
        {
            try
            {
                value = m_uint24.Decode(baseValue);
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
        /// <param name="baseValue">Base value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string baseValue, out int value)
        {
            try
            {
                value = m_int32.Decode(baseValue);
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
        /// <param name="baseValue">Base value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string baseValue, out uint value)
        {
            try
            {
                value = m_uint32.Decode(baseValue);
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
        /// <param name="baseValue">Base value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string baseValue, out long value)
        {
            try
            {
                value = m_int64.Decode(baseValue);
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
        /// <param name="baseValue">Base value to convert.</param>
        /// <param name="value">Decoded integer value.</param>
        /// <returns><c>true</c> if decode succeeds; otherwise, <c>false</c>.</returns>
        public bool TryDecode(string baseValue, out ulong value)
        {
            try
            {
                value = m_uint64.Decode(baseValue);
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
        /// <param name="value">Base value to convert.</param>
        /// <returns>Decoded integer value.</returns>
        /// <exception cref="ArgumentNullException">Base value is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Invalid radix value character.</exception>
        /// <exception cref="OverflowException">Decoded radix value would overflow integer type.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Only integer types Int16, UInt16, Int24, UInt24, Int32, UInt32, Int64 and UInt64 are supported.</exception>
        public T Decode<T>(string value) => (T)Decode(typeof(T), value);

        /// <summary>
        /// Converts a radix value to an integer value.
        /// </summary>
        /// <param name="type">Integer type to convert.</param>
        /// <param name="value">Base value to convert.</param>
        /// <returns>Decoded integer value.</returns>
        /// <exception cref="ArgumentNullException">Base value is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Invalid radix value character.</exception>
        /// <exception cref="OverflowException">Decoded radix value would overflow integer type.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Only integer types Int16, UInt16, Int24, UInt24, Int32, UInt32, Int64 and UInt64 are supported.</exception>
        public object Decode(Type type, string value)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16:
                    return m_int16.Decode(value);
                case TypeCode.UInt16:
                    return m_uint16.Decode(value);
                case TypeCode.Int32:
                    if (type == typeof(Int24))
                        return m_int24.Decode(value);
                    return m_int32.Decode(value);
                case TypeCode.UInt32:
                    if (type == typeof(UInt24))
                        return m_uint24.Decode(value);
                    return m_uint32.Decode(value);
                case TypeCode.Int64:
                    return m_int64.Decode(value);
                case TypeCode.UInt64:
                    return m_uint64.Decode(value);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Only integer types Int16, UInt16, Int24, UInt24, Int32, UInt32, Int64 and UInt64 are supported.");
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static RadixCodec s_base16;
        private static RadixCodec s_base32;
        private static RadixCodec s_base36;
        private static RadixCodec s_base64;

        // Static Properties

        /// <summary>
        /// Gets a base-16 value encoding.
        /// </summary>
        public static RadixCodec Base16 = s_base16 ?? (s_base16 = new RadixCodec("0123456789ABCDEF", false));

        /// <summary>
        /// Gets a base-32 value encoding.
        /// </summary>
        public static RadixCodec Base32 = s_base32 ?? (s_base32 = new RadixCodec("0123456789ABCDEFGHIJKLMNOPQRSTUV", false));

        /// <summary>
        /// Gets a base-36 value encoding.
        /// </summary>
        public static RadixCodec Base36 = s_base36 ?? (s_base36 = new RadixCodec("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", false));

        /// <summary>
        /// Gets a base-64 value encoding.
        /// </summary>
        public static RadixCodec Base64 = s_base64 ?? (s_base64 = new RadixCodec("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+/", true));

        #endregion
    }

    internal class RadixCodec<T> where T : IComparable<T>, IEquatable<T>
    {
        #region [ Members ]

        private readonly RadixCodec m_parent;
        private readonly bool m_caseSensitive;
        private readonly int m_bitSize;
        private readonly string m_minRadix;
        private readonly T m_minValue;
        private readonly T m_zeroValue;
        private readonly Func<T, T> m_abs;
        private readonly Func<T, int, int> m_mod;
        private readonly Func<T, int, T> m_divide;
        private readonly Func<long, bool, T> m_convert;

        #endregion

        #region [ Constructors ]

        public RadixCodec(RadixCodec parent, bool caseSensitive, int bitSize, string minRadix, T minValue, T zeroValue, Func<T, T> abs, Func<T, int, int> mod, Func<T, int, T> divide, Func<long, bool, T> convert)
        {
            m_parent = parent;
            m_caseSensitive = caseSensitive;
            m_bitSize = bitSize;
            m_minRadix = minRadix;
            m_minValue = minValue;
            m_zeroValue = zeroValue;
            m_abs = abs;
            m_mod = mod;
            m_divide = divide;
            m_convert = convert;
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
                    throw new ArgumentException($"Invalid characters in base-{m_parent.Digits.Length} radix value: \"{value}\".", nameof(value));

                result += digit * multiplier;
                multiplier *= radix;
            }

            return m_convert(result, isNegative);
        }

        public string Encode(T value)
        {
            // Check for minimum T value, this avoids a Math.Abs exception:
            // "Negating the minimum value of a twos complement number is invalid."
            if (value.Equals(m_minValue))
                return m_minRadix;

            char[] buffer = new char[m_bitSize];
            int index = m_bitSize - 1;
            int radix = m_parent.Digits.Length;

            value = m_abs(value);

            do
                buffer[index--] = m_parent.Digits[m_mod(value, radix)];
            while ((value = m_divide(value, radix)).CompareTo(m_zeroValue) != 0);

            string result = new string(buffer, index + 1, m_bitSize - index - 1);

            if (value.CompareTo(m_zeroValue) < 0)
                result = $"-{result}";

            return result;
        }

        #endregion
    }
}
