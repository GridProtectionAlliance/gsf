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

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Units
{
    /// <summary>Represents an energy measurement, in joules (i.e., watt-seconds), as a double-precision floating-point number.</summary>
    /// <remarks>
    /// This class behaves just like a <see cref="Double"/> representing an energy in joules; it is implictly
    /// castable to and from a <see cref="Double"/> and therefore can be generally used "as" a double, but it
    /// has the advantage of handling conversions to and from other energy representations, specifically
    /// watt-hours, BTU, Celsius heat unit, liter-atmosphere, calorie, horsepower-hour, barrel of oil and ton of coal
    /// Metric conversions are handled simply by applying the needed <see cref="SI"/> conversion factor, for example:
    /// <example>
    /// Convert energy in joules to megajoules:
    /// <code>
    /// public double GetMegajoules(Energy joules)
    /// {
    ///     return joules / SI.Mega;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// This example converts megajoules to kilowatt-hours:
    /// <code>
    /// public double GetKilowattHours(double megajoules)
    /// {
    ///     return (new Energy(megajoules * SI.Mega)).ToWattHours() / SI.Kilo;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// This example converts kilowatt-hours to megawatt-hours:
    /// <code>
    /// public double GetMegawattHours(double kilowattHours)
    /// {
    ///     return (kilowattHours * SI.Kilo) / SI.Mega;
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [Serializable()]
    public struct Energy : IComparable, IFormattable, IConvertible, IComparable<Energy>, IComparable<Double>, IEquatable<Energy>, IEquatable<Double>
    {
        #region [ Members ]

        // Constants
        private const double WattHoursFactor = Time.SecondsPerHour; // 1 joule = 1 watt-second

        private const double BTUFactor = 1.05505585262E+3D;

        private const double CelsiusHeatUnitsFactor = 1.899100534716E+3D;

        private const double LitersAtmosphereFactor = 101.325D;

        private const double CaloriesFactor = 4.1868D;

        private const double HorsepowerHoursFactor = 2.684519537696172792E+6D;

        private const double BarrelsOfOilFactor = 6.12E+9D;

        private const double TonsOfCoalFactor = 29.3076E+9D;

        // Fields
        private double m_value; // Energy value stored in joules

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="Energy"/>.
        /// </summary>
        /// <param name="value">New energy value in joules.</param>
        public Energy(double value)
        {
            m_value = value;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the <see cref="Charge"/> value in coulombs given the specified <paramref name="volts"/>.
        /// </summary>
        /// <param name="volts">Source <see cref="Voltage"/> used to calculate <see cref="Charge"/> value.</param>
        /// <returns><see cref="Charge"/> value in coulombs given the specified <paramref name="volts"/>.</returns>
        public Charge ToCoulombs(Voltage volts)
        {
            return m_value / (double)volts;
        }

        /// <summary>
        /// Gets the <see cref="Energy"/> value in watt-hours.
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in watt-hours.</returns>
        public double ToWattHours()
        {
            return m_value / WattHoursFactor;
        }

        /// <summary>
        /// Gets the <see cref="Energy"/> value in BTU (International Table).
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in BTU.</returns>
        public double ToBTU()
        {
            return m_value / BTUFactor;
        }

        /// <summary>
        /// Gets the <see cref="Energy"/> value in Celsius heat units (International Table).
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in Celsius heat units.</returns>
        public double ToCelsiusHeatUnits()
        {
            return m_value / CelsiusHeatUnitsFactor;
        }

        /// <summary>
        /// Gets the <see cref="Energy"/> value in liters-atmosphere.
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in liters-atmosphere.</returns>
        public double ToLitersAtmosphere()
        {
            return m_value / LitersAtmosphereFactor;
        }

        /// <summary>
        /// Gets the <see cref="Energy"/> value in calories (International Table).
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in calories.</returns>
        public double ToCalories()
        {
            return m_value / CaloriesFactor;
        }

        /// <summary>
        /// Gets the <see cref="Energy"/> value in horsepower-hours.
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in horsepower-hours.</returns>
        public double ToHorsepowerHours()
        {
            return m_value / HorsepowerHoursFactor;
        }

        /// <summary>
        /// Gets the <see cref="Energy"/> value in equivalent barrels of oil.
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in equivalent barrels of oil.</returns>
        public double ToBarrelsOfOil()
        {
            return m_value / BarrelsOfOilFactor;
        }

        /// <summary>
        /// Gets the <see cref="Energy"/> value in equivalent tons of coal.
        /// </summary>
        /// <returns>Value of <see cref="Energy"/> in equivalent tons of coal.</returns>
        public double ToTonsOfCoal()
        {
            return m_value / TonsOfCoalFactor;
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
        /// <exception cref="ArgumentException">value is not a <see cref="Double"/> or <see cref="Energy"/>.</exception>
        public int CompareTo(object value)
        {
            if (value == null) return 1;

            if (!(value is double) && !(value is Energy))
                throw new ArgumentException("Argument must be a Double or an Energy");

            double num = (double)value;
            return (m_value < num ? -1 : (m_value > num ? 1 : 0));
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Energy"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">An <see cref="Energy"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(Energy value)
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
        /// True if obj is an instance of <see cref="Double"/> or <see cref="Energy"/> and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is double || obj is Energy)
                return Equals((double)obj);

            return false;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Energy"/> value.
        /// </summary>
        /// <param name="obj">An <see cref="Energy"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(Energy obj)
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
        /// Converts the string representation of a number to its <see cref="Energy"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <returns>
        /// An <see cref="Energy"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Energy.MinValue"/> or greater than <see cref="Energy.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        public static Energy Parse(string s)
        {
            return (Energy)double.Parse(s);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style to its <see cref="Energy"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <returns>
        /// An <see cref="Energy"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Energy.MinValue"/> or greater than <see cref="Energy.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        public static Energy Parse(string s, NumberStyles style)
        {
            return (Energy)double.Parse(s, style);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified culture-specific format to its <see cref="Energy"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// An <see cref="Energy"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Energy.MinValue"/> or greater than <see cref="Energy.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        public static Energy Parse(string s, IFormatProvider provider)
        {
            return (Energy)double.Parse(s, provider);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its <see cref="Energy"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// An <see cref="Energy"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Energy.MinValue"/> or greater than <see cref="Energy.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        public static Energy Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            return (Energy)double.Parse(s, style, provider);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Energy"/> equivalent. A return value
        /// indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Energy"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parajoule is null,
        /// is not of the correct format, or represents a number less than <see cref="Energy.MinValue"/> or greater than <see cref="Energy.MaxValue"/>.
        /// This parajoule is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out Energy result)
        {
            double parseResult;
            bool parseResponse;

            parseResponse = double.TryParse(s, out parseResult);
            result = parseResult;

            return parseResponse;
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its
        /// <see cref="Energy"/> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Energy"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parajoule is null,
        /// is not in a format compliant with style, or represents a number less than <see cref="Energy.MinValue"/> or
        /// greater than <see cref="Energy.MaxValue"/>. This parajoule is passed uninitialized.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> object that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Energy result)
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
        /// Implicitly converts value, represented in joules, to an <see cref="Energy"/>.
        /// </summary>
        public static implicit operator Energy(Double value)
        {
            return new Energy(value);
        }

        /// <summary>
        /// Implicitly converts <see cref="Energy"/>, represented in joules, to a <see cref="Double"/>.
        /// </summary>
        public static implicit operator Double(Energy value)
        {
            return value.m_value;
        }

        #endregion

        #region [ Arithmetic Operators ]

        /// <summary>
        /// Returns computed remainder after dividing first value by the second.
        /// </summary>
        public static Energy operator %(Energy value1, Energy value2)
        {
            return value1.m_value % value2.m_value;
        }

        /// <summary>
        /// Returns computed sum of values.
        /// </summary>
        public static Energy operator +(Energy value1, Energy value2)
        {
            return value1.m_value + value2.m_value;
        }

        /// <summary>
        /// Returns computed difference of values.
        /// </summary>
        public static Energy operator -(Energy value1, Energy value2)
        {
            return value1.m_value - value2.m_value;
        }

        /// <summary>
        /// Returns computed product of values.
        /// </summary>
        public static Energy operator *(Energy value1, Energy value2)
        {
            return value1.m_value * value2.m_value;
        }

        /// <summary>
        /// Returns computed division of values.
        /// </summary>
        public static Energy operator /(Energy value1, Energy value2)
        {
            return value1.m_value / value2.m_value;
        }

        // C# doesn't expose an exponent operator but some other .NET languages do,
        // so we expose the operator via its native special IL function name

        /// <summary>
        /// Returns result of first value raised to power of second value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced), SpecialName()]
        public static double op_Exponent(Energy value1, Energy value2)
        {
            return Math.Pow((double)value1.m_value, (double)value2.m_value);
        }

        #endregion

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>Represents the largest possible value of an <see cref="Energy"/>. This field is constant.</summary>
        public static readonly Energy MaxValue;

        /// <summary>Represents the smallest possible value of an <see cref="Energy"/>. This field is constant.</summary>
        public static readonly Energy MinValue;

        // Static Constructor
        static Energy()
        {
            MaxValue = (Energy)double.MaxValue;
            MinValue = (Energy)double.MinValue;
        }

        // Static Methods

        /// <summary>
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in watt-hours.
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in watt-hours.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in watt-hours.</returns>
        public static Energy FromWattHours(double value)
        {
            return new Energy(value * WattHoursFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in BTU (International Table).
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in BTU.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in BTU.</returns>
        public static Energy FromBTU(double value)
        {
            return new Energy(value * BTUFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in Celsius heat units (International Table).
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in Celsius heat units.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in Celsius heat units.</returns>
        public static Energy FromCelsiusHeatUnits(double value)
        {
            return new Energy(value * CelsiusHeatUnitsFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in liters-atmosphere.
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in liters-atmosphere.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in liters-atmosphere.</returns>
        public static Energy FromLitersAtmosphere(double value)
        {
            return new Energy(value * LitersAtmosphereFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in calories (International Table).
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in calories.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in calories.</returns>
        public static Energy FromCalories(double value)
        {
            return new Energy(value * CaloriesFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in horsepower-hours.
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in horsepower-hours.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in horsepower-hours.</returns>
        public static Energy FromHorsepowerHours(double value)
        {
            return new Energy(value * HorsepowerHoursFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in equivalent barrels of oil.
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in equivalent barrels of oil.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in equivalent barrels of oil.</returns>
        public static Energy FromBarrelsOfOil(double value)
        {
            return new Energy(value * BarrelsOfOilFactor);
        }

        /// <summary>
        /// Creates a new <see cref="Energy"/> value from the specified <paramref name="value"/> in equivalent tons of coal.
        /// </summary>
        /// <param name="value">New <see cref="Energy"/> value in equivalent tons of coal.</param>
        /// <returns>New <see cref="Energy"/> object from the specified <paramref name="value"/> in equivalent tons of coal.</returns>
        public static Energy FromTonOfCoal(double value)
        {
            return new Energy(value * TonsOfCoalFactor);
        }

        #endregion        
    }
}