using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
//using TVA.Common;
using TVA.Interop;

//*******************************************************************************************************
//  TVA.Math.Common.vb - Math Functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/29/2005 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Math).
//  01/04/2006 - J. Ritchie Carroll
//       Added crytographically strong random number generation functions.
//  01/24/2006 - J. Ritchie Carroll
//       Added curve fit function (courtesy of Brian Fox from DatAWare client code).
//  11/08/2006 - J. Ritchie Carroll
//       Added standard devitaion and average functions.
//  12/07/2006 - J. Ritchie Carroll
//       Added strongly-typed generic Not "comparator" functions (e.g., NotEqualTo).
//  08/23/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************


namespace TVA
{
    namespace Math
    {

        /// <summary>Defines common math functions.</summary>
        public sealed class Common
        {


            private static System.Security.Cryptography.RNGCryptoServiceProvider m_randomNumberGenerator = new System.Security.Cryptography.RNGCryptoServiceProvider();

            private Common()
            {

                // This class contains only global functions and is not meant to be instantiated.

            }



            /// <summary>Generates a cryptographically strong floating-point random number between zero and one.</summary>
            public static double RandomNumber
            {
                get
                {
                    return BitwiseCast.ToUInt32(RandomInt32) / UInt32.MaxValue;
                }
            }

            /// <summary>Generates a cryptographically strong random decimal between zero and one.</summary>
            public static decimal RandomDecimal
            {
                get
                {
                    return Convert.ToDecimal(BitwiseCast.ToUInt64(RandomInt64)) / Convert.ToDecimal(UInt64.MaxValue);
                }
            }

            /// <summary>Generates a cryptographically strong random integer between specified values.</summary>
            public static double RandomBetween(double startNumber, double stopNumber)
            {
                if (stopNumber < startNumber)
                {
                    throw (new ArgumentException("stopNumber must be greater than startNumber"));
                }
                return RandomNumber * (stopNumber - startNumber) + startNumber;
            }

            /// <summary>Generates a cryptographically strong random boolean (i.e., a coin toss).</summary>
            public static bool RandomBoolean
            {
                get
                {
                    byte[] value = new byte[1];

                    m_randomNumberGenerator.GetBytes(value);

                    return TVA.Common.IIf(value(0) % 2 == 0, true, false);
                }
            }

            /// <summary>Generates a cryptographically strong 8-bit random integer.</summary>
            public static byte RandomByte
            {
                get
                {
                    byte[] value = new byte[1];

                    m_randomNumberGenerator.GetBytes(value);

                    return value(0);
                }
            }

            /// <summary>Generates a cryptographically strong 8-bit random integer between specified values.</summary>
            public static byte RandomByteBetween(byte startNumber, byte stopNumber)
            {
                if (stopNumber < startNumber)
                {
                    throw (new ArgumentException("stopNumber must be greater than startNumber"));
                }
                return Convert.ToByte(RandomNumber * (stopNumber - startNumber) + startNumber);
            }

            /// <summary>Generates a cryptographically strong 16-bit random integer.</summary>
            public static short RandomInt16
            {
                get
                {
                    byte[] value = new byte[2];

                    m_randomNumberGenerator.GetBytes(value);

                    return BitConverter.ToInt16(value, 0);
                }
            }

            /// <summary>Generates a cryptographically strong 16-bit random integer between specified values.</summary>
            public static short RandomInt16Between(short startNumber, short stopNumber)
            {
                if (stopNumber < startNumber)
                {
                    throw (new ArgumentException("stopNumber must be greater than startNumber"));
                }
                return Convert.ToInt16(RandomNumber * (stopNumber - startNumber) + startNumber);
            }

            /// <summary>Generates a cryptographically strong 32-bit random integer.</summary>
            public static int RandomInt32
            {
                get
                {
                    byte[] value = new byte[4];

                    m_randomNumberGenerator.GetBytes(value);

                    return BitConverter.ToInt32(value, 0);
                }
            }

            /// <summary>Generates a cryptographically strong 32-bit random integer between specified values.</summary>
            public static int RandomInt32Between(int startNumber, int stopNumber)
            {
                if (stopNumber < startNumber)
                {
                    throw (new ArgumentException("stopNumber must be greater than startNumber"));
                }
                return Convert.ToInt32(RandomNumber * (stopNumber - startNumber) + startNumber);
            }

            /// <summary>Generates a cryptographically strong 64-bit random integer.</summary>
            public static long RandomInt64
            {
                get
                {
                    byte[] value = new byte[8];

                    m_randomNumberGenerator.GetBytes(value);

                    return BitConverter.ToInt64(value, 0);
                }
            }

            /// <summary>Generates a cryptographically strong 64-bit random integer between specified values.</summary>
            public static long RandomInt64Between(long startNumber, long stopNumber)
            {
                if (stopNumber < startNumber)
                {
                    throw (new ArgumentException("stopNumber must be greater than startNumber"));
                }
                return Convert.ToInt64(RandomNumber * (stopNumber - startNumber) + startNumber);
            }


        }

    }

}
