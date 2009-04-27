//*******************************************************************************************************
//  ConfigurationFrame.cs
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
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using PCS.Interop;
using PCS.IO.Checksums;
using PCS.Parsing;
using PCS.Reflection;

namespace PCS.PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IConfigurationFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class ConfigurationFrame : ConfigurationFrameBase, ISupportFrameImage<FrameType>
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
        public const string DefaultCurrentPhasorEntry = "I,600.00,0.000040382,0,1,1.0,Default Current";

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
        private ConfigurationCellCollection m_configurationFileCells;
        private PhasorDefinition m_defaultPhasorV;
        private PhasorDefinition m_defaultPhasorI;
        private FrequencyDefinition m_defaultFrequency;
        private ushort m_rowLength;
        private ushort m_packetsPerSample;
        private StreamType m_streamType;
        private RevisionNumber m_revisionNumber;

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
        public ConfigurationFrame(Ticks timestamp, string configurationFileName, ushort packetsPerSample)
            : base(0, new ConfigurationCellCollection(), timestamp, 30)
        {
            m_iniFile = new IniFile(configurationFileName);
            m_packetsPerSample = packetsPerSample;
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
            Refresh(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new ConfigurationCellCollection Cells
        {
            get
            {
                return base.Cells as ConfigurationCellCollection;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> loaded from the configuration file of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public ConfigurationCellCollection ConfigurationFileCells
        {
            get
            {
                return m_configurationFileCells;
            }
        }

        /// <summary>
        /// Gets the <see cref="FrameType"/> of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public FrameType TypeID
        {
            get
            {
                return BpaPdcStream.FrameType.ConfigurationFrame;
            }
        }

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            get
            {
                // Make sure frame header exists
                if (m_frameHeader == null)
                    m_frameHeader = new CommonFrameHeader(Common.DescriptorPacketFlag);

                return m_frameHeader;
            }
            set
            {
                m_frameHeader = value;

                if (m_frameHeader != null)
                {
                    ConfigurationFrameParsingState parsingState = m_frameHeader.State as ConfigurationFrameParsingState;

                    if (parsingState != null)
                    {
                        State = parsingState;
                        m_iniFile = new IniFile(parsingState.ConfigurationFileName);
                    }
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
        /// Gets or sets the BPA PDCstream protocol <see cref="BpaPdcStream.StreamType"/>, i.e., legacy or compact, of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public StreamType StreamType
        {
            get
            {
                return m_streamType;
            }
            set
            {
                m_streamType = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="BpaPdcStream.RevisionNumber"/> of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public RevisionNumber RevisionNumber
        {
            get
            {
                return m_revisionNumber;
            }
            set
            {
                m_revisionNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of packets per sample of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public ushort PacketsPerSample
        {
            get
            {
                return m_packetsPerSample;
            }
            set
            {
                m_packetsPerSample = value;
            }
        }

        /// <summary>
        /// Gets the INI based configuration file name of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public string ConfigurationFileName
        {
            get
            {
                return m_iniFile.FileName;
            }
        }

        /// <summary>
        /// Gets the default voltage phasor definition.
        /// </summary>
        public PhasorDefinition DefaultPhasorV
        {
            get
            {
                return m_defaultPhasorV;
            }
        }

        /// <summary>
        /// Gets the default current phasor definition.
        /// </summary>
        public PhasorDefinition DefaultPhasorI
        {
            get
            {
                return m_defaultPhasorI;
            }
        }

        /// <summary>
        /// Gets the default frequency definition.
        /// </summary>
        public FrequencyDefinition DefaultFrequency
        {
            get
            {
                return m_defaultFrequency;
            }
        }

        /// <summary>
        /// Gets a generated INI configuration file image.
        /// </summary>
        public string IniFileImage
        {
            get
            {
                StringBuilder fileImage = new StringBuilder();

                fileImage.Append("; File - " + m_iniFile.FileName + Environment.NewLine);
                fileImage.Append("; Auto-generated on " + DateTime.Now + Environment.NewLine);
                fileImage.Append(";    Assembly: " + AssemblyInfo.ExecutingAssembly.Name + Environment.NewLine);
                fileImage.Append(";    Compiled: " + File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location) + Environment.NewLine);
                fileImage.Append(";" + Environment.NewLine);
                fileImage.Append(";" + Environment.NewLine);
                fileImage.Append("; Format:" + Environment.NewLine);
                fileImage.Append(";   Each Column in data file is given a bracketed identifier, numbered in the order it" + Environment.NewLine);
                fileImage.Append(";   appears in the data file, and identified by data type ( PMU, PDC, or other)" + Environment.NewLine);
                fileImage.Append(";     PMU designates column data format from a single PMU" + Environment.NewLine);
                fileImage.Append(";     PDC designates column data format from another PDC which is somewhat different from a single PMU" + Environment.NewLine);
                fileImage.Append(";   Default gives default values for a processing algorithm in case quantities are omitted" + Environment.NewLine);
                fileImage.Append(";   Name= gives the overall station name for print labels" + Environment.NewLine);
                fileImage.Append(";   NumberPhasors= :  for PMU data, gives the number of phasors contained in column" + Environment.NewLine);
                fileImage.Append(";                     for PDC data, gives the number of PMUs data included in the column" + Environment.NewLine);
                fileImage.Append(";                     Note - for PDC data, there will be 2 phasors & 1 freq per PMU" + Environment.NewLine);
                fileImage.Append(";   Quantities within the column are listed by PhasorI=, Frequency=, etc" + Environment.NewLine);
                fileImage.Append(";   Each quantity has 7 comma separated fields followed by an optional comment" + Environment.NewLine);
                fileImage.Append(";" + Environment.NewLine);
                fileImage.Append(";   Phasor entry format:  Type, Ratio, Cal Factor, Offset, Shunt, VoltageRef/Class, Label  ;Comments" + Environment.NewLine);
                fileImage.Append(";    Type:       Type of measurement, V=voltage, I=current, N=don\'t care, single ASCII character" + Environment.NewLine);
                fileImage.Append(";    Ratio:      PT/CT ratio N:1 where N is a floating point number" + Environment.NewLine);
                fileImage.Append(";    Cal Factor: Conversion factor between integer in file and secondary volts, floating point" + Environment.NewLine);
                fileImage.Append(";    Offset:     Phase Offset to correct for phase angle measurement errors or differences, floating point" + Environment.NewLine);
                fileImage.Append(";    Shunt:      Current- shunt resistence in ohms, or the equivalent ratio for aux CTs, floating point" + Environment.NewLine);
                fileImage.Append(";                Voltage- empty, not used" + Environment.NewLine);
                fileImage.Append(";    VoltageRef: Current- phasor number (1-10) of voltage phasor to use for power calculation, integer" + Environment.NewLine);
                fileImage.Append(";                Voltage- voltage class, standard l-l voltages, 500, 230, 115, etc, integer" + Environment.NewLine);
                fileImage.Append(";    Label:      Phasor quantity label for print label, text" + Environment.NewLine);
                fileImage.Append(";    Comments:   All text after the semicolon on a line are optional comments not for processing" + Environment.NewLine);
                fileImage.Append(";" + Environment.NewLine);
                fileImage.Append(";   Voltage Magnitude = MAG(Real,Imaginary) * CalFactor * PTR (line-neutral)" + Environment.NewLine);
                fileImage.Append(";   Current Magnitude = MAG(Real,Imaginary) * CalFactor * CTR / Shunt (phase current)" + Environment.NewLine);
                fileImage.Append(";   Phase Angle = ATAN(Imaginary/Real) + Phase Offset (usually degrees)" + Environment.NewLine);
                fileImage.Append(";     Note: Usually phase Offset is 0, but is sometimes required for comparing measurements" + Environment.NewLine);
                fileImage.Append(";           from different systems or through transformer banks" + Environment.NewLine);
                fileImage.Append(";" + Environment.NewLine);
                fileImage.Append(";   Frequency entry format:  scale, offset, dF/dt scale, dF/dt offset, dummy, label  ;Comments" + Environment.NewLine);
                fileImage.Append(";   Frequency = Number / scale + offset" + Environment.NewLine);
                fileImage.Append(";   dF/dt = Number / (dF/dt scale) + (dF/dt offset)" + Environment.NewLine);
                fileImage.Append(";" + Environment.NewLine);
                fileImage.Append(";" + Environment.NewLine);

                fileImage.Append("[DEFAULT]" + Environment.NewLine);
                fileImage.Append("PhasorV=" + PhasorDefinition.ConfigFileFormat(DefaultPhasorV) + Environment.NewLine);
                fileImage.Append("PhasorI=" + PhasorDefinition.ConfigFileFormat(DefaultPhasorI) + Environment.NewLine);
                fileImage.Append("Frequency=" + FrequencyDefinition.ConfigFileFormat(DefaultFrequency) + Environment.NewLine);
                fileImage.AppendLine();

                fileImage.Append("[CONFIG]" + Environment.NewLine);
                fileImage.Append("SampleRate=" + FrameRate + Environment.NewLine);
                fileImage.Append("NumberOfPMUs=" + Cells.Count + Environment.NewLine);
                fileImage.AppendLine();

                for (int x = 0; x < Cells.Count; x++)
                {
                    fileImage.Append("[" + Cells[x].IDLabel + "]" + Environment.NewLine);
                    fileImage.Append("Name=" + Cells[x].StationName + Environment.NewLine);
                    fileImage.Append("PMU=" + x + Environment.NewLine);
                    fileImage.Append("NumberPhasors=" + Cells[x].PhasorDefinitions.Count + Environment.NewLine);
                    for (int y = 0; y < Cells[x].PhasorDefinitions.Count; y++)
                    {
                        fileImage.Append("Phasor" + (y + 1) + "=" + PhasorDefinition.ConfigFileFormat(Cells[x].PhasorDefinitions[y]) + Environment.NewLine);
                    }
                    fileImage.Append("Frequency=" + FrequencyDefinition.ConfigFileFormat(Cells[x].FrequencyDefinition) + Environment.NewLine);
                    fileImage.AppendLine();
                }

                return fileImage.ToString();
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength
        {
            get
            {
                return FixedHeaderLength;
            }
        }

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                // Make sure to provide proper frame length for use in the common header image
                CommonHeader.FrameLength = (ushort)BinaryLength;

                byte[] buffer = new byte[FixedHeaderLength];
                int index = 0;

                CommonHeader.BinaryImage.CopyImage(buffer, ref index, CommonFrameHeader.FixedLength);
                buffer[4] = (byte)StreamType;
                buffer[5] = (byte)RevisionNumber;
                EndianOrder.BigEndian.CopyBytes(FrameRate, buffer, 6);
                EndianOrder.BigEndian.CopyBytes(RowLength(true), buffer, 8); // <-- Important: This step calculates all PMU row offsets!
                EndianOrder.BigEndian.CopyBytes(PacketsPerSample, buffer, 12);
                EndianOrder.BigEndian.CopyBytes((ushort)Cells.Count, buffer, 14);

                return buffer;
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

                if (m_iniFile != null)
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
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] binaryImage, int startIndex, int length)
        {
            // Skip past header that was already parsed...
            startIndex += CommonFrameHeader.FixedLength;

            // Only need to parse what wan't already parsed in common frame header
            m_streamType = (StreamType)binaryImage[startIndex];
            m_revisionNumber = (RevisionNumber)binaryImage[startIndex + 1];
            FrameRate = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 2);
            m_rowLength = (ushort)EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex + 4);
            m_packetsPerSample = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 8);
            State.CellCount = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 10);

            // The data that's in the data stream will take precedence over what's in the
            // in the configuration file.  The configuration file may define more PMU's than
            // are in the stream - in my opinion that's OK - it's when you have PMU's in the
            // stream that aren't defined in the INI file that you'll have trouble...

            return FixedHeaderLength;
        }

        /// <summary>
        /// Parses the binary image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is overriden so INI file can be loaded after binary image has been parsed.
        /// </remarks>
        public override int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            int parsedLength = base.Initialize(binaryImage, startIndex, length);

            // Load INI file image and associate parsed cells to cells in configuration file...
            Refresh(true);

            return parsedLength;
        }

        /// <summary>
        /// Reload BPA PDcstream INI based configuration file.
        /// </summary>
        public void Refresh()
        {
            Refresh(false);
        }

        private void Refresh(bool refreshCausedByFrameParse)
        {
            // The only time we need an access lock is when we reload the config file...
            lock (m_iniFile)
            {
                if (File.Exists(m_iniFile.FileName))
                {
                    ConfigurationCell pmuCell;
                    int x;
                    int phasorCount;

                    m_defaultPhasorV = new PhasorDefinition(null, 0, m_iniFile["DEFAULT", "PhasorV", DefaultVoltagePhasorEntry]);
                    m_defaultPhasorI = new PhasorDefinition(null, 0, m_iniFile["DEFAULT", "PhasorI", DefaultCurrentPhasorEntry]);
                    m_defaultFrequency = new FrequencyDefinition(null, m_iniFile["DEFAULT", "Frequency", DefaultFrequencyEntry]);
                    FrameRate = ushort.Parse(m_iniFile["CONFIG", "SampleRate", "30"]);

                    // We read all cells in the config file into their own configuration cell collection - cells parsed
                    // from the configuration frame will be mapped to their associated config file cell by ID label
                    // when the configuration cell is parsed from the configuration frame
                    if (m_configurationFileCells == null)
                        m_configurationFileCells = new ConfigurationCellCollection();

                    m_configurationFileCells.Clear();

                    // Load phasor data for each section in config file...
                    foreach (string section in m_iniFile.GetSectionNames())
                    {
                        if (section.Length > 0)
                        {
                            // Make sure this is not a special section
                            if (string.Compare(section, "DEFAULT", true) != 0 && string.Compare(section, "CONFIG", true) != 0)
                            {
                                // Create new PMU entry structure from config file settings...
                                phasorCount = int.Parse(m_iniFile[section, "NumberPhasors", "0"]);

                                pmuCell = new ConfigurationCell(this, 0, LineFrequency.Hz60);

                                pmuCell.SectionEntry = section; // This will automatically assign ID label as first 4 digits of section
                                pmuCell.StationName = m_iniFile[section, "Name", section];
                                pmuCell.IDCode = ushort.Parse(m_iniFile[section, "PMU", Cells.Count.ToString()]);

                                for (x = 0; x < phasorCount; x++)
                                {
                                    pmuCell.PhasorDefinitions.Add(new PhasorDefinition(pmuCell, x + 1, m_iniFile[section, "Phasor" + (x + 1), DefaultVoltagePhasorEntry]));
                                }

                                pmuCell.FrequencyDefinition = new FrequencyDefinition(pmuCell, m_iniFile[section, "Frequency", DefaultFrequencyEntry]);

                                m_configurationFileCells.Add(pmuCell);
                            }
                        }
                    }

                    // Associate parsed cells with cells defined in INI file
                    if (m_configurationFileCells.Count > 0 && (Cells != null))
                    {
                        ConfigurationCell configurationFileCell = null;
                        IConfigurationCell configurationCell = null;

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
                                m_configurationFileCells.TryGetByIDLabel(cell.IDLabel, ref configurationCell);
                                configurationFileCell = (ConfigurationCell)configurationCell;

                                if (configurationFileCell == null)
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
                                            m_configurationFileCells.TryGetBySectionEntry(cell.IDLabel + "pmu" + index, ref configurationFileCell);

                                            // Add PDC block PMU configuration cell to the collection
                                            if (configurationFileCell != null)
                                                cellCollection.Add(configurationFileCell);

                                            index++;
                                        }
                                        while (configurationFileCell != null);
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
                                m_configurationFileCells.TryGetBySectionEntry(cell.SectionEntry, ref configurationFileCell);
                                cell.ConfigurationFileCell = configurationFileCell;
                            }
                        }
                    }
                }
                else
                    throw new InvalidOperationException("PDC config file \"" + m_iniFile.FileName + "\" does not exist.");
            }

            // In case other classes want to know, we send out a notification that the config file has been reloaded (make sure
            // you do this after the write lock has been released to avoid possible dead-lock situations)
            if (ConfigurationFileReloaded != null)
                ConfigurationFileReloaded(this, EventArgs.Empty);

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

                    cell.Offset = m_rowLength;

                    m_rowLength += (ushort)(12 + FrequencyValue.CalculateBinaryLength(cell.FrequencyDefinition));

                    for (int y = 0; y < cell.PhasorDefinitions.Count; y++)
                    {
                        m_rowLength += (ushort)PhasorValue.CalculateBinaryLength(cell.PhasorDefinitions[y]);
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
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize configuration frame
            info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
            info.AddValue("packetsPerSample", m_packetsPerSample);
            info.AddValue("streamType", m_streamType, typeof(StreamType));
            info.AddValue("revisionNumber", m_revisionNumber, typeof(RevisionNumber));
            info.AddValue("configurationFileName", m_iniFile.FileName);
        }

        #endregion
    }
}