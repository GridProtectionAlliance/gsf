//*******************************************************************************************************
//  TVA.Interop.Bit.vb - Bit Manipulation Functions
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
//  02/24/2004 - J. Ritchie Carroll
//       Original version of source code generated
//  01/14/2005 - J. Ritchie Carroll
//       Moved bit constants into Bit class - made sense to me :p
//       Deprecated LShiftWord and RShiftWord since VB now supports << and >> operators
//       Converted other functions to use standard .NET bit conversion operations, this will
//           be more reliable and more OS portable than having to deal with the "sign" bit
//           as the older code was doing...
//  12/29/2005 - Pinal C. Patel
//       2.0 version of source code migrated from 1.1 source (TVA.Shared.Bit)
//  01/04/2006 - J. Ritchie Carroll
//       Added code comments - moved into Interop namespace
//  10/10/2007 - J. Ritchie Carroll
//       Added bit-rotation functions (BitRotL and BitRotR)
//  09/08/2008 - J. Ritchie Carroll
//      Converted to C# (some available as extensions).
//
//*******************************************************************************************************

using System;

namespace TVA.Interop
{
    public static class Bit
    {
        /// <summary>No bits set (8-bit)</summary>
        public const byte Nill = 0x0;

        // Byte 0, Bits 0-7

        /// <summary>Bit 0 (0x00000001)</summary>
        public const byte Bit0 = 0x00000001;    // 00000001 = 1

        /// <summary>Bit 1 (0x00000002)</summary>
        public const byte Bit1 = 0x00000002;    // 00000010 = 2

        /// <summary>Bit 2 (0x00000004)</summary>
        public const byte Bit2 = 0x00000004;    // 00000100 = 4

        /// <summary>Bit 3 (0x00000008)</summary>
        public const byte Bit3 = 0x00000008;    // 00001000 = 8

        /// <summary>Bit 4 (0x00000010)</summary>
        public const byte Bit4 = 0x00000010;    // 00010000 = 16

        /// <summary>Bit 6 (0x00000020)</summary>
        public const byte Bit5 = 0x00000020;    // 00100000 = 32

        /// <summary>Bit 6 (0x00000040)</summary>
        public const byte Bit6 = 0x00000040;    // 01000000 = 64

        /// <summary>Bit 7 (0x00000080)</summary>
        public const byte Bit7 = 0x00000080;    // 10000000 = 128

        // Byte 1, Bits 8-15

        /// <summary>Bit 8 (0x00000100)</summary>
        public const short Bit8 = 0x00000100;

        /// <summary>Bit 9 (0x00000200)</summary>
        public const short Bit9 = 0x00000200;

        /// <summary>Bit 10 (0x00000400)</summary>
        public const short Bit10 = 0x00000400;

        /// <summary>Bit 11 (0x00000800)</summary>
        public const short Bit11 = 0x00000800;

        /// <summary>Bit 12 (0x00001000)</summary>
        public const short Bit12 = 0x00001000;

        /// <summary>Bit 13 (0x00002000)</summary>
        public const short Bit13 = 0x00002000;

        /// <summary>Bit 14 (0x00004000)</summary>
        public const short Bit14 = 0x00002000;

        /// <summary>Bit 15 (0x00008000)</summary>
        public const short Bit15 = -32768; //0x00008000;

        // Byte 2, Bits 16-23

        /// <summary>Bit 16 (0x00010000)</summary>
        public const int Bit16 = 0x00010000;

        /// <summary>Bit 17 (0x00020000)</summary>
        public const int Bit17 = 0x00020000;

        /// <summary>Bit 18 (0x00040000)</summary>
        public const int Bit18 = 0x00040000;

        /// <summary>Bit 19 (0x00080000)</summary>
        public const int Bit19 = 0x00080000;

        /// <summary>Bit 20 (0x00100000)</summary>
        public const int Bit20 = 0x00100000;

        /// <summary>Bit 21 (0x00200000)</summary>
        public const int Bit21 = 0x00200000;

        /// <summary>Bit 22 (0x00400000)</summary>
        public const int Bit22 = 0x00400000;

        /// <summary>Bit 23 (0x00800000)</summary>
        public const int Bit23 = 0x00800000;

        // Byte 3, Bits 24-31

