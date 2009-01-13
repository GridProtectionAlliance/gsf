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
    /// <summary>Represents a temperature, in kelvin, as a double-precision floating-point number.</summary>
    /// <remarks>
    /// This class behaves just like a <see cref="Double"/> representing a temperature in kelvin; it is implictly
    /// castable to and from a <see cref="Double"/> and therefore can be generally used "as" a double, but it
    /// has the advantage of handling conversions to and from other temperature representations, specifically
    /// Celsius, Fahrenheit, Newton, Rankine, Delisle, Réaumur and Rømer.
    /// <example>
    /// This example converts Celsius to Fahrenheit:
    /// <code>
    /// public double GetFahrenheit(double celsius)
    /// {
    ///     return Temperature.FromCelsius(celsius).ToFahrenheit();
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [Serializable()]
    public struct Temperature : IComparable, IFormattable, IConvertible, IComparable<Temperature>, IComparable<Double>, IEquatable<Temperature>, IEquatable<Double>
    {
        #region [ Members ]

        // Constants
        private const double CelsiusFactor = 1.0D;
        private const double CelsiusOffset = 273.15D;
        private const double CelsiusStep = 0.0D;

        private const double FahrenheitFactor = 5.0D / 9.0D;
        private const double FahrenheitOffset = 0.0D;
        private const double FahrenheitStep = 459.67D;

        private const double NewtonFactor = 100.0D / 33.0D;
        private const double NewtonOffset = CelsiusOffset;
        private const double NewtonStep = 0.0D;

        private const double RankineFactor = FahrenheitFactor;
        private const double RankineOffset = 0.0D;
        private const double RankineStep = 0.0D;

        private const double DelisleFactor = -2.0D / 3.0D;
        private const double DelisleOffset = 373.15D;
        private const double DelisleStep = 0.0D;

        private const double RéaumurFactor = 5.0D / 4.0D;
        private const double RéaumurOffset = CelsiusOffset;
        private const double RéaumurStep = 0.0D;

        private const double RømerFactor = 40.0D / 21.0D;
        private const double RømerOffset = CelsiusOffset;
        private const double RømerStep = -7.5D;

        // Fields
        private double m_value; // Temperature value stored in kelvin

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="Temperature"/>.
        /// </summary>
        /// <param name="value">New temperature value in kelvin.</param>
        public Temperature(double value)
        {
            m_value = value;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the <see cref="Temperature"/> value in Celsius.
        /// </summary>
        /// <returns>Value of <see cref="Temperature"/> in Celsius.</returns>
        public double ToCelsius()
        {
            return ToTemperature(CelsiusFactor, CelsiusOffset, CelsiusStep);
        }

        /// <summary>
        /// Gets the <see cref="Temperature"/> value in Fahrenheit.
        /// </summary>
        /// <returns>Value of <see cref="Temperature"/> in Fahrenheit.</returns>
        public double ToFahrenheit()
        {
            return ToTemperature(FahrenheitFactor, FahrenheitOffset, FahrenheitStep);
        }

        /// <summary>
        /// Gets the <see cref="Temperature"/> value in Newton.
        /// </summary>
        /// <returns>Value of <see cref="Temperature"/> in Newton.</returns>
        public double ToNewton()
        {
            return ToTemperature(NewtonFactor, NewtonOffset, NewtonStep);
        }

        /// <summary>
        /// Gets the <see cref="Temperature"/> value in Rankine.
        /// </summary>
        /// <returns>Value of <see cref="Temperature"/> in Rankine.</returns>
        public double ToRankine()
        {
            return ToTemperature(RankineFactor, RankineOffset, RankineStep);
        }

        /// <summary>
        /// Gets the <see cref="Temperature"/> value in Delisle.
        /// </summary>
        /// <returns>Value of <see cref="Temperature"/> in Delisle.</returns>
        public double ToDelisle()
        {
            return ToTemperature(DelisleFactor, DelisleOffset, DelisleStep);
        }

        /// <summary>
        /// Gets the <see cref="Temperature"/> value in Réaumur.
        /// </summary>
        /// <returns>Value of <see cref="Temperature"/> in Réaumur.</returns>
        public double ToRéaumur()
        {
            return ToTemperature(RéaumurFactor, RéaumurOffset, RéaumurStep);
        }

        /// <summary>
        /// Gets the <see cref="Temperature"/> value in Rømer.
        /// </summary>
        /// <returns>Value of <see cref="Temperature"/> in Rømer.</returns>
        public double ToRømer()
        {
            return ToTemperature(RømerFactor, RømerOffset, RømerStep);
        }

        // Calculate temperature based on value = (K - offset) / factor - step
        private Temperature ToTemperature(double factor, double offset, double step)
        {
            return (m_value - offset) / factor - step;
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
        /// <exception cref="ArgumentException">value is not a <see cref="Double"/> or <see cref="Temperature"/>.</exception>
        public int CompareTo(object value)
        {
            if (value == null) return 1;

            if (!(value is double) && !(value is Temperature))
                throw new ArgumentException("Argument must be a Double or a Temperature");

            double num = (double)value;
            return (m_value < num ? -1 : (m_value > num ? 1 : 0));
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Temperature"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="Temperature"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(Temperature value)
        {
            return CompareTo((double)value);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Double"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="Double"/> to compare.</param>
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
        /// True if obj is an instance of <see cref="Double"/> or <see cref="Temperature"/> and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is double || obj is Temperature)
                return Equals((double)obj);

            return false;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Temperature"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="Temperature"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(Temperature obj)
        {
            return Equals((double)obj);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Double"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="Double"/> value to compare to this instance.</param>
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
        /// Converts the string representation of a number to its <see cref="Temperature"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <returns>
        /// A <see cref="Temperature"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Temperature.MinValue"/> or greater than <see cref="Temperature.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        public static Temperature Parse(string s)
        {
            return (Temperature)double.Parse(s);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style to its <see cref="Temperature"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <returns>
        /// A <see cref="Temperature"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Temperature.MinValue"/> or greater than <see cref="Temperature.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        public static Temperature Parse(string s, NumberStyles style)
        {
            return (Temperature)double.Parse(s, style);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified culture-specific format to its <see cref="Temperature"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Temperature"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Temperature.MinValue"/> or greater than <see cref="Temperature.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        public static Temperature Parse(string s, IFormatProvider provider)
        {
            return (Temperature)double.Parse(s, provider);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its <see cref="Temperature"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Temperature"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Temperature.MinValue"/> or greater than <see cref="Temperature.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        public static Temperature Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            return (Temperature)double.Parse(s, style, provider);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Temperature"/> equivalent. A return value
        /// indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Temperature"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
        /// is not of the correct format, or represents a number less than <see cref="Temperature.MinValue"/> or greater than <see cref="Temperature.MaxValue"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out Temperature result)
        {
            double parseResult;
            bool parseResponse;

            parseResponse = double.TryParse(s, out parseResult);
            result = parseResult;

            return parseResponse;
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its
        /// <see cref="Temperature"/> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Temperature"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
        /// is not in a format compliant with style, or represents a number less than <see cref="Temperature.MinValue"/> or
        /// greater than <see cref="Temperature.MaxValue"/>. This parameter is passed uninitialized.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> object that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Temperature result)
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

        #region [ Type Conversion Operators ]

        /// <summary>
        /// Implicitly converts value, represented in kelvin, to a <see cref="Temperature"/>.
        /// </summary>
        public static implicit operator Temperature(Double value)
        {
            return new Temperature(value);
        }

        /// <summary>
        /// Implicitly converts <see cref="Temperature"/>, represented in kelvin, to a <see cref="Double"/>.
        /// </summary>
        public static implicit operator Double(Temperature value)
        {
            return value.m_value;
        }

        #endregion

        #region [ Arithmetic Operators ]

        /// <summary>
        /// Returns computed remainder after dividing first value by the second.
        /// </summary>
        public static Temperature operator %(Temperature value1, Temperature value2)
        {
            return value1.m_value % value2.m_value;
        }

        /// <summary>
        /// Returns computed sum of values.
        /// </summary>
        public static Temperature operator +(Temperature value1, Temperature value2)
        {
            return value1.m_value + value2.m_value;
        }

        /// <summary>
        /// Returns computed difference of values.
        /// </summary>
        public static Temperature operator -(Temperature value1, Temperature value2)
        {
            return value1.m_value - value2.m_value;
        }

        /// <summary>
        /// Returns computed product of values.
        /// </summary>
        public static Temperature operator *(Temperature value1, Temperature value2)
        {
            return value1.m_value * value2.m_value;
        }

        // Integer division operators

        /// <summary>
        /// Returns computed division of values.
        /// </summary>
        public static Temperature operator /(Temperature value1, Temperature value2)
        {
            return value1.m_value / value2.m_value;
        }

        // C# doesn't expose an exponent operator but some other .NET languages do,
        // so we expose the operator via its native special IL function name

        /// <summary>
        /// Returns result of first value raised to power of second value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced), SpecialName()]
        public static double op_Exponent(Temperature value1, Temperature value2)
        {
            return Math.Pow((double)value1.m_value, (double)value2.m_value);
        }

        #endregion

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>Represents the largest possible value of a <see cref="Temperature"/>. This field is constant.</summary>
        public static readonly Temperature MaxValue;

        /// <summary>Represents the smallest possible value of a <see cref="Temperature"/>. This field is constant.</summary>
        public static readonly Temperature MinValue;

        // Static Constructor
        static Temperature()
        {
            MaxValue = (Temperature)double.MaxValue;
            MinValue = (Temperature)double.MinValue;
        }

        // Static Methods
        
        /// <summary>
        /// Creates a new <see cref="Temperature"/> from the specified <paramref name="value"/> in Celsius.
        /// </summary>
        /// <param name="value">New <see cref="Temperature"/> value in Celsius.</param>
        /// <returns>New <see cref="Temperature"/> object from the specified <paramref name="value"/> in Celsius.</returns>
        public static Temperature FromCelsius(double value)
        {
            return FromTemperature(value, CelsiusFactor, CelsiusOffset, CelsiusStep);
        }
        
        /// <summary>
        /// Creates a new <see cref="Temperature"/> from the specified <paramref name="value"/> in Fahrenheit.
        /// </summary>
        /// <param name="value">New <see cref="Temperature"/> value in Fahrenheit.</param>
        /// <returns>New <see cref="Temperature"/> object from the specified <paramref name="value"/> in Fahrenheit.</returns>
        public static Temperature FromFahrenheit(double value)
        {
            return FromTemperature(value, FahrenheitFactor, FahrenheitOffset, FahrenheitStep);
        }

        /// <summary>
        /// Creates a new <see cref="Temperature"/> from the specified <paramref name="value"/> in Newton.
        /// </summary>
        /// <param name="value">New <see cref="Temperature"/> value in Newton.</param>
        /// <returns>New <see cref="Temperature"/> object from the specified <paramref name="value"/> in Newton.</returns>
        public static Temperature FromNewton(double value)
        {
            return FromTemperature(value, NewtonFactor, NewtonOffset, NewtonStep);
        }

        /// <summary>
        /// Creates a new <see cref="Temperature"/> from the specified <paramref name="value"/> in Rankine.
        /// </summary>
        /// <param name="value">New <see cref="Temperature"/> value in Rankine.</param>
        /// <returns>New <see cref="Temperature"/> object from the specified <paramref name="value"/> in Rankine.</returns>
        public static Temperature FromRankine(double value)
        {
            return FromTemperature(value, RankineFactor, RankineOffset, RankineStep);
        }

        /// <summary>
        /// Creates a new <see cref="Temperature"/> from the specified <paramref name="value"/> in Delisle.
        /// </summary>
        /// <param name="value">New <see cref="Temperature"/> value in Delisle.</param>
        /// <returns>New <see cref="Temperature"/> object from the specified <paramref name="value"/> in Delisle.</returns>
        public static Temperature FromDelisle(double value)
        {
            return FromTemperature(value, DelisleFactor, DelisleOffset, DelisleStep);
        }

        /// <summary>
        /// Creates a new <see cref="Temperature"/> from the specified <paramref name="value"/> in Réaumur.
        /// </summary>
        /// <param name="value">New <see cref="Temperature"/> value in Réaumur.</param>
        /// <returns>New <see cref="Temperature"/> object from the specified <paramref name="value"/> in Réaumur.</returns>
        public static Temperature FromRéaumur(double value)
        {
            return FromTemperature(value, RéaumurFactor, RéaumurOffset, RéaumurStep);
        }

        /// <summary>
        /// Creates a new <see cref="Temperature"/> from the specified <paramref name="value"/> in Rømer.
        /// </summary>
        /// <param name="value">New <see cref="Temperature"/> value in Rømer.</param>
        /// <returns>New <see cref="Temperature"/> object from the specified <paramref name="value"/> in Rømer.</returns>
        public static Temperature FromRømer(double value)
        {
            return FromTemperature(value, RømerFactor, RømerOffset, RømerStep);
        }

        // Calculate temperature based on K = (value + step) * factor + offset
        private static Temperature FromTemperature(double value, double factor, double offset, double step)
        {
            return new Temperature((value + step) * factor + offset);
        }

        #endregion        
    }
}
