/**************************************************************************\
   Copyright © 2009 - Gbtc, James Ritchie Carroll, Pinal C. Patel
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

namespace System
{
    /// <summary>
    /// Defines extension methods related to bit operations.
    /// </summary>
    public static class BitExtensions
    {
        /// <summary>
        /// Gets the bit value for the specified bit index (0 - 63).
        /// </summary>
        /// <param name="bit">Bit index (0 - 63)</param>
        /// <returns>Value of the specified <paramref name="bit"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Parameter must be between 0 and 63.</exception>
        [CLSCompliant(false)]
        public static Bits BitVal(this byte bit)
        {
            switch (bit)
            {
                #region [ Bit Cases (0 - 63) ]

                case 00: return Bits.Bit0;
                case 01: return Bits.Bit1;
                case 02: return Bits.Bit2;
                case 03: return Bits.Bit3;
                case 04: return Bits.Bit4;
                case 05: return Bits.Bit5;
                case 06: return Bits.Bit6;
                case 07: return Bits.Bit7;
                case 08: return Bits.Bit8;
                case 09: return Bits.Bit9;
                case 10: return Bits.Bit10;
                case 11: return Bits.Bit11;
                case 12: return Bits.Bit12;
                case 13: return Bits.Bit13;
                case 14: return Bits.Bit14;
                case 15: return Bits.Bit15;
                case 16: return Bits.Bit16;
                case 17: return Bits.Bit17;
                case 18: return Bits.Bit18;
                case 19: return Bits.Bit19;
                case 20: return Bits.Bit20;
                case 21: return Bits.Bit21;
                case 22: return Bits.Bit22;
                case 23: return Bits.Bit23;
                case 24: return Bits.Bit24;
                case 25: return Bits.Bit25;
                case 26: return Bits.Bit26;
                case 27: return Bits.Bit27;
                case 28: return Bits.Bit28;
                case 29: return Bits.Bit29;
                case 30: return Bits.Bit30;
                case 31: return Bits.Bit31;
                case 32: return Bits.Bit32;
                case 33: return Bits.Bit33;
                case 34: return Bits.Bit34;
                case 35: return Bits.Bit35;
                case 36: return Bits.Bit36;
                case 37: return Bits.Bit37;
                case 38: return Bits.Bit38;
                case 39: return Bits.Bit39;
                case 40: return Bits.Bit40;
                case 41: return Bits.Bit41;
                case 42: return Bits.Bit42;
                case 43: return Bits.Bit43;
                case 44: return Bits.Bit44;
                case 45: return Bits.Bit45;
                case 46: return Bits.Bit46;
                case 47: return Bits.Bit47;
                case 48: return Bits.Bit48;
                case 49: return Bits.Bit49;
                case 50: return Bits.Bit50;
                case 51: return Bits.Bit51;
                case 52: return Bits.Bit52;
                case 53: return Bits.Bit53;
                case 54: return Bits.Bit54;
                case 55: return Bits.Bit55;
                case 56: return Bits.Bit56;
                case 57: return Bits.Bit57;
                case 58: return Bits.Bit58;
                case 59: return Bits.Bit59;
                case 60: return Bits.Bit60;
                case 61: return Bits.Bit61;
                case 62: return Bits.Bit62;
                case 63: return Bits.Bit63;

                #endregion

                default:
                    throw new ArgumentOutOfRangeException("bit", "Parameter must be between 0 and 63.");
            }
        }

        public static byte SetBit(this byte source, byte bit)
        {
            return SetBits(source, BitVal(bit));
        }

        public static byte SetBits(this byte source, Bits bits)
        {
            checked
            {
                return SetBits(source, (byte)bits);
            }
        }

        public static byte SetBits(this byte source, byte bits)
        {
            return ((byte)(source | bits));
        }

        /// <summary>
        /// Determines if specified <paramref name="bit"/> is set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 7) to check.</param>
        /// <returns>true if specified <paramref name="bit"/> is set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBit(this byte source, byte bit)
        {
            return CheckBits(source, BitVal(bit));
        }

        /// <summary>
        /// Determines if specified <paramref name="bit"/> is set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 15) to check.</param>
        /// <returns>true if specified <paramref name="bit"/> is set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBit(this short source, byte bit)
        {
            return CheckBits(source, BitVal(bit));
        }

        /// <summary>
        /// Determines if specified <paramref name="bit"/> is set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 15) to check.</param>
        /// <returns>true if specified <paramref name="bit"/> is set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBit(this ushort source, byte bit)
        {
            return CheckBits(source, BitVal(bit), true);
        }

        /// <summary>
        /// Determines if specified <paramref name="bit"/> is set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 31) to check.</param>
        /// <returns>true if specified <paramref name="bit"/> is set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBit(this int source, byte bit)
        {
            return CheckBits(source, BitVal(bit), true);
        }

        /// <summary>
        /// Determines if specified <paramref name="bit"/> is set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 31) to check.</param>
        /// <returns>true if specified <paramref name="bit"/> is set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBit(this uint source, byte bit)
        {
            return CheckBits(source, BitVal(bit), true);
        }

        /// <summary>
        /// Determines if specified <paramref name="bit"/> is set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 63) to check.</param>
        /// <returns>true if specified <paramref name="bit"/> is set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBit(this long source, byte bit)
        {
            return CheckBits(source, BitVal(bit), true);
        }

        /// <summary>
        /// Determines if specified <paramref name="bit"/> is set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 63) to check.</param>
        /// <returns>true if specified <paramref name="bit"/> is set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBit(this ulong source, byte bit)
        {
            return CheckBits(source, BitVal(bit), true);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this byte source, Bits bits)
        {
            checked
            {
                return CheckBits(source, (byte)bits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this byte source, byte bits)
        {
            return CheckBits(source, bits, false);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this byte source, Bits bits, bool allBits)
        {
            checked
            {
                return CheckBits(source, (byte)bits, allBits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this byte source, byte bits, bool allBits)
        {
            if (allBits)
                return ((source & bits) == bits);
            else
                return ((source & bits) != 0);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this short source, Bits bits)
        {
            checked
            {
                return CheckBits(source, (short)bits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this short source, short bits)
        {
            return CheckBits(source, bits, false);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this short source, Bits bits, bool allBits)
        {
            checked
            {
                return CheckBits(source, (short)bits, allBits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this short source, short bits, bool allBits)
        {
            if (allBits)
                return ((source & bits) == bits);
            else
                return ((source & bits) != 0);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this ushort source, Bits bits)
        {
            checked
            {
                return CheckBits(source, (ushort)bits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this ushort source, ushort bits)
        {
            return CheckBits(source, bits, false);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this ushort source, Bits bits, bool allBits)
        {
            checked
            {
                return CheckBits(source, (ushort)bits, allBits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this ushort source, ushort bits, bool allBits)
        {
            if (allBits)
                return ((source & bits) == bits);
            else
                return ((source & bits) != 0);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this int source, Bits bits)
        {
            checked
            {
                return CheckBits(source, (int)bits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this int source, int bits)
        {
            return CheckBits(source, bits, false);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this int source, Bits bits, bool allBits)
        {
            checked
            {
                return CheckBits(source, (int)bits, allBits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this int source, int bits, bool allBits)
        {
            if (allBits)
                return ((source & bits) == bits);
            else
                return ((source & bits) != 0);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this uint source, Bits bits)
        {
            checked
            {
                return CheckBits(source, (uint)bits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this uint source, uint bits)
        {
            return CheckBits(source, bits, false);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this uint source, Bits bits, bool allBits)
        {
            checked
            {
                return CheckBits(source, (uint)bits, allBits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this uint source, uint bits, bool allBits)
        {
            if (allBits)
                return ((source & bits) == bits);
            else
                return ((source & bits) != 0);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this long source, Bits bits)
        {
            checked
            {
                return CheckBits(source, (long)bits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this long source, long bits)
        {
            return CheckBits(source, bits, false);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this long source, Bits bits, bool allBits)
        {
            checked
            {
                return CheckBits(source, (long)bits, allBits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this long source, long bits, bool allBits)
        {
            if (allBits)
                return ((source & bits) == bits);
            else
                return ((source & bits) != 0);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this ulong source, Bits bits)
        {
            checked
            {
                return CheckBits(source, (ulong)bits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this ulong source, ulong bits)
        {
            return CheckBits(source, bits, false);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this ulong source, Bits bits, bool allBits)
        {
            checked
            {
                return CheckBits(source, (ulong)bits, allBits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this ulong source, ulong bits, bool allBits)
        {
            if (allBits)
                return ((source & bits) == bits);
            else
                return ((source & bits) != 0);
        }
    }
}
