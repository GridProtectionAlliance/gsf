//******************************************************************************************************
//  Random.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/04/2006 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/17/2008 - J. Ritchie Carroll
//       Converted to C#.
//  02/16/2009 - Josh L. Patterson
//       Edited Code Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  11/17/2016 - Steven E. Chisholm
//       Fixed the crypto random nature of numbers when a range is specified.
//
//******************************************************************************************************

using System;
using System.Security.Cryptography;

namespace GSF.Security.Cryptography
{
    /// <summary>
    /// Generates cryptographically strong random numbers.
    /// </summary>
    public static class Random
    {
        private static readonly RNGCryptoServiceProvider RandomNumberGenerator = new RNGCryptoServiceProvider();

        /// <summary>
        /// Generates a semi cryptographically strong double-precision floating-point random number between zero and one. i.e. [0-1)
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static double Number
        {
            get
            {
                unchecked
                {
                    return UInt32 / (uint.MaxValue + 1.0D);
                }
            }
        }

        /// <summary>
        /// Generates a semi cryptographically strong random decimal between zero and one. i.e. [0-1)
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static decimal Decimal
        {
            get
            {
                unchecked
                {
                    return UInt64 / (ulong.MaxValue + 1.0M);
                }
            }
        }

        /// <summary>
        /// Generates a semi cryptographically strong random integer between specified values. i.e. [<paramref name="startNumber"/>-<paramref name="stopNumber"/>)
        /// </summary>
        /// <param name="startNumber">A <see cref="double"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="double"/> that is the high end of our range.</param>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <returns>A <see cref="double"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>, or an exception.  </returns>
        public static double Between(double startNumber, double stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return Number * (stopNumber - startNumber) + startNumber;
        }

        /// <summary>
        /// Fills an array of bytes with a cryptographically strong sequence of random values.
        /// </summary>
        /// <param name="buffer">The array to fill with a cryptographically strong sequence of random values.</param>
        /// <remarks>
        /// <para>The length of the byte array determines how many cryptographically strong random bytes are produced.</para>
        /// <para>This method is thread safe.</para>
        /// </remarks>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <exception cref="ArgumentNullException">buffer is null.</exception>
        public static void GetBytes(byte[] buffer)
        {
            RandomNumberGenerator.GetBytes(buffer);
        }

        /// <summary>
        /// Generates a cryptographically strong random boolean (i.e., a coin toss).
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static bool Boolean
        {
            get
            {
                byte[] value = new byte[1];

                RandomNumberGenerator.GetBytes(value);

                return (value[0] & 1) == 0;
            }
        }

        /// <summary>
        /// Generates a cryptographically strong 8-bit random integer.
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static byte Byte
        {
            get
            {
                byte[] value = new byte[1];

                RandomNumberGenerator.GetBytes(value);

                return value[0];
            }
        }

