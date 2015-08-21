using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
using System.Security.Cryptography;
//using PCS.Common;
using PCS.Interop;
using PCS.Parsing;
//using PCS.Math.Common;
//using PCS.Security.Cryptography.Common;

//*******************************************************************************************************
//  PCS.Security.Radius.RadiusPacket.vb - RADIUS authentication packet
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/11/2008 - Pinal C. Patel
//       Original version of source code generated.
//
//*******************************************************************************************************


namespace PCS.Security
{
	namespace Radius
	{
		
		public class RadiusPacket : IBinaryImageProvider, IBinaryDataConsumer
		{
			
			
			// 0                   1                   2                   3
			// 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
			//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
			//|     Code      |  Identifier   |            Length             |
			//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
			//|                                                               |
			//|                         Authenticator                         |
			//|                                                               |
			//|                                                               |
			//+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
			//|  Attributes ...
			//+-+-+-+-+-+-+-+-+-+-+-+-+-
			
			#region " Member Declaration "
			
			private PacketType m_type;
			private byte m_identifier;
			private byte[] m_authenticator;
			private List<RadiusPacketAttribute> m_attributes;
			
			#endregion
			
			#region " Code Scope: Public "
			
			/// <summary>
			/// Encoding format for encoding text.
			/// </summary>
			public static Encoding Encoding;
			
			/// <summary>
			/// Creates a default instance of RADIUS packet.
			/// </summary>
			public RadiusPacket()
			{
				
				m_identifier = (byte) (PCS.Math.Common.RandomBetween(0, 255));
				m_authenticator = PCS.Common.CreateArray<byte>(16);
				m_attributes = new List<RadiusPacketAttribute>();
				
			}
			
			/// <summary>
			/// Creates an instance of RADIUS packet.
			/// </summary>
			/// <param name="type">Type of the packet.</param>
			public RadiusPacket(PacketType type) : this()
			{
				
				m_type = type;
				
			}
			
			/// <summary>
			/// Creates an instance of RADIUS packet.
			/// </summary>
			/// <param name="binaryImage">A byte array.</param>
			/// <param name="startIndex">Starting point in the byte array.</param>
			public RadiusPacket(byte[] binaryImage, int startIndex) : this()
			{
				
				Initialize(binaryImage, startIndex);
				
			}
			
			/// <summary>
			/// Gets or sets the type of the packet.
			/// </summary>
			/// <value></value>
			/// <returns>Type of the packet.</returns>
			public PacketType Type
			{
				get
				{
					return m_type;
				}
				set
				{
					m_type = value;
				}
			}
			
			/// <summary>
			/// Gets or sets the packet identifier.
			/// </summary>
			/// <value></value>
			/// <returns>Identifier of the packet.</returns>
			public byte Identifier
			{
				get
				{
					return m_identifier;
				}
				set
				{
					m_identifier = value;
				}
			}
			
			/// <summary>
			/// Gets or sets the packet authenticator.
			/// </summary>
			/// <value></value>
			/// <returns>Authenticator of the packet.</returns>
			public byte[] Authenticator
			{
				get
				{
					return m_authenticator;
				}
				set
				{
					if (value != null)
					{
						if (value.Length == 16)
						{
							m_authenticator = value;
						}
						else
						{
							throw (new ArgumentException("Authenticator must 16-byte long."));
						}
					}
					else
					{
						throw (new ArgumentNullException("Authenticator"));
					}
				}
			}
			
			/// <summary>
			/// Gets a list of packet attributes.
			/// </summary>
			/// <value></value>
			/// <returns>Attributes of the packet.</returns>
			public List<RadiusPacketAttribute> Attributes
			{
				get
				{
					return m_attributes;
				}
			}
			
			/// <summary>
			/// Gets the value of the specified attribute if it is present in the packet.
			/// </summary>
			/// <param name="type">Type of the attribute whose value is to be retrieved.</param>
			/// <returns>Attribute value as a byte array if attribute is present; otherwise Nothing.</returns>
			public byte[] GetAttributeValue(AttributeType type)
			{
				
				foreach (RadiusPacketAttribute attrib in m_attributes)
				{
					// Attribute found, return its value.
					if (attrib.Type == type)
					{
						return attrib.Value;
					}
				}
				
				return null; // Attribute is not present in the packet.
				
			}
			
