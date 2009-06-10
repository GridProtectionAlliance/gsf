//*******************************************************************************************************
//  DataFrame.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using PCS.IO.Checksums;
using PCS.Parsing;

namespace PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IDataFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class DataFrame : DataFrameBase, ISupportFrameImage<FrameType>
    {
        #region [ Members ]

        // Fields
        private CommonFrameHeader m_frameHeader;
        private ushort m_sampleNumber;
        private string[] m_legacyLabels;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataFrame"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to parse a BPA PDCstream data frame.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DataFrame()
            : base(new DataCellCollection(), 0, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DataFrame"/> from specified parameters.
        /// </summary>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="DataFrame"/>.</param>
        /// <param name="configurationFrame">The <see cref="ConfigurationFrame"/> associated with this <see cref="DataFrame"/>.</param>
        /// <param name="packetNumber">Packet number for this <see cref="DataFrame"/>.</param>
        /// <param name="sampleNumber">Sample number for thie <see cref="DataFrame"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate a BPA PDCstream data frame.
        /// </remarks>
        public DataFrame(Ticks timestamp, ConfigurationFrame configurationFrame, byte packetNumber, ushort sampleNumber)
            : base(new DataCellCollection(), timestamp, configurationFrame)
        {
            PacketNumber = packetNumber;
            m_sampleNumber = sampleNumber;
        }

        /// <summary>
        /// Creates a new <see cref="DataFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DataFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize data frame
            m_frameHeader = (CommonFrameHeader)info.GetValue("frameHeader", typeof(CommonFrameHeader));
            m_sampleNumber = info.GetUInt16("sampleNumber");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="DataCellCollection"/> for this <see cref="DataFrame"/>.
        /// </summary>
        public new DataCellCollection Cells
        {
            get
            {
                return base.Cells as DataCellCollection;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ConfigurationFrame"/> associated with this <see cref="DataFrame"/>.
        /// </summary>
        public new ConfigurationFrame ConfigurationFrame
        {
            get
            {
                return base.ConfigurationFrame as ConfigurationFrame;
            }
            set
            {
                base.ConfigurationFrame = value;
            }
        }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="DataFrame"/>.
        /// </summary>
        public new DataFrameParsingState State
        {
            get
            {
                return base.State as DataFrameParsingState;
            }
            set
            {
                base.State = value;
            }
        }

        /// <summary>
        /// Gets the identifier that is used to identify the IEEE C37.118 frame.
        /// </summary>
        public FrameType TypeID
        {
            get
            {
                return BpaPdcStream.FrameType.DataFrame;
            }
        }

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            get
            {
                // Make sure frame header exists - using base class timestamp to
                // prevent recursion (m_frameHeader doesn't exist yet)
                if (m_frameHeader == null)
                    m_frameHeader = new CommonFrameHeader(1);

                return m_frameHeader;
            }
            set
            {
                m_frameHeader = value;

                if (m_frameHeader != null)
                {
                    State = m_frameHeader.State as DataFrameParsingState;
                    Timestamp = m_frameHeader.RoughTimestamp;
                }
            }
        }

        // This interface implementation satisfies ISupportFrameImage<FrameType>.CommonHeader
        ICommonHeader<FrameType> ISupportFrameImage<FrameType>.CommonHeader
        {
            get
            {
                return CommonHeader;
            }
            set
            {
                CommonHeader = value as CommonFrameHeader;
            }
        }

        /// <summary>
        /// Gets or sets the packet number of this <see cref="DataFrame"/>.
        /// </summary>
        public byte PacketNumber
        {
            get
            {
                return CommonHeader.PacketNumber;
            }
            set
            {
                CommonHeader.PacketNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the sample number of this <see cref="DataFrame"/>.
        /// </summary>
        public ushort SampleNumber
        {
            get
            {
                return m_sampleNumber;
            }
            set
            {
                m_sampleNumber = value;
            }
        }

        /// <summary>
        /// Gets the timestamp of this <see cref="DataFrame"/> formatted as NTP.
        /// </summary>
        public NtpTimeTag NtpTimeTag
        {
            get
            {
                return new NtpTimeTag(Timestamp);
            }
        }

        /// <summary>
        /// Gets the legacy labels parsed from the <see cref="DataFrame"/>, if any.
        /// </summary>
        public string[] LegacyLabels
        {
            get
            {
                return m_legacyLabels;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength
        {
            get
            {
                if (ConfigurationFrame.StreamType == StreamType.Legacy)
                    return 12 + ConfigurationFrame.Cells.Count * 8;
                else
                    return 12;
            }
        }

        /// <summary>
        /// Gets the binary header image of the <see cref="DataFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[HeaderLength];
                int index = 0;

                // Copy in common frame header portion of header image
                CommonHeader.BinaryImage.CopyImage(buffer, ref index, CommonFrameHeader.FixedLength);

                if (ConfigurationFrame.RevisionNumber == RevisionNumber.Revision0)
                    EndianOrder.BigEndian.CopyBytes((uint)Math.Truncate(NtpTimeTag.Value), buffer, 4);
                else
                    EndianOrder.BigEndian.CopyBytes((uint)Math.Truncate(TimeTag.Value), buffer, 4);

                EndianOrder.BigEndian.CopyBytes(m_sampleNumber, buffer, 8);
                EndianOrder.BigEndian.CopyBytes((ushort)Cells.Count, buffer, 10);

                index += 12;

                // If producing a legacy format, include additional header
                if (ConfigurationFrame.StreamType == StreamType.Legacy)
                {
                    byte[] reservedBytes = new byte[2];
                    ushort offset = 0;

                    for (int x = 0; x < Cells.Count; x++)
                    {
                        DataCell dataCell = Cells[x];
                        Encoding.ASCII.GetBytes(dataCell.IDLabel).CopyImage(buffer, ref index, 4);
                        reservedBytes.CopyImage(buffer, ref index, 2);
                        EndianOrder.BigEndian.CopyBytes(offset, buffer, index);
                        index += 2;
                        offset += (ushort)dataCell.BinaryLength;
                    }
                }

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DataFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                CommonHeader.AppendHeaderAttributes(baseAttributes);
                baseAttributes.Add("Sample Number", m_sampleNumber.ToString());

                if (m_legacyLabels != null)
                {
                    baseAttributes.Add("Legacy Label Count", m_legacyLabels.Length.ToString());

                    for (int x = 0; x < m_legacyLabels.Length; x++)
                    {
                        baseAttributes.Add("    Legacy Label " + x, m_legacyLabels[x]);
                    }
                }

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] binaryImage, int startIndex, int length)
        {
            IDataFrameParsingState state = State;
            int index = startIndex + CommonFrameHeader.FixedLength;

            // Only need to parse what wan't already parsed in common frame header
            ConfigurationFrame configurationFrame = state.ConfigurationFrame as ConfigurationFrame;

            // Parse frame timestamp
            uint secondOfCentury = EndianOrder.BigEndian.ToUInt32(binaryImage, index);
            m_sampleNumber = EndianOrder.BigEndian.ToUInt16(binaryImage, index + 4);
            index += 6;

            if (configurationFrame.RevisionNumber == RevisionNumber.Revision0)
                Timestamp = (new NtpTimeTag(secondOfCentury, 0)).ToDateTime().Ticks + (long)(m_sampleNumber * configurationFrame.TicksPerFrame);
            else
                Timestamp = (new UnixTimeTag(secondOfCentury)).ToDateTime().Ticks + (long)(m_sampleNumber * configurationFrame.TicksPerFrame);

            // Because in cases where PDCxchng is being used the data cell count will be smaller than the
            // configuration cell count - we save this count to calculate the offsets later
            state.CellCount = EndianOrder.BigEndian.ToUInt16(binaryImage, index);
            index += 2;

            if (state.CellCount > configurationFrame.Cells.Count)
                throw new InvalidOperationException("Stream/Config File Mismatch: PMU count (" + state.CellCount + ") in stream does not match defined count in configuration file (" + configurationFrame.Cells.Count + ")");
            
            // We'll at least retrieve legacy labels if defined (might be useful for debugging dynamic changes in data-stream)
            if (configurationFrame.StreamType == StreamType.Legacy)
            {
                m_legacyLabels = new string[state.CellCount];

                for (int x = 0; x < state.CellCount; x++)
                {
                    m_legacyLabels[x] = Encoding.ASCII.GetString(binaryImage, index, 4);
                    // We don't need offsets, so we skip them...
                    index += 8;
                }
            }

            return (index - startIndex);
        }

        /// <summary>
        /// Calculates checksum of given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer image over which to calculate checksum.</param>
        /// <param name="offset">Start index into <paramref name="buffer"/> to calculate checksum.</param>
        /// <param name="length">Length of data within <paramref name="buffer"/> to calculate checksum.</param>
        /// <returns>Checksum over specified portion of <paramref name="buffer"/>.</returns>
        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            // PDCstream uses a 16-bit XOR based check sum
            return buffer.Xor16CheckSum(offset, length);
        }

        /// <summary>
        /// Appends checksum onto <paramref name="buffer"/> starting at <paramref name="startIndex"/>.
        /// </summary>
        /// <param name="buffer">Buffer image on which to append checksum.</param>
        /// <param name="startIndex">Index into <paramref name="buffer"/> where checksum should be appended.</param>
        /// <remarks>
        /// We override default implementation since BPA PDCstream implements check sum for frames in little-endian.
        /// </remarks>
        protected override void AppendChecksum(byte[] buffer, int startIndex)
        {
            // Oddly enough, check sum for frames in BPA PDC stream is little-endian
            EndianOrder.LittleEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex);
        }

        /// <summary>
        /// Determines if checksum in the <paramref name="buffer"/> is valid.
        /// </summary>
        /// <param name="buffer">Buffer image to validate.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to perform checksum.</param>
        /// <returns>Flag that determines if checksum over <paramref name="buffer"/> is valid.</returns>
        /// <remarks>
        /// We override default implementation since BPA PDCstream implements check sum for frames in little-endian.
        /// </remarks>
        protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
        {
            int sumLength = BinaryLength - 2;
            return EndianOrder.LittleEndian.ToUInt16(buffer, startIndex + sumLength) == CalculateChecksum(buffer, startIndex, sumLength);
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize data frame
            info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
            info.AddValue("sampleNumber", m_sampleNumber);
        }

        #endregion
    }
}