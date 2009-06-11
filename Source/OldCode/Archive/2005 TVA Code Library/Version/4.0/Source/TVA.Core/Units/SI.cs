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
using System.Text;

namespace TVA.Units
{
    /// <summary>
    /// Defines constant factors for SI units of measure to handle metric conversions.
    /// </summary>
    public static class SI
    {
        // Unit factor SI names
        private static string[] m_names = new string[] { "yocto", "zepto", "atto", "femto", "pico", "nano", "micro", "milli", "centi", "deci", "deca", "hecto", "kilo", "mega", "giga", "tera", "peta", "exa", "zetta", "yotta" };

        // Unit factor SI symbols
        private static string[] m_symbols = new string[] { "y", "z", "a", "f", "p", "n", "µ", "m", "c", "d", "da", "h", "k", "M", "G", "T", "P", "E", "Z", "Y" };
        
        // Unit factor SI factors
        private static double[] m_factors = new double[] { Yocto, Zepto, Atto, Femto, Pico, Nano, Micro, Milli, Centi, Deci, Deca, Hecto, Kilo, Mega, Giga, Tera, Peta, Exa, Zetta, Yotta };

        /// <summary>
        /// SI prefix Y, 10^24
        /// </summary>
        public const double Yotta = 1.0e+24D;

        /// <summary>
        /// SI prefix Z, 10^21
        /// </summary>
        public const double Zetta = 1.0e+21D;

        /// <summary>
        /// SI prefix E, 10^18
        /// </summary>
        public const double Exa = 1.0e+18D;

        /// <summary>
        /// SI prefix P, 10^15
        /// </summary>
        public const double Peta = 1.0e+15D;

        /// <summary>
        /// SI prefix T, 10^12
        /// </summary>
        public const double Tera = 1.0e+12D;

        /// <summary>
        /// SI prefix G, 10^9
        /// </summary>
        public const double Giga = 1.0e+9D;

        /// <summary>
        /// SI prefix M, 10^6
        /// </summary>
        public const double Mega = 1.0e+6D;

        /// <summary>
        /// SI prefix k, 10^3
        /// </summary>
        public const double Kilo = 1.0e+3D;

        /// <summary>
        /// SI prefix h, 10^2
        /// </summary>
        public const double Hecto = 1.0e+2D;

        /// <summary>
        /// SI prefix da, 10^1
        /// </summary>
        public const double Deca = 1.0e+1D;

        /// <summary>
        /// SI prefix d, 10^-1
        /// </summary>
        public const double Deci = 1.0e-1D;

        /// <summary>
        /// SI prefix c, 10^-2
        /// </summary>
        public const double Centi = 1.0e-2D;

        /// <summary>
        /// SI prefix m, 10^-3
        /// </summary>
        public const double Milli = 1.0e-3D;

        /// <summary>
        /// SI prefix µ, 10^-6
        /// </summary>
        public const double Micro = 1.0e-6D;

        /// <summary>
        /// SI prefix n, 10^-9
        /// </summary>
        public const double Nano = 1.0e-9D;

        /// <summary>
        /// SI prefix p, 10^-12
        /// </summary>
        public const double Pico = 1.0e-12D;

        /// <summary>
        /// SI prefix f, 10^-15
        /// </summary>
        public const double Femto = 1.0e-15D;

        /// <summary>
        /// SI prefix a, 10^-18
        /// </summary>
        public const double Atto = 1.0e-18D;

        /// <summary>
        /// SI prefix z, 10^-21
        /// </summary>
        public const double Zepto = 1.0e-21D;

        /// <summary>
        /// SI prefix y, 10^-24
        /// </summary>
        public const double Yocto = 1.0e-24D;

        /// <summary>
        /// Gets an array of all the defined unit factor SI names ordered from least (<see cref="Yocto"/>) to greatest (<see cref="Yotta"/>).
        /// </summary>
        public static string[] Names
        {
            get
            {
                return m_names;
            }
        }

        /// <summary>
        /// Gets an array of all the defined unit factor SI prefix symbols ordered from least (<see cref="Yocto"/>) to greatest (<see cref="Yotta"/>).
        /// </summary>
        public static string[] Symbols
        {
            get
            {
                return m_symbols;
            }
        }

        /// <summary>
        /// Gets an array of all the defined SI unit factors ordered from least (<see cref="Yocto"/>) to greatest (<see cref="Yotta"/>).
        /// </summary>
        public static double[] Factors
        {
            get
            {
                return m_factors;
            }
        }

        /// <summary>
        /// Turns the given number of units into a textual representation with an appropriate unit scaling.
        /// </summary>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        /// <remarks>
        /// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/> and
        /// three decimal places are used for displayed <paramref name="totalUnits"/> precision.
        /// </remarks>
        public static string ToScaledString(double totalUnits, string unitName)
        {
            return ToScaledString(totalUnits, 3, unitName);
        }

        /// <summary>
        /// Turns the given number of units into a textual representation with an appropriate unit scaling.
        /// </summary>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="format">A numeric string format for scaled <paramref name="totalUnits"/>.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        /// <remarks>
        /// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/>.
        /// </remarks>
        public static string ToScaledString(double totalUnits, string format, string unitName)
        {
            return ToScaledString(totalUnits, format, unitName, m_symbols);
        }

        /// <summary>
        /// Turns the given number of units into a textual representation with an appropriate unit scaling.
        /// </summary>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="decimalPlaces">Number of decimal places to display.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        /// <remarks>
        /// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalPlaces"/> cannot be negative.</exception>
        public static string ToScaledString(double totalUnits, int decimalPlaces, string unitName)
        {
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException("decimalPlaces", "decimalPlaces cannot be negative.");

            string format;

            if (decimalPlaces > 0)
                format = "0." + new string('0', decimalPlaces);
            else
                format = "0";

            return ToScaledString(totalUnits, format, unitName, m_symbols);
        }

        /// <summary>
        /// Turns the given number of units into a textual representation with an appropriate unit scaling
        /// given string array of factor names or symbols.
        /// </summary>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="format">A numeric string format for scaled <paramref name="totalUnits"/>.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        /// <param name="symbolNames">SI factor symbol or name array to use during textual conversion.</param>
        /// <remarks>
        /// The <paramref name="symbolNames"/> array needs one string entry for each defined SI item ordered from
        /// least (<see cref="Yocto"/>) to greatest (<see cref="Yotta"/>), see <see cref="Names"/> or <see cref="Symbols"/>
        /// arrays for examples.
        /// </remarks>
        public static string ToScaledString(double totalUnits, string format, string unitName, string[] symbolNames)
        {
            StringBuilder bytesImage = new StringBuilder();

            double factor;

            for (int i = m_factors.Length - 1; i >= 0; i--)
            {
                // See if total number of units ranges in the specified factor range
                factor = totalUnits / m_factors[i];

                if (factor >= 1.0D)
                {
                    bytesImage.Append(factor.ToString(format));
                    bytesImage.Append(' ');
                    bytesImage.Append(symbolNames[i]);
                    bytesImage.Append(unitName);
                    break;
                }
            }

            if (bytesImage.Length == 0)
            {
                // Display total number of units
                bytesImage.Append(totalUnits.ToString(format));
                bytesImage.Append(' ');
                bytesImage.Append(unitName);
            }

            return bytesImage.ToString();
        }
    }
}