        /// <summary>Bit 24 (0x01000000)</summary>
        public const int Bit24 = 0x01000000;

        /// <summary>Bit 25 (0x02000000)</summary>
        public const int Bit25 = 0x02000000;

        /// <summary>Bit 26 (0x04000000)</summary>
        public const int Bit26 = 0x04000000;

        /// <summary>Bit 27 (0x08000000)</summary>
        public const int Bit27 = 0x08000000;

        /// <summary>Bit 28 (0x10000000)</summary>
        public const int Bit28 = 0x10000000;

        /// <summary>Bit 29 (0x20000000)</summary>
        public const int Bit29 = 0x20000000;

        /// <summary>Bit 30 (0x40000000)</summary>
        public const int Bit30 = 0x40000000;

        /// <summary>Bit 31 (0x80000000)</summary>
        public const int Bit31 = -2147483648; // 0x80000000;

        /// <summary>Gets the bit value for the specified bit index (0 - 31).</summary>
        /// <param name="bit">Bit index (0 - 31)</param>
        /// <returns>Value of the specified bit.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Parameter must be between 0 and 31.</exception>
        public static int BitVal(int bit)
        {
            switch (bit)
            {
                #region [ Bit Cases (0 - 31) ]

                case 00: return Bit0;
                case 01: return Bit1;
                case 02: return Bit2;
                case 03: return Bit3;
                case 04: return Bit4;
                case 05: return Bit5;
                case 06: return Bit6;
                case 07: return Bit7;
                case 08: return Bit8;
                case 09: return Bit9;
                case 10: return Bit10;
                case 11: return Bit11;
                case 12: return Bit12;
                case 13: return Bit13;
                case 14: return Bit14;
                case 15: return Bit15;
                case 16: return Bit16;
                case 17: return Bit17;
                case 18: return Bit18;
                case 19: return Bit19;
                case 20: return Bit20;
                case 21: return Bit21;
                case 22: return Bit22;
                case 23: return Bit23;
                case 24: return Bit24;
                case 25: return Bit25;
                case 26: return Bit26;
                case 27: return Bit27;
                case 28: return Bit28;
                case 29: return Bit29;
                case 30: return Bit30;
                case 31: return Bit31;

                #endregion

                default:
                    throw new ArgumentOutOfRangeException("bit", "Parameter must be between 0 and 31.");
            }
        }

        /// <summary>Determines if specified bit is set.</summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 7) to check.</param>
        /// <returns>True if specified bit is set in source value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Parameter must be between 0 and 7 for a 8-bit source value.</exception>
        public static bool BitIsSet(this byte source, int bit)
        {
            if (bit < 0 || bit > 7)
                throw new ArgumentOutOfRangeException("bit", "Parameter must be between 0 and 7 for an 8-bit source value.");

            return ((source & BitVal(bit)) > 0);
        }

        /// <summary>Determines if specified bit is set.</summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 15) to check.</param>
        /// <returns>True if specified bit is set in source value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Parameter must be between 0 and 15 for a 16-bit source value.</exception>
        public static bool BitIsSet(this short source, int bit)
        {
            if (bit < 0 || bit > 15)
                throw new ArgumentOutOfRangeException("bit", "Parameter must be between 0 and 15 for a 16-bit source value.");

            return ((source & BitVal(bit)) > 0);
        }

        /// <summary>Determines if specified bit is set.</summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 15) to check.</param>
        /// <returns>True if specified bit is set in source value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Parameter must be between 0 and 15 for a 16-bit source value.</exception>
        [CLSCompliant(false)]
        public static bool BitIsSet(this ushort source, int bit)
        {
            if (bit < 0 || bit > 15)
                throw new ArgumentOutOfRangeException("bit", "Parameter must be between 0 and 15 for a 16-bit source value.");

            return ((source & BitVal(bit)) > 0);
        }

        /// <summary>Determines if specified bit is set.</summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 31) to check.</param>
        /// <returns>True if specified bit is set in source value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Parameter must be between 0 and 31 for a 32-bit source value.</exception>
        public static bool BitIsSet(this int source, int bit)
        {
            if (bit < 0 || bit > 31)
                throw new ArgumentOutOfRangeException("bit", "Parameter must be between 0 and 31 for a 32-bit source value.");

            return ((source & BitVal(bit)) > 0);
        }

