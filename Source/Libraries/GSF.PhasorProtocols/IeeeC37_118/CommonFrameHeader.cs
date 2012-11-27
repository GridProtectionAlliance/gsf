//*******************************************************************************************************
//  CommonFrameHeader.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
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
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSF.Parsing;
using GSF;

namespace GSF.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents the common header for all IEEE C37.118 frames of data.
    /// </summary>
    [Serializable()]
    public class CommonFrameHeader : ICommonHeader<FrameType>, ISerializable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Total fixed length of <see cref="CommonFrameHeader"/>.
        /// </summary>
        public const ushort FixedLength = 14;

        // Fields
        private FrameType m_frameType;
        private byte m_version;
        private ushort m_frameLength;
        private ushort m_idCode;
        private Ticks m_timestamp;
        private uint m_timebase;
        private uint m_timeQualityFlags;
        private IChannelParsingState m_state;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from specified parameters.
        /// </summary>
        /// <param name="typeID">The IEEE C37.118 specific frame type of this frame.</param>
        /// <param name="idCode">The ID code of this frame.</param>
        /// <param name="timestamp">The timestamp of this frame.</param>
        public CommonFrameHeader(FrameType typeID, ushort idCode, Ticks timestamp)
        {
            m_frameType = typeID;
            m_idCode = idCode;
            m_timestamp = timestamp;
            m_version = 1;
            m_timebase = (UInt24)100000;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="configurationFrame">IEEE C37.118 <see cref="ConfigurationFrame1"/> if already parsed.</param>
        /// <param name="buffer">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        public CommonFrameHeader(ConfigurationFrame1 configurationFrame, byte[] buffer, int startIndex)
        {
            if (buffer[startIndex] != GSF.PhasorProtocols.Common.SyncByte)
                throw new InvalidOperationException("Bad data stream, expected sync byte 0xAA as first byte in IEEE C37.118 frame, got 0x" + buffer[startIndex].ToString("X").PadLeft(2, '0'));

            // Strip out frame type and version information...
            m_frameType = (FrameType)buffer[startIndex + 1] & ~IeeeC37_118.FrameType.VersionNumberMask;
            m_version = (byte)(buffer[startIndex + 1] & (byte)IeeeC37_118.FrameType.VersionNumberMask);

            m_frameLength = EndianOrder.BigEndian.ToUInt16(buffer, startIndex + 2);
            m_idCode = EndianOrder.BigEndian.ToUInt16(buffer, startIndex + 4);

            uint secondOfCentury = EndianOrder.BigEndian.ToUInt32(buffer, startIndex + 6);
            uint fractionOfSecond = EndianOrder.BigEndian.ToUInt32(buffer, startIndex + 10);

            // Without timebase, the best timestamp you can get is down to the whole second
            m_timestamp = (new UnixTimeTag((double)secondOfCentury)).ToDateTime().Ticks;

            if (configurationFrame != null)
            {
                // If config frame is available, frames have enough information for subsecond time resolution
                m_timebase = configurationFrame.Timebase;
                decimal fractionalSeconds = (fractionOfSecond & ~Common.TimeQualityFlagsMask) / (decimal)m_timebase;
                m_timestamp += (long)(fractionalSeconds * (decimal)Ticks.PerSecond);
            }

            m_timeQualityFlags = fractionOfSecond & Common.TimeQualityFlagsMask;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommonFrameHeader(SerializationInfo info, StreamingContext context)
        {
            // Deserialize common frame header
            m_frameType = (FrameType)info.GetValue("frameType", typeof(IeeeC37_118.FrameType));
            m_version = info.GetByte("version");
            m_frameLength = info.GetUInt16("frameLength");
            m_timebase = info.GetUInt32("timebase");
            m_timeQualityFlags = info.GetUInt32("timeQualityFlags");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets timestamp of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public Ticks Timestamp
        {
            get
            {
                return m_timestamp;
            }
            set
            {
                m_timestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 specific frame type of this frame.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This returns the protocol specific frame classification which uniquely identifies the frame type.
        /// </para>
        /// <para>
        /// This is the <see cref="ICommonHeader{TTypeIdentifier}.TypeID"/> implementation.
        /// </para>
        /// </remarks>
        public FrameType TypeID
        {
            get
            {
                return m_frameType;
            }
            set
            {
                m_frameType = value;
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 version of this frame.
        /// </summary>
        public byte Version
        {
            get
            {
                return m_version;
            }
            set
            {
                m_version = value;
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 frame length of this frame.
        /// </summary>
        public ushort FrameLength
        {
            get
            {
                return m_frameLength;
            }
            set
            {
                m_frameLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the length of the data in the IEEE C37.118 frame (i.e., the <see cref="FrameLength"/> minus the header length and checksum: <see cref="FrameLength"/> - 8).
        /// </summary>
        public ushort DataLength
        {
            get
            {
                // Data length will be frame length minus common header length minus crc16
                return (ushort)(FrameLength - FixedLength - 2);
            }
            set
            {
                if (value > Common.MaximumDataLength)
                    throw new OverflowException("Data length value cannot exceed " + Common.MaximumDataLength);
                else
                    FrameLength = (ushort)(value + FixedLength + 2);
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 ID code of this frame.
        /// </summary>
        public ushort IDCode
        {
            get
            {
                return m_idCode;
            }
            set
            {
                m_idCode = value;
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 resolution of fractional time stamps.
        /// </summary>
        public uint Timebase
        {
            get
            {
                return m_timebase;
            }
            set
            {
                m_timebase = value;
            }
        }

        /// <summary>
        /// Gets the IEEE C37.118 second of century.
        /// </summary>
        public uint SecondOfCentury
        {
            get
            {
                return (uint)Math.Truncate(TimeTag.Value);
            }
        }

        /// <summary>
        /// Gets the IEEE C37.118 fraction of second.
        /// </summary>
        public UInt24 FractionOfSecond
        {
            get
            {
                return (UInt24)((decimal)m_timestamp.DistanceBeyondSecond() / (decimal)Ticks.PerSecond * (decimal)m_timebase);
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 <see cref="TimeQualityFlags"/>.
        /// </summary>
        public TimeQualityFlags TimeQualityFlags
        {
            get
            {
                return (TimeQualityFlags)(m_timeQualityFlags & ~(uint)IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorCodeMask);
            }
            set
            {
                m_timeQualityFlags = (m_timeQualityFlags & (uint)IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorCodeMask) | (uint)value;
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 <see cref="TimeQualityIndicatorCode"/>.
        /// </summary>
        public TimeQualityIndicatorCode TimeQualityIndicatorCode
        {
            get
            {
                return (TimeQualityIndicatorCode)(m_timeQualityFlags & (uint)IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorCodeMask);
            }
            set
            {
                m_timeQualityFlags = (m_timeQualityFlags & ~(uint)IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorCodeMask) | (uint)value;
            }
        }

        /// <summary>
        /// Gets time as a <see cref="UnixTimeTag"/> representing seconds of current <see cref="Timestamp"/>.
        /// </summary>
        public UnixTimeTag TimeTag
        {
            get
            {
                return new UnixTimeTag(m_timestamp);
            }
        }

        /// <summary>
        /// Gets or sets the parsing state for the <see cref="CommonFrameHeader"/> object.
        /// </summary>
        public IChannelParsingState State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }

        // Gets or sets any additional state information - satifies ICommonHeader<FrameType>.State interface property
        object ICommonHeader<FrameType>.State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value as IChannelParsingState;
            }
        }

        /// <summary>
        /// Gets the fundamental frame type of this frame.
        /// </summary>
        /// <remarks>
        /// Frames are generally classified as data, configuration or header frames. This returns the general frame classification.
        /// </remarks>
        public FundamentalFrameType FrameType
        {
            get
            {
                // Translate IEEE C37.118 specific frame type to fundamental frame type
                switch (m_frameType)
                {
                    case IeeeC37_118.FrameType.DataFrame:
                        return FundamentalFrameType.DataFrame;
                    case IeeeC37_118.FrameType.ConfigurationFrame1:
                    case IeeeC37_118.FrameType.ConfigurationFrame2:
                    case IeeeC37_118.FrameType.ConfigurationFrame3:
                        return FundamentalFrameType.ConfigurationFrame;
                    case IeeeC37_118.FrameType.HeaderFrame:
                        return FundamentalFrameType.HeaderFrame;
                    case IeeeC37_118.FrameType.CommandFrame:
                        return FundamentalFrameType.CommandFrame;
                    default:
                        return FundamentalFrameType.Undetermined;
                }
            }
        }

        /// <summary>
        /// Gets the binary image of the common header portion of this frame.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] buffer = new byte[FixedLength];

                buffer[0] = GSF.PhasorProtocols.Common.SyncByte;
                buffer[1] = (byte)((byte)TypeID | Version);
                EndianOrder.BigEndian.CopyBytes(FrameLength, buffer, 2);
                EndianOrder.BigEndian.CopyBytes(IDCode, buffer, 4);
                EndianOrder.BigEndian.CopyBytes(SecondOfCentury, buffer, 6);
                EndianOrder.BigEndian.CopyBytes(FractionOfSecond | (int)TimeQualityFlags, buffer, 10);

                return buffer;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Appends header specific attributes to <paramref name="attributes"/> dictionary.
        /// </summary>
        /// <param name="attributes">Dictionary to append header specific attributes to.</param>
        internal void AppendHeaderAttributes(Dictionary<string, string> attributes)
        {
            attributes.Add("Frame Type", (ushort)TypeID + ": " + TypeID);
            attributes.Add("Frame Length", FrameLength.ToString());
            attributes.Add("Version", Version.ToString());
            attributes.Add("Second of Century", SecondOfCentury.ToString());
            attributes.Add("Fraction of Second", FractionOfSecond.ToString());

            uint timeQualityFlags = (uint)TimeQualityFlags;

            attributes.Add("Time Quality Flags", timeQualityFlags.ToString());

            if (timeQualityFlags > 0)
                attributes.Add("Leap Second State", TimeQualityFlags.ToString());
            else
                attributes.Add("Leap Second State", "No leap second is currently pending");

            attributes.Add("Time Quality Indicator Code", (uint)TimeQualityIndicatorCode + ": " + TimeQualityIndicatorCode);
            attributes.Add("Time Base", Timebase.ToString());
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize unique common frame header values
            info.AddValue("frameType", m_frameType, typeof(IeeeC37_118.FrameType));
            info.AddValue("version", m_version);
            info.AddValue("frameLength", m_frameLength);
            info.AddValue("timebase", m_timebase);
            info.AddValue("timeQualityFlags", m_timeQualityFlags);
        }

        #endregion
    }
}