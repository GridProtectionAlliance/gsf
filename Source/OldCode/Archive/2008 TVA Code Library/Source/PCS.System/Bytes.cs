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

namespace System
{
    /// <summary>
    /// Defines constants and functions related to bytes for related SI prefixes based on 1024.
    /// </summary>
    public static class Bytes
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
            static readonly public int Bytes = 6;
        }

        // Standard factor suffix names used by ToText function
        private static string[] m_standardFactorSuffixes = new string[] { "kB", "MB", "GB", "TB", "PB", "EB", "bytes" };

        /// <summary>
        /// 1 exabyte (EB) = 1,152,921,504,606,846,976 bytes
        /// </summary>
        public const long PerExabyte = 1024L * PerPetabyte;

        /// <summary>
        /// 1 petabyte (PB) = 1,125,899,906,842,624 bytes
        /// </summary>
        public const long PerPetabyte = 1024L * PerTerabyte;

        /// <summary>
        /// 1 terabyte (TB) = 1,099,511,627,776 bytes
        /// </summary>
        public const long PerTerabyte = 1024L * PerGigabyte;

        /// <summary>
        /// 1 gigabyte (GB) = 1,073,741,824 bytes
        /// </summary>
        public const long PerGigabyte = 1024L * PerMegabyte;

        /// <summary>
        /// 1 megabyte (MB) = 1,048,576 bytes
        /// </summary>
        public const long PerMegabyte = 1024L * PerKilobyte;

        /// <summary>
        /// 1 kilobyte (kB) = 1,024 bytes
        /// </summary>
        public const long PerKilobyte = 1024L;

        /// <summary>
        /// Turns the given number of bytes into a textual representation with an appropriate
        /// SI scaling representation (e.g., kB, GB, TB, etc.).
        /// </summary>
        /// <param name="totalBytes">Total bytes to represent textually.</param>
        /// <param name="decimalPlaces">Number of decimal places to display.</param>
        public static string ToText(long totalBytes, int decimalPlaces)
        {
            return ToText(totalBytes, decimalPlaces, m_standardFactorSuffixes);
        }

        /// <summary>
        /// Turns the given number of bytes into a textual representation with an appropriate
        /// SI scaling representation given string array of factor suffix names.
        /// </summary>
        /// <param name="totalBytes">Total bytes to represent textually.</param>
        /// <param name="decimalPlaces">Number of decimal places to display.</param>
        /// <param name="suffixNames">Factor suffix name array to use during textual conversion.</param>
        /// <remarks>
        /// Factor <paramref name="suffixNames"/> array needs one string entry for each of the following names:<br/>
        /// "kilobyte", "megabyte", "gigabyte", "terabyte", "petabyte", "exabyte", "bytes".
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalPlaces"/> cannot be negative.</exception>
        public static string ToText(long totalBytes, int decimalPlaces, string[] suffixNames)
        {
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException("decimalPlaces", "decimalPlaces cannot be negative.");

            StringBuilder bytesImage = new StringBuilder();

            // See if total number of bytes ranges in exabytes
            double factor = totalBytes / (double)Bytes.PerExabyte;

            if (factor >= 1.0D)
            {
                bytesImage.Append(factor.ToString(Format(decimalPlaces)));
                bytesImage.Append(' ');
                bytesImage.Append(suffixNames[Factor.Exa]);
            }
            else
            {
                // See if total number of bytes ranges in petabytes
                factor = totalBytes / (double)Bytes.PerPetabyte;

                if (factor >= 1.0D)
                {
                    bytesImage.Append(factor.ToString(Format(decimalPlaces)));
                    bytesImage.Append(' ');
                    bytesImage.Append(suffixNames[Factor.Peta]);
                }
                else
                {
                    // See if total number of bytes ranges in terabytes
                    factor = totalBytes / (double)Bytes.PerTerabyte;

                    if (factor >= 1.0D)
                    {
                        bytesImage.Append(factor.ToString(Format(decimalPlaces)));
                        bytesImage.Append(' ');
                        bytesImage.Append(suffixNames[Factor.Tera]);
                    }
                    else
                    {
                        // See if total number of bytes ranges in gigabytes
                        factor = totalBytes / (double)Bytes.PerGigabyte;

                        if (factor >= 1.0D)
                        {
                            bytesImage.Append(factor.ToString(Format(decimalPlaces)));
                            bytesImage.Append(' ');
                            bytesImage.Append(suffixNames[Factor.Giga]);
                        }
                        else
                        {
                            // See if total number of bytes ranges in megabytes
                            factor = totalBytes / (double)Bytes.PerMegabyte;

                            if (factor >= 1.0D)
                            {
                                bytesImage.Append(factor.ToString(Format(decimalPlaces)));
                                bytesImage.Append(' ');
                                bytesImage.Append(suffixNames[Factor.Mega]);
                            }
                            else
                            {
                                // See if total number of bytes ranges in kilobytes
                                factor = totalBytes / (double)Bytes.PerKilobyte;

                                if (factor >= 1.0D)
                                {
                                    bytesImage.Append(factor.ToString(Format(decimalPlaces)));
                                    bytesImage.Append(' ');
                                    bytesImage.Append(suffixNames[Factor.Kilo]);
                                }
                                else
                                {
                                    // Display total number of bytes
                                    bytesImage.Append(totalBytes);
                                    bytesImage.Append(' ');
                                    bytesImage.Append(suffixNames[Factor.Bytes]);
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
