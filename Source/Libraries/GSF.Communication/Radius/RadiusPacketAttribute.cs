//******************************************************************************************************
//  RadiusPacketAttribute.cs - Gbtc
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
using System.Net;
using GSF.Parsing;

namespace GSF.Communication.Radius
{
    #region [ Enumerations ]

    /// <summary>
    /// Specifies the type of <see cref="RadiusPacketAttribute"/>.
    /// </summary>
    public enum AttributeType
    {
        /// <summary>
        /// Attribute indicates the name of the user to be authenticated.
        /// </summary>
        UserName = 1,
        /// <summary>
        /// Attribute indicates the password of the user to be authenticated.
        /// </summary>
        UserPassword = 2,
        /// <summary>
        /// Attribute indicates the response provided by a PPP CHAP user in reponse to the challenge.
        /// </summary>
        ChapPassword = 3,
        /// <summary>
        /// Attribute indicates the identifying IP address of the NAS requesting user authentication.
        /// </summary>
        NasIpAddress = 4,
        /// <summary>
        /// Attribute indicates the physical port number of the NAS which is authenticating the user.
        /// </summary>
        NasPort = 5,
        /// <summary>
        /// Attribute indicates the type of service the user has requested, or the type of service to be provided.
        /// </summary>
        ServiceType = 6,
        /// <summary>
        /// Attribute indicates the framing to be used for framed access.
        /// </summary>
        FramedProtocol = 7,
        /// <summary>
        /// Attribute indicates the address to be configured for the user.
        /// </summary>
        FramedIpAddress = 8,
        /// <summary>
        /// Attribute indicates the IP netmask to be configured for the user when user is a router to a network.
        /// </summary>
        FramedIpNetmask = 9,
        /// <summary>
        /// Attribute indicates the routing method for the user when user is a router to a network.
        /// </summary>
        FramedRouting = 10,
        /// <summary>
        /// Attribute indicates the name of the filter list for this user.
        /// </summary>
        FilterId = 11,
        /// <summary>
        /// Attribute indicates the MTU to be configured for the user when it is not negotiated by some other means.
        /// </summary>
        FramedMtu = 12,
        /// <summary>
        /// Attribute indicates a compression protocol to be used for the link.
        /// </summary>
        FramedCompression = 13,
        /// <summary>
        /// Attribute indicates the system with which to connect the user when <see cref="AttributeType.LoginService"/> attribute is included.
        /// </summary>
        LoginIpHost = 14,
        /// <summary>
        /// Attribute indicates the service to use to connect the user to the login host.
        /// </summary>
        LoginService = 15,
        /// <summary>
        /// Attribute indicates the TCP port with which the user is to be connected when <see cref="AttributeType.LoginService"/> attribute is included.
        /// </summary>
        LoginTcpPort = 16,
        /// <summary>
        /// Attribute indicates the text which may be displayed to the user.
        /// </summary>
        ReplyMessage = 18,
        /// <summary>
        /// Attribute indicates a dialing string to be used for callback.
        /// </summary>
        CallbackNumber = 19,
        /// <summary>
        /// Attribute indicates the name of a place to be called.
        /// </summary>
        CallbackId = 20,
        /// <summary>
        /// Attribute provides routing information to be configured for the user on the NAS.
        /// </summary>
        FramedRoute = 22,
        /// <summary>
        /// Attribute indicates the IPX Network number to be configured for the user.
        /// </summary>
        FramedIpxNetwork = 23,
        /// <summary>
        /// Attribute available to be sent by the server to the client in an <see cref="PacketType.AccessChallenge"/> and must be 
        /// sent unmodified from the client to the server in the new <see cref="PacketType.AccessRequest"/> reply to the challenge.
        /// </summary>
        State = 24,
        /// <summary>
        /// Attribute available to be sent by the server to the client in an <see cref="PacketType.AccessAccept"/> and should 
        /// be sent unmodified by the client to the accounting server as part of the <see cref="PacketType.AccountingRequest"/>.
        /// </summary>
        Class = 25,
        /// <summary>
        /// Attribute available to allow vendors to support their own extended attributes.
        /// </summary>
        VendorSpecific = 26,
        /// <summary>
        /// Attribute sets the maximum number of seconds of service to be provided to the user before termination of the session or prompt.
        /// </summary>
        SessionTimeout = 27,
        /// <summary>
        /// Attribute sets the maximum number of consecutive seconds of idle connection allowed to the user before termination of the session or prompt.
        /// </summary>
        IdleTimeout = 28,
        /// <summary>
        /// Attribute indicates the action the NAS should take when the specified service is complete.
        /// </summary>
        TerminationAction = 29,
        /// <summary>
        /// Attribute indicates the phone number that the user called using DNIS or similar technology.
        /// </summary>
        CallerStationId = 30,
        /// <summary>
        /// Attribute indicates the phone number the call came from using ANI or similar technology.
        /// </summary>
        CallingStationId = 31,
        /// <summary>
        /// Attribute indicates a string identifier for the NAS originating the AccessRequest.
        /// </summary>
        NasIdentifier = 32,
        /// <summary>
        /// Attribute indicates the state a proxy server forwarding requests to the server.
        /// </summary>
        ProxyState = 33,
        /// <summary>
        /// Attribute indicates the system with which the user is to be connected by LAT.
        /// </summary>
        LoginLatService = 34,
        /// <summary>
        /// Attribute indicates the Node with which the user is to be automatically connected by LAT.
        /// </summary>
        LoginLatNode = 35,
        /// <summary>
        /// Attribute indicates the string identifier for the LAT group codes which the user is authorized to use.
        /// </summary>
        LoginLatGroup = 36,
        /// <summary>
        /// Attribute indicates the AppleTalk network number which should be used for the serial link to the user.
        /// </summary>
        FramedAppleTalkLink = 37,
        /// <summary>
        /// Attribute indicates the AppleTalk Network number which the NAS should probe to allocate an AppleTalk node for the user.
        /// </summary>
        FramedAppleTalkNetwork = 38,
        /// <summary>
        /// Attribute indicates the AppleTalk Default Zone to be used for this user.
        /// </summary>
        FramedAppleTalkZone = 39,
        /// <summary>
        /// Attribute contains the CHAP Challenge sent by the NAS to a PPP CHAP user.
        /// </summary>
        ChapChallenge = 60,
        /// <summary>
        /// Attribute indicates the type of physical port of the NAS which is authenticating the user.
        /// </summary>
        NasPortType = 61,
        /// <summary>
        /// Attribute sets the maximum number of ports to be provided to the user by the NAS.
        /// </summary>
        PortLimit = 62,
        /// <summary>
        /// Attribute indicates the Port with which the user is to be connected by the LAT.
        /// </summary>
        LoginLatPort = 63
    }

