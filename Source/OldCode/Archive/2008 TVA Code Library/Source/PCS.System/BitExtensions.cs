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
    #region [ Enumerations ]

    /// <summary>
    /// Represents bits in a signed or unsigned integer value.
    /// </summary>
    [CLSCompliant(false)]
    [Flags()]
    public enum Bits : ulong
    {
        /// <summary>No bits set (8-bit)</summary>
        Nill = 0x00000000,

        // Byte 0, Bits 0-7

        /// <summary>Bit 0 (0x0000000000000001)</summary>
        Bit0 = 0x00000001,

        /// <summary>Bit 1 (0x0000000000000002)</summary>
        Bit1 = Bit0 << 1,

        /// <summary>Bit 2 (0x0000000000000004)</summary>
        Bit2 = Bit1 << 1,

        /// <summary>Bit 3 (0x0000000000000008)</summary>
        Bit3 = Bit2 << 1,

        /// <summary>Bit 4 (0x0000000000000010)</summary>
        Bit4 = Bit3 << 1,

        /// <summary>Bit 5 (0x0000000000000020)</summary>
        Bit5 = Bit4 << 1,

        /// <summary>Bit 6 (0x0000000000000040)</summary>
        Bit6 = Bit5 << 1,

        /// <summary>Bit 7 (0x0000000000000080)</summary>
        Bit7 = Bit6 << 1,

        // Byte 1, Bits 8-15

        /// <summary>Bit 8 (0x0000000000000100)</summary>
        Bit8 = Bit7 << 1,

        /// <summary>Bit 9 (0x0000000000000200)</summary>
        Bit9 = Bit8 << 1,

        /// <summary>Bit 10 (0x0000000000000400)</summary>
        Bit10 = Bit9 << 1,

        /// <summary>Bit 11 (0x0000000000000800)</summary>
        Bit11 = Bit10 << 1,

        /// <summary>Bit 12 (0x0000000000001000)</summary>
        Bit12 = Bit11 << 1,

        /// <summary>Bit 13 (0x0000000000002000)</summary>
        Bit13 = Bit12 << 1,

        /// <summary>Bit 14 (0x0000000000004000)</summary>
        Bit14 = Bit13 << 1,

        /// <summary>Bit 15 (0x0000000000008000)</summary>
        Bit15 = Bit14 << 1,

        // Byte 2, Bits 16-23

        /// <summary>Bit 16 (0x0000000000010000)</summary>
        Bit16 = Bit15 << 1,

        /// <summary>Bit 17 (0x0000000000020000)</summary>
        Bit17 = Bit16 << 1,

        /// <summary>Bit 18 (0x0000000000040000)</summary>
        Bit18 = Bit17 << 1,

        /// <summary>Bit 19 (0x0000000000080000)</summary>
        Bit19 = Bit18 << 1,

        /// <summary>Bit 20 (0x0000000000100000)</summary>
        Bit20 = Bit19 << 1,

        /// <summary>Bit 21 (0x0000000000200000)</summary>
        Bit21 = Bit20 << 1,

        /// <summary>Bit 22 (0x0000000000400000)</summary>
        Bit22 = Bit21 << 1,

        /// <summary>Bit 23 (0x0000000000800000)</summary>
        Bit23 = Bit22 << 1,

        // Byte 3, Bits 24-31

        /// <summary>Bit 24 (0x0000000001000000)</summary>
        Bit24 = Bit23 << 1,

        /// <summary>Bit 25 (0x0000000002000000)</summary>
        Bit25 = Bit24 << 1,

        /// <summary>Bit 26 (0x0000000004000000)</summary>
        Bit26 = Bit25 << 1,

        /// <summary>Bit 27 (0x0000000008000000)</summary>
        Bit27 = Bit26 << 1,

        /// <summary>Bit 28 (0x0000000010000000)</summary>
        Bit28 = Bit27 << 1,

        /// <summary>Bit 29 (0x0000000020000000)</summary>
        Bit29 = Bit28 << 1,

        /// <summary>Bit 30 (0x0000000040000000)</summary>
        Bit30 = Bit29 << 1,

        /// <summary>Bit 31 (0x0000000080000000)</summary>
        Bit31 = Bit30 << 1,

        // Byte 4, Bits 32-39

        /// <summary>Bit 32 (0x0000000100000000)</summary>
        Bit32 = Bit31 << 1,

        /// <summary>Bit 33 (0x0000000200000000)</summary>
        Bit33 = Bit32 << 1,

        /// <summary>Bit 34 (0x0000000400000000)</summary>
        Bit34 = Bit33 << 1,

        /// <summary>Bit 35 (0x0000000800000000)</summary>
        Bit35 = Bit34 << 1,

        /// <summary>Bit 36 (0x0000001000000000)</summary>
        Bit36 = Bit35 << 1,

        /// <summary>Bit 37 (0x0000002000000000)</summary>
        Bit37 = Bit36 << 1,

        /// <summary>Bit 38 (0x0000004000000000)</summary>
        Bit38 = Bit37 << 1,

        /// <summary>Bit 39 (0x0000008000000000)</summary>
        Bit39 = Bit38 << 1,

        // Byte 5, Bits 40-47

        /// <summary>Bit 40 (0x0000010000000000)</summary>
        Bit40 = Bit39 << 1,

        /// <summary>Bit 41 (0x0000020000000000)</summary>
        Bit41 = Bit40 << 1,

        /// <summary>Bit 42 (0x0000040000000000)</summary>
        Bit42 = Bit41 << 1,

        /// <summary>Bit 43 (0x0000080000000000)</summary>
        Bit43 = Bit42 << 1,

        /// <summary>Bit 44 (0x0000100000000000)</summary>
        Bit44 = Bit43 << 1,

        /// <summary>Bit 45 (0x0000200000000000)</summary>
        Bit45 = Bit44 << 1,

        /// <summary>Bit 46 (0x0000400000000000)</summary>
        Bit46 = Bit45 << 1,

        /// <summary>Bit 47 (0x0000800000000000)</summary>
        Bit47 = Bit46 << 1,

        // Byte 6, Bits 48-55

        /// <summary>Bit 48 (0x0001000000000000)</summary>
        Bit48 = Bit47 << 1,

        /// <summary>Bit 49 (0x0002000000000000)</summary>
        Bit49 = Bit48 << 1,

        /// <summary>Bit 50 (0x0004000000000000)</summary>
        Bit50 = Bit49 << 1,

        /// <summary>Bit 51 (0x0008000000000000)</summary>
        Bit51 = Bit50 << 1,

        /// <summary>Bit 52 (0x0010000000000000)</summary>
        Bit52 = Bit51 << 1,

        /// <summary>Bit 53 (0x0020000000000000)</summary>
        Bit53 = Bit52 << 1,

        /// <summary>Bit 54 (0x0040000000000000)</summary>
        Bit54 = Bit53 << 1,

        /// <summary>Bit 55 (0x0080000000000000)</summary>
        Bit55 = Bit54 << 1,

        // Byte 7, Bits 56-63

        /// <summary>Bit 56 (0x0100000000000000)</summary>
        Bit56 = Bit55 << 1,

        /// <summary>Bit 57 (0x0200000000000000)</summary>
        Bit57 = Bit56 << 1,

        /// <summary>Bit 58 (0x0400000000000000)</summary>
        Bit58 = Bit57 << 1,

        /// <summary>Bit 59 (0x0800000000000000)</summary>
        Bit59 = Bit58 << 1,

        /// <summary>Bit 60 (0x1000000000000000)</summary>
        Bit60 = Bit59 << 1,

        /// <summary>Bit 61 (0x2000000000000000)</summary>
        Bit61 = Bit60 << 1,

        /// <summary>Bit 62 (0x4000000000000000)</summary>
        Bit62 = Bit61 << 1,

        /// <summary>Bit 63 (0x8000000000000000)</summary>
        Bit63 = Bit62 << 1
    }

    #endregion

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

        #region [ SetBit(s) Extensions ]

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 6) to set.</param>
        /// <returns><see cref="sbyte"/> value with specified <paramref name="bit"/> set.</returns>
        public static sbyte SetBit(this sbyte source, byte bit)
        {
            return SetBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 7) to set.</param>
        /// <returns><see cref="byte"/> value with specified <paramref name="bit"/> set.</returns>
        public static byte SetBit(this byte source, byte bit)
        {
            return SetBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 14) to set.</param>
        /// <returns><see cref="short"/> value with specified <paramref name="bit"/> set.</returns>
        public static short SetBit(this short source, byte bit)
        {
            return SetBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 15) to set.</param>
        /// <returns><see cref="ushort"/> value with specified <paramref name="bit"/> set.</returns>
        [CLSCompliant(false)]
        public static ushort SetBit(this ushort source, byte bit)
        {
            return SetBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 22) to set.</param>
        /// <returns><see cref="Int24"/> value with specified <paramref name="bit"/> set.</returns>
        public static Int24 SetBit(this Int24 source, byte bit)
        {
            return SetBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 23) to set.</param>
        /// <returns><see cref="UInt24"/> value with specified <paramref name="bit"/> set.</returns>
        [CLSCompliant(false)]
        public static UInt24 SetBit(this UInt24 source, byte bit)
        {
            return SetBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 30) to set.</param>
        /// <returns><see cref="int"/> value with specified <paramref name="bit"/> set.</returns>
        public static int SetBit(this int source, byte bit)
        {
            return SetBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 31) to set.</param>
        /// <returns><see cref="uint"/> value with specified <paramref name="bit"/> set.</returns>
        [CLSCompliant(false)]
        public static uint SetBit(this uint source, byte bit)
        {
            return SetBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 62) to set.</param>
        /// <returns><see cref="long"/> value with specified <paramref name="bit"/> set.</returns>
        public static long SetBit(this long source, byte bit)
        {
            return SetBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 63) to set.</param>
        /// <returns><see cref="ulong"/> value with specified <paramref name="bit"/> set.</returns>
        [CLSCompliant(false)]
        public static ulong SetBit(this ulong source, byte bit)
        {
            return SetBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to set.</param>
        /// <returns><see cref="sbyte"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static sbyte SetBits(this sbyte source, Bits bits)
        {
            checked
            {
                return SetBits(source, (sbyte)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to set.</param>
        /// <returns><see cref="sbyte"/> value with specified <paramref name="bits"/> set.</returns>
        public static sbyte SetBits(this sbyte source, sbyte bits)
        {
            return ((sbyte)(source | bits));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to set.</param>
        /// <returns><see cref="byte"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static byte SetBits(this byte source, Bits bits)
        {
            checked
            {
                return SetBits(source, (byte)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to set.</param>
        /// <returns><see cref="byte"/> value with specified <paramref name="bits"/> set.</returns>
        public static byte SetBits(this byte source, byte bits)
        {
            return ((byte)(source | bits));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to set.</param>
        /// <returns><see cref="short"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static short SetBits(this short source, Bits bits)
        {
            checked
            {
                return SetBits(source, (short)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to set.</param>
        /// <returns><see cref="short"/> value with specified <paramref name="bits"/> set.</returns>
        public static short SetBits(this short source, short bits)
        {
            return ((short)(source | bits));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to set.</param>
        /// <returns><see cref="ushort"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static ushort SetBits(this ushort source, Bits bits)
        {
            checked
            {
                return SetBits(source, (ushort)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to set.</param>
        /// <returns><see cref="ushort"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static ushort SetBits(this ushort source, ushort bits)
        {
            return ((ushort)(source | bits));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to set.</param>
        /// <returns><see cref="Int24"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static Int24 SetBits(this Int24 source, Bits bits)
        {
            checked
            {
                return SetBits(source, (Int24)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to set.</param>
        /// <returns><see cref="Int24"/> value with specified <paramref name="bits"/> set.</returns>
        public static Int24 SetBits(this Int24 source, Int24 bits)
        {
            return (source | bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to set.</param>
        /// <returns><see cref="UInt24"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static UInt24 SetBits(this UInt24 source, Bits bits)
        {
            checked
            {
                return SetBits(source, (UInt24)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to set.</param>
        /// <returns><see cref="UInt24"/> value with specified <paramref name="bits"/> set.</returns>
        public static UInt24 SetBits(this UInt24 source, UInt24 bits)
        {
            return (source | bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to set.</param>
        /// <returns><see cref="int"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static int SetBits(this int source, Bits bits)
        {
            checked
            {
                return SetBits(source, (int)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to set.</param>
        /// <returns><see cref="int"/> value with specified <paramref name="bits"/> set.</returns>
        public static int SetBits(this int source, int bits)
        {
            return (source | bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to set.</param>
        /// <returns><see cref="uint"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static uint SetBits(this uint source, Bits bits)
        {
            checked
            {
                return SetBits(source, (uint)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to set.</param>
        /// <returns><see cref="uint"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static uint SetBits(this uint source, uint bits)
        {
            return (source | bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to set.</param>
        /// <returns><see cref="long"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static long SetBits(this long source, Bits bits)
        {
            checked
            {
                return SetBits(source, (long)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to set.</param>
        /// <returns><see cref="long"/> value with specified <paramref name="bits"/> set.</returns>
        public static long SetBits(this long source, long bits)
        {
            return (source | bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to set.</param>
        /// <returns><see cref="ulong"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static ulong SetBits(this ulong source, Bits bits)
        {
            checked
            {
                return SetBits(source, (ulong)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to set.</param>
        /// <returns><see cref="ulong"/> value with specified <paramref name="bits"/> set.</returns>
        [CLSCompliant(false)]
        public static ulong SetBits(this ulong source, ulong bits)
        {
            return (source | bits);
        }

        #endregion

        #region [ ClearBit(s) Extensions ]

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 6) to clear.</param>
        /// <returns><see cref="sbyte"/> value with specified <paramref name="bit"/> cleared.</returns>
        public static sbyte ClearBit(this sbyte source, byte bit)
        {
            return ClearBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 7) to clear.</param>
        /// <returns><see cref="byte"/> value with specified <paramref name="bit"/> cleared.</returns>
        public static byte ClearBit(this byte source, byte bit)
        {
            return ClearBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 14) to clear.</param>
        /// <returns><see cref="short"/> value with specified <paramref name="bit"/> cleared.</returns>
        public static short ClearBit(this short source, byte bit)
        {
            return ClearBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 15) to clear.</param>
        /// <returns><see cref="ushort"/> value with specified <paramref name="bit"/> cleared.</returns>
        [CLSCompliant(false)]
        public static ushort ClearBit(this ushort source, byte bit)
        {
            return ClearBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 22) to clear.</param>
        /// <returns><see cref="Int24"/> value with specified <paramref name="bit"/> cleared.</returns>
        public static Int24 ClearBit(this Int24 source, byte bit)
        {
            return ClearBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 23) to clear.</param>
        /// <returns><see cref="UInt24"/> value with specified <paramref name="bit"/> cleared.</returns>
        [CLSCompliant(false)]
        public static UInt24 ClearBit(this UInt24 source, byte bit)
        {
            return ClearBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 30) to clear.</param>
        /// <returns><see cref="int"/> value with specified <paramref name="bit"/> cleared.</returns>
        public static int ClearBit(this int source, byte bit)
        {
            return ClearBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 31) to clear.</param>
        /// <returns><see cref="uint"/> value with specified <paramref name="bit"/> cleared.</returns>
        [CLSCompliant(false)]
        public static uint ClearBit(this uint source, byte bit)
        {
            return ClearBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 62) to clear.</param>
        /// <returns><see cref="long"/> value with specified <paramref name="bit"/> cleared.</returns>
        public static long ClearBit(this long source, byte bit)
        {
            return ClearBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 63) to clear.</param>
        /// <returns><see cref="ulong"/> value with specified <paramref name="bit"/> cleared.</returns>
        [CLSCompliant(false)]
        public static ulong ClearBit(this ulong source, byte bit)
        {
            return ClearBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to clear.</param>
        /// <returns><see cref="sbyte"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static sbyte ClearBits(this sbyte source, Bits bits)
        {
            checked
            {
                return ClearBits(source, (sbyte)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to clear.</param>
        /// <returns><see cref="sbyte"/> value with specified <paramref name="bits"/> cleared.</returns>
        public static sbyte ClearBits(this sbyte source, sbyte bits)
        {
            return ((sbyte)(source & ~bits));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to clear.</param>
        /// <returns><see cref="byte"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static byte ClearBits(this byte source, Bits bits)
        {
            checked
            {
                return ClearBits(source, (byte)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to clear.</param>
        /// <returns><see cref="byte"/> value with specified <paramref name="bits"/> cleared.</returns>
        public static byte ClearBits(this byte source, byte bits)
        {
            return ((byte)(source & ~bits));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to clear.</param>
        /// <returns><see cref="short"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static short ClearBits(this short source, Bits bits)
        {
            checked
            {
                return ClearBits(source, (short)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to clear.</param>
        /// <returns><see cref="short"/> value with specified <paramref name="bits"/> cleared.</returns>
        public static short ClearBits(this short source, short bits)
        {
            return ((short)(source & ~bits));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to clear.</param>
        /// <returns><see cref="ushort"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static ushort ClearBits(this ushort source, Bits bits)
        {
            checked
            {
                return ClearBits(source, (ushort)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to clear.</param>
        /// <returns><see cref="ushort"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static ushort ClearBits(this ushort source, ushort bits)
        {
            return ((ushort)(source & ~bits));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to clear.</param>
        /// <returns><see cref="Int24"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static Int24 ClearBits(this Int24 source, Bits bits)
        {
            checked
            {
                return ClearBits(source, (Int24)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to clear.</param>
        /// <returns><see cref="Int24"/> value with specified <paramref name="bits"/> cleared.</returns>
        public static Int24 ClearBits(this Int24 source, Int24 bits)
        {
            return (source & ~bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to clear.</param>
        /// <returns><see cref="UInt24"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static UInt24 ClearBits(this UInt24 source, Bits bits)
        {
            checked
            {
                return ClearBits(source, (UInt24)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to clear.</param>
        /// <returns><see cref="UInt24"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static UInt24 ClearBits(this UInt24 source, UInt24 bits)
        {
            return (source & ~bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to clear.</param>
        /// <returns><see cref="int"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static int ClearBits(this int source, Bits bits)
        {
            checked
            {
                return ClearBits(source, (int)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to clear.</param>
        /// <returns><see cref="int"/> value with specified <paramref name="bits"/> cleared.</returns>
        public static int ClearBits(this int source, int bits)
        {
            return (source & ~bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to clear.</param>
        /// <returns><see cref="uint"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static uint ClearBits(this uint source, Bits bits)
        {
            checked
            {
                return ClearBits(source, (uint)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to clear.</param>
        /// <returns><see cref="uint"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static uint ClearBits(this uint source, uint bits)
        {
            return (source & ~bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to clear.</param>
        /// <returns><see cref="long"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static long ClearBits(this long source, Bits bits)
        {
            checked
            {
                return ClearBits(source, (long)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to clear.</param>
        /// <returns><see cref="long"/> value with specified <paramref name="bits"/> cleared.</returns>
        public static long ClearBits(this long source, long bits)
        {
            return (source & ~bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to clear.</param>
        /// <returns><see cref="ulong"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static ulong ClearBits(this ulong source, Bits bits)
        {
            checked
            {
                return ClearBits(source, (ulong)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> cleared.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to clear.</param>
        /// <returns><see cref="ulong"/> value with specified <paramref name="bits"/> cleared.</returns>
        [CLSCompliant(false)]
        public static ulong ClearBits(this ulong source, ulong bits)
        {
            return (source & ~bits);
        }

        #endregion

        #region [ CheckBit(s) Extensions ]

        /// <summary>
        /// Determines if specified <paramref name="bit"/> is set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 6) to check.</param>
        /// <returns>true if specified <paramref name="bit"/> is set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBit(this sbyte source, byte bit)
        {
            return CheckBits(source, BitVal(bit));
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
        /// <param name="bit">Bit index (0 - 14) to check.</param>
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
        /// <param name="bit">Bit index (0 - 22) to check.</param>
        /// <returns>true if specified <paramref name="bit"/> is set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBit(this Int24 source, byte bit)
        {
            return CheckBits(source, BitVal(bit), true);
        }

        /// <summary>
        /// Determines if specified <paramref name="bit"/> is set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 23) to check.</param>
        /// <returns>true if specified <paramref name="bit"/> is set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBit(this UInt24 source, byte bit)
        {
            return CheckBits(source, BitVal(bit), true);
        }

        /// <summary>
        /// Determines if specified <paramref name="bit"/> is set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 30) to check.</param>
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
        /// <param name="bit">Bit index (0 - 62) to check.</param>
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
        public static bool CheckBits(this sbyte source, Bits bits)
        {
            checked
            {
                return CheckBits(source, (sbyte)bits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this sbyte source, sbyte bits)
        {
            return CheckBits(source, bits, true);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this sbyte source, Bits bits, bool allBits)
        {
            checked
            {
                return CheckBits(source, (sbyte)bits, allBits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this sbyte source, sbyte bits, bool allBits)
        {
            return (allBits ? ((source & bits) == bits) : ((source & bits) != 0));
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this byte source, byte bits)
        {
            return CheckBits(source, bits, true);
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this byte source, byte bits, bool allBits)
        {
            return (allBits ? ((source & bits) == bits) : ((source & bits) != 0));
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this short source, short bits)
        {
            return CheckBits(source, bits, true);
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this short source, short bits, bool allBits)
        {
            return (allBits ? ((source & bits) == bits) : ((source & bits) != 0));
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this ushort source, ushort bits)
        {
            return CheckBits(source, bits, true);
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this ushort source, ushort bits, bool allBits)
        {
            return (allBits ? ((source & bits) == bits) : ((source & bits) != 0));
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this Int24 source, Bits bits)
        {
            checked
            {
                return CheckBits(source, (Int24)bits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this Int24 source, Int24 bits)
        {
            return CheckBits(source, bits, true);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this Int24 source, Bits bits, bool allBits)
        {
            checked
            {
                return CheckBits(source, (Int24)bits, allBits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this Int24 source, Int24 bits, bool allBits)
        {
            return (allBits ? ((source & bits) == bits) : ((source & bits) != 0));
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this UInt24 source, Bits bits)
        {
            checked
            {
                return CheckBits(source, (UInt24)bits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this UInt24 source, UInt24 bits)
        {
            return CheckBits(source, bits, true);
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this UInt24 source, Bits bits, bool allBits)
        {
            checked
            {
                return CheckBits(source, (UInt24)bits, allBits);
            }
        }

        /// <summary>
        /// Determines if specified <paramref name="bits"/> are set.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this UInt24 source, UInt24 bits, bool allBits)
        {
            return (allBits ? ((source & bits) == bits) : ((source & bits) != 0));
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this int source, int bits)
        {
            return CheckBits(source, bits, true);
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this int source, int bits, bool allBits)
        {
            return (allBits ? ((source & bits) == bits) : ((source & bits) != 0));
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this uint source, uint bits)
        {
            return CheckBits(source, bits, true);
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this uint source, uint bits, bool allBits)
        {
            return (allBits ? ((source & bits) == bits) : ((source & bits) != 0));
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this long source, long bits)
        {
            return CheckBits(source, bits, true);
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        public static bool CheckBits(this long source, long bits, bool allBits)
        {
            return (allBits ? ((source & bits) == bits) : ((source & bits) != 0));
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this ulong source, ulong bits)
        {
            return CheckBits(source, bits, true);
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
        /// <param name="bits">Bit-mask of the bits to check.</param>
        /// <param name="allBits">true to check if all <paramref name="bits"/> are set; otherwise false.</param>
        /// <returns>true if specified <paramref name="bits"/> are set in <paramref name="source"/> value; otherwise false.</returns>
        [CLSCompliant(false)]
        public static bool CheckBits(this ulong source, ulong bits, bool allBits)
        {
            return (allBits ? ((source & bits) == bits) : ((source & bits) != 0));
        }

        #endregion

        #region [ ToggleBit(s) Extensions ]

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 6) to toggle.</param>
        /// <returns><see cref="sbyte"/> value with specified <paramref name="bit"/> toggled.</returns>
        public static sbyte ToggleBit(this sbyte source, byte bit)
        {
            return ToggleBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 7) to toggle.</param>
        /// <returns><see cref="byte"/> value with specified <paramref name="bit"/> toggled.</returns>
        public static byte ToggleBit(this byte source, byte bit)
        {
            return ToggleBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 14) to toggle.</param>
        /// <returns><see cref="short"/> value with specified <paramref name="bit"/> toggled.</returns>
        public static short ToggleBit(this short source, byte bit)
        {
            return ToggleBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 15) to toggle.</param>
        /// <returns><see cref="ushort"/> value with specified <paramref name="bit"/> toggled.</returns>
        [CLSCompliant(false)]
        public static ushort ToggleBit(this ushort source, byte bit)
        {
            return ToggleBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 22) to toggle.</param>
        /// <returns><see cref="Int24"/> value with specified <paramref name="bit"/> toggled.</returns>
        public static Int24 ToggleBit(this Int24 source, byte bit)
        {
            return ToggleBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 31) to toggle.</param>
        /// <returns><see cref="UInt24"/> value with specified <paramref name="bit"/> toggled.</returns>
        [CLSCompliant(false)]
        public static UInt24 ToggleBit(this UInt24 source, byte bit)
        {
            return ToggleBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 30) to toggle.</param>
        /// <returns><see cref="int"/> value with specified <paramref name="bit"/> toggled.</returns>
        public static int ToggleBit(this int source, byte bit)
        {
            return ToggleBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 31) to toggle.</param>
        /// <returns><see cref="uint"/> value with specified <paramref name="bit"/> toggled.</returns>
        [CLSCompliant(false)]
        public static uint ToggleBit(this uint source, byte bit)
        {
            return ToggleBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 62) to toggle.</param>
        /// <returns><see cref="long"/> value with specified <paramref name="bit"/> toggled.</returns>
        public static long ToggleBit(this long source, byte bit)
        {
            return ToggleBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bit"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit (0 - 63) to toggle.</param>
        /// <returns><see cref="ulong"/> value with specified <paramref name="bit"/> toggled.</returns>
        [CLSCompliant(false)]
        public static ulong ToggleBit(this ulong source, byte bit)
        {
            return ToggleBits(source, BitVal(bit));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to toggle.</param>
        /// <returns><see cref="sbyte"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static sbyte ToggleBits(this sbyte source, Bits bits)
        {
            checked
            {
                return ToggleBits(source, (sbyte)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to toggle.</param>
        /// <returns><see cref="sbyte"/> value with specified <paramref name="bits"/> toggled.</returns>
        public static sbyte ToggleBits(this sbyte source, sbyte bits)
        {
            return ((sbyte)(source ^ bits));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to toggle.</param>
        /// <returns><see cref="byte"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static byte ToggleBits(this byte source, Bits bits)
        {
            checked
            {
                return ToggleBits(source, (byte)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to toggle.</param>
        /// <returns><see cref="byte"/> value with specified <paramref name="bits"/> toggled.</returns>
        public static byte ToggleBits(this byte source, byte bits)
        {
            return ((byte)(source ^ bits));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to toggle.</param>
        /// <returns><see cref="short"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static short ToggleBits(this short source, Bits bits)
        {
            checked
            {
                return ToggleBits(source, (short)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to toggle.</param>
        /// <returns><see cref="short"/> value with specified <paramref name="bits"/> toggled.</returns>
        public static short ToggleBits(this short source, short bits)
        {
            return ((short)(source ^ bits));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to toggle.</param>
        /// <returns><see cref="ushort"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static ushort ToggleBits(this ushort source, Bits bits)
        {
            checked
            {
                return ToggleBits(source, (ushort)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to toggle.</param>
        /// <returns><see cref="ushort"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static ushort ToggleBits(this ushort source, ushort bits)
        {
            return ((ushort)(source ^ bits));
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to toggle.</param>
        /// <returns><see cref="Int24"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static Int24 ToggleBits(this Int24 source, Bits bits)
        {
            checked
            {
                return ToggleBits(source, (Int24)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to toggle.</param>
        /// <returns><see cref="Int24"/> value with specified <paramref name="bits"/> toggled.</returns>
        public static Int24 ToggleBits(this Int24 source, Int24 bits)
        {
            return (source ^ bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to toggle.</param>
        /// <returns><see cref="UInt24"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static UInt24 ToggleBits(this UInt24 source, Bits bits)
        {
            checked
            {
                return ToggleBits(source, (UInt24)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to toggle.</param>
        /// <returns><see cref="UInt24"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static UInt24 ToggleBits(this UInt24 source, UInt24 bits)
        {
            return (source ^ bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to toggle.</param>
        /// <returns><see cref="int"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static int ToggleBits(this int source, Bits bits)
        {
            checked
            {
                return ToggleBits(source, (int)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to toggle.</param>
        /// <returns><see cref="int"/> value with specified <paramref name="bits"/> toggled.</returns>
        public static int ToggleBits(this int source, int bits)
        {
            return (source ^ bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to toggle.</param>
        /// <returns><see cref="uint"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static uint ToggleBits(this uint source, Bits bits)
        {
            checked
            {
                return ToggleBits(source, (uint)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to toggle.</param>
        /// <returns><see cref="uint"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static uint ToggleBits(this uint source, uint bits)
        {
            return (source ^ bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to toggle.</param>
        /// <returns><see cref="long"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static long ToggleBits(this long source, Bits bits)
        {
            checked
            {
                return ToggleBits(source, (long)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to toggle.</param>
        /// <returns><see cref="long"/> value with specified <paramref name="bits"/> toggled.</returns>
        public static long ToggleBits(this long source, long bits)
        {
            return (source ^ bits);
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits"><see cref="Bits"/> to toggle.</param>
        /// <returns><see cref="ulong"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static ulong ToggleBits(this ulong source, Bits bits)
        {
            checked
            {
                return ToggleBits(source, (ulong)bits);
            }
        }

        /// <summary>
        /// Returns value with specified <paramref name="bits"/> toggled.
        /// </summary>
        /// <param name="source">Value source.</param>
        /// <param name="bits">Bit-mask of the bits to toggle.</param>
        /// <returns><see cref="ulong"/> value with specified <paramref name="bits"/> toggled.</returns>
        [CLSCompliant(false)]
        public static ulong ToggleBits(this ulong source, ulong bits)
        {
            return (source ^ bits);
        }

        #endregion

        #region [ GetMaskedValue Extensions ]

        #endregion

        #region [ SetMaskedValue Extensions ]

        #endregion

        #region [ Bit Rotation Extensions ]

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        /// architectures.
        /// </remarks>
        public static byte BitRotL(this byte value, int rotations)
        {
            bool hiBitSet;

            for (int x = 1; x <= (rotations % 8); x++)
            {
                hiBitSet = value.CheckBit(7);

                value <<= 1;

                if (hiBitSet)
                    value = value.SetBit(0);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static sbyte BitRotL(this sbyte value, int rotations)
        {
            bool hiBitSet;

            for (int x = 1; x <= (rotations % 8); x++)
            {
                hiBitSet = value.CheckBit(7);

                value <<= 1;

                if (hiBitSet)
                    value = value.SetBit(0);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        /// architectures.
        /// </remarks>
        public static short BitRotL(this short value, int rotations)
        {
            bool hiBitSet;

            for (int x = 1; x <= (rotations % 16); x++)
            {
                hiBitSet = value.CheckBit(15);

                value <<= 1;

                if (hiBitSet)
                    value = value.SetBit(0);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static ushort BitRotL(this ushort value, int rotations)
        {
            bool hiBitSet;

            for (int x = 1; x <= (rotations % 16); x++)
            {
                hiBitSet = value.CheckBit(15);

                value <<= 1;

                if (hiBitSet)
                    value = value.SetBit(0);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        /// architectures.
        /// </remarks>
        public static Int24 BitRotL(this Int24 value, int rotations)
        {
            bool hiBitSet;

            for (int x = 1; x <= (rotations % 24); x++)
            {
                hiBitSet = value.CheckBit(23);

                value <<= 1;

                if (hiBitSet)
                    value = value.SetBit(0);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static UInt24 BitRotL(this UInt24 value, int rotations)
        {
            bool hiBitSet;

            for (int x = 1; x <= (rotations % 24); x++)
            {
                hiBitSet = value.CheckBit(23);

                value <<= 1;

                if (hiBitSet)
                    value = value.SetBit(0);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        /// architectures.
        /// </remarks>
        public static int BitRotL(this int value, int rotations)
        {
            bool hiBitSet;

            for (int x = 1; x <= (rotations % 32); x++)
            {
                hiBitSet = value.CheckBit(31);

                value <<= 1;

                if (hiBitSet)
                    value = value.SetBit(0);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static uint BitRotL(this uint value, int rotations)
        {
            bool hiBitSet;

            for (int x = 1; x <= (rotations % 32); x++)
            {
                hiBitSet = value.CheckBit(31);

                value <<= 1;

                if (hiBitSet)
                    value = value.SetBit(0);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        public static byte BitRotR(this byte value, int rotations)
        {
            bool loBitSet;

            for (int x = 1; x <= (rotations % 8); x++)
            {
                loBitSet = value.CheckBit(0);

                value >>= 1;

                if (loBitSet)
                    value = value.SetBit(7);
                else
                    value = value.ClearBit(7);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static sbyte BitRotR(this sbyte value, int rotations)
        {
            bool loBitSet;

            for (int x = 1; x <= (rotations % 8); x++)
            {
                loBitSet = value.CheckBit(0);

                value >>= 1;

                if (loBitSet)
                    value = value.SetBit(7);
                else
                    value = value.ClearBit(7);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        public static short BitRotR(this short value, int rotations)
        {
            bool loBitSet;

            for (int x = 1; x <= (rotations % 16); x++)
            {
                loBitSet = value.CheckBit(0);

                value >>= 1;

                if (loBitSet)
                    value = value.SetBit(15);
                else
                    value = value.ClearBit(15);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static ushort BitRotR(this ushort value, int rotations)
        {
            bool loBitSet;

            for (int x = 1; x <= (rotations % 16); x++)
            {
                loBitSet = value.CheckBit(0);

                value >>= 1;

                if (loBitSet)
                    value = value.SetBit(15);
                else
                    value = value.ClearBit(15);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        public static Int24 BitRotR(this Int24 value, int rotations)
        {
            bool loBitSet;

            for (int x = 1; x <= (rotations % 24); x++)
            {
                loBitSet = value.CheckBit(0);

                value >>= 1;

                if (loBitSet)
                    value = value.SetBit(23);
                else
                    value = value.ClearBit(23);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static UInt24 BitRotR(this UInt24 value, int rotations)
        {
            bool loBitSet;

            for (int x = 1; x <= (rotations % 24); x++)
            {
                loBitSet = value.CheckBit(0);

                value >>= 1;

                if (loBitSet)
                    value = value.SetBit(23);
                else
                    value = value.ClearBit(23);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        public static int BitRotR(this int value, int rotations)
        {
            bool loBitSet;

            for (int x = 1; x <= (rotations % 32); x++)
            {
                loBitSet = value.CheckBit(0);

                value >>= 1;

                if (loBitSet)
                    value = value.SetBit(31);
                else
                    value = value.ClearBit(31);
            }

            return value;
        }

        /// <summary>
        /// Performs rightwise bit-rotation for the specified number of rotations.
        /// </summary>
        /// <param name="value">Value used for bit-rotation.</param>
        /// <param name="rotations">Number of rotations to perform.</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times.</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static uint BitRotR(this uint value, int rotations)
        {
            bool loBitSet;

            for (int x = 1; x <= (rotations % 32); x++)
            {
                loBitSet = value.CheckBit(0);

                value >>= 1;

                if (loBitSet)
                    value = value.SetBit(31);
                else
                    value = value.ClearBit(31);
            }

            return value;
        }

        #endregion
    }
}
