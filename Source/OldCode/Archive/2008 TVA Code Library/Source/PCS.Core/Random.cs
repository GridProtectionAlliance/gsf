//*******************************************************************************************************
//  Random.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/04/2006 - James R Carroll
//      Generated original version of source code.
//  09/17/2008 - James R Carroll
//      Converted to C#.
//  02/16/2009 - Josh Patterson
//      Edited Code Comments
//
//*******************************************************************************************************

using System;
using System.Security.Cryptography;

namespace PCS
{
    /// <summary>Generates cryptographically strong random numbers.</summary>
    public static class Random
    {
        private static RNGCryptoServiceProvider m_randomNumberGenerator = new RNGCryptoServiceProvider();

        /// <summary>Generates a cryptographically strong double-precision floating-point random number between zero and one.</summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static double Number
        {
            get
            {
                unchecked
                {
                    return (double)((uint)Int32) / (double)uint.MaxValue;
                }
            }
        }

        /// <summary>Generates a cryptographically strong random decimal between zero and one.</summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static decimal Decimal
        {
            get
            {
                unchecked
                {
                    return (decimal)((ulong)Int64) / (decimal)ulong.MaxValue;
                }
            }
        }

        /// <summary>Generates a cryptographically strong random integer between specified values.</summary>
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
            m_randomNumberGenerator.GetBytes(buffer);
        }

        /// <summary>Generates a cryptographically strong random boolean (i.e., a coin toss).</summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static bool Boolean
        {
            get
            {
                byte[] value = new byte[1];

                m_randomNumberGenerator.GetBytes(value);

                return (value[0] % 2 == 0 ? true : false);
            }
        }

        /// <summary>Generates a cryptographically strong 8-bit random integer.</summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static byte Byte
        {
            get
            {
                byte[] value = new byte[1];

                m_randomNumberGenerator.GetBytes(value);

                return value[0];
            }
        }

        /// <summary>Generates a cryptographically strong 8-bit random integer between specified values.</summary>
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

        /// <summary>Generates a cryptographically strong 16-bit random integer.</summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static short Int16
        {
            get
            {
                byte[] value = new byte[2];

                m_randomNumberGenerator.GetBytes(value);

                return BitConverter.ToInt16(value, 0);
            }
        }

        /// <summary>Generates a cryptographically strong 16-bit random integer between specified values.</summary>
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

        /// <summary>Generates a cryptographically strong 24-bit random integer.</summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static System.Int24 Int24
        {
            get
            {
                byte[] value = new byte[3];

                m_randomNumberGenerator.GetBytes(value);

                return System.Int24.GetValue(value, 0);
            }
        }

        /// <summary>Generates a cryptographically strong 24-bit random integer between specified values.</summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        /// <param name="startNumber">A <see cref="System.Int24"/> that is the low end of our range.</param>
        /// <param name="stopNumber">A <see cref="System.Int24"/> that is the high end of our range.</param>
        /// <returns>A <see cref="System.Int24"/> that is generated between the <paramref name="startNumber"/> and the <paramref name="stopNumber"/>.</returns>
        public static System.Int24 Int24Between(System.Int24 startNumber, System.Int24 stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (System.Int24)(Number * (stopNumber - startNumber) + startNumber);
        }

        /// <summary>Generates a cryptographically strong 32-bit random integer.</summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static int Int32
        {
            get
            {
                byte[] value = new byte[4];

                m_randomNumberGenerator.GetBytes(value);

                return BitConverter.ToInt32(value, 0);
            }
        }

        /// <summary>Generates a cryptographically strong 32-bit random integer between specified values.</summary>
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

        /// <summary>Generates a cryptographically strong 64-bit random integer.</summary>
        /// <exception cref="System.Security.Cryptography.CryptographicException">The cryptographic service provider (CSP) cannot be acquired.</exception>
        public static long Int64
        {
            get
            {
                byte[] value = new byte[8];

                m_randomNumberGenerator.GetBytes(value);

                return BitConverter.ToInt64(value, 0);
            }
        }

        /// <summary>Generates a cryptographically strong 64-bit random integer between specified values.</summary>
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
    }
}