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
//      Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    namespace Interop
    {
        public sealed class Bit
        {
            private Bit()
            {
                // This class contains only global functions and is not meant to be instantiated
            }

            /// <summary>No bits set (8-bit)</summary>
            public const byte Nill = 0x0;

            /// <summary>No bits set (16-bit)</summary>
            public const short Nill16 = 0x0;

            /// <summary>No bits set (32-bit)</summary>
            public const int Nill32 = 0x0;

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
            public const short Bit15 = (short)0x00008000;

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
            public const int Bit31 = (int)0x80000000;

            /// <summary>Performs leftwise bit-rotation for the specified number of rotations</summary>
            /// <param name="value">Value used for bit-rotation</param>
            /// <param name="rotations">Number of rotations to perform</param>
            /// <returns>Value that has its bits rotated to the left the specified number of times</returns>
            /// <remarks>
            /// Actual rotation direction is from a big-endian perspective - this is an artifact of the native
            /// .NET bit shift operators. As a result bits may actually appear to rotate right on little-endian
            /// architectures.
            /// </remarks>
            public static byte BitRotL(byte value, int rotations)
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
            public static short BitRotL(short value, int rotations)
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
            public static Int24 BitRotL(Int24 value, int rotations)
			{				
				bool hiBitSet;
				Int24 int24Bit0 = 0x1;
                Int24 int24Bit23 = (Int24)8388608;
				
				for (int x = 1; x <= (rotations % 24); x++)
				{
					hiBitSet = ((value & int24Bit23) == int24Bit23);
					value <<= 1;
					if (hiBitSet) value |= int24Bit0;
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
            public static int BitRotL(int value, int rotations)
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
            public static byte BitRotR(byte value, int rotations)
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
            public static short BitRotR(short value, int rotations)
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
            public static Int24 BitRotR(Int24 value, int rotations)
			{				
				bool loBitSet;
				Int24 int24Bit0 = 0x1;
                Int24 int24Bit23 = (Int24)8388608;
				
				for (int x = 1; x <= (rotations % 24); x++)
				{
					loBitSet = ((value & int24Bit0) == int24Bit0);
					value >>= 1;					
					if (loBitSet) value |= int24Bit23; else value &= ~int24Bit23;
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
            public static int BitRotR(int value, int rotations)
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

            /// <summary>Returns the high-byte from a word (Int16).</summary>
            /// <param name="word">2-byte, 16-bit signed integer value.</param>
            /// <returns>The high-order byte of the specified 16-bit signed integer value.</returns>
            /// <remarks>
            /// On little-endian architectures (e.g., Intel platforms), this will be the byte value whose in-memory representation
            /// is the same as the right-most, most-significant-byte of the integer value.
            /// </remarks>
            public static byte HiByte(short word)
            {
                if (BitConverter.IsLittleEndian)
                {
                    return BitConverter.GetBytes(word)[1];
                }
                else
                {
                    return BitConverter.GetBytes(word)[0];
                }
            }

            /// <summary>Returns the high-word (Int16) from a double-word (Int32).</summary>
            /// <param name="doubleWord">4-byte, 32-bit signed integer value.</param>
            /// <returns>The high-order word of the specified 32-bit signed integer value.</returns>
            /// <remarks>
            /// On little-endian architectures (e.g., Intel platforms), this will be the word value
            /// whose in-memory representation is the same as the right-most, most-significant-word
            /// of the integer value.
            /// </remarks>
            public static short HiWord(int doubleWord)
            {
                if (BitConverter.IsLittleEndian)
                {
                    return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 2);
                }
                else
                {
                    return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 0);
                }
           }

            /// <summary>Returns the low-byte from a word (Int16).</summary>
            /// <param name="word">2-byte, 16-bit signed integer value.</param>
            /// <returns>The low-order byte of the specified 16-bit signed integer value.</returns>
            /// <remarks>
            /// On little-endian architectures (e.g., Intel platforms), this will be the byte value
            /// whose in-memory representation is the same as the left-most, least-significant-byte
            /// of the integer value.
            /// </remarks>
            public static byte LoByte(short word)
            {
                if (BitConverter.IsLittleEndian)
                {
                    return BitConverter.GetBytes(word)[0];
                }
                else
                {
                    return BitConverter.GetBytes(word)[1];
                }
            }

            /// <summary>Returns the low-word (Int16) from a double-word (Int32).</summary>
            /// <param name="doubleWord">4-byte, 32-bit signed integer value.</param>
            /// <returns>The low-order word of the specified 32-bit signed integer value.</returns>
            /// <remarks>
            /// On little-endian architectures (e.g., Intel platforms), this will be the word value
            /// whose in-memory representation is the same as the left-most, least-significant-word
            /// of the integer value.
            /// </remarks>
            public static short LoWord(int doubleWord)
            {
                if (BitConverter.IsLittleEndian)
                {
                    return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 0);
                }
                else
                {
                    return BitConverter.ToInt16(BitConverter.GetBytes(doubleWord), 2);
                }
            }

            /// <summary>Makes a word (Int16) from two bytes.</summary>
            /// <returns>A 16-bit word made from the two specified bytes.</returns>
            public static short MakeWord(byte high, byte low)
            {
                if (BitConverter.IsLittleEndian)
                {
                    return BitConverter.ToInt16(new byte[] { low, high }, 0);
                }
                else
                {
                    return BitConverter.ToInt16(new byte[] { high, low }, 0);
                }
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
        }
    }
}