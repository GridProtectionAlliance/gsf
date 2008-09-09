//*******************************************************************************************************
//  TVA.Int24.vb - Representation of a 3-byte, 24-bit signed integer
//  Copyright © 2007 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  10/04/2007 - J. Ritchie Carroll
//       Original version of source code generated
//  09/08/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Globalization;
using System.ComponentModel;
using TVA.Interop;

/// <summary>Represents a 24-bit signed integer.</summary>
/// <remarks>
/// <para>
/// This class behaves like most other intrinsic signed integers but allows a 3-byte, 24-bit integer implementation
/// that is often found in many digital-signal processing arenas and different kinds of protocol parsing.  A signed
/// 24-bit integer is typically used to save storage space on disk where its value range of -8388608 to 8388607 is
/// sufficient, but the signed Int16 value range of -32768 to 32767 is too small.
/// </para>
/// <para>
/// This structure uses an Int32 internally for storage and most other common expected integer functionality, so using
/// a 24-bit integer will not save memory.  However, if the 24-bit signed integer range (-8388608 to 8388607) suits your
/// data needs you can save disk space by only storing the three bytes that this integer actually consumes.  You can do
/// this by calling the Int24.GetBytes function to return a three byte binary array that can be serialized to the desired
/// destination and then calling the Int24.GetValue function to restore the Int24 value from those three bytes.
/// </para>
/// <para>
/// All the standard operators for the Int24 have been fully defined for use with both Int24 and Int32 signed integers;
/// you should find that without the exception Int24 can be compared and numerically calculated with an Int24 or Int32.
/// Necessary casting should be minimal and typical use should be very simple - just as if you are using any other native
/// signed integer.
/// </para>
/// </remarks>
namespace TVA
{
    [Serializable()]
    public struct Int24 : IComparable, IFormattable, IConvertible, IComparable<Int24>, IComparable<Int32>, IEquatable<Int24>, IEquatable<Int32>
    {
        #region " Public Constants "

        /// <summary>High byte bit-mask used when a 24-bit integer is stored within a 32-bit integer. This field is constant.</summary>
        public const int BitMask = (int)(Bit.Bit24 | Bit.Bit25 | Bit.Bit26 | Bit.Bit27 | Bit.Bit28 | Bit.Bit29 | Bit.Bit30 | Bit.Bit31);

        /// <summary>Represents the largest possible value of an Int24 as an Int32. This field is constant.</summary>
        public const int MaxValue32 = 8388607;

        /// <summary>Represents the smallest possible value of an Int24 as an Int32. This field is constant.</summary>
        public const int MinValue32 = -8388608;

        #endregion

        #region " Member Fields "

        // We internally store the Int24 value in a 4-byte integer for convenience
        private int m_value;

        private static Int24 m_maxValue;
        private static Int24 m_minValue;

        #endregion

        #region " Constructors "

        static Int24()
        {
            m_maxValue = new Int24(MaxValue32);
            m_minValue = new Int24(MinValue32);
        }

        /// <summary>Creates 24-bit signed integer from an existing 24-bit signed integer.</summary>
        public Int24(Int24 value)
        {
            m_value = ApplyBitMask((int)value);
        }

        /// <summary>Creates 24-bit signed integer from a 32-bit signed integer.</summary>
        /// <param name="value">32-bit signed integer to use as new 24-bit signed integer value.</param>
        /// <exception cref="OverflowException">Source values outside 24-bit min/max range will cause an overflow exception.</exception>
        public Int24(int value)
        {
            ValidateNumericRange(value);
            m_value = ApplyBitMask(value);
        }

        /// <summary>Creates 24-bit signed integer from three bytes at a specified position in a byte array.</summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <remarks>
        /// <para>You can use this constructor in-lieu of a System.BitConverter.ToInt24 function.</para>
        /// <para>Bytes endian order assumed to match that of currently executing process architecture (little-endian on Intel platforms).</para>
        /// </remarks>
        public Int24(byte[] value, int startIndex)
        {
            m_value = ApplyBitMask((int)Int24.GetValue(value, startIndex));
        }

        #endregion

        #region " BitConverter Stand-in Operations "

        /// <summary>Returns the Int24 value as an array of three bytes.</summary>
        /// <returns>An array of bytes with length 3.</returns>
        /// <remarks>
        /// <para>You can use this function in-lieu of a System.BitConverter.GetBytes function.</para>
        /// <para>Bytes will be returned in endian order of currently executing process architecture (little-endian on Intel platforms).</para>
        /// </remarks>
        public byte[] GetBytes()
        {
            // Return serialized 3-byte representation of Int24
            return Int24.GetBytes(this);
        }

