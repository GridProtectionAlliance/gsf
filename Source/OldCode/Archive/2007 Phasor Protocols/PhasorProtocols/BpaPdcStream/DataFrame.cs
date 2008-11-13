//*******************************************************************************************************
//  DataPacket.vb - PDCstream data packet
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using PCS;
using PCS.IO.Checksums;

namespace PCS.PhasorProtocols
{
    namespace BpaPdcStream
    {
        // This is essentially a "row" of PMU data at a given timestamp
        [CLSCompliant(false), Serializable()]
        public class DataFrame : DataFrameBase, ICommonFrameHeader
        {
            private byte m_packetNumber;
            private short m_sampleNumber;
            private string[] m_legacyLabels;
            private int m_parsedCellCount;

            public DataFrame()
                : base(new DataCellCollection())
            {
                m_packetNumber = 1;
            }

            protected DataFrame(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                // Deserialize data frame
                m_packetNumber = info.GetByte("packetNumber");
                m_sampleNumber = info.GetInt16("sampleNumber");
            }

            public DataFrame(short sampleNumber)
                : this()
            {
                m_sampleNumber = sampleNumber;
            }

            // If you are going to create multiple data packets, you can use this constructor
            // Note that this only starts becoming necessary if you start hitting data size
            // limits imposed by the nature of the transport protocol...
            public DataFrame(byte packetNumber, short sampleNumber)
                : this(sampleNumber)
            {
                this.PacketNumber = packetNumber;
            }

            public DataFrame(ICommonFrameHeader parsedFrameHeader, IConfigurationFrame configurationFrame, byte[] binaryImage, int startIndex)
                : base(new DataFrameParsingState(new DataCellCollection(), parsedFrameHeader.FrameLength, configurationFrame, BpaPdcStream.DataCell.CreateNewDataCell), binaryImage, startIndex)
            {
                CommonFrameHeader.Clone(parsedFrameHeader, this);
            }

            public DataFrame(IDataFrame dataFrame)
                : base(dataFrame)
            {
            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new DataCellCollection Cells
            {
                get
                {
                    return (DataCellCollection)base.Cells;
                }
            }

            public new ConfigurationFrame ConfigurationFrame
            {
                get
                {
                    return (ConfigurationFrame)base.ConfigurationFrame;
                }
                set
                {
                    base.ConfigurationFrame = value;
                }
            }

            public byte PacketNumber
            {
                get
                {
                    return m_packetNumber;
                }
                set
                {
                    m_packetNumber = value;
                }
            }

            public short SampleNumber
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

            public FrameType FrameType
            {
                get
                {
                    return BpaPdcStream.FrameType.DataFrame;
                }
            }

            FundamentalFrameType ICommonFrameHeader.FundamentalFrameType
            {
                get
                {
                    return base.FundamentalFrameType;
                }
            }

            public short WordCount
            {
                get
                {
                    return (short)(base.BinaryLength / 2);
                }
                set
                {
                    base.ParsedBinaryLength = (ushort)(value * 2);
                }
            }

            public ushort FrameLength
            {
                get
                {
                    return base.BinaryLength;
                }
            }

            public NtpTimeTag NtpTimeTag
            {
                get
                {
                    return new NtpTimeTag(PCS.Ticks.ToSeconds(Ticks));
                }
            }

            public string[] LegacyLabels
            {
                get
                {
                    return m_legacyLabels;
                }
            }

            [CLSCompliant(false)]
            protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
            {
                // PDCstream uses simple XOR checksum
                return buffer.Xor16CheckSum(offset, length);
            }

            protected override void AppendChecksum(byte[] buffer, int startIndex)
            {
                // Oddly enough, check sum for frames in BPA PDC stream is little-endian
                EndianOrder.LittleEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex);
            }

            protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
            {
                int sumLength = BinaryLength - 2;
                return EndianOrder.LittleEndian.ToUInt16(buffer, startIndex + sumLength) == CalculateChecksum(buffer, startIndex, sumLength);
            }

            protected override ushort HeaderLength
            {
                get
                {
                    if (ConfigurationFrame.StreamType == StreamType.Legacy)
                    {
                        if (m_parsedCellCount > 0)
                        {
                            return (ushort)(12 + m_parsedCellCount * 8); // PDCxchng correction
                        }
                        else
                        {
                            return (ushort)(12 + ConfigurationFrame.Cells.Count * 8);
                        }
                    }
                    else
                    {
                        return 12;
                    }
                }
            }

