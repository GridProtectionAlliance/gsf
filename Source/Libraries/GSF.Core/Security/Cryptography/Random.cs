//******************************************************************************************************
//  Random.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
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
        private static readonly RNGCryptoServiceProvider s_randomNumberGenerator = new RNGCryptoServiceProvider();

        /// <summary>
        /// Generates a cryptographically strong double-precision floating-point random number between zero and one.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static double Number
        {
            get
            {
                unchecked
                {
                    return (double)UInt32 / (double)uint.MaxValue;
                }
            }
        }

        /// <summary>
        /// Generates a cryptographically strong random decimal between zero and one.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static decimal Decimal
        {
            get
            {
                unchecked
                {
                    return (decimal)UInt64 / (decimal)ulong.MaxValue;
                }
            }
        }

        /// <summary>
        /// Generates a cryptographically strong random integer between specified values.
        /// </summary>
        /// <param name="startNumber">A <see cref="double"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="double"/> that is the high end of our range.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <returns>A <see cref="double"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>, or an excception.</returns>
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
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <exception cref="System.ArgumentNullException">buffer is null.</exception>
        public static void GetBytes(byte[] buffer)
        {
            s_randomNumberGenerator.GetBytes(buffer);
        }

        /// <summary>
        /// Generates a cryptographically strong random boolean (i.e., a coin toss).
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static bool Boolean
        {
            get
            {
                byte[] value = new byte[1];

                s_randomNumberGenerator.GetBytes(value);

                return value[0] % 2 == 0;
            }
        }

        /// <summary>
        /// Generates a cryptographically strong 8-bit random integer.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static byte Byte
        {
            get
            {
                byte[] value = new byte[1];

                s_randomNumberGenerator.GetBytes(value);

                return value[0];
            }
        }

        /// <summary>
        /// Generates a cryptographically strong 8-bit random integer between specified values.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="byte"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="byte"/> that is the high end of our range.</param>
        /// <returns>A <see cref="byte"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static byte ByteBetween(byte startNumber, byte stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (byte)(Number * (stopNumber - startNumber) + startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong 16-bit random integer.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static short Int16
        {
            get
            {
                byte[] value = new byte[2];

                s_randomNumberGenerator.GetBytes(value);

                return BitConverter.ToInt16(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong 16-bit random integer between specified values.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="short"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="short"/> that is the high end of our range.</param>
        /// <returns>A <see cref="short"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static short Int16Between(short startNumber, short stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (short)(Number * (stopNumber - startNumber) + startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 16-bit random integer.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static ushort UInt16
        {
            get
            {
                byte[] value = new byte[2];

                s_randomNumberGenerator.GetBytes(value);

                return BitConverter.ToUInt16(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 16-bit random integer between specified values.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="ushort"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="ushort"/> that is the high end of our range.</param>
        /// <returns>A <see cref="ushort"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static ushort UInt16Between(ushort startNumber, ushort stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (ushort)(Number * (stopNumber - startNumber) + startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong 24-bit random integer.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static Int24 Int24
        {
            get
            {
                byte[] value = new byte[3];

                s_randomNumberGenerator.GetBytes(value);

                return Int24.GetValue(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong 24-bit random integer between specified values.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="Int24"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="Int24"/> that is the high end of our range.</param>
        /// <returns>A <see cref="Int24"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static Int24 Int24Between(Int24 startNumber, Int24 stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (Int24)(Number * (stopNumber - startNumber) + startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 24-bit random integer.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static UInt24 UInt24
        {
            get
            {
                byte[] value = new byte[3];

                s_randomNumberGenerator.GetBytes(value);

                return UInt24.GetValue(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 24-bit random integer between specified values.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="UInt24"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="UInt24"/> that is the high end of our range.</param>
        /// <returns>A <see cref="UInt24"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static UInt24 Int24Between(UInt24 startNumber, UInt24 stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (UInt24)(Number * (stopNumber - startNumber) + startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong 32-bit random integer.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static int Int32
        {
            get
            {
                byte[] value = new byte[4];

                s_randomNumberGenerator.GetBytes(value);

                return BitConverter.ToInt32(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong 32-bit random integer between specified values.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="int"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="int"/> that is the high end of our range.</param>
        /// <returns>A <see cref="int"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static int Int32Between(int startNumber, int stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (int)(Number * (stopNumber - startNumber) + startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 32-bit random integer.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static uint UInt32
        {
            get
            {
                byte[] value = new byte[4];

                s_randomNumberGenerator.GetBytes(value);

                return BitConverter.ToUInt32(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 32-bit random integer between specified values.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="uint"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="uint"/> that is the high end of our range.</param>
        /// <returns>A <see cref="uint"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static uint UInt32Between(uint startNumber, uint stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (uint)(Number * (stopNumber - startNumber) + startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong 64-bit random integer.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static long Int64
        {
            get
            {
                byte[] value = new byte[8];

                s_randomNumberGenerator.GetBytes(value);

                return BitConverter.ToInt64(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong 64-bit random integer between specified values.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="long"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="long"/> that is the high end of our range.</param>
        /// <returns>A <see cref="long"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static long Int64Between(long startNumber, long stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (long)(Number * (stopNumber - startNumber) + startNumber);
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 64-bit random integer.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static ulong UInt64
        {
            get
            {
                byte[] value = new byte[8];

                s_randomNumberGenerator.GetBytes(value);

                return BitConverter.ToUInt64(value, 0);
            }
        }

        /// <summary>
        /// Generates a cryptographically strong unsigned 64-bit random integer between specified values.
        /// </summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="ulong"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="ulong"/> that is the high end of our range.</param>
        /// <returns>A <see cref="ulong"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static ulong UInt64Between(ulong startNumber, ulong stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (ulong)(Number * (stopNumber - startNumber) + startNumber);
        }
    }
}