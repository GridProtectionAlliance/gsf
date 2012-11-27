//******************************************************************************************************
//  CommonFrameHeader.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/19/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using GSF.Parsing;
using GSF;

namespace GSF.PhasorProtocols.Iec61850_90_5
{
    /// <summary>
    /// Represents the common header for all IEC 61850-90-5 frames of data.
    /// </summary>
    [Serializable()]
    public class CommonFrameHeader : ICommonHeader<FrameType>, ISerializable
    {
        #region [ Members ]

        /// <summary>Computes a Hash-based Message Authentication Code (HMAC) using the AES hash function.</summary>
        private class AesHmac : HMAC
        {
            /// <summary>Initializes a new instance of the AesHmac class with the specified key data.</summary>
            /// <param name="key">The secret key for AesHmac encryption.</param>
            public AesHmac(byte[] key)
            {
                HashName = "System.Security.Cryptography.AesCryptoServiceProvider";
                HashSizeValue = 128;
                BlockSizeValue = 128;
                Initialize();
                Key = (byte[])key.Clone();
            }
        }

        /// <summary>Computes a Hash-based Message Authentication Code (HMAC) using the SHA256 hash function.</summary>
        private class ShaHmac : HMAC
        {
            /// <summary>Initializes a new instance of the ShaHmac class with the specified key data.</summary>
            /// <param name="key">The secret key for ShaHmac encryption.</param>
            public ShaHmac(byte[] key)
            {
                HashName = "System.Security.Cryptography.SHA256CryptoServiceProvider";
                HashSizeValue = 256;
                BlockSizeValue = 128;
                Initialize();
                Key = (byte[])key.Clone();
            }
        }

        // Constants

        /// <summary>
        /// Total fixed length of <see cref="CommonFrameHeader"/>.
        /// </summary>
        public const ushort FixedLength = 14;

