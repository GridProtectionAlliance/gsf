//******************************************************************************************************
//  RadiusPacket.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/26/2010 - Pinal C. Patel
//       Generated original version of source code.
//  11/23/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using GSF.Parsing;
using Random = GSF.Security.Cryptography.Random;

namespace GSF.Communication.Radius
{
    #region [ Enumerations ]

    /// <summary>
    /// Specifies the type of the <see cref="RadiusPacket"/>.
    /// </summary>
    public enum PacketType
    {
        /// <summary>
        /// Packet sent to a RADIUS server for verification of credentials.
        /// </summary>
        AccessRequest = 1,
        /// <summary>
        /// Packet sent by a RADIUS server when credential verification is successful.
        /// </summary>
        AccessAccept = 2,
        /// <summary>
        /// Packet sent by a RADIUS server when credential verification is unsuccessful.
        /// </summary>
        AccessReject = 3,
        /// <summary>
        /// Not used. No description available. [RFC 2882]
        /// </summary>
        AccountingRequest = 4,
        /// <summary>
        /// Not used. No description available. [RFC 2882]
        /// </summary>
        AccountingResponse = 5,
        /// <summary>
        /// Not used. No description available. [RFC 2882]
        /// </summary>
        AccountingStatus = 6,
        /// <summary>
        /// Not used. No description available. [RFC 2882]
        /// </summary>
        PasswordRequest = 7,
        /// <summary>
        /// Not used. No description available. [RFC 2882]
        /// </summary>
        PasswordAccept = 8,
        /// <summary>
        /// Not used. No description available. [RFC 2882]
        /// </summary>
        PasswordReject = 9,
        /// <summary>
        /// Not used. No description available. [RFC 2882]
        /// </summary>
        AccountingMessage = 10,
        /// <summary>
        /// Packet sent by a RADIUS server when further information is needed for credential verification.
        /// </summary>
        AccessChallenge = 11,
        /// <summary>
        /// Not used. No description available. [RFC 2882]
        /// </summary>
        StatuServer = 12,
        /// <summary>
        /// Not used. No description available. [RFC 2882]
        /// </summary>
        StatusClient = 13
    }

    #endregion

    /// <summary>
    /// Represents a data packet transferred between RADIUS client and server.
    /// </summary>
    /// <seealso cref="RadiusPacketAttribute"/>
    /// <seealso cref="RadiusClient"/>
    public class RadiusPacket : ISupportBinaryImage
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

        #region [ Members ]