        /// <summary>Returns the specified Int24 value as an array of three bytes.</summary>
        /// <param name="value">Int24 value to </param>
        /// <returns>An array of bytes with length 3.</returns>
        /// <remarks>
        /// <para>You can use this function in-lieu of a System.BitConverter.GetBytes function.</para>
        /// <para>Bytes will be returned in endian order of currently executing process architecture (little-endian on Intel platforms).</para>
        /// </remarks>
        public static byte[] GetBytes(Int24 value)
        {
            // We use a 32-bit integer to store 24-bit integer internally
            byte[] int32Bytes = BitConverter.GetBytes((int)value);
            byte[] int24Bytes = new byte[3];

            if (BitConverter.IsLittleEndian)
            {
                // Copy little-endian bytes starting at index 0
                Buffer.BlockCopy(int32Bytes, 0, int24Bytes, 0, 3);
            }
            else
            {
                // Copy big-endian bytes starting at index 1
                Buffer.BlockCopy(int32Bytes, 1, int24Bytes, 0, 3);
            }

            // Return serialized 3-byte representation of Int24
            return int24Bytes;
        }

        /// <summary>Returns a 24-bit signed integer from three bytes at a specified position in a byte array.</summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A 24-bit signed integer formed by three bytes beginning at startIndex.</returns>
        /// <remarks>
        /// <para>You can use this function in-lieu of a System.BitConverter.ToInt24 function.</para>
        /// <para>Bytes endian order assumed to match that of currently executing process architecture (little-endian on Intel platforms).</para>
        /// </remarks>
        public static Int24 GetValue(byte[] value, int startIndex)
        {
            // We use a 32-bit integer to store 24-bit integer internally
            byte[] bytes = new byte[4];

            if (BitConverter.IsLittleEndian)
            {
                // Copy little-endian bytes starting at index 0 leaving byte at index 3 blank
                Buffer.BlockCopy(value, 0, bytes, 0, 3);
            }
            else
            {
                // Copy big-endian bytes starting at index 1 leaving byte at index 0 blank
                Buffer.BlockCopy(value, 0, bytes, 1, 3);
            }

            // Deserialize value
            return ((Int24)(ApplyBitMask(BitConverter.ToInt32(bytes, 0))));
        }

        #endregion

        #region " Int24 Operators "

        // Every effort has been made to make Int24 as cleanly interoperable with Int32 as possible...

        #region " Comparison Operators "

