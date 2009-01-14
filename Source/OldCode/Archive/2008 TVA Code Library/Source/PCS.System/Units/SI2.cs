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

using System.Text;

namespace System.Units
{
    /// <summary>
    /// Defines constant factors based on 1024 for computationally related binary SI units of measure.
    /// </summary>
    public static class SI2
    {
        // Common unit factor SI names
        private static string[] m_names = new string[] { "kilo", "mega", "giga", "tera", "peta", "exa" };

        // Common unit factor SI symbols
        private static string[] m_symbols = new string[] { "K", "M", "G", "T", "P", "E", };

        // IEC unit factor SI names
        private static string[] m_iecNames = new string[] { "kibi", "mebi", "gibi", "tebi", "pebi", "exbi" };

        // IEC unit factor SI symbols
        private static string[] m_iecSymbols = new string[] { "Ki", "Mi", "Gi", "Ti", "Pi", "Ei", };

        // Unit factor SI factors
        private static long[] m_factors = new long[] { Kilo, Mega, Giga, Tera, Peta, Exa };

        /// <summary>
        /// 1 exa, binary (E) = 1,152,921,504,606,846,976
        /// </summary>
        /// <remarks>
        /// This is the common name.
        /// </remarks>
        public const long Exa = 1024L * Peta;
        
        /// <summary>
        /// 1 exbi (Ei) = 1,152,921,504,606,846,976
        /// </summary>
        /// <remarks>
        /// This is the IEC standard name.
        /// </remarks>
        public const long Exbi = Exa;

        /// <summary>
        /// 1 peta, binary (P) = 1,125,899,906,842,624
        /// </summary>
        /// <remarks>
        /// This is the common name.
        /// </remarks>
        public const long Peta = 1024L * Tera;

        /// <summary>
        /// 1 pebi (Pi) = 1,125,899,906,842,624
        /// </summary>
        /// <remarks>
        /// This is the IEC standard name.
        /// </remarks>
        public const long Pebi = Peta;

        /// <summary>
        /// 1 tera, binary (T) = 1,099,511,627,776
        /// </summary>
        /// <remarks>
        /// This is the common name.
        /// </remarks>
        public const long Tera = 1024L * Giga;

        /// <summary>
        /// 1 tebi (Ti) = 1,099,511,627,776
        /// </summary>
        /// <remarks>
        /// This is the IEC standard name.
        /// </remarks>
        public const long Tebi = Tera;

        /// <summary>
        /// 1 giga, binary (G) = 1,073,741,824
        /// </summary>
        /// <remarks>
        /// This is the common name.
        /// </remarks>
        public const long Giga = 1024L * Mega;

        /// <summary>
        /// 1 gibi (Gi) = 1,073,741,824
        /// </summary>
        /// <remarks>
        /// This is the IEC standard name.
        /// </remarks>
        public const long Gibi = Giga;

        /// <summary>
        /// 1 mega, binary (M) = 1,048,576
        /// </summary>
        /// <remarks>
        /// This is the common name.
        /// </remarks>
        public const long Mega = 1024L * Kilo;

        /// <summary>
        /// 1 mebi (Mi) = 1,048,576
        /// </summary>
        /// <remarks>
        /// This is the IEC standard name.
        /// </remarks>
        public const long Mebi = Mega;

        /// <summary>
        /// 1 kilo, binary (K) = 1,024
        /// </summary>
        /// <remarks>
        /// This is the common name.
        /// </remarks>
        public const long Kilo = 1024L;

        /// <summary>
        /// 1 kibi (Ki) = 1,024
        /// </summary>
        /// <remarks>
        /// This is the IEC standard name.
        /// </remarks>
        public const long Kibi = Kilo;

        /// <summary>
        /// Gets an array of all the defined common binary unit factor SI names ordered from least (<see cref="Kilo"/>) to greatest (<see cref="Exa"/>).
        /// </summary>
        public static string[] Names
        {
            get
            {
                return m_names;
            }
        }

        /// <summary>
        /// Gets an array of all the defined common binary unit factor SI prefix symbols ordered from least (<see cref="Kilo"/>) to greatest (<see cref="Exa"/>).
        /// </summary>
        public static string[] Symbols
        {
            get
            {
                return m_symbols;
            }
        }

        /// <summary>
        /// Gets an array of all the defined IEC binary unit factor SI names ordered from least (<see cref="Kibi"/>) to greatest (<see cref="Exbi"/>).
        /// </summary>
        public static string[] IECNames
        {
            get
            {
                return m_iecNames;
            }
        }

