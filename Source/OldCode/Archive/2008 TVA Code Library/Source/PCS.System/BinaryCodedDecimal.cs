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
    /// Represents functions related to binary-coded decimals (or BCD).
    /// </summary>
    [CLSCompliant(false)]
    public static class BinaryCodedDecimal
    {
        /// <summary>
        /// Gets binary value from binary-coded decimal.
        /// </summary>
        /// <param name="bcd">Binary-coded decimal value.</param>
        /// <returns>Standard binary representation of binary-coded decimal value.</returns>
        public static byte Decode(byte bcd)
        {
            return (byte)(bcd.HighNibble() * 10U + bcd.LowNibble());
        }

        /// <summary>
        /// Gets binary value from two-byte binary-coded decimal.
        /// </summary>
        /// <param name="bcd">Two-byte binary-coded decimal value.</param>
        /// <returns>Standard binary representation of binary-coded decimal value.</returns>
        public static ushort Decode(ushort bcd)
        {
            return (ushort)(Decode(bcd.HighByte()) * 100U + Decode(bcd.LowByte()));
        }

        /// <summary>
        /// Gets binary value from four-byte binary-coded decimal.
        /// </summary>
        /// <param name="bcd">Four-byte binary-coded decimal value.</param>
        /// <returns>Standard binary representation of binary-coded decimal value.</returns>
        public static uint Decode(uint bcd)
        {
            return Decode(bcd.HighWord()) * 10000U + Decode(bcd.LowWord());
        }

        /// <summary>
        /// Gets binary value from eight-byte binary-coded decimal.
        /// </summary>
        /// <param name="bcd">Eight-byte binary-coded decimal value.</param>
        /// <returns>Standard binary representation of binary-coded decimal value.</returns>
        public static ulong Decode(ulong bcd)
        {
            return Decode(bcd.HighDword()) * 100000000U + Decode(bcd.LowDword());
        }

        /// <summary>
        /// Gets binary-coded decimal from binary value.
        /// </summary>
        /// <param name="value">Binary value.</param>
        /// <param name="bcd">Binary-coded decimal result.</param>
        /// <returns>Binary-coded decimal representation of standard binary value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">A binary-coded decimal has a maximum value of 99 for a single byte.</exception>
        public static void Encode(byte value, out byte bcd)
        {
            if (value > 99U)
                throw new ArgumentOutOfRangeException("value", "A binary-coded decimal has a maximum value of 99 for a single byte");

            byte high =(byte)((value / 10U) & 0x0F);
            byte low = (byte)((value % 10U) & 0x0F);
            
            bcd = (byte)(low + (high << 4));
        }

        /// <summary>
        /// Gets binary-coded decimal from binary value.
        /// </summary>
        /// <param name="value">Binary value.</param>
        /// <param name="bcd">Binary-coded decimal result.</param>
        /// <returns>Binary-coded decimal representation of standard binary value.</returns>
        public static void Encode(byte value, out ushort bcd)
        {
            byte high = (byte)(value / 100U);
            byte low = (byte)(value % 100U);

            bcd = Word.MakeWord(high, low);
        }

        /// <summary>
        /// Gets binary-coded decimal from binary value.
        /// </summary>
        /// <param name="value">Binary value.</param>
        /// <param name="bcd">Binary-coded decimal result.</param>
        /// <returns>Binary-coded decimal representation of standard binary value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">A binary-coded decimal has a maximum value of 9,999 for two bytes.</exception>
        public static void Encode(ushort value, out ushort bcd)
        {
            if (value > 9999U)
                throw new ArgumentOutOfRangeException("value", "A binary-coded decimal has a maximum value of 9,999 for two bytes");

            byte high = (byte)(value / 100U);
            byte low = (byte)(value % 100U);

            bcd = Word.MakeWord(high, low);
        }

        /// <summary>
        /// Gets binary-coded decimal from binary value.
        /// </summary>
        /// <param name="value">Binary value.</param>
        /// <param name="bcd">Binary-coded decimal result.</param>
        /// <returns>Binary-coded decimal representation of standard binary value.</returns>
        public static void Encode(ushort value, out uint bcd)
        {
            ushort high = (ushort)(value / 10000U);
            ushort low = (ushort)(value % 10000U);

            bcd = Word.MakeDword(high, low);
        }

        /// <summary>
        /// Gets binary-coded decimal from binary value.
        /// </summary>
        /// <param name="value">Binary value.</param>
        /// <param name="bcd">Binary-coded decimal result.</param>
        /// <returns>Binary-coded decimal representation of standard binary value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">A binary-coded decimal has a maximum value of 99,999,999 for four bytes.</exception>
        public static void Encode(uint value, out uint bcd)
        {
            if (value > 99999999U)
                throw new ArgumentOutOfRangeException("value", "A binary-coded decimal has a maximum value of 99,999,999 for four bytes");

            ushort high = (ushort)(value / 10000U);
            ushort low = (ushort)(value % 10000U);

            bcd = Word.MakeDword(high, low);
        }

        /// <summary>
        /// Gets binary-coded decimal from binary value.
        /// </summary>
        /// <param name="value">Binary value.</param>
        /// <param name="bcd">Binary-coded decimal result.</param>
        /// <returns>Binary-coded decimal representation of standard binary value.</returns>
        public static void Encode(uint value, out ulong bcd)
        {
            uint high = (uint)(value / 100000000U);
            uint low = (uint)(value % 100000000U);

            bcd = Word.MakeQword(high, low);
        }

        /// <summary>
        /// Gets binary-coded decimal from binary value.
        /// </summary>
        /// <param name="value">Binary value.</param>
        /// <param name="bcd">Binary-coded decimal result.</param>
        /// <returns>Binary-coded decimal representation of standard binary value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">A binary-coded decimal has a maximum value of 9,999,999,999,999,999 for eight bytes.</exception>
        public static void Encode(ulong value, out ulong bcd)
        {
            if (value > 9999999999999999U)
                throw new ArgumentOutOfRangeException("value", "A binary-coded decimal has a maximum value of 9,999,999,999,999,999 for eight bytes");

            uint high = (uint)(value / 100000000U);
            uint low = (uint)(value % 100000000U);

            bcd = Word.MakeQword(high, low);
        }
    }
}
