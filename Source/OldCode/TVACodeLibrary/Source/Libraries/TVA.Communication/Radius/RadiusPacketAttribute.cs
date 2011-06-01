//*******************************************************************************************************
//  RadiusPacketAttribute.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/26/2010 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Net;
using TVA.Parsing;

namespace TVA.Communication.Radius
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
        private AttributeType m_type;
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
        public RadiusPacketAttribute(AttributeType type, string value)
            : this(type, RadiusPacket.Encoding.GetBytes(value))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacketAttribute"/> class.
        /// </summary>
        /// <param name="type">Type of the <see cref="RadiusPacketAttribute"/>.</param>
        /// <param name="value">32-bit unsigned integer value of the <see cref="RadiusPacketAttribute"/>.</param>
        [CLSCompliant(false)]
        public RadiusPacketAttribute(AttributeType type, UInt32 value)
            : this(type, RadiusPacket.EndianOrder.GetBytes(value))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacketAttribute"/> class.
        /// </summary>
        /// <param name="type">Type of the <see cref="RadiusPacketAttribute"/>.</param>
        /// <param name="value">IP address value of the <see cref="RadiusPacketAttribute"/>.</param>
        public RadiusPacketAttribute(AttributeType type, IPAddress value)
            : this(type, value.GetAddressBytes())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacketAttribute"/> class.
        /// </summary>
        /// <param name="type">Type of the <see cref="RadiusPacketAttribute"/>.</param>
        /// <param name="value">Byte array value of the <see cref="RadiusPacketAttribute"/>.</param>
        public RadiusPacketAttribute(AttributeType type, byte[] value)
        {
            this.Type = type;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadiusPacketAttribute"/> class.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="RadiusPacketAttribute"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        public RadiusPacketAttribute(byte[] binaryImage, int startIndex, int length)
        {
            Initialize(binaryImage, startIndex, length);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the type of the <see cref="RadiusPacketAttribute"/>.
        /// </summary>
        public AttributeType Type
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
        /// Gets or sets the value of the <see cref="RadiusPacketAttribute"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or zero-length byte array.</exception>
        public byte[] Value
        {
            get
            {
                return m_value;
            }
            set
            {
                // By definition, attribute value cannot be null or zero-length.
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException("value");

                m_value = value;
            }
        }

        /// <summary>
        /// Gets the lenght of the <see cref="BinaryImage"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                // 2 bytes are fixed + length of the value
                if (m_value == null)
                    return 2;
                else
                    return 2 + m_value.Length;
            }
        }

        /// <summary>
        /// Gets the binary representation of the <see cref="RadiusPacketAttribute"/>.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[BinaryLength];

                image[0] = Convert.ToByte(m_type);
                image[1] = (byte)image.Length;
                Array.Copy(m_value, 0, image, 2, m_value.Length);

                return image;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="RadiusPacketAttribute"/> from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="RadiusPacketAttribute"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing <see cref="RadiusPacketAttribute"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="binaryImage"/> is null.</exception>
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (binaryImage == null)
                throw new ArgumentNullException("binaryImage");

            if (length >= BinaryLength)
            {
                // Binary image has sufficient data.
                m_type = (AttributeType)(binaryImage[startIndex]);
                m_value = new byte[Convert.ToInt16(binaryImage[startIndex + 1] - 2)];
                Array.Copy(binaryImage, startIndex + 2, m_value, 0, m_value.Length);

                return BinaryLength;
            }
            else
            {
                // Binary image does not have sufficient data.
                return 0;
            }
        }

        #endregion
    }
}