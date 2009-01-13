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
    /// Defines constant factors based on 1024 for computationally related binary SI prefixes.
    /// </summary>
    public static class SI2
    {
        private struct Factor
        {
            // Note that this is a structure so elements may be used as an index in
            // a string array without having to cast as (int)
            static readonly public int Kilo = 0;
            static readonly public int Mega = 1;
            static readonly public int Giga = 2;
            static readonly public int Tera = 3;
            static readonly public int Peta = 4;
            static readonly public int Exa = 5;
        }

        // Common factor prefix names used by GetBestFit function
        private static string[] m_commonPrefixes = new string[] { "K", "M", "G", "T", "P", "E" };

        // IEC factor prefix names used by GetBestFit function
        private static string[] m_iecPrefixes = new string[] { "Ki", "Mi", "Gi", "Ti", "Pi", "Ei" };

        /// <summary>
        /// 1 exabinary (E) = 1,152,921,504,606,846,976
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
        /// 1 petabinary (P) = 1,125,899,906,842,624
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
        /// 1 terabinary (T) = 1,099,511,627,776
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
        /// 1 gigabinary (G) = 1,073,741,824
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
        /// 1 megabinary (M) = 1,048,576
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
        /// 1 kilobinary (K) = 1,024
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
        /// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate
        /// binary unit scaling and common named representation (e.g., KB, MB, GB, TB, etc.).
        /// </summary>
        /// <param name="totalUnits">Total units (e.g., bytes) to represent textually.</param>
        /// <param name="decimalPlaces">Number of decimal places to display.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "B" for byte).</param>
        public static string ToScaledString(long totalUnits, int decimalPlaces, string unitName)
        {
            return ToScaledString(totalUnits, decimalPlaces, unitName, m_commonPrefixes);
        }

        /// <summary>
        /// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate
        /// binary unit scaling and an IEC named representation (e.g., KiB, MiB, GiB, TiB, etc.).
        /// </summary>
        /// <param name="totalUnits">Total units (e.g., bytes) to represent textually.</param>
        /// <param name="decimalPlaces">Number of decimal places to display.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "B" for byte).</param>
        public static string ToScaledIECString(long totalUnits, int decimalPlaces, string unitName)
        {
            return ToScaledString(totalUnits, decimalPlaces, unitName, m_commonPrefixes);
        }

        /// <summary>
        /// Turns the given number of units (e.g., bytes) into a textual representation with an appropriate
        /// binary unit scaling given string array of factor prefix names.
        /// </summary>
        /// <param name="totalUnits">Total units (e.g., bytes) to represent textually.</param>
        /// <param name="decimalPlaces">Number of decimal places to display.</param>
        /// <param name="unitName">Name of unit display (e.g., you could use "B" for byte).</param>
        /// <param name="prefixNames">SI factor prefix name array to use during textual conversion.</param>
        /// <remarks>
        /// SI factor <paramref name="prefixNames"/> array needs one string entry for each of the following names:<br/>
        /// "kilo", "mega", "giga", "tera", "peta", "exa".
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalPlaces"/> cannot be negative.</exception>
        public static string ToScaledString(long totalUnits, int decimalPlaces, string unitName, string[] prefixNames)
        {
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException("decimalPlaces", "decimalPlaces cannot be negative.");

            StringBuilder bytesImage = new StringBuilder();

            // See if total number of units ranges in exaunits
            double factor = totalUnits / (double)SI2.Exa;

            if (factor >= 1.0D)
            {
                bytesImage.Append(factor.ToString(Format(decimalPlaces)));
                bytesImage.Append(' ');
                bytesImage.Append(prefixNames[Factor.Exa]);
                bytesImage.Append(unitName);
            }
            else
            {
                // See if total number of units ranges in petaunits
                factor = totalUnits / (double)SI2.Peta;

                if (factor >= 1.0D)
                {
                    bytesImage.Append(factor.ToString(Format(decimalPlaces)));
                    bytesImage.Append(' ');
                    bytesImage.Append(prefixNames[Factor.Peta]);
                    bytesImage.Append(unitName);
                }
                else
                {
                    // See if total number of units ranges in teraunits
                    factor = totalUnits / (double)SI2.Tera;

                    if (factor >= 1.0D)
                    {
                        bytesImage.Append(factor.ToString(Format(decimalPlaces)));
                        bytesImage.Append(' ');
                        bytesImage.Append(prefixNames[Factor.Tera]);
                        bytesImage.Append(unitName);
                    }
                    else
                    {
                        // See if total number of units ranges in gigaunits
                        factor = totalUnits / (double)SI2.Giga;

                        if (factor >= 1.0D)
                        {
                            bytesImage.Append(factor.ToString(Format(decimalPlaces)));
                            bytesImage.Append(' ');
                            bytesImage.Append(prefixNames[Factor.Giga]);
                            bytesImage.Append(unitName);
                        }
                        else
                        {
                            // See if total number of units ranges in megaunits
                            factor = totalUnits / (double)SI2.Mega;

                            if (factor >= 1.0D)
                            {
                                bytesImage.Append(factor.ToString(Format(decimalPlaces)));
                                bytesImage.Append(' ');
                                bytesImage.Append(prefixNames[Factor.Mega]);
                                bytesImage.Append(unitName);
                            }
                            else
                            {
                                // See if total number of units ranges in kilounits
                                factor = totalUnits / (double)SI2.Kilo;

                                if (factor >= 1.0D)
                                {
                                    bytesImage.Append(factor.ToString(Format(decimalPlaces)));
                                    bytesImage.Append(' ');
                                    bytesImage.Append(prefixNames[Factor.Kilo]);
                                    bytesImage.Append(unitName);
                                }
                                else
                                {
                                    // Display total number of units
                                    bytesImage.Append(totalUnits);
                                    bytesImage.Append(' ');
                                    bytesImage.Append(unitName);
                                }
                            }
                        }
                    }
                }
            }

            return bytesImage.ToString();
        }

        private static string Format(int decimalPlaces)
        {
            if (decimalPlaces > 0)
                return "0." + new string('0', decimalPlaces);
            else
                return "0";
        }
    }
}
