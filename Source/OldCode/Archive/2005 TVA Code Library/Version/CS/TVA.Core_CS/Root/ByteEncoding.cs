using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
//using TVA.Common;
using TVA.Interop;
using TVA.Interop.Bit;

//*******************************************************************************************************
//  TVA.ByteEncoding.vb - Byte encoding functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/29/2005 - Pinal C. Patel
//       Generated original version of source code.
//  04/05/2006 - J. Ritchie Carroll
//       Transformed code into System.Text.Encoding styled hierarchy.
//  02/13/2007 - J. Ritchie Carroll
//       Added ASCII character extraction as an encoding option.
//  04/30/2007 - J. Ritchie Carroll
//       Cached binary bit images when first called to optimize generation.
//  12/13/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************


/// <summary>Handles conversion of byte buffers to and from user presentable data formats.</summary>
namespace TVA
{
	public abstract class ByteEncoding
	{
		
		
		#region " Hexadecimal Encoding Class "
		
		[EditorBrowsable(EditorBrowsableState.Never)]public class HexadecimalEncoding : ByteEncoding
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
				
				if (! string.IsNullOrEmpty(hexData))
				{
					// Removes spacing characters, if needed.
					hexData = hexData.Trim();
					if (spacingCharacter != NoSpacing)
					{
						hexData = hexData.Replace(spacingCharacter, "");
					}
					
					// Processes the string only if it has data in hex format (Example: 48 65 6C 6C 21).
					if (Regex.Matches(hexData, "[^a-fA-F0-9]").Count() == 0)
					{
						// Trims the end of the string to discard any additional characters, if present in the string, that
						// would prevent the string from being a hex encoded string.
						// Note: Requires that each character be represented by its 2 character hex value.
						hexData = hexData.Substring(0, hexData.Length - hexData.Length % 2);
						
						byte[] bytes = TVA.Common.CreateArray<byte>(hexData.Length / 2);
						int index;
						
						for (int x = 0; x <= hexData.Length - 1; x += 2)
						{
							bytes[index] = Convert.ToByte(hexData.Substring(x, 2), 16);
							index++;
						}
						
						return bytes;
					}
					else
					{
						throw (new ArgumentException("Input string is not a valid hex encoded string - invalid characters encountered", "hexData"));
					}
				}
				else
				{
					throw (new ArgumentNullException("hexData", "Input string cannot be null or empty"));
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
		
		#region " Decimal Encoding Class "
		
		[EditorBrowsable(EditorBrowsableState.Never)]public class DecimalEncoding : ByteEncoding
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
				
				if (! string.IsNullOrEmpty(decData))
				{
					// Removes spacing characters, if needed.
					decData = decData.Trim();
					if (spacingCharacter != NoSpacing)
					{
						decData = decData.Replace(spacingCharacter, "");
					}
					
					// Processes the string only if it has data in decimal format (Example: 072 101 108 108 033).
					if (Regex.Matches(decData, "[^0-9]").Count() == 0)
					{
						// Trims the end of the string to discard any additional characters, if present in the
						// string, that would prevent the string from being an integer encoded string.
						// Note: Requires that each character be represented by its 3 character decimal value.
						decData = decData.Substring(0, decData.Length - decData.Length % 3);
						
						byte[] bytes = TVA.Common.CreateArray<byte>(decData.Length / 3);
						int index;
						
						for (int x = 0; x <= decData.Length - 1; x += 3)
						{
							bytes[index] = Convert.ToByte(decData.Substring(x, 3), 10);
							index++;
						}
						
						return bytes;
					}
					else
					{
						throw (new ArgumentException("Input string is not a valid decimal encoded string - invalid characters encountered", "decData"));
					}
				}
				else
				{
					throw (new ArgumentNullException("decData", "Input string cannot be null or empty"));
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
		
		#region " Binary Encoding Class "
		
		[EditorBrowsable(EditorBrowsableState.Never)]public class BinaryEncoding : ByteEncoding
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
				
				if (! string.IsNullOrEmpty(binaryData))
				{
					// Removes spacing characters, if needed.
					binaryData = binaryData.Trim();
					if (spacingCharacter != NoSpacing)
					{
						binaryData = binaryData.Replace(spacingCharacter, "");
					}
					
					// Processes the string only if it has data in binary format (Example: 01010110 1010101).
					if (Regex.Matches(binaryData, "[^0-1]").Count() == 0)
					{
						// Trims the end of the string to discard any additional characters, if present in the
						// string, that would prevent the string from being a binary encoded string.
						// Note: Requires each character be represented by its 8 character binary value.
						binaryData = binaryData.Substring(0, binaryData.Length - binaryData.Length % 8);
						
						byte[] bytes = TVA.Common.CreateArray<byte>(binaryData.Length / 8);
						int index;
						
						for (int x = 0; x <= binaryData.Length - 1; x += 8)
						{
							bytes[index] = Nill;
							
							if (m_reverse)
							{
								if (binaryData[x + 7] == '1')
								{
									bytes[index] = bytes[index] || Bit0;
								}
								if (binaryData[x + 6] == '1')
								{
									bytes[index] = bytes[index] || Bit1;
								}
								if (binaryData[x + 5] == '1')
								{
									bytes[index] = bytes[index] || Bit2;
								}
								if (binaryData[x + 4] == '1')
								{
									bytes[index] = bytes[index] || Bit3;
								}
								if (binaryData[x + 3] == '1')
								{
									bytes[index] = bytes[index] || Bit4;
								}
								if (binaryData[x + 2] == '1')
								{
									bytes[index] = bytes[index] || Bit5;
								}
								if (binaryData[x + 1] == '1')
								{
									bytes[index] = bytes[index] || Bit6;
								}
								if (binaryData[x + 0] == '1')
								{
									bytes[index] = bytes[index] || Bit7;
								}
							}
							else
							{
								if (binaryData[x + 0] == '1')
								{
									bytes[index] = bytes[index] || Bit0;
								}
								if (binaryData[x + 1] == '1')
								{
									bytes[index] = bytes[index] || Bit1;
								}
								if (binaryData[x + 2] == '1')
								{
									bytes[index] = bytes[index] || Bit2;
								}
								if (binaryData[x + 3] == '1')
								{
									bytes[index] = bytes[index] || Bit3;
								}
								if (binaryData[x + 4] == '1')
								{
									bytes[index] = bytes[index] || Bit4;
								}
								if (binaryData[x + 5] == '1')
								{
									bytes[index] = bytes[index] || Bit5;
								}
								if (binaryData[x + 6] == '1')
								{
									bytes[index] = bytes[index] || Bit6;
								}
								if (binaryData[x + 7] == '1')
								{
									bytes[index] = bytes[index] || Bit7;
								}
							}
							
							index++;
						}
						
						return bytes;
					}
					else
					{
						throw (new ArgumentException("Input string is not a valid binary encoded string - invalid characters encountered", "binaryData"));
					}
				}
				else
				{
					throw (new ArgumentNullException("binaryData", "Input string cannot be null or empty"));
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
				
				// Initializes byte image array on first call for speed in future calls.
				if (m_byteImages == null)
				{
					m_byteImages = TVA.Common.CreateArray<string>(256);
					
					for (int imageByte = byte.MinValue; imageByte <= byte.MaxValue; imageByte++)
					{
						System.Text.StringBuilder with_1 = new StringBuilder;
						if (m_reverse)
						{
							if ((imageByte && Bit7) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit6) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit5) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit4) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit3) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit2) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit1) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit0) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
						}
						else
						{
							if ((imageByte && Bit0) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit1) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit2) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit3) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit4) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit5) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit6) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
							if ((imageByte && Bit7) > 0)
							{
								with_1.Append('1');
							}
							else
							{
								with_1.Append('0');
							}
						}
						
						m_byteImages[imageByte] = with_1.ToString();
					}
				}
				
				System.Text.StringBuilder with_2 = new StringBuilder;
				if (bytes != null)
				{
					for (int x = 0; x <= length - 1; x++)
					{
						if (spacingCharacter != NoSpacing && x > 0)
						{
							with_2.Append(spacingCharacter);
						}
						with_2.Append(m_byteImages[bytes[offset + x]]);
					}
				}
				
				return with_2.ToString();
				
			}
			
		}
		
		#endregion
		
		#region " Base64 Encoding Class "
		
		[EditorBrowsable(EditorBrowsableState.Never)]public class Base64Encoding : ByteEncoding
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
				if (spacingCharacter != NoSpacing)
				{
					binaryData = binaryData.Replace(spacingCharacter, "");
				}
				
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
				
				if (spacingCharacter == NoSpacing)
				{
					return Convert.ToBase64String(bytes, offset, length);
				}
				else
				{
					System.Text.StringBuilder with_1 = new StringBuilder;
					if (bytes != null)
					{
						string base64String = Convert.ToBase64String(bytes, offset, length);
						
						for (int x = 0; x <= base64String.Length - 1; x++)
						{
							if (x > 0)
							{
								with_1.Append(spacingCharacter);
							}
							with_1.Append(base64String[x]);
						}
					}
					
					return with_1.ToString();
				}
				
			}
			
		}
		
		#endregion
		
		#region " ASCII Encoding Class "
		
		[EditorBrowsable(EditorBrowsableState.Never)]public class ASCIIEncoding : ByteEncoding
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
				if (spacingCharacter != NoSpacing)
				{
					binaryData = binaryData.Replace(spacingCharacter, "");
				}
				
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
				
				if (spacingCharacter == NoSpacing)
				{
					return Encoding.ASCII.GetString(bytes, offset, length);
				}
				else
				{
					System.Text.StringBuilder with_1 = new StringBuilder;
					if (bytes != null)
					{
						string asciiString = Encoding.ASCII.GetString(bytes, offset, length);
						
						for (int x = 0; x <= asciiString.Length - 1; x++)
						{
							if (x > 0)
							{
								with_1.Append(spacingCharacter);
							}
							with_1.Append(asciiString[x]);
						}
					}
					
					return with_1.ToString();
				}
				
			}
			
		}
		
		#endregion
		
		public const char NoSpacing = char.MinValue;
		
		private static HexadecimalEncoding m_hexadecimalEncoding;
		private static DecimalEncoding m_decimalEncoding;
		private static BinaryEncoding m_bigEndianBinaryEncoding;
		private static BinaryEncoding m_littleEndianBinaryEncoding;
		private static Base64Encoding m_base64Encoding;
		private static ASCIIEncoding m_asciiEncoding;
		
		private ByteEncoding()
		{
			
			// This class contains only global functions and is not meant to be instantiated.
			
		}
		
		/// <summary>Handles encoding and decoding of a byte buffer into a hexadecimal-based presentation format.</summary>
		public static HexadecimalEncoding Hexadecimal
		{
			get
			{
				if (m_hexadecimalEncoding == null)
				{
					m_hexadecimalEncoding = new HexadecimalEncoding();
				}
				return m_hexadecimalEncoding;
			}
		}
		
		/// <summary>Handles encoding and decoding of a byte buffer into an integer-based presentation format.</summary>
		public static DecimalEncoding @decimal
		{
			get
			{
				if (m_decimalEncoding == null)
				{
					m_decimalEncoding = new DecimalEncoding();
				}
				return m_decimalEncoding;
			}
		}
		
		/// <summary>Handles encoding and decoding of a byte buffer into a big-endian binary (i.e., 0 and 1's) based
		/// presentation format.</summary>
		/// <remarks>
		/// Although endianness is typically used in the context of byte order (see TVA.Interop.EndianOrder to handle byte
		/// order swapping), this property allows you visualize "bits" in big-endian order, right-to-left (Note that bits
		/// are normally stored in the same order as their bytes.).
		/// </remarks>
		public static BinaryEncoding BigEndianBinary
		{
			get
			{
				if (m_bigEndianBinaryEncoding == null)
				{
					m_bigEndianBinaryEncoding = new BinaryEncoding(Endianness.BigEndian);
				}
				return m_bigEndianBinaryEncoding;
			}
		}
		
		/// <summary>Handles encoding and decoding of a byte buffer into a little-endian binary (i.e., 0 and 1's) based
		/// presentation format.</summary>
		/// <remarks>
		/// Although endianness is typically used in the context of byte order (see TVA.Interop.EndianOrder to handle byte
		/// order swapping), this property allows you visualize "bits" in little-endian order, left-to-right (Note that bits
		/// are normally stored in the same order as their bytes.).
		/// </remarks>
		public static BinaryEncoding LittleEndianBinary
		{
			get
			{
				if (m_littleEndianBinaryEncoding == null)
				{
					m_littleEndianBinaryEncoding = new BinaryEncoding(Endianness.LittleEndian);
				}
				return m_littleEndianBinaryEncoding;
			}
		}
		
		/// <summary>Handles encoding and decoding of a byte buffer into a base64 presentation format.</summary>
		public static Base64Encoding Base64
		{
			get
			{
				if (m_base64Encoding == null)
				{
					m_base64Encoding = new Base64Encoding();
				}
				return m_base64Encoding;
			}
		}
		
		/// <summary>Handles encoding and decoding of a byte buffer into an ASCII character presentation format.</summary>
		public static ASCIIEncoding ASCII
		{
			get
			{
				if (m_asciiEncoding == null)
				{
					m_asciiEncoding = new ASCIIEncoding();
				}
				return m_asciiEncoding;
			}
		}
		
		#region " Inheritable Functionality "
		
		/// <summary>Encodes given buffer into a user presentable representation.</summary>
		/// <param name="bytes">Bytes to encode.</param>
		
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
			
			if (bytes == null)
			{
				throw (new ArgumentNullException("bytes", "Input buffer cannot be null"));
			}
			return GetString(bytes, 0, bytes.Length, spacingCharacter);
			
		}
		
		/// <summary>Encodes given buffer into a user presentable representation.</summary>
		/// <param name="bytes">Bytes to encode.</param>
		/// <param name="offset">Offset into buffer to begin encoding.</param>
		/// <param name="length">Length of buffer to encode.</param>
		/// <returns>String of encoded bytes.</returns>
		public virtual string GetString(byte[] bytes, int offset, int length)
		{
			
			if (bytes == null)
			{
				throw (new ArgumentNullException("bytes", "Input buffer cannot be null"));
			}
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
			
			if (string.IsNullOrEmpty(value))
			{
				throw (new ArgumentNullException("value", "Input string cannot be null"));
			}
			return GetBytes(value, NoSpacing);
			
		}
		
		/// <summary>Decodes given string back into a byte buffer.</summary>
		/// <param name="value">Encoded string to decode.</param>
		/// <param name="spacingCharacter">Original spacing character that was inserted between encoded bytes.</param>
		/// <returns>Decoded bytes</returns>
		public abstract byte[] GetBytes(string value, char spacingCharacter);
		
		/// <summary>Handles byte to string conversions for implementations that are available from Byte.ToString.</summary>
		protected string BytesToString(byte[] bytes, int offset, int length, char spacingCharacter, string format)
		{
			
			if (bytes == null)
			{
				throw (new ArgumentNullException("bytes", "Input buffer cannot be null"));
			}
			
			System.Text.StringBuilder with_1 = new StringBuilder;
			if (bytes != null)
			{
				for (int x = 0; x <= length - 1; x++)
				{
					if (spacingCharacter != NoSpacing && x > 0)
					{
						with_1.Append(spacingCharacter);
					}
					with_1.Append(bytes[x + offset].ToString(format));
				}
			}
			
			return with_1.ToString();
			
		}
		
		#endregion
		
	}
	
}