            protected override byte[] HeaderImage
            {
                get
                {
                    byte[] buffer = new byte[HeaderLength];

                    // Copy in common frame header portion of header image
                    System.Buffer.BlockCopy(CommonFrameHeader.BinaryImage(this), 0, buffer, 0, CommonFrameHeader.BinaryLength);

                    if (ConfigurationFrame.RevisionNumber == RevisionNumber.Revision0)
                    {
                        EndianOrder.BigEndian.CopyBytes((uint)NtpTimeTag.Value, buffer, 4);
                    }
                    else
                    {
                        EndianOrder.BigEndian.CopyBytes((uint)TimeTag.Value, buffer, 4);
                    }

                    EndianOrder.BigEndian.CopyBytes(m_sampleNumber, buffer, 8);
                    EndianOrder.BigEndian.CopyBytes((short)Cells.Count, buffer, 10);

                    // If producing a legacy format, include additional header
                    if (ConfigurationFrame.StreamType == StreamType.Legacy)
                    {
                        int index = 12;
                        byte[] reservedBytes = new byte[2];
                        ushort offset = 0;

                        for (int x = 0; x <= Cells.Count - 1; x++)
                        {
                            DataCell dataCell = Cells[x];
                            PhasorProtocols.Common.CopyImage(Encoding.ASCII.GetBytes(dataCell.IDLabel), buffer, ref index, 4);
                            PhasorProtocols.Common.CopyImage(reservedBytes, buffer, ref index, 2);
                            EndianOrder.BigEndian.CopyBytes(offset, buffer, index);
                            index += 2;
                            offset += dataCell.BinaryLength;
                        }
                    }

                    return buffer;
                }
            }

            protected override void ParseHeaderImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                // Only need to parse what wan't already parsed in common frame header
                DataFrameParsingState frameParsingState = (DataFrameParsingState)state;
                BpaPdcStream.ConfigurationFrame configurationFrame = (BpaPdcStream.ConfigurationFrame)frameParsingState.ConfigurationFrame;

                // Because in cases where PDCxchng is being used the data cell count will be smaller than the
                // configuration cell count - we save this count to calculate the offsets later
                frameParsingState.CellCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 10);

                if (frameParsingState.CellCount > configurationFrame.Cells.Count)
                {
                    throw (new InvalidOperationException("Stream/Config File Mismatch: PMU count (" + frameParsingState.CellCount + ") in stream does not match defined count in configuration file (" + configurationFrame.Cells.Count + ")"));
                }

                m_parsedCellCount = frameParsingState.CellCount;

                // Note: because "HeaderLength" needs configuration frame and is called before associated configuration frame
                // assignment normally occurs - we assign configuration frame in advance...
                this.ConfigurationFrame = configurationFrame;

                // We'll at least retrieve legacy labels if defined (might be useful for debugging dynamic changes in data-stream)
                if (configurationFrame.StreamType == StreamType.Legacy)
                {
                    int index = 12;
                    m_legacyLabels = new string[frameParsingState.CellCount];

                    for (int x = 0; x <= frameParsingState.CellCount - 1; x++)
                    {
                        m_legacyLabels[x] = Encoding.ASCII.GetString(binaryImage, index, 4);
                        index += 8;
                    }
                }
            }

            protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {
                // We override normal frame body image parsing because any cells in PDCxchng format
                // will contain several PMU's within a "PDC Block" - when we encounter these we must
                // advance the cell index by the number of PMU's parsed instead of one at a time
                DataFrameParsingState parsingState = (DataFrameParsingState)state;

                int index = 0;
                DataCell cell;
                int cellCount = parsingState.CellCount;

                while (!(index >= cellCount))
                {
                    // No need to call delegate since this is a custom override anyway
                    cell = new DataCell(this, parsingState, index, binaryImage, startIndex);

                    if (cell.UsingPDCExchangeFormat)
                    {
                        // Advance start index beyond PMU's added from PDC block
                        startIndex += cell.PdcBlockLength;

                        // Advance current cell index and total cell count
                        index += cell.PdcBlockPmuCount;
                        cellCount += cell.PdcBlockPmuCount - 1; // Subtract one since PDC block counted as one cell
                    }
                    else
                    {
                        // Handle normal case of adding an individual PMU
                        Cells.Add(cell);
                        startIndex += cell.BinaryLength;
                        index++;
                    }
                }
            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {
                base.GetObjectData(info, context);

                // Serialize data frame
                info.AddValue("packetNumber", m_packetNumber);
                info.AddValue("sampleNumber", m_sampleNumber);
            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Packet Number", m_packetNumber.ToString());
                    baseAttributes.Add("Sample Number", m_sampleNumber.ToString());

                    if (m_legacyLabels != null)
                    {
                        baseAttributes.Add("Legacy Label Count", m_legacyLabels.Length.ToString());

                        for (int x = 0; x <= m_legacyLabels.Length - 1; x++)
                        {
                            baseAttributes.Add("    Legacy Label " + x, m_legacyLabels[x]);
                        }
                    }

                    return baseAttributes;
                }
            }
        }
    }
}
