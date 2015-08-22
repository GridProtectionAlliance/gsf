/**************************************************************************\
   Copyright © 2009 - Gbtc, James Ritchie Carroll
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
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/25/2008 - James R. Carroll
//       Initial version of source generated.
//  09/11/2008 - James R. Carroll
//      Converted to C#.
//  08/10/2009 - Josh Patterson
//      Edited Comments
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace TVA.Units
{
    /// <summary>Represents a time measurement, in seconds, as a double-precision floating-point number.</summary>
    /// <remarks>
    /// This class behaves just like a <see cref="Double"/> representing a time in seconds; it is implictly
    /// castable to and from a <see cref="Double"/> and therefore can be generally used "as" a double, but it
    /// has the advantage of handling conversions to and from other time representations, specifically
    /// minutes, hours, days, weeks, atomic units of time, Planck time and ke. Metric conversions are handled
    /// simply by applying the needed <see cref="SI"/> conversion factor, for example:
    /// <example>
    /// Convert time in nanoseconds to seconds:
    /// <code>
    /// public Time GetSeconds(double nanoseconds)
    /// {
    ///     return nanoseconds * SI.Nano;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// Convert time in seconds to milliseconds:
    /// <code>
    /// public double GetMilliseconds(Time seconds)
    /// {
    ///     return time / SI.Milli;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// This example converts minutes to hours:
    /// <code>
    /// public double GetHours(double minutes)
    /// {
    ///     return Time.FromMinutes(minutes).ToHours();
    /// }
    /// </code>
    /// </example>
    /// <para>
    /// Note that the <see cref="Time.ToString()"/> method will convert the <see cref="Time"/> value, in seconds,
    /// into a textual representation of years, days, hours, minutes and seconds.
    /// </para>
    /// </remarks>
    [Serializable()]
    public struct Time : IComparable, IFormattable, IConvertible, IComparable<Time>, IComparable<TimeSpan>, IComparable<Double>, IEquatable<Time>, IEquatable<TimeSpan>, IEquatable<Double>
    {       
        #region [ Members ]

        // Constants
        private const double AtomicUnitsOfTimeFactor = 2.418884254e-17D;

        private const double PlanckTimeFactor = 1.351211818e-43D;

        private const double KeFactor = 864.0D;

        /// <summary>Fractional number of seconds in one tick.</summary>
        public const double SecondsPerTick = 1.0D / Ticks.PerSecond;

        /// <summary>Number of seconds in one minute.</summary>
        public const int SecondsPerMinute = 60;

        /// <summary>Number of seconds in one hour.</summary>
        public const int SecondsPerHour = 60 * SecondsPerMinute;

        /// <summary>Number of seconds in one day.</summary>
        public const int SecondsPerDay = 24 * SecondsPerHour;

        /// <summary>Number of seconds in one week.</summary>
        public const int SecondsPerWeek = 7 * SecondsPerDay;

        // Fields
        private double m_value; // Time value stored in seconds

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="Time"/>.
        /// </summary>
        /// <param name="value">New time value in seconds.</param>
        public Time(double value)
        {
            m_value = value;
        }

        /// <summary>
        /// Creates a new <see cref="Time"/>.
        /// </summary>
        /// <param name="value">New time value as a <see cref="TimeSpan"/>.</param>
        public Time(TimeSpan value)
        {
            m_value = value.TotalSeconds;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the <see cref="Time"/> value in atomic units of time.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in atomic units of time.</returns>
        public double ToAtomicUnitsOfTime()
        {
            return m_value / AtomicUnitsOfTimeFactor;
        }

        /// <summary>
        /// Gets the <see cref="Time"/> value in Planck time.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in Planck time.</returns>
        public double ToPlanckTime()
        {
            return m_value / PlanckTimeFactor;
        }

        /// <summary>
        /// Gets the <see cref="Time"/> value in ke, the traditional Chinese unit of decimal time.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in ke.</returns>
        public double ToKe()
        {
            return m_value / KeFactor;
        }

        /// <summary>
        /// Gets the <see cref="Time"/> value in minutes.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in minutes.</returns>
        public double ToMinutes()
        {
            return m_value / SecondsPerMinute;
        }

        /// <summary>
        /// Gets the <see cref="Time"/> value in hours.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in hours.</returns>
        public double ToHours()
        {
            return m_value / SecondsPerHour;
        }

        /// <summary>
        /// Gets the <see cref="Time"/> value in days.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in days.</returns>
        public double ToDays()
        {
            return m_value / SecondsPerDay;
        }

        /// <summary>
        /// Gets the <see cref="Time"/> value in weeks.
        /// </summary>
        /// <returns>Value of <see cref="Time"/> in weeks.</returns>
        public double ToWeeks()
        {
            return m_value / SecondsPerWeek;
        }

        /// <summary>
        /// Converts the <see cref="Time"/> value, in seconds, to 100-nanosecond tick intervals.
        /// </summary>
        /// <returns>A <see cref="Ticks"/> object.</returns>
        public Ticks ToTicks()
        {
            return (Ticks)(m_value * Ticks.PerSecond);
        }

        /// <summary>
        /// Converts the <see cref="Time"/> value into a textual representation of years, days, hours,
        /// minutes and seconds.
        /// </summary>
        /// <remarks>
        /// Note that this ToString overload will not display fractional seconds. To allow display of
        /// fractional seconds, or completely remove second resolution from the textual representation,
        /// use the <see cref="Time.ToString(int)"/> overload instead.
        /// </remarks>
        /// <returns>
        /// The string representation of the value of this instance, consisting of the number of
        /// years, days, hours, minutes and seconds represented by this value.
        /// </returns>
        public override string ToString()
        {
            return ToTicks().ToString();
        }

        /// <summary>
        /// Converts the <see cref="Time"/> value into a textual representation of years, days, hours,
        /// minutes and seconds with the specified number of fractional digits.
        /// </summary>
        /// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        /// <remarks>Set second precision to -1 to suppress seconds display.</remarks>
        /// <returns>
        /// The string representation of the value of this instance, consisting of the number of
        /// years, days, hours, minutes and seconds represented by this value.
        /// </returns>
        public string ToString(int secondPrecision)
        {
            return ToTicks().ToString(secondPrecision);
        }

        /// <summary>
        /// Converts the <see cref="Time"/> value into a textual representation of years, days, hours,
        /// minutes and seconds with the specified number of fractional digits given string array of
        /// time names.
        /// </summary>
        /// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        /// <param name="timeNames">Time names array to use during textual conversion.</param>
        /// <remarks>
        /// <para>Set second precision to -1 to suppress seconds display.</para>
        /// <para>
        /// <paramref name="timeNames"/> array needs one string entry for each of the following names:<br/>
        /// "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds",
        /// "Less Than 60 Seconds", "0 Seconds".
        /// </para>
        /// </remarks>
        /// <returns>
        /// The string representation of the value of this instance, consisting of the number of
        /// years, days, hours, minutes and seconds represented by this value.
        /// </returns>
        public string ToString(int secondPrecision, string[] timeNames)
        {
            return ToTicks().ToString(secondPrecision, timeNames);
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
        /// <exception cref="ArgumentException">value is not a <see cref="Double"/> or <see cref="Time"/>.</exception>
        public int CompareTo(object value)
        {
            if (value == null) return 1;

            if (!(value is double) && !(value is Time) && !(value is DateTime) && !(value is TimeSpan))
                throw new ArgumentException("Argument must be a Double or a Time");

            double num = (Time)value;
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
            return CompareTo((double)value);
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
            return CompareTo(value.TotalSeconds);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Double"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">An <see cref="Double"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(double value)
        {
            return (m_value < value ? -1 : (m_value > value ? 1 : 0));
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// True if obj is an instance of <see cref="Double"/> or <see cref="Time"/> and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is double || obj is Time || obj is TimeSpan)
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
            return Equals((double)obj);
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
            return Equals(obj.TotalSeconds);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Double"/> value.
        /// </summary>
        /// <param name="obj">An <see cref="Double"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(double obj)
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
        /// Converts the numeric value of this instance to its equivalent string representation, using
        /// the specified format.
        /// </summary>
        /// <remarks>
        /// Note that this ToString overload matches <see cref="Double.ToString(string)"/>, use
        /// <see cref="Time.ToString(int)"/> to convert <see cref="Time"/> value into a textual
        /// representation of years, days, hours, minutes and seconds.
        /// </remarks>
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
        /// <remarks>
        /// Note that this ToString overload matches <see cref="Double.ToString(IFormatProvider)"/>, use
        /// <see cref="Time.ToString(int)"/> to convert <see cref="Time"/> value into a textual
        /// representation of years, days, hours, minutes and seconds.
        /// </remarks>
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
        /// <remarks>
        /// Note that this ToString overload matches <see cref="Double.ToString(string,IFormatProvider)"/>, use
        /// <see cref="Time.ToString(int)"/> to convert <see cref="Time"/> value into a textual representation
        /// of years, days, hours, minutes and seconds.
        /// </remarks>
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
            return (Time)double.Parse(s);
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
            return (Time)double.Parse(s, style);
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
            return (Time)double.Parse(s, provider);
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
            return (Time)double.Parse(s, style, provider);
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
            double parseResult;
            bool parseResponse;

            parseResponse = double.TryParse(s, out parseResult);
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
            double parseResult;
            bool parseResponse;

            parseResponse = double.TryParse(s, style, provider, out parseResult);
            result = parseResult;

            return parseResponse;
        }

        /// <summary>
        /// Returns the <see cref="TypeCode"/> for value type <see cref="Double"/>.
        /// </summary>
        /// <returns>The enumerated constant, <see cref="TypeCode.Double"/>.</returns>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Double;
        }

        #region [ Explicit IConvertible Implementation ]

        // These are explicitly implemented on the native System.Double implementations, so we do the same...

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
            return Convert.ToInt64(m_value);
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
            return m_value;
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

        #region [ Comparison Operators ]

        /// <summary>
        /// Compares the two values for equality.
        /// </summary>
        /// <param name="value1">A <see cref="Time"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Time"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> as the result of the operation.</returns>
        public static bool operator ==(Time value1, Time value2)
        {
            return value1.Equals(value2);
        }

        /// <summary>
        /// Compares the two values for inequality.
        /// </summary>
        /// <param name="value1">A <see cref="Time"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Time"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> as the result of the operation.</returns>
        public static bool operator !=(Time value1, Time value2)
        {
            return !value1.Equals(value2);
        }

        /// <summary>
        /// Returns true if left value is less than right value.
        /// </summary>
        /// <param name="value1">A <see cref="Time"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Time"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> as the result of the operation.</returns>
        public static bool operator <(Time value1, Time value2)
        {
            return (value1.CompareTo(value2) < 0);
        }

        /// <summary>
        /// Returns true if left value is less or equal to than right value.
        /// </summary>
        /// <param name="value1">A <see cref="Time"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Time"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> as the result of the operation.</returns>
        public static bool operator <=(Time value1, Time value2)
        {
            return (value1.CompareTo(value2) <= 0);
        }

        /// <summary>
        /// Returns true if left value is greater than right value.
        /// </summary>
        /// <param name="value1">A <see cref="Time"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Time"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> as the result of the operation.</returns>
        public static bool operator >(Time value1, Time value2)
        {
            return (value1.CompareTo(value2) > 0);
        }

        /// <summary>
        /// Returns true if left value is greater than or equal to right value.
        /// </summary>
        /// <param name="value1">A <see cref="Time"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Time"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> as the result of the operation.</returns>
        public static bool operator >=(Time value1, Time value2)
        {
            return (value1.CompareTo(value2) >= 0);
        }

        #endregion

        #region [ Type Conversion Operators ]

        /// <summary>
        /// Implicitly converts value, represented in seconds, to a <see cref="Time"/>.
        /// </summary>
        /// <param name="value">A <see cref="Double"/> value.</param>
        /// <returns>A <see cref="Time"/> object.</returns>
        public static implicit operator Time(Double value)
        {
            return new Time(value);
        }

        /// <summary>
        /// Implicitly converts value, represented as a <see cref="TimeSpan"/>, to a <see cref="Time"/>.
        /// </summary>
        /// <param name="value">A <see cref="TimeSpan"/> object.</param>
        /// <returns>A <see cref="Time"/> object.</returns>
        public static implicit operator Time(TimeSpan value)
        {
            return new Time(value);
        }

        /// <summary>
        /// Implicitly converts <see cref="Time"/>, represented in seconds, to a <see cref="Double"/>.
        /// </summary>
        /// <param name="value">A <see cref="Time"/> object.</param>
        /// <returns>A <see cref="Double"/> value.</returns>
        public static implicit operator Double(Time value)
        {
            return value.m_value;
        }

        /// <summary>
        /// Implicitly converts <see cref="Time"/>, represented in seconds, to a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="value">A <see cref="Time"/> object.</param>
        /// <returns>A <see cref="TimeSpan"/> object.</returns>
        public static implicit operator TimeSpan(Time value)
        {
            return new TimeSpan(value.ToTicks());
        }

        #endregion

        #region [ Arithmetic Operators ]

        /// <summary>
        /// Returns computed remainder after dividing first value by the second.
        /// </summary>
        /// <param name="value1">A <see cref="Time"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Time"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Time"/> object as the result of the operation.</returns>
        public static Time operator %(Time value1, Time value2)
        {
            return value1.m_value % value2.m_value;
        }

        /// <summary>
        /// Returns computed sum of values.
        /// </summary>
        /// <param name="value1">A <see cref="Time"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Time"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Time"/> object as the result of the operation.</returns>
        public static Time operator +(Time value1, Time value2)
        {
            return value1.m_value + value2.m_value;
        }

        /// <summary>
        /// Returns computed difference of values.
        /// </summary>
        /// <param name="value1">A <see cref="Time"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Time"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Time"/> object as the result of the operation.</returns>
        public static Time operator -(Time value1, Time value2)
        {
            return value1.m_value - value2.m_value;
        }

        /// <summary>
        /// Returns computed product of values.
        /// </summary>
        /// <param name="value1">A <see cref="Time"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Time"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Time"/> object as the result of the operation.</returns>
        public static Time operator *(Time value1, Time value2)
        {
            return value1.m_value * value2.m_value;
        }

        /// <summary>
        /// Returns computed division of values.
        /// </summary>
        /// <param name="value1">A <see cref="Time"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Time"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Time"/> object as the result of the operation.</returns>
        public static Time operator /(Time value1, Time value2)
        {
            return value1.m_value / value2.m_value;
        }

        // C# doesn't expose an exponent operator but some other .NET languages do,
        // so we expose the operator via its native special IL function name

        /// <summary>
        /// Returns result of first value raised to power of second value.
        /// </summary>
        /// <param name="value1">A <see cref="Time"/> object as the left hand operand.</param>
        /// <param name="value2">A <see cref="Time"/> object as the right hand operand.</param>
        /// <returns>A <see cref="Double"/> value as the result of the operation.</returns>
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
        public static readonly Time MaxValue = (Time)double.MaxValue;

        /// <summary>Represents the smallest possible value of a <see cref="Time"/>. This field is constant.</summary>
        public static readonly Time MinValue = (Time)double.MinValue;

        // Static Methods

        /// <summary>
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in atomic units of time.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in atomic units of time.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in atomic units of time.</returns>
        public static Time FromAtomicUnitsOfTime(double value)
        {
            return new Time(value * AtomicUnitsOfTimeFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in Planck time.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in Planck time.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in Planck time.</returns>
        public static Time FromPlanckTime(double value)
        {
            return new Time(value * PlanckTimeFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in ke,
        /// the traditional Chinese unit of decimal time.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in ke.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in ke.</returns>
        public static Time FromKe(double value)
        {
            return new Time(value * KeFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in minutes.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in minutes.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in minutes.</returns>
        public static Time FromMinutes(double value)
        {
            return new Time(value * SecondsPerMinute);
        }

        /// <summary>
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in hours.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in hours.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in hours.</returns>
        public static Time FromHours(double value)
        {
            return new Time(value * SecondsPerHour);
        }

        /// <summary>
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in days.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in days.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in days.</returns>
        public static Time FromDays(double value)
        {
            return new Time(value * SecondsPerDay);
        }

        /// <summary>
        /// Creates a new <see cref="Time"/> value from the specified <paramref name="value"/> in weeks.
        /// </summary>
        /// <param name="value">New <see cref="Time"/> value in weeks.</param>
        /// <returns>New <see cref="Time"/> object from the specified <paramref name="value"/> in weeks.</returns>
        public static Time FromWeeks(double value)
        {
            return new Time(value * SecondsPerWeek);
        }

        /// <summary>
        /// Returns the number of seconds in the specified month and year.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month (a number ranging from 1 to 12).</param>
        /// <returns>
        /// The number of seconds, as a <see cref="Time"/>, in the month for the specified year.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Month is less than 1 or greater than 12. -or- year is less than 1 or greater than 9999.
        /// </exception>
        public static int SecondsPerMonth(int year, int month)
        {
            return DateTime.DaysInMonth(year, month) * SecondsPerDay;
        }

        /// <summary>
        /// Returns the number of seconds in the specified year.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>
        /// The number of seconds in the specified year.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Year is less than 1 or greater than 9999.
        /// </exception>
        public static long SecondsPerYear(int year)
        {
            long total = 0;

            for (int month = 1; month <= 12; month++)
            {
                total += SecondsPerMonth(year, month);
            }

            return total;
        }

        #endregion        
    }
}