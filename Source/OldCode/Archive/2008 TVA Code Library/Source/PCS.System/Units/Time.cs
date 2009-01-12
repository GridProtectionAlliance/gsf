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

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Units
{
    /// <summary>
    /// Represents an instant in time as a 64-bit signed integer with a value that is expressed as the
    /// number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
    /// </summary>
    /// <remarks>
    /// This class behaves just like an <see cref="Int64"/> representing a time in ticks; it is implictly
    /// castable to and from an <see cref="Int64"/>, a <see cref="DateTime"/> and a <see cref="TimeSpan"/>
    /// therefore can be generally used "as" an Int64, DateTime or TimeSpan directly, but it has the
    /// advantage of handling conversions to and from other time representations, specifically seconds,
    /// milliseconds, <see cref="NtpTimeTag"/>, <see cref="UnixTimeTag"/>, etc.
    /// </remarks>
    [Serializable()]
    public struct Time : IComparable, IFormattable, IConvertible, IComparable<Time>, IComparable<Int64>, IComparable<DateTime>, IComparable<TimeSpan>, IEquatable<Time>, IEquatable<Int64>, IEquatable<DateTime>, IEquatable<TimeSpan>
    {
        #region [ Members ]

        // Fields
        private long m_value; // Time value stored in ticks

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="Time"/>.
        /// </summary>
        /// <param name="value">New time value in ticks.</param>
        public Time(long value)
        {
            m_value = value;
        }

        /// <summary>
        /// Creates a new <see cref="Time"/>.
        /// </summary>
        /// <param name="value">New time value as a <see cref="DateTime"/>.</param>
        public Time(DateTime value)
        {
            m_value = value.Ticks;
        }

        /// <summary>
        /// Creates a new <see cref="Time"/>.
        /// </summary>
        /// <param name="value">New time value as a <see cref="TimeSpan"/>.</param>
        public Time(TimeSpan value)
        {
            m_value = value.Ticks;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the <see cref="Time"/> value in seconds representing the number of seconds that
        /// have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in seconds.</returns>
        public double ToSeconds()
        {
            return Ticks.ToSeconds(m_value);
        }

        /// <summary>
        /// Gets the <see cref="Time"/> value in milliseconds representing the number of milliseconds
        /// that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in milliseconds.</returns>
        public double ToMilliseconds()
        {
            return Ticks.ToMilliseconds(m_value);
        }

        /// <summary>
        /// Gets the <see cref="Time"/> value as an <see cref="NtpTimeTag"/>.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> as an <see cref="NtpTimeTag"/>.</returns>
        public NtpTimeTag ToNtpTimeTag()
        {
            return new NtpTimeTag((DateTime)this);
        }

        /// <summary>
        /// Gets the <see cref="Time"/> value as a <see cref="UnixTimeTag"/>.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> as a <see cref="UnixTimeTag"/>.</returns>
        public UnixTimeTag ToUnixTimeTag()
        {
            return new UnixTimeTag((DateTime)this);
        }

        /// <summary>
        /// Returns textual representation of <see cref="Time"/> in years, days, hours, minutes and seconds.
        /// </summary>
        /// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        /// <remarks>
        /// Set second precision to -1 to suppress seconds display.
        /// </remarks>
        public string ToText(int secondPrecision)
        {
            return Ticks.ToText(m_value, secondPrecision);
        }

        #region [ Numeric Interface Implementations ]

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">An object to compare, or null.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        /// <exception cref="ArgumentException">value is not an <see cref="Int64"/> or <see cref="Time"/>.</exception>
        public int CompareTo(object value)
        {
            if (value == null) return 1;

            if (!(value is long) && !(value is Time) && !(value is DateTime) && !(value is TimeSpan))
                throw new ArgumentException("Argument must be an Int64 or an Time");

            long num = (Time)value;
            return (m_value < num ? -1 : (m_value > num ? 1 : 0));
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Time"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="Time"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(Time value)
        {
            return CompareTo((long)value);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="DateTime"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(DateTime value)
        {
            return CompareTo((Time)value);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="TimeSpan"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="TimeSpan"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(TimeSpan value)
        {
            return CompareTo((Time)value);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Int64"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">An <see cref="Int64"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(long value)
        {
            return (m_value < value ? -1 : (m_value > value ? 1 : 0));
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// True if obj is an instance of <see cref="Int64"/> or <see cref="Time"/> and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is long || obj is Time || obj is DateTime || obj is TimeSpan)
                return Equals((Time)obj);

            return false;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Time"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="Time"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(Time obj)
        {
            return Equals((long)obj);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="DateTime"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(DateTime obj)
        {
            return Equals((Time)obj);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="TimeSpan"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(TimeSpan obj)
        {
            return Equals((Time)obj);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Int64"/> value.
        /// </summary>
        /// <param name="obj">An <see cref="Int64"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(long obj)
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
            return m_value.GetHashCode();
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
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.
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
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// The string representation of the value of this instance as specified by format and provider.
        /// </returns>
        public string ToString(string format, IFormatProvider provider)
        {
            return m_value.ToString(format, provider);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Time"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <returns>
        /// A <see cref="Time"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Time.MinValue"/> or greater than <see cref="Time.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        public static Time Parse(string s)
        {
            return (Time)long.Parse(s);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style to its <see cref="Time"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <returns>
        /// A <see cref="Time"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Time.MinValue"/> or greater than <see cref="Time.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        public static Time Parse(string s, NumberStyles style)
        {
            return (Time)long.Parse(s, style);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified culture-specific format to its <see cref="Time"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Time"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Time.MinValue"/> or greater than <see cref="Time.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        public static Time Parse(string s, IFormatProvider provider)
        {
            return (Time)long.Parse(s, provider);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its <see cref="Time"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Time"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Time.MinValue"/> or greater than <see cref="Time.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        public static Time Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            return (Time)long.Parse(s, style, provider);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Time"/> equivalent. A return value
        /// indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Time"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
        /// is not of the correct format, or represents a number less than <see cref="Time.MinValue"/> or greater than <see cref="Time.MaxValue"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out Time result)
        {
            long parseResult;
            bool parseResponse;

            parseResponse = long.TryParse(s, out parseResult);
            result = parseResult;

            return parseResponse;
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its
        /// <see cref="Time"/> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Time"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
        /// is not in a format compliant with style, or represents a number less than <see cref="Time.MinValue"/> or
        /// greater than <see cref="Time.MaxValue"/>. This parameter is passed uninitialized.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> object that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Time result)
        {
            long parseResult;
            bool parseResponse;

            parseResponse = long.TryParse(s, style, provider, out parseResult);
            result = parseResult;

            return parseResponse;
        }

        /// <summary>
        /// Returns the <see cref="TypeCode"/> for value type <see cref="Int64"/>.
        /// </summary>
        /// <returns>The enumerated constant, <see cref="TypeCode.Int64"/>.</returns>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Int64;
        }

        #region [ Explicit IConvertible Implementation ]

        // These are explicitly implemented on the native System.Int64 implementations, so we do the same...

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
            return Convert.ToInt32(m_value, provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(m_value, provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return m_value;
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

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(m_value, provider);
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.ChangeType(m_value, type, provider);
        }

        #endregion

        #endregion

        #endregion

        #region [ Operators ]

        #region [ Type Conversion Operators ]

        /// <summary>
        /// Implicitly converts value, represented in ticks, to a <see cref="Time"/>.
        /// </summary>
        public static implicit operator Time(Int64 value)
        {
            return new Time(value);
        }

        /// <summary>
        /// Implicitly converts value, represented as a <see cref="DateTime"/>, to a <see cref="Time"/>.
        /// </summary>
        public static implicit operator Time(DateTime value)
        {
            return new Time(value);
        }

        /// <summary>
        /// Implicitly converts value, represented as a <see cref="TimeSpan"/>, to a <see cref="Time"/>.
        /// </summary>
        public static implicit operator Time(TimeSpan value)
        {
            return new Time(value);
        }

        /// <summary>
        /// Implicitly converts <see cref="Time"/>, represented in ticks, to an <see cref="Int64"/>.
        /// </summary>
        public static implicit operator Int64(Time value)
        {
            return value.m_value;
        }

        /// <summary>
        /// Implicitly converts <see cref="Time"/>, represented in ticks, to an <see cref="DateTime"/>.
        /// </summary>
        public static implicit operator DateTime(Time value)
        {
            return new DateTime(value.m_value);
        }

        /// <summary>
        /// Implicitly converts <see cref="Time"/>, represented in ticks, to an <see cref="TimeSpan"/>.
        /// </summary>
        public static implicit operator TimeSpan(Time value)
        {
            return new TimeSpan(value.m_value);
        }

        #endregion

        #region [ Boolean and Bitwise Operators ]

        /// <summary>
        /// Returns true if value is not zero.
        /// </summary>
        public static bool operator true(Time value)
        {
            return (value.m_value != 0);
        }

        /// <summary>
        /// Returns true if value is equal to zero.
        /// </summary>
        public static bool operator false(Time value)
        {
            return (value.m_value == 0);
        }

        /// <summary>
        /// Returns bitwise complement of value.
        /// </summary>
        public static Time operator ~(Time value)
        {
            return ~value.m_value;
        }

        /// <summary>
        /// Returns logical bitwise AND of values.
        /// </summary>
        public static Time operator &(Time value1, Time value2)
        {
            return value1.m_value & value2.m_value;
        }

        /// <summary>
        /// Returns logical bitwise OR of values.
        /// </summary>
        public static Time operator |(Time value1, Time value2)
        {
            return value1.m_value | value2.m_value;
        }

        /// <summary>
        /// Returns logical bitwise exclusive-OR of values.
        /// </summary>
        public static Time operator ^(Time value1, Time value2)
        {
            return value1.m_value ^ value2.m_value;
        }

        /// <summary>
        /// Returns value after right shifts of first value by the number of bits specified by second value.
        /// </summary>
        public static Time operator >>(Time value, int shifts)
        {
            return (Time)(value.m_value >> shifts);
        }

        /// <summary>
        /// Returns value after left shifts of first value by the number of bits specified by second value.
        /// </summary>
        public static Time operator <<(Time value, int shifts)
        {
            return (Time)(value.m_value << shifts);
        }

        #endregion

        #region [ Arithmetic Operators ]

        /// <summary>
        /// Returns computed remainder after dividing first value by the second.
        /// </summary>
        public static Time operator %(Time value1, Time value2)
        {
            return value1.m_value % value2.m_value;
        }

        /// <summary>
        /// Returns computed sum of values.
        /// </summary>
        public static Time operator +(Time value1, Time value2)
        {
            return value1.m_value + value2.m_value;
        }

        /// <summary>
        /// Returns computed difference of values.
        /// </summary>
        public static Time operator -(Time value1, Time value2)
        {
            return value1.m_value - value2.m_value;
        }

        /// <summary>
        /// Returns computed product of values.
        /// </summary>
        public static Time operator *(Time value1, Time value2)
        {
            return value1.m_value * value2.m_value;
        }

        // Integer division operators

        /// <summary>
        /// Returns computed division of values.
        /// </summary>
        public static Time operator /(Time value1, Time value2)
        {
            return value1.m_value / value2.m_value;
        }

        // C# doesn't expose an exponent operator but some other .NET languages do,
        // so we expose the operator via its native special IL function name

        /// <summary>
        /// Returns result of first value raised to power of second value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced), SpecialName()]
        public static double op_Exponent(Time value1, Time value2)
        {
            return Math.Pow((double)value1.m_value, (double)value2.m_value);
        }

        #endregion

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>Represents the largest possible value of a <see cref="Time"/>. This field is constant.</summary>
        public static readonly Time MaxValue;

        /// <summary>Represents the smallest possible value of a <see cref="Time"/>. This field is constant.</summary>
        public static readonly Time MinValue;

        // Static Constructor
        static Time()
        {
            MaxValue = (Time)long.MaxValue;
            MinValue = (Time)long.MinValue;
        }

        // Static Methods
        
        /// <summary>
        /// Creates a new <see cref="Time"/> from the specified <paramref name="value"/> in seconds representing
        /// the number of seconds that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in seconds.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in seconds.</returns>
        public static Time FromSeconds(double value)
        {
            return new Time(Seconds.ToTicks(value));
        }

        /// <summary>
        /// Creates a new <see cref="Time"/> from the specified <paramref name="value"/> in milliseconds representing
        /// the number of milliseconds that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in milliseconds.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in milliseconds.</returns>
        public static Time FromMilliseconds(double value)
        {
            return new Time(Milliseconds.ToTicks(value));
        }

        /// <summary>
        /// Creates a new <see cref="Time"/> from the specified <see cref="NtpTimeTag"/>.
        /// </summary>
        /// <param name="value">New value for <see cref="Time"/> represented as an <see cref="NtpTimeTag"/>.</param>
        /// <returns>New <see cref="Time"/> object from the specified <see cref="NtpTimeTag"/>.</returns>
        public static Time FromNtpTimeTag(NtpTimeTag value)
        {
            return new Time(value.ToDateTime());
        }

        /// <summary>
        /// Creates a new <see cref="Time"/> from the specified <see cref="UnixTimeTag"/>.
        /// </summary>
        /// <param name="value">New value for <see cref="Time"/> represented as a <see cref="UnixTimeTag"/>.</param>
        /// <returns>New <see cref="Time"/> object from the specified <see cref="UnixTimeTag"/>.</returns>
        public static Time FromUnixTimeTag(UnixTimeTag value)
        {
            return new Time(value.ToDateTime());
        }

        #endregion        
    }
}