        public static bool operator ==(Int24 value1, Int24 value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator ==(int value1, Int24 value2)
        {
            return value1.Equals((int)value2);
        }

        public static bool operator ==(Int24 value1, int value2)
        {
            return ((int)value1).Equals(value2);
        }

        public static bool operator !=(Int24 value1, Int24 value2)
        {
            return !value1.Equals(value2);
        }

        public static bool operator !=(int value1, Int24 value2)
        {
            return !value1.Equals((int)value2);
        }

        public static bool operator !=(Int24 value1, int value2)
        {
            return !((int)value1).Equals(value2);
        }

        public static bool operator <(Int24 value1, Int24 value2)
        {
            return (value1.CompareTo(value2) < 0);
        }

        public static bool operator <(int value1, Int24 value2)
        {
            return (value1.CompareTo((int)value2) < 0);
        }

        public static bool operator <(Int24 value1, int value2)
        {
            return (value1.CompareTo(value2) < 0);
        }

        public static bool operator <=(Int24 value1, Int24 value2)
        {
            return (value1.CompareTo(value2) <= 0);
        }

        public static bool operator <=(int value1, Int24 value2)
        {
            return (value1.CompareTo((int)value2) <= 0);
        }

        public static bool operator <=(Int24 value1, int value2)
        {
            return (value1.CompareTo(value2) <= 0);
        }

        public static bool operator >(Int24 value1, Int24 value2)
        {
            return (value1.CompareTo(value2) > 0);
        }

        public static bool operator >(int value1, Int24 value2)
        {
            return (value1.CompareTo((int)value2) > 0);
        }

        public static bool operator >(Int24 value1, int value2)
        {
            return (value1.CompareTo(value2) > 0);
        }

        public static bool operator >=(Int24 value1, Int24 value2)
        {
            return (value1.CompareTo(value2) >= 0);
        }

        public static bool operator >=(int value1, Int24 value2)
        {
            return (value1.CompareTo((int)value2) >= 0);
        }

        public static bool operator >=(Int24 value1, int value2)
        {
            return (value1.CompareTo(value2) >= 0);
        }

        #endregion

        #region " Type Conversion Operators "

        #region " Explicit Narrowing Conversions "

        public static explicit operator Int24(string value)
        {
            return new Int24(Convert.ToInt32(value));
        }

        public static explicit operator Int24(decimal value)
        {
            return new Int24(Convert.ToInt32(value));
        }

        public static explicit operator Int24(double value)
        {
            return new Int24(Convert.ToInt32(value));
        }

        public static explicit operator Int24(float value)
        {
            return new Int24(Convert.ToInt32(value));
        }

        public static explicit operator Int24(long value)
        {
            return new Int24(Convert.ToInt32(value));
        }

        public static explicit operator Int24(int value)
        {
            return new Int24(value);
        }

        public static explicit operator short(Int24 value)
        {
            return ((short)((int)value));
        }

        public static explicit operator byte(Int24 value)
        {
            return ((byte)((int)value));
        }

        #endregion

        #region " Implicit Widening Conversions "

        public static implicit operator Int24(byte value)
        {
            return new Int24(Convert.ToInt32(value));
        }

        public static implicit operator Int24(char value)
        {
            return new Int24(Convert.ToInt32(value));
        }

        public static implicit operator Int24(short value)
        {
            return new Int24(Convert.ToInt32(value));
        }

        public static implicit operator int(Int24 value)
        {
            return value.ToInt32(null);
        }

        [CLSCompliant(false)]
        public static implicit operator uint(Int24 value)
        {
            return value.ToUInt32(null);
        }

        public static implicit operator long(Int24 value)
        {
            return value.ToInt64(null);
        }

        [CLSCompliant(false)]
        public static implicit operator ulong(Int24 value)
        {
            return value.ToUInt64(null);
        }

        public static implicit operator double(Int24 value)
        {
            return value.ToDouble(null);
        }

        public static implicit operator float(Int24 value)
        {
            return value.ToSingle(null);
        }

        public static implicit operator decimal(Int24 value)
        {
            return value.ToDecimal(null);
        }

        public static implicit operator string(Int24 value)
        {
            return value.ToString();
        }

        #endregion

        #endregion

        #region " Boolean and Bitwise Operators "

        public static bool operator true(Int24 value)
        {
            return (value != 0);
        }

        public static bool operator false(Int24 value)
        {
            return (value == 0);
        }

        public static Int24 operator ~(Int24 value)
        {
            return ((Int24)(ApplyBitMask(~(int)value)));
        }

        public static Int24 operator &(Int24 value1, Int24 value2)
        {
            return ((Int24)(ApplyBitMask((int)value1 & (int)value2)));
        }

        public static int operator &(int value1, Int24 value2)
        {
            return (value1 & (int)value2);
        }

        public static int operator &(Int24 value1, int value2)
        {
            return ((int)value1 & value2);
        }

        public static Int24 operator |(Int24 value1, Int24 value2)
        {
            return ((Int24)(ApplyBitMask((int)value1 | (int)value2)));
        }

        public static int operator |(int value1, Int24 value2)
        {
            return (value1 | (int)value2);
        }

        public static int operator |(Int24 value1, int value2)
        {
            return ((int)value1 | value2);
        }

        public static Int24 operator ^(Int24 value1, Int24 value2)
        {
            return ((Int24)(ApplyBitMask((int)value1 ^ (int)value2)));
        }

        public static int operator ^(int value1, Int24 value2)
        {
            return (value1 ^ (int)value2);
        }

        public static int operator ^(Int24 value1, int value2)
        {
            return ((int)value1 ^ value2);
        }

        // C# doesn't expose an exponent operator but some other .NET languages do,
        // so we expose the operator via its native special IL function name

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static double op_Exponent(Int24 value1, Int24 value2)
        {
            return System.Math.Pow((double)value1, (double)value2);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static double op_Exponent(int value1, Int24 value2)
        {
            return System.Math.Pow((double)value1, (double)value2);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static double op_Exponent(Int24 value1, int value2)
        {
            return System.Math.Pow((double)value1, (double)value2);
        }

        #endregion

        #region " Arithmetic Operators "

        public static Int24 operator %(Int24 value1, Int24 value2)
        {
            return ((Int24)((int)value1 % (int)value2));
        }

        public static int operator %(int value1, Int24 value2)
        {
            return (value1 % (int)value2);
        }

        public static int operator %(Int24 value1, int value2)
        {
            return ((int)value1 % value2);
        }

        public static Int24 operator +(Int24 value1, Int24 value2)
        {
            return ((Int24)((int)value1 + (int)value2));
        }

        public static int operator +(int value1, Int24 value2)
        {
            return (value1 + (int)value2);
        }

        public static int operator +(Int24 value1, int value2)
        {
            return ((int)value1 + value2);
        }

        public static Int24 operator -(Int24 value1, Int24 value2)
        {
            return ((Int24)((int)value1 - (int)value2));
        }

        public static int operator -(int value1, Int24 value2)
        {
            return (value1 - (int)value2);
        }

        public static int operator -(Int24 value1, int value2)
        {
            return ((int)value1 - value2);
        }

        public static Int24 operator *(Int24 value1, Int24 value2)
        {
            return ((Int24)((int)value1 * (int)value2));
        }

        public static int operator *(int value1, Int24 value2)
        {
            return (value1 * (int)value2);
        }

        public static int operator *(Int24 value1, int value2)
        {
            return ((int)value1 * value2);
        }

        // Integer division operators
        public static Int24 operator /(Int24 value1, Int24 value2)
        {
            return ((Int24)((int)value1 / (int)value2));
        }

        public static int operator /(int value1, Int24 value2)
        {
            return (value1 / (int)value2);
        }

        public static int operator /(Int24 value1, int value2)
        {
            return ((int)value1 / value2);
        }

        // Standard division operators
        public static double operator /(Int24 value1, Int24 value2)
        {
            return ((double)value1 / (double)value2);
        }

        public static double operator /(int value1, Int24 value2)
        {
            return ((double)value1 / (double)value2);
        }

        public static double operator /(Int24 value1, int value2)
        {
            return ((double)value1 / (double)value2);
        }

        public static Int24 operator >>(Int24 value, int shifts)
        {
            return ((Int24)(ApplyBitMask((int)value >> shifts)));
        }

        public static Int24 operator <<(Int24 value, int shifts)
        {
            return ((Int24)(ApplyBitMask((int)value << shifts)));
        }

        #endregion

        #endregion

        #region " Int24 Specific Functions "

        /// <summary>Represents the largest possible value of an Int24. This field is constant.</summary>
        public static Int24 MaxValue
        {
            get
            {
                return m_maxValue;
            }
        }

        /// <summary>Represents the smallest possible value of an Int24. This field is constant.</summary>
        public static Int24 MinValue
        {
            get
            {
                return m_minValue;
            }
        }

        private static void ValidateNumericRange(int value)
        {
            if (value > (Int24.MaxValue32 + 1) || value < Int24.MinValue32)
            {
                throw new OverflowException(string.Format("Value of {0} will not fit in a 24-bit signed integer", value));
            }
        }

        private static int ApplyBitMask(int value)
        {
            if ((value & Bit.Bit23) > 0)
            {
                // If the sign-bit is set, this number will be negative - set all high-byte bits (keeps 32-bit number in 24-bit range)
                value |= BitMask;
            }
            else
            {
                // If the sign-bit is not set, this number will be positive - clear all high-byte bits (keeps 32-bit number in 24-bit range)
                value &= ~BitMask;
            }

            return value;
        }

        #endregion

        #region " Standard Numeric Operations "

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">An object to compare, or null.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        /// <exception cref="ArgumentException">value is not an Int32 or Int24.</exception>
        public int CompareTo(object value)
        {
            if (value == null) return 1;
            if (!(value is int) && !(value is Int24)) throw new ArgumentException("Argument must be an Int32 or an Int24");

            int num = (int)value;
            return (m_value < num ? -1 : (m_value > num ? 1 : 0));
        }

        /// <summary>
        /// Compares this instance to a specified 32-bit signed integer and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">An integer to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(Int24 value)
        {
            return CompareTo((int)value);
        }

        /// <summary>
        /// Compares this instance to a specified 32-bit signed integer and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">An integer to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(int value)
        {
            return (m_value < value ? -1 : (m_value > value ? 1 : 0));
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// True if obj is an instance of Int32 or Int24 and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is int || obj is Int24) return Equals((int)obj);
            return false;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified Int24 value.
        /// </summary>
        /// <param name="obj">An Int24 value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(Int24 obj)
        {
            return Equals((int)obj);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified Int32 value.
        /// </summary>
        /// <param name="obj">An Int32 value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(int obj)
        {
            return (m_value == obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
        {
            return m_value;
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>
        /// The string representation of the value of this instance, consisting of a minus sign if
        /// the value is negative, and a sequence of digits ranging from 0 to 9 with no leading zeroes.
        /// </returns>
        public override string ToString()
        {
            return m_value.ToString();
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation, using
        /// the specified format.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <returns>
        /// The string representation of the value of this instance as specified by format.
        /// </returns>
        public string ToString(string format)
        {
            return m_value.ToString(format);
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation using the
        /// specified culture-specific format information.
        /// </summary>
        /// <param name="provider">
        /// An System.IFormatProvider that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// The string representation of the value of this instance as specified by provider.
        /// </returns>
        public string ToString(IFormatProvider provider)
        {
            return m_value.ToString(provider);
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation using the
        /// specified format and culture-specific format information.
        /// </summary>
        /// <param name="format">A format specification.</param>
        /// <param name="provider">
        /// An System.IFormatProvider that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// The string representation of the value of this instance as specified by format and provider.
        /// </returns>
        public string ToString(string format, IFormatProvider provider)
        {
            return m_value.ToString(format, provider);
        }

        /// <summary>
        /// Converts the string representation of a number to its 24-bit signed integer equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <returns>
        /// A 24-bit signed integer equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowAction">
        /// s represents a number less than Int24.MinValue or greater than Int24.MaxValue.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        public static Int24 Parse(string s)
        {
            return ((Int24)int.Parse(s));
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style to its 24-bit signed integer equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// A typical value to specify is System.Globalization.NumberStyles.Integer.
        /// </param>
        /// <returns>
        /// A 24-bit signed integer equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowAction">
        /// s represents a number less than Int24.MinValue or greater than Int24.MaxValue.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        public static Int24 Parse(string s, NumberStyles style)
        {
            return ((Int24)int.Parse(s, style));
        }

        /// <summary>
        /// Converts the string representation of a number in a specified culture-specific format to its 24-bit
        /// signed integer equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="provider">
        /// An System.IFormatProvider that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A 24-bit signed integer equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowAction">
        /// s represents a number less than Int24.MinValue or greater than Int24.MaxValue.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        public static Int24 Parse(string s, IFormatProvider provider)
        {
            return ((Int24)int.Parse(s, provider));
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its 24-bit
        /// signed integer equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// A typical value to specify is System.Globalization.NumberStyles.Integer.
        /// </param>
        /// <param name="provider">
        /// An System.IFormatProvider that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A 24-bit signed integer equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowAction">
        /// s represents a number less than Int24.MinValue or greater than Int24.MaxValue.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        public static Int24 Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            return ((Int24)int.Parse(s, style, provider));
        }

        /// <summary>
        /// Converts the string representation of a number to its 24-bit signed integer equivalent. A return value
        /// indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the 24-bit signed integer value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
        /// is not of the correct format, or represents a number less than Int24.MinValue or greater than Int24.MaxValue.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out Int24 result)
        {
            int parseResult;
            bool parseResponse;

            parseResponse = int.TryParse(s, out parseResult);

            try
            {
                result = (Int24)parseResult;
            }
            catch
            {
                result = (Int24)(0);
                parseResponse = false;
            }

            return parseResponse;
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its
        /// 24-bit signed integer equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// A typical value to specify is System.Globalization.NumberStyles.Integer.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the 24-bit signed integer value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
        /// is not in a format compliant with style, or represents a number less than Int24.MinValue or greater than
        /// Int24.MaxValue. This parameter is passed uninitialized.
        /// </param>
        /// <param name="provider">
        /// An System.IFormatProvider objectthat supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Int24 result)
        {
            int parseResult;
            bool parseResponse;

            parseResponse = int.TryParse(s, style, provider, out parseResult);

            try
            {
                result = (Int24)parseResult;
            }
            catch
            {
                result = (Int24)(0);
                parseResponse = false;
            }

            return parseResponse;
        }

        /// <summary>
        /// Returns the System.TypeCode for value type System.Int32 (there is no defined type code for an Int24).
        /// </summary>
        /// <returns>The enumerated constant, System.TypeCode.Int32.</returns>
        /// <remarks>
        /// There is no defined Int24 type code and since an Int24 will easily fit inside an Int32, the
        /// Int32 type code is returned.
        /// </remarks>
        public TypeCode GetTypeCode()
        {
            // There is no Int24 type code, and an Int24 will fit inside an Int32 - so we return an Int32 type code
            return TypeCode.Int32;
        }

        #region " Explicit IConvertible Implementation "

        // These are explicitly implemented on the native integer implementations, so we do the same...

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(m_value, provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(m_value, provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(m_value, provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(m_value, provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(m_value, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(m_value, provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return m_value;
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(m_value, provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(m_value, provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(m_value, provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(m_value, provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(m_value, provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(m_value, provider);
        }

        System.DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(m_value, provider);
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.ChangeType(m_value, type, provider);
        }

        #endregion

        #endregion
    }
}
