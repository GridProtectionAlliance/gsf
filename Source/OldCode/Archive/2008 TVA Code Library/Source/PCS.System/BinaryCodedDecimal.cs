//*******************************************************************************************************
//  BinaryCodedDecimal.cs
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
//  04/29/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace System
{
    /// <summary>
    /// Represents functions related to binary-coded decimals.
    /// </summary>
    /// <remarks>
    /// See <a href="http://en.wikipedia.org/wiki/Binary-coded_decimal">Binary-coded decimal</a> for details.
    /// </remarks>
    [CLSCompliant(false)]
    public static class BinaryCodedDecimal
    {
        private const byte TenP1 = 10;          // 10 to the power of 1 (for one byte integer)
        private const ushort TenP2 = 100;       // 10 to the power of 2 (for two byte integer)
        private const uint TenP4 = 10000;       // 10 to the power of 4 (for four byte integer)
        private const ulong TenP8 = 100000000;  // 10 to the power of 8 (for eight byte integer)

        /// <summary>
        /// Gets binary value from binary-coded decimal.
        /// </summary>
        /// <param name="bcd">Binary-coded decimal value.</param>
        /// <returns>Standard binary representation of binary-coded decimal value.</returns>
        public static byte Decode(byte bcd)
        {
            return (byte)(bcd.HighNibble() * TenP1 + bcd.LowNibble());
        }

        /// <summary>
        /// Gets binary value from two-byte binary-coded decimal.
        /// </summary>
        /// <param name="bcd">Two-byte binary-coded decimal value.</param>
        /// <returns>Standard binary representation of binary-coded decimal value.</returns>
        public static ushort Decode(ushort bcd)
        {
            return (ushort)(Decode(bcd.HighByte()) * TenP2 + Decode(bcd.LowByte()));
        }

        /// <summary>
        /// Gets binary value from four-byte binary-coded decimal.
        /// </summary>
        /// <param name="bcd">Four-byte binary-coded decimal value.</param>
        /// <returns>Standard binary representation of binary-coded decimal value.</returns>
        public static uint Decode(uint bcd)
        {
            return (uint)(Decode(bcd.HighWord()) * TenP4 + Decode(bcd.LowWord()));
        }

        /// <summary>
        /// Gets binary value from eight-byte binary-coded decimal.
        /// </summary>
        /// <param name="bcd">Eight-byte binary-coded decimal value.</param>
        /// <returns>Standard binary representation of binary-coded decimal value.</returns>
        public static ulong Decode(ulong bcd)
        {
            return (ulong)(Decode(bcd.HighDword()) * TenP8 + Decode(bcd.LowDword()));
        }

        /// <summary>
        /// Gets binary-coded decimal from binary value.
        /// </summary>
        /// <param name="value">Binary value.</param>
        /// <returns>Binary-coded decimal representation of standard binary value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">A binary-coded decimal has a maximum value of 99 for a single byte.</exception>
        public static byte Encode(byte value)
        {
            if (value > (byte)99)
                throw new ArgumentOutOfRangeException("value", "A binary-coded decimal has a maximum value of 99 for a single byte");

            byte high = (byte)((value / TenP1) & 0x0F);
            byte low = (byte)((value % TenP1) & 0x0F);
            
            return (byte)(low + (high << 4));
        }

        /// <summary>
        /// Gets binary-coded decimal from binary value.
        /// </summary>
        /// <param name="value">Binary value.</param>
        /// <returns>Binary-coded decimal representation of standard binary value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">A binary-coded decimal has a maximum value of 9,999 for two bytes.</exception>
        public static ushort Encode(ushort value)
        {
            if (value > (ushort)9999)
                throw new ArgumentOutOfRangeException("value", "A binary-coded decimal has a maximum value of 9,999 for two bytes");

            byte high = Encode((byte)(value / TenP2));
            byte low = Encode((byte)(value % TenP2));

            return Word.MakeWord(high, low);
        }

        /// <summary>
        /// Gets binary-coded decimal from binary value.
        /// </summary>
        /// <param name="value">Binary value.</param>
        /// <returns>Binary-coded decimal representation of standard binary value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">A binary-coded decimal has a maximum value of 99,999,999 for four bytes.</exception>
        public static uint Encode(uint value)
        {
            if (value > (uint)99999999)
                throw new ArgumentOutOfRangeException("value", "A binary-coded decimal has a maximum value of 99,999,999 for four bytes");

            ushort high = Encode((ushort)(value / TenP4));
            ushort low = Encode((ushort)(value % TenP4));

            return Word.MakeDword(high, low);
        }

        /// <summary>
        /// Gets binary-coded decimal from binary value.
        /// </summary>
        /// <param name="value">Binary value.</param>
        /// <returns>Binary-coded decimal representation of standard binary value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">A binary-coded decimal has a maximum value of 9,999,999,999,999,999 for eight bytes.</exception>
        public static ulong Encode(ulong value)
        {
            if (value > (ulong)9999999999999999)
                throw new ArgumentOutOfRangeException("value", "A binary-coded decimal has a maximum value of 9,999,999,999,999,999 for eight bytes");

            uint high = Encode((uint)(value / TenP8));
            uint low = Encode((uint)(value % TenP8));

            return Word.MakeQword(high, low);
        }
    }
}