        /// <summary>Determines if specified bit is set.</summary>
        /// <param name="source">Value source.</param>
        /// <param name="bit">Bit index (0 - 31) to check.</param>
        /// <returns>True if specified bit is set in source value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Parameter must be between 0 and 31 for a 32-bit source value.</exception>
        [CLSCompliant(false)]
        public static bool BitIsSet(this uint source, int bit)
        {
            if (bit < 0 || bit > 31)
                throw new ArgumentOutOfRangeException("bit", "Parameter must be between 0 and 31 for a 32-bit source value.");

            return ((source & BitVal(bit)) > 0);
        }

        /// <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the left the specified number of times</returns>
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
                hiBitSet = ((value & Bit7) == Bit7);
                value <<= 1;
                if (hiBitSet) value |= Bit0;
            }

            return value;
        }

        /// <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the left the specified number of times</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static sbyte BitRotL(this sbyte value, int rotations)
        {
            bool hiBitSet;
            sbyte bit0 = (sbyte)Bit0;

            for (int x = 1; x <= (rotations % 8); x++)
            {
                hiBitSet = ((value & Bit7) == Bit7);
                value <<= 1;
                if (hiBitSet) value |= bit0;
            }

            return value;
        }

        /// <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the left the specified number of times</returns>
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
                hiBitSet = ((value & Bit15) == Bit15);
                value <<= 1;
                if (hiBitSet) value |= Bit0;
            }

            return value;
        }

        /// <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the left the specified number of times</returns>
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
                hiBitSet = ((value & Bit15) == Bit15);
                value <<= 1;
                if (hiBitSet) value |= Bit0;
            }

            return value;
        }

        /// <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the left the specified number of times</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        /// architectures.
        /// </remarks>
        public static Int24 BitRotL(this Int24 value, int rotations)
        {
            bool hiBitSet;
            Int24 bit0 = Bit0;

            for (int x = 1; x <= (rotations % 24); x++)
            {
                hiBitSet = ((value & Bit23) == Bit23);
                value <<= 1;
                if (hiBitSet) value |= bit0;
            }

            return value;
        }

        /// <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the left the specified number of times</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static UInt24 BitRotL(this UInt24 value, int rotations)
        {
            bool hiBitSet;
            UInt24 bit0 = Bit0;

            for (int x = 1; x <= (rotations % 24); x++)
            {
                hiBitSet = ((value & Bit23) == Bit23);
                value <<= 1;
                if (hiBitSet) value |= bit0;
            }

            return value;
        }

        /// <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the left the specified number of times</returns>
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
                hiBitSet = ((value & Bit31) == Bit31);
                value <<= 1;
                if (hiBitSet) value |= Bit0;
            }

            return value;
        }

        /// <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the left the specified number of times</returns>
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
                hiBitSet = ((value & Bit31) == Bit31);
                value <<= 1;
                if (hiBitSet) value |= Bit0;
            }

            return value;
        }

        /// <summary>Performs rightwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times</returns>
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
                loBitSet = ((value & Bit0) == Bit0);
                value >>= 1;
                if (loBitSet) value |= Bit7; else value = (byte)(value & ~Bit7);
            }

            return value;
        }

        /// <summary>Performs rightwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static sbyte BitRotR(this sbyte value, int rotations)
        {
            unchecked
            {
                bool loBitSet;
                sbyte bit7 = (sbyte)Bit7;

                for (int x = 1; x <= (rotations % 8); x++)
                {
                    loBitSet = ((value & Bit0) == Bit0);
                    value >>= 1;
                    if (loBitSet) value |= bit7; else value &= (sbyte)(value & ~bit7);
                }
            }

            return value;
        }

        /// <summary>Performs rightwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times</returns>
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
                loBitSet = ((value & Bit0) == Bit0);
                value >>= 1;
                if (loBitSet) value |= Bit15; else value &= ~Bit15;
            }

            return value;
        }

        /// <summary>Performs rightwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static ushort BitRotR(this ushort value, int rotations)
        {
            unchecked
            {
                bool loBitSet;
                ushort bit15 = (ushort)Bit15;

                for (int x = 1; x <= (rotations % 16); x++)
                {
                    loBitSet = ((value & Bit0) == Bit0);
                    value >>= 1;
                    if (loBitSet) value |= bit15; else value = (ushort)(value & ~bit15);
                }
            }

            return value;
        }

        /// <summary>Performs rightwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        public static Int24 BitRotR(this Int24 value, int rotations)
        {
            bool loBitSet;
            Int24 bit23 = (Int24)Bit23;

            for (int x = 1; x <= (rotations % 24); x++)
            {
                loBitSet = ((value & Bit0) == Bit0);
                value >>= 1;
                if (loBitSet) value |= bit23; else value &= ~bit23;
            }

            return value;
        }

        /// <summary>Performs rightwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static UInt24 BitRotR(this UInt24 value, int rotations)
        {
            bool loBitSet;
            UInt24 bit23 = (UInt24)Bit23;

            for (int x = 1; x <= (rotations % 24); x++)
            {
                loBitSet = ((value & Bit0) == Bit0);
                value >>= 1;
                if (loBitSet) value |= bit23; else value &= ~bit23;
            }

            return value;
        }

        /// <summary>Performs rightwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times</returns>
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
                loBitSet = ((value & Bit0) == Bit0);
                value >>= 1;
                if (loBitSet) value |= Bit31; else value &= ~Bit31;
            }

            return value;
        }

        /// <summary>Performs rightwise bit-rotation for the specified number of rotations</summary>
        /// <param name="value">Value used for bit-rotation</param>
        /// <param name="rotations">Number of rotations to perform</param>
        /// <returns>Value that has its bits rotated to the right the specified number of times</returns>
        /// <remarks>
        /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
        /// .NET bit shift operators. As a result bits may actually appear to rotate left on little-endian
        /// architectures.
        /// </remarks>
        [CLSCompliant(false)]
        public static uint BitRotR(this uint value, int rotations)
        {
            unchecked
            {
                bool loBitSet;
                uint bit31 = (uint)Bit31;

                for (int x = 1; x <= (rotations % 32); x++)
                {
                    loBitSet = ((value & Bit0) == Bit0);
                    value >>= 1;
                    if (loBitSet) value |= bit31; else value &= ~bit31;
                }
            }

            return value;
        }

        /// <summary>Returns the high-byte from a word (Int16).</summary>
        /// <param name="word">2-byte, 16-bit signed integer value.</param>
        /// <returns>The high-order byte of the specified 16-bit signed integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the byte value whose in-memory representation
        /// is the same as the right-most, most-significant-byte of the integer value.
        /// </remarks>
        public static byte HiByte(this short word)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.GetBytes(word)[1];
            else
                return BitConverter.GetBytes(word)[0];
        }

        /// <summary>Returns the high-byte from an unsigned word (UInt16).</summary>
        /// <param name="word">2-byte, 16-bit unsigned integer value.</param>
        /// <returns>The high-order byte of the specified 16-bit unsigned integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the byte value whose in-memory representation
        /// is the same as the right-most, most-significant-byte of the integer value.
        /// </remarks>
        [CLSCompliant(false)]
        public static byte HiByte(this ushort word)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.GetBytes(word)[1];
            else
                return BitConverter.GetBytes(word)[0];
        }

        /// <summary>Returns the high-word (Int16) from a double-word (Int32).</summary>
        /// <param name="doubleWord">4-byte, 32-bit signed integer value.</param>
        /// <returns>The high-order word of the specified 32-bit signed integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the word value
        /// whose in-memory representation is the same as the right-most, most-significant-word
        /// of the integer value.
        /// </remarks>
        public static short HiWord(this int doubleWord)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 2);
            else
                return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 0);
        }

        /// <summary>Returns the unsigned high-word (UInt16) from an unsigned double-word (UInt32).</summary>
        /// <param name="doubleWord">4-byte, 32-bit unsigned integer value.</param>
        /// <returns>The unsigned high-order word of the specified 32-bit unsigned integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the word value
        /// whose in-memory representation is the same as the right-most, most-significant-word
        /// of the integer value.
        /// </remarks>
        [CLSCompliant(false)]
        public static ushort HiWord(this uint doubleWord)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToUInt16(BitConverter.GetBytes(doubleWord), 2);
            else
                return BitConverter.ToUInt16(BitConverter.GetBytes(doubleWord), 0);
        }

        /// <summary>Returns the low-byte from a word (Int16).</summary>
        /// <param name="word">2-byte, 16-bit signed integer value.</param>
        /// <returns>The low-order byte of the specified 16-bit signed integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the byte value
        /// whose in-memory representation is the same as the left-most, least-significant-byte
        /// of the integer value.
        /// </remarks>
        public static byte LoByte(this short word)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.GetBytes(word)[0];
            else
                return BitConverter.GetBytes(word)[1];
        }

        /// <summary>Returns the low-byte from an unsigned word (UInt16).</summary>
        /// <param name="word">2-byte, 16-bit unsigned integer value.</param>
        /// <returns>The low-order byte of the specified 16-bit unsigned integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the byte value
        /// whose in-memory representation is the same as the left-most, least-significant-byte
        /// of the integer value.
        /// </remarks>
        [CLSCompliant(false)]
        public static byte LoByte(this ushort word)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.GetBytes(word)[0];
            else
                return BitConverter.GetBytes(word)[1];
        }

        /// <summary>Returns the low-word (Int16) from a double-word (Int32).</summary>
        /// <param name="doubleWord">4-byte, 32-bit signed integer value.</param>
        /// <returns>The low-order word of the specified 32-bit signed integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the word value
        /// whose in-memory representation is the same as the left-most, least-significant-word
        /// of the integer value.
        /// </remarks>
        public static short LoWord(this int doubleWord)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 0);
            else
                return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 2);
        }

        /// <summary>Returns the unsigned low-word (UInt16) from an unsigned double-word (UInt32).</summary>
        /// <param name="doubleWord">4-byte, 32-bit unsigned integer value.</param>
        /// <returns>The unsigned low-order word of the specified 32-bit unsigned integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the word value
        /// whose in-memory representation is the same as the left-most, least-significant-word
        /// of the integer value.
        /// </remarks>
        [CLSCompliant(false)]
        public static ushort LoWord(this uint doubleWord)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToUInt16(BitConverter.GetBytes(doubleWord), 0);
            else
                return BitConverter.ToUInt16(BitConverter.GetBytes(doubleWord), 2);
        }

        /// <summary>Makes a word (Int16) from two bytes.</summary>
        /// <returns>A 16-bit word made from the two specified bytes.</returns>
        public static short MakeWord(byte high, byte low)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToInt16(new byte[] { low, high }, 0);
            else
                return BitConverter.ToInt16(new byte[] { high, low }, 0);
        }

        /// <summary>Makes an unsigned word (UInt16) from two bytes.</summary>
        /// <returns>An unsigned 16-bit word made from the two specified bytes.</returns>
        [CLSCompliant(false)]
        public static ushort MakeUWord(byte high, byte low)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToUInt16(new byte[] { low, high }, 0);
            else
                return BitConverter.ToUInt16(new byte[] { high, low }, 0);
        }

        /// <summary>Makes a double-word (Int32) from two words (Int16).</summary>
        /// <returns>A 32-bit double-word made from the two specified 16-bit words.</returns>
        public static int MakeDWord(short high, short low)
        {
            byte[] bytes = new byte[4];

            if (BitConverter.IsLittleEndian)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(low), 0, bytes, 0, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(high), 0, bytes, 2, 2);
            }
            else
            {
                Buffer.BlockCopy(BitConverter.GetBytes(high), 0, bytes, 0, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(low), 0, bytes, 2, 2);
            }

            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>Makes an unsigned double-word (UInt32) from two unsigned words (UInt16).</summary>
        /// <returns>An unsigned 32-bit double-word made from the two specified unsigned 16-bit words.</returns>
        [CLSCompliant(false)]
        public static uint MakeUDWord(ushort high, ushort low)
        {
            byte[] bytes = new byte[4];

            if (BitConverter.IsLittleEndian)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(low), 0, bytes, 0, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(high), 0, bytes, 2, 2);
            }
            else
            {
                Buffer.BlockCopy(BitConverter.GetBytes(high), 0, bytes, 0, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(low), 0, bytes, 2, 2);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}