        // Fields
        private PacketType m_type;
        private byte m_identifier;
        private byte[] m_authenticator;
        private readonly List<RadiusPacketAttribute> m_attributes;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacket"/> class.
        /// </summary>
        public RadiusPacket()
        {
            m_identifier = (byte)(Random.Between(0, 255));
            m_authenticator = new byte[16];
            m_attributes = new List<RadiusPacketAttribute>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacket"/> class.
        /// </summary>
        /// <param name="type">Type of the <see cref="RadiusPacket"/>.</param>
        public RadiusPacket(PacketType type)
            : this()
        {
            m_type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacket"/> class.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="RadiusPacket"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        public RadiusPacket(byte[] binaryImage, int startIndex, int length)
            : this()
        {
            ParseBinaryImage(binaryImage, startIndex, length);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the type of the <see cref="RadiusPacket"/>.
        /// </summary>
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
        /// Gets or sets the <see cref="RadiusPacket"/> identifier.
        /// </summary>
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
        /// Gets or sets the <see cref="RadiusPacket"/> authenticator.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null.</exception>
        /// <exception cref="ArgumentException">The value being assigned is not 16-bytes in length.</exception>
        public byte[] Authenticator
        {
            get
            {
                return m_authenticator;
            }
            set
            {
                if ((object)value == null)
                    throw new ArgumentNullException(nameof(value));

                if ((object)value == null || value.Length != 16)
                    throw new ArgumentException("Value must 16-byte long.");

                m_authenticator = value;
            }
        }

        /// <summary>
        /// Gets a list of <see cref="RadiusPacketAttribute"/>s.
        /// </summary>
        public List<RadiusPacketAttribute> Attributes
        {
            get
            {
                return m_attributes;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="RadiusPacket"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                // 20 bytes are fixed + length of all attributes combined
                int length = 20;

                foreach (RadiusPacketAttribute attribute in m_attributes)
                {
                    if (attribute != null)
                        length += attribute.BinaryLength;
                }

                return length;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses <see cref="RadiusPacket"/> object by parsing the specified <paramref name="buffer"/> containing a binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing binary image to parse.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start parsing.</param>
        /// <param name="length">Valid number of bytes within <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed), or 0 if not enough data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        public int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            if ((object)buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            int imageLength = BinaryLength;

            if (length >= imageLength)
            {
                // Binary image has sufficient data.
                UInt16 size;
                m_type = (PacketType)(buffer[startIndex]);
                m_identifier = buffer[startIndex + 1];
                size = EndianOrder.ToUInt16(buffer, startIndex + 2);
                Buffer.BlockCopy(buffer, startIndex + 4, m_authenticator, 0, m_authenticator.Length);

                // Parse all attributes in the packet.
                int cursor = 20;

                while (cursor < size)
                {
                    RadiusPacketAttribute attribute = new RadiusPacketAttribute(buffer, startIndex + cursor, length);
                    m_attributes.Add(attribute);
                    cursor += attribute.BinaryLength;
                }

                return imageLength;
            }

            // Binary image does not have sufficient data.
            return 0;
        }

        /// <summary>
        /// Generates a binary representation of this <see cref="RadiusPacket"/> object and copies it into the given buffer.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <see cref="ISupportBinaryImage.BinaryLength"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <see cref="ISupportBinaryImage.BinaryLength"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public int GenerateBinaryImage(byte[] buffer, int startIndex)
        {
            int length = BinaryLength;

            buffer.ValidateParameters(startIndex, length);

            // Populate the buffer
            buffer[startIndex] = Convert.ToByte(m_type);
            buffer[startIndex + 1] = m_identifier;
            Buffer.BlockCopy(EndianOrder.GetBytes((ushort)BinaryLength), 0, buffer, startIndex + 2, 2);
            Buffer.BlockCopy(m_authenticator, 0, buffer, startIndex + 4, m_authenticator.Length);
            startIndex += 20;

            foreach (RadiusPacketAttribute attribute in m_attributes)
            {
                if (attribute != null)
                    startIndex += attribute.GenerateBinaryImage(buffer, startIndex);
            }

            return length;
        }

        /// <summary>
        /// Gets the value of the specified <paramref name="attributeType"/> if it is present in the <see cref="RadiusPacket"/>.
        /// </summary>
        /// <param name="attributeType"><see cref="RadiusPacketAttribute.Type"/> of the <see cref="RadiusPacketAttribute"/> whose value is to be retrieved.</param>
        /// <returns><see cref="RadiusPacketAttribute"/>.<see cref="RadiusPacketAttribute.Value"/> if <see cref="RadiusPacketAttribute"/> is present; otherwise null.</returns>
        public byte[] GetAttributeValue(AttributeType attributeType)
        {
            RadiusPacketAttribute match = m_attributes.Find(attribute => attribute.Type == attributeType);

            if (match == null)
                return null;

            return match.Value;
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// <see cref="Encoding "/> format for encoding text.
        /// </summary>
        public static Encoding Encoding = Encoding.UTF8;

        /// <summary>
        /// <see cref="EndianOrder"/> to use for byte conversion.
        /// </summary>
        public static EndianOrder EndianOrder = EndianOrder.BigEndian;

        // Static Methods

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
            byte[] randomBuffer = new byte[16];
            byte[] secretBuffer = Encoding.GetBytes(sharedSecret);
            Random.GetBytes(randomBuffer);

            return new MD5CryptoServiceProvider().ComputeHash(randomBuffer.Combine(secretBuffer));
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
            // Response authenticator is generated as follows:
            // MD5(Code + Identifier + Length + Request Authenticator + Attributes + Shared Secret)
            //   where:
            //   Code, Identifier, Length & Attributes are from the response RADIUS packet
            //   Request Authenticator is from the request RADIUS packet
            //   Shared Secret is the shared secret key
            int length = responsePacket.BinaryLength;
            byte[] sharedSecretBytes = Encoding.GetBytes(sharedSecret);
            byte[] buffer = new byte[length + sharedSecretBytes.Length];

            responsePacket.GenerateBinaryImage(buffer, 0);
            Buffer.BlockCopy(requestPacket.BinaryImage(), 4, buffer, 4, 16);
            Buffer.BlockCopy(sharedSecretBytes, 0, buffer, length, sharedSecretBytes.Length);

            return new MD5CryptoServiceProvider().ComputeHash(buffer);
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
            // Avoiding Null Dereferences
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentException("Shared secret cannot be null or empty.");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.");

            if ((object)requestAuthenticator == null)
                throw new ArgumentException("Request authenticator cannot be null.");

            // Max length of the password can be 130 according to RFC 2865. Since 128 is the closest multiple
            // of 16 (password segment length), we allow the password to be no longer than 128 characters.
            if (password.Length <= 128)
            {
                byte[] result;
                byte[] xorBytes = null;
                byte[] passwordBytes = Encoding.GetBytes(password);
                byte[] sharedSecretBytes = Encoding.GetBytes(sharedSecret);
                byte[] md5HashInputBytes = new byte[sharedSecretBytes.Length + 16];

                MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();

                if (passwordBytes.Length % 16 == 0)
                {
                    // Length of password is a multiple of 16.
                    result = new byte[passwordBytes.Length];
                }
                else
                {
                    // Length of password is not a multiple of 16, so we'll take the multiple of 16 that's next
                    // closest to the password's length and leave the empty space at the end as padding.
                    result = new byte[((passwordBytes.Length / 16) * 16) + 16];
                }

                // Copy the password to the result buffer where it'll be XORed.
                Buffer.BlockCopy(passwordBytes, 0, result, 0, passwordBytes.Length);

                // For the first 16-byte segment of the password, password characters are to be XORed with the
                // MD5 hash value that's computed as follows:
                //   MD5(Shared secret key + Request authenticator)
                Buffer.BlockCopy(sharedSecretBytes, 0, md5HashInputBytes, 0, sharedSecretBytes.Length);
                Buffer.BlockCopy(requestAuthenticator, 0, md5HashInputBytes, sharedSecretBytes.Length, requestAuthenticator.Length);

                for (int i = 0; i <= result.Length - 1; i += 16)
                {
                    // Perform XOR-based encryption of the password in 16-byte segments.
                    if (i > 0)
                    {
                        // For passwords that are more than 16 characters in length, each consecutive 16-byte
                        // segment of the password is XORed with MD5 hash value that's computed as follows:
                        //   MD5(Shared secret key + XOR bytes used in the previous segment)
                        // ReSharper disable once PossibleNullReferenceException
                        Buffer.BlockCopy(xorBytes, 0, md5HashInputBytes, sharedSecretBytes.Length, xorBytes.Length);
                    }

                    xorBytes = md5Provider.ComputeHash(md5HashInputBytes);

                    // XOR the password bytes in the current segment with the XOR bytes.
                    for (int j = i; j <= (i + 16) - 1; j++)
                    {
                        result[j] = (byte)(result[j] ^ xorBytes[j]);
                    }
                }

                return result;
            }

            throw new ArgumentException("Password can be a maximum of 128 characters in length.");
        }

        #endregion
    }
}