			#region " Interface Implementations "
			
			#region " IBinaryDataProvider "
			
			/// <summary>
			/// Gets the binary lenght of the packet.
			/// </summary>
			/// <value></value>
			/// <returns>32-bit signed integer value.</returns>
			public int BinaryLength
			{
				get
				{
					// 20 bytes are fixed + length of all attributes combined
					int length = 20;
					foreach (RadiusPacketAttribute attribute in m_attributes)
					{
						length += attribute.BinaryLength;
					}
					
					return length;
				}
			}
			
			/// <summary>
			/// Gets the binary image of the packet.
			/// </summary>
			/// <value></value>
			/// <returns>A byte array.</returns>
			public byte[] BinaryImage
			{
				get
				{
					byte[] image = PCS.Common.CreateArray<byte>(BinaryLength);
					image[0] = System.Convert.ToByte(m_type);
					image[1] = m_identifier;
					Array.Copy(GetBytes((ushort) BinaryLength), 0, image, 2, 2);
					Array.Copy(m_authenticator, 0, image, 4, m_authenticator.Length);
					int cursor = 20;
					foreach (RadiusPacketAttribute attribute in m_attributes)
					{
						Array.Copy(attribute.BinaryImage, 0, image, cursor, attribute.BinaryLength);
						cursor += attribute.BinaryLength;
					}
					
					return image;
				}
			}
			
			#endregion
			
			#region " IBinaryDataConsumer "
			
			/// <summary>
			/// Initializes the packet from the specified binary image.
			/// </summary>
			/// <param name="binaryImage">A byte array.</param>
			/// <param name="startIndex">Starting point in the byte array.</param>
			/// <returns>Number of bytes used to initialize the packet.</returns>
			public int Initialize(byte[] binaryImage, int startIndex)
			{
				
				if ((binaryImage != null)&& binaryImage.Length >= 20)
				{
					// We have a valid buffer to work with.
					UInt16 length;
					m_type = (PacketType) (binaryImage[startIndex]);
					m_identifier = binaryImage[startIndex + 1];
					length = ToUInt16(binaryImage, startIndex + 2);
					Array.Copy(binaryImage, 4, m_authenticator, 0, m_authenticator.Length);
					// Parse any attributes in the packet.
					int cursor = 20;
					while (cursor < length)
					{
						RadiusPacketAttribute attribute = new RadiusPacketAttribute(binaryImage, startIndex + cursor);
						m_attributes.Add(attribute);
						cursor += attribute.BinaryLength;
					}
					
					return BinaryLength;
				}
				else
				{
					throw (new ArgumentException("Buffer is not valid."));
				}
				
			}
			
			#endregion
			
			#endregion
			
			#region " Shared "
			
			/// <summary>
			/// Gets bytes for the specified text.
			/// </summary>
			/// <param name="value">Text blob.</param>
			/// <returns>A byte array.</returns>
			public static byte[] GetBytes(string value)
			{
				
				return Encoding.GetBytes(value);
				
			}
			
			/// <summary>
			/// Gets bytes for the specified 16-bit signed integer value.
			/// </summary>
			/// <param name="value">16-bit signed integer value.</param>
			/// <returns>A byte array.</returns>
			public static byte[] GetBytes(short value)
			{
				
				// Integer values are in Big-endian (most significant byte first) format.
				return EndianOrder.BigEndian.GetBytes(value);
				
			}
			
			/// <summary>
			/// Gets bytes for the specified 16-bit unsigned integer value.
			/// </summary>
			/// <param name="value">16-bit unsigned integer value.</param>
			/// <returns>A byte array.</returns>
			[CLSCompliant(false)]public static byte[] GetBytes(UInt16 value)
			{
				
				// Integer values are in Big-endian (most significant byte first) format.
				return EndianOrder.BigEndian.GetBytes(value);
				
			}
			
			/// <summary>
			/// Gets bytes for the specified 32-bit signed integer value.
			/// </summary>
			/// <param name="value">32-bit signed integer value.</param>
			/// <returns>A byte array.</returns>
			public static byte[] GetBytes(int value)
			{
				
				// Integer values are in Big-endian (most significant byte first) format.
				return EndianOrder.BigEndian.GetBytes(value);
				
			}
			
