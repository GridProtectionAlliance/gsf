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
//
//*******************************************************************************************************

using System;
using System.Security.Cryptography;

namespace TVA
{
    /// <summary>Generates cryptographically strong random numbers.</summary>
    public static class Random
    {
        private static RNGCryptoServiceProvider m_randomNumberGenerator = new RNGCryptoServiceProvider();

        /// <summary>Generates a cryptographically strong double-precision floating-point random number between zero and one.</summary>
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
        public static double Between(double startNumber, double stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return Number * (stopNumber - startNumber) + startNumber;
        }

        /// <summary>Generates a cryptographically strong random boolean (i.e., a coin toss).</summary>
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
        public static byte ByteBetween(byte startNumber, byte stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (byte)(Number * (stopNumber - startNumber) + startNumber);
        }

        /// <summary>Generates a cryptographically strong 16-bit random integer.</summary>
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
        public static short Int16Between(short startNumber, short stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (short)(Number * (stopNumber - startNumber) + startNumber);
        }

        /// <summary>Generates a cryptographically strong 32-bit random integer.</summary>
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
        public static int Int32Between(int startNumber, int stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (int)(Number * (stopNumber - startNumber) + startNumber);
        }

        /// <summary>Generates a cryptographically strong 64-bit random integer.</summary>
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
        public static long Int64Between(long startNumber, long stopNumber)
        {
            if (stopNumber < startNumber)
                throw new ArgumentException("stopNumber must be greater than startNumber");

            return (long)(Number * (stopNumber - startNumber) + startNumber);
        }
    }
}