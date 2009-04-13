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
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// Handles conversion of byte buffers to and from user presentable data formats.
    /// </summary>
    public abstract class ByteEncoding
    {
        #region [ Members ]

        // Nested Types
        #region [ Hexadecimal Encoding Class ]

        /// <summary>
        /// Handles conversion of byte buffers to and from a hexadecimal data format.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class HexadecimalEncoding : ByteEncoding
        {
            internal HexadecimalEncoding()
            {
                // This class is meant for internal instatiation only.
            }

            /// <summary>Decodes given string back into a byte buffer.</summary>
            /// <param name="hexData">Encoded hexadecimal data string to decode.</param>
            /// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
            /// <returns>Decoded bytes.</returns>
            public override byte[] GetBytes(string hexData, char spacingCharacter)
            {
                if (!string.IsNullOrEmpty(hexData))
                {
                    // Removes spacing characters, if needed.
                    hexData = hexData.Trim();
                    if (spacingCharacter != NoSpacing) hexData = hexData.Replace(spacingCharacter.ToString(), "");

                    // Processes the string only if it has data in hex format (Example: 48 65 6C 6C 21).
                    if (Regex.Matches(hexData, "[^a-fA-F0-9]").Count == 0)
                    {
                        // Trims the end of the string to discard any additional characters, if present in the string,
                        // that would prevent the string from being a hex encoded string.
                        // Note: Requires that each character be represented by its 2 character hex value.
                        hexData = hexData.Substring(0, hexData.Length - hexData.Length % 2);

                        byte[] bytes = new byte[hexData.Length / 2];
                        int index = 0;

                        for (int x = 0; x <= hexData.Length - 1; x += 2)
                        {
                            bytes[index] = Convert.ToByte(hexData.Substring(x, 2), 16);
                            index++;
                        }

                        return bytes;
                    }
                    else
                    {
                        throw new ArgumentException("Input string is not a valid hex encoded string - invalid characters encountered", "hexData");
                    }
                }
                else
                {
                    throw new ArgumentNullException("hexData", "Input string cannot be null or empty");
                }
            }

            /// <summary>Encodes given buffer into a user presentable representation.</summary>
            /// <param name="bytes">Bytes to encode.</param>
            /// <param name="offset">Offset into buffer to begin encoding.</param>
            /// <param name="length">Length of buffer to encode.</param>
            /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
            /// <returns>String of encoded bytes.</returns>
            public override string GetString(byte[] bytes, int offset, int length, char spacingCharacter)
            {
                return BytesToString(bytes, offset, length, spacingCharacter, "X2");
            }
        }

        #endregion

        #region [ Decimal Encoding Class ]

        /// <summary>
        /// Handles conversion of byte buffers to and from a decimal data format.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class DecimalEncoding : ByteEncoding
        {
            internal DecimalEncoding()
            {
                // This class is meant for internal instatiation only.
            }

            /// <summary>Decodes given string back into a byte buffer.</summary>
            /// <param name="decData">Encoded decimal data string to decode.</param>
            /// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
            /// <returns>Decoded bytes.</returns>
            public override byte[] GetBytes(string decData, char spacingCharacter)
            {
                if (!string.IsNullOrEmpty(decData))
                {
                    // Removes spacing characters, if needed.
                    decData = decData.Trim();
                    if (spacingCharacter != NoSpacing) decData = decData.Replace(spacingCharacter.ToString(), "");

                    // Processes the string only if it has data in decimal format (Example: 072 101 108 108 033).
                    if (Regex.Matches(decData, "[^0-9]").Count == 0)
                    {
                        // Trims the end of the string to discard any additional characters, if present in the
                        // string, that would prevent the string from being an integer encoded string.
                        // Note: Requires that each character be represented by its 3 character decimal value.
                        decData = decData.Substring(0, decData.Length - decData.Length % 3);

                        byte[] bytes = new byte[decData.Length / 3];
                        int index = 0;

                        for (int x = 0; x <= decData.Length - 1; x += 3)
                        {
                            bytes[index] = Convert.ToByte(decData.Substring(x, 3), 10);
                            index++;
                        }

                        return bytes;
                    }
                    else
                    {
                        throw new ArgumentException("Input string is not a valid decimal encoded string - invalid characters encountered", "decData");
                    }
                }
                else
                {
                    throw new ArgumentNullException("decData", "Input string cannot be null or empty");
                }
            }

            /// <summary>Encodes given buffer into a user presentable representation.</summary>
            /// <param name="bytes">Bytes to encode.</param>
            /// <param name="offset">Offset into buffer to begin encoding.</param>
            /// <param name="length">Length of buffer to encode.</param>
            /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
            /// <returns>String of encoded bytes.</returns>
            public override string GetString(byte[] bytes, int offset, int length, char spacingCharacter)
            {
                return BytesToString(bytes, offset, length, spacingCharacter, "D3");
            }
        }

        #endregion

        #region [ Binary Encoding Class ]

        /// <summary>
        /// Handles conversion of byte buffers to and from a binary (i.e., 0 and 1's) data format.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class BinaryEncoding : ByteEncoding
        {
            private string[] m_byteImages;
            private bool m_reverse;

            // This class is meant for internal instantiation only.
            internal BinaryEncoding(Endianness targetEndianness)
            {
                if (targetEndianness == Endianness.BigEndian)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        // If OS is little endian and we want big endian, this reverses the bit order.
                        m_reverse = true;
                    }
                    else
                    {
                        // If OS is big endian and we want big endian, this keeps the OS bit order.
                        m_reverse = false;
                    }
                }
                else
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        // If OS is little endian and we want little endian, this keeps OS bit order.
                        m_reverse = false;
                    }
                    else
                    {
                        // If OS is big endian and we want little endian, this reverses the bit order.
                        m_reverse = true;
                    }
                }
            }

            /// <summary>Decodes given string back into a byte buffer.</summary>
            /// <param name="binaryData">Encoded binary data string to decode.</param>
            /// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
            /// <returns>Decoded bytes.</returns>
            public override byte[] GetBytes(string binaryData, char spacingCharacter)
            {
                if (!string.IsNullOrEmpty(binaryData))
                {
                    // Removes spacing characters, if needed.
                    binaryData = binaryData.Trim();
                    if (spacingCharacter != NoSpacing) binaryData = binaryData.Replace(spacingCharacter.ToString(), "");

                    // Processes the string only if it has data in binary format (Example: 01010110 1010101).
                    if (Regex.Matches(binaryData, "[^0-1]").Count == 0)
                    {
                        // Trims the end of the string to discard any additional characters, if present in the
                        // string, that would prevent the string from being a binary encoded string.
                        // Note: Requires each character be represented by its 8 character binary value.
                        binaryData = binaryData.Substring(0, binaryData.Length - binaryData.Length % 8);

                        byte[] bytes = new byte[binaryData.Length / 8];
                        int index = 0;

                        for (int x = 0; x <= binaryData.Length - 1; x += 8)
                        {
                            bytes[index] = (byte)Bits.Nil;

                            if (m_reverse)
                            {
                                if (binaryData[x + 7] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit0);
                                if (binaryData[x + 6] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit1);
                                if (binaryData[x + 5] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit2);
                                if (binaryData[x + 4] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit3);
                                if (binaryData[x + 3] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit4);
                                if (binaryData[x + 2] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit5);
                                if (binaryData[x + 1] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit6);
                                if (binaryData[x + 0] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit7);
                            }
                            else
                            {
                                if (binaryData[x + 0] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit0);
                                if (binaryData[x + 1] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit1);
                                if (binaryData[x + 2] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit2);
                                if (binaryData[x + 3] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit3);
                                if (binaryData[x + 4] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit4);
                                if (binaryData[x + 5] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit5);
                                if (binaryData[x + 6] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit6);
                                if (binaryData[x + 7] == '1') bytes[index] = bytes[index].SetBits(Bits.Bit7);
                            }

                            index++;
                        }

                        return bytes;
                    }
                    else
                    {
                        throw new ArgumentException("Input string is not a valid binary encoded string - invalid characters encountered", "binaryData");
                    }
                }
                else
                {
                    throw new ArgumentNullException("binaryData", "Input string cannot be null or empty");
                }
            }

            /// <summary>Encodes given buffer into a user presentable representation.</summary>
            /// <param name="bytes">Bytes to encode.</param>
            /// <param name="offset">Offset into buffer to begin encoding.</param>
            /// <param name="length">Length of buffer to encode.</param>
            /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
            /// <returns>String of encoded bytes.</returns>
            public override string GetString(byte[] bytes, int offset, int length, char spacingCharacter)
            {
                if (bytes == null) throw new ArgumentNullException("bytes", "Input buffer cannot be null");

                // Initializes byte image array on first call for speed in future calls.
                if (m_byteImages == null)
                {
                    StringBuilder byteImage;

                    m_byteImages = new string[256];

                    for (int imageByte = byte.MinValue; imageByte <= byte.MaxValue; imageByte++)
                    {
                        byteImage = new StringBuilder();

                        if (m_reverse)
                        {
                            if (imageByte.CheckBits(Bits.Bit7)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit6)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit5)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit4)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit3)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit2)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit1)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit0)) byteImage.Append('1'); else byteImage.Append('0');
                        }
                        else
                        {
                            if (imageByte.CheckBits(Bits.Bit0)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit1)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit2)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit3)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit4)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit5)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit6)) byteImage.Append('1'); else byteImage.Append('0');
                            if (imageByte.CheckBits(Bits.Bit7)) byteImage.Append('1'); else byteImage.Append('0');
                        }

                        m_byteImages[imageByte] = byteImage.ToString();
                    }
                }

                StringBuilder binaryImage = new StringBuilder();

                for (int x = 0; x <= length - 1; x++)
                {
                    if (spacingCharacter != NoSpacing && x > 0) binaryImage.Append(spacingCharacter);
                    binaryImage.Append(m_byteImages[bytes[offset + x]]);
                }

                return binaryImage.ToString();
            }
        }

        #endregion

        #region [ Base64 Encoding Class ]

        /// <summary>
        /// Handles conversion of byte buffers to and from a base64 data format.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class Base64Encoding : ByteEncoding
        {
            internal Base64Encoding()
            {
                // This class is meant for internal instantiation only.
            }

            /// <summary>Decodes given string back into a byte buffer.</summary>
            /// <param name="binaryData">Encoded binary data string to decode.</param>
            /// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
            /// <returns>Decoded bytes.</returns>
            public override byte[] GetBytes(string binaryData, char spacingCharacter)
            {
                // Removes spacing characters, if needed.
                binaryData = binaryData.Trim();
                if (spacingCharacter != NoSpacing) binaryData = binaryData.Replace(spacingCharacter.ToString(), "");
                return Convert.FromBase64String(binaryData);
            }

            /// <summary>Encodes given buffer into a user presentable representation.</summary>
            /// <param name="bytes">Bytes to encode.</param>
            /// <param name="offset">Offset into buffer to begin encoding.</param>
            /// <param name="length">Length of buffer to encode.</param>
            /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
            /// <returns>String of encoded bytes.</returns>
            public override string GetString(byte[] bytes, int offset, int length, char spacingCharacter)
            {
                if (bytes == null) throw new ArgumentNullException("bytes", "Input buffer cannot be null");

                string base64String = Convert.ToBase64String(bytes, offset, length);

                if (spacingCharacter == NoSpacing)
                {
                    return base64String;
                }
                else
                {
                    StringBuilder base64Image = new StringBuilder();

                    for (int x = 0; x <= base64String.Length - 1; x++)
                    {
                        if (x > 0) base64Image.Append(spacingCharacter);
                        base64Image.Append(base64String[x]);
                    }

                    return base64Image.ToString();
                }
            }
        }

        #endregion

        #region [ ASCII Encoding Class ]

        /// <summary>
        /// Handles conversion of byte buffers to and from a ASCII data format.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class ASCIIEncoding : ByteEncoding
        {
            internal ASCIIEncoding()
            {
                // This class is meant for internal instantiation only.
            }

            /// <summary>Decodes given string back into a byte buffer.</summary>
            /// <param name="binaryData">Encoded binary data string to decode.</param>
            /// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
            /// <returns>Decoded bytes.</returns>
            public override byte[] GetBytes(string binaryData, char spacingCharacter)
            {
                // Removes spacing characters, if needed.
                binaryData = binaryData.Trim();
                if (spacingCharacter != NoSpacing) binaryData = binaryData.Replace(spacingCharacter.ToString(), "");
                return Encoding.ASCII.GetBytes(binaryData);
            }

            /// <summary>Encodes given buffer into a user presentable representation.</summary>
            /// <param name="bytes">Bytes to encode.</param>
            /// <param name="offset">Offset into buffer to begin encoding.</param>
            /// <param name="length">Length of buffer to encode.</param>
            /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
            /// <returns>String of encoded bytes.</returns>
            public override string GetString(byte[] bytes, int offset, int length, char spacingCharacter)
            {
                if (bytes == null) throw new ArgumentNullException("bytes", "Input buffer cannot be null");

                string asciiString = Encoding.ASCII.GetString(bytes, offset, length);

                if (spacingCharacter == NoSpacing)
                {
                    return asciiString;
                }
                else
                {
                    StringBuilder asciiImage = new StringBuilder();

                    for (int x = 0; x <= asciiString.Length - 1; x++)
                    {
                        if (x > 0) asciiImage.Append(spacingCharacter);
                        asciiImage.Append(asciiString[x]);
                    }

                    return asciiImage.ToString();
                }
            }
        }

        #endregion

        /// <summary>
        /// Constant used to specify that "no spacing" should be used for data conversion.
        /// </summary>
        public const char NoSpacing = char.MinValue;

        #endregion

        #region [ Methods ]

        /// <summary>Encodes given buffer into a user presentable representation.</summary>
        /// <param name="bytes">Bytes to encode.</param>
        /// <returns>String representation of byte array.</returns>
        public virtual string GetString(byte[] bytes)
        {
            return GetString(bytes, NoSpacing);
        }

        /// <summary>Encodes given buffer into a user presentable representation.</summary>
        /// <param name="bytes">Bytes to encode.</param>
        /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
        /// <returns>String of encoded bytes.</returns>
        public virtual string GetString(byte[] bytes, char spacingCharacter)
        {
            if (bytes == null) throw new ArgumentNullException("bytes", "Input buffer cannot be null");
            return GetString(bytes, 0, bytes.Length, spacingCharacter);
        }

        /// <summary>Encodes given buffer into a user presentable representation.</summary>
        /// <param name="bytes">Bytes to encode.</param>
        /// <param name="offset">Offset into buffer to begin encoding.</param>
        /// <param name="length">Length of buffer to encode.</param>
        /// <returns>String of encoded bytes.</returns>
        public virtual string GetString(byte[] bytes, int offset, int length)
        {
            if (bytes == null) throw new ArgumentNullException("bytes", "Input buffer cannot be null");
            return GetString(bytes, offset, length, NoSpacing);
        }

        /// <summary>Encodes given buffer into a user presentable representation.</summary>
        /// <param name="bytes">Bytes to encode.</param>
        /// <param name="offset">Offset into buffer to begin encoding.</param>
        /// <param name="length">Length of buffer to encode.</param>
        /// <param name="spacingCharacter">Spacing character to place between encoded bytes.</param>
        /// <returns>String of encoded bytes.</returns>
        public abstract string GetString(byte[] bytes, int offset, int length, char spacingCharacter);

        /// <summary>Decodes given string back into a byte buffer.</summary>
        /// <param name="value">Encoded string to decode.</param>
        /// <returns>Decoded bytes.</returns>
        public virtual byte[] GetBytes(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value", "Input string cannot be null");
            return GetBytes(value, NoSpacing);
        }

        /// <summary>Decodes given string back into a byte buffer.</summary>
        /// <param name="value">Encoded string to decode.</param>
        /// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
        /// <returns>Decoded bytes</returns>
        public abstract byte[] GetBytes(string value, char spacingCharacter);

        /// <summary>Handles byte to string conversions for implementations that are available from Byte.ToString.</summary>
        /// <param name="bytes">Encoded string to decode.</param>
        /// <param name="offset">Offset into byte array to begin decoding straing at.</param>
        /// <param name="length">Number of bytes to decode starting at <paramref name="offset"/></param>
        /// <param name="spacingCharacter">Character to insert between each byte</param>
        /// <param name="format">String decoding format.</param>
        /// <returns>Decoded string</returns>
        protected string BytesToString(byte[] bytes, int offset, int length, char spacingCharacter, string format)
        {
            if (bytes == null) throw new ArgumentNullException("bytes", "Input buffer cannot be null");

            StringBuilder byteString = new StringBuilder();

            for (int x = 0; x <= length - 1; x++)
            {
                if (spacingCharacter != NoSpacing && x > 0) byteString.Append(spacingCharacter);
                byteString.Append(bytes[x + offset].ToString(format));
            }

            return byteString.ToString();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static ByteEncoding m_hexadecimalEncoding;
        private static ByteEncoding m_decimalEncoding;
        private static ByteEncoding m_bigEndianBinaryEncoding;
        private static ByteEncoding m_littleEndianBinaryEncoding;
        private static ByteEncoding m_base64Encoding;
        private static ByteEncoding m_asciiEncoding;

        /// <summary>Handles encoding and decoding of a byte buffer into a hexadecimal-based presentation format.</summary>
        public static ByteEncoding Hexadecimal
        {
            get
            {
                if (m_hexadecimalEncoding == null) m_hexadecimalEncoding = new HexadecimalEncoding();
                return m_hexadecimalEncoding;
            }
        }

        /// <summary>Handles encoding and decoding of a byte buffer into an integer-based presentation format.</summary>
        public static ByteEncoding Decimal
        {
            get
            {
                if (m_decimalEncoding == null) m_decimalEncoding = new DecimalEncoding();
                return m_decimalEncoding;
            }
        }

        /// <summary>Handles encoding and decoding of a byte buffer into a big-endian binary (i.e., 0 and 1's) based
        /// presentation format.</summary>
        /// <remarks>
        /// Although endianness is typically used in the context of byte order (see PCS.Interop.EndianOrder to handle byte
        /// order swapping), this property allows you visualize "bits" in big-endian order, right-to-left (Note that bits
        /// are normally stored in the same order as their bytes.).
        /// </remarks>
        public static ByteEncoding BigEndianBinary
        {
            get
            {
                if (m_bigEndianBinaryEncoding == null) m_bigEndianBinaryEncoding = new BinaryEncoding(Endianness.BigEndian);
                return m_bigEndianBinaryEncoding;
            }
        }

        /// <summary>Handles encoding and decoding of a byte buffer into a little-endian binary (i.e., 0 and 1's) based
        /// presentation format.</summary>
        /// <remarks>
        /// Although endianness is typically used in the context of byte order (see PCS.Interop.EndianOrder to handle byte
        /// order swapping), this property allows you visualize "bits" in little-endian order, left-to-right (Note that bits
        /// are normally stored in the same order as their bytes.).
        /// </remarks>
        public static ByteEncoding LittleEndianBinary
        {
            get
            {
                if (m_littleEndianBinaryEncoding == null) m_littleEndianBinaryEncoding = new BinaryEncoding(Endianness.LittleEndian);
                return m_littleEndianBinaryEncoding;
            }
        }

        /// <summary>Handles encoding and decoding of a byte buffer into a base64 presentation format.</summary>
        public static ByteEncoding Base64
        {
            get
            {
                if (m_base64Encoding == null) m_base64Encoding = new Base64Encoding();
                return m_base64Encoding;
            }
        }

        /// <summary>Handles encoding and decoding of a byte buffer into an ASCII character presentation format.</summary>
        public static ByteEncoding ASCII
        {
            get
            {
                if (m_asciiEncoding == null) m_asciiEncoding = new ASCIIEncoding();
                return m_asciiEncoding;
            }
        }

        #endregion
    }
}