    #endregion

    /// <summary>
    /// Represents an attribute of <see cref="RadiusPacket"/>.
    /// </summary>
    /// <seealso cref="RadiusPacket"/>
    /// <seealso cref="RadiusClient"/>
    public class RadiusPacketAttribute : ISupportBinaryImage
    {
        // 0                   1                   2
        // 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0
        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
        //|     Type      |    Length     |  Value ...
        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

        #region [ Members ]

        // Fields
        private byte[] m_value;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacketAttribute"/> class.
        /// </summary>
        public RadiusPacketAttribute()
        {
            // No initialization required.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacketAttribute"/> class.
        /// </summary>
        /// <param name="type">Type of the <see cref="RadiusPacketAttribute"/>.</param>
        /// <param name="value">Text value of the <see cref="RadiusPacketAttribute"/>.</param>
        public RadiusPacketAttribute(AttributeType type, string value) : this(type, RadiusPacket.Encoding.GetBytes(value))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacketAttribute"/> class.
        /// </summary>
        /// <param name="type">Type of the <see cref="RadiusPacketAttribute"/>.</param>
        /// <param name="value">32-bit unsigned integer value of the <see cref="RadiusPacketAttribute"/>.</param>
        public RadiusPacketAttribute(AttributeType type, uint value) : this(type, RadiusPacket.EndianOrder.GetBytes(value))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacketAttribute"/> class.
        /// </summary>
        /// <param name="type">Type of the <see cref="RadiusPacketAttribute"/>.</param>
        /// <param name="value">IP address value of the <see cref="RadiusPacketAttribute"/>.</param>
        public RadiusPacketAttribute(AttributeType type, IPAddress value) : this(type, value.GetAddressBytes())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacketAttribute"/> class.
        /// </summary>
        /// <param name="type">Type of the <see cref="RadiusPacketAttribute"/>.</param>
        /// <param name="value">Byte array value of the <see cref="RadiusPacketAttribute"/>.</param>
        public RadiusPacketAttribute(AttributeType type, byte[] value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacketAttribute"/> class.
        /// </summary>
        /// <param name="buffer">Buffer containing binary image to be used for initializing <see cref="RadiusPacketAttribute"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="buffer"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
        public RadiusPacketAttribute(byte[] buffer, int startIndex, int length) => ParseBinaryImage(buffer, startIndex, length);

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the type of the <see cref="RadiusPacketAttribute"/>.
        /// </summary>
        public AttributeType Type { get; set; }

        /// <summary>
        /// Gets or sets the value of the <see cref="RadiusPacketAttribute"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or zero-length byte array.</exception>
        public byte[] Value
        {
            get => m_value;
            set
            {
                // By definition, attribute value cannot be null or zero-length.
                if (value is null || value.Length == 0)
                    throw new ArgumentNullException(nameof(value));

                m_value = value;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="RadiusPacketAttribute"/>.
        /// </summary>
        // 2 bytes are fixed + length of the value
        public int BinaryLength => 2 + m_value?.Length ?? 0;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses <see cref="RadiusPacketAttribute"/> object by parsing the specified <paramref name="buffer"/> containing a binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing binary image to parse.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start parsing.</param>
        /// <param name="length">Valid number of bytes within <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed), or 0 if not enough data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        public int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            int imageLength = BinaryLength;

            // Binary image does not have sufficient data.
            if (length < imageLength)
                return 0;

            // Binary image has sufficient data.
            Type = (AttributeType)buffer[startIndex];
            m_value = new byte[Convert.ToInt16(buffer[startIndex + 1] - 2)];
            Buffer.BlockCopy(buffer, startIndex + 2, m_value, 0, m_value.Length);

            return imageLength;
        }

        /// <summary>
        /// Generates a binary representation of this <see cref="RadiusPacketAttribute"/> object and copies it into the given buffer.
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
            buffer[startIndex] = Convert.ToByte(Type);
            buffer[startIndex + 1] = (byte)length;
            Buffer.BlockCopy(m_value, 0, buffer, startIndex + 2, m_value.Length);

            return length;
        }

        #endregion
    }
}