			/// <summary>
			/// Gets bytes for the specified 32-bit unsigned integer value.
			/// </summary>
			/// <param name="value">32-bit unsigned integer value.</param>
			/// <returns>A byte array.</returns>
			[CLSCompliant(false)]public static byte[] GetBytes(UInt32 value)
			{
				
				// Integer values are in Big-endian (most significant byte first) format.
				return EndianOrder.BigEndian.GetBytes(value);
				
			}
			
			/// <summary>
			/// Converts the specified byte array to text.
			/// </summary>
			/// <param name="buffer">A byte array.</param>
			/// <param name="index">Starting point in the byte array.</param>
			/// <param name="length">Number of bytes to be converted.</param>
			/// <returns>A text blob.</returns>
			public static string ToText(byte[] buffer, int index, int length)
			{
				
				return Encoding.GetString(buffer, index, length);
				
			}
			
			/// <summary>
			/// Converts the specified byte array to a signed 16-bit integer value.
			/// </summary>
			/// <param name="buffer">A byte array.</param>
			/// <param name="index">Starting point in the byte array.</param>
			/// <returns>A 16-bit signed integer value.</returns>
			public static short ToInt16(byte[] buffer, int index)
			{
				
				// Integer values are in Big-endian (most significant byte first) format.
				return EndianOrder.BigEndian.ToInt16(buffer, index);
				
			}
			
			/// <summary>
			/// Converts the specified byte array to an unsigned 16-bit integer value.
			/// </summary>
			/// <param name="buffer">A byte array.</param>
			/// <param name="index">Starting point in the byte array.</param>
			/// <returns>A 16-bit unsigned integer value.</returns>
			[CLSCompliant(false)]public static UInt16 ToUInt16(byte[] buffer, int index)
			{
				
				// Integer values are in Big-endian (most significant byte first) format.
				return EndianOrder.BigEndian.ToUInt16(buffer, index);
				
			}
			
			/// <summary>
			/// Converts the specified byte array to a signed 32-bit integer value.
			/// </summary>
			/// <param name="buffer">A byte array.</param>
			/// <param name="index">Starting point in the byte array.</param>
			/// <returns>A 32-bit signed integer value.</returns>
			public static int ToInt32(byte[] buffer, int index)
			{
				
				// Integer values are in Big-endian (most significant byte first) format.
				return EndianOrder.BigEndian.ToInt32(buffer, index);
				
			}
			
			/// <summary>
			/// Converts the specified byte array to an unsigned 32-bit integer value.
			/// </summary>
			/// <param name="buffer">A byte array.</param>
			/// <param name="index">Starting point in the byte array.</param>
			/// <returns>A 32-bit unsigned integer value.</returns>
			[CLSCompliant(false)]public static UInt32 ToUInt32(byte[] buffer, int index)
			{
				
				// Integer values are in Big-endian (most significant byte first) format.
				return EndianOrder.BigEndian.ToUInt32(buffer, index);
				
			}
			
			/// <summary>
			/// Generates an "Authenticator" value used in a RADIUS request packet sent by the client to server.
			/// </summary>
			/// <param name="sharedSecret">The shared secret to be used in generating the output.</param>
			/// <returns>A byte array.</returns>
			public static byte[] CreateRequestAuthenticator(string sharedSecret)
			{
				
				// We create a input buffer that'll be used to create a 16-byte value using the RSA MD5 algorithm.
				// Since the output value (The Authenticator) has to be unique over the life of the shared secret,
				// we prepend a randomly generated "salt" text to ensure the uniqueness of the output value.
				string inputString = PCS.Security.Cryptography.Common.GenerateKey() + sharedSecret;
				byte[] inputBuffer = RadiusPacket.GetBytes(inputString);
				
				return new MD5CryptoServiceProvider().ComputeHash(inputBuffer);
				
			}
			
