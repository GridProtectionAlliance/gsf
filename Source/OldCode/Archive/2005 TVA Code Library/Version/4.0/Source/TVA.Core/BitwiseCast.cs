/**************************************************************************\
   Copyright © 2009 - Gbtc, James Ritchie Carroll
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

using System;

namespace TVA
{
    /// <summary>Defines specialized bitwise integer data type conversion functions</summary>
    /// <remarks>
    /// This class allows for proper bitwise casting between signed and unsigned integers. It may be most
    /// useful in languages that do not allow override of numerical overflow checks.  For example, C#
    /// provides an "unchecked" keyword to allow for bitwise casting, but VB.NET does not.
    /// </remarks>
    public static class BitwiseCast
    {
        /// <summary>Performs proper bitwise conversion between unsigned and signed value</summary>
        /// <remarks>
        /// <para>This function is useful because Convert.ToInt16 will throw an OverflowException for values greater than Int16.MaxValue.</para>
        /// <para>For example, this function correctly converts unsigned 16-bit integer 65535 (i.e., UInt16.MaxValue) to signed 16-bit integer -1.</para>
        /// <param name="unsignedInt">Unsigned short that is passed in to be converted to a signed short by a bitwise conversion.</param>
        /// <returns>The converted short value.</returns>
        /// </remarks>
        [CLSCompliant(false)]
        public static short ToInt16(ushort unsignedInt)
        {
            //return BitConverter.ToInt16(BitConverter.GetBytes(unsignedInt), 0);
            unchecked
            {
                return (short)unsignedInt;
            }
        }

        /// <summary>Performs proper bitwise conversion between unsigned and signed value</summary>
        /// <remarks>
        /// <para>This function is useful because CType(n, Int24) will throw an OverflowException for values greater than Int24.MaxValue.</para>
        /// <para>For example, this function correctly converts unsigned 24-bit integer 16777215 (i.e., UInt24.MaxValue) to signed 24-bit integer -1.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public static Int24 ToInt24(UInt24 unsignedInt)
        {
            return Int24.GetValue(UInt24.GetBytes(unsignedInt), 0);
        }

        /// <summary>Performs proper bitwise conversion between unsigned and signed value</summary>
        /// <remarks>
        /// <para>This function is useful because Convert.ToInt32 will throw an OverflowException for values greater than Int32.MaxValue.</para>
        /// <para>For example, this function correctly converts unsigned 32-bit integer 4294967295 (i.e., UInt32.MaxValue) to signed 32-bit integer -1.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public static int ToInt32(uint unsignedInt)
        {
            //return BitConverter.ToInt32(BitConverter.GetBytes(unsignedInt), 0);
            unchecked
            {
                return (int)unsignedInt;
            }
        }

        /// <summary>Performs proper bitwise conversion between unsigned and signed value</summary>
        /// <remarks>
        /// <para>This function is useful because Convert.ToInt64 will throw an OverflowException for values greater than Int64.MaxValue.</para>
        /// <para>For example, this function correctly converts unsigned 64-bit integer 18446744073709551615 (i.e., UInt64.MaxValue) to signed 64-bit integer -1.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public static long ToInt64(ulong unsignedInt)
        {
            //return BitConverter.ToInt64(BitConverter.GetBytes(unsignedInt), 0);
            unchecked
            {
                return (long)unsignedInt;
            }
        }

        /// <summary>Performs proper bitwise conversion between signed and unsigned value</summary>
        /// <remarks>
        /// <para>This function is useful because Convert.ToUInt16 will throw an OverflowException for values less than zero.</para>
        /// <para>For example, this function correctly converts signed 16-bit integer -32768 (i.e., Int16.MinValue) to unsigned 16-bit integer 32768.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public static ushort ToUInt16(short signedInt)
        {
            //return BitConverter.ToUInt16(BitConverter.GetBytes(signedInt), 0);
            unchecked
            {
                return (ushort)signedInt;
            }
        }

        /// <summary>Performs proper bitwise conversion between signed and unsigned value</summary>
        /// <remarks>
        /// <para>This function is useful because CType(n, UInt24) will throw an OverflowException for values less than zero.</para>
        /// <para>For example, this function correctly converts signed 24-bit integer -8388608 (i.e., Int24.MinValue) to unsigned 24-bit integer 8388608.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public static UInt24 ToUInt24(Int24 signedInt)
        {
            return UInt24.GetValue(Int24.GetBytes(signedInt), 0);
        }

        /// <summary>Performs proper bitwise conversion between signed and unsigned value</summary>
        /// <remarks>
        /// <para>This function is useful because Convert.ToUInt32 will throw an OverflowException for values less than zero.</para>
        /// <para>For example, this function correctly converts signed 32-bit integer -2147483648 (i.e., Int32.MinValue) to unsigned 32-bit integer 2147483648.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public static uint ToUInt32(int signedInt)
        {
            //return BitConverter.ToUInt32(BitConverter.GetBytes(signedInt), 0);
            unchecked
            {
                return (uint)signedInt;
            }
        }

        /// <summary>Performs proper bitwise conversion between signed and unsigned value</summary>
        /// <remarks>
        /// <para>This function is useful because Convert.ToUInt64 will throw an OverflowException for values less than zero.</para>
        /// <para>For example, this function correctly converts signed 64-bit integer -9223372036854775808 (i.e., Int64.MinValue) to unsigned 64-bit integer 9223372036854775808.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public static ulong ToUInt64(long signedInt)
        {
            //return BitConverter.ToUInt64(BitConverter.GetBytes(signedInt), 0);
            unchecked
            {
                return (ulong)signedInt;
            }
        }
    }
}