        /// <summary>
        /// Generates a cryptographically strong 8-bit random integer between specified values. i.e. [<paramref name="startNumber"/>-<paramref name="stopNumber"/>)
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="byte"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="byte"/> that is the high end of our range.</param>
        /// <returns>A <see cref="byte"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static byte ByteBetween(byte startNumber, byte stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (byte)(GetRandomNumberLessThan((uint)stopNumber - (uint)startNumber) + (uint)startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong 16-bit random integer.
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static short Int16
        {
            get
            {
                byte[] value = new byte[2];

                RandomNumberGenerator.GetBytes(value);

                return BitConverter.ToInt16(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong 16-bit random integer between specified values. i.e. [<paramref name="startNumber"/>-<paramref name="stopNumber"/>)
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="short"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="short"/> that is the high end of our range.</param>
        /// <returns>A <see cref="short"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static short Int16Between(short startNumber, short stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (short)(GetRandomNumberLessThan((int)stopNumber - (int)startNumber) + (int)startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 16-bit random integer.
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static ushort UInt16
        {
            get
            {
                byte[] value = new byte[2];

                RandomNumberGenerator.GetBytes(value);

                return BitConverter.ToUInt16(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 16-bit random integer between specified values. i.e. [<paramref name="startNumber"/>-<paramref name="stopNumber"/>)
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="ushort"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="ushort"/> that is the high end of our range.</param>
        /// <returns>A <see cref="ushort"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static ushort UInt16Between(ushort startNumber, ushort stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (ushort)(GetRandomNumberLessThan((uint)stopNumber - (uint)startNumber) + (uint)startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong 24-bit random integer.
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static Int24 Int24
        {
            get
            {
                byte[] value = new byte[3];

                RandomNumberGenerator.GetBytes(value);

                return Int24.GetValue(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong 24-bit random integer between specified values. i.e. [<paramref name="startNumber"/>-<paramref name="stopNumber"/>)
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="Int24"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="Int24"/> that is the high end of our range.</param>
        /// <returns>A <see cref="Int24"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static Int24 Int24Between(Int24 startNumber, Int24 stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (Int24)(GetRandomNumberLessThan((int)stopNumber - (int)startNumber) + (int)startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 24-bit random integer.
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static UInt24 UInt24
        {
            get
            {
                byte[] value = new byte[3];

                RandomNumberGenerator.GetBytes(value);

                return UInt24.GetValue(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 24-bit random integer between specified values. i.e. [<paramref name="startNumber"/>-<paramref name="stopNumber"/>)
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="UInt24"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="UInt24"/> that is the high end of our range.</param>
        /// <returns>A <see cref="UInt24"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static UInt24 Int24Between(UInt24 startNumber, UInt24 stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (UInt24)(GetRandomNumberLessThan((uint)stopNumber - (uint)startNumber) + (uint)startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong 32-bit random integer.
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static int Int32
        {
            get
            {
                byte[] value = new byte[4];

                RandomNumberGenerator.GetBytes(value);

                return BitConverter.ToInt32(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong 32-bit random integer between specified values. i.e. [<paramref name="startNumber"/>-<paramref name="stopNumber"/>)
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="int"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="int"/> that is the high end of our range.</param>
        /// <returns>A <see cref="int"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static int Int32Between(int startNumber, int stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return GetRandomNumberLessThan(stopNumber - startNumber) + startNumber;
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 32-bit random integer. 
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static uint UInt32
        {
            get
            {
                byte[] value = new byte[4];

                RandomNumberGenerator.GetBytes(value);

                return BitConverter.ToUInt32(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 32-bit random integer between specified values. i.e. [<paramref name="startNumber"/>-<paramref name="stopNumber"/>)
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="uint"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="uint"/> that is the high end of our range.</param>
        /// <returns>A <see cref="uint"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static uint UInt32Between(uint startNumber, uint stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return GetRandomNumberLessThan(stopNumber - startNumber) + startNumber;
        }

        /// <summary>
        /// Generates a cryptographically strong 64-bit random integer.
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static long Int64
        {
            get
            {
                byte[] value = new byte[8];

                RandomNumberGenerator.GetBytes(value);

                return BitConverter.ToInt64(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong 64-bit random integer between specified values. i.e. [<paramref name="startNumber"/>-<paramref name="stopNumber"/>)
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="long"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="long"/> that is the high end of our range.</param>
        /// <returns>A <see cref="long"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static long Int64Between(long startNumber, long stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return GetRandomNumberLessThan(stopNumber - startNumber) + startNumber;
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 64-bit random integer.
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static ulong UInt64
        {
            get
            {
                byte[] value = new byte[8];

                RandomNumberGenerator.GetBytes(value);

                return BitConverter.ToUInt64(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 64-bit random integer between specified values. i.e. [<paramref name="startNumber"/>-<paramref name="stopNumber"/>)
        /// </summary>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="ulong"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="ulong"/> that is the high end of our range.</param>
        /// <returns>A <see cref="ulong"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static ulong UInt64Between(ulong startNumber, ulong stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return GetRandomNumberLessThan(stopNumber - startNumber) + startNumber;
        }

        /// <summary>
        /// Returns a cryptographically strong number that is less than the the supplied value
        /// </summary>
        /// <param name="maxValue">the max value to return exclusive</param>
        /// <returns></returns>
        /// <remarks>
        /// if 0 is provided, 0 is returned
        /// </remarks>
        private static uint GetRandomNumberLessThan(uint maxValue)
        {
            //A crypto number cannot be achieved via *, /, or % since these 
            //operations have rounding and uneven redistribution properties. 

            //Method: In order to get a crypto number <= maxValue
            //A random number must be generated. If the random number is in range, it 
            //may be kept, otherwise a new number must be generated.
            //To increase the likelihood that the number is in range,
            //bit masking can be used on the higher order bits to
            //create a 75% likelihood (on average) that the number will be in range.

            //Exception cases where algorithm doesn't work
            if (maxValue == 0 || maxValue == 1)
                return 0;

            //Determine the number of random bits that I need
            int leadingZeroes = BitMath.CountLeadingZeros(maxValue);

            uint value = UInt32;
            //By shifting the value by the number of leading zeros, I'll have
            //a number with the highest likelihood of being in range
            while (value >> leadingZeroes >= maxValue)
            {
                //If the number is outside of range, all bits must
                //be discarded and new ones generated. Not doing this
                //technically alters the crypto-random nature of the value being generated.
                value = UInt32;
            }
            return value >> leadingZeroes;
        }

        /// <summary>
        /// Returns a cryptographically strong number that is less than the the supplied value
        /// </summary>
        /// <param name="maxValue">the max value to return exclusive</param>
        /// <returns></returns>
        /// <remarks>
        /// if 0 is provided, 0 is returned
        /// </remarks>
        private static ulong GetRandomNumberLessThan(ulong maxValue)
        {
            //A crypto number cannot be achieved via *, /, or % since these 
            //operations have rounding and uneven redistribution properties. 

            //Method: In order to get a crypto number <= maxValue
            //A random number must be generated. If the random number is in range, it 
            //may be kept, otherwise a new number must be generated.
            //To increase the likelihood that the number is in range,
            //bit masking can be used on the higher order bits to
            //create a 75% likelihood (on average) that the number will be in range.

            //Exception cases where algorithm doesn't work
            if (maxValue == 0 || maxValue == 1)
                return 0;

            //Determine the number of random bits that I need
            int leadingZeroes = BitMath.CountLeadingZeros(maxValue);

            ulong value = UInt64;
            //By shifting the value by the number of leading zeros, I'll have
            //a number with the highest likelihood of being in range
            while (value >> leadingZeroes >= maxValue)
            {
                //If the number is outside of range, all bits must
                //be discarded and new ones generated. Not doing this
                //technically alters the crypto-random nature of the value being generated.
                value = UInt64;
            }
            return value >> leadingZeroes;
        }

        /// <summary>
        /// Returns a cryptographically strong number that is less the the supplied value
        /// </summary>
        /// <param name="maxValue">the max value to return exclusive</param>
        /// <returns></returns>
        /// <remarks>
        /// A number less than a negative number rolls down to long.MinValue, then to long.MaxValue
        /// if 0 is provided, 0 is returned
        /// </remarks>
        private static long GetRandomNumberLessThan(long maxValue)
        {
            return (long)GetRandomNumberLessThan((ulong)maxValue);
        }

        /// <summary>
        /// Returns a cryptographically strong number that is less the the supplied value
        /// </summary>
        /// <param name="maxValue">the max value to return exclusive</param>
        /// <returns></returns>
        /// <remarks>
        /// A number less than a negative number rolls down to int.MinValue, then to int.MaxValue
        /// if 0 is provided, 0 is returned
        /// </remarks>
        private static int GetRandomNumberLessThan(int maxValue)
        {
            return (int)GetRandomNumberLessThan((uint)maxValue);
        }

    }
}