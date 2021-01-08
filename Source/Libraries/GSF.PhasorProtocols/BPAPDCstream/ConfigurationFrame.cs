//******************************************************************************************************
//  ConfigurationFrame.cs - Gbtc
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
//  05/19/2011 - Ritchie
//       Added DST file support.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using GSF.Interop;
using GSF.IO.Checksums;
using GSF.Parsing;
using GSF.Reflection;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.BPAPDCstream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IConfigurationFrame"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class ConfigurationFrame : ConfigurationFrameBase, ISupportSourceIdentifiableFrameImage<SourceChannel, FrameType>
    {
        #region [ Members ]

        // Constants
        private const int FixedHeaderLength = CommonFrameHeader.FixedLength + 12;

        /// <summary>
        /// Default voltage phasor INI based configuration entry.
        /// </summary>
        public const string DefaultVoltagePhasorEntry = "V,4500.0,0.0060573,0,0,500,Default 500kV";

        /// <summary>
        /// Default current phasor INI based configuration entry.
        /// </summary>
        public const string DefaultCurrentPhasorEntry = "I,600.00,0.000040382,0,1,1,Default Current";

        /// <summary>
        /// Default frequency INI based configuration entry.
        /// </summary>
        public const string DefaultFrequencyEntry = "F,1000,60,1000,0,0,Frequency";

        // Events

        /// <summary>
        /// Occurs when the BPA PDCstream INI based configuration file has been reloaded.
        /// </summary>
        public event EventHandler ConfigurationFileReloaded;

        // Fields
        private CommonFrameHeader m_frameHeader;
        private IniFile m_iniFile;
        private uint m_rowLength;
        private ushort m_packetsPerSample;
        private StreamType m_streamType;
        private RevisionNumber m_revisionNumber;
        private uint m_sampleIndex;

    #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to parse a BPA PDCstream configuration frame.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConfigurationFrame()
            : base(0, new ConfigurationCellCollection(), 0, 0)
        {
            // We just assume current timestamp for configuration frames since they don't provide one
            Timestamp = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from specified parameters.
        /// </summary>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="configurationFileName">The required external BPA PDCstream INI based configuration file.</param>
        /// <param name="revisionNumber">Defines the <see cref="RevisionNumber"/> for this BPA PDCstream configuration frame.</param>
        /// <param name="streamType">Defines the <see cref="StreamType"/> for this BPA PDCstream configuration frame.</param>
        /// <param name="packetsPerSample">Number of packets per sample.</param>
        /// <remarks>
        /// <para>
        /// This constructor is used by a consumer to generate a BPA PDCstream configuration frame.
        /// </para>
        /// <para>
        /// If you are going to create multiple data packets set <paramref name="packetsPerSample"/> to a number
        /// greater than one. This will only start becoming necessary if you start hitting data size limits imposed
        /// by the nature of the transport protocol.
        /// </para>
        /// </remarks>
        public ConfigurationFrame(Ticks timestamp, string configurationFileName, ushort packetsPerSample, RevisionNumber revisionNumber, StreamType streamType)
            : base(0, new ConfigurationCellCollection(), timestamp, 30)
        {
            m_iniFile = new IniFile(configurationFileName);
            m_packetsPerSample = packetsPerSample;
            m_revisionNumber = revisionNumber;
            m_streamType = streamType;
            Refresh(false);
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize configuration frame
            m_frameHeader = (CommonFrameHeader)info.GetValue("frameHeader", typeof(CommonFrameHeader));
            m_packetsPerSample = info.GetUInt16("packetsPerSample");
            m_streamType = (StreamType)info.GetValue("streamType", typeof(StreamType));
            m_revisionNumber = (RevisionNumber)info.GetValue("revisionNumber", typeof(RevisionNumber));
            m_iniFile = new IniFile(info.GetString("configurationFileName"));

            // The usePhasorDataFileFormat flag and other new elements did not exist in prior versions so we protect against possible deserialization failures
            try
            {
                UsePhasorDataFileFormat = info.GetBoolean("usePhasorDataFileFormat");
                m_rowLength = info.GetUInt32("rowLength");
            }
            catch (SerializationException)
            {
                UsePhasorDataFileFormat = false;
                m_rowLength = 0;
            }

            Refresh(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new ConfigurationCellCollection Cells => base.Cells as ConfigurationCellCollection;

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> loaded from the configuration file of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public ConfigurationCellCollection ConfigurationFileCells { get; private set; }

        /// <summary>
        /// Gets the <see cref="FrameType"/> of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public FrameType TypeID => BPAPDCstream.FrameType.ConfigurationFrame;

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            // Make sure frame header exists
            get => m_frameHeader ?? (m_frameHeader = new CommonFrameHeader(Common.DescriptorPacketFlag));
            set
            {
                m_frameHeader = value;

                if (m_frameHeader is null)
                    return;

                UsePhasorDataFileFormat = m_frameHeader.UsePhasorDataFileFormat;
                m_sampleIndex = m_frameHeader.StartSample;
                Timestamp = m_frameHeader.RoughTimestamp;
                m_rowLength = m_frameHeader.RowLength;

                if (m_frameHeader.State is ConfigurationFrameParsingState parsingState)
                {
                    State = parsingState;
                    m_iniFile = new IniFile(parsingState.ConfigurationFileName);
                }
            }
        }

        // This interface implementation satisfies ISupportFrameImage<FrameType>.CommonHeader
        ICommonHeader<FrameType> ISupportFrameImage<FrameType>.CommonHeader
        {
            get => CommonHeader;
            set => CommonHeader = value as CommonFrameHeader;
        }

        /// <summary>
        /// Gets or sets the BPA PDCstream protocol <see cref="BPAPDCstream.StreamType"/>, i.e., legacy or compact, of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public StreamType StreamType
        {
            get => m_streamType;
            set => m_streamType = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="BPAPDCstream.RevisionNumber"/> of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public RevisionNumber RevisionNumber
        {
            get => m_revisionNumber;
            set => m_revisionNumber = value;
        }

        /// <summary>
        /// Gets or sets the number of packets per sample of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public ushort PacketsPerSample
        {
            get => m_packetsPerSample;
            set => m_packetsPerSample = value;
        }

        /// <summary>
        /// Gets or sets current sample index used to calculate row time stamp when source data is in the Phasor Data File Format (i.e., a DST file).
        /// </summary>
        public uint SampleIndex
        {
            get => m_sampleIndex;
            set => m_sampleIndex = value;
        }

        /// <summary>
        /// Gets the INI based configuration file name of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public string ConfigurationFileName => m_iniFile.FileName;

        /// <summary>
        /// Gets the default voltage phasor definition.
        /// </summary>
        public PhasorDefinition DefaultPhasorV { get; private set; }

        /// <summary>
        /// Gets the default current phasor definition.
        /// </summary>
        public PhasorDefinition DefaultPhasorI { get; private set; }

        /// <summary>
        /// Gets the default frequency definition.
        /// </summary>
        public FrequencyDefinition DefaultFrequency { get; private set; }

        /// <summary>
        /// Gets or sets flag that determines if source data is in the Phasor Data File Format (i.e., a DST file).
        /// </summary>
        public bool UsePhasorDataFileFormat { get; private set; }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => FixedHeaderLength;

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                if (!UsePhasorDataFileFormat)
                {
                    // Make sure to provide proper frame length for use in the common header image
                    CommonHeader.FrameLength = unchecked((ushort)BinaryLength);

                    byte[] buffer = new byte[FixedHeaderLength];
                    int index = 0;

                    CommonHeader.BinaryImage.CopyImage(buffer, ref index, CommonFrameHeader.FixedLength);
                    buffer[4] = (byte)StreamType;
                    buffer[5] = (byte)RevisionNumber;
                    BigEndian.CopyBytes(FrameRate, buffer, 6);
                    BigEndian.CopyBytes(RowLength(true), buffer, 8); // <-- Important: This step calculates all PMU row offsets!
                    BigEndian.CopyBytes(PacketsPerSample, buffer, 12);
                    BigEndian.CopyBytes((ushort)Cells.Count, buffer, 14);

                    return buffer;
                }

                throw new NotSupportedException("Creation of the phasor file format (i.e., DST files) is not currently supported.");
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ConfigurationFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                CommonHeader.AppendHeaderAttributes(baseAttributes);

                if (!(m_iniFile is null))
                    baseAttributes.Add("Configuration File Name", m_iniFile.FileName);

                baseAttributes.Add("Stream Type", (int)m_streamType + ": " + m_streamType);
                baseAttributes.Add("Revision Number", (int)m_revisionNumber + ": " + m_revisionNumber);
                baseAttributes.Add("Packets Per Sample", m_packetsPerSample.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] buffer, int startIndex, int length)
        {
            if (UsePhasorDataFileFormat)
            {
                // Common frame header will have parsed all phasor data file header information at this point...
                State.CellCount = unchecked((int)CommonHeader.PmuCount);

                return CommonFrameHeader.DstHeaderFixedLength;
            }

            // Skip past header that was already parsed...
            startIndex += CommonFrameHeader.FixedLength;

            // Only need to parse what wan't already parsed in common frame header
            m_streamType = (StreamType)buffer[startIndex];
            m_revisionNumber = (RevisionNumber)buffer[startIndex + 1];
            FrameRate = BigEndian.ToUInt16(buffer, startIndex + 2);
            m_rowLength = BigEndian.ToUInt32(buffer, startIndex + 4);
            m_packetsPerSample = BigEndian.ToUInt16(buffer, startIndex + 8);
            State.CellCount = BigEndian.ToUInt16(buffer, startIndex + 10);

            // The data that's in the data stream will take precedence over what's in the
            // in the configuration file.  The configuration file may define more PMU's than
            // are in the stream - in my opinion that's OK - it's when you have PMU's in the
            // stream that aren't defined in the INI file that you'll have trouble...

            return FixedHeaderLength;
        }

        /// <summary>
        /// Parses the binary image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is overridden so INI file can be loaded after binary image has been parsed.
        /// </remarks>
        public override int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            int parsedLength = base.ParseBinaryImage(buffer, startIndex, length);

            // Load INI file image and associate parsed cells to cells in configuration file...
            Refresh(true);

            // Move past 4 EOH bytes, minus 2 since DST files do not contain a CRC at the end of each frame
            if (UsePhasorDataFileFormat)
                parsedLength += 2;

            return parsedLength;
        }

        /// <summary>
        /// Reload BPA PDcstream INI based configuration file.
        /// </summary>
        public void Refresh()
        {
            Refresh(false);
        }

        /// <summary>
        /// Reload BPA PDcstream INI based configuration file specifying if the refresh was caused by a frame parse.
        /// </summary>
        /// <param name="refreshCausedByFrameParse">Flag that specifies if the refresh was caused by a frame parse.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void Refresh(bool refreshCausedByFrameParse)
        {
            // The only time we need an access lock is when we reload the config file...
            lock (m_iniFile)
            {
                if (File.Exists(m_iniFile.FileName))
                {
                    ConfigurationCell pmuCell;
                    int phasorCount, pmuCount, x, y;

                    DefaultPhasorV = new PhasorDefinition(null, 0, m_iniFile["DEFAULT", "PhasorV", DefaultVoltagePhasorEntry]);
                    DefaultPhasorI = new PhasorDefinition(null, 0, m_iniFile["DEFAULT", "PhasorI", DefaultCurrentPhasorEntry]);
                    DefaultFrequency = new FrequencyDefinition(null, m_iniFile["DEFAULT", "Frequency", DefaultFrequencyEntry]);
                    FrameRate = ushort.Parse(m_iniFile["CONFIG", "SampleRate", "30"]);

                    // We read all cells in the config file into their own configuration cell collection - cells parsed
                    // from the configuration frame will be mapped to their associated config file cell by ID label
                    // when the configuration cell is parsed from the configuration frame
                    if (ConfigurationFileCells is null)
                        ConfigurationFileCells = new ConfigurationCellCollection();

                    ConfigurationFileCells.Clear();

                    // Load phasor data for each section in config file...
                    foreach (string section in m_iniFile.GetSectionNames())
                    {
                        if (section.Length > 0)
                        {
                            // Make sure this is not a special section
                            if (!string.Equals(section, "DEFAULT", StringComparison.OrdinalIgnoreCase) && !string.Equals(section, "CONFIG", StringComparison.OrdinalIgnoreCase))
                            {
                                // Create new PMU entry structure from config file settings...
                                phasorCount = int.Parse(m_iniFile[section, "NumberPhasors", "0"]);

                                // Check for PDC code
                                int pdcID = int.Parse(m_iniFile[section, "PDC", "-1"]);

                                if (pdcID == -1)
                                {
                                    // No PDC entry exists, assume this is a PMU
                                    pmuCell = new ConfigurationCell(this, 0)
                                    {
                                        IDCode = ushort.Parse(m_iniFile[section, "PMU", Cells.Count.ToString()]),
                                        SectionEntry = section,
                                        StationName = m_iniFile[section, "Name", section]
                                    };

                                    // This will automatically assign ID label as first 4 digits of section
                                    for (x = 0; x < phasorCount; x++)
                                    {
                                        pmuCell.PhasorDefinitions.Add(new PhasorDefinition(pmuCell, x + 1, m_iniFile[section, "Phasor" + (x + 1), DefaultVoltagePhasorEntry]));
                                    }

                                    pmuCell.FrequencyDefinition = new FrequencyDefinition(pmuCell, m_iniFile[section, "Frequency", DefaultFrequencyEntry]);
                                    ConfigurationFileCells.Add(pmuCell);
                                }
                                else
                                {
                                    // This is a PDC, need to define one virtual entry for each PMU
                                    pmuCount = int.Parse(m_iniFile[section, "NumberPMUs", "0"]);

                                    for (x = 0; x < pmuCount; x++)
                                    {
                                        // Create a new PMU cell for each PDC entry that exists
                                        pmuCell = new ConfigurationCell(this, 0);

                                        // For BPA INI files, PMUs tradionally have an ID number indexed starting at zero or one - so we multiply
                                        // ID by 1000 and add index to attempt to create a fairly unique ID to help optimize downstream parsing
                                        pmuCell.IDCode = unchecked((ushort)(pdcID * 1000 + x));
                                        pmuCell.SectionEntry = $"{section}pmu{x}"; // This will automatically assign ID label as first 4 digits of section
                                        pmuCell.StationName = $"{m_iniFile[section, "Name", section]} - Device {x + 1}";

                                        for (y = 0; y < 2; y++)
                                        {
                                            pmuCell.PhasorDefinitions.Add(new PhasorDefinition(pmuCell, y + 1, m_iniFile[section, "Phasor" + (x * 2 + y + 1), DefaultVoltagePhasorEntry]));
                                        }

                                        pmuCell.FrequencyDefinition = new FrequencyDefinition(pmuCell, m_iniFile[section, "Frequency", DefaultFrequencyEntry]);
                                        ConfigurationFileCells.Add(pmuCell);
                                    }
                                }
                            }
                        }
                    }

                    // Associate parsed cells with cells defined in INI file
                    if (ConfigurationFileCells.Count > 0 && !(Cells is null))
                    {
                        ConfigurationCell configurationFileCell = null;

                        if (refreshCausedByFrameParse)
                        {
                            // Create a new configuration cell collection that will account for PDC block cells
                            ConfigurationCellCollection cellCollection = new ConfigurationCellCollection();
                            ConfigurationCell cell;

                            // For freshly parsed configuration frames we'll have no PMU's in configuration
                            // frame for PDCxchng blocks - so we'll need to dynamically create them
                            for (x = 0; x < Cells.Count; x++)
                            {
                                // Get current configuration cell
                                cell = Cells[x];

                                // Lookup INI file configuration cell by ID label
                                ConfigurationFileCells.TryGetByIDLabel(cell.IDLabel, out IConfigurationCell configurationCell);
                                configurationFileCell = (ConfigurationCell)configurationCell;

                                if (configurationFileCell is null)
                                {
                                    // Couldn't find associated INI file cell - just append the parsed cell to the collection
                                    cellCollection.Add(cell);
                                }
                                else
                                {
                                    if (configurationFileCell.IsPdcBlockSection)
                                    {
                                        // This looks like a PDC block section - so we'll keep adding cells for each defined PMU in the PDC block
                                        int index = 0;

                                        do
                                        {
                                            // Lookup PMU by section name
                                            ConfigurationFileCells.TryGetBySectionEntry($"{cell.IDLabel}pmu{index}", ref configurationFileCell);

                                            // Add PDC block PMU configuration cell to the collection
                                            if (!(configurationFileCell is null))
                                                cellCollection.Add(configurationFileCell);

                                            index++;
                                        }
                                        while (!(configurationFileCell is null));
                                    }
                                    else
                                    {
                                        // As far as we can tell from INI file, this is just a regular PMU
                                        cell.ConfigurationFileCell = configurationFileCell;
                                        cellCollection.Add(cell);
                                    }
                                }
                            }

                            // Assign "new" cell collection which will include PMU's from defined PDC blocks
                            Cells.Clear();
                            Cells.AddRange(cellCollection);
                        }
                        else
                        {
                            // For simple INI file updates, we just re-assign INI file cells associating by section entry
                            foreach (ConfigurationCell cell in Cells)
                            {
                                // Attempt to associate this configuration cell with information read from external INI based configuration file
                                ConfigurationFileCells.TryGetBySectionEntry(cell.SectionEntry, ref configurationFileCell);
                                cell.ConfigurationFileCell = configurationFileCell;
                            }
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("PDC config file \"" + m_iniFile.FileName + "\" does not exist.");
                }
            }

            // In case other classes want to know, we send out a notification that the config file has been reloaded (make sure
            // you do this after the write lock has been released to avoid possible dead-lock situations)
            ConfigurationFileReloaded?.Invoke(this, EventArgs.Empty);
        }

        // RowLength property calculates cell offsets - so it must be called before
        // accessing cell offsets - this happens automatically since HeaderImage is
        // called before base class BodyImage which just gets Cells.BinaryImage
        internal uint RowLength(bool recalculate)
        {
            if (m_rowLength == 0 || recalculate)
            {
                m_rowLength = 0;

                for (int x = 0; x < Cells.Count; x++)
                {
                    ConfigurationCell cell = Cells[x];

                    cell.Offset = unchecked((ushort)m_rowLength);

                    m_rowLength += 8 + FrequencyValue.CalculateBinaryLength(cell.FrequencyDefinition);

                    for (int y = 0; y < cell.PhasorDefinitions.Count; y++)
                    {
                        m_rowLength += PhasorValue.CalculateBinaryLength(y);
                    }
                }
            }

            return m_rowLength;
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
            if (UsePhasorDataFileFormat)
            {
                // DST files don't use checksums
                return true;
            }

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

            // Serialize configuration frame
            info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
            info.AddValue("packetsPerSample", m_packetsPerSample);
            info.AddValue("streamType", m_streamType, typeof(StreamType));
            info.AddValue("revisionNumber", m_revisionNumber, typeof(RevisionNumber));
            info.AddValue("configurationFileName", m_iniFile.FileName);
            info.AddValue("usePhasorDataFileFormat", UsePhasorDataFileFormat);
            info.AddValue("rowLength", m_rowLength);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Gets a generated INI configuration file image.
        /// </summary>
        public static string GetIniFileImage(IConfigurationFrame configFrame)
        {
            StringBuilder fileImage = new StringBuilder();

            fileImage.AppendLine("; BPA PDCstream IniFile for Configuration " + configFrame.IDCode);
            fileImage.AppendLine("; Auto-generated on " + DateTime.Now);
            fileImage.AppendLine(";    Assembly: " + AssemblyInfo.ExecutingAssembly.Name);
            fileImage.AppendLine(";    Compiled: " + File.GetLastWriteTime(AssemblyInfo.ExecutingAssembly.Location));
            fileImage.AppendLine(";");
            fileImage.AppendLine(";");
            fileImage.AppendLine("; Format:");
            fileImage.AppendLine(";   Each Column in data file is given a bracketed identifier, numbered in the order it");
            fileImage.AppendLine(";   appears in the data file, and identified by data type ( PMU, PDC, or other)");
            fileImage.AppendLine(";     PMU designates column data format from a single PMU");
            fileImage.AppendLine(";     PDC designates column data format from another PDC which is somewhat different from a single PMU");
            fileImage.AppendLine(";   Default gives default values for a processing algorithm in case quantities are omitted");
            fileImage.AppendLine(";   Name= gives the overall station name for print labels");
            fileImage.AppendLine(";   NumberPhasors= :  for PMU data, gives the number of phasors contained in column");
            fileImage.AppendLine(";                     for PDC data, gives the number of PMUs data included in the column");
            fileImage.AppendLine(";                     Note - for PDC data, there will be 2 phasors & 1 freq per PMU");
            fileImage.AppendLine(";   Quantities within the column are listed by PhasorI=, Frequency=, etc");
            fileImage.AppendLine(";   Each quantity has 7 comma separated fields followed by an optional comment");
            fileImage.AppendLine(";");
            fileImage.AppendLine(";   Phasor entry format:  Type, Ratio, Cal Factor, Offset, Shunt, VoltageRef/Class, Label  ;Comments");
            fileImage.AppendLine(";    Type:       Type of measurement, V=voltage, I=current, N=don\'t care, single ASCII character");
            fileImage.AppendLine(";    Ratio:      PT/CT ratio N:1 where N is a floating point number");
            fileImage.AppendLine(";    Cal Factor: Conversion factor between integer in file and secondary volts, floating point");
            fileImage.AppendLine(";    Offset:     Phase Offset to correct for phase angle measurement errors or differences, floating point");
            fileImage.AppendLine(";    Shunt:      Current- shunt resistence in ohms, or the equivalent ratio for aux CTs, floating point");
            fileImage.AppendLine(";                Voltage- empty, not used");
            fileImage.AppendLine(";    VoltageRef: Current- phasor number (1-10) of voltage phasor to use for power calculation, integer");
            fileImage.AppendLine(";                Voltage- voltage class, standard l-l voltages, 500, 230, 115, etc, integer");
            fileImage.AppendLine(";    Label:      Phasor quantity label for print label, text");
            fileImage.AppendLine(";    Comments:   All text after the semicolon on a line are optional comments not for processing");
            fileImage.AppendLine(";");
            fileImage.AppendLine(";   Voltage Magnitude = MAG(Real,Imaginary) * CalFactor * PTR (line-neutral)");
            fileImage.AppendLine(";   Current Magnitude = MAG(Real,Imaginary) * CalFactor * CTR / Shunt (phase current)");
            fileImage.AppendLine(";   Phase Angle = ATAN(Imaginary/Real) + Phase Offset (usually degrees)");
            fileImage.AppendLine(";     Note: Usually phase Offset is 0, but is sometimes required for comparing measurements");
            fileImage.AppendLine(";           from different systems or through transformer banks");
            fileImage.AppendLine(";");
            fileImage.AppendLine(";   Frequency entry format:  scale, offset, dF/dt scale, dF/dt offset, dummy, label  ;Comments");
            fileImage.AppendLine(";   Frequency = Number / scale + offset");
            fileImage.AppendLine(";   dF/dt = Number / (dF/dt scale) + (dF/dt offset)");
            fileImage.AppendLine(";");
            fileImage.AppendLine(";");

            fileImage.AppendLine("[DEFAULT]");
            fileImage.AppendLine("PhasorV=" + DefaultVoltagePhasorEntry); //PhasorDefinition.ConfigFileFormat(DefaultPhasorV));
            fileImage.AppendLine("PhasorI=" + DefaultCurrentPhasorEntry); //PhasorDefinition.ConfigFileFormat(DefaultPhasorI));
            fileImage.AppendLine("Frequency=" + DefaultFrequencyEntry);   //FrequencyDefinition.ConfigFileFormat(DefaultFrequency));
            fileImage.AppendLine();

            fileImage.AppendLine("[CONFIG]");
            fileImage.AppendLine("SampleRate=" + configFrame.FrameRate);
            fileImage.AppendLine("NumberOfPMUs=" + configFrame.Cells.Count);
            fileImage.AppendLine();

            for (int x = 0; x < configFrame.Cells.Count; x++)
            {
                fileImage.AppendLine("[" + configFrame.Cells[x].IDLabel + "]");
                fileImage.AppendLine("Name=" + configFrame.Cells[x].StationName);
                fileImage.AppendLine("PMU=" + x);
                fileImage.AppendLine("NumberPhasors=" + configFrame.Cells[x].PhasorDefinitions.Count);
                for (int y = 0; y < configFrame.Cells[x].PhasorDefinitions.Count; y++)
                {
                    fileImage.AppendLine("Phasor" + (y + 1) + "=" + PhasorDefinition.ConfigFileFormat(configFrame.Cells[x].PhasorDefinitions[y]));
                }
                fileImage.AppendLine("Frequency=" + FrequencyDefinition.ConfigFileFormat(configFrame.Cells[x].FrequencyDefinition));
                fileImage.AppendLine();
            }

            return fileImage.ToString();
        }

        #endregion
    }
}