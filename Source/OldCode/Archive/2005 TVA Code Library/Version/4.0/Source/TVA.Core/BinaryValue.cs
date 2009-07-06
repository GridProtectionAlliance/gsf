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
    /// <summary>
    /// Represents a binary data sample stored as a byte array ordered in the
    /// endianness of the OS, but implicitly castable to most common native types.
    /// </summary>
    public class BinaryValue : BinaryValueBase<NativeEndianOrder>
    {
        #region [ Constructors ]

        /// <summary>Creates a new binary value, ordered in the endianness of the OS, from the given byte array.</summary>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        /// <param name="startIndex">The offset in the buffer where the data starts.</param>
        /// <param name="length">The number of data bytes that make up the binary value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of the <paramref name="buffer"/> -or-
        /// <paramref name="length"/> is less than 0 -or-
        /// <paramref name="startIndex"/> and <paramref name="length"/> do not specify a valid region in the <paramref name="buffer"/>
        /// </exception>
        /// <remarks>This constructor assumes a type code of Empty to represent "undefined".</remarks>
        public BinaryValue(byte[] buffer, int startIndex, int length)
            : base(TypeCode.Empty, buffer, startIndex, length)
        {
        }

        /// <summary>Creates a new binary value, ordered in the endianness of the OS, from the given byte array.</summary>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        /// <remarks>This constructor assumes a type code of Empty to represent "undefined".</remarks>
        public BinaryValue(byte[] buffer)
            : base(TypeCode.Empty, buffer, 0, buffer.Length)
        {
        }

        /// <summary>Creates a new binary value, ordered in the endianness of the OS, from the given byte array.</summary>
        /// <param name="typeCode">The type code of the native value that the binary value represents.</param>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        /// <param name="startIndex">The offset in the buffer where the data starts.</param>
        /// <param name="length">The number of data bytes that make up the binary value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of the <paramref name="buffer"/> -or-
        /// <paramref name="length"/> is less than 0 -or-
        /// <paramref name="startIndex"/> and <paramref name="length"/> do not specify a valid region in the <paramref name="buffer"/>
        /// </exception>
        public BinaryValue(TypeCode typeCode, byte[] buffer, int startIndex, int length)
            : base(typeCode, buffer, startIndex, length)
        {
        }

        /// <summary>Creates a new binary value, ordered in the endianness of the OS, from the given byte array.</summary>
        /// <param name="typeCode">The type code of the native value that the binary value represents.</param>
        /// <param name="buffer">The buffer which contains the binary representation of the value.</param>
        public BinaryValue(TypeCode typeCode, byte[] buffer)
            : base(typeCode, buffer, 0, buffer.Length)
        {
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="BinaryValue"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="BinaryValue"/>.</returns>
        public override string ToString()
        {
            return ((Double)ConvertToType(TypeCode.Double)).ToString();
        }

        /// <summary>
        /// Returns a <see cref="BinaryValue"/> representation of source value converted to specified <see cref="TypeCode"/>.
        /// </summary>
        /// <param name="typeCode">Desired <see cref="TypeCode"/> for destination value.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of source value converted to specified <see cref="TypeCode"/>.</returns>
        /// <exception cref="InvalidOperationException">Unable to convert binary value to specified type.</exception>
        public BinaryValue ConvertToType(TypeCode typeCode)
        {
            switch (this.TypeCode)
            {
                case TypeCode.Byte:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return ToByte();
                        case TypeCode.Double:
                            return (Double)ToByte();
                        case TypeCode.Int16:
                            return (Int16)ToByte();
                        case TypeCode.Int32:
                            return (Int32)ToByte();
                        case TypeCode.Int64:
                            return (Int64)ToByte();
                        case TypeCode.Single:
                            return (Single)ToByte();
                        case TypeCode.UInt16:
                            return (UInt16)ToByte();
                        case TypeCode.UInt32:
                            return (UInt32)ToByte();
                        case TypeCode.UInt64:
                            return (UInt64)ToByte();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.Int16:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToInt16();
                        case TypeCode.Double:
                            return (Double)ToInt16();
                        case TypeCode.Int16:
                            return ToInt16();
                        case TypeCode.Int32:
                            return (Int32)ToInt16();
                        case TypeCode.Single:
                            return (Single)ToInt16();
                        case TypeCode.Int64:
                            return (Int64)ToInt16();
                        case TypeCode.UInt16:
                            return (UInt16)ToInt16();
                        case TypeCode.UInt32:
                            return (UInt32)ToInt16();
                        case TypeCode.UInt64:
                            return (UInt64)ToInt16();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.Int32:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToInt32();
                        case TypeCode.Double:
                            return (Double)ToInt32();
                        case TypeCode.Int16:
                            return (Int16)ToInt32();
                        case TypeCode.Int32:
                            return ToInt32();
                        case TypeCode.Int64:
                            return (Int64)ToInt32();
                        case TypeCode.Single:
                            return (Single)ToInt32();
                        case TypeCode.UInt16:
                            return (UInt16)ToInt32();
                        case TypeCode.UInt32:
                            return (UInt32)ToInt32();
                        case TypeCode.UInt64:
                            return (UInt64)ToInt32();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.Int64:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToInt64();
                        case TypeCode.Double:
                            return (Double)ToInt64();
                        case TypeCode.Int16:
                            return (Int16)ToInt64();
                        case TypeCode.Int32:
                            return (Int32)ToInt64();
                        case TypeCode.Single:
                            return (Single)ToInt64();
                        case TypeCode.Int64:
                            return ToInt64();
                        case TypeCode.UInt16:
                            return (UInt16)ToInt64();
                        case TypeCode.UInt32:
                            return (UInt32)ToInt64();
                        case TypeCode.UInt64:
                            return (UInt64)ToInt64();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.Single:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToSingle();
                        case TypeCode.Double:
                            return (Double)ToSingle();
                        case TypeCode.Int16:
                            return (Int16)ToSingle();
                        case TypeCode.Int32:
                            return (Int32)ToSingle();
                        case TypeCode.Int64:
                            return (Int64)ToSingle();
                        case TypeCode.Single:
                            return ToSingle();
                        case TypeCode.UInt16:
                            return (UInt16)ToSingle();
                        case TypeCode.UInt32:
                            return (UInt32)ToSingle();
                        case TypeCode.UInt64:
                            return (UInt64)ToSingle();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.Double:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToDouble();
                        case TypeCode.Double:
                            return ToDouble();
                        case TypeCode.Int16:
                            return (Int16)ToDouble();
                        case TypeCode.Int32:
                            return (Int32)ToDouble();
                        case TypeCode.Int64:
                            return (Int64)ToDouble();
                        case TypeCode.Single:
                            return (Single)ToDouble();
                        case TypeCode.UInt16:
                            return (UInt16)ToDouble();
                        case TypeCode.UInt32:
                            return (UInt32)ToDouble();
                        case TypeCode.UInt64:
                            return (UInt64)ToDouble();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.UInt16:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToUInt16();
                        case TypeCode.Double:
                            return (Double)ToUInt16();
                        case TypeCode.Int16:
                            return (Int16)ToUInt16();
                        case TypeCode.Int32:
                            return (Int32)ToUInt16();
                        case TypeCode.Int64:
                            return (Int64)ToUInt16();
                        case TypeCode.Single:
                            return (Single)ToUInt16();
                        case TypeCode.UInt16:
                            return ToUInt16();
                        case TypeCode.UInt32:
                            return (UInt32)ToUInt16();
                        case TypeCode.UInt64:
                            return (UInt64)ToUInt16();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.UInt32:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToUInt32();
                        case TypeCode.Double:
                            return (Double)ToUInt32();
                        case TypeCode.Int16:
                            return (Int16)ToUInt32();
                        case TypeCode.Int32:
                            return (Int32)ToUInt32();
                        case TypeCode.Int64:
                            return (Int64)ToUInt32();
                        case TypeCode.Single:
                            return (Single)ToUInt32();
                        case TypeCode.UInt16:
                            return (UInt16)ToUInt32();
                        case TypeCode.UInt32:
                            return ToUInt32();
                        case TypeCode.UInt64:
                            return (UInt64)ToUInt32();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                case TypeCode.UInt64:
                    switch (typeCode)
                    {
                        case TypeCode.Byte:
                            return (Byte)ToUInt64();
                        case TypeCode.Double:
                            return (Double)ToUInt64();
                        case TypeCode.Int16:
                            return (Int16)ToUInt64();
                        case TypeCode.Int32:
                            return (Int32)ToUInt64();
                        case TypeCode.Int64:
                            return (Int64)ToUInt64();
                        case TypeCode.Single:
                            return (Single)ToUInt64();
                        case TypeCode.UInt16:
                            return (UInt16)ToUInt64();
                        case TypeCode.UInt32:
                            return (UInt32)ToUInt64();
                        case TypeCode.UInt64:
                            return ToUInt64();
                        default:
                            throw new InvalidOperationException("Unable to convert binary value to " + typeCode);
                    }
                default:
                    throw new InvalidOperationException("Unable to convert binary value from " + this.TypeCode);
            }
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Byte"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Byte"/>.</param>
        /// <returns>A <see cref="Byte"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Byte(BinaryValue value)
        {
            return value.ToByte();
        }

        /// <summary>
        /// Implicitly converts <see cref="Byte"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Byte"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Byte"/>.</returns>
        public static implicit operator BinaryValue(Byte value)
        {
            return new BinaryValue(TypeCode.Byte, new byte[] { value });
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Int16"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Int16"/>.</param>
        /// <returns>A <see cref="Int16"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Int16(BinaryValue value)
        {
            return value.ToInt16();
        }

        /// <summary>
        /// Implicitly converts <see cref="Int16"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Int16"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Int16"/>.</returns>
        public static implicit operator BinaryValue(Int16 value)
        {
            return new BinaryValue(TypeCode.Int16, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="UInt16"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="UInt16"/>.</param>
        /// <returns>A <see cref="UInt16"/> representation of <see cref="BinaryValue"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator UInt16(BinaryValue value)
        {
            return value.ToUInt16();
        }

        /// <summary>
        /// Implicitly converts <see cref="UInt16"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="UInt16"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="UInt16"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt16 value)
        {
            return new BinaryValue(TypeCode.UInt16, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Int24"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Int24"/>.</param>
        /// <returns>A <see cref="Int24"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Int24(BinaryValue value)
        {
            return value.ToInt24();
        }

        /// <summary>
        /// Implicitly converts <see cref="Int24"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Int24"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Int24"/>.</returns>
        public static implicit operator BinaryValue(Int24 value)
        {
            return new BinaryValue(TypeCode.Empty, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="UInt24"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="UInt24"/>.</param>
        /// <returns>A <see cref="UInt24"/> representation of <see cref="BinaryValue"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator UInt24(BinaryValue value)
        {
            return value.ToUInt24();
        }

        /// <summary>
        /// Implicitly converts <see cref="UInt24"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="UInt24"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="UInt24"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt24 value)
        {
            return new BinaryValue(TypeCode.Empty, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Int32"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Int32"/>.</param>
        /// <returns>A <see cref="Int32"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Int32(BinaryValue value)
        {
            return value.ToInt32();
        }

        /// <summary>
        /// Implicitly converts <see cref="Int32"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Int32"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Int32"/>.</returns>
        public static implicit operator BinaryValue(Int32 value)
        {
            return new BinaryValue(TypeCode.Int32, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="UInt32"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="UInt32"/>.</param>
        /// <returns>A <see cref="UInt32"/> representation of <see cref="BinaryValue"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator UInt32(BinaryValue value)
        {
            return value.ToUInt32();
        }

        /// <summary>
        /// Implicitly converts <see cref="UInt32"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="UInt32"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="UInt32"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt32 value)
        {
            return new BinaryValue(TypeCode.UInt32, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Int64"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Int64"/>.</param>
        /// <returns>A <see cref="Int64"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Int64(BinaryValue value)
        {
            return value.ToInt64();
        }

        /// <summary>
        /// Implicitly converts <see cref="Int64"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Int64"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Int64"/>.</returns>
        public static implicit operator BinaryValue(Int64 value)
        {
            return new BinaryValue(TypeCode.Int64, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="UInt64"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="UInt64"/>.</param>
        /// <returns>A <see cref="UInt64"/> representation of <see cref="BinaryValue"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator UInt64(BinaryValue value)
        {
            return value.ToUInt64();
        }

        /// <summary>
        /// Implicitly converts <see cref="UInt64"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="UInt64"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="UInt64"/>.</returns>
        [CLSCompliant(false)]
        public static implicit operator BinaryValue(UInt64 value)
        {
            return new BinaryValue(TypeCode.UInt64, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Single"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Single"/>.</param>
        /// <returns>A <see cref="Single"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Single(BinaryValue value)
        {
            return value.ToSingle();
        }

        /// <summary>
        /// Implicitly converts <see cref="Single"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Single"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Single"/>.</returns>
        public static implicit operator BinaryValue(Single value)
        {
            return new BinaryValue(TypeCode.Single, m_endianOrder.GetBytes(value));
        }

        /// <summary>
        /// Implicitly converts <see cref="BinaryValue"/> to <see cref="Double"/>.
        /// </summary>
        /// <param name="value"><see cref="BinaryValue"/> to convert to <see cref="Double"/>.</param>
        /// <returns>A <see cref="Double"/> representation of <see cref="BinaryValue"/>.</returns>
        public static implicit operator Double(BinaryValue value)
        {
            return value.ToDouble();
        }

        /// <summary>
        /// Implicitly converts <see cref="Double"/> to <see cref="BinaryValue"/>.
        /// </summary>
        /// <param name="value"><see cref="Double"/> to convert to <see cref="BinaryValue"/>.</param>
        /// <returns>A <see cref="BinaryValue"/> representation of <see cref="Double"/>.</returns>
        public static implicit operator BinaryValue(Double value)
        {
            return new BinaryValue(TypeCode.Double, m_endianOrder.GetBytes(value));
        }

        #endregion

        #region [ Static ]

        static BinaryValue()
        {
            m_endianOrder = NativeEndianOrder.Default;
        }

        #endregion
    }
}