        // Fields
        private FrameType m_frameType;
        private byte m_version;
        private ushort m_frameLength;
        private ushort m_dataLength;
        private ushort m_headerLength;
        private uint m_spduLength;
        private int m_asduCount;
        private bool m_simulatedData;
        private ushort m_applicationID;
        private ushort m_payloadSize;
        private ushort m_idCode;
        private uint m_packetNumber;
        private byte[] m_keyID;
        private SignatureAlgorithm m_signatureAlgorithm;
        private SecurityAlgorithm m_securityAlgorithm;
        private byte[] m_sourceHash;
        private byte[] m_calculatedHash;
        private Ticks m_timestamp;
        private uint m_timebase;
        private uint m_timeQualityFlags;
        private bool m_useETRConfiguration;
        private bool m_guessConfiguration;
        private bool m_parseRedundantASDUs;
        private bool m_ignoreSignatureValidationFailures;
        private bool m_ignoreSampleSizeValidationFailures;
        private AngleFormat m_angleFormat;
        private IChannelParsingState m_state;
        private Action<IChannelFrame> m_publishFrame;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from specified parameters.
        /// </summary>
        /// <param name="typeID">The IEC 61850-90-5 specific frame type of this frame.</param>
        /// <param name="idCode">The ID code of this frame.</param>
        /// <param name="timestamp">The timestamp of this frame.</param>
        public CommonFrameHeader(FrameType typeID, ushort idCode, Ticks timestamp)
        {
            m_frameType = typeID;
            m_idCode = idCode;
            m_timestamp = timestamp;
            m_version = 1;
            m_timebase = Common.Timebase;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="configurationFrame">IEC 61850-90-5 <see cref="ConfigurationFrame"/> if already parsed.</param>
        /// <param name="useETRConfiguration">Determines if system should find associated ETR file using MSVID with same name for configuration.</param>
        /// <param name="guessConfiguration">Determines if system should try to guess at a possible configuration given payload size.</param>
        /// <param name="parseRedundantASDUs">Determines if system should expose redundantly parsed ASDUs.</param>
        /// <param name="ignoreSignatureValidationFailures">Determines if system should ignore checksum signature validation errors.</param>
        /// <param name="ignoreSampleSizeValidationFailures">Determines if system should ignore sample size validation errors.</param>
        /// <param name="angleFormat">Allows customization of the angle parsing format.</param>
        /// <param name="buffer">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        /// <param name="length">Maximum length of valid data from offset.</param>
        public CommonFrameHeader(ConfigurationFrame configurationFrame, bool useETRConfiguration, bool guessConfiguration, bool parseRedundantASDUs, bool ignoreSignatureValidationFailures, bool ignoreSampleSizeValidationFailures, AngleFormat angleFormat, byte[] buffer, int startIndex, int length)
        {
            // Cache behavioral connection parameters
            m_useETRConfiguration = useETRConfiguration;
            m_guessConfiguration = guessConfiguration;
            m_parseRedundantASDUs = parseRedundantASDUs;
            m_ignoreSignatureValidationFailures = ignoreSignatureValidationFailures;
            m_ignoreSampleSizeValidationFailures = ignoreSampleSizeValidationFailures;
            m_angleFormat = angleFormat;

            // See if frame is for a common IEEE C37.118 frame (e.g., for configuration or command)
            if (buffer[startIndex] == GSF.PhasorProtocols.Common.SyncByte)
            {
                // Strip out frame type and version information...
                m_frameType = (FrameType)buffer[startIndex + 1] & ~Iec61850_90_5.FrameType.VersionNumberMask;
                m_version = (byte)(buffer[startIndex + 1] & (byte)Iec61850_90_5.FrameType.VersionNumberMask);

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
            else if (buffer[startIndex + 1] == Common.CltpTag)
            {
                // Make sure there is enough data to parse session header from frame
                if (length > Common.SessionHeaderSize)
                {
                    // Manually assign frame type - this is an IEC 61850-90-5 data frame
                    m_frameType = Iec61850_90_5.FrameType.DataFrame;

                    // Calulate CLTP tag length
                    int cltpTagLength = buffer[startIndex] + 1;

                    // Initialize buffer parsing index starting past connectionless transport protocol header
                    int index = startIndex + cltpTagLength;

                    // Start calculating total frame length
                    int frameLength = cltpTagLength;

                    // Get session type (Goose, sampled values, etc.)
                    SessionType sessionType = (SessionType)buffer[index++];

                    // Make sure session type is sampled values
                    if (sessionType == SessionType.SampledValues)
                    {
                        byte headerSize = buffer[index];

                        // Make sure header size is standard
                        if (headerSize == Common.SessionHeaderSize)
                        {
                            // Skip common header tag
                            index += 3;

                            // Get SPDU length
                            m_spduLength = EndianOrder.BigEndian.ToUInt32(buffer, index);
                            index += 4;

                            // Add SPDU length to total frame length (updated as of 10/3/2012 to accomodate extra 6 bytes)
                            frameLength += (int)m_spduLength + 8;

                            // Make sure full frame of data is available - cannot calculate full frame length needed for check sum
                            // without the entire frame since signature algorithm calculation length varies by type and size
                            if (length > m_spduLength + 13)
                            {
                                // Get SPDU packet number
                                m_packetNumber = EndianOrder.BigEndian.ToUInt32(buffer, index);

                                // Get security algorithm type
                                m_securityAlgorithm = (SecurityAlgorithm)buffer[index + 12];

                                // Get signature algorithm type
                                m_signatureAlgorithm = (SignatureAlgorithm)buffer[index + 13];

                                // Get current key ID
                                m_keyID = buffer.BlockCopy(index + 14, Common.KeySize);

                                // Add signature calculation result length to total frame length
                                switch (m_signatureAlgorithm)
                                {
                                    case SignatureAlgorithm.None:
                                        break;
                                    case SignatureAlgorithm.Sha80:
                                        frameLength += 11;
                                        break;
                                    case SignatureAlgorithm.Sha128:
                                    case SignatureAlgorithm.Aes128:
                                        frameLength += 17;
                                        break;
                                    case SignatureAlgorithm.Sha256:
                                        frameLength += 33;
                                        break;
                                    case SignatureAlgorithm.Aes64:
                                        frameLength += 9;
                                        break;
                                    default:
                                        throw new InvalidOperationException("Invalid IEC 61850-90-5 signature algorithm detected: 0x" + buffer[index].ToString("X").PadLeft(2, '0'));
                                }

                                // Check signature algorithm packet checksum here, this step is skipped in data frame parsing due to non-standard location...
                                if (m_signatureAlgorithm != SignatureAlgorithm.None)
                                {
                                    int packetIndex = startIndex + cltpTagLength;
                                    int hmacIndex = (int)(packetIndex + m_spduLength + 2);

                                    // Check for signature tag
                                    if (buffer[hmacIndex++] == 0x85)
                                    {
                                        // KeyID is tehcnically a lookup into derived rotating keys, but all these are using dummy key for now
                                        HMAC hmac = m_signatureAlgorithm <= SignatureAlgorithm.Sha256 ? (HMAC)(new ShaHmac(Common.DummyKey)) : (HMAC)(new AesHmac(Common.DummyKey));
                                        int result = 0;

                                        switch (m_signatureAlgorithm)
                                        {
                                            case SignatureAlgorithm.None:
                                                break;
                                            case SignatureAlgorithm.Aes64:
                                                m_sourceHash = buffer.BlockCopy(hmacIndex, 8);
                                                m_calculatedHash = hmac.ComputeHash(buffer, packetIndex, (int)m_spduLength).BlockCopy(0, 8);
                                                result = m_sourceHash.CompareTo(0, m_calculatedHash, 0, 8);
                                                break;
                                            case SignatureAlgorithm.Sha80:
                                                m_sourceHash = buffer.BlockCopy(hmacIndex, 10);
                                                m_calculatedHash = hmac.ComputeHash(buffer, packetIndex, (int)m_spduLength).BlockCopy(0, 10);
                                                result = m_sourceHash.CompareTo(0, m_calculatedHash, 0, 10);
                                                break;
                                            case SignatureAlgorithm.Sha128:
                                            case SignatureAlgorithm.Aes128:
                                                m_sourceHash = buffer.BlockCopy(hmacIndex, 16);
                                                m_calculatedHash = hmac.ComputeHash(buffer, packetIndex, (int)m_spduLength).BlockCopy(0, 16);
                                                result = m_sourceHash.CompareTo(0, m_calculatedHash, 0, 16);
                                                break;
                                            case SignatureAlgorithm.Sha256:
                                                m_sourceHash = buffer.BlockCopy(hmacIndex, 32);
                                                m_calculatedHash = hmac.ComputeHash(buffer, packetIndex, (int)m_spduLength).BlockCopy(0, 32);
                                                result = m_sourceHash.CompareTo(0, m_calculatedHash, 0, 32);
                                                break;
                                            default:
                                                throw new NotSupportedException(string.Format("IEC 61850-90-5 signature algorithm \"{0}\" is not currently supported: ", m_signatureAlgorithm));
                                        }

                                        if (result != 0 && !m_ignoreSignatureValidationFailures)
                                            throw new CrcException("Invalid binary image detected - IEC 61850-90-5 check sum does not match.");
                                    }
                                    else
                                    {
                                        throw new CrcException("Invalid binary image detected - expected IEC 61850-90-5 check sum does not exist.");
                                    }
                                }

                                // Get payload length
                                index += 18;
                                m_dataLength = (ushort)EndianOrder.BigEndian.ToUInt32(buffer, index);
                                index += 4;

                                // Confirm payload type tag is sampled values
                                if (buffer[index] != 0x82)
                                    throw new InvalidOperationException("Encountered a payload that is not tagged 0x82 for sampled values: 0x" + buffer[index].ToString("X").PadLeft(2, '0'));

                                index++;

                                // Get simulated bit value
                                m_simulatedData = buffer[index++] != 0;

                                // Get application ID
                                m_applicationID = EndianOrder.BigEndian.ToUInt16(buffer, index);
                                index += 2;

                                // Get ASDU payload size
                                m_payloadSize = EndianOrder.BigEndian.ToUInt16(buffer, index);
                                index += 2;

                                // Validate sampled value PDU tag exists and skip past it
                                buffer.ValidateTag(SampledValueTag.SvPdu, ref index);

                                // Parse number of ASDUs tag
                                m_asduCount = buffer.ParseByteTag(SampledValueTag.AsduCount, ref index);

                                if (m_asduCount == 0)
                                    throw new InvalidOperationException("Total number of ADSUs must be greater than zero.");

                                // Validate sequence of ASDU tag exists and skip past it
                                buffer.ValidateTag(SampledValueTag.SequenceOfAsdu, ref index);

                                // Set header length
                                m_headerLength = (ushort)(index - startIndex);

                                // Set calculated frame length
                                m_frameLength = (ushort)frameLength;
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Bad data stream, encountered an invalid session header size: " + headerSize);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("This library can only parse IEC 61850-90-5 sampled value sessions, type \"{0}\" is not supported.", sessionType));
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Bad data stream, expected sync byte 0xAA or 0x01 as first byte in IEC 61850-90-5 frame, got 0x" + buffer[startIndex].ToString("X").PadLeft(2, '0'));
            }
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommonFrameHeader(SerializationInfo info, StreamingContext context)
        {
            // Deserialize common frame header
            m_frameType = (FrameType)info.GetValue("frameType", typeof(Iec61850_90_5.FrameType));
            m_version = info.GetByte("version");
            m_frameLength = info.GetUInt16("frameLength");
            m_timebase = info.GetUInt32("timebase");
            m_timeQualityFlags = info.GetUInt32("timeQualityFlags");

            if (m_frameType == Iec61850_90_5.FrameType.DataFrame)
            {
                m_headerLength = info.GetUInt16("headerLength");
                m_dataLength = info.GetUInt16("dataLength");
                m_packetNumber = info.GetUInt32("packetNumber");
                m_signatureAlgorithm = (SignatureAlgorithm)info.GetValue("signatureAlgorithm", typeof(Iec61850_90_5.SignatureAlgorithm));
                m_securityAlgorithm = (SecurityAlgorithm)info.GetValue("m_securityAlgorithm", typeof(Iec61850_90_5.SecurityAlgorithm));
                m_asduCount = info.GetInt32("adsuCount");
                m_simulatedData = info.GetBoolean("simulatedData");
                m_applicationID = info.GetUInt16("applicationID");
                m_payloadSize = info.GetUInt16("payloadSize");
                m_keyID = (byte[])info.GetValue("keyID", typeof(byte[]));
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets delegate to method used to publish new frames.
        /// </summary>
        public Action<IChannelFrame> PublishFrame
        {
            get
            {
                return m_publishFrame;
            }
            set
            {
                m_publishFrame = value;
            }
        }

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
        /// Gets or sets the IEC 61850-90-5 specific frame type of this frame.
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
        /// Gets or sets the IEC 61850-90-5 version of this frame.
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
        /// Gets the IEC 61850-90-5 frame header length.
        /// </summary>
        public ushort Length
        {
            get
            {
                if (m_frameType != Iec61850_90_5.FrameType.DataFrame)
                    return FixedLength;

                return m_headerLength;
            }
        }

        /// <summary>
        /// Gets packet number of this frame.
        /// </summary>
        public uint PacketNumber
        {
            get
            {
                return m_packetNumber;
            }
        }

        /// <summary>
        /// Gets number of ASDUs in this frame.
        /// </summary>
        public int AsduCount
        {
            get
            {
                return m_asduCount;
            }
        }

        /// <summary>
        /// Gets or sets the IEC 61850-90-5 frame length of this frame.
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
        /// Gets or sets the length of the data in the IEC 61850-90-5 frame (i.e., the <see cref="FrameLength"/> minus the header length and checksum: <see cref="FrameLength"/> - 8).
        /// </summary>
        public ushort DataLength
        {
            get
            {
                if (m_frameType != Iec61850_90_5.FrameType.DataFrame)
                {
                    // Data length will be frame length minus common header length minus crc16
                    return (ushort)(FrameLength - FixedLength - 2);
                }
                else
                {
                    return m_dataLength;
                }
            }
            set
            {

                if (m_frameType != Iec61850_90_5.FrameType.DataFrame)
                {
                    if (value > Common.MaximumDataLength)
                        throw new OverflowException("Data length value cannot exceed " + Common.MaximumDataLength);
                    else
                        FrameLength = (ushort)(value + FixedLength + 2);
                }
                else
                {
                    m_dataLength = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the IEC 61850-90-5 ID code of this frame.
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
        /// Gets or sets the IEC 61850-90-5 resolution of fractional time stamps.
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
        /// Gets the IEC 61850-90-5 second of century.
        /// </summary>
        public uint SecondOfCentury
        {
            get
            {
                return (uint)Math.Truncate(TimeTag.Value);
            }
        }

        /// <summary>
        /// Gets the IEC 61850-90-5 fraction of second.
        /// </summary>
        public UInt24 FractionOfSecond
        {
            get
            {
                return (UInt24)((decimal)m_timestamp.DistanceBeyondSecond() / (decimal)Ticks.PerSecond * (decimal)m_timebase);
            }
        }

        /// <summary>
        /// Gets or sets the IEC 61850-90-5 <see cref="TimeQualityFlags"/>.
        /// </summary>
        public TimeQualityFlags TimeQualityFlags
        {
            get
            {
                return (TimeQualityFlags)(m_timeQualityFlags & ~(uint)Iec61850_90_5.TimeQualityFlags.TimeQualityIndicatorCodeMask);
            }
            set
            {
                m_timeQualityFlags = (m_timeQualityFlags & (uint)Iec61850_90_5.TimeQualityFlags.TimeQualityIndicatorCodeMask) | (uint)value;
            }
        }

        /// <summary>
        /// Gets or sets the IEC 61850-90-5 <see cref="TimeQualityIndicatorCode"/>.
        /// </summary>
        public TimeQualityIndicatorCode TimeQualityIndicatorCode
        {
            get
            {
                return (TimeQualityIndicatorCode)(m_timeQualityFlags & (uint)Iec61850_90_5.TimeQualityFlags.TimeQualityIndicatorCodeMask);
            }
            set
            {
                m_timeQualityFlags = (m_timeQualityFlags & ~(uint)Iec61850_90_5.TimeQualityFlags.TimeQualityIndicatorCodeMask) | (uint)value;
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
        /// Gets or sets flag that determines if system should find associated ETR file using MSVID with same name for configuration.
        /// </summary>
        public bool UseETRConfiguration
        {
            get
            {
                return m_useETRConfiguration;
            }
            set
            {
                m_useETRConfiguration = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if system should try to guess at a possible configuration given payload size.
        /// </summary>
        public bool GuessConfiguration
        {
            get
            {
                return m_guessConfiguration;
            }
            set
            {
                m_guessConfiguration = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if system should expose redundantly parsed ASDUs.
        /// </summary>
        public bool ParseRedundantASDUs
        {
            get
            {
                return m_parseRedundantASDUs;
            }
            set
            {
                m_parseRedundantASDUs = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if system should ignore checksum signature validation errors.
        /// </summary>
        public bool IgnoreSignatureValidationFailures
        {
            get
            {
                return m_ignoreSignatureValidationFailures;
            }
            set
            {
                m_ignoreSignatureValidationFailures = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if system should ignore sample size validation errors.
        /// </summary>
        public bool IgnoreSampleSizeValidationFailures
        {
            get
            {
                return m_ignoreSampleSizeValidationFailures;
            }
            set
            {
                m_ignoreSampleSizeValidationFailures = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="AngleFormat"/> for the <see cref="IPhasorDefinition"/> objects.
        /// </summary>
        /// <remarks>
        /// Base class defines default angle format since this is rarely not radians.
        /// </remarks>
        public virtual AngleFormat PhasorAngleFormat
        {
            get
            {
                return m_angleFormat;
            }
            set
            {
                m_angleFormat = value;
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
                // Translate IEC 61850-90-5 specific frame type to fundamental frame type
                switch (m_frameType)
                {
                    case Iec61850_90_5.FrameType.DataFrame:
                        return FundamentalFrameType.DataFrame;
                    case Iec61850_90_5.FrameType.ConfigurationFrame:
                        return FundamentalFrameType.ConfigurationFrame;
                    case Iec61850_90_5.FrameType.CommandFrame:
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
            attributes.Add("Header Length", Length.ToString());
            attributes.Add("Payload Length", DataLength.ToString());
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

            if (m_frameType != Iec61850_90_5.FrameType.DataFrame)
            {
                attributes.Add("Version", Version.ToString());
            }
            else
            {
                attributes.Add("SPDU Length", m_spduLength.ToString());
                attributes.Add("ASDU Payload Length", m_payloadSize.ToString());
                attributes.Add("Packet Number", PacketNumber.ToString());
                attributes.Add("Key ID", ByteEncoding.Hexadecimal.GetString(m_keyID, ' '));
                attributes.Add("Security Algorithm", (byte)m_securityAlgorithm + ": " + m_securityAlgorithm);
                attributes.Add("Signature Algorithm", (byte)m_signatureAlgorithm + ": " + m_signatureAlgorithm);
                attributes.Add("Parsed Signature Hash", ByteEncoding.Hexadecimal.GetString(m_sourceHash, ' '));
                attributes.Add("Calculated Signature Hash", ByteEncoding.Hexadecimal.GetString(m_calculatedHash, ' '));
                attributes.Add("Ignoring Checksum Validation", IgnoreSignatureValidationFailures.ToString());
                attributes.Add("Number of ASDUs", m_asduCount.ToString());
                attributes.Add("Simulated Data", m_simulatedData.ToString());
                attributes.Add("Application ID", m_applicationID.ToString());
                attributes.Add("Using ETR Configuration", UseETRConfiguration.ToString());
                attributes.Add("Configuration Guessing Allowed", GuessConfiguration.ToString());
                attributes.Add("Parsing Redundant ASDUs", ParseRedundantASDUs.ToString());
                attributes.Add("Selected Angle Format", m_angleFormat.ToString());
            }
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize unique common frame header values
            info.AddValue("frameType", m_frameType, typeof(Iec61850_90_5.FrameType));
            info.AddValue("version", m_version);
            info.AddValue("frameLength", m_frameLength);
            info.AddValue("timebase", m_timebase);
            info.AddValue("timeQualityFlags", m_timeQualityFlags);

            if (m_frameType == Iec61850_90_5.FrameType.DataFrame)
            {
                info.AddValue("headerLength", m_headerLength);
                info.AddValue("dataLength", m_dataLength);
                info.AddValue("packetNumber", m_packetNumber);
                info.AddValue("signatureAlgorithm", m_signatureAlgorithm, typeof(Iec61850_90_5.SignatureAlgorithm));
                info.AddValue("securityAlgorithm", m_securityAlgorithm, typeof(Iec61850_90_5.SecurityAlgorithm));
                info.AddValue("adsuCount", m_asduCount);
                info.AddValue("simulatedData", m_simulatedData);
                info.AddValue("applicationID", m_applicationID);
                info.AddValue("payloadSize", m_payloadSize);
                info.AddValue("keyID", m_keyID, typeof(byte[]));
            }
        }

        #endregion
    }
}