			/// <summary>
			/// Generates an "Authenticator" value used in a RADIUS response packet sent by the server to client.
			/// </summary>
			/// <param name="sharedSecret">The shared secret key.</param>
			/// <param name="requestPacket">RADIUS packet sent from client to server.</param>
			/// <param name="responsePacket">RADIUS packet sent from server to client.</param>
			/// <returns>A byte array.</returns>
			public static byte[] CreateResponseAuthenticator(string sharedSecret, RadiusPacket requestPacket, RadiusPacket responsePacket)
			{
				
				byte[] requestBytes = requestPacket.BinaryImage;
				byte[] responseBytes = responsePacket.BinaryImage;
				byte[] sharedSecretBytes = RadiusPacket.GetBytes(sharedSecret);
				byte[] inputBuffer = PCS.Common.CreateArray<byte>(responseBytes.Length + sharedSecretBytes.Length);
				
				// Response authenticator is generated as follows:
				// MD5(Code + Identifier + Length + Request Authenticator + Attributes + Shared Secret)
				//   where:
				//   Code, Identifier, Length & Attributes are from the response RADIUS packet
				//   Request Authenticator if from the request RADIUS packet
				//   Shared Secret is the shared secret ket
				
				Array.Copy(responseBytes, 0, inputBuffer, 0, responseBytes.Length);
				Array.Copy(requestBytes, 4, inputBuffer, 4, 16);
				Array.Copy(sharedSecretBytes, 0, inputBuffer, responseBytes.Length, sharedSecretBytes.Length);
				
				return new MD5CryptoServiceProvider().ComputeHash(inputBuffer);
				
			}
			
			/// <summary>
			/// Generates an encrypted password using the RADIUS protocol specification (RFC 2285).
			/// </summary>
			/// <param name="password">User's password.</param>
			/// <param name="sharedSecret">Shared secret key.</param>
			/// <param name="requestAuthenticator">Request authenticator byte array.</param>
			/// <returns>A byte array.</returns>
			public static byte[] EncryptPassword(string password, string sharedSecret, byte[] requestAuthenticator)
			{
				
				// Max length of the password can be 130 according to RFC 2865. Since 128 is the closest multiple
				// of 16 (password segment length), we allow the password to be no longer than 128 characters.
				if (password.Length <= 128)
				{
					byte[] result;
					byte[] xorBytes = null;
					byte[] passwordBytes = RadiusPacket.GetBytes(password);
					byte[] sharedSecretBytes = RadiusPacket.GetBytes(sharedSecret);
					byte[] md5HashInputBytes = PCS.Common.CreateArray<byte>(sharedSecretBytes.Length + 16);
					MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
					if (passwordBytes.Length % 16 == 0)
					{
						// Length of password is a multiple of 16.
						result = PCS.Common.CreateArray<byte>(passwordBytes.Length);
					}
					else
					{
						// Length of password is not a multiple of 16, so we'll take the multiple of 16 that's next
						// closest to the password's length and leave the empty space at the end as padding.
						result = PCS.Common.CreateArray<byte>(((passwordBytes.Length / 16) * 16) + 16);
					}
					
					// Copy the password to the result buffer where it'll be XORed.
					Array.Copy(passwordBytes, 0, result, 0, passwordBytes.Length);
					// For the first 16-byte segment of the password, password characters are to be XORed with the
					// MD5 hash value that's computed as follows:
					//   MD5(Shared secret key + Request authenticator)
					Array.Copy(sharedSecretBytes, 0, md5HashInputBytes, 0, sharedSecretBytes.Length);
					Array.Copy(requestAuthenticator, 0, md5HashInputBytes, sharedSecretBytes.Length, requestAuthenticator.Length);
					for (int i = 0; i <= result.Length - 1; i += 16)
					{
						// Perform XOR-based encryption of the password in 16-byte segments.
						if (i > 0)
						{
							// For passwords that are more than 16 characters in length, each consecutive 16-byte
							// segment of the password is XORed with MD5 hash value that's computed as follows:
							//   MD5(Shared secret key + XOR bytes used in the previous segment)
							Array.Copy(xorBytes, 0, md5HashInputBytes, sharedSecretBytes.Length, xorBytes.Length);
						}
						xorBytes = md5Provider.ComputeHash(md5HashInputBytes);
						
						// XOR the password bytes in the current segment with the XOR bytes.
						for (int j = i; j <= (i + 16) - 1; j++)
						{
							result[j] = result[j] ^ xorBytes[j];
						}
					}
					
					return result;
				}
				else
				{
					throw (new ArgumentException("Password can be a maximum of 128 characters in length."));
				}
				
			}
			
			#endregion
			
			#endregion
			
		}
		
	}
}
