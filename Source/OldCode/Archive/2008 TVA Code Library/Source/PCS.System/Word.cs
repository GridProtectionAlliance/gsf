//*******************************************************************************************************
//  Word.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/10/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    /// Represents functions and extensions related to 16-bit words, 32-bit double words and 64-bit quad words.
    /// </summary>
    [CLSCompliant(false)]
    public static class Word
    {
        /// <summary>
        /// Returns the high-byte from an unsigned word (UInt16).
        /// </summary>
        /// <param name="word">2-byte, 16-bit unsigned integer value.</param>
        /// <returns>The high-order byte of the specified 16-bit unsigned integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the byte value whose in-memory representation
        /// is the same as the right-most, most-significant-byte of the integer value.
        /// </remarks>
        public static byte HighByte(this ushort word)
        {
            return (byte)((word & (ushort)0xFF00) >> 8);
        }

        /// <summary>
        /// Returns the unsigned high-word (UInt16) from an unsigned double-word (UInt32).
        /// </summary>
        /// <param name="doubleWord">4-byte, 32-bit unsigned integer value.</param>
        /// <returns>The unsigned high-order word of the specified 32-bit unsigned integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the word value
        /// whose in-memory representation is the same as the right-most, most-significant-word
        /// of the integer value.
        /// </remarks>
        public static ushort HighWord(this uint doubleWord)
        {
            return (ushort)((doubleWord & (uint)0xFFFF0000) >> 16);
        }

        /// <summary>
        /// Returns the unsigned high-double-word (UInt32) from an unsigned quad-word (UInt64).
        /// </summary>
        /// <param name="quadWord">8-byte, 64-bit unsigned integer value.</param>
        /// <returns>The high-order double-word of the specified 64-bit unsigned integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the word value
        /// whose in-memory representation is the same as the right-most, most-significant-word
        /// of the integer value.
        /// </remarks>
        public static uint HighDword(this ulong quadWord)
        {
            return (uint)((quadWord & (ulong)0xFFFFFFFF00000000) >> 32);
        }

        /// <summary>
        /// Returns the low-byte from an unsigned word (UInt16).
        /// </summary>
        /// <param name="word">2-byte, 16-bit unsigned integer value.</param>
        /// <returns>The low-order byte of the specified 16-bit unsigned integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the byte value
        /// whose in-memory representation is the same as the left-most, least-significant-byte
        /// of the integer value.
        /// </remarks>
        public static byte LowByte(this ushort word)
        {
            return (byte)(word & (ushort)0x00FF);
        }

        /// <summary>
        /// Returns the unsigned low-word (UInt16) from an unsigned double-word (UInt32).
        /// </summary>
        /// <param name="doubleWord">4-byte, 32-bit unsigned integer value.</param>
        /// <returns>The unsigned low-order word of the specified 32-bit unsigned integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the word value
        /// whose in-memory representation is the same as the left-most, least-significant-word
        /// of the integer value.
        /// </remarks>
        public static ushort LowWord(this uint doubleWord)
        {
            return (ushort)(doubleWord & (uint)0x0000FFFF);
        }

        /// <summary>
        /// Returns the unsigned low-double-word (UInt32) from an unsigned quad-word (UInt64).
        /// </summary>
        /// <param name="quadWord">8-byte, 64-bit unsigned integer value.</param>
        /// <returns>The low-order double-word of the specified 64-bit unsigned integer value.</returns>
        /// <remarks>
        /// On little-endian architectures (e.g., Intel platforms), this will be the word value
        /// whose in-memory representation is the same as the left-most, least-significant-word
        /// of the integer value.
        /// </remarks>
        public static uint LowDword(this ulong quadWord)
        {
            return (uint)(quadWord & (ulong)0x00000000FFFFFFFF);
        }

        /// <summary>
        /// Makes an unsigned word (UInt16) from two bytes.
        /// </summary>
        /// <param name="high">High byte.</param>
        /// <param name="low">Low byte.</param>
        /// <returns>An unsigned 16-bit word made from the two specified bytes.</returns>
        public static ushort MakeWord(byte high, byte low)
        {
            return (ushort)(low + ((ushort)high << 8));
        }

        /// <summary>
        /// Makes an unsigned double-word (UInt32) from two unsigned words (UInt16).
        /// </summary>
        /// <param name="high">High word.</param>
        /// <param name="low">Low word.</param>
        /// <returns>An unsigned 32-bit double-word made from the two specified unsigned 16-bit words.</returns>
        public static uint MakeDword(ushort high, ushort low)
        {
            return (uint)(low + ((uint)high << 16));
        }

        /// <summary>
        /// Makes an unsigned quad-word (UInt64) from two unsigned double-words (UInt32).
        /// </summary>
        /// <param name="high">High double-word.</param>
        /// <param name="low">Low double-word.</param>
        /// <returns>An unsigned 64-bit quad-word made from the two specified unsigned 32-bit double-words.</returns>
        public static ulong MakeQword(uint high, uint low)
        {
            return (ulong)(low + ((ulong)high << 32));
        }
    }
}