        /// <summary>
        /// Gets an array of all the defined IEC binary unit factor SI prefix symbols ordered from least (<see cref="Kibi"/>) to greatest (<see cref="Exbi"/>).
        /// </summary>
        public static string[] IECSymbols
        {
            get
            {
                return m_iecSymbols;
            }
        }

        /// <summary>
        /// Gets an array of all the defined binary SI unit factors ordered from least (<see cref="Kilo"/>) to greatest (<see cref="Exa"/>).
        /// </summary>
        public static long[] Factors
        {
            get
            {
                return m_factors;
            }
        }

        /// <summary>
        /// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        /// and common named representation (e.g., KB, MB, GB, TB, etc.).
        /// </summary>
        /// <remarks>
        /// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/> and
        /// three decimal places are used for displayed <paramref name="totalUnits"/> precision.
        /// </remarks>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        public static string ToScaledString(long totalUnits, string unitName)
        {
            return ToScaledString(totalUnits, 3, unitName);
        }

        /// <summary>
        /// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        /// and common named representation (e.g., KB, MB, GB, TB, etc.).
        /// </summary>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="format">A numeric string format for scaled <paramref name="totalUnits"/>.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        /// <remarks>
        /// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/>.
        /// </remarks>
        public static string ToScaledString(long totalUnits, string format, string unitName)
        {
            return ToScaledString(totalUnits, format, unitName, m_symbols);
        }

        /// <summary>
        /// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        /// and common named representation (e.g., KB, MB, GB, TB, etc.).
        /// </summary>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="decimalPlaces">Number of decimal places to display.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        /// <remarks>
        /// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalPlaces"/> cannot be negative.</exception>
        public static string ToScaledString(long totalUnits, int decimalPlaces, string unitName)
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
        /// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        /// and IEC named representation (e.g., KiB, MiB, GiB, TiB, etc.).
        /// </summary>
        /// <remarks>
        /// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/> and
        /// three decimal places are used for displayed <paramref name="totalUnits"/> precision.
        /// </remarks>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        public static string ToScaledIECString(long totalUnits, string unitName)
        {
            return ToScaledIECString(totalUnits, 3, unitName);
        }

        /// <summary>
        /// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        /// and IEC named representation (e.g., KiB, MiB, GiB, TiB, etc.).
        /// </summary>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="format">A numeric string format for scaled <paramref name="totalUnits"/>.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        /// <remarks>
        /// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/>.
        /// </remarks>
        public static string ToScaledIECString(long totalUnits, string format, string unitName)
        {
            return ToScaledString(totalUnits, format, unitName, m_iecSymbols);
        }

        /// <summary>
        /// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        /// and IEC named representation (e.g., KiB, MiB, GiB, TiB, etc.).
        /// </summary>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="decimalPlaces">Number of decimal places to display.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        /// <remarks>
        /// <see cref="Symbols"/> array is used for displaying SI symbol prefix for <paramref name="unitName"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalPlaces"/> cannot be negative.</exception>
        public static string ToScaledIECString(long totalUnits, int decimalPlaces, string unitName)
        {
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException("decimalPlaces", "decimalPlaces cannot be negative.");

            string format;

            if (decimalPlaces > 0)
                format = "0." + new string('0', decimalPlaces);
            else
                format = "0";

            return ToScaledString(totalUnits, format, unitName, m_iecSymbols);
        }

        /// <summary>
        /// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate unit scaling
        /// given string array of factor names or symbols.
        /// </summary>
        /// <param name="totalUnits">Total units to represent textually.</param>
        /// <param name="format">A numeric string format for scaled <paramref name="totalUnits"/>.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "m/h" for meters per hour).</param>
        /// <param name="symbolNames">SI factor symbol or name array to use during textual conversion.</param>
        /// <remarks>
        /// The <paramref name="symbolNames"/> array needs one string entry for each defined SI item ordered from
        /// least (<see cref="Kilo"/>) to greatest (<see cref="Exa"/>), see <see cref="Names"/> or <see cref="Symbols"/>
        /// arrays for examples.
        /// </remarks>
        public static string ToScaledString(long totalUnits, string format, string unitName, string[] symbolNames)
        {
            StringBuilder bytesImage = new StringBuilder();

            double factor;

            for (int i = m_factors.Length - 1; i >= 0; i--)
            {
                // See if total number of units ranges in the specified factor range
                factor = totalUnits / (double)m_factors[i];

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
                bytesImage.Append(totalUnits);
                bytesImage.Append(' ');
                bytesImage.Append(unitName);
            }

            return bytesImage.ToString();
        }
    }
}
