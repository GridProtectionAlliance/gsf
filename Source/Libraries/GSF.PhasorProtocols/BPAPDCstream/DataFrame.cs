//******************************************************************************************************
//  DataFrame.cs - Gbtc
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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  05/20/2011 - J. Ritchie Carroll
//       Added DST file support.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using GSF.IO.Checksums;
using GSF.Parsing;

namespace GSF.PhasorProtocols.BPAPDCstream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IDataFrame"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class DataFrame : DataFrameBase, ISupportSourceIdentifiableFrameImage<SourceChannel, FrameType>
    {
        #region [ Members ]

        // Fields
        private CommonFrameHeader m_frameHeader;
        private ushort m_sampleNumber;

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
        /// <param name="sampleNumber">Sample number for this <see cref="DataFrame"/>.</param>
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
        public new DataCellCollection Cells => base.Cells as DataCellCollection;

        /// <summary>
        /// Gets or sets <see cref="ConfigurationFrame"/> associated with this <see cref="DataFrame"/>.
        /// </summary>
        public new ConfigurationFrame ConfigurationFrame
        {
            get => base.ConfigurationFrame as ConfigurationFrame;
            set => base.ConfigurationFrame = value;
        }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="DataFrame"/>.
        /// </summary>
        public new DataFrameParsingState State
        {
            get => base.State as DataFrameParsingState;
            set => base.State = value;
        }

        /// <summary>
        /// Gets the identifier that is used to identify the IEEE C37.118 frame.
        /// </summary>
        public FrameType TypeID => BPAPDCstream.FrameType.DataFrame;

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            // Make sure frame header exists - using base class timestamp to
            // prevent recursion (m_frameHeader doesn't exist yet)
            get => m_frameHeader ??= new CommonFrameHeader(1);
            set
            {
                m_frameHeader = value;

                if (m_frameHeader is null)
                    return;

                State = m_frameHeader.State as DataFrameParsingState;
                Timestamp = m_frameHeader.RoughTimestamp;
                UsePhasorDataFileFormat = m_frameHeader.UsePhasorDataFileFormat;
            }
        }

        // This interface implementation satisfies ISupportFrameImage<FrameType>.CommonHeader
        ICommonHeader<FrameType> ISupportFrameImage<FrameType>.CommonHeader
        {
            get => CommonHeader;
            set => CommonHeader = value as CommonFrameHeader;
        }

        /// <summary>
        /// Gets or sets the packet number of this <see cref="DataFrame"/>.
        /// </summary>
        public byte PacketNumber
        {
            get => CommonHeader.PacketNumber;
            set => CommonHeader.PacketNumber = value;
        }

        /// <summary>
        /// Gets or sets the sample number of this <see cref="DataFrame"/>.
        /// </summary>
        public ushort SampleNumber
        {
            get => m_sampleNumber;
            set => m_sampleNumber = value;
        }

        /// <summary>
        /// Gets the timestamp of this <see cref="DataFrame"/> formatted as NTP.
        /// </summary>
        public NtpTimeTag NtpTimeTag => new(Timestamp);

        /// <summary>
        /// Gets the legacy labels parsed from the <see cref="DataFrame"/>, if any.
        /// </summary>
        public string[] LegacyLabels { get; private set; }

        /// <summary>
        /// Gets or sets flag that determines if source data is in the Phasor Data File Format (i.e., a DST file).
        /// </summary>
        public bool UsePhasorDataFileFormat { get; private set; }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => 
            ConfigurationFrame is null || ConfigurationFrame.StreamType != StreamType.Legacy ? 12 : 12 + ConfigurationFrame.Cells.Count * 8;

        /// <summary>
        /// Gets the binary header image of the <see cref="DataFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[HeaderLength];
                int index = 0;

                // Make sure to provide proper frame length for use in the common header image
                CommonHeader.FrameLength = unchecked((ushort)BinaryLength);

                // Copy in common frame header portion of header image
                CommonHeader.BinaryImage.CopyImage(buffer, ref index, CommonFrameHeader.FixedLength);

                if (ConfigurationFrame.RevisionNumber == RevisionNumber.Revision0)
                    BigEndian.CopyBytes((uint)Math.Truncate(NtpTimeTag.Value), buffer, 4);
                else
                    BigEndian.CopyBytes((uint)Math.Truncate(TimeTag.Value), buffer, 4);

                BigEndian.CopyBytes(m_sampleNumber, buffer, 8);
                BigEndian.CopyBytes((ushort)Cells.Count, buffer, 10);

                index += 8;

                // If producing a legacy format, include additional header
                if (ConfigurationFrame.StreamType == StreamType.Legacy)
                {
                    ushort offset = 0;

                    for (int x = 0; x < Cells.Count; x++)
                    {
                        DataCell dataCell = Cells[x];

                        // Add label to data frame header
                        Encoding.ASCII.GetBytes(dataCell.IDLabel).CopyImage(buffer, ref index, 4);

                        // Skip reserved bytes
                        index += 2;

                        // Add offset to data frame header
                        BigEndian.CopyBytes(offset, buffer, index);
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

                if (LegacyLabels is not null)
                {
                    baseAttributes.Add("Legacy Label Count", LegacyLabels.Length.ToString());

                    for (int x = 0; x < LegacyLabels.Length; x++)
                        baseAttributes.Add($"    Legacy Label {x}", LegacyLabels[x]);
                }

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is overridden to compensate for lack of CRC in DST files.
        /// </remarks>
        public override int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            int parsedLength = base.ParseBinaryImage(buffer, startIndex, length);

            // Subtract 2 bytes from total length when using phasor data file format, DST files do not use CRC
            if (UsePhasorDataFileFormat)
                parsedLength -= 2;

            return parsedLength;
        }

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] buffer, int startIndex, int length)
        {
            IDataFrameParsingState state = State;

            // Check for unlikely occurrence of unexpected configuration frame type
            if (state.ConfigurationFrame is not ConfigurationFrame configurationFrame)
                throw new InvalidOperationException("Unexpected configuration frame encountered - BPA PDCstream configuration frame expected, cannot parse data frame.");

            if (UsePhasorDataFileFormat)
            {
                // Because in cases where PDCxchng is being used the data cell count will be smaller than the
                // configuration cell count - we save this count to calculate the offsets later
                state.CellCount = unchecked((int)configurationFrame.CommonHeader.PmuCount);

                if (state.CellCount > configurationFrame.Cells.Count)
                    throw new InvalidOperationException($"Stream/Config File Mismatch: PMU count ({state.CellCount}) in stream does not match defined count in configuration file ({configurationFrame.Cells.Count})");

                return CommonFrameHeader.FixedLength;
            }

            // Only need to parse what wasn't already parsed in common frame header
            int index = startIndex + CommonFrameHeader.FixedLength;

            // Parse frame timestamp
            uint secondOfCentury = BigEndian.ToUInt32(buffer, index);
            m_sampleNumber = BigEndian.ToUInt16(buffer, index + 4);
            index += 6;

            if (configurationFrame.RevisionNumber == RevisionNumber.Revision0)
                Timestamp = new NtpTimeTag(secondOfCentury, 0).ToDateTime().Ticks + (long)((m_sampleNumber - 1) * configurationFrame.TicksPerFrame);
            else
                Timestamp = new UnixTimeTag(secondOfCentury).ToDateTime().Ticks + (long)((m_sampleNumber - 1) * configurationFrame.TicksPerFrame);

            // Because in cases where PDCxchng is being used the data cell count will be smaller than the
            // configuration cell count - we save this count to calculate the offsets later
            state.CellCount = BigEndian.ToUInt16(buffer, index);
            index += 2;

            if (state.CellCount > configurationFrame.Cells.Count)
                throw new InvalidOperationException($"Stream/Config File Mismatch: PMU count ({state.CellCount}) in stream does not match defined count in configuration file ({configurationFrame.Cells.Count})");

            // We'll at least retrieve legacy labels if defined (might be useful for debugging dynamic changes in data-stream)
            if (configurationFrame.StreamType == StreamType.Legacy)
            {
                LegacyLabels = new string[state.CellCount];

                for (int x = 0; x < state.CellCount; x++)
                {
                    LegacyLabels[x] = Encoding.ASCII.GetString(buffer, index, 4);

                    // We don't need offsets, so we skip them...
                    index += 8;
                }
            }

            return index - startIndex;
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
            return buffer.Xor16Checksum(offset, length);
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
            LittleEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex);
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
            // DST files don't use checksums
            if (UsePhasorDataFileFormat)
                return true;

            int sumLength = BinaryLength - 2;
            return LittleEndian.ToUInt16(buffer, startIndex + sumLength) == CalculateChecksum(buffer, startIndex, sumLength);
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize data frame
            info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
            info.AddValue("sampleNumber", m_sampleNumber);
        }

        #